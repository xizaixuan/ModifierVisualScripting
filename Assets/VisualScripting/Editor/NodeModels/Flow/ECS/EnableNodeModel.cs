using System;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [Serializable, DotsSearcherItem("GameObjects/Enable")]
    class EnableNodeModel : DotsNodeModel<Enable>, IHasMainExecutionInputPort, IHasMainInputPort
    {
        public IPortModel ExecutionInputPort { get; set; }
        public IPortModel InputPort { get; set; }
    }
}
