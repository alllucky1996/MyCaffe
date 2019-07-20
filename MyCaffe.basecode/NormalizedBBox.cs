﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCaffe.basecode
{
    /// <summary>
    /// The NormalizedBBox manages a bounding box used in SSD.
    /// </summary>
    /// <remarks>
    /// @see [SSD: Single Shot MultiBox Detector](https://arxiv.org/abs/1512.02325) by Wei Liu, Dragomir Anguelov, Dumitru Erhan, Christian Szegedy, Scott Reed, Cheng-Yang Fu, Alexander C. Berg, 2016.
    /// @see [GitHub: SSD: Single Shot MultiBox Detector](https://github.com/weiliu89/caffe/tree/ssd), by weiliu89/caffe, 2016
    /// </remarks>
    public class NormalizedBBox
    {
        float m_fxmin = 0;  // [0]
        float m_fymin = 0;  // [1]
        float m_fxmax = 0;  // [2]
        float m_fymax = 0;  // [3]
        int m_nLabel = -1;
        bool m_bDifficult = false;
        float m_fScore = 0;
        float m_fSize = 0;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="fxmin">Specifies the bounding box x minimum.</param>
        /// <param name="fymin">Specifies the bounding box y minimum.</param>
        /// <param name="fxmax">Specifies the bounding box x maximum.</param>
        /// <param name="fymax">Specifies the bounding box y maximum.</param>
        /// <param name="nLabel">Specifies the label.</param>
        /// <param name="bDifficult">Specifies the difficulty.</param>
        /// <param name="fScore">Specifies the score.</param>
        /// <param name="fSize">Specifies the size.</param>
        public NormalizedBBox(float fxmin, float fymin, float fxmax, float fymax, int nLabel = 0, bool bDifficult = false, float fScore = 0, float fSize = 0)
        {
            m_fxmin = fxmin;
            m_fxmax = fxmax;
            m_fymin = fymin;
            m_fymax = fymax;
            m_nLabel = nLabel;
            m_bDifficult = bDifficult;
            m_fScore = fScore;
            m_fSize = fSize;
        }

        /// <summary>
        /// Return a copy of the object.
        /// </summary>
        /// <returns>A new copy of the object is returned.</returns>
        public NormalizedBBox Clone()
        {
            return new NormalizedBBox(m_fxmin, m_fymin, m_fxmax, m_fymax, m_nLabel, m_bDifficult, m_fScore, m_fSize);
        }

        /// <summary>
        /// Get/set the x minimum.
        /// </summary>
        public float xmin
        {
            get { return m_fxmin; }
            set { m_fxmin = value; }
        }

        /// <summary>
        /// Get/set the x maximum.
        /// </summary>
        public float xmax
        {
            get { return m_fxmax; }
            set { m_fxmax = value; }
        }

        /// <summary>
        /// Get/set the y minimum.
        /// </summary>
        public float ymin
        {
            get { return m_fymin; }
            set { m_fymin = value; }
        }

        /// <summary>
        /// Get/set the y maximum.
        /// </summary>
        public float ymax
        {
            get { return m_fymax; }
            set { m_fymax = value; }
        }

        /// <summary>
        /// Get/set the label.
        /// </summary>
        public int label
        {
            get { return m_nLabel; }
            set { m_nLabel = value; }
        }

        /// <summary>
        /// Get/set the difficulty.
        /// </summary>
        public bool difficult
        {
            get { return m_bDifficult; }
            set { m_bDifficult = value; }
        }

        /// <summary>
        /// Get/set the score.
        /// </summary>
        public float score
        {
            get { return m_fScore; }
            set { m_fScore = value; }
        }

        /// <summary>
        /// Get/set the size.
        /// </summary>
        public float size
        {
            get { return m_fSize; }
            set { m_fSize = value; }
        }
    }
}