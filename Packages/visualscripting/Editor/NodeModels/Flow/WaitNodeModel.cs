using System;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [Serializable, DotsSearcherItem("Flow/Wait")]
    class WaitNodeModel : DotsNodeModel<Wait>, IHasMainExecutionInputPort, IHasMainExecutionOutputPort,
        IHasMainInputPort
    {
        public IPortModel ExecutionInputPort { get; set; }
        public IPortModel ExecutionOutputPort { get; set; }
        public IPortModel InputPort { get; set; }
    }
}
