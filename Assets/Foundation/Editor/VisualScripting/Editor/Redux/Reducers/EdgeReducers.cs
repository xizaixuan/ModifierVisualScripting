﻿using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEditor.Modifier.VisualScripting.Model.VSPreferences;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    static class EdgeReducers
    {
        const int k_NodeOffset = 60;
        const int k_StackOffset = 120;

        public static void Register(Store store)
        {
            store.Register<CreateNodeFromExecutionPortAction>(CreateNodeFromExecutionPort);
            store.Register<CreateNodeFromInputPortAction>(CreateGraphNodeFromInputPort);
            store.Register<CreateStackedNodeFromOutputPortAction>(CreateStackedNodeFromOutputPort);
            store.Register<CreateNodeFromOutputPortAction>(CreateNodeFromOutputPort);
            store.Register<CreateEdgeAction>(CreateEdge);
            store.Register<SplitEdgeAndInsertNodeAction>(SplitEdgeAndInsertNode);
            store.Register<CreateNodeOnEdgeAction>(CreateNodeOnEdge);
            store.Register<AddControlPointOnEdgeAction>(AddControlPointOnEdgeAction.DefaultReducer);
            store.Register<MoveEdgeControlPointAction>(MoveEdgeControlPointAction.DefaultReducer);
            store.Register<RemoveEdgeControlPointAction>(RemoveEdgeControlPointAction.DefaultReducer);
            store.Register<SetEdgeEditModeAction>(SetEdgeEditModeAction.DefaultReducer);
            store.Register<ConvertEdgesToPortalsAction>(ConvertEdgesToPortals);
        }

        static State CreateNodeFromExecutionPort(State previousState, CreateNodeFromExecutionPortAction action)
        {
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;
            graphModel.DeleteEdges(action.EdgesToDelete.OfType<IEdgeModel>());

            var stackPosition = action.Position - Vector2.right * k_StackOffset;
            var stack = graphModel.CreateStack(string.Empty, stackPosition);

            if (action.PortModel.Direction == Direction.Output)
                graphModel.CreateEdge(stack.InputPorts[0], action.PortModel);
            else
                graphModel.CreateEdge(action.PortModel, stack.OutputPorts[0]);

            graphModel.LastChanges?.ChangedElements.Add(action.PortModel.NodeModel);

            return previousState;
        }

        static State CreateGraphNodeFromInputPort(State previousState, CreateNodeFromInputPortAction action)
        {
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;
            graphModel.DeleteEdges(action.EdgesToDelete.OfType<IEdgeModel>());

            var position = action.Position - Vector2.up * k_NodeOffset;
            var elementModels = action.SelectedItem.CreateElements.Invoke(
                new GraphNodeCreationData(graphModel, position));

            if (elementModels.Length == 0 || !(elementModels[0] is INodeModel selectedNodeModel))
                return previousState;

            var outputPortModel = action.PortModel.DataTypeHandle == TypeHandle.Unknown
                ? selectedNodeModel.OutputsByDisplayOrder.FirstOrDefault()
                : GetFirstPortModelOfType(action.PortModel.DataTypeHandle, selectedNodeModel.OutputsByDisplayOrder, true);

            if (outputPortModel != null)
            {
                var newEdge = graphModel.CreateEdge(action.PortModel, outputPortModel);
                if (newEdge != null && previousState.Preferences.GetBool(BoolPref.AutoAlignDraggedEdges))
                    graphModel.LastChanges?.ModelsToAutoAlign.Add(newEdge);
            }

            graphModel.LastChanges?.ChangedElements.Add(action.PortModel.NodeModel);

            return previousState;
        }

        static State CreateStackedNodeFromOutputPort(State previousState, CreateStackedNodeFromOutputPortAction action)
        {
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;
            Undo.RegisterCompleteObjectUndo((Object)graphModel.AssetModel, "Create Node From Output Port");
            graphModel.DeleteEdges(action.EdgesToDelete.OfType<IEdgeModel>());

            var elementModels = action.SelectedItem.CreateElements.Invoke(
                new StackNodeCreationData(action.StackModel, action.Index));

            if (elementModels.Length == 0 || !(elementModels[0] is INodeModel selectedNodeModel))
                return previousState;

            var outputPortModel = action.PortModel;
            var newInput = selectedNodeModel.InputsByDisplayOrder.FirstOrDefault();
            if (newInput != null)
            {
                CreateItemizedNode(previousState, graphModel, ref outputPortModel);
                var newEdge = graphModel.CreateEdge(newInput, outputPortModel);
                if (newEdge != null && previousState.Preferences.GetBool(BoolPref.AutoAlignDraggedEdges))
                    graphModel.LastChanges?.ModelsToAutoAlign.Add(newEdge);
            }

            graphModel.LastChanges?.ChangedElements.Add(action.PortModel.NodeModel);

            return previousState;
        }

        static State CreateNodeFromOutputPort(State previousState, CreateNodeFromOutputPortAction action)
        {
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;
            graphModel.DeleteEdges(action.EdgesToDelete.OfType<IEdgeModel>());

            var position = action.Position - Vector2.up * k_NodeOffset;
            var elementModels = action.SelectedItem.CreateElements.Invoke(
                new GraphNodeCreationData(graphModel, position));

            if (!elementModels.Any() || !(elementModels[0] is INodeModel selectedNodeModel))
                return previousState;

            var inputPortModel = action.PortModel.DataTypeHandle == TypeHandle.Unknown
                ? selectedNodeModel.InputsByDisplayOrder.FirstOrDefault()
                : GetFirstPortModelOfType(action.PortModel.DataTypeHandle, selectedNodeModel.InputsByDisplayOrder, true);

            if (inputPortModel == null)
                return previousState;

            var outputPortModel = action.PortModel;

            CreateItemizedNode(previousState, graphModel, ref outputPortModel);
            var newEdge = graphModel.CreateEdge(inputPortModel, outputPortModel);

            if (newEdge != null && previousState.Preferences.GetBool(BoolPref.AutoAlignDraggedEdges))
                graphModel.LastChanges?.ModelsToAutoAlign.Add(newEdge);

            graphModel.LastChanges?.ChangedElements.Add(action.PortModel.NodeModel);

            return previousState;
        }

        static State CreateNodeOnEdge(State previousState, CreateNodeOnEdgeAction action)
        {
            var edgeInput = action.EdgeModel.InputPortModel;
            var edgeOutput = action.EdgeModel.OutputPortModel;

            // Instantiate node
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;

            var position = action.Position - Vector2.up * k_NodeOffset;

            List<GUID> guids = action.Guid.HasValue ? new List<GUID> { action.Guid.Value } : null;

            var elementModels = action.SelectedItem.CreateElements.Invoke(
                new GraphNodeCreationData(graphModel, position, guids: guids));

            if (elementModels.Length == 0 || !(elementModels[0] is INodeModel selectedNodeModel))
                return previousState;

            // Delete old edge
            graphModel.DeleteEdge(action.EdgeModel);

            // Connect input port
            var inputPortModel = selectedNodeModel.InputsByDisplayOrder.FirstOrDefault();

            if (inputPortModel != null)
                graphModel.CreateEdge(inputPortModel, edgeOutput);

            // Find first matching output type and connect it
            var outputPortModel = GetFirstPortModelOfType(edgeInput.DataTypeHandle,
                selectedNodeModel.OutputsByDisplayOrder, true);

            if (outputPortModel != null)
                graphModel.CreateEdge(edgeInput, outputPortModel);

            return previousState;
        }

        static State CreateEdge(State previousState, CreateEdgeAction action)
        {
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;

            if (action.EdgeModelsToDelete != null)
                graphModel.DeleteEdges(action.EdgeModelsToDelete.OfType<IEdgeModel>());

            // PF remove cast
            IPortModel outputPortModel = action.OutputPortModel as IPortModel;
            IPortModel inputPortModel = action.InputPortModel as IPortModel;

            CreateItemizedNode(previousState, graphModel, ref outputPortModel);
            graphModel.CreateEdge(inputPortModel, outputPortModel);

            if (action.PortAlignment.HasFlag(CreateEdgeAction.PortAlignmentType.Input))
                graphModel.LastChanges.ModelsToAutoAlign.Add(inputPortModel.NodeModel);
            if (action.PortAlignment.HasFlag(CreateEdgeAction.PortAlignmentType.Output))
                graphModel.LastChanges.ModelsToAutoAlign.Add(outputPortModel.NodeModel);

            return previousState;
        }

        static State SplitEdgeAndInsertNode(State previousState, SplitEdgeAndInsertNodeAction action)
        {
            Assert.IsTrue(action.NodeModel.InputsById.Count > 0);
            Assert.IsTrue(action.NodeModel.OutputsById.Count > 0);

            var graphModel = ((VSGraphModel)previousState.CurrentGraphModel);
            var edgeInput = action.EdgeModel.InputPortModel;
            var edgeOutput = action.EdgeModel.OutputPortModel;
            graphModel.DeleteEdge(action.EdgeModel);
            graphModel.CreateEdge(edgeInput, action.NodeModel.OutputsByDisplayOrder.First());
            graphModel.CreateEdge(action.NodeModel.InputsByDisplayOrder.First(), edgeOutput);

            return previousState;
        }

        [CanBeNull]
        static IPortModel GetFirstPortModelOfType(TypeHandle typeHandle, IEnumerable<IPortModel> portModels, bool fallbackToFirstPort)
        {
            Stencil stencil = portModels.First().VSGraphModel.Stencil;
            IPortModel unknownPortModel = null;

            // Return the first matching Input portModel
            // If no match was found, return the first Unknown typed portModel
            // Else return null.
            foreach (IPortModel portModel in portModels)
            {
                if (portModel.DataTypeHandle == TypeHandle.Unknown && unknownPortModel == null)
                {
                    unknownPortModel = portModel;
                }

                if (typeHandle.IsAssignableFrom(portModel.DataTypeHandle, stencil))
                {
                    return portModel;
                }
            }

            if (unknownPortModel != null)
                return unknownPortModel;
            return fallbackToFirstPort ? portModels.FirstOrDefault() : null;
        }

        static void CreateItemizedNode(State state, VSGraphModel graphModel, ref IPortModel outputPortModel)
        {
            ItemizeOptions currentItemizeOptions = state.Preferences.CurrentItemizeOptions;

            // automatically itemize, i.e. duplicate variables as they get connected
            if (!outputPortModel.IsConnected || currentItemizeOptions == ItemizeOptions.Nothing)
                return;

            INodeModel nodeToConnect = outputPortModel.NodeModel;

            bool itemizeContant = currentItemizeOptions.HasFlag(ItemizeOptions.Constants)
                && nodeToConnect is ConstantNodeModel;
            bool itemizeVariable = currentItemizeOptions.HasFlag(ItemizeOptions.Variables)
                && (nodeToConnect is VariableNodeModel || nodeToConnect is ThisNodeModel);
            bool itemizeSystemConstant = currentItemizeOptions.HasFlag(ItemizeOptions.SystemConstants) &&
                nodeToConnect is SystemConstantNodeModel;
            if (itemizeContant || itemizeVariable || itemizeSystemConstant)
            {
                Vector2 offset = Vector2.up * k_NodeOffset;
                nodeToConnect = graphModel.DuplicateUnstackedNode(outputPortModel.NodeModel, new Dictionary<INodeModel, NodeModel>(), offset);
                outputPortModel = nodeToConnect.OutputsById[outputPortModel.UniqueId];
            }
        }

        static readonly Vector2 k_EntryPortalBaseOffset = Vector2.right * 75;
        static readonly Vector2 k_ExitPortalBaseOffset = Vector2.left * 250;
        const int k_PortalHeight = 24;

        static State ConvertEdgesToPortals(State previousState, ConvertEdgesToPortalsAction action)
        {
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;

            if (action.EdgeData == null)
                return previousState;

            var edgeData = action.EdgeData.ToList();
            if (!edgeData.Any())
                return previousState;

            Undo.RegisterCompleteObjectUndo((Object)previousState.AssetModel, "Convert edges to portals");
            EditorUtility.SetDirty((Object)previousState.AssetModel);

            var existingPortalEntries = new Dictionary<IPortModel, IEdgePortalEntryModel>();
            var existingPortalExits = new Dictionary<IPortModel, List<IEdgePortalExitModel>>();

            foreach (var edgeModel in edgeData)
                ConvertEdgeToPortals(edgeModel);

            // Adjust placement in case of multiple incoming exit portals so they don't overlap
            foreach (var portalList in existingPortalExits.Values.Where(l => l.Count > 1))
            {
                var cnt = portalList.Count;
                bool isEven = cnt % 2 == 0;
                int offset = isEven ? k_PortalHeight / 2 : 0;
                for (int i = (cnt - 1) / 2; i >= 0; i--)
                {
                    portalList[i].Position = new Vector2(portalList[i].Position.x, portalList[i].Position.y - offset);
                    portalList[cnt - 1 - i].Position = new Vector2(portalList[cnt - 1 - i].Position.x, portalList[cnt - 1 - i].Position.y + offset);
                    offset += k_PortalHeight;
                }
            }

            graphModel.DeleteEdges(edgeData.Select(d => d.edge));
            previousState.MarkForUpdate(UpdateFlags.GraphTopology);
            return previousState;

            void ConvertEdgeToPortals((IEdgeModel edgeModel, Vector2 startPos, Vector2 endPos) data)
            {
                // Only a single portal per output port. Don't recreate if we already created one.
                var outputPortModel = data.edgeModel.OutputPortModel;
                if (!existingPortalEntries.TryGetValue(outputPortModel, out var portalEntry))
                {
                    if (outputPortModel.PortType == PortType.Execution)
                        portalEntry = graphModel.CreateNode<ExecutionEdgePortalEntryModel>();
                    else
                        portalEntry = graphModel.CreateNode<DataEdgePortalEntryModel>();
                    existingPortalEntries[outputPortModel] = portalEntry;

                    var nodeModel = outputPortModel.NodeModel;
                    portalEntry.Position = data.startPos + k_EntryPortalBaseOffset;

                    // y offset based on port order. hurgh.
                    var idx = nodeModel.OutputsByDisplayOrder.IndexOf(outputPortModel);
                    portalEntry.Position += Vector2.down * (k_PortalHeight * idx + 16); // Fudgy.

                    string portalName;
                    if (nodeModel is IConstantNodeModel constantNodeModel)
                        portalName = constantNodeModel.Type.FriendlyName();
                    else
                    {
                        portalName = nodeModel.Title;
                        if (!string.IsNullOrEmpty(outputPortModel.Name))
                            portalName += " - " + outputPortModel.Name;
                    }

                    ((EdgePortalModel)portalEntry).DeclarationModel = graphModel.CreateGraphPortalDeclaration(portalName);

                    graphModel.CreateEdge(portalEntry.InputPort, outputPortModel);
                }

                // We can have multiple portals on input ports however
                var inputPortModel = data.edgeModel.InputPortModel;
                if (!existingPortalExits.TryGetValue(inputPortModel, out var portalExits))
                {
                    portalExits = new List<IEdgePortalExitModel>();
                    existingPortalExits[inputPortModel] = portalExits;
                }

                IEdgePortalExitModel portalExit;
                if (inputPortModel.PortType == PortType.Execution)
                    portalExit = graphModel.CreateNode<ExecutionEdgePortalExitModel>();
                else
                    portalExit = graphModel.CreateNode<DataEdgePortalExitModel>();

                portalExits.Add(portalExit);

                portalExit.Position = data.endPos + k_ExitPortalBaseOffset;
                {
                    var nodeModel = inputPortModel.NodeModel;
                    // y offset based on port order. hurgh.
                    var idx = nodeModel.InputsByDisplayOrder.IndexOf(inputPortModel);
                    portalExit.Position += Vector2.down * (k_PortalHeight * idx + 16); // Fudgy.
                }

                ((EdgePortalModel)portalExit).DeclarationModel = portalEntry.DeclarationModel;

                graphModel.CreateEdge(inputPortModel, portalExit.OutputPort);
            }
        }
    }
}