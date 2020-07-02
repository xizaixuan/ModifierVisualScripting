using System;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [Serializable, DotsSearcherItem("Math/Combine Quaternion Rotations")]
    class MathQuaternionNodeModel : DotsNodeModel<CombineQuaternionRotations>, IHasMainOutputPort
    {
        public override string Title => "Combine Quaternion Rotations";
        public IPortModel OutputPort { get; set; }
    }
}
