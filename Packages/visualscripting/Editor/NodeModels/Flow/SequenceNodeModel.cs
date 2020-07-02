using System;
using System.Collections.Generic;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [Serializable, DotsSearcherItem("Flow/Stepper")]
    class SequenceNodeModel : DotsNodeModel<Stepper>, IHasMainExecutionInputPort, IHasMainExecutionOutputPort
    {
        public IPortModel ExecutionInputPort { get; set; }
        public IPortModel ExecutionOutputPort { get; set; }

        [HackContextualMenuVariableCount("Step", 2)]
        public int numSteps = 2; // TODO allow changing this through the UI

        public override IReadOnlyDictionary<string, List<PortMetaData>> PortCustomData => new Dictionary<string, List<PortMetaData>>
        {
            {nameof(Stepper.Step), new List<PortMetaData>(OutputPortsMetadata())},
        };

        IEnumerable<PortMetaData> OutputPortsMetadata()
        {
            var defaultData = GetPortMetadata(nameof(Stepper.Step), m_Node);
            for (int i = 0; i < numSteps; i++)
            {
                defaultData.Name = $"Exec {i + 1}";
                yield return defaultData;
            }
        }
    }
}
