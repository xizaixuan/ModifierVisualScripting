using System;
using System.Collections.Generic;
using System.Linq;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;

namespace Modifier.DotsStencil
{
    [Serializable, EnumNodeSearcher(typeof(MathBinaryBool.BinaryBoolType), "Math")]
    class MathBinaryBoolNodeModel : DotsNodeModel<MathBinaryBool>, IHasMainInputPort, IHasMainOutputPort
    {
        public override string Title => TypedNode.Type.ToString().Nicify();
        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }

        [HackContextualMenuVariableCount("Input", min: 2)]
        public int numCases = 2; // TODO allow changing this through the UI

        public override IReadOnlyDictionary<string, List<PortMetaData>> PortCustomData => new Dictionary<string, List<PortMetaData>>
        {
            {nameof(MathBinaryBool.Inputs), Enumerable.Repeat(GetPortMetadata(nameof(MathBinaryBool.Inputs), m_Node), numCases).ToList()},
        };
    }
}
