using System;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [Serializable, DotsSearcherItem("GameObjects/" + k_Title)]
    class GetPositionNodeModel : DotsNodeModel<GetPosition>, IHasMainInputPort
    {
        const string k_Title = "Get Position";

        public override string Title => k_Title;

        public IPortModel InputPort { get; set; }
    }
}
