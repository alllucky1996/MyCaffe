﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyCaffe.basecode;
using System.ComponentModel;

/// <summary>
/// The MyCaffe.param.beta parameters are used by the MyCaffe.layer.beta layers.
/// </summary>
/// <remarks>
/// Using parameters within the MyCaffe.layer.beta namespace are used by layers that require the MyCaffe.layers.beta.dll.
/// </remarks>
namespace MyCaffe.param.beta
{
    /// <summary>
    /// Specifies the parameters for the DecodeLayer and the AccuracyEncodingLayer.
    /// </summary>
    public class DecodeParameter : LayerParameterBase 
    {
        int m_nCentroidTheshold = 20;
        double m_dfMinAlpha = 0.0001;

        /** @copydoc LayerParameterBase */
        public DecodeParameter()
        {
        }

        /// <summary>
        /// Specifies the minimum number of items to observe per label before using the calculated cenntroid for each label (default = 20).
        /// </summary>
        [Description("Specifies the minimum number of items to observe per label before using the calculated cenntroid for each label (default = 20).")]
        public int centroid_threshold
        {
            get { return m_nCentroidTheshold; }
            set { m_nCentroidTheshold = value; }
        }

        /// <summary>
        /// Specifies the minimum alpha value used when averaging the items per label to create the centroid (default = 0.0001).  Ultimately this value dictates how many observations factor into the final centroid after a long training session.
        /// </summary>
        [Description("Specifies the minimum alpha value used when averaging the items per label to create the centroid (default = 0.0001).  Ultimately this value dictates how many observations factor into the final centroid after a long training session.")]
        public double min_alpha
        {
            get { return m_dfMinAlpha; }
            set { m_dfMinAlpha = value; }
        }

        /** @copydoc LayerParameterBase::Load */
        public override object Load(System.IO.BinaryReader br, bool bNewInstance = true)
        {
            RawProto proto = RawProto.Parse(br.ReadString());
            DecodeParameter p = FromProto(proto);

            if (!bNewInstance)
                Copy(p);

            return p;
        }

        /** @copydoc LayerParameterBase::Copy */
        public override void Copy(LayerParameterBase src)
        {
            DecodeParameter p = (DecodeParameter)src;
            m_nCentroidTheshold = p.m_nCentroidTheshold;
            m_dfMinAlpha = p.m_dfMinAlpha;
        }

        /** @copydoc LayerParameterBase::Clone */
        public override LayerParameterBase Clone()
        {
            DecodeParameter p = new DecodeParameter();
            p.Copy(this);
            return p;
        }

        /** @copydoc LayerParameterBase::ToProto */
        public override RawProto ToProto(string strName)
        {
            RawProtoCollection rgChildren = new RawProtoCollection();

            rgChildren.Add("centroid_threshold", centroid_threshold.ToString());
            rgChildren.Add("min_alpha", min_alpha.ToString());

            return new RawProto(strName, "", rgChildren);
        }

        /// <summary>
        /// Parses the parameter from a RawProto.
        /// </summary>
        /// <param name="rp">Specifies the RawProto to parse.</param>
        /// <returns>A new instance of the parameter is returned.</returns>
        public static DecodeParameter FromProto(RawProto rp)
        {
            string strVal;
            DecodeParameter p = new DecodeParameter();

            if ((strVal = rp.FindValue("centroid_threshold")) != null)
                p.centroid_threshold = int.Parse(strVal);

            if ((strVal = rp.FindValue("min_alpha")) != null)
                p.min_alpha = int.Parse(strVal);

            return p;
        }
    }
}