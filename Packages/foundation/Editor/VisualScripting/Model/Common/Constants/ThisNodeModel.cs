using System;
using Unity.Modifier.GraphElements;
using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    [Serializable]
    public class ThisNodeModel : NodeModel, IVariableModel
    {
        public IPortModel OutputPort { get; private set; }
        public IVariableDeclarationModel DeclarationModel => null;

        const string k_Title = "This";

        public override string Title => k_Title;

        public override string DataTypeString => VSGraphModel?.FriendlyScriptName ?? string.Empty;
        public override string VariableString => "Variable";

        protected override void OnDefineNode()
        {
            OutputPort = AddDataOutputPort(null, TypeHandle.ThisType);
        }

        public IGTFPortModel GTFInputPort => OutputPort.Direction == Direction.Input ? OutputPort as IGTFPortModel : null;
        public IGTFPortModel GTFOutputPort => OutputPort.Direction == Direction.Output ? OutputPort as IGTFPortModel : null;
    }
}