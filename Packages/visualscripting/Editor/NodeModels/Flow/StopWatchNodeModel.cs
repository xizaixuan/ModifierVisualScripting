using System;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [Serializable, DotsSearcherItem("Flow/" + k_Title)]
    class StopWatchNodeModel : DotsNodeModel<StopWatch>, IHasMainExecutionInputPort, IHasMainExecutionOutputPort, IHasMainInputPort, IHasMainOutputPort
    {
        const string k_Title = "Stop Watch";

        public override string Title => k_Title;

        public IPortModel ExecutionInputPort { get; set; }
        public IPortModel ExecutionOutputPort { get; set; }
        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }
    }
}
