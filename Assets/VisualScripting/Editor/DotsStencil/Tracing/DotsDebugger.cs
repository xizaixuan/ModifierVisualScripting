using System;
using System.Collections.Generic;
using System.Linq;
using Modifier.Runtime;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEditor.Modifier.VisualScripting.Editor.Plugins;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine.Assertions;

namespace Modifier.DotsStencil
{
    public class DotsDebugger : IDebugger
    {
        class EntitySearcherItem : SearcherItem
        {
            public readonly Entity Entity;

            public EntitySearcherItem(Entity entity, string entityName)
                : base($"{entity}: {entityName}")
            {
                Entity = entity;
            }
        }

        public DotsDebugger()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.playModeStateChanged += x =>
            {
                if (x == PlayModeStateChange.ExitingPlayMode)
                    OnBeforeAssemblyReload();
            };
        }

        private void OnBeforeAssemblyReload()
        {
            if (_data == null)
                return;
            foreach (var graphTrace in _data.Values.SelectMany(graphTraces => graphTraces.Values))
                graphTrace.Frames.Dispose();
        }

        public Dictionary<NodeId, INodeModel> NodeMapping = new Dictionary<NodeId, INodeModel>();

        public Dictionary<uint, IPortModel> PortMapping = new Dictionary<uint, IPortModel>();

        /// <summary>
        /// Asset to (entity.Index, Entity+Frames[]) mapping
        /// </summary>
        private Dictionary<int, Dictionary<int, GraphTrace>> _data;

        public void Start(IGraphModel graphModel, bool tracingEnabled)
        {
            DotsFrameTrace.OnRecordFrameTraceDelegate = OnRecordFrameTraceDelegate;
            OnToggleTracing(graphModel, tracingEnabled);
        }

        public void Stop()
        {
            DotsFrameTrace.OnRecordFrameTraceDelegate = null;
        }

        public IEnumerable<int> GetDebuggingTargets(IGraphModel graphModel)
        {
            if (!GetGraphData(graphModel, out var graphData))
                return null;
            return graphData.Select(x => x.Value.Entity.Index).ToList();
        }

        private bool GetGraphData(IGraphModel graphModel, out Dictionary<int, GraphTrace> graphData)
        {
            graphData = null;
            if (_data == null)
                return false;
            int? scriptingGraphAssetID = (graphModel?.Stencil as DotsStencil)?.CompiledScriptingGraphAsset.GetInstanceID();
            if (!scriptingGraphAssetID.HasValue)
                return false;
            return _data.TryGetValue(scriptingGraphAssetID.Value, out graphData);
        }

        public string GetTargetLabel(IGraphModel graphModel, int target)
        {
            if (!GetGraphData(graphModel, out var graphData))
                return null;
            if (target == -1)
            {
                if (graphData.Count == 1)
                    return graphData.Values.Single().EntityName;
                return null;
            }

            return graphData.TryGetValue(target, out var data) ? data.EntityName : null;
        }

        public bool GetTracingSteps(IGraphModel currentGraphModel, int frame, int tracingTarget,
            out List<TracingStep> stepList)
        {
            stepList = null;

            var graphTrace = (GraphTrace)GetGraphTrace(currentGraphModel, tracingTarget);
            if (graphTrace?.Frames == null ||
                !graphTrace.Frames.BinarySearch(new EntityFrameData(frame, default), new EntityFrameData.FrameDataComparer(), out EntityFrameData entityFrameData))
            {
                return false;
            }

            foreach (var debuggingDataModel in entityFrameData.GetDebuggingSteps(currentGraphModel.Stencil))
            {
                if (stepList == null)
                    stepList = new List<TracingStep>();
                stepList.Add(debuggingDataModel);
            }

            return true;
        }

        internal bool ReadDebuggingDataModel(ref NativeStream.Reader reader, EntityFrameData entityFrameData, out TracingStep tracingStep)
        {
            tracingStep = default;
            IPortModel portModel;
            switch (reader.Read<DotsFrameTrace.StepType>())
            {
                case DotsFrameTrace.StepType.ExecutedNode:
                    entityFrameData.FrameTrace.ReadExecutedNode(ref reader, out NodeId id, out byte progress);
                    if (!NodeMapping.TryGetValue(id, out var nodeModel))
                        return false;
                    tracingStep = TracingStep.ExecutedNode(nodeModel, progress);
                    break;
                case DotsFrameTrace.StepType.TriggeredPort:
                    entityFrameData.FrameTrace.ReadTriggeredPort(ref reader, out OutputTriggerPort triggeredPort);
                    if (!PortMapping.TryGetValue(triggeredPort.Port.Index, out portModel))
                        return false;
                    tracingStep = TracingStep.TriggeredPort(portModel);
                    break;
                case DotsFrameTrace.StepType.WrittenValue:
                    entityFrameData.FrameTrace.ReadWrittenValue(ref reader, out Value writtenValue,
                        out OutputDataPort outputDataPort);
                    if (!PortMapping.TryGetValue(outputDataPort.Port.Index, out portModel))
                        return false;
                    tracingStep = TracingStep.WrittenValue(portModel, writtenValue.ToPrettyString());
                    break;
                case DotsFrameTrace.StepType.ReadValue:
                    entityFrameData.FrameTrace.ReadReadValue(ref reader, out Value readValue, out InputDataPort inputDataPort);
                    if (!PortMapping.TryGetValue(inputDataPort.Port.Index, out portModel))
                        return false;
                    tracingStep = TracingStep.ReadValue(portModel, readValue.ToPrettyString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        public IGraphTrace GetGraphTrace(IGraphModel graphModel, int currentTracingTarget)
        {
            Assert.IsNotNull(graphModel);
            int? scriptingGraphAssetID = (graphModel.Stencil as DotsStencil)?.CompiledScriptingGraphAsset.GetInstanceID();
            if (!scriptingGraphAssetID.HasValue || _data == null)
                return null;
            if (!_data.TryGetValue(scriptingGraphAssetID.Value, out var graphData))
                return null;
            if (currentTracingTarget == -1 && graphData.Count == 1)
                return graphData.Values.Single();
            if (!graphData.TryGetValue(currentTracingTarget, out var data))
                return null;
            // TODO TRACING
            return data;
        }

        public void OnToggleTracing(IGraphModel currentGraphModel, bool enabled)
        {
            ScriptingGraphRuntime.s_TracingEnabled = enabled;
        }

        private void OnRecordFrameTraceDelegate(int scriptingGraphAssetID, int frame, Entity entity,
            string entityName, DotsFrameTrace frameTrace)
        {
            frameTrace.EndRecording();
            if (_data == null)
                _data = new Dictionary<int, Dictionary<int, GraphTrace>>();
            if (!_data.TryGetValue(scriptingGraphAssetID, out var graphData))
                _data.Add(scriptingGraphAssetID, graphData = new Dictionary<int, GraphTrace>());
            if (!graphData.TryGetValue(entity.Index, out var frameBuffer))
                graphData.Add(entity.Index, frameBuffer = new GraphTrace(entity, entityName));

            frameBuffer.Frames.PushBack(new EntityFrameData(frame, frameTrace));
        }

        internal void CreateDebugSymbols(Dictionary<INodeModel, GraphBuilder.MappedNode> nodeMapping, Dictionary<INodeModel, PortMapper> portOffsetMappings)
        {
            NodeMapping = nodeMapping
                .Where(p => p.Value.NodeId.IsValid()) // some translations might be funky
                .ToDictionary(p => p.Value.NodeId, p => p.Key);
            PortMapping = portOffsetMappings
                .SelectMany(x => F(nodeMapping, x))
                .ToDictionary(x => x.PortIndex, x => x.PortModel);
        }

        private IEnumerable<(uint PortIndex, IPortModel PortModel)> F(Dictionary<INodeModel, GraphBuilder.MappedNode> nodeMapping,
            KeyValuePair<INodeModel, PortMapper> keyValuePair)
        {
            return keyValuePair.Value.Map((id, direction, offset) =>
            {
                // "OrDefault" because embedded constants won't have a matching port.
                var nodeModel = keyValuePair.Key;
                var portModel = nodeModel.GetPortModels().SingleOrDefault(p => p.Direction == direction && p.UniqueId == id);
                return (PortIndex: nodeMapping[nodeModel].FirstPortIndex + offset, PortModel: portModel);
            });
        }
    }
}
