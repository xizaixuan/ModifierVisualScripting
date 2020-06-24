using System;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [DotsSearcherItem(k_Title), Serializable]
    class MathUnaryNodeBool : DotsNodeModel<MathUnaryNotBool>, IHasMainInputPort, IHasMainOutputPort
    {
        const string k_Title = "Math/Not";

        public override string Title => k_Title;

        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }
    }
}
