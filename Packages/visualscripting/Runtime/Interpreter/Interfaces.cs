using System;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Modifier.Runtime
{
    public interface IPort
    {
        Port GetPort();
        int GetDataCount();
    }

    public enum PortDirection
    {
        Input,
        Output
    }

    public enum PortType
    {
        Data,
        Trigger
    }

    public interface INodeState
    {
    }

    public interface INode
    {
    }
    public interface IDataNode : INode
    {
        void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance;
    }

    public interface IConstantNode : IDataNode
    {
    }

    // ReSharper disable once UnusedTypeParameter
    public interface IConstantNode<T> : IConstantNode
    {
    }

    public interface IGraphInstance
    {
        TimeData Time { get; }
        Random Random { get; }
        Entity CurrentEntity { get; }
        EntityManager EntityManager { get; }

        // ReSharper disable once UnusedParameter.Global
        ref T GetState<T>(in IFlowNode<T> _) where T : unmanaged, INodeState;
        NativeString32 GetString32(StringReference messageStringReference);
        NativeString64 GetString64(StringReference messageStringReference);
        NativeString128 GetString128(StringReference messageStringReference);
        NativeString512 GetString512(StringReference messageStringReference);
        NativeString4096 GetString4096(StringReference messageStringReference);
        string GetString(StringReference messageStringReference);
        string GetString(Entity entity);
        void DispatchEvent(EventNodeData evt, int typeSize);
        bool IsNodeCurrentlyScheduledForUpdate();

        void Log(string message, GraphInstance.LogItem item = GraphInstance.LogItem.Node);

        int GetTriggeredIndex(InputTriggerMultiPort nodePort, InputTriggerPort triggeredPort);

        /// <summary>
        /// Trigger execution from a Trigger output node
        /// </summary>
        /// <param name="output">Output Trigger Node to execute from</param>
        void Trigger(OutputTriggerPort output);

        /// <summary>
        /// Runs the nested graph immediately on the specified entity. If the nested graph activates a GraphTriggerOutput, it will be interrupted, resume the parent graph, then go back to the nested graph and so on
        /// </summary>
        /// <param name="graphReference">The node holding the graph reference state, just used to enforce the type of the node state</param>
        /// <param name="target">The entity holding the referenced graph</param>
        /// <param name="triggerIndex">The index of the activated trigger in the graph reference node input list, it will be used to index the referenced graph definition's input trigger list</param>
        /// <returns>Execution.Running if the nested graph has active coroutines, done otherwise</returns>
        Execution RunNestedGraph(in GraphReference graphReference, Entity target, int triggerIndex);

        /// <summary>
        /// Activates a graph trigger output and interrupt the current graph/frame execution/>
        /// </summary>
        /// <param name="outputIndex">The index of the output trigger in the graph reference node, used to index the referenced graph definition's output trigger list. If uint.MaxValue, will only cause a copy of the graph output values to the graph reference node output data ports</param>
        /// <returns><see cref="Execution.Interrupt"/></returns>
        Execution TriggerGraphOutput(uint outputIndex);

        void Write(OutputDataPort port, Value value);

        bool ReadBool(InputDataPort port);
        int ReadInt(InputDataPort port);
        float ReadFloat(InputDataPort port);
        float2 ReadFloat2(InputDataPort port);
        float3 ReadFloat3(InputDataPort port);
        float4 ReadFloat4(InputDataPort port);
        quaternion ReadQuaternion(InputDataPort port);
        Entity ReadEntity(InputDataPort port);
        Value ReadValue(InputDataPort port);
        Value ReadValueOfType(InputDataPort port, ValueType valueType);
        bool HasConnectedValue(IOutputDataPort port);
        bool HasConnectedValue(IOutputTriggerPort port);
        bool HasConnectedValue(IInputDataPort port);
        Value GetComponentDefaultValue(TypeReference componentType, int fieldIndex);
        Value GetComponentValue(Entity e, TypeReference componentType, int fieldIndex);
        void SetComponentValue(Entity e, TypeReference componentType, int fieldIndex, Value value);
    }

    public interface IEntryPointNode : INode
    {
        void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance;
    }

    public interface IBaseFlowNode : INode
    {
    }

    public interface INodeReportProgress : IBaseFlowNode
    {
        byte GetProgress<TCtx>(TCtx ctx) where TCtx : IGraphInstance;
    }

    public interface IFlowNode : IBaseFlowNode
    {
        void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance;
    }

    public interface IStateFlowNode : IBaseFlowNode
    {
        Execution Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance;
        Execution Update<TCtx>(TCtx ctx) where TCtx : IGraphInstance;
    }

    // ReSharper disable once UnusedTypeParameter
    public interface IFlowNode<T> : IStateFlowNode where T : struct, INodeState
    {
    }

    public interface IEventNode
    {
        ulong EventId { get; set; }
    }

    public interface IEventDispatcherNode : IEventNode, IFlowNode
    {
        int EventTypeSize { get; set; }
    }

    public interface IEventReceiverNode : IEventNode, INode
    {
        Execution Execute<TCtx>(TCtx ctx, EventNodeData data) where TCtx : IGraphInstance;
    }

    public enum Execution : byte
    {
        Running,
        Done,
        Interrupt
    }
}