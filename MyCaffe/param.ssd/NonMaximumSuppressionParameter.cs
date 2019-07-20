﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCaffe.basecode;

namespace MyCaffe.param.ssd
{
    /// <summary>
    /// Specifies the parameters for the NonMaximumSuppressionParameter used with SSD.
    /// </summary>
    /// <remarks>
    /// @see [SSD: Single Shot MultiBox Detector](https://arxiv.org/abs/1512.02325) by Wei Liu, Dragomir Anguelov, Dumitru Erhan, Christian Szegedy, Scott Reed, Cheng-Yang Fu, Alexander C. Berg, 2016.
    /// @see [GitHub: SSD: Single Shot MultiBox Detector](https://github.com/weiliu89/caffe/tree/ssd), by weiliu89/caffe, 2016
    /// </remarks>
    public class NonMaximumSuppressionParameter
    {
        float m_fNmsThreshold = 0.3f;
        int m_nTopK = 1;
        float m_fEta = 1.0f;

        /// <summary>
        /// The constructor.
        /// </summary>
        public NonMaximumSuppressionParameter()
        {
        }

        /// <summary>
        /// Get/set the threshold to be used in nms.
        /// </summary>
        public float nms_threshold
        {
            get { return m_fNmsThreshold; }
            set { m_fNmsThreshold = value; }
        }

        /// <summary>
        /// Get/set the maximum number of results kept.
        /// </summary>
        public int top_k
        {
            get { return m_nTopK; }
            set { m_nTopK = value; }
        }

        /// <summary>
        /// Get/set the parameter for adaptive nms.
        /// </summary>
        public float eta
        {
            get { return m_fEta; }
            set { m_fEta = value; }
        }

        /// <summary>
        /// Copy the object.
        /// </summary>
        /// <param name="src">The copy is placed in this parameter.</param>
        public void Copy(NonMaximumSuppressionParameter src)
        {
            m_fNmsThreshold = src.nms_threshold;
            m_nTopK = src.top_k;
            m_fEta = src.eta;
        }

        /// <summary>
        /// Return a clone of the object.
        /// </summary>
        /// <returns>A new copy of the object is returned.</returns>
        public NonMaximumSuppressionParameter Clone()
        {
            NonMaximumSuppressionParameter p = new param.ssd.NonMaximumSuppressionParameter();
            p.Copy(this);
            return p;
        }

        /// <summary>
        /// Convert this object to a raw proto.
        /// </summary>
        /// <param name="strName">Specifies the name of the proto.</param>
        /// <returns>The new proto is returned.</returns>
        public RawProto ToProto(string strName)
        {
            RawProtoCollection rgChildren = new RawProtoCollection();

            rgChildren.Add(new RawProto("nms_threshold", nms_threshold.ToString()));
            rgChildren.Add(new RawProto("top_k", top_k.ToString()));
            rgChildren.Add(new RawProto("eta", eta.ToString()));

            return new RawProto(strName, "", rgChildren);
        }

        /// <summary>
        /// Parses the parameter from a RawProto.
        /// </summary>
        /// <param name="rp">Specifies the RawProto to parse.</param>
        /// <returns>A new instance of the parameter is returned.</returns>
        public static NonMaximumSuppressionParameter FromProto(RawProto rp)
        {
            NonMaximumSuppressionParameter p = new NonMaximumSuppressionParameter();
            string strVal;

            if ((strVal = rp.FindValue("nms_threshold")) != null)
                p.nms_threshold = float.Parse(strVal);

            if ((strVal = rp.FindValue("top_k")) != null)
                p.top_k = int.Parse(strVal);

            if ((strVal = rp.FindValue("eta")) != null)
                p.eta = float.Parse(strVal);

            return p;
        }
    }
}