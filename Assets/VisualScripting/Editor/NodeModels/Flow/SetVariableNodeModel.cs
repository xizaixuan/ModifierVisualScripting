using System;
using System.Collections.Generic;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;
using ValueType = Modifier.Runtime.ValueType;

namespace Modifier.DotsStencil
{
    [Serializable, DotsSearcherItem("Set Var")]
    class SetVariableNodeModel : VariableNodeModel, IDotsNodeModel
    {
        [SerializeField]
        bool m_IsGetter = true;
        PortMapper m_PortToOffsetMapping;
        SetVariable m_Node;
        public Type NodeType => typeof(SetVariable);

        public INode Node => m_Node;

        public PortMapper PortToOffsetMapping => m_PortToOffsetMapping;

        public bool IsGetter
        {
            get => m_IsGetter;
            set => m_IsGetter = value;
        }

        public override void UpdateTypeFromDeclaration()
        {
            base.UpdateTypeFromDeclaration();
            m_Node.VariableType = DeclarationModel.DataType.ToValueTypeOrUnknown();
            m_Node.VariableKind = DeclarationModel.IsGraphVariable()
                ? VariableKind.GraphVariable
                : VariableKind.OutputData;
        }

        protected override void OnDefineNode()
        {
            if (m_PortToOffsetMapping == null)
                m_PortToOffsetMapping = new PortMapper();
            else
                m_PortToOffsetMapping.Clear();
            var triggerSet = AddExecutionInputPort("", "Set");
            var triggerDone = AddExecutionOutputPort("", "Done");
            var dataSet = AddDataInputPort("", DeclarationModel?.DataType ?? TypeHandle.Unknown, "setvalue");

            DotsTranslator.MapPort(m_PortToOffsetMapping, triggerSet, ref m_Node.Input.Port, m_Node);
            DotsTranslator.MapPort(m_PortToOffsetMapping, triggerDone, ref m_Node.Output.Port, m_Node);
            DotsTranslator.MapPort(m_PortToOffsetMapping, dataSet, ref m_Node.Value.Port, m_Node);
            if (m_IsGetter)
            {
                var dataGet = m_MainPortModel = AddDataOutputPort("Value", DeclarationModel?.DataType ?? TypeHandle.Unknown, "getvalue");
                DotsTranslator.MapPort(m_PortToOffsetMapping, dataGet, ref m_Node.OutValue.Port, m_Node);
            }

            m_Node.VariableType = DeclarationModel?.DataType.ToValueTypeOrUnknown() ?? ValueType.Unknown;
        }
    }
}
