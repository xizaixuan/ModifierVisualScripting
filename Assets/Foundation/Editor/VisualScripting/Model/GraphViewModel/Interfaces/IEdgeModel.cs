using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    [Serializable]
    public struct ControlPoint
    {
        [SerializeField]
        public Vector2 Position;
        [SerializeField]
        public float Tightness;
    }

    public interface IEdgeModel : IGraphElementModel
    {
        string OutputId { get; }
        string InputId { get; }
        GUID InputNodeGuid { get; }
        GUID OutputNodeGuid { get; }
        IPortModel InputPortModel { get; }
        IPortModel OutputPortModel { get; }
        string EdgeLabel { get; }
        ReadOnlyCollection<ControlPoint> EdgeControlPoints { get; }
        void InsertEdgeControlPoint(int atIndex, Vector2 point, float tightness);
        void ModifyEdgeControlPoint(int index, Vector2 point, float tightness);
        void RemoveEdgeControlPoint(int index);
        bool EditMode { get; set; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class IEdgeModelExtensions
    {
        public static IEnumerable<IPortModel> GetPortModels(this IEdgeModel edge)
        {
            yield return edge.InputPortModel;
            yield return edge.OutputPortModel;
        }

        public static bool IsValid(this IEdgeModel edge)
        {
            return edge.InputPortModel != null && edge.OutputPortModel != null;
        }
    }
}