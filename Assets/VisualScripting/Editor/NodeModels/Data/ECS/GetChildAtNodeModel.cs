using System;
using Modifier.DotsStencil;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.NodeModels
{
    [DotsSearcherItem("GameObjects/" + k_Title), Serializable]
    class GetChildAtNodeModel : DotsNodeModel<GetChildAt>, IHasMainInputPort, IHasMainOutputPort
    {
        const string k_Title = "Get Child At";

        public override string Title => k_Title;
        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }
    }
}
