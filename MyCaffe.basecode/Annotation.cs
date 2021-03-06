﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCaffe.basecode
{
    /// <summary>
    /// The Annotation class is used by annotations attached to SimpleDatum's and used in SSD.
    /// </summary>
    /// <remarks>
    /// @see [SSD: Single Shot MultiBox Detector](https://arxiv.org/abs/1512.02325) by Wei Liu, Dragomir Anguelov, Dumitru Erhan, Christian Szegedy, Scott Reed, Cheng-Yang Fu, Alexander C. Berg, 2016.
    /// @see [GitHub: SSD: Single Shot MultiBox Detector](https://github.com/weiliu89/caffe/tree/ssd), by weiliu89/caffe, 2016
    /// </remarks>
    public class Annotation
    {
        int m_nInstanceId = 0;
        NormalizedBBox m_bbox;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="bbox">Specifies the bounding box.</param>
        /// <param name="nInstanceId">Specifies the instance ID.</param>
        public Annotation(NormalizedBBox bbox, int nInstanceId = 0)
        {
            m_bbox = bbox;
            m_nInstanceId = nInstanceId;
        }

        /// <summary>
        /// Returns a copy of the Annotation.
        /// </summary>
        /// <returns>A new copy of the annotation is returned.</returns>
        public Annotation Clone()
        {
            NormalizedBBox bbox = null;

            if (m_bbox != null)
                bbox = m_bbox.Clone();

            return new Annotation(bbox, m_nInstanceId);
        }

        /// <summary>
        /// Get/set the instance ID.
        /// </summary>
        public int instance_id
        {
            get { return m_nInstanceId; }
            set { m_nInstanceId = value; }
        }

        /// <summary>
        /// Get/set the bounding box.
        /// </summary>
        public NormalizedBBox bbox
        {
            get { return m_bbox; }
            set { m_bbox = value; }
        }

        /// <summary>
        /// Save the annotation data using the binary writer.
        /// </summary>
        /// <param name="bw">Specifies the binary writer used to save the data.</param>
        public void Save(BinaryWriter bw)
        {
            bw.Write(m_nInstanceId);
            m_bbox.Save(bw);
        }

        /// <summary>
        /// Load the annotation using a binary reader.
        /// </summary>
        /// <param name="br">Specifies the binary reader used to load the data.</param>
        /// <returns>The newly loaded annoation is returned.</returns>
        public static Annotation Load(BinaryReader br)
        {
            int nInstanceId = br.ReadInt32();
            NormalizedBBox bbox = NormalizedBBox.Load(br);

            return new Annotation(bbox, nInstanceId);
        }
    }

    /// <summary>
    /// The AnnoationGroup class manages a group of annotations.
    /// </summary>
    public class AnnotationGroup
    {
        int m_nGroupLabel = 0;
        List<Annotation> m_rgAnnotations = new List<Annotation>();

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="rgAnnotations">Optionally, specifies the list of group annotations.</param>
        /// <param name="nGroupLabel">Specifies the group label.</param>
        public AnnotationGroup(List<Annotation> rgAnnotations = null, int nGroupLabel = 0)
        {
            if (rgAnnotations != null && rgAnnotations.Count > 0)
                m_rgAnnotations.AddRange(rgAnnotations);

            m_nGroupLabel = nGroupLabel;
        }

        /// <summary>
        /// Create a copy of the annotation group.
        /// </summary>
        /// <returns>A copy of the annotation group is returned.</returns>
        public AnnotationGroup Clone()
        {
            List<Annotation> rg = null;

            if (m_rgAnnotations != null)
            {
                rg = new List<Annotation>();

                foreach (Annotation a in m_rgAnnotations)
                {
                    rg.Add(a.Clone());
                }
            }

            return new AnnotationGroup(rg, m_nGroupLabel);
        }

        /// <summary>
        /// Get/set the group annoations.
        /// </summary>
        public List<Annotation> annotations
        {
            get { return m_rgAnnotations; }
            set { m_rgAnnotations = value; }
        }

        /// <summary>
        /// Get/set the group label.
        /// </summary>
        public int group_label
        {
            get { return m_nGroupLabel; }
            set { m_nGroupLabel = value; }
        }

        /// <summary>
        /// Save the annotation group to the binary writer.
        /// </summary>
        /// <param name="bw">Specifies the binary writer used to write the data.</param>
        public void Save(BinaryWriter bw)
        {
            bw.Write(m_nGroupLabel);
            bw.Write(m_rgAnnotations.Count);

            for (int i = 0; i < m_rgAnnotations.Count; i++)
            {
                m_rgAnnotations[i].Save(bw);
            }
        }

        /// <summary>
        /// Load an annotation group using the binary reader.
        /// </summary>
        /// <param name="br">Specifies the binary reader used to load the data.</param>
        /// <returns>The new AnnotationGroup loaded is returned.</returns>
        public static AnnotationGroup Load(BinaryReader br)
        {
            int nGroupLabel = br.ReadInt32();
            int nCount = br.ReadInt32();
            List<Annotation> rgAnnotations = new List<Annotation>();

            for (int i = 0; i < nCount; i++)
            {
                rgAnnotations.Add(Annotation.Load(br));
            }

            return new AnnotationGroup(rgAnnotations, nGroupLabel);
        }
    }
}
