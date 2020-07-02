using System;
using System.Collections.Generic;
using System.Linq;
using Modifier.DotsStencil;
using Modifier.Runtime;
using Modifier.Runtime.Mathematics;
using Unity.Mathematics;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using ValueType = Modifier.Runtime.ValueType;

namespace Modifier.NodeModels
{
    [Serializable, GeneratedMathSearcherAttribute("Math")]
    class MathNodeModel : DotsNodeModel<MathGenericNode>, IHasMainOutputPort
    {
        public override string Title => m_MethodName;

        [HackContextualMenuVariableCount("Input")]
        public int InputCount;

        public IPortModel OutputPort { get; set; }

        string m_MethodName;
        MathOperationsMetaData.OpSignature m_CurrentMethod;
        MathOperationsMetaData.OpSignature[] m_CompatibleMethods;

        public override IReadOnlyDictionary<string, List<PortMetaData>> PortCustomData => new Dictionary<string, List<PortMetaData>>
        {
            {nameof(MathGenericNode.Inputs), new List<PortMetaData>(InputPortsMetadata())},
            {nameof(MathGenericNode.Result), new List<PortMetaData> { ResultPortsMetadata() } },
        };

        public override IReadOnlyDictionary<string, PortCountProperties> PortCountData => new Dictionary<string, PortCountProperties>
        {
            {nameof(InputCount), InputPortCountData()},
        };

        PortCountProperties InputPortCountData()
        {
            var portCount = TypedNode.Inputs.DataCount;
            var result = new PortCountProperties { Min = portCount, Max = portCount, Name = "Input" };
            var methodName = TypedNode.Function.GetMethodsSignature().OpType;
            if (MathOperationsMetaData.MethodNameSupportsMultipleInputs(methodName))
            {
                result.Max = -1;
                result.Min = 2;
            }
            return result;
        }

        PortMetaData ResultPortsMetadata()
        {
            var returnData = GetPortMetadata(nameof(MathGenericNode.Result), m_Node);
            returnData.Type = m_CurrentMethod.Return;
            return returnData;
        }

        IEnumerable<PortMetaData> InputPortsMetadata()
        {
            if (m_CurrentMethod.Params != null)
            {
                var defaultData = GetPortMetadata(nameof(MathGenericNode.Inputs), m_Node);
                for (int i = 0; i < InputCount; i++)
                {
                    defaultData.Name = "";
                    defaultData.Type = m_CurrentMethod.Params[math.min(i, m_CurrentMethod.Params.Length - 1)];
                    defaultData.DefaultValue = Activator.CreateInstance(defaultData.Type.ValueTypeToTypeHandle().Resolve(Stencil));
                    yield return defaultData;
                }
            }
        }

        protected override void OnDefineNode()
        {
            m_MethodName = TypedNode.Type.GetMethodsSignature().OpType;
            if (m_MethodName != null && !MathOperationsMetaData.MethodsByName.TryGetValue(m_MethodName, out m_CompatibleMethods))
                m_CompatibleMethods = new MathOperationsMetaData.OpSignature[0];

            if (m_CompatibleMethods != null)
                m_CurrentMethod = m_CompatibleMethods.SingleOrDefault(o => o.EnumName == TypedNode.Type.ToString());

            if (InputCount == default)
                InputCount = m_CurrentMethod.Params?.Length ?? 2;

            var mathGenericNode = TypedNode;
            mathGenericNode.GenerationVersion = MathGeneratedDelegates.GenerationVersion;
            Node = mathGenericNode;

            base.OnDefineNode();
        }

        public override void OnDisconnection(IPortModel selfConnectedPortModel, IPortModel otherConnectedPortModel)
        {
            base.OnDisconnection(selfConnectedPortModel, otherConnectedPortModel);
            OnConnection(selfConnectedPortModel, null);
        }

        public override void OnConnection(IPortModel selfConnectedPortModel, IPortModel otherConnectedPortModel)
        {
            var connectedInputTypes = this.InputsByDisplayOrder
                .Select(i =>
                {
                    if (i == selfConnectedPortModel) // not connected yet
                        return otherConnectedPortModel?.DataTypeHandle.ToValueTypeOrUnknown() ?? ValueType.Unknown;
                    if (i.IsConnected)
                        return i.ConnectionPortModels.First().DataTypeHandle.ToValueTypeOrUnknown();
                    return ValueType.Unknown;
                }).ToArray();
            var bestCandidate = MathOperationsMetaData.ScoreCompatibleMethodsAccordingToInputParameters(m_CompatibleMethods, connectedInputTypes)
                .FirstOrDefault();
            if (bestCandidate.Score > 0)
            {
                m_CurrentMethod = bestCandidate.Signature;
                var mathGenericNode = TypedNode;
                mathGenericNode.Type = (MathGeneratedFunction)Enum.Parse(typeof(MathGeneratedFunction), bestCandidate.Signature.EnumName);
                Node = mathGenericNode;
                DefineNode();
            }
            base.OnConnection(selfConnectedPortModel, otherConnectedPortModel);
        }
    }
}
