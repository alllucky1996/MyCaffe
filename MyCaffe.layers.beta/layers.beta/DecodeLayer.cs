﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using MyCaffe.basecode;
using MyCaffe.common;
using MyCaffe.param;

namespace MyCaffe.layers.beta
{
    /// <summary>
    /// The DecodeLayer decodes the label of a classification for an encoding produced by a Siamese Network or similar type of net that creates 
    /// an encoding mapped to a set of distances where the smallest distance indicates the label for which the encoding belongs.
    /// </summary>
    /// <remarks>
    /// Centroids:
    /// @see [A New Loss Function for CNN Classifier Based on Pre-defined Evenly-Distributed Class Centroids](https://arxiv.org/abs/1904.06008) by Qiuyu Zhu, Pengju Zhang, and Xin Ye, arXiv:1904.06008, 2019.
    /// </remarks>
    /// <typeparam name="T">Specifies the base type <i>float</i> or <i>double</i>.  Using <i>float</i> is recommended to conserve GPU memory.</typeparam>
    public class DecodeLayer<T> : Layer<T>
    {
        int m_nCentroidThresholdStart = 300;
        int m_nCentroidThresholdEnd = 500;
        int m_nNum = 0;
        int m_nEncodingDim = 0;
        Blob<T> m_blobData;
        Blob<T> m_blobDistSq; 
        Blob<T> m_blobSummerVec;
        Dictionary<int, int> m_rgLabelCounts = new Dictionary<int, int>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cuda">Cuda engine.</param>
        /// <param name="log">General log.</param>
        /// <param name="p">provides the generic parameter for the DecodeLayer.</param>
        public DecodeLayer(CudaDnn<T> cuda, Log log, LayerParameter p)
            : base(cuda, log, p)
        {
            m_type = LayerParameter.LayerType.DECODE;
            m_blobDistSq = new Blob<T>(cuda, log, false);
            m_blobDistSq.Name = m_param.name + " distsq";
            m_blobSummerVec = new Blob<T>(cuda, log, false);
            m_blobSummerVec.Name = m_param.name + " sum";
            m_blobData = new Blob<T>(cuda, log);
            m_blobData.Name = m_param.name + " data";
        }

        /** @copydoc Layer::dispose */
        protected override void dispose()
        {
            if (m_blobDistSq != null)
            {
                m_blobDistSq.Dispose();
                m_blobDistSq = null;
            }

            if (m_blobSummerVec != null)
            {
                m_blobSummerVec.Dispose();
                m_blobSummerVec = null;
            }

            if (m_blobData != null)
            {
                m_blobData.Dispose();
                m_blobData = null;
            }

            base.dispose();
        }

        /** @copydoc Layer::internal_blobs */
        public override BlobCollection<T> internal_blobs
        {
            get
            {
                BlobCollection<T> col = new BlobCollection<T>();
                col.Add(m_blobDistSq);
                col.Add(m_blobSummerVec);
                col.Add(m_blobData);
                return col;
            }
        }

        /// <summary>
        /// Returns the minimum number of bottom blobs used: predicted (RUN phase)
        /// </summary>
        public override int MinBottomBlobs
        {
            get { return 1; }
        }

        /// <summary>
        /// Returns the maximum number of bottom blobs used: predicted, label (TRAIN and TEST phase)
        /// </summary>
        public override int MaxBottomBlobs
        {
            get { return 2; }
        }

        /// <summary>
        /// Returns the min number of top blobs: distances
        /// </summary>
        public override int MinTopBlobs
        {
            get { return 1; }
        }

        /// <summary>
        /// Returns the min number of top blobs: distances, centroids
        /// </summary>
        public override int MaxTopBlobs
        {
            get { return 2; }
        }

        /// <summary>
        /// Setup the layer.
        /// </summary>
        /// <param name="colBottom">Specifies the collection of bottom (input) Blobs.</param>
        /// <param name="colTop">Specifies the collection of top (output) Blobs.</param>
        public override void LayerSetUp(BlobCollection<T> colBottom, BlobCollection<T> colTop)
        {
            m_nEncodingDim = colBottom[0].channels;

            m_nCentroidThresholdStart = m_param.decode_param.centroid_threshold_start;
            m_nCentroidThresholdEnd = m_param.decode_param.centroid_threshold_end;
            m_log.CHECK_GE(m_nCentroidThresholdStart, 10, "The centroid threshold start must be >= 10, and the recommended setting is 300.");
            m_log.CHECK_GT(m_nCentroidThresholdEnd, m_nCentroidThresholdStart, "The centroid threshold end must be > than the centroid threshold start.");

            if (m_colBlobs.Count == 0)
            {
                Blob<T> blobCentroids = new Blob<T>(m_cuda, m_log, false);
                blobCentroids.Name = m_param.name + " centroids";
                blobCentroids.reshape_when_sharing = true;

                List<int> rgCentroidShape = new List<int>() { 0 }; // skip size check.
                if (!shareParameter(blobCentroids, rgCentroidShape))
                {
                    blobCentroids.Reshape(2, m_nEncodingDim, 1, 1); // set to at least two labels initially (may get expanded in forward).
                    blobCentroids.SetData(0);
                }

                m_colBlobs.Add(blobCentroids);

                Blob<T> blobStatus = new Blob<T>(m_cuda, m_log, false);
                blobStatus.Name = m_param.name + " status";
                blobStatus.reshape_when_sharing = true;

                List<int> rgStatusShape = new List<int>() { 0 }; // skip size check.
                if (!shareParameter(blobStatus, rgStatusShape))
                {
                    blobStatus.Reshape(1, 1, 1, 1); // This will be resized to the label count
                    blobStatus.SetData(0);
                }

                m_colBlobs.Add(blobStatus);
            }
        }

        /// <summary>
        /// Reshape the bottom (input) and top (output) blobs.
        /// </summary>
        /// <param name="colBottom">Specifies the collection of bottom (input) Blobs.</param>
        /// <param name="colTop">Specifies the collection of top (output) Blobs.</param>
        public override void Reshape(BlobCollection<T> colBottom, BlobCollection<T> colTop)
        {
            int nNum = colBottom[0].num;
            bool bFirstReshape = (nNum != m_nNum) ? true : false;
            m_nNum = nNum;
            m_nEncodingDim = colBottom[0].channels;

            if (colBottom.Count > 1)
                m_log.CHECK_EQ(colBottom[1].num, m_nNum, "The number of labels does not match the number of items at bottom[0].");

            // vector of ones used to sum along channels.
            m_blobSummerVec.Reshape(colBottom[0].channels, 1, 1, 1);
            m_blobSummerVec.SetData(1.0);
        }

        /// <summary>
        /// Forward compuation.
        /// </summary>
        /// <param name="colBottom">bottom input blob (length 2)
        ///  -# @f$ (N \times C \times 1 \times 1) @f$
        ///     the encoding predictions @f$ x @f$, a blob with values in
        ///     @f$ [-\infty, +\infty] @f$ indicating the embedding of each of
        ///     the @f$ K @f$ classes.  Each embedding @f$ x @f$ is mapped to a predicted 
        ///     label.
        ///  -# @f$ (N \times 1 \times 1 \times 1) @f$
        ///     the labels l, an integer-valued blob with values
        ///     @f$ l_n \in [0, 1, 2, ..., K-1] @f$
        ///     indicating the correct class label among the @f$ K @f$ classes.
        /// </param>
        /// <param name="colTop">top output blob vector (length 1)
        ///  -# @f$ (1 \times 1 \times 1 \times 1) @f$
        ///     the computed accuracy each calculated by finding the label with the minimum
        ///     distance to each encoding.
        /// </param>
        protected override void forward(BlobCollection<T> colBottom, BlobCollection<T> colTop)
        {
            int nActiveLabels = m_param.decode_param.active_label_count;
            int nItemNum = (colBottom[0].num == 1) ? 1 : colBottom[0].num / 2;
            int nCentroidStart = m_nCentroidThresholdStart * nItemNum;
            int nCentroidEnd = m_nCentroidThresholdEnd * nItemNum;
            double dfAlpha = 1.0 / (double)(nCentroidEnd - nCentroidStart);
            double[] rgBottomLabel = null;

            if (m_param.phase == Phase.TRAIN)
            {
                m_log.CHECK_EQ(colBottom[1].count() % 2, 0, "The bottom[1] count must be a factor of 2 for {lbl1, lbl2}.");
                rgBottomLabel = convertD(colBottom[1].update_cpu_data());

                int nMaxLabel = rgBottomLabel.Max(p => (int)p);
                int nMaxKey = (m_rgLabelCounts.Count == 0) ? 0 : m_rgLabelCounts.Max(p => p.Key);
                if (nMaxLabel > nMaxKey)
                {
                    int nNumLabels = nMaxLabel + 1;

                    m_colBlobs[0].Reshape(nNumLabels, m_nEncodingDim, 1, 1);
                    m_colBlobs[0].SetData(0);
                    m_colBlobs[1].Reshape(nNumLabels, 1, 1, 1);
                    m_colBlobs[1].SetData(0);
                    m_blobData.Reshape(nNumLabels, m_nEncodingDim, 1, 1);
                    m_blobDistSq.Reshape(nNumLabels, 1, 1, 1);
                    m_rgLabelCounts.Clear();
                }
            }
            else
            {
                m_blobData.ReshapeLike(m_colBlobs[0]);
                m_blobDistSq.Reshape(m_colBlobs[0].num, 1, 1, 1);
            }

            if (nActiveLabels <= 0)
                nActiveLabels = m_colBlobs[0].num;

            colTop[0].Reshape(colBottom[0].num, m_colBlobs[0].num, 1, 1);

            for (int i = 0; i < colBottom[0].num; i++)
            {
                // When training, we calculate the centroids during observations between nCentroidStart and nCentroidEnd.
                if (rgBottomLabel != null)
                {
                    int nLabel = (int)rgBottomLabel[i * 2]; // Only the first embedding and first label are used (second is ignored).

                    T fReady = m_colBlobs[1].GetData(nLabel);
                    double dfReady = convertD(fReady);

                    if (dfReady == 0)
                    {
                        if (!m_rgLabelCounts.ContainsKey(nLabel))
                            m_rgLabelCounts.Add(nLabel, 1);
                        else
                            m_rgLabelCounts[nLabel]++;

                        // Create the centroid when counts fall between Centroid Start and Centroid End by
                        // averaging all items within these counts together to create the centroid.
                        if (m_rgLabelCounts[nLabel] < nCentroidStart)
                        {
                            // do nothing.
                        }
                        else if (m_rgLabelCounts[nLabel] == nCentroidStart)
                        {
                            // Add initial centroid portion for the label.
                            m_cuda.copy(m_nEncodingDim, colBottom[0].gpu_data, m_colBlobs[0].mutable_gpu_data, i * m_nEncodingDim, nLabel * m_nEncodingDim);
                            m_cuda.scale(m_nEncodingDim, convert(dfAlpha), m_colBlobs[0].gpu_data, m_colBlobs[0].mutable_gpu_data, nLabel * m_nEncodingDim, nLabel * m_nEncodingDim);
                        }
                        else if (m_rgLabelCounts[nLabel] > nCentroidStart && m_rgLabelCounts[nLabel] < nCentroidEnd)
                        {
                            // Add portion of current item to centroids for the label.
                            m_cuda.add(m_nEncodingDim, colBottom[0].gpu_data, m_colBlobs[0].gpu_data, m_colBlobs[0].mutable_gpu_data, dfAlpha, 1, i * m_nEncodingDim, nLabel * m_nEncodingDim, nLabel * m_nEncodingDim);
                        }
                        else
                        {
                            m_colBlobs[1].SetData(1.0, nLabel);

                            int nCompleted = (int)convertD(m_colBlobs[1].asum_data());
                            if (nCompleted == nActiveLabels)
                                m_colBlobs[1].snapshot_requested = true;
                        }
                    }
                }

                // Wait until we have at least Centroid Threshold number of items for each label before calcuating the distances.
                // NOTE: During the TEST and RUN phases, this should have 
                int nCompletedCentroids = (int)convertD(m_colBlobs[1].asum_data());
                if (nCompletedCentroids == nActiveLabels)
                {
                    int nLabelCount = m_colBlobs[0].num;
                    if (nLabelCount == 0)
                        break;

                    // Load data with the current data embedding across each label 'slot'.
                    for (int k = 0; k < nLabelCount; k++)
                    {
                        m_cuda.copy(m_nEncodingDim, colBottom[0].gpu_data, m_blobData.mutable_gpu_data, i * m_nEncodingDim, k * m_nEncodingDim);
                    }

                    int nCount = m_blobData.count();

                    m_cuda.sub(nCount,
                               m_blobData.gpu_data,              // a
                               m_colBlobs[0].gpu_data,           // b
                               m_blobData.mutable_gpu_diff);     // a_i - b_i

                    m_cuda.powx(nCount,
                               m_blobData.gpu_diff,              // a_i - b_i
                               2.0,
                               m_blobData.mutable_gpu_diff);     // (a_i - b_i)^2

                    m_cuda.gemv(false,
                               m_blobData.num,                   // label count.
                               m_blobData.channels,              // encoding size.
                               1.0,
                               m_blobData.gpu_diff,              // (a_i - b_i)^2
                               m_blobSummerVec.gpu_data,
                               0.0,
                               m_blobDistSq.mutable_gpu_data);   // \Sum (a_i - b_i)^2

                    // The distances are returned in top[0], where the smallest distance is the detected label.
                    m_cuda.copy(nLabelCount, m_blobDistSq.gpu_data, colTop[0].mutable_gpu_data, 0, i * nLabelCount);
                }
            }

            if (colTop.Count > 1)
            {
                colTop[1].ReshapeLike(m_colBlobs[0]);

                int nCompletedCentroids = (int)convertD(m_colBlobs[1].asum_data());
                if (nCompletedCentroids == nActiveLabels)
                {
                    m_cuda.copy(m_colBlobs[0].count(), m_colBlobs[0].gpu_data, colTop[1].mutable_gpu_data);
                }
                else
                {
                    if (m_phase != Phase.TRAIN)
                        m_log.WriteLine("WARNING: The centroids for the decode layer are not completed!  You must train the model first to calculate the centroids.");

                    colTop[1].SetData(0);
                }
            }
        }

        /// @brief Not implemented -- DecodeLayer cannot be used as a loss.
        protected override void backward(BlobCollection<T> colTop, List<bool> rgbPropagateDown, BlobCollection<T> colBottom)
        {
            // do nothing.
        }
    }
}
