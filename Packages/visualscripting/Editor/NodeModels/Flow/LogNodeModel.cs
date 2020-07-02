using System;
using System.Collections.Generic;
using System.Linq;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [Serializable,  EnumNodeSearcher(typeof(Log.LogType), "Debug", format: k_Title + " {0}")]
    class LogNodeModel : DotsNodeModel<Log>, IHasMainExecutionInputPort, IHasMainExecutionOutputPort, IHasMainInputPort
    {
        private const string k_Title = "Log";
        public override string Title => $"{k_Title} {TypedNode.Type}";
        public IPortModel ExecutionInputPort { get; set; }
        public IPortModel ExecutionOutputPort { get; set; }
        public IPortModel InputPort { get; set; }

        [HackContextualMenuVariableCount("Message")]
        public int numCases = 1; // TODO allow changing this through the UI

        public override IReadOnlyDictionary<string, List<PortMetaData>> PortCustomData => new Dictionary<string, List<PortMetaData>>
        {
            { nameof(Log.Messages), InputPortsMetadata().ToList() }
        };

        IEnumerable<PortMetaData> InputPortsMetadata()
        {
            var defaultData = GetPortMetadata(nameof(Log.Messages), m_Node);
            for (int i = 0; i < numCases; i++)
            {
                defaultData.Name = $"Message {i + 1}";
                yield return defaultData;
            }
        }
    }
}
