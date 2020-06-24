
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    public class GraphDefinition
    {
        /// <summary>
        /// Maps a BindingId (the source VariableModel GUID) to a DataIndex
        /// </summary>
        [Serializable]
        public struct InputBinding
        {
            public BindingId Id;
            public uint DataIndex;
        }

        [Serializable]
        public struct VariableInitValue
        {
            public uint DataIndex;
            public Value Value;
        }

        [Serializable]
        public struct InputOutputTrigger
        {
            public NodeId NodeId;
            public string Name;
            public InputOutputTrigger(NodeId nodeId, string name)
            {
                NodeId = nodeId;
                Name = name;
            }
        }

        [Serializable]
        public struct InputData
        {
            public uint DataPortIndex;
            public BindingId Name;
            public ValueType Type;

            public InputData(uint dataPortIndex, BindingId name, ValueType type)
            {
                DataPortIndex = dataPortIndex;
                Name = name;
                Type = type;
            }
        }

        [Serializable]
        public struct OutputData
        {
            public uint DataPortIndex;
            public BindingId Name;
            public ValueType Type;

            public OutputData(uint dataPortIndex, BindingId name, ValueType type)
            {
                DataPortIndex = dataPortIndex;
                Name = name;
                Type = type;
            }
        }

        [Serializable]
        public struct SubgraphReference
        {
            public uint SubgraphEntityDataIndex;
            public ScriptingGraphAsset Subgraph;

            public SubgraphReference(uint subgraphEntityDataIndex, ScriptingGraphAsset subgraph)
            {
                SubgraphEntityDataIndex = subgraphEntityDataIndex;
                Subgraph = subgraph;
            }
        }

        Dictionary<ulong, List<FieldDescription>> m_ComponentFieldDescriptions;
        Dictionary<ulong, List<FieldDescription>> m_EventFieldDescriptions;

        [SerializeReference]
        public List<INode> NodeTable = new List<INode>();          // Contain node list
        public List<PortInfo> PortInfoTable = new List<PortInfo>();   // Contain info on each port
        public List<NodeId> DataPortTable = new List<NodeId>();     // Contain Node owning each data entry
        public List<uint> TriggerTable = new List<uint>();        // Contain a list of ports to trigger
        public List<InputBinding> Bindings = new List<InputBinding>();
        public List<VariableInitValue> VariableInitValues = new List<VariableInitValue>();
        public List<string> Strings = new List<string>();
        public List<InputOutputTrigger> InputTriggers = new List<InputOutputTrigger>();
        public List<InputOutputTrigger> OutputTriggers = new List<InputOutputTrigger>();
        public List<InputData> InputDatas = new List<InputData>();
        public List<OutputData> OutputDatas = new List<OutputData>();
        public List<SubgraphReference> SubgraphReferences = new List<SubgraphReference>();
        public List<FieldDescription> ComponentFields = new List<FieldDescription>();
        public List<FieldDescription> EventFields = new List<FieldDescription>();
        public IReadOnlyDictionary<ulong, List<FieldDescription>> ComponentFieldDescriptions =>
            m_ComponentFieldDescriptions ?? (m_ComponentFieldDescriptions = CompileFieldDescriptions(ComponentFields));
        public IReadOnlyDictionary<ulong, List<FieldDescription>> EventFieldDescriptions =>
            m_EventFieldDescriptions ?? (m_EventFieldDescriptions = CompileFieldDescriptions(EventFields));

        public bool ContainsEventReceiver => NodeTable.OfType<IEventReceiverNode>().Any();

        Dictionary<ulong, List<FieldDescription>> CompileFieldDescriptions(IEnumerable<FieldDescription> source)
        {
            var d = new Dictionary<ulong, List<FieldDescription>>();
            foreach (var description in source)
            {
                if (!d.ContainsKey(description.DeclaringTypeHash))
                {
                    d.Add(description.DeclaringTypeHash, new List<FieldDescription> { description });
                }
                else
                {
                    d[description.DeclaringTypeHash].Add(description);
                }
            }
            return d;
        }

        public uint ComputeHash()
        {
            uint hash = 0;
            hash = HashUtility.HashCollection(NodeTable, HashUtility.HashBoxedUnmanagedStruct, hash);
            hash = HashUtility.HashCollection(PortInfoTable, HashUtility.HashManaged, hash);
            hash = HashUtility.HashCollection(DataPortTable, HashUtility.HashUnmanagedStruct, hash);
            hash = HashUtility.HashCollection(TriggerTable, HashUtility.HashUnmanagedStruct, hash);
            hash = HashUtility.HashCollection(Bindings, HashUtility.HashUnmanagedStruct, hash);
            hash = HashUtility.HashCollection(VariableInitValues, HashUtility.HashUnmanagedStruct, hash);
            hash = HashUtility.HashCollection(Strings, HashUtility.HashManaged, hash);
            return hash;
        }

        public string GraphDump()
        {
            var result = new List<string>
            {
                " Graph Dump",
                "------------",
                $"Number of nodes    : {NodeTable.Count}",
                $"Number of ports    : {PortInfoTable.Count - 1}", // -1 because Port 0 is NULL
                $"Number of data     : {DataPortTable.Count - 1}", // -1 because Data 0 is NULL
                $"Number of triggers : {TriggerTable.Count - 1}", // -1 because Trigger 0 is NULL
                $"Number of strings  : {Strings.Count}", // List of string constants
                $"Number of Components  : {ComponentFieldDescriptions.Count}", // List of serialized component type info
                "",
                "NODES"
            };

            result.AddRange(NodeTable.Select((t, i) => $"Node {i} => {t.GetType()}"));
            result.Add("");
            result.Add("PORT TABLE");

            for (int i = 0; i < PortInfoTable.Count; i++)
            {
                var portInfo = PortInfoTable[i];
                var slotType = portInfo.IsDataPort ? "Data" : "Trigger";
                var slotDir = portInfo.IsOutputPort ? "Output" : "Input";
                var str = $"{slotType} {slotDir} Port({i}, {portInfo.PortName}), belongs to Node {portInfo.NodeId.GetIndex()}";

                if (portInfo.IsDataPort)
                    str += portInfo.DataIndex == 0 ? " <UNCONNECTED PORT>" : $", uses {slotType} slot {portInfo.DataIndex}";
                else if (portInfo.IsOutputPort)
                {
                    str += portInfo.DataIndex == 0 ? " <UNCONNECTED PORT>" : " Port(s) to trigger on execution: ";
                    var triggerIndex = (int)portInfo.DataIndex;
                    while (triggerIndex < TriggerTable.Count && TriggerTable[triggerIndex] != 0)
                        str += $"{TriggerTable[triggerIndex++]}, ";
                }

                result.Add(str);
            }

            result.Add("");
            result.Add("TRIGGER TABLE");

            for (int i = 0; i < TriggerTable.Count; i++)
            {
                if (TriggerTable[i] != 0)
                {
                    string str = $"[{i}] ";
                    var portId = TriggerTable[i];
                    str += $"Port {portId}={NodeTable[(int)PortInfoTable[(int)portId].NodeId.GetIndex()].GetType()}.{PortInfoTable[(int)portId].PortName} ";
                    result.Add(str);
                }
            }

            result.Add("");
            result.Add("STRING TABLE");
            result.AddRange(Strings.Select((t, i) => $"[{i}] {t}"));
            result.Add("------------");

            return string.Join("\n\r", result.ToArray());
        }

        public bool GetInputBindingId(BindingId guid, out uint index)
        {
            for (int i = 0; i < Bindings.Count; i++)
            {
                if (Bindings[i].Id.Equals(guid))
                {
                    index = Bindings[i].DataIndex;
                    return true;
                }
            }

            index = 0;
            return false;
        }

        internal bool HasConnectedValue(IPort port)
        {
            if (port is IInputTriggerPort)
                throw new NotImplementedException();

            return PortInfoTable[(int)port.GetPort().Index].DataIndex != 0;
        }
    }
}