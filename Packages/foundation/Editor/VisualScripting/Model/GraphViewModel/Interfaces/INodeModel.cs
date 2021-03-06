﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEngine;
using Port = Unity.Modifier.GraphElements.Port;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public enum LoopConnectionType
    {
        None,
        Stack,
        LoopStack
    }

    public interface IPropertyVisitorNodeTarget
    {
        object Target { get; set; }
        bool IsExcluded(object value);
    }

    public interface INodeModel : IGraphElementModelWithGuid, IUndoRedoAware
    {
        Vector2 Position { get; set; }
        ModelState State { get; }
        IStackModel ParentStackModel { get; }
        string Title { get; }
        GUID Guid { get; }
        IReadOnlyDictionary<string, IPortModel> InputsById { get; }
        IReadOnlyDictionary<string, IPortModel> OutputsById { get; }
        IReadOnlyList<IPortModel> InputsByDisplayOrder { get; }
        IReadOnlyList<IPortModel> OutputsByDisplayOrder { get; }
        bool IsStacked { get; }
        bool IsCondition { get; }
        bool IsInsertLoop { get; }
        LoopConnectionType LoopConnectionType { get; }
        bool IsBranchType { get; }
        Color Color { get; set; }
        bool HasUserColor { get; set; }
        bool Destroyed { get; }
        string ToolTip { get; }
        bool HasProgress { get; }

        void OnConnection(IPortModel selfConnectedPortModel, IPortModel otherConnectedPortModel);
        void OnDisconnection(IPortModel selfConnectedPortModel, IPortModel otherConnectedPortModel);

        void PostGraphLoad();

        PortCapacity GetPortCapacity(PortModel portModel);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class INodeModelExtensions
    {
        public static IEnumerable<IPortModel> GetPortModels(this INodeModel node)
        {
            return node.InputsByDisplayOrder.Concat(node.OutputsByDisplayOrder);
        }

        public static IEnumerable<IEdgeModel> GetConnectedEdges(this INodeModel nodeModel)
        {
            var graphModel = nodeModel.VSGraphModel;
            return nodeModel.GetPortModels().SelectMany(p => graphModel.GetEdgesConnections(p));
        }

        public static IEnumerable<INodeModel> GetConnectedNodes(this INodeModel nodeModel)
        {
            foreach (IPortModel portModel in nodeModel.GetPortModels())
            {
                foreach (IPortModel connectionPortModel in portModel.ConnectionPortModels)
                {
                    yield return connectionPortModel.NodeModel;
                }
            }
        }
    }
}