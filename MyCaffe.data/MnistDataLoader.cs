﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCaffe.basecode;
using MyCaffe.db.image;
using MyCaffe.basecode.descriptors;
using System.Threading;

namespace MyCaffe.data
{
    /// <summary>
    /// The MnistDataLoader is used to create the MNIST dataset and load it into the database managed by the MyCaffe Image Database.
    /// </summary>
    /// <remarks>
    /// @see [MNIST Dataset](http://yann.lecun.com/exdb/mnist/)
    /// </remarks>
    public class MnistDataLoader
    {
        MnistDataParameters m_param;
        DatasetFactory m_factory = new DatasetFactory();
        Log m_log;
        CancelEvent m_evtCancel;

        /// <summary>
        /// The OnProgress event fires during the creation process to show the progress.
        /// </summary>
        public event EventHandler<ProgressArgs> OnProgress;
        /// <summary>
        /// The OnError event fires when an error occurs.
        /// </summary>
        public event EventHandler<ProgressArgs> OnError;
        /// <summary>
        /// The OnComplete event fires once the dataset creation has completed.
        /// </summary>
        public event EventHandler OnCompleted;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="param">Specifies the creation parameters.</param>
        /// <param name="log">Specifies the output log used to show status updates.</param>
        /// <param name="evtCancel">Specifies the cancel event used to abort the creation process.</param>
        public MnistDataLoader(MnistDataParameters param, Log log, CancelEvent evtCancel)
        {
            m_param = param;
            m_log = log;
            m_evtCancel = evtCancel;
            m_evtCancel.Reset();
        }

        /// <summary>
        /// Create the dataset and load it into the database.
        /// </summary>
        /// <returns>On successful creation, <i>true</i> is returned, otherwise <i>false</i> is returned on abort.</returns>
        public bool LoadDatabase()
        {
            int nIdx = 0;
            int nTotal = 0;

            try
            {
                reportProgress(nIdx, nTotal, "Unpacking files...");
                string strTrainImagesBin = expandFile(m_param.TrainImagesFile);
                string strTrainLabelsBin = expandFile(m_param.TrainLabelsFile);
                string strTestImagesBin = expandFile(m_param.TestImagesFile);
                string strTestLabelsBin = expandFile(m_param.TestLabelsFile);

                reportProgress(nIdx, nTotal, "Loading MNIST database...");

                DatasetFactory factory = new DatasetFactory();

                int nSrcId = factory.GetSourceID("MNIST.training");
                if (nSrcId != 0)
                    factory.DeleteSourceData(nSrcId);

                if (!loadFile(strTrainImagesBin, strTrainLabelsBin, "MNIST.training"))
                    return false;

                nSrcId = factory.GetSourceID("MNIST.testing");
                if (nSrcId != 0)
                    factory.DeleteSourceData(nSrcId);

                if (!loadFile(strTestImagesBin, strTestLabelsBin, "MNIST.testing"))
                    return false;

                SourceDescriptor srcTrain = factory.LoadSource("MNIST.training");
                SourceDescriptor srcTest = factory.LoadSource("MNIST.testing");
                DatasetDescriptor ds = new DatasetDescriptor(0, "MNIST", null, null, srcTrain, srcTest, "MNIST", "MNIST Character Dataset");
                factory.AddDataset(ds);
                factory.UpdateDatasetCounts(ds.ID);

                return true;
            }
            catch (Exception excpt)
            {
                throw excpt;
            }
            finally
            {
                if (OnCompleted != null)
                    OnCompleted(this, new EventArgs());
            }
        }

        private bool loadFile(string strImagesFile, string strLabelsFile, string strSourceName)
        {
            Stopwatch sw = new Stopwatch();

            reportProgress(0, 0, " Source: " + strSourceName);
            reportProgress(0, 0, "  loading " + strImagesFile + "...");

            BinaryFile image_file = new BinaryFile(strImagesFile);
            BinaryFile label_file = new BinaryFile(strLabelsFile);

            try
            {
                // Verify the files
                uint magicImg = image_file.ReadUInt32();
                uint magicLbl = label_file.ReadUInt32();

                if (magicImg != 2051)
                    throw new Exception("Incorrect image file magic.");

                if (magicLbl != 2049)
                    throw new Exception("Incorrect label file magic.");

                uint num_items = image_file.ReadUInt32();
                uint num_labels = label_file.ReadUInt32();

                if (num_items != num_labels)
                    throw new Exception("The number of items must be equal to the number of labels!");


                // Add the data source to the database.
                uint rows = image_file.ReadUInt32();
                uint cols = image_file.ReadUInt32();
                int nChannels = 1;  // black and white

                int nSrcId = m_factory.AddSource(strSourceName, nChannels, (int)cols, (int)rows, false, 0, true);

                m_factory.Open(nSrcId, 500, false, m_log);
                m_factory.DeleteSourceData();

                // Storing to database;
                byte[] rgLabel;
                byte[] rgPixels;

                Datum datum = new Datum(false, nChannels, (int)cols, (int)rows, -1, DateTime.MinValue, null, null, 0, false, -1);

                reportProgress(0, (int)num_items, "  loading a total of " + num_items.ToString() + " items.");
                reportProgress(0, (int)num_items, "   (with rows: " + rows.ToString() + ", cols: " + cols.ToString() + ")");

                sw.Start();

                List<SimpleDatum> rgImg = new List<SimpleDatum>();

                for (int i = 0; i < num_items; i++)
                {
                    rgPixels = image_file.ReadBytes((int)(rows * cols));
                    rgLabel = label_file.ReadBytes(1);

                    if (sw.Elapsed.TotalMilliseconds > 1000)
                    {
                        reportProgress(i, (int)num_items, " loading data...");
                        sw.Restart();
                    }

                    datum.SetData(rgPixels.ToList(), (int)rgLabel[0]);
                    m_factory.PutRawImageCache(i, datum);
                    rgImg.Add(new SimpleDatum(datum));

                    if (m_evtCancel.WaitOne(0))
                        return false;
                }

                m_factory.ClearImageCashe(true);
                m_factory.UpdateSourceCounts();
                m_factory.SaveImageMean(SimpleDatum.CalculateMean(m_log, rgImg.ToArray(), new WaitHandle[] { new ManualResetEvent(false) }), true);

                reportProgress((int)num_items, (int)num_items, " loading completed.");
            }
            finally
            {
                image_file.Dispose();
                label_file.Dispose();
            }

            return true;
        }

        private void Log_OnWriteLine(object sender, LogArg e)
        {
            reportProgress((int)(e.Progress * 1000), 1000, e.Message);
        }

        private string expandFile(string strFile)
        {
            FileInfo fi = new FileInfo(strFile);
            string strNewFile = fi.DirectoryName;
            int nPos = fi.Name.LastIndexOf('.');

            if (nPos >= 0)
                strNewFile += "\\" + fi.Name.Substring(0, nPos) + ".bin";
            else
                strNewFile += "\\" + fi.Name + ".bin";

            if (!File.Exists(strNewFile))
            {
                using (FileStream fs = fi.OpenRead())
                {
                    using (FileStream fsBin = File.Create(strNewFile))
                    {
                        using (GZipStream decompStrm = new GZipStream(fs, CompressionMode.Decompress))
                        {
                            decompStrm.CopyTo(fsBin);
                        }
                    }
                }
            }

            return strNewFile;
        }

        private void reportProgress(int nIdx, int nTotal, string strMsg)
        {
            if (OnProgress != null)
                OnProgress(this, new ProgressArgs(new ProgressInfo(nIdx, nTotal, strMsg)));
        }

        private void reportError(int nIdx, int nTotal, Exception err)
        {
            if (OnError != null)
                OnError(this, new ProgressArgs(new ProgressInfo(nIdx, nTotal, "ERROR", err)));
        }
    }

    public class ProgressArgs : EventArgs
    {
        ProgressInfo m_pi;

        public ProgressArgs(ProgressInfo pi)
        {
            m_pi = pi;
        }

        public ProgressInfo Progress
        {
            get { return m_pi; }
        }
    }
}