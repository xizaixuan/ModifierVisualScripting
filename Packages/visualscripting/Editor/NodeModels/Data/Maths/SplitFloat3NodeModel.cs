using System;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [DotsSearcherItem("Math/" + k_Title), Serializable]
    class SplitFloat3NodeModel : DotsNodeModel<SplitFloat3>, IHasMainInputPort, IHasMainOutputPort
    {
        const string k_Title = "Split Vector 3";

        public override string Title => k_Title;

        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }
    }
}
