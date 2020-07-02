using System;
using Modifier.DotsStencil;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;

namespace Modifier.NodeModels
{
    [Serializable, EnumNodeSearcher(typeof(InterpolationType), "Math", "{0} Interpolation")]
    class InterpolateNodeModel : DotsNodeModel<Interpolate>, IHasMainInputPort, IHasMainOutputPort
    {
        public override string Title => $"{TypedNode.Type.ToString().Nicify()} Interpolation";
        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }
    }
}
