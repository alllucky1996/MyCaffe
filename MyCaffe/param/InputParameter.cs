﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCaffe.basecode;

namespace MyCaffe.param
{
    /// <summary>
    /// Specifies the parameters for the InputLayer.
    /// </summary>
    /// <remarks>
    /// This layer produces N >= 1 top blob(s) to be assigned manually.
    /// </remarks>
    public class InputParameter : LayerParameterBase
    {
        List<BlobShape> m_rgShape = new List<BlobShape>();
        bool m_bUseHalfSize = false;

        /** @copydoc LayerParameterBase */
        public InputParameter()
        {
        }

        /// <summary>
        /// When true and using the CUDNN engine, half sizes are used on the weights (FP16), otherwise when using the CAFFE engine
        /// this setting is ignored.
        /// </summary>
        [Description("When true and using the CUDNN engine, half sizes (FP16) are used on the weights, otherwise when using the CAFFE engine, this setting is ignored.")]
        public bool cudnn_use_halfsize
        {
            get { return m_bUseHalfSize; }
            set { m_bUseHalfSize = value; }
        }

        /// <summary>
        /// Define N shapes to set a shape for each top.
        /// Define 1 shape to set the same shape for every top.
        /// Define no shape to defer to reshaping manually.
        /// </summary>
        [Description("Define N shapes to set a shape for each top; Define 1 shape to set the same shape for every top; Define no shapes to defer to manual reshaping.")]
        public List<BlobShape> shape
        {
            get { return m_rgShape; }
            set { m_rgShape = value; }
        }

        /** @copydoc LayerParameterBase::Load */
        public override object Load(BinaryReader br, bool bNewInstance = true)
        {
            RawProto proto = RawProto.Parse(br.ReadString());
            InputParameter p = FromProto(proto);

            if (!bNewInstance)
                Copy(p);

            return p;
        }

        /** @copydoc LayerParameterBase::Copy */
        public override void Copy(LayerParameterBase src)
        {
            InputParameter p = (InputParameter)src;

            m_rgShape = new List<param.BlobShape>();

            foreach (BlobShape b in p.shape)
            {
                m_rgShape.Add(b.Clone());
            }

            m_bUseHalfSize = p.cudnn_use_halfsize;
        }

        /** @copydoc LayerParameterBase::Clone */
        public override LayerParameterBase Clone()
        {
            InputParameter p = new param.InputParameter();
            p.Copy(this);
            return p;
        }

        /** @copydoc LayerParameterBase::ToProto */
        public override RawProto ToProto(string strName)
        {
            RawProtoCollection rgChildren = new RawProtoCollection();

            foreach (BlobShape b in m_rgShape)
            {
                rgChildren.Add(b.ToProto("shape"));
            }

            if (cudnn_use_halfsize)
                rgChildren.Add("cudnn_use_halfsize", cudnn_use_halfsize.ToString());

            return new RawProto(strName, "", rgChildren);
        }

        /// <summary>
        /// Parses the parameter from a RawProto.
        /// </summary>
        /// <param name="rp">Specifies the RawProto to parse.</param>
        /// <returns>A new instance of the parameter is returned.</returns>
        public static InputParameter FromProto(RawProto rp)
        {
            InputParameter p = new InputParameter();
            string strVal;

            RawProtoCollection col = rp.FindChildren("shape");
            foreach (RawProto rp1 in col)
            {
                BlobShape b = BlobShape.FromProto(rp1);
                p.m_rgShape.Add(b);
            }

            if ((strVal = rp.FindValue("cudnn_use_halfsize")) != null)
                p.cudnn_use_halfsize = bool.Parse(strVal);

            return p;
        }
    }
}
