using System;
using System.Collections.ObjectModel;
using Unity.Modifier.GraphElements;
using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEngine;

namespace Unity.Modifier.GraphToolsFoundation.Model
{
    [Serializable]
    public class EdgeControlPointModel
    {
        [SerializeField]
        public Vector2 m_Position;
        [SerializeField]
        public float m_Tightness;
        public Vector2 Position
        {
            get => m_Position;
            set => m_Position = value;
        }

        public float Tightness
        {
            get => m_Tightness;
            set => m_Tightness = value;
        }
    }

    public interface IGTFEdgeModel : IGTFGraphElementModel, ISelectable, IDeletable, IPositioned, ICopiable
    {
        IGTFPortModel FromPort { get; }
        IGTFPortModel ToPort { get; }
        ReadOnlyCollection<EdgeControlPointModel> EdgeControlPoints { get; }
        void InsertEdgeControlPoint(int atIndex, Vector2 point, float tightness);
        void ModifyEdgeControlPoint(int index, Vector2 point, float tightness);
        void RemoveEdgeControlPoint(int index);
        bool EditMode { get; set; }
    }
}
