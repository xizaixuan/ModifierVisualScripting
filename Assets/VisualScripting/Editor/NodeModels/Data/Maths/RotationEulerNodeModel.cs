using System;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [Serializable, DotsSearcherItem("Math/" + k_Title)]
    class RotationEulerNodeModel : DotsNodeModel<RotationEuler>, IHasMainInputPort, IHasMainOutputPort
    {
        const string k_Title = "Rotation Euler";

        public override string Title => k_Title;
        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }
    }
}
