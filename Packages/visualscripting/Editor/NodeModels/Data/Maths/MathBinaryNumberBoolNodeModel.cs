using System;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;

namespace Modifier.DotsStencil
{
    [Serializable, EnumNodeSearcher(typeof(MathBinaryNumberBool.BinaryNumberType), "Math")]
    class MathBinaryNumberBoolNodeModel : DotsNodeModel<MathBinaryNumberBool>, IHasMainInputPort, IHasMainOutputPort
    {
        public override string Title => TypedNode.Type.ToString().Nicify();
        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }
    }
}
