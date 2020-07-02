using System;
using System.Diagnostics;
using Modifier.Runtime;
using Unity.Collections;
using Unity.Entities;

public class DotsFrameTrace : IDisposable
{
    public enum StepType : byte
    {
        None,
        ExecutedNode,
        TriggeredPort,
        WrittenValue,
        ReadValue,
    }

    public static Action<int, int, Entity, string, DotsFrameTrace> OnRecordFrameTraceDelegate;
    public bool IsValid => NativeStream.IsCreated;

    public static void FlushFrameTrace(int scriptingGraphAssetID, int frameCount, Entity entity,
        string entityName, DotsFrameTrace frameTrace)
    {
        if (OnRecordFrameTraceDelegate != null)
            OnRecordFrameTraceDelegate?.Invoke(scriptingGraphAssetID, frameCount, entity, entityName, frameTrace);
        else // TODO there's a frame delay to register the delegate - the first frames are disposed right away to avoid leaks
            frameTrace.Dispose();
    }

    private NativeStream NativeStream;

    private NativeStream.Writer _writer;

    public DotsFrameTrace(Allocator allocator)
    {
        NativeStream = new NativeStream(1, allocator);
        _writer = NativeStream.AsWriter();
        _writer.BeginForEachIndex(0);
    }

    [Conditional("VS_TRACING")]
    public void RecordExecutedNode(NodeId executedNode, byte progress)
    {
        _writer.Write(StepType.ExecutedNode);
        _writer.Write(executedNode.GetIndex());
        _writer.Write(progress);
    }

    public void ReadExecutedNode(ref NativeStream.Reader reader, out NodeId nodeId, out byte progress)
    {
        nodeId = new NodeId(reader.Read<uint>());
        progress = reader.Read<byte>();
    }

    [Conditional("VS_TRACING")]
    public void RecordTriggeredPort(OutputTriggerPort triggeredPort)
    {
        _writer.Write(StepType.TriggeredPort);
        _writer.Write(triggeredPort.Port.Index);
    }

    public void ReadTriggeredPort(ref NativeStream.Reader reader, out OutputTriggerPort triggeredPort)
    {
        triggeredPort = new OutputTriggerPort { Port = new Port { Index = reader.Read<uint>() } };
    }

    [Conditional("VS_TRACING")]
    public void RecordWrittenValue(Value value, OutputDataPort outputDataPort)
    {
        _writer.Write(StepType.WrittenValue);
        _writer.Write(outputDataPort.Port.Index);
        _writer.Write(value);
    }

    public void ReadWrittenValue(ref NativeStream.Reader reader, out Value value, out OutputDataPort outputDataPort)
    {
        outputDataPort = new OutputDataPort { Port = new Port { Index = reader.Read<uint>() } };
        value = reader.Read<Value>();
    }

    [Conditional("VS_TRACING")]
    public void RecordReadValue(Value value, InputDataPort inputDataPort)
    {
        _writer.Write(StepType.ReadValue);
        _writer.Write(inputDataPort.Port.Index);
        _writer.Write(value);
    }

    public void ReadReadValue(ref NativeStream.Reader reader, out Value value, out InputDataPort inputDataPort)
    {
        inputDataPort = new InputDataPort { Port = new Port { Index = reader.Read<uint>() } };
        value = reader.Read<Value>();
    }

    public void EndRecording()
    {
        _writer.EndForEachIndex();
    }

    public NativeStream.Reader AsReader()
    {
        return NativeStream.AsReader();
    }

    public void Dispose()
    {
        if (NativeStream.IsCreated)
            NativeStream.Dispose();
    }
}