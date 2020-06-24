using System;
using Modifier.DotsStencil;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace NodeModels
{
    [DotsSearcherItem("GameObjects/" + k_Title), Serializable]
    class GetParentNodeModel : DotsNodeModel<GetParent>, IHasMainInputPort, IHasMainOutputPort
    {
        const string k_Title = "Get Parent";

        public override string Title => k_Title;
        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }
    }
}
