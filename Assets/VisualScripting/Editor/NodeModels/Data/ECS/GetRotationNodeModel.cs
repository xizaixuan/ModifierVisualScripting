using System;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [Serializable, DotsSearcherItem("GameObjects/" + k_Title)]
    class GetRotationNodeModel : DotsNodeModel<GetRotation>, IHasMainInputPort
    {
        const string k_Title = "Get Rotation";

        public override string Title => k_Title;

        public IPortModel InputPort { get; set; }
    }
}
