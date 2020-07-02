using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Modifier.DotsStencil;
using Modifier.Runtime;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Modifier.GraphElements;
using UnityEditor;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.Assertions;
using Port = Modifier.Runtime.Port;
using PortType = Modifier.Runtime.PortType;
using SetVariableNodeModel = Modifier.DotsStencil.SetVariableNodeModel;
using ValueType = Modifier.Runtime.ValueType;

public class GraphBuilder
{
    internal struct MappedNode
    {
        public uint FirstPortIndex;
        public NodeId NodeId;
    }

    const string k_GraphDumpMenu = "Visual Scripting/Dump Graph";

    static bool s_IsGraphDumpEnabled;
    static List<FieldDescription> s_EventFields = new List<FieldDescription>();

    /// <summary>
    /// Used during edge creation to get the port indices and to create the debug symbols to map NodeIds back to node models for tracing
    /// </summary>
    Dictionary<INodeModel, MappedNode> m_NodeMapping;
    TranslationSetupContext m_Ctx;
    Dictionary<INodeModel, PortMapper> m_PortOffsetMappings;
    List<(uint outputPortIndex, uint inputPortIndex)> m_EdgeTable;
    Dictionary<BindingId, VariableHandle> m_VariableToDataIndex;
    HashSet<TypeReference> m_ReferencedComponentTypeIndices;
    StringBuilder m_GraphDump;
    DotsTranslator.DotsCompilationResult m_Result;

    public GraphBuilder()
    {
        m_Result = new DotsTranslator.DotsCompilationResult();
        m_NodeMapping = new Dictionary<INodeModel, MappedNode>();
        m_Ctx = new TranslationSetupContext();
        m_PortOffsetMappings = new Dictionary<INodeModel, PortMapper>();
        m_EdgeTable = new List<(uint, uint)>();
        m_VariableToDataIndex = new Dictionary<BindingId, VariableHandle>();
        m_GraphDump = new StringBuilder();
        m_ReferencedComponentTypeIndices = new HashSet<TypeReference>();

        m_Result.GraphDefinition.PortInfoTable.Add(new PortInfo { NodeId = NodeId.Null, PortName = "NULL" });
        m_Result.GraphDefinition.DataPortTable.Add(NodeId.Null);
    }

    public void AddError(string description, INodeModel nodeModel)
    {
        m_Result.AddError(description, nodeModel);
    }

    public void AddWarning(string description, INodeModel nodeModel)
    {
        m_Result.AddWarning(description, nodeModel);
    }

    public NodeId GetNextNodeId()
    {
        return new NodeId((uint)(m_Result.GraphDefinition.NodeTable.Count));
    }

    public uint AllocateDataIndex()
    {
        var index = (uint)m_Result.GraphDefinition.DataPortTable.Count;
        m_Result.GraphDefinition.DataPortTable.Add(NodeId.Null);
        return index;
    }

    public VariableHandle GetVariableDataIndex(IVariableDeclarationModel variableModelDeclarationModel)
    {
        return m_VariableToDataIndex[GetBindingId(variableModelDeclarationModel)];
    }

    public enum VariableType
    {
        ObjectReference,
        Variable,
        InputOutput,
        SmartObject
    }

    public void DeclareVariable(IVariableDeclarationModel graphVariableModel, VariableType type, Value? initValue = null)
    {
        Assert.IsFalse(graphVariableModel.IsInputOrOutputTrigger());
        var bindingId = GetBindingId(graphVariableModel);
        uint dataIndex;
        if (type == VariableType.ObjectReference || type == VariableType.SmartObject)
        {
            Assert.IsFalse(initValue.HasValue);
            dataIndex = DeclareObjectReferenceVariable(bindingId).DataIndex;
        }
        else
        {
            dataIndex = AllocateDataIndex();

            if (initValue.HasValue)
            {
                m_Result.GraphDefinition.VariableInitValues.Add(new GraphDefinition.VariableInitValue { DataIndex = dataIndex, Value = initValue.Value });
            }
            var variableHandle = new VariableHandle(dataIndex);

            m_VariableToDataIndex.Add(bindingId, variableHandle);

            if (type == VariableType.InputOutput)
            {
                if (graphVariableModel.IsDataOutput())
                {
                    DeclareOutputData(bindingId, graphVariableModel.DataType.ToValueType(), variableHandle);
                }
                else
                {
                    DeclareInputData(bindingId, graphVariableModel.DataType.ToValueType(), variableHandle);
                }
            }
        }
    }

    public VariableHandle DeclareObjectReferenceVariable(BindingId bindingId)
    {
        var variableHandle = BindVariableToDataIndex(bindingId);
        m_Result.GraphDefinition.Bindings.Add(new GraphDefinition.InputBinding { Id = bindingId, DataIndex = variableHandle.DataIndex });
        return variableHandle;
    }

    public struct VariableHandle
    {
        public uint DataIndex;

        public VariableHandle(uint dataIndex)
        {
            DataIndex = dataIndex;
        }
    }

    public VariableHandle BindVariableToDataIndex(BindingId variableId)
    {
        uint dataIndex = AllocateDataIndex();
        m_VariableToDataIndex.Add(variableId, new VariableHandle(dataIndex));
        return new VariableHandle { DataIndex = dataIndex };
    }

    public static BindingId GetBindingId(IVariableDeclarationModel graphVariableModel)
    {
        var strGuid = graphVariableModel.GetId();
        GUID.TryParse(strGuid.Replace("-", null), out var guid);
        SerializableGUID serializableGUID = guid;
        serializableGUID.ToParts(out var p1, out var p2);
        BindingId id = BindingId.From(p1, p2);
        return id;
    }

    public int StoreStringConstant(string value)
    {
        if (value == null)
            value = string.Empty;
        var index = m_Result.GraphDefinition.Strings.FindIndex(s => s == value);
        if (index == -1)
        {
            index = m_Result.GraphDefinition.Strings.Count;
            m_Result.GraphDefinition.Strings.Add(value);
        }

        return index;
    }

    public void AddNodeFromModel(INodeModel nodeModel, NodeId nodeId, INode node, PortMapper portToOffsetMapping, Func<IPort, uint?> getOutputDataPortPreAllocatedDataIndex)
    {
        if (portToOffsetMapping == null)
            portToOffsetMapping = new PortMapper();
        m_GraphDump?.AppendLine($"  Node GUID: {nodeModel.Guid} Name: {nodeModel.Title}:\r\n" + portToOffsetMapping);

        // things to set up here: portOffsetMappings, nodeMapping + AddNode everything
        var lastPortIndex = LastPortIndex;
        AddNodeInternal(nodeId, node, getOutputDataPortPreAllocatedDataIndex);
        ReplaceNodeModelMapping(nodeModel, portToOffsetMapping, nodeId, lastPortIndex);
    }

    public uint LastPortIndex => m_Ctx.LastPortIndex;

    public void ReplaceNodeModelMapping(INodeModel nodeModel, PortMapper portToOffsetMapping, NodeId? nodeId, uint firstPortIndex)
    {
        m_PortOffsetMappings.Add(nodeModel, portToOffsetMapping);
        m_NodeMapping.Add(nodeModel, new MappedNode { NodeId = nodeId.GetValueOrDefault(), FirstPortIndex = firstPortIndex });
    }

    public T AddNode<T>(T onUpdate, Func<IPort, uint?> getOutputDataPortPreAllocatedDataIndex = null) where T : struct, INode
    {
        return (T)AddNodeInternal(GetNextNodeId(), onUpdate, getOutputDataPortPreAllocatedDataIndex);
    }

    INode AddNodeInternal(NodeId id, INode node, Func<IPort, uint?> getOutputDataPortPreAllocatedDataIndex)
    {
        // For each port, bake its indices
        foreach (var fieldInfo in BaseDotsNodeModel.GetNodePorts(node.GetType()))
        {
            var innerPort = m_Ctx.SetupPort(node, fieldInfo, out var direction, out var portType, out var portName);

            for (int i = 0; i < innerPort.GetDataCount(); i++)
            {
                uint dataIndex = 0;
                if (portType == PortType.Data && direction == PortDirection.Output)
                {
                    dataIndex = getOutputDataPortPreAllocatedDataIndex?.Invoke(innerPort) ?? AllocateDataIndex();

                    // store port default values in graph definition
                    var metadata = BaseDotsNodeModel.GetPortMetadata(node, fieldInfo);
                    if (metadata.DefaultValue != null)
                    {
                        m_Result.GraphDefinition.VariableInitValues.Add(new GraphDefinition.VariableInitValue
                        {
                            DataIndex = dataIndex,
                            Value = ValueFromTypeAndObject(fieldInfo, metadata.DefaultValue, node)
                        });
                    }
                }

                bool isDataPort = portType == PortType.Data;
                bool isOutputPort = direction == PortDirection.Output;
                var newPortInfo = new PortInfo { IsDataPort = isDataPort, IsOutputPort = isOutputPort, DataIndex = dataIndex, NodeId = id, PortName = portName };
                m_Result.GraphDefinition.PortInfoTable.Add(newPortInfo);
            }
        }

        // Add the node to the definition
        m_Result.GraphDefinition.NodeTable.Add(node);
        return node;
    }

    Value ValueFromTypeAndObject(FieldInfo fieldInfo, object value, INode node)
    {
        var metadata = BaseDotsNodeModel.GetPortMetadata(node, fieldInfo);
        switch (metadata.Type)
        {
            case ValueType.Bool:
                return (bool)value;
            case ValueType.Int:
                return (int)value;
            case ValueType.Float:
            case ValueType.Float2: // cannot put a float2/3/4 in an attribute
            case ValueType.Float3:
            case ValueType.Float4:
                return (float)value;
            case ValueType.StringReference:
                return new Value
                {
                    StringReference = new StringReference(
                        StoreStringConstant((string)value),
                        StringReference.Storage.Managed)
                };
            default:
                throw new ArgumentOutOfRangeException(metadata.Type.ToString());
        }
    }

    public void CreateEdge(OutputTriggerPort outputPortModel, InputTriggerPort inputPortModel) => CreateEdge(outputPortModel.Port.Index, inputPortModel.Port.Index);

    public void CreateEdge(OutputDataPort outputPortModel, InputDataPort inputPortModel) => CreateEdge(outputPortModel.Port.Index, inputPortModel.Port.Index);

    public void CreateEdge(IPortModel outputPortModel, IPortModel inputPortModel)
    {
        var outputNode = outputPortModel.NodeModel;
        var outputPortUniqueId = outputPortModel.UniqueId;
        var inputNode = inputPortModel.NodeModel;
        var inputPortUniqueId = inputPortModel.UniqueId;

        CreateEdge(outputNode, outputPortUniqueId, inputNode, inputPortUniqueId);
    }

    public void CreateEdge(INodeModel outputNode, string outputPortUniqueId, INodeModel inputNode, string inputPortUniqueId)
    {
        if (outputNode.State == ModelState.Disabled)
            return;
        if (inputNode.State == ModelState.Disabled)
            return;

        if (GetPortIndex(inputNode, inputPortUniqueId, Direction.Input, out var inputPortIndex))
        {
            if (outputNode is VariableNodeModel variableNodeModel)
            {
                var variableType = GetVariableType(variableNodeModel.DeclarationModel);
                if (variableType == VariableType.Variable || variableNodeModel.DeclarationModel.IsDataInput())
                {
                    if (!(variableNodeModel is SetVariableNodeModel setVariableNodeModel) || setVariableNodeModel.IsGetter)
                    {
                        var varIndex = GetVariableDataIndex(variableNodeModel.DeclarationModel);
                        BindVariableToInput(varIndex, new InputDataPort { Port = new Port { Index = inputPortIndex } });
                        return;
                    }
                }
            }

            if (GetPortIndex(outputNode, outputPortUniqueId, Direction.Output, out var outputPortIndex))
            {
                CreateEdge(outputPortIndex, inputPortIndex);

                m_GraphDump?.AppendLine(
                    $"  {outputPortUniqueId}:{outputPortIndex} -> {inputPortUniqueId}:{inputPortIndex}");
            }
        }

        bool GetPortIndex(INodeModel nodeMode, string portUniqueId, Direction portDirection, out uint portIndex)
        {
            portIndex = default;
            if (m_NodeMapping.TryGetValue(nodeMode, out var mapping))
            {
                portIndex = mapping.FirstPortIndex + m_PortOffsetMappings[nodeMode].GetOffset(portUniqueId, portDirection);
                return true;
            }
            Debug.LogError($"Cannot resolve port for portmodel: {nodeMode} {portUniqueId}");
            return false;
        }
    }

    public void BindVariableToInput(VariableHandle variableHandle, in InputDataPort inputPort)
    {
        var portInfo = m_Result.GraphDefinition.PortInfoTable[(int)inputPort.Port.Index];
        portInfo.DataIndex = variableHandle.DataIndex;
        m_Result.GraphDefinition.PortInfoTable[(int)inputPort.Port.Index] = portInfo;
    }

    void CreateEdge(uint outputPortIndex, uint inputPortIndex)
    {
        m_EdgeTable.Add((outputPortIndex, inputPortIndex));
        if (outputPortIndex >= m_Result.GraphDefinition.PortInfoTable.Count)
        {
            Debug.LogError("!!!!");
            return;
        }

        // Count the number of output edge for each output trigger port
        PortInfo outputPortInfo = m_Result.GraphDefinition.PortInfoTable[(int)outputPortIndex];
        if (!outputPortInfo.IsDataPort)
        {
            outputPortInfo.DataIndex++;
            m_Result.GraphDefinition.PortInfoTable[(int)outputPortIndex] = outputPortInfo;
        }

        PortInfo inputPortInfo = m_Result.GraphDefinition.PortInfoTable[(int)inputPortIndex];
        Assert.AreEqual(outputPortInfo.IsDataPort, inputPortInfo.IsDataPort, "Only ports of the same kind (trigger or data) can be connected");
    }

    public DotsTranslator.DotsCompilationResult Build(Modifier.DotsStencil.DotsStencil stencil)
    {
        // Compute the output trigger edge table. Start at 1, because 0 is reserved for null
        // For each output trigger port, add an entry if they have at least 1 edge. Allocate an extra slot for the trailing null
        uint edgeTableSize = 1;
        var definition = m_Result.GraphDefinition;

        for (int i = 0; i < definition.PortInfoTable.Count; i++)
        {
            var port = definition.PortInfoTable[i];
            if (port.IsOutputPort && !port.IsDataPort)
            {
                uint nbOutputEdge = port.DataIndex + 1;
                if (nbOutputEdge > 1)
                {
                    port.DataIndex = edgeTableSize;
                    definition.PortInfoTable[i] = port;
                    edgeTableSize += nbOutputEdge;
                }
            }
        }

        // Fill the table
        for (uint i = 0; i < edgeTableSize; i++)
            definition.TriggerTable.Add(0);

        var connectedOutputDataPorts = new HashSet<PortInfo>();
        // Process the edge table
        foreach (var edge in m_EdgeTable)
        {
            // Retrieve the input & output port info
            PortInfo outputPortInfo = definition.PortInfoTable[(int)edge.Item1];
            PortInfo inputPortInfo = definition.PortInfoTable[(int)edge.Item2];
            Assert.AreEqual(outputPortInfo.IsDataPort, inputPortInfo.IsDataPort, "Only ports of the same kind (trigger or data) can be connected");
            if (outputPortInfo.IsDataPort)
            {
                // For data port, copy the DataIndex of the output port in the dataindex of the input port &
                // Keep track of the output node (because we will pull on it & execute it)
                // TODO: Optim opportunity here: We could detect flownode & constant here & avoid runtime checks by cutting link
                inputPortInfo.DataIndex = outputPortInfo.DataIndex;
                definition.DataPortTable[(int)inputPortInfo.DataIndex] = outputPortInfo.NodeId;
                definition.PortInfoTable[(int)edge.Item2] = inputPortInfo;
                connectedOutputDataPorts.Add(outputPortInfo);
            }
            else
            {
                // For trigger port, we need to find a spot in the trigger table & set it
                int triggerTableIndex = (int)outputPortInfo.DataIndex;
                while (definition.TriggerTable[triggerTableIndex] != 0)
                    triggerTableIndex++;
                definition.TriggerTable[triggerTableIndex] = edge.Item2;
            }
        }

        // Reset DataIndex of non-connected outputDataPort
        for (var i = 0; i < definition.PortInfoTable.Count; ++i)
        {
            if (definition.PortInfoTable[i].IsOutputPort && definition.PortInfoTable[i].IsDataPort)
            {
                var isConnected = connectedOutputDataPorts.Contains(definition.PortInfoTable[i]);
                var isVariablePort = m_VariableToDataIndex.Any(h => h.Value.DataIndex == definition.PortInfoTable[i].DataIndex);

                if (!isConnected && !isVariablePort)
                {
                    var port = definition.PortInfoTable[i];
                    port.DataIndex = 0;
                    definition.PortInfoTable[i] = port;
                }
            }
        }

        // Serialize runtime data for each referenced component type
        foreach (var typeReference in m_ReferencedComponentTypeIndices)
        {
            var t = TypeManager.GetType(typeReference.TypeIndex);
            var fields = t.GetFields();
            foreach (var field in fields)
            {
                if (field.FieldType.GenerateTypeHandle(stencil).ToValueType(out var valueType))
                {
                    var fieldDescription = new FieldDescription
                    {
                        Offset = UnsafeUtility.GetFieldOffset(field),
                        FieldValueType = valueType,
                        DeclaringTypeHash = typeReference.TypeHash,
                        Storage = GetStringStorage(field.FieldType)
                    };
                    m_Result.GraphDefinition.ComponentFields.Add(fieldDescription);
                }
                else
                {
                    // skip any field we don't know how to handle
                    Debug.LogWarning($"Skipping {t.FullName}.{field.Name} of type {field.FieldType.Name}");
                }
            }
        }

        // Store events fields descriptions
        if (!s_EventFields.Any())
        {
            var eventTypes = TypeCache.GetTypesDerivedFrom<IVisualScriptingEvent>();
            foreach (var eventType in eventTypes)
            {
                var eventTypeHash = TypeHash.CalculateStableTypeHash(eventType);
                var fields = eventType.GetFields(BindingFlags.Instance | BindingFlags.Public);

                foreach (var field in fields)
                {
                    if (field.FieldType.GenerateTypeHandle(stencil).ToValueType(out var valueType))
                    {
                        s_EventFields.Add(new FieldDescription
                        {
                            Offset = UnsafeUtility.GetFieldOffset(field),
                            FieldValueType = valueType,
                            DeclaringTypeHash = eventTypeHash,
                            Storage = GetStringStorage(field.FieldType)
                        });
                    }
                    else
                    {
                        // skip any field we don't know how to handle
                        Debug.LogWarning($"Skipping {eventType.FullName}.{field.Name} of type {field.FieldType.Name}");
                    }
                }
            }
        }

        m_Result.GraphDefinition.EventFields.AddRange(s_EventFields);

        if (s_IsGraphDumpEnabled)
            Debug.Log(m_Result.GraphDefinition.GraphDump());

        return m_Result;
    }

    static StringReference.Storage GetStringStorage(Type type)
    {
        if (type == typeof(string)) return StringReference.Storage.Managed;
        if (type == typeof(NativeString32)) return StringReference.Storage.Unmanaged32;
        if (type == typeof(NativeString64)) return StringReference.Storage.Unmanaged64;
        if (type == typeof(NativeString128)) return StringReference.Storage.Unmanaged128;
        if (type == typeof(NativeString512)) return StringReference.Storage.Unmanaged512;
        return type == typeof(NativeString4096) ? StringReference.Storage.Unmanaged4096 : StringReference.Storage.None;
    }

    public void CreateDebugSymbols(Modifier.DotsStencil.DotsStencil stencil)
    {
        ((DotsDebugger)stencil.Debugger).CreateDebugSymbols(m_NodeMapping, m_PortOffsetMappings);
    }

    public static VariableType GetVariableType(IVariableDeclarationModel graphVariableModel)
    {
        VariableDeclarationModel decl = (VariableDeclarationModel)graphVariableModel;
        if (decl.IsSmartObject())
            return VariableType.SmartObject;
        if (decl.IsObjectReference())
            return VariableType.ObjectReference;
        if (decl.IsInputOrOutput())
            return VariableType.InputOutput;
        return VariableType.Variable;
    }

    public void DeclareInputData(BindingId name, ValueType type, VariableHandle dataIndex)
    {
        Assert.IsTrue(m_VariableToDataIndex.ContainsKey(name));

        var triggerList = m_Result.GraphDefinition.InputDatas;
        var triggerIndex = triggerList.FindIndex(t => t.Name.Equals(name));
        Assert.AreEqual(-1, triggerIndex, $"An input data with the same name '{name}' already exists");

        triggerList.Add(new GraphDefinition.InputData(dataIndex.DataIndex, name, type));
    }

    /// <summary>
    /// Declares a graph data output, returns its index + 1 in the data output list
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="dataIndex"></param>
    /// <returns>The data output id (its index + 1 in the data output list)</returns>
    public uint DeclareOutputData(BindingId name, ValueType type, VariableHandle dataIndex)
    {
        Assert.IsTrue(m_VariableToDataIndex.ContainsKey(name));

        var triggerList = m_Result.GraphDefinition.OutputDatas;
        var triggerIndex = triggerList.FindIndex(t => t.Name.Equals(name));
        Assert.AreEqual(-1, triggerIndex, $"An input data with the same name '{name}' already exists");

        // data index will be patched when building the graph
        triggerList.Add(new GraphDefinition.OutputData(dataIndex.DataIndex, name, type));
        return (uint)triggerList.Count;
    }

    /// <summary>
    /// Declares an input trigger, but doesn't add the node to the internal node table
    /// </summary>
    /// <param name="name"></param>
    /// <returns>The graph trigger input node</returns>
    public GraphTriggerInput DeclareInputTrigger(string name)
    {
        var triggerList = m_Result.GraphDefinition.InputTriggers;
        var triggerIndex = triggerList.FindIndex(t => t.Name == name);
        Assert.AreEqual(-1, triggerIndex, $"An input trigger with the same name '{name}' already exists");

        var id = GetNextNodeId();
        var input = new GraphTriggerInput();
        triggerList.Add(new GraphDefinition.InputOutputTrigger(id, name));
        return input;
    }

    public bool GetExistingInputTrigger(string name, out GraphTriggerInput trigger)
    {
        var triggerList = m_Result.GraphDefinition.InputTriggers;
        var triggerIndex = triggerList.FindIndex(t => t.Name == name);
        if (triggerIndex == -1)
        {
            trigger = default;
            return false;
        }
        trigger = (GraphTriggerInput)m_Result.GraphDefinition.NodeTable[(int)triggerList[triggerIndex].NodeId.GetIndex()];
        return true;
    }

    /// <summary>
    /// Declares an output trigger, but doesn't add the node to the internal node table
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GraphTriggerOutput DeclareOutputTrigger(string name)
    {
        List<GraphDefinition.InputOutputTrigger> triggerList = m_Result.GraphDefinition.OutputTriggers;
        int triggerIndex = triggerList.FindIndex(t => t.Name == name);
        Assert.AreEqual(-1, triggerIndex, $"An output trigger with the same name '{name}' already exists");

        NodeId id = GetNextNodeId();
        int outputIndex = triggerList.Count;
        GraphTriggerOutput input = new GraphTriggerOutput { OutputIndex = (uint)outputIndex };
        triggerList.Add(new GraphDefinition.InputOutputTrigger(id, name));
        return input;
    }

    public bool GetExistingOutputTrigger(string name, out GraphTriggerOutput trigger)
    {
        var triggerList = m_Result.GraphDefinition.OutputTriggers;
        var triggerIndex = triggerList.FindIndex(t => t.Name == name);
        if (triggerIndex == -1)
        {
            trigger = default;
            return false;
        }
        trigger = (GraphTriggerOutput)m_Result.GraphDefinition.NodeTable[(int)triggerList[triggerIndex].NodeId.GetIndex()];
        return true;
    }

    public void AddReferencedComponent(TypeReference typeReference)
    {
        m_ReferencedComponentTypeIndices.Add(typeReference);
    }

    [MenuItem("internal:" + k_GraphDumpMenu, false)]
    static void CreateGraphDumpMenu(MenuCommand menuCommand)
    {
        s_IsGraphDumpEnabled = !s_IsGraphDumpEnabled;
    }

    [MenuItem("internal:" + k_GraphDumpMenu, true)]
    static bool SwitchGraphDumpMenu()
    {
        Menu.SetChecked(k_GraphDumpMenu, s_IsGraphDumpEnabled);
        return true;
    }

    public void BindSubgraph(uint subgraphEntityDataIndex, VSGraphAssetModel subgraph)
    {
        if (!subgraph || !(subgraph.GraphModel?.Stencil is Modifier.DotsStencil.DotsStencil subgraphStencil))
        {
            Unity.Assertions.Assert.IsTrue(false);
            return;
        }
        if (!subgraphStencil.CompiledScriptingGraphAsset)
            DotsGraphTemplate.CreateDotsCompiledScriptingGraphAsset(subgraph.GraphModel);
        BindSubgraph(subgraphEntityDataIndex, subgraphStencil.CompiledScriptingGraphAsset);
    }

    internal void BindSubgraph(uint subgraphEntityDataIndex, ScriptingGraphAsset subgraphAsset)
    {
        m_Result.GraphDefinition.SubgraphReferences.Add(
            new GraphDefinition.SubgraphReference(subgraphEntityDataIndex, subgraphAsset));
    }
}