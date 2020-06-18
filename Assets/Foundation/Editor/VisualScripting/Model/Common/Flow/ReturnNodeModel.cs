using System;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    [SearcherItem(typeof(ClassStencil), SearcherContext.Stack, "Control Flow/Return")]
    [BranchedNode]
    [Serializable]
    public class ReturnNodeModel : NodeModel, IHasMainInputPort
    {
        const string k_Title = "Return";

        public override string Title => k_Title;

        PortModel m_InputPort;
        public IPortModel InputPort => m_InputPort;

        protected override void OnDefineNode()
        {
            var returnType = ParentStackModel?.OwningFunctionModel?.ReturnType;
            m_InputPort = returnType != null && returnType.Value.IsValid && returnType != TypeHandle.Void
                ? AddDataInputPort("value", returnType.Value)
                : null;
        }
    }
}