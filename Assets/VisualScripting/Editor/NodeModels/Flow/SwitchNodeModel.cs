using System;
using System.Collections.Generic;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [Serializable, DotsSearcherItem("Flow/Switch")]
    class SwitchNodeModel : DotsNodeModel<Switch>, IHasMainInputPort, IHasMainExecutionInputPort, IHasMainExecutionOutputPort
    {
        public IPortModel ExecutionInputPort { get; set; }
        public IPortModel ExecutionOutputPort { get; set; }
        public IPortModel InputPort { get; set; }

        [HackContextualMenuVariableCount("Case", 2)]
        public int numCases = 2; // TODO allow changing this through the UI

        public override IReadOnlyDictionary<string, List<PortMetaData>> PortCustomData => new Dictionary<string, List<PortMetaData>>
        {
            {nameof(Switch.SwitchValues), new List<PortMetaData>(InputPortsMetadata())},
            {nameof(Switch.SwitchTriggers), new List<PortMetaData>(OutputPortsMetadata())},
        };

        IEnumerable<PortMetaData> InputPortsMetadata()
        {
            var defaultData = GetPortMetadata(nameof(Switch.SwitchValues), m_Node);
            for (int i = 0; i < numCases; i++)
            {
                defaultData.Name = $"Case {i + 1}";
                yield return defaultData;
            }
        }

        IEnumerable<PortMetaData> OutputPortsMetadata()
        {
            var defaultData = GetPortMetadata(nameof(Switch.SwitchTriggers), m_Node);
            for (int i = 0; i < numCases; i++)
            {
                defaultData.Name = $"Exec {i + 1}";
                yield return defaultData;
            }
        }
    }
}
