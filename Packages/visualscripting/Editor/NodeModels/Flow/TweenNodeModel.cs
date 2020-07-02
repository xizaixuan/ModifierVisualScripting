using System;
using System.Collections.Generic;
using System.Reflection;
using Modifier.DotsStencil;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using ValueType = Modifier.Runtime.ValueType;

namespace Modifier.NodeModels
{
    [Serializable, EnumNodeSearcher(typeof(InterpolationType), "Flow", "{0} Tween")]
    class TweenNodeModel : DotsNodeModel<Tween>, IHasMainInputPort, IHasMainOutputPort,
        IHasMainExecutionInputPort, IHasMainExecutionOutputPort
    {
        Dictionary<string, List<PortMetaData>> s_MetaData;
        public override string Title => $"{TypedNode.Type.ToString().Nicify()} Tween";
        public IPortModel InputPort { get; set; }
        public IPortModel OutputPort { get; set; }
        public IPortModel ExecutionInputPort { get; set; }
        public IPortModel ExecutionOutputPort { get; set; }

        public override IReadOnlyDictionary<string, List<PortMetaData>> PortCustomData => GetMetadata();

        IReadOnlyDictionary<string, List<PortMetaData>> GetMetadata()
        {
            if (s_MetaData == null)
            {
                s_MetaData = new Dictionary<string, List<PortMetaData>>();
            }

            var type = GetValueType(TypedNode.TweenValueType);
            Make(nameof(Tween.From));
            Make(nameof(Tween.To));
            Make(nameof(Tween.Result));
            return s_MetaData;

            void Make(string name)
            {
                PortMetaData portMetaData = GetPortMetadata(name, Node);
                portMetaData.Type = type;
                s_MetaData[name] = new List<PortMetaData> {portMetaData};
            }

            ValueType GetValueType(Tween.ETweenValueType typedNodeTweenValueType)
            {
                switch (typedNodeTweenValueType)
                {
                    case Tween.ETweenValueType.Float:
                        return ValueType.Float;
                    case Tween.ETweenValueType.Vector2:
                        return ValueType.Float2;
                    case Tween.ETweenValueType.Vector3:
                        return ValueType.Float3;
                    case Tween.ETweenValueType.Vector4:
                        return ValueType.Float4;
                    case Tween.ETweenValueType.Int:
                        return ValueType.Int;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(typedNodeTweenValueType), typedNodeTweenValueType,
                            null);
                }
            }
        }
    }
}
