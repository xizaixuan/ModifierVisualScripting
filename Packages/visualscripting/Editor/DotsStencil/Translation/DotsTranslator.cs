using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Modifier.DotsStencil.Expression;
using Modifier.Runtime;
using Unity.Entities;
using Unity.Modifier.GraphElements;
using Unity.Mathematics;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEditor.Modifier.VisualScripting.Model.Translators;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Modifier.VisualScripting;
using INode = Modifier.Runtime.INode;
using Object = UnityEngine.Object;
using Port = Modifier.Runtime.Port;
using PortType = UnityEditor.Modifier.VisualScripting.Model.PortType;
using ValueType = Modifier.Runtime.ValueType;

namespace Modifier.DotsStencil
{
    public class DotsTranslator : ITranslator
    {
        public class DotsCompilationResult : CompilationResult
        {
            public GraphDefinition GraphDefinition = new GraphDefinition();
        }

        public bool SupportsCompilation() => true;

        static bool TranslateNode(GraphBuilder builder, INodeModel nodeModel, out INode node,
            out PortMapper portToOffsetMapping, out uint? preAllocatedDataIndex)
        {
            Assert.IsNotNull(nodeModel);
            preAllocatedDataIndex = null;

            switch (nodeModel)
            {
                case SetVariableNodeModel setVariableNodeModel:
                    {
                        node = setVariableNodeModel.Node;
                        portToOffsetMapping = setVariableNodeModel.PortToOffsetMapping;
                        if (setVariableNodeModel.DeclarationModel == null)
                        {
                            return false;
                        }
                        preAllocatedDataIndex = builder.GetVariableDataIndex(setVariableNodeModel.DeclarationModel).DataIndex;
                        return true;
                    }
                case IEventNodeModel eventNodeModel:
                    node = eventNodeModel.Node;
                    var type = eventNodeModel.TypeHandle.Resolve(nodeModel.VSGraphModel.Stencil);
                    ((IEventNode)node).EventId = TypeHash.CalculateStableTypeHash(type);

                    if (node is IEventDispatcherNode eventDispatcherNode)
                        eventDispatcherNode.EventTypeSize = Marshal.SizeOf(type);

                    portToOffsetMapping = eventNodeModel.PortToOffsetMapping;
                    return true;

                case SubgraphReferenceNodeModel subgraphReferenceNodeModel:
                    node = subgraphReferenceNodeModel.Node;
                    portToOffsetMapping = subgraphReferenceNodeModel.PortToOffsetMapping;
                    return true;

                case IDotsNodeModel dotsNodeModel:
                    node = dotsNodeModel.Node;
                    portToOffsetMapping = dotsNodeModel.PortToOffsetMapping;
                    if (nodeModel is IReferenceComponentTypes referenceComponentTypes)
                    {
                        foreach (var typeReference in referenceComponentTypes.ReferencedTypes)
                        {
                            if (typeReference.TypeIndex != -1)
                            {
                                builder.AddReferencedComponent(typeReference);
                            }
                        }
                    }
                    return true;

                case IConstantNodeModel constantNodeModel:
                    HandleConstants(builder, out node, out portToOffsetMapping, constantNodeModel);
                    return true;

                case IVariableModel variableModel:
                    return HandleVariable(builder, out node, out portToOffsetMapping,
                        out preAllocatedDataIndex, variableModel);
                case ExpressionNodeModel exp:
                    return exp.Translate(builder, out node, out portToOffsetMapping, out preAllocatedDataIndex);

                default:
                    throw new NotImplementedException(
                        $"Don't know how to translate a node of type {nodeModel.GetType()}: {nodeModel}");
            }
        }

        static bool HandleVariable(GraphBuilder builder, out INode node,
            out PortMapper portToOffsetMapping, out uint? preAllocatedDataIndex, IVariableModel variableModel)
        {
            if (variableModel.DeclarationModel.IsInputOrOutputTrigger())
            {
                preAllocatedDataIndex = null;
                portToOffsetMapping = new PortMapper();
                if (variableModel.DeclarationModel.Modifiers == ModifierFlags.ReadOnly) // Input
                {
                    var trigger = builder.DeclareInputTrigger(variableModel.DeclarationModel.VariableName);
                    node = MapPort(portToOffsetMapping, variableModel.OutputPort, ref trigger.Output.Port, trigger);
                }
                else
                {
                    var trigger = builder.DeclareOutputTrigger(variableModel.DeclarationModel.VariableName);
                    node = MapPort(portToOffsetMapping, variableModel.OutputPort, ref trigger.Input.Port, trigger);
                }
                return true;
            }

            var valueType = variableModel.DeclarationModel.DataType.ToValueType();
            var type = GraphBuilder.GetVariableType(variableModel.DeclarationModel);
            switch (type)
            {
                case GraphBuilder.VariableType.ObjectReference:
                    switch (valueType)
                    {
                        case ValueType.Entity:
                            preAllocatedDataIndex = builder.GetVariableDataIndex(variableModel.DeclarationModel).DataIndex;
                            portToOffsetMapping = new PortMapper();
                            var cf = new ConstantEntity();
                            node = MapPort(portToOffsetMapping, variableModel.OutputPort, ref cf.ValuePort.Port, cf);
                            return true;
                    }
                    break;
                case GraphBuilder.VariableType.Variable: // Data
                    throw new NotImplementedException();
                case GraphBuilder.VariableType.InputOutput:
                    // Just create an edge later
                    node = default;
                    portToOffsetMapping = null;
                    preAllocatedDataIndex = null;
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("Variable type not supported: " + type);
            }

            throw new ArgumentOutOfRangeException(valueType.ToString());
        }

        static void HandleConstants(GraphBuilder builder, out INode node, out PortMapper portToOffsetMapping,
            IConstantNodeModel constantNodeModel)
        {
            portToOffsetMapping = new PortMapper();
            var outputPortId = constantNodeModel.OutputPort?.UniqueId ?? "";
            switch (constantNodeModel)
            {
                case StringConstantModel stringConstantModel:
                    {
                        var index = builder.StoreStringConstant(stringConstantModel.value ?? string.Empty);
                        var cf = new ConstantString { Value = new StringReference(index, StringReference.Storage.Managed) };
                        node = MapPort(portToOffsetMapping, outputPortId, Direction.Output, ref cf.ValuePort.Port, cf);
                        return;
                    }
                case BooleanConstantNodeModel booleanConstantNodeModel:
                    {
                        var cf = new ConstantBool { Value = booleanConstantNodeModel.value };
                        node = MapPort(portToOffsetMapping, outputPortId, Direction.Output, ref cf.ValuePort.Port, cf);
                        return;
                    }
                case IntConstantModel intConstantModel:
                    {
                        var cf = new ConstantInt { Value = intConstantModel.value };
                        node = MapPort(portToOffsetMapping, outputPortId, Direction.Output, ref cf.ValuePort.Port, cf);
                        return;
                    }
                case FloatConstantModel floatConstantModel:
                    {
                        var cf = new ConstantFloat { Value = floatConstantModel.value };
                        node = MapPort(portToOffsetMapping, outputPortId, Direction.Output, ref cf.ValuePort.Port, cf);
                        return;
                    }
                case Vector2ConstantModel vector2ConstantModel:
                    {
                        var cf = new ConstantFloat2 { Value = vector2ConstantModel.value };
                        node = MapPort(portToOffsetMapping, outputPortId, Direction.Output, ref cf.ValuePort.Port, cf);
                        return;
                    }
                case Vector3ConstantModel vector3ConstantModel:
                    {
                        var cf = new ConstantFloat3 { Value = vector3ConstantModel.value };
                        node = MapPort(portToOffsetMapping, outputPortId, Direction.Output, ref cf.ValuePort.Port, cf);
                        return;
                    }
                case Vector4ConstantModel vector4ConstantModel:
                    {
                        var cf = new ConstantFloat4 { Value = vector4ConstantModel.value };
                        node = MapPort(portToOffsetMapping, outputPortId, Direction.Output, ref cf.ValuePort.Port, cf);
                        return;
                    }
                case QuaternionConstantModel quaternionConstantModel:
                    {
                        var cf = new ConstantQuaternion { Value = quaternionConstantModel.value };
                        node = MapPort(portToOffsetMapping, outputPortId, Direction.Output, ref cf.ValuePort.Port, cf);
                        return;
                    }
                case ObjectConstantModel _:
                    {
                        throw new NotImplementedException(
                            "Conversion and all - either a prefab (might live in a graph) or a scene object (must be injected during runtime bootstrap)");

                        // portToOffsetMapping = new Dictionary<IPortModel, uint>();
                        // var cf = new ConstantEntity {Value = objectConstantModel.value};
                        // MapPort(portToOffsetMapping, objectConstantModel.OutputPort, ref cf.ValuePort.Port, nodeId);
                        // node = cf;
                        // return;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        public static INode MapPort(PortMapper portToOffsetMapping, string portUniqueId, Direction direction, ref Port port, in INode node)
        {
            uint index = (uint)(portToOffsetMapping.Count + 1);
            portToOffsetMapping.Add(portUniqueId, direction, index);
            port.Index = index;
            return node;
        }

        public static INode MapPort(PortMapper portToOffsetMapping, IPortModel portUniqueId, ref Port port, in INode node)
        {
            uint index = (uint)(portToOffsetMapping.Count + 1);
            portToOffsetMapping.Add(portUniqueId.UniqueId, portUniqueId.Direction, index);
            port.Index = index;
            return node;
        }

        public CompilationResult TranslateAndCompile(VSGraphModel graphModel, AssemblyType assemblyType,
            CompilationOptions compilationOptions)
        {
            var builder = new GraphBuilder();

            // Pre-allocate data indices for graph variables
            foreach (var graphVariableModel in graphModel.GraphVariableModels)
            {
                // used inputs/outputs are created by their unique variableNodeModel in the graph. we'll create unused i/os later
                // this means duplicate variable I/O nodes will throw
                if (graphVariableModel.IsInputOrOutputTrigger())
                    continue;
                Value? initValue = GetValueFromConstant(builder, graphVariableModel.InitializationModel);
                builder.DeclareVariable(graphVariableModel, GraphBuilder.GetVariableType(graphVariableModel), initValue);
            }

            foreach (var nodeModel in graphModel.NodeModels)
            {
                if (nodeModel.State == ModelState.Disabled || nodeModel is IEdgePortalModel)
                    continue;

                {
                    var nodeId = builder.GetNextNodeId();
                    if (TranslateNode(builder, nodeModel, out var rnode, out var mapping, out var preAllocatedDataIndex))
                    {
                        // TODO theor this lambda is broken. we need per port index. a SubgraphReferenceNodeModel has two input data ports (Target and DataInputs) so that won't work
                        bool used = false;
                        builder.AddNodeFromModel(nodeModel, nodeId, rnode, mapping, _ =>
                        {
                            if (preAllocatedDataIndex.HasValue)
                            {
                                Unity.Assertions.Assert.IsFalse(used, "Preallocated data index used multiple times for the same node: " + nodeModel);
                                used = true;
                            }

                            return preAllocatedDataIndex;
                        });
                        switch (nodeModel)
                        {
                            // TODO not pretty
                            case SmartObjectReferenceNodeModel smartObjectReferenceNodeModel:
                                {
                                    uint targetEntityPreAllocatedDataIndex = builder.GetVariableDataIndex(smartObjectReferenceNodeModel.DeclarationModel).DataIndex;
                                    builder.BindVariableToInput(new GraphBuilder.VariableHandle(targetEntityPreAllocatedDataIndex), ((GraphReference)rnode).Target);
                                    FlagUnusedDataInputs(builder, smartObjectReferenceNodeModel);
                                    break;
                                }
                            case SubgraphReferenceNodeModel subgraphReferenceNodeModel:
                                {
                                    var targetIndex = builder.AllocateDataIndex();
                                    builder.BindVariableToInput(new GraphBuilder.VariableHandle(targetIndex), ((GraphReference)rnode).Target);
                                    builder.BindSubgraph(targetIndex, subgraphReferenceNodeModel.GraphReference);
                                    FlagUnusedDataInputs(builder, subgraphReferenceNodeModel);
                                    break;
                                }
                        }
                    }
                }

                // create a node and an edge for each embedded constant
                foreach (var portModel in nodeModel.InputsByDisplayOrder)
                {
                    if (portModel.EmbeddedValue == null || portModel.IsConnected)
                        continue;
                    var embeddedNodeId = builder.GetNextNodeId();
                    if (!TranslateNode(builder, portModel.EmbeddedValue, out var embeddedNode, out var embeddedPortMapping, out var embeddedPreAllocatedDataIndex))
                        continue;
                    builder.AddNodeFromModel(portModel.EmbeddedValue, embeddedNodeId, embeddedNode, embeddedPortMapping, _ => embeddedPreAllocatedDataIndex);
                    builder.CreateEdge(portModel.EmbeddedValue, string.Empty, nodeModel, portModel.UniqueId);
                }
            }

            foreach (var graphVariableModel in graphModel.GraphVariableModels)
            {
                // unused i/os only
                if (graphVariableModel.IsInputOrOutputTrigger())
                {
                    if (graphVariableModel.Modifiers == ModifierFlags.ReadOnly) // Input
                    {
                        if (!builder.GetExistingInputTrigger(graphVariableModel.VariableName, out _))
                            builder.AddNode(builder.DeclareInputTrigger(graphVariableModel.VariableName));
                    }
                    else
                    {
                        if (!builder.GetExistingOutputTrigger(graphVariableModel.VariableName, out _))
                            builder.AddNode(builder.DeclareOutputTrigger(graphVariableModel.VariableName));
                    }
                }
            }

            EvaluateEdgePortals<ExecutionEdgePortalEntryModel, ExecutionEdgePortalExitModel>();
            EvaluateEdgePortals<DataEdgePortalEntryModel, DataEdgePortalExitModel>();

            foreach (var edgeModel in graphModel.EdgeModels)
            {
                if (edgeModel?.OutputPortModel == null || edgeModel.InputPortModel == null)
                    continue;
                if (edgeModel.OutputPortModel.NodeModel is IEdgePortalModel || edgeModel.InputPortModel.NodeModel is IEdgePortalModel)
                    continue;
                if (edgeModel.OutputPortModel.NodeModel.State == ModelState.Disabled || edgeModel.InputPortModel.NodeModel.State == ModelState.Disabled)
                    continue;
                builder.CreateEdge(edgeModel.OutputPortModel, edgeModel.InputPortModel);
            }

            var stencil = ((DotsStencil)graphModel.Stencil);
            var result = builder.Build(stencil);
            var graphModelAssetModel = graphModel.AssetModel as Object;
            if (graphModelAssetModel)
            {
                if (!stencil.CompiledScriptingGraphAsset)
                {
                    stencil.CompiledScriptingGraphAsset = ScriptableObject.CreateInstance<ScriptingGraphAsset>();
                }

                stencil.CompiledScriptingGraphAsset.Definition = result.GraphDefinition;

                builder.CreateDebugSymbols(stencil);

                Utility.SaveAssetIntoObject(stencil.CompiledScriptingGraphAsset, graphModelAssetModel);
            }

            return result;

            void EvaluateEdgePortals<TEntry, TExit>()
                where TEntry : IEdgePortalEntryModel
                where TExit : IEdgePortalExitModel
            {
                // Create dictionaries of <PortalGroupID, List of all portals of that group in eval order> for entry and exit portals
                var portalEntryModels = graphModel.NodeModels.OfType<TEntry>()
                    .GroupBy(p => p.DeclarationModel.GetId())
                    .ToDictionary(x => x.Key, x => x.OrderBy(p => p.EvaluationOrder).ToList());
                var portalExitModels = graphModel.NodeModels.OfType<TExit>()
                    .GroupBy(p => p.DeclarationModel.GetId())
                    .ToDictionary(x => x.Key, x => x.OrderBy(p => p.EvaluationOrder).ToList());

                // Find all entry portals that have at least one connection to a non-portal node.
                var portalEntriesToEvaluate = portalEntryModels.SelectMany(kvp => kvp.Value)
                    .Where(p => p.InputPort.IsConnected && p.InputPort.ConnectionPortModels.Any(c => !(c.NodeModel is IEdgePortalModel)));

                foreach (var portalEntry in portalEntriesToEvaluate)
                {
                    var visitedPortals = new Stack<TEntry>();
                    var exitPortModels = new List<IPortModel>();

                    // Consider only the input ports from nodes that are not portals.
                    var entryPortModels = portalEntry.InputPort.ConnectionPortModels.Where(c => !(c.NodeModel is IEdgePortalModel));
                    GetAllUltimateExitPortsFrom(portalEntry);
                    foreach (var outputPort in entryPortModels)
                        foreach (var inputPort in exitPortModels)
                            builder.CreateEdge(outputPort, inputPort);

                    void GetAllUltimateExitPortsFrom(TEntry portalEntryModel)
                    {
                        // Guard against portal loops (they are not forbidden in the UI, but they don't make any sense)
                        if (visitedPortals.Contains(portalEntryModel))
                            return;
                        visitedPortals.Push(portalEntryModel);

                        if (portalExitModels.TryGetValue(portalEntryModel.DeclarationModel.GetId(), out var matchingExits))
                        {
                            foreach (var portModel in matchingExits.SelectMany(matchingExit => matchingExit.OutputPort.ConnectionPortModels))
                            {
                                // Recurse down any exit ports connected to a portal entry. Add the others to the list of exit ports we're after.
                                // Note that it's entirely possible and correct for a port to be there twice.
                                if (portModel.NodeModel is TEntry connectedPortalEntryModel)
                                    GetAllUltimateExitPortsFrom(connectedPortalEntryModel);
                                else
                                    exitPortModels.Add(portModel);
                            }
                        }
                        visitedPortals.Pop();
                    }
                }
            }
        }

        private void FlagUnusedDataInputs(GraphBuilder builder,
            IGraphReferenceNodeModel referenceNodeModel)
        {
            var executionPortsConnected = referenceNodeModel.InputsById.Count(x => x.Value.PortType == PortType.Execution && x.Value.IsConnected);
            if (executionPortsConnected == 0)
            {
                var dataPortsConnected = referenceNodeModel.InputsById.Count(x => x.Value.PortType != PortType.Execution && x.Value.IsConnected);
                if (dataPortsConnected != 0)
                    builder.AddWarning("This Graph Reference Node has connected data inputs, but no connected execution inputs. The data inputs won't be used.", referenceNodeModel);
            }
        }

        Value? GetValueFromConstant(GraphBuilder builder, IConstantNodeModel initializationModel)
        {
            if (initializationModel == null)
                return null;
            switch (initializationModel.Type.GenerateTypeHandle(initializationModel.VSGraphModel.Stencil).ToValueType())
            {
                case ValueType.Bool:
                    return (bool)initializationModel.ObjectValue;
                case ValueType.Int:
                    return (int)initializationModel.ObjectValue;
                case ValueType.Float:
                    return (float)initializationModel.ObjectValue;
                case ValueType.Float2:
                    return (float2)(Vector2)initializationModel.ObjectValue;
                case ValueType.Float3:
                    return (float3)(Vector3)initializationModel.ObjectValue;
                case ValueType.Float4:
                    return (float4)(Vector4)initializationModel.ObjectValue;
                case ValueType.Quaternion:
                    return (quaternion)(Quaternion)initializationModel.ObjectValue;
                // case ValueType.Entity:
                // return (Entity)initializationModel.ObjectValue;
                case ValueType.StringReference:
                    string s = (string)initializationModel.ObjectValue;
                    return new StringReference(builder.StoreStringConstant(s), StringReference.Storage.Managed);
                default:
                    return null;
            }
        }
    }

    public static class ValueExtensions
    {
        public static TypeHandle ValueTypeToTypeHandle(this ValueType valueType)
        {
            switch (valueType)
            {
                case ValueType.Unknown:
                    return TypeHandle.Unknown;
                case ValueType.Bool:
                    return TypeHandle.Bool;
                case ValueType.Int:
                    return TypeHandle.Int;
                case ValueType.Float:
                    return TypeHandle.Float;
                case ValueType.Float2:
                    return TypeHandle.Vector2;
                case ValueType.Float3:
                    return TypeHandle.Vector3;
                case ValueType.Float4:
                    return TypeHandle.Vector4;
                case ValueType.Quaternion:
                    return TypeHandle.Quaternion;
                case ValueType.Entity:
                    return DotsTypeHandle.Entity;
                case ValueType.StringReference:
                    return TypeHandle.String;
                default:
                    throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null);
            }
        }
    }
}