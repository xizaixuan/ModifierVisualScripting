using System;
using Modifier.DotsStencil;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.NodeModels
{
    [DotsSearcherItem("GameObjects/Instantiate"), Serializable]
    class InstantiateNodeModel : DotsNodeModel<Instantiate>, IHasMainExecutionInputPort, IHasMainExecutionOutputPort,
        IHasMainInputPort, IHasMainOutputPort
    {
        public IPortModel ExecutionInputPort { get; set; }
        public IPortModel ExecutionOutputPort { get; set; }
        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }
    }
}
