using System;
using System.Linq;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    [Serializable]
    public class UnaryOperatorNodeModel : NodeModel, IOperationValidator
    {
        public UnaryOperatorKind Kind;

        public override string Title => Kind.ToString();
        public IPortModel InputPort { get; private set; }
        public IPortModel OutputPort { get; private set; }

        protected override void OnDefineNode()
        {
            var portType = Kind == UnaryOperatorKind.LogicalNot ? TypeHandle.Bool : TypeHandle.Float;
            InputPort = AddDataInputPort("A", portType);

            if (Kind == UnaryOperatorKind.LogicalNot || Kind == UnaryOperatorKind.Minus)
                OutputPort = AddDataOutputPort("Out", portType);
        }

        public virtual bool HasValidOperationForInput(IPortModel _, TypeHandle typeHandle)
        {
            var type = typeHandle.Resolve(Stencil);
            return TypeSystem.GetOverloadedUnaryOperators(type).Contains(Kind);
        }
    }
}