using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    [Serializable]
    public class ForEachNodeModel : LoopNodeModel, IHasMainInputPort
    {
        public override bool IsInsertLoop => true;
        public override LoopConnectionType LoopConnectionType => LoopConnectionType.LoopStack;

        public override string InsertLoopNodeTitle => "For Each Loop";
        public override Type MatchingStackType => typeof(ForEachHeaderModel);

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            var typeHandle = typeof(IEnumerable<object>).GenerateTypeHandle(Stencil);
            InputPort = AddDataInputPort(ForEachHeaderModel.DefaultCollectionName, typeHandle);
        }

        public override void OnConnection(IPortModel selfConnectedPortModel, IPortModel otherConnectedPortModel)
        {
            if (selfConnectedPortModel != null)
            {
                var output = selfConnectedPortModel.Direction == Direction.Input
                    ? OutputPort.ConnectionPortModels.FirstOrDefault()?.NodeModel
                    : otherConnectedPortModel?.NodeModel;
                if (output is ForEachHeaderModel foreachStack)
                {
                    var input = selfConnectedPortModel.Direction == Direction.Input
                        ? otherConnectedPortModel
                        : InputPort.ConnectionPortModels.FirstOrDefault();
                    foreachStack.CreateLoopVariables(input);

                    ((VSGraphModel)GraphModel).LastChanges.ChangedElements.Add(foreachStack);
                }
            }
        }
    }
}
