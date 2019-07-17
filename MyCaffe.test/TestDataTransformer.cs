﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyCaffe.basecode;
using MyCaffe.param;
using MyCaffe.common;
using MyCaffe.layers;
using MyCaffe.data;

namespace MyCaffe.test
{
    [TestClass]
    public class TestDataTransformer
    {
        [TestMethod]
        public void TestSetRangeNone()
        {
            DataTransformerTest test = new DataTransformerTest();

            try
            {
                foreach (IDataTransformerTest t in test.Tests)
                {
                    t.TestSetRangeNone();
                }
            }
            finally
            {
                test.Dispose();
            }
        }

        [TestMethod]
        public void TestSetRange()
        {
            DataTransformerTest test = new DataTransformerTest();

            try
            {
                foreach (IDataTransformerTest t in test.Tests)
                {
                    t.TestSetRange();
                }
            }
            finally
            {
                test.Dispose();
            }
        }

        [TestMethod]
        public void TestSetRangeBig()
        {
            DataTransformerTest test = new DataTransformerTest();

            try
            {
                foreach (IDataTransformerTest t in test.Tests)
                {
                    t.TestSetRangeBig();
                }
            }
            finally
            {
                test.Dispose();
            }
        }

        [TestMethod]
        public void TestEmptyTransform()
        {
            DataTransformerTest test = new DataTransformerTest();

            try
            {
                foreach (IDataTransformerTest t in test.Tests)
                {
                    t.TestEmptyTransform();
                }
            }
            finally
            {
                test.Dispose();
            }
        }

        [TestMethod]
        public void TestEmptyTransformUniquePixels()
        {
            DataTransformerTest test = new DataTransformerTest();

            try
            {
                foreach (IDataTransformerTest t in test.Tests)
                {
                    t.TestEmptyTransformUniquePixels();
                }
            }
            finally
            {
                test.Dispose();
            }
        }

        [TestMethod]
        public void TestCropSize()
        {
            DataTransformerTest test = new DataTransformerTest();

            try
            {
                foreach (IDataTransformerTest t in test.Tests)
                {
                    t.TestCropSize();
                }
            }
            finally
            {
                test.Dispose();
            }
        }

        [TestMethod]
        public void TestCropTrain()
        {
            DataTransformerTest test = new DataTransformerTest();

            try
            {
                foreach (IDataTransformerTest t in test.Tests)
                {
                    t.TestCropTrain();
                }
            }
            finally
            {
                test.Dispose();
            }
        }

        [TestMethod]
        public void TestCropTest()
        {
            DataTransformerTest test = new DataTransformerTest();

            try
            {
                foreach (IDataTransformerTest t in test.Tests)
                {
                    t.TestCropTest();
                }
            }
            finally
            {
                test.Dispose();
            }
        }

        [TestMethod]
        public void TestMirrorTrain()
        {
            DataTransformerTest test = new DataTransformerTest();

            try
            {
                foreach (IDataTransformerTest t in test.Tests)
                {
                    t.TestMirrorTrain();
                }
            }
            finally
            {
                test.Dispose();
            }
        }

        [TestMethod]
        public void TestMirrorTest()
        {
            DataTransformerTest test = new DataTransformerTest();

            try
            {
                foreach (IDataTransformerTest t in test.Tests)
                {
                    t.TestMirrorTest();
                }
            }
            finally
            {
                test.Dispose();
            }
        }

        [TestMethod]
        public void TestCropMirrorTest()
        {
            DataTransformerTest test = new DataTransformerTest();

            try
            {
                foreach (IDataTransformerTest t in test.Tests)
                {
                    t.TestCropMirrorTest();
                }
            }
            finally
            {
                test.Dispose();
            }
        }

        [TestMethod]
        public void TestMeanValue()
        {
            DataTransformerTest test = new DataTransformerTest();

            try
            {
                foreach (IDataTransformerTest t in test.Tests)
                {
                    t.TestMeanValue();
                }
            }
            finally
            {
                test.Dispose();
            }
        }

        [TestMethod]
        public void TestMeanValues()
        {
            DataTransformerTest test = new DataTransformerTest();

            try
            {
                foreach (IDataTransformerTest t in test.Tests)
                {
                    t.TestMeanValues();
                }
            }
            finally
            {
                test.Dispose();
            }
        }
    }


    interface IDataTransformerTest : ITest
    {
        void TestSetRangeNone();
        void TestSetRange();
        void TestSetRangeBig();
        void TestEmptyTransform();
        void TestEmptyTransformUniquePixels();
        void TestCropSize();
        void TestCropTrain();
        void TestCropTest();
        void TestMirrorTrain();
        void TestMirrorTest();
        void TestCropMirrorTrain();
        void TestCropMirrorTest();
        void TestMeanValue();
        void TestMeanValues();
    }

    class DataTransformerTest : TestBase
    {
        public DataTransformerTest(EngineParameter.Engine engine = EngineParameter.Engine.DEFAULT)
            : base("Data Transformer Test", TestBase.DEFAULT_DEVICE_ID, engine)
        {
        }

        protected override ITest create(common.DataType dt, string strName, int nDeviceID, EngineParameter.Engine engine)
        {
            if (dt == common.DataType.DOUBLE)
                return new DataTransformerTest<double>(strName, nDeviceID, engine);
            else
                return new DataTransformerTest<float>(strName, nDeviceID, engine);
        }
    }

    class DataTransformerTest<T> : TestEx<T>, IDataTransformerTest
    {
        int m_nNumIter = 10;

        public DataTransformerTest(string strName, int nDeviceID, EngineParameter.Engine engine)
            : base(strName, new List<int>() { 10, 1, 1, 1 }, nDeviceID)
        {
            m_engine = engine;
        }

        public void TestSetRangeNone()
        {
            TransformationParameter p = new TransformationParameter();
            DataTransformer<T> transformer = new DataTransformer<T>(m_cuda, m_log, p, Phase.TRAIN, 3, 56, 56);

            double[] rgData = convert(m_blob_bottom.mutable_cpu_data);
            double dfVal = -5;

            for (int i=0; i<rgData.Length; i++)
            {
                rgData[i] = dfVal;

                dfVal += 1;
                if (dfVal == 0)
                    dfVal += 1;
            }

            m_blob_bottom.mutable_cpu_data = convert(rgData);
            transformer.SetRange(m_blob_bottom);

            rgData = convert(m_blob_bottom.mutable_cpu_data);
            dfVal = -5;

            for (int i = 0; i < rgData.Length; i++)
            {
                m_log.CHECK_EQ(rgData[i], dfVal, "The values are not as expected!");

                dfVal += 1;
                if (dfVal == 0)
                    dfVal += 1;
            }
        }

        public void TestSetRange()
        {
            TransformationParameter p = new TransformationParameter();
            p.forced_positive_range_max = 10.0;
            DataTransformer<T> transformer = new DataTransformer<T>(m_cuda, m_log, p, Phase.TRAIN, 3, 56, 56);

            double[] rgData = convert(m_blob_bottom.mutable_cpu_data);
            double dfVal = -5;

            for (int i = 0; i < rgData.Length; i++)
            {
                rgData[i] = dfVal;

                dfVal += 1;
                if (dfVal == 0)
                    dfVal += 1;
            }

            m_blob_bottom.mutable_cpu_data = convert(rgData);
            transformer.SetRange(m_blob_bottom);

            rgData = convert(m_blob_bottom.mutable_cpu_data);
            dfVal = -5;

            for (int i = 0; i < rgData.Length; i++)
            {
                double dfScale = (10.0) / (5.0 - -5.0);
                double dfMin = -5.0;
                double dfExpected = (dfVal - dfMin) * dfScale;
                m_log.CHECK_EQ(rgData[i], dfExpected, "The values are not as expected!");

                dfVal += 1;
                if (dfVal == 0)
                    dfVal += 1;
            }
        }

        public void TestSetRangeBig()
        {
            TransformationParameter p = new TransformationParameter();
            p.forced_positive_range_max = 10.0;
            DataTransformer<T> transformer = new DataTransformer<T>(m_cuda, m_log, p, Phase.TRAIN, 3, 56, 56);
            Blob<T> b = new Blob<T>(m_cuda, m_log, 128, 3, 56, 56);

            double[] rgData = convert(b.mutable_cpu_data);
            double dfVal = -5;

            for (int i = 0; i < rgData.Length; i++)
            {                
                rgData[i] = dfVal;

                dfVal += 1;
                if (dfVal == 0)
                    dfVal += 1;

                if (dfVal > 5)
                    dfVal = -5;
            }

            b.mutable_cpu_data = convert(rgData);
            transformer.SetRange(b);

            rgData = convert(b.mutable_cpu_data);
            dfVal = -5;

            for (int i = 0; i < rgData.Length; i++)
            {
                double dfScale = (10.0) / (5.0 - -5.0);
                double dfMin = -5.0;
                double dfExpected = (dfVal - dfMin) * dfScale;
                m_log.CHECK_EQ(rgData[i], dfExpected, "The values are not as expected!");

                dfVal += 1;
                if (dfVal == 0)
                    dfVal += 1;

                if (dfVal > 5)
                    dfVal = -5;
            }
        }

        public Datum CreateDatum(int nLabel, int nChannels, int nHeight, int nWidth, bool bUniquePixels)
        {
            List<byte> rgData = new List<byte>();

            int nCount = nChannels * nHeight * nWidth;
            for (int i = 0; i < nCount; i++)
            {
                rgData.Add((byte)(bUniquePixels ? i : nLabel));
            }

            return new Datum(false, nChannels, nWidth, nHeight, nLabel, DateTime.Today, rgData, null, 0, false, 0);
        }

        public int NumSequenceMatches(TransformationParameter p, Datum d, Phase phase)
        {
            // Get crop sequence with Caffe seed 1701
            p.random_seed = 1701;
            DataTransformer<T> transformer = new DataTransformer<T>(m_cuda, m_log, p, phase, d.channels, d.height, d.width);
            int nCropSize = (int)p.crop_size;

            transformer.InitRand();
            Blob<T> blob = new Blob<T>(m_cuda, m_log, 1, d.channels, d.height, d.width);

            if (p.crop_size > 0)
                blob.Reshape(1, d.channels, nCropSize, nCropSize);

            List<double[]> rgdfdfCropSequence = new List<double[]>();

            for (int i = 0; i < m_nNumIter; i++)
            {
                List<double> rgIterCropSequence = new List<double>();
                transformer.Transform(d, blob);
                rgdfdfCropSequence.Add(convert(blob.update_cpu_data()));
            }

            // Check if the sequence differs from the previous
            int nNumSequenceMatches = 0;
            for (int i = 0; i < m_nNumIter; i++)
            {
                blob.mutable_cpu_data = transformer.Transform(d);

                double[] rgData = convert(blob.update_cpu_data());

                for (int j = 0; j < blob.count(); j++)
                {
                    if (rgdfdfCropSequence[i][j] == rgData[j])
                        nNumSequenceMatches++;
                }
            }

            blob.Dispose();

            return nNumSequenceMatches;
        }

        public void TestEmptyTransform()
        {
            TransformationParameter p = new TransformationParameter();
            bool unique_pixels = false; // all pixels the same equal to label.
            int nLabel = 0;
            int nChannels = 3;
            int nHeight = 4;
            int nWidth = 5;

            Datum datum = CreateDatum(nLabel, nChannels, nHeight, nWidth, unique_pixels);
            Blob<T> blob = new Blob<T>(m_cuda, m_log, 1, nChannels, nHeight, nWidth);
            DataTransformer<T> transformer = new DataTransformer<T>(m_cuda, m_log, p, Phase.TEST, nChannels, nHeight, nWidth);
            transformer.InitRand();
            transformer.Transform(datum, blob);

            m_log.CHECK_EQ(blob.num, 1, "The blob num should equal 1.");
            m_log.CHECK_EQ(blob.channels, nChannels, "The blob channels should equal " + nChannels.ToString());
            m_log.CHECK_EQ(blob.height, nHeight, "The blob height should equal " + nHeight.ToString());
            m_log.CHECK_EQ(blob.width, nWidth, "The blob width should equal " + nWidth.ToString());

            double[] rgData = convert(blob.update_cpu_data());

            for (int j = 0; j < blob.count(); j++)
            {
                double dfVal = rgData[j];
                m_log.CHECK_EQ(dfVal, nLabel, "The data value at " + j.ToString() + " should equal the label " + nLabel.ToString());
            }

            blob.Dispose();
        }

        public void TestEmptyTransformUniquePixels()
        {
            TransformationParameter p = new TransformationParameter();
            bool unique_pixels = true; 
            int nLabel = 0;
            int nChannels = 3;
            int nHeight = 4;
            int nWidth = 5;

            Datum datum = CreateDatum(nLabel, nChannels, nHeight, nWidth, unique_pixels);
            Blob<T> blob = new Blob<T>(m_cuda, m_log, 1, nChannels, nHeight, nWidth);
            DataTransformer<T> transformer = new DataTransformer<T>(m_cuda, m_log, p, Phase.TEST, nChannels, nHeight, nWidth);
            transformer.InitRand();
            transformer.Transform(datum, blob);

            m_log.CHECK_EQ(blob.num, 1, "The blob num should equal 1.");
            m_log.CHECK_EQ(blob.channels, nChannels, "The blob channels should equal " + nChannels.ToString());
            m_log.CHECK_EQ(blob.height, nHeight, "The blob height should equal " + nHeight.ToString());
            m_log.CHECK_EQ(blob.width, nWidth, "The blob width should equal " + nWidth.ToString());

            double[] rgData = convert(blob.update_cpu_data());

            for (int j = 0; j < blob.count(); j++)
            {
                double dfVal = rgData[j];
                m_log.CHECK_EQ(dfVal, j, "The data value at " + j.ToString() + " should equal the label " + nLabel.ToString());
            }

            blob.Dispose();
        }

        public void TestCropSize()
        {
            TransformationParameter p = new TransformationParameter();
            bool unique_pixels = false; // all pixels the same equal to label.
            int nLabel = 0;
            int nChannels = 3;
            int nHeight = 4;
            int nWidth = 5;
            int nCropSize = 2;

            p.crop_size = (uint)nCropSize;
            Datum datum = CreateDatum(nLabel, nChannels, nHeight, nWidth, unique_pixels);
            Blob<T> blob = new Blob<T>(m_cuda, m_log, 1, nChannels, nCropSize, nCropSize);
            DataTransformer<T> transformer = new DataTransformer<T>(m_cuda, m_log, p, Phase.TEST, nChannels, nHeight, nWidth);
            transformer.InitRand();

            for (int i = 0; i < m_nNumIter; i++)
            {
                transformer.Transform(datum, blob);

                m_log.CHECK_EQ(blob.num, 1, "The blob num should equal 1.");
                m_log.CHECK_EQ(blob.channels, nChannels, "The blob channels should equal " + nChannels.ToString());
                m_log.CHECK_EQ(blob.height, nCropSize, "The blob height should equal " + nCropSize.ToString());
                m_log.CHECK_EQ(blob.width, nCropSize, "The blob width should equal " + nCropSize.ToString());

                double[] rgData = convert(blob.update_cpu_data());

                for (int j = 0; j < blob.count(); j++)
                {
                    double dfVal = rgData[j];
                    m_log.CHECK_EQ(dfVal, nLabel, "The data value at " + j.ToString() + " should equal the label " + nLabel.ToString());
                }
            }

            blob.Dispose();
        }

        public void TestCropTrain()
        {
            TransformationParameter p = new TransformationParameter();
            bool unique_pixels = true; 
            int nLabel = 0;
            int nChannels = 3;
            int nHeight = 4;
            int nWidth = 5;
            int nCropSize = 2;
            int nSize = nChannels * nCropSize * nCropSize;

            p.crop_size = (uint)nCropSize;
            Datum datum = CreateDatum(nLabel, nChannels, nHeight, nWidth, unique_pixels);
            int nNumMatches = NumSequenceMatches(p, datum, Phase.TRAIN);

            m_log.CHECK_LT(nNumMatches, nSize * m_nNumIter, "The number of sequence matches is not as expected.");
        }

        public void TestCropTest()
        {
            TransformationParameter p = new TransformationParameter();
            bool unique_pixels = true;
            int nLabel = 0;
            int nChannels = 3;
            int nHeight = 4;
            int nWidth = 5;
            int nCropSize = 2;
            int nSize = nChannels * nCropSize * nCropSize;

            p.crop_size = (uint)nCropSize;
            Datum datum = CreateDatum(nLabel, nChannels, nHeight, nWidth, unique_pixels);
            int nNumMatches = NumSequenceMatches(p, datum, Phase.TEST);

            m_log.CHECK_EQ(nNumMatches, nSize * m_nNumIter, "The number of sequence matches is not as expected.");
        }

        public void TestMirrorTrain()
        {
            TransformationParameter p = new TransformationParameter();
            bool unique_pixels = true;
            int nLabel = 0;
            int nChannels = 3;
            int nHeight = 4;
            int nWidth = 5;
            int nSize = nChannels * nHeight * nWidth;

            p.mirror = true;
            Datum datum = CreateDatum(nLabel, nChannels, nHeight, nWidth, unique_pixels);
            int nNumMatches = NumSequenceMatches(p, datum, Phase.TRAIN);

            m_log.CHECK_LT(nNumMatches, nSize * m_nNumIter, "The number of sequence matches is not as expected.");
        }

        public void TestMirrorTest()
        {
            TransformationParameter p = new TransformationParameter();
            bool unique_pixels = true;
            int nLabel = 0;
            int nChannels = 3;
            int nHeight = 4;
            int nWidth = 5;
            int nSize = nChannels * nHeight * nWidth;

            p.mirror = true;
            Datum datum = CreateDatum(nLabel, nChannels, nHeight, nWidth, unique_pixels);
            int nNumMatches = NumSequenceMatches(p, datum, Phase.TEST);

            m_log.CHECK_LT(nNumMatches, nSize * m_nNumIter, "The number of sequence matches is not as expected.");
        }

        public void TestCropMirrorTrain()
        {
            TransformationParameter p = new TransformationParameter();
            bool unique_pixels = true;
            int nLabel = 0;
            int nChannels = 3;
            int nHeight = 4;
            int nWidth = 5;
            int nCropSize = 2;
            int nSize = nChannels * nCropSize * nCropSize;

            p.crop_size = (uint)nCropSize;
            Datum datum = CreateDatum(nLabel, nChannels, nHeight, nWidth, unique_pixels);
            int nNumMatchesCrop = NumSequenceMatches(p, datum, Phase.TRAIN);

            p.mirror = true;
            int nNumMatchesCropMirror = NumSequenceMatches(p, datum, Phase.TRAIN);

            // When doing crop and mirror we expect less num_matches than just crop
            m_log.CHECK_LT(nNumMatchesCropMirror, nNumMatchesCrop, "The number of sequence matches is not as expected.");
        }

        public void TestCropMirrorTest()
        {
            TransformationParameter p = new TransformationParameter();
            bool unique_pixels = true; // pixels eare consecutive ints [0, size]
            int nLabel = 0;
            int nChannels = 3;
            int nHeight = 4;
            int nWidth = 5;
            int nCropSize = 2;
            int nSize = nChannels * nCropSize * nCropSize;

            p.crop_size = (uint)nCropSize;
            Datum datum = CreateDatum(nLabel, nChannels, nHeight, nWidth, unique_pixels);
            int nNumMatchesCrop = NumSequenceMatches(p, datum, Phase.TEST);

            p.mirror = true;
            int nNumMatchesCropMirror = NumSequenceMatches(p, datum, Phase.TEST);

            // When doing crop and mirror we expect less num_matches than just crop
            m_log.CHECK_LT(nNumMatchesCropMirror, nNumMatchesCrop, "The number of sequence matches is not as expected.");
        }

        public void TestMeanValue()
        {
            TransformationParameter p = new TransformationParameter();
            bool unique_pixels = false; // pixels are equal to label
            int nLabel = 0;
            int nChannels = 3;
            int nHeight = 4;
            int nWidth = 5;
            int nMeanValue = 2;

            p.mean_value.Add(nMeanValue);
            Datum datum = CreateDatum(nLabel, nChannels, nHeight, nWidth, unique_pixels);
            Blob<T> blob = new Blob<T>(m_cuda, m_log, 1, nChannels, nHeight, nWidth);
            DataTransformer<T> transformer = new DataTransformer<T>(m_cuda, m_log, p, Phase.TEST, nChannels, nHeight, nWidth);
            transformer.InitRand();
            blob.mutable_cpu_data = transformer.Transform(datum);

            double[] rgData = convert(blob.update_cpu_data());

            for (int j = 0; j < blob.count(); j++)
            {
                double dfVal = rgData[j];

                m_log.CHECK_EQ(dfVal, nLabel - nMeanValue, "The data at " + j.ToString() + " is not as expected.");
            }
        }

        public void TestMeanValues()
        {
            TransformationParameter p = new TransformationParameter();
            bool unique_pixels = false; // pixels are equal to label
            int nLabel = 0;
            int nChannels = 3;
            int nHeight = 4;
            int nWidth = 5;

            p.mean_value.Add(0);
            p.mean_value.Add(1);
            p.mean_value.Add(2);
            Datum datum = CreateDatum(nLabel, nChannels, nHeight, nWidth, unique_pixels);
            Blob<T> blob = new Blob<T>(m_cuda, m_log, 1, nChannels, nHeight, nWidth);
            DataTransformer<T> transformer = new DataTransformer<T>(m_cuda, m_log, p, Phase.TEST, nChannels, nHeight, nWidth);
            transformer.InitRand();
            blob.mutable_cpu_data = transformer.Transform(datum);

            double[] rgData = convert(blob.update_cpu_data());

            for (int c = 0; c < nChannels; c++)
            {
                for (int j = 0; j < nHeight * nWidth; j++)
                {
                    int nIdx = blob.offset(0, c) + j;
                    double dfVal = rgData[nIdx];

                    m_log.CHECK_EQ(dfVal, nLabel - c, "The data at " + j.ToString() + " is not as expected.");
                }
            }
        }
    }
}
