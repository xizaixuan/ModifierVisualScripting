using System;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [Serializable, DotsSearcherItem("GameObjects/" + k_Title)]
    class HasComponentNodeModel : DotsNodeModel<HasComponent>, IHasMainInputPort, IHasMainOutputPort
    {
        const string k_Title = "Has Component";

        public override string Title => k_Title;

        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }
    }
}
