using System;
using Modifier.DotsStencil;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.NodeModels
{
    [DotsSearcherItem("GameObjects/" + k_Title), Serializable]
    class EnumerateChildrenNodeModel : DotsNodeModel<EnumerateChildren>, IHasMainExecutionInputPort,
        IHasMainExecutionOutputPort, IHasMainInputPort, IHasMainOutputPort
    {
        const string k_Title = "Enumerate Children";

        public override string Title => k_Title;
        public IPortModel ExecutionInputPort { get; set; }
        public IPortModel ExecutionOutputPort { get; set; }
        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }
    }
}
