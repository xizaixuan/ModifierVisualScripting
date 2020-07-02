using System;
using System.Collections.Generic;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [Serializable, DotsSearcherItem("Flow/Wait for all triggers")]
    class AllTriggersNodeModel : DotsNodeModel<WaitForAll>, IHasMainExecutionInputPort, IHasMainExecutionOutputPort
    {
        public IPortModel ExecutionInputPort { get; set; }
        public IPortModel ExecutionOutputPort { get; set; }

        [HackContextualMenuVariableCount("Input", min: 2)]
        public int numCases = 2; // TODO allow changing this through the UI

        public override IReadOnlyDictionary<string, List<PortMetaData>> PortCustomData => new Dictionary<string, List<PortMetaData>>
        {
            {nameof(WaitForAll.Input), new List<PortMetaData>(InputPortsMetadata())},
        };

        IEnumerable<PortMetaData> InputPortsMetadata()
        {
            var defaultData = GetPortMetadata(nameof(WaitForAll.Input), m_Node);

            for (int i = 0; i < numCases; i++)
            {
                yield return defaultData;
            }
        }
    }
}
