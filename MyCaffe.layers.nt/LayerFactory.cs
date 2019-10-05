﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCaffe.basecode;
using MyCaffe.common;
using MyCaffe.db.image;
using MyCaffe.param;

/// <summary>
/// The MyCaffe.layers.nt namespace contains all Neural Transfer related layers.
/// </summary>
/// <remarks>
/// @see [A Neural Algorithm of Artistic Style](https://arxiv.org/abs/1508.06576) by Leon A. Gatys, Alexander S. Ecker, and Matthias Bethge, 2015 
/// @see [ftokarev/caffe-neural-style Github](https://github.com/ftokarev/caffe-neural-style) by ftokarev, 2017. 
/// </remarks>
namespace MyCaffe.layers.nt
{
    /// <summary>
    /// The LayerFactor is responsible for creating all layers implemented in the MyCaffe.layers.ssd namespace.
    /// </summary>
    public class LayerFactory : ILayerCreator
    {
        /// <summary>
        /// Create the layers when using the <i>double</i> base type.
        /// </summary>
        /// <param name="cuda">Specifies the connection to the low-level CUDA interfaces.</param>
        /// <param name="log">Specifies the output log.</param>
        /// <param name="p">Specifies the layer parameter.</param>
        /// <param name="evtCancel">Specifies the cancellation event.</param>
        /// <param name="imgDb">Specifies an interface to the image database, who's use is optional.</param>
        /// <returns>If supported, the layer is returned, otherwise <i>null</i> is returned.</returns>
        public Layer<double> CreateDouble(CudaDnn<double> cuda, Log log, LayerParameter p, CancelEvent evtCancel, IXImageDatabase imgDb)
        {
            switch (p.type)
            {
                case LayerParameter.LayerType.EVENT:
                    return new EventLayer<double>(cuda, log, p);

                case LayerParameter.LayerType.GRAM:
                    return new GramLayer<double>(cuda, log, p);

                case LayerParameter.LayerType.ONEHOT:
                    return new OneHotLayer<double>(cuda, log, p);

                case LayerParameter.LayerType.SCALAR:
                    return new ScalarLayer<double>(cuda, log, p);

                case LayerParameter.LayerType.TV_LOSS:
                    return new TVLossLayer<double>(cuda, log, p);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Create the layers when using the <i>float</i> base type.
        /// </summary>
        /// <param name="cuda">Specifies the connection to the low-level CUDA interfaces.</param>
        /// <param name="log">Specifies the output log.</param>
        /// <param name="p">Specifies the layer parameter.</param>
        /// <param name="evtCancel">Specifies the cancellation event.</param>
        /// <param name="imgDb">Specifies an interface to the image database, who's use is optional.</param>
        /// <returns>If supported, the layer is returned, otherwise <i>null</i> is returned.</returns>
        public Layer<float> CreateSingle(CudaDnn<float> cuda, Log log, LayerParameter p, CancelEvent evtCancel, IXImageDatabase imgDb)
        {
            switch (p.type)
            {
                case LayerParameter.LayerType.EVENT:
                    return new EventLayer<float>(cuda, log, p);

                case LayerParameter.LayerType.GRAM:
                    return new GramLayer<float>(cuda, log, p);

                case LayerParameter.LayerType.ONEHOT:
                    return new OneHotLayer<float>(cuda, log, p);

                case LayerParameter.LayerType.SCALAR:
                    return new ScalarLayer<float>(cuda, log, p);

                case LayerParameter.LayerType.TV_LOSS:
                    return new TVLossLayer<float>(cuda, log, p);

                default:
                    return null;
            }
        }
    }
}
