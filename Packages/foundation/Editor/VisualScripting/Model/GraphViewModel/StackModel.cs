using System;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    [Serializable]
    public class StackModel : StackBaseModel, IHasMainInputPort
    {
        public IPortModel InputPort { get; private set; }

        protected override void OnDefineNode()
        {
            InputPort = AddExecutionInputPort(null);
            AddExecutionOutputPort(null);
        }
    }
}