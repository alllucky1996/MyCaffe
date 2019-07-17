﻿using MyCaffe.basecode;
using MyCaffe.param;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCaffe.common
{
    /// <summary>
    /// The BBox class processes the NormalizedBBox data used with SSD.
    /// </summary>
    /// <remarks>
    /// @see [SSD: Single Shot MultiBox Detector](https://arxiv.org/abs/1512.02325) by Wei Liu, Dragomir Anguelov, Dumitru Erhan, Christian Szegedy, Scott Reed, Cheng-Yang Fu, Alexander C. Berg, 2016.
    /// @see [GitHub: SSD: Single Shot MultiBox Detector](https://github.com/weiliu89/caffe/tree/ssd), by weiliu89/caffe, 2016
    /// </remarks>
    public class BBoxUtility<T>
    {
        CudaDnn<T> m_cuda;
        Log m_log;


        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="cuda">Specifies the connection to Cuda and cuDNN.</param>
        /// <param name="log">Specifies the output log.</param>
        public BBoxUtility(CudaDnn<T> cuda, Log log)
        {
            m_cuda = cuda;
            m_log = log;
        }

        /// <summary>
        /// Decode a set of bounding box.
        /// </summary>
        /// <param name="rgPriorBbox">Specifies an list of prior bounding boxs.</param>
        /// <param name="rgrgfPriorVariance">Specifies the list of prior variance (must have 4 elements each of which are > 0).</param>
        /// <param name="code_type">Specifies the code type.</param>
        /// <param name="bEncodeVarianceInTarget">Specifies whether or not to encode the variance in the target.</param>
        /// <param name="rgBbox">Specifies a list of bounding boxs.</param>
        /// <returns>A list of decoded bounding box is returned.</returns>
        public List<NormalizedBBox> Decode(List<NormalizedBBox> rgPriorBbox, List<List<float>> rgrgfPriorVariance, PriorBoxParameter.CodeType code_type, bool bEncodeVarianceInTarget, bool bClip, List<NormalizedBBox> rgBbox)
        {
            m_log.CHECK_EQ(rgPriorBbox.Count, rgrgfPriorVariance.Count, "The number of prior boxes must match the number of variance lists.");
            m_log.CHECK_EQ(rgPriorBbox.Count, rgBbox.Count, "The number of prior boxes must match the number of boxes.");
            int nNumBoxes = rgPriorBbox.Count;

            if (nNumBoxes >= 1)
                m_log.CHECK_EQ(rgrgfPriorVariance[0].Count, 4, "The variance lists must have 4 items.");

            List<NormalizedBBox> rgDecodeBoxes = new List<NormalizedBBox>();

            for (int i = 0; i < nNumBoxes; i++)
            {
                NormalizedBBox decode_box = Decode(rgPriorBbox[i], rgrgfPriorVariance[i], code_type, bEncodeVarianceInTarget, bClip, rgBbox[i]);
                rgDecodeBoxes.Add(decode_box);
            }

            return rgDecodeBoxes;
        }

        /// <summary>
        /// Decode a bounding box.
        /// </summary>
        /// <param name="prior_bbox">Specifies the prior bounding box.</param>
        /// <param name="rgfPriorVariance">Specifies the prior variance (must have 4 elements each of which are > 0).</param>
        /// <param name="code_type">Specifies the code type.</param>
        /// <param name="bEncodeVarianceInTarget">Specifies whether or not to encode the variance in the target.</param>
        /// <param name="bbox">Specifies the bounding box.</param>
        /// <returns>The decoded bounding box is returned.</returns>
        public NormalizedBBox Decode(NormalizedBBox prior_bbox, List<float> rgfPriorVariance, PriorBoxParameter.CodeType code_type, bool bEncodeVarianceInTarget, bool bClip, NormalizedBBox bbox)
        {
            NormalizedBBox decode_bbox;

            switch (code_type)
            {
                case PriorBoxParameter.CodeType.CORNER:
                    if (bEncodeVarianceInTarget)
                    {
                        // Variance is encoded in target, we simply need to add the offset predictions.
                        decode_bbox = new NormalizedBBox(prior_bbox.xmin + bbox.xmin,
                                                         prior_bbox.ymin + bbox.ymin,
                                                         prior_bbox.xmax + bbox.xmax,
                                                         prior_bbox.ymax + bbox.ymax);
                    }
                    else
                    {
                        // Variance is encoded in the bbox, we need to scale the offset accordingly.
                        m_log.CHECK_EQ(rgfPriorVariance.Count, 4, "The variance must have 4 values!");
                        foreach (float fVar in rgfPriorVariance)
                        {
                            m_log.CHECK_GT(fVar, 0, "Each variance must be greater than 0.");
                        }

                        decode_bbox = new NormalizedBBox(prior_bbox.xmin + rgfPriorVariance[0] * bbox.xmin,
                                                         prior_bbox.ymin + rgfPriorVariance[1] * bbox.ymin,
                                                         prior_bbox.xmax + rgfPriorVariance[2] * bbox.xmax,
                                                         prior_bbox.ymax + rgfPriorVariance[3] * bbox.ymax);
                    }
                    break;

                case PriorBoxParameter.CodeType.CENTER_SIZE:
                    {
                        float fPriorWidth = prior_bbox.xmax - prior_bbox.xmin;
                        m_log.CHECK_GT(fPriorWidth, 0, "The prior width must be greater than zero.");
                        float fPriorHeight = prior_bbox.ymax - prior_bbox.ymin;
                        m_log.CHECK_GT(fPriorHeight, 0, "The prior height must be greater than zero.");
                        float fPriorCenterX = (prior_bbox.xmin + prior_bbox.xmax) / 2;
                        float fPriorCenterY = (prior_bbox.ymin + prior_bbox.ymax) / 2;

                        float fDecodeBboxCenterX;
                        float fDecodeBboxCenterY;
                        float fDecodeBboxWidth;
                        float fDecodeBboxHeight;

                        if (bEncodeVarianceInTarget)
                        {
                            // Variance is encoded in target, we simply need to resote the offset prdedictions.
                            fDecodeBboxCenterX = bbox.xmin * fPriorWidth + fPriorCenterX;
                            fDecodeBboxCenterY = bbox.ymin * fPriorHeight + fPriorCenterY;
                            fDecodeBboxWidth = (float)Math.Exp(bbox.xmax) * fPriorWidth;
                            fDecodeBboxHeight = (float)Math.Exp(bbox.ymax) * fPriorHeight;
                        }
                        else
                        {
                            // Variance is encoded in the bbox, we need to scale the offset accordingly.
                            fDecodeBboxCenterX = rgfPriorVariance[0] * bbox.xmin * fPriorWidth + fPriorCenterX;
                            fDecodeBboxCenterY = rgfPriorVariance[1] * bbox.ymin * fPriorHeight + fPriorCenterY;
                            fDecodeBboxWidth = (float)Math.Exp(rgfPriorVariance[2] * bbox.xmax) * fPriorWidth;
                            fDecodeBboxHeight = (float)Math.Exp(rgfPriorVariance[3] * bbox.ymax) * fPriorHeight;
                        }

                        decode_bbox = new NormalizedBBox(fDecodeBboxCenterX - fDecodeBboxWidth / 2,
                                                         fDecodeBboxCenterY - fDecodeBboxHeight / 2,
                                                         fDecodeBboxCenterX + fDecodeBboxWidth / 2,
                                                         fDecodeBboxCenterY + fDecodeBboxHeight / 2);
                    }
                    break;

                case PriorBoxParameter.CodeType.CORNER_SIZE:
                    {
                        float fPriorWidth = prior_bbox.xmax - prior_bbox.xmin;
                        m_log.CHECK_GT(fPriorWidth, 0, "The prior width must be greater than zero.");
                        float fPriorHeight = prior_bbox.ymax - prior_bbox.ymin;
                        m_log.CHECK_GT(fPriorHeight, 0, "The prior height must be greater than zero.");

                        if (bEncodeVarianceInTarget)
                        {
                            // Variance is encoded in target, we simply need to add the offset predictions.
                            decode_bbox = new NormalizedBBox(prior_bbox.xmin + bbox.xmin * fPriorWidth,
                                                             prior_bbox.ymin + bbox.ymin * fPriorHeight,
                                                             prior_bbox.xmax + bbox.xmax * fPriorWidth,
                                                             prior_bbox.ymax + bbox.ymax * fPriorHeight);
                        }
                        else
                        {
                            // Encode variance in bbox.
                            m_log.CHECK_EQ(rgfPriorVariance.Count, 4, "The variance must have 4 values!");
                            foreach (float fVar in rgfPriorVariance)
                            {
                                m_log.CHECK_GT(fVar, 0, "Each variance must be greater than 0.");
                            }

                            decode_bbox = new NormalizedBBox(prior_bbox.xmin + rgfPriorVariance[0] * bbox.xmin * fPriorWidth,
                                                             prior_bbox.ymin + rgfPriorVariance[1] * bbox.ymin * fPriorHeight,
                                                             prior_bbox.xmax + rgfPriorVariance[2] * bbox.xmax * fPriorWidth,
                                                             prior_bbox.ymax + rgfPriorVariance[3] * bbox.ymax * fPriorHeight);
                        }
                    }
                    break;

                default:
                    m_log.FAIL("Unknown code type '" + code_type.ToString());
                    return null;
            }

            decode_bbox.size = Size(decode_bbox);
            if (bClip)
                decode_bbox = Clip(decode_bbox);

            return decode_bbox;
        }

        /// <summary>
        /// Encode a bounding box.
        /// </summary>
        /// <param name="prior_bbox">Specifies the prior bounding box.</param>
        /// <param name="rgfPriorVariance">Specifies the prior variance (must have 4 elements each of which are > 0).</param>
        /// <param name="code_type">Specifies the code type.</param>
        /// <param name="bEncodeVarianceInTarget">Specifies whether or not to encode the variance in the target.</param>
        /// <param name="bbox">Specifies the bounding box.</param>
        /// <returns>The encoded bounding box is returned.</returns>
        public NormalizedBBox Encode(NormalizedBBox prior_bbox, List<float> rgfPriorVariance, PriorBoxParameter.CodeType code_type, bool bEncodeVarianceInTarget, NormalizedBBox bbox)
        {
            NormalizedBBox encode_bbox;

            switch (code_type)
            {
                case PriorBoxParameter.CodeType.CORNER:
                    if (bEncodeVarianceInTarget)
                    {
                        encode_bbox = new NormalizedBBox(bbox.xmin - prior_bbox.xmin,
                                                         bbox.ymin - prior_bbox.ymin,
                                                         bbox.xmax - prior_bbox.xmax,
                                                         bbox.ymax - prior_bbox.ymax);
                    }
                    else
                    {
                        // Encode variance in bbox.
                        m_log.CHECK_EQ(rgfPriorVariance.Count, 4, "The variance must have 4 values!");
                        foreach (float fVar in rgfPriorVariance)
                        {
                            m_log.CHECK_GT(fVar, 0, "Each variance must be greater than 0.");
                        }

                        encode_bbox = new NormalizedBBox((bbox.xmin - prior_bbox.xmin) / rgfPriorVariance[0],
                                                         (bbox.ymin - prior_bbox.ymin) / rgfPriorVariance[1],
                                                         (bbox.xmax - prior_bbox.xmax) / rgfPriorVariance[2],
                                                         (bbox.ymax - prior_bbox.ymax) / rgfPriorVariance[3]);
                    }
                    break;

                case PriorBoxParameter.CodeType.CENTER_SIZE:
                    {
                        float fPriorWidth = prior_bbox.xmax - prior_bbox.xmin;
                        m_log.CHECK_GT(fPriorWidth, 0, "The prior width must be greater than zero.");
                        float fPriorHeight = prior_bbox.ymax - prior_bbox.ymin;
                        m_log.CHECK_GT(fPriorHeight, 0, "The prior height must be greater than zero.");
                        float fPriorCenterX = (prior_bbox.xmin + prior_bbox.xmax) / 2;
                        float fPriorCenterY = (prior_bbox.ymin + prior_bbox.ymax) / 2;

                        float fBboxWidth = bbox.xmax - bbox.xmin;
                        m_log.CHECK_GT(fBboxWidth, 0, "The bbox width must be greater than zero.");
                        float fBboxHeight = bbox.ymax - bbox.ymin;
                        m_log.CHECK_GT(fBboxHeight, 0, "The bbox height must be greater than zero.");
                        float fBboxCenterX = (bbox.xmin + bbox.xmax) / 2;
                        float fBboxCenterY = (bbox.ymin + bbox.ymax) / 2;

                        if (bEncodeVarianceInTarget)
                        {
                            encode_bbox = new NormalizedBBox((fBboxCenterX - fPriorCenterX) / fPriorWidth,
                                                             (fBboxCenterY - fPriorCenterY) / fPriorHeight,
                                                             (float)Math.Log(fBboxWidth / fPriorWidth),
                                                             (float)Math.Log(fBboxHeight / fPriorHeight));
                        }
                        else
                        {
                            // Encode variance in bbox.
                            m_log.CHECK_EQ(rgfPriorVariance.Count, 4, "The variance must have 4 values!");
                            foreach (float fVar in rgfPriorVariance)
                            {
                                m_log.CHECK_GT(fVar, 0, "Each variance must be greater than 0.");
                            }

                            encode_bbox = new NormalizedBBox((fBboxCenterX - fPriorCenterX) / fPriorWidth / rgfPriorVariance[0],
                                                             (fBboxCenterY - fPriorCenterY) / fPriorHeight / rgfPriorVariance[1],
                                                             (float)Math.Log(fBboxWidth / fPriorWidth) / rgfPriorVariance[2],
                                                             (float)Math.Log(fBboxHeight / fPriorHeight) / rgfPriorVariance[3]);
                        }
                    }
                    break;

                case PriorBoxParameter.CodeType.CORNER_SIZE:
                    {
                        float fPriorWidth = prior_bbox.xmax - prior_bbox.xmin;
                        m_log.CHECK_GT(fPriorWidth, 0, "The prior width must be greater than zero.");
                        float fPriorHeight = prior_bbox.ymax - prior_bbox.ymin;
                        m_log.CHECK_GT(fPriorHeight, 0, "The prior height must be greater than zero.");
                        float fPriorCenterX = (prior_bbox.xmin + prior_bbox.xmax) / 2;
                        float fPriorCenterY = (prior_bbox.ymin + prior_bbox.ymax) / 2;

                        if (bEncodeVarianceInTarget)
                        {
                            encode_bbox = new NormalizedBBox((bbox.xmin - prior_bbox.xmin) / fPriorWidth,
                                                             (bbox.ymin - prior_bbox.ymin) / fPriorHeight,
                                                             (bbox.xmax - prior_bbox.xmax) / fPriorWidth,
                                                             (bbox.ymax - prior_bbox.ymax) / fPriorHeight);
                        }
                        else
                        {
                            // Encode variance in bbox.
                            m_log.CHECK_EQ(rgfPriorVariance.Count, 4, "The variance must have 4 values!");
                            foreach (float fVar in rgfPriorVariance)
                            {
                                m_log.CHECK_GT(fVar, 0, "Each variance must be greater than 0.");
                            }

                            encode_bbox = new NormalizedBBox((bbox.xmin - prior_bbox.xmin) / fPriorWidth / rgfPriorVariance[0],
                                                             (bbox.ymin - prior_bbox.ymin) / fPriorHeight / rgfPriorVariance[1],
                                                             (bbox.xmax - prior_bbox.xmax) / fPriorWidth / rgfPriorVariance[2],
                                                             (bbox.ymax - prior_bbox.ymax) / fPriorHeight / rgfPriorVariance[3]);
                        }
                    }
                    break;

                default:
                    m_log.FAIL("Unknown code type '" + code_type.ToString());
                    return null;
            }

            return encode_bbox;
        }

        /// <summary>
        /// Calculates the Jaccard overlap between two bounding boxes.
        /// </summary>
        /// <param name="bbox1">Specifies the first bounding box.</param>
        /// <param name="bbox2">Specifies the second bounding box.</param>
        /// <returns>The Jaccard overlap is returned.</returns>
        public float JaccardOverlap(NormalizedBBox bbox1, NormalizedBBox bbox2, bool bNormalized = true)
        {
            NormalizedBBox intersect_bbox = Intersect(bbox1, bbox2);
            float fIntersectWidth = intersect_bbox.xmax - intersect_bbox.xmin;
            float fIntersectHeight = intersect_bbox.ymax - intersect_bbox.ymin;

            if (!bNormalized)
            {
                fIntersectWidth += 1;
                fIntersectHeight += 1;
            }

            if (fIntersectWidth > 0 && fIntersectHeight > 0)
            {
                float fIntersectSize = fIntersectWidth * fIntersectHeight;
                float fBbox1Size = Size(bbox1);
                float fBbox2Size = Size(bbox2);
                return fIntersectSize / (fBbox1Size + fBbox2Size - fIntersectSize);
            }

            return 0;
        }

        public NormalizedBBox Output(NormalizedBBox bbox, SizeF szImg, ResizeParameter p)
        {
            int height = (int)szImg.Height;
            int width = (int)szImg.Width;
            NormalizedBBox temp_bbox = bbox.Clone();

            if (p != null)
            {
                float fResizeHeight = p.height;
                float fResizeWidth = p.width;
                float fResizeAspect = fResizeWidth / fResizeHeight;
                int nHeightScale = (int)p.height_scale;
                int nWidthScale = (int)p.width_scale;
                float fAspect = (float)width / (float)height;
                float fPadding;

                switch (p.resize_mode)
                {
                    case ResizeParameter.ResizeMode.WARP:
                        temp_bbox = Clip(temp_bbox);
                        return Scale(temp_bbox, height, width);

                    case ResizeParameter.ResizeMode.FIT_LARGE_SIZE_AND_PAD:
                        float fxmin = 0.0f;
                        float fymin = 0.0f;
                        float fxmax = 1.0f;
                        float fymax = 1.0f;

                        if (fAspect > fResizeAspect)
                        {
                            fPadding = (fResizeHeight - fResizeWidth / fAspect) / 2;
                            fymin = fPadding / fResizeHeight;
                            fymax = 1.0f - fPadding / fResizeHeight;
                        }
                        else
                        {
                            fPadding = (fResizeWidth - fResizeHeight * fAspect) / 2;
                            fxmin = fPadding / fResizeWidth;
                            fxmax = 1.0f - fPadding / fResizeWidth;
                        }

                        Project(new NormalizedBBox(fxmin, fymin, fxmax, fymax), bbox, out temp_bbox);
                        temp_bbox = Clip(temp_bbox);
                        return Scale(temp_bbox, height, width);

                    case ResizeParameter.ResizeMode.FIT_SMALL_SIZE:
                        if (nHeightScale == 0 || nWidthScale == 0)
                        {
                            temp_bbox = Clip(temp_bbox);
                            return Scale(temp_bbox, height, width);
                        }
                        else
                        {
                            temp_bbox = Scale(temp_bbox, nHeightScale, nWidthScale);
                            return Clip(temp_bbox, height, width);
                        }

                    default:
                        m_log.FAIL("Unknown resize mode '" + p.resize_mode.ToString() + "'!");
                        return null;
                }
            }
            else
            {
                // Clip the normalized bbox first.
                temp_bbox = Clip(temp_bbox);
                // Scale the bbox according to the original image size.
                return Scale(temp_bbox, height, width);
            }
        }

        /// <summary>
        /// Project one bbox onto another.
        /// </summary>
        /// <param name="src">Specifies the source bbox.</param>
        /// <param name="bbox">Specifies the second bbox.</param>
        /// <returns>The new project bbox is returned if a projection was made, otherwise the original bbox is returned.</returns>
        public bool Project(NormalizedBBox src, NormalizedBBox bbox, out NormalizedBBox proj_bbox)
        {
            proj_bbox = bbox.Clone();

            if (bbox.xmin >= src.xmax || bbox.xmax <= src.xmin ||
                bbox.ymin >= src.ymax || bbox.ymax <= src.ymin)
                return false;

            float src_width = src.xmax - src.xmin;
            float src_height = src.ymax - src.ymin;
            proj_bbox = new NormalizedBBox((bbox.xmin - src.xmin) / src_width,
                                           (bbox.ymin - src.ymin) / src_height,
                                           (bbox.xmax - src.xmin) / src_width,
                                           (bbox.ymax - src.ymin) / src_height,
                                           bbox.label, bbox.difficult);
            proj_bbox = Clip(proj_bbox);

            float fSize = Size(proj_bbox);
            if (fSize > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Create the intersection of two bounding boxes.
        /// </summary>
        /// <param name="bbox1">Specifies the first bounding box.</param>
        /// <param name="bbox2">Specifies the second bounding box.</param>
        /// <returns>The intersection of the two bounding boxes is returned.</returns>
        public NormalizedBBox Intersect(NormalizedBBox bbox1, NormalizedBBox bbox2)
        {
            // Return [0,0,0,0] if there is no intersection.
            if (bbox2.xmin > bbox1.xmax || bbox2.xmax < bbox1.xmin ||
                bbox2.ymin > bbox1.ymax || bbox2.ymax < bbox1.ymin)
                return new NormalizedBBox(0.0f, 0.0f, 0.0f, 0.0f);

            return new NormalizedBBox(Math.Max(bbox1.xmin, bbox2.xmin),
                                      Math.Max(bbox1.ymin, bbox2.ymin),
                                      Math.Min(bbox1.xmax, bbox2.xmax),
                                      Math.Min(bbox1.ymax, bbox2.ymax));
        }

        /// <summary>
        /// Clip the BBox to a set range.
        /// </summary>
        /// <param name="bbox">Specifies the input bounding box.</param>
        /// <param name="fHeight">Specifies the clipping height.</param>
        /// <param name="fWidth">Specifies the clipping width.</param>
        /// <returns>A new, clipped NormalizedBBox is returned.</returns>
        public NormalizedBBox Clip(NormalizedBBox bbox, float fHeight = 1.0f, float fWidth = 1.0f)
        {
            NormalizedBBox clipped = bbox.Clone();
            clipped.xmin = Math.Max(Math.Min(bbox.xmin, fWidth), 0.0f);
            clipped.ymin = Math.Max(Math.Min(bbox.ymin, fHeight), 0.0f);
            clipped.xmax = Math.Max(Math.Min(bbox.xmax, fWidth), 0.0f);
            clipped.ymax = Math.Max(Math.Min(bbox.ymax, fHeight), 0.0f);
            clipped.size = Size(clipped);
            return clipped;
        }

        /// <summary>
        /// Scale the BBox to a set range.
        /// </summary>
        /// <param name="bbox">Specifies the input bounding box.</param>
        /// <param name="fHeight">Specifies the scaling height.</param>
        /// <param name="fWidth">Specifies the scaling width.</param>
        /// <returns>A new, scaled NormalizedBBox is returned.</returns>
        public NormalizedBBox Scale(NormalizedBBox bbox, int nHeight, int nWidth)
        {
            NormalizedBBox scaled = bbox.Clone();
            scaled.xmin = bbox.xmin * nWidth;
            scaled.ymin = bbox.ymin * nHeight;
            scaled.xmax = bbox.xmax * nWidth;
            scaled.ymax = bbox.ymax * nHeight;
            bool bNormalized = !(nWidth > 1 || nHeight > 1);
            scaled.size = Size(scaled, bNormalized);
            return scaled;
        }

        /// <summary>
        /// Calculate the size of a BBox.
        /// </summary>
        /// <param name="bbox">Specifies the input bounding box.</param>
        /// <param name="bNormalized">Specifies whether or not the bounding box falls within the range [0,1].</param>
        /// <returns>The size of the bounding box is returned.</returns>
        public float Size(NormalizedBBox bbox, bool bNormalized = true)
        {
            if (bbox.xmax < bbox.xmin || bbox.ymax < bbox.ymin)
            {
                // If bbox is invalid (e.g. xmax < xmin or ymax < ymin), return 0
                return 0f;
            }

            float fWidth = bbox.xmax - bbox.xmin;
            float fHeight = bbox.ymax - bbox.ymin;

            if (bNormalized)
                return fWidth * fHeight;
            else // bbox is not in range [0,1]
                return (fWidth + 1) * (fHeight + 1);
        }
    }
}
