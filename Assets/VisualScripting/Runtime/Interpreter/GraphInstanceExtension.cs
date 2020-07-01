using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace Modifier.Runtime
{
    public interface IModifierTrigger
    {
    }

    public partial class GraphInstance
    {
        public static GraphInstance Create(GraphDefinition definition) 
            => definition?.NodeTable == null || definition.PortInfoTable == null ? null : new GraphInstance(definition);
        
        GraphInstance(GraphDefinition definition)
        {
            m_Definition = definition;
            m_DispatchedEvents = new List<EventNodeData>();
            _state.Init();
            m_DataValues = new Dictionary<uint, Value>();

            if ((logFilter & LogItem.Graph) != 0)
                Debug.Log(m_Definition.GraphDump());

            m_NodeStateOffsets = new int[definition.NodeTable.Count];
            int totalSize = 0;
            for (var i = 0; i < definition.NodeTable.Count; i++)
            {
                if (!(definition.NodeTable[i] is IBaseFlowNode baseFlowNode) || baseFlowNode is IFlowNode)
                {
                    m_NodeStateOffsets[i] = -1;
                    continue;
                }
                int stateSize = GetNodeStateSize(baseFlowNode);
                Assert.AreNotEqual(0, stateSize);
                m_NodeStateOffsets[i] = totalSize;
                totalSize += stateSize;
            }
            m_NodeStates = new NativeArray<byte>(totalSize, Allocator.Persistent, NativeArrayOptions.ClearMemory);

            for (var i = 0; i < definition.NodeTable.Count; i++)
            {
                if (definition.NodeTable[i] is IConstantNode constantNode)
                {
                    Log("Executing Constant node " + i + " " + constantNode.GetType().ToString(), LogItem.Init);
                    IDataNode dataNode = constantNode;
                    ExecuteDataNode(ref dataNode);
                    constantNode = (IConstantNode)dataNode;
                }
            }

            foreach (var initValue in definition.VariableInitValues)
            {
                m_DataValues[initValue.DataIndex] = initValue.Value;
            }
        }

        public bool TriggerModifier<T>() where T : struct, IModifierTrigger
        {
            bool anyTriggered = false;
            for (int i = 0; i < m_Definition.NodeTable.Count; i++)
            {
                var index = i;
                var n = m_Definition.NodeTable[i];
                if (n is T)
                {
                    anyTriggered = true;
                    _state.AddExecutionThisFrame(new NodeId((uint)index));
                }
            }

            return anyTriggered;
        }

        public bool ResumeFrame(TimeData time, EventNodeData evt, NativeMultiHashMap<Entity, uint> mOutputTriggersPerEntityGraphActivated)
        {
            OutputTriggersActivated = mOutputTriggersPerEntityGraphActivated;
            Time = time;
            var startTime = UnityEngine.Time.realtimeSinceStartup;
            ClearLog();
            Log("GraphInstance executing event" + evt);

            int nodeExecuted = 0;
            bool interrupt = false;
            while (_state.NodesToExecute.Count > 0 && !interrupt)
            {
                if (nodeExecuted++ >= k_MaxNodesPerFrame)
                {
                    Debug.LogWarning($"Trying to execute more than {k_MaxNodesPerFrame} nodes in a frame, something seems wrong.");
                    break;
                }
                
                var activeExec = _state.NodesToExecute.Pop();
                bool alreadyRunning = _state.NextFrameNodes.Any(ex => ex.nodeId.GetIndex() == activeExec.nodeId.GetIndex());
                var exec = ExecuteNode(activeExec, alreadyRunning, evt);
                switch (exec)
                {
                    case Execution.Interrupt:
                        interrupt = true;
                        break;

                    case Execution.Running:
                        {
                            if (!alreadyRunning)
                                _state.NextFrameNodes.Add(new NodeExecution { nodeId = activeExec.nodeId, portIndex = 0 });
                            break;
                        }
                }
            }

            Log($"Entity {GetString(CurrentEntity)} ran {nodeExecuted} nodes this frame", LogItem.Performance);
            var durationTime = UnityEngine.Time.realtimeSinceStartup - startTime;
            Log("Graph execution time for event " + evt + " = " + durationTime * 1000000 + "usec", LogItem.Performance);
            DumpLog();

            CurrentEntity = Entity.Null;
            return _state.NodesToExecute.Count > 0;
        }
    }
}
