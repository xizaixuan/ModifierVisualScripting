using System;
using Modifier.DotsStencil;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace NodeModels
{
    [DotsSearcherItem("GameObjects/" + k_Title), Serializable]
    class GetChildrenCountNodeModel : DotsNodeModel<GetChildrenCount>, IHasMainInputPort, IHasMainOutputPort
    {
        const string k_Title = "Get Children Count";

        public override string Title => k_Title;
        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }
    }
}
