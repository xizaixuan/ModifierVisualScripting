using JetBrains.Annotations;
using System;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.VisualScripting.GraphViewModel;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    [Serializable]
    public class KeyDownEventModel : FunctionModel, IEventFunctionModel
    {
        public override bool IsInstanceMethod => true;

        [PublicAPI]
        public enum EventMode { Held, Pressed, Released }

        const string k_Title = "On Key Event";

        public override string Title => k_Title;
        public override bool AllowMultipleInstances => true;
        public override bool HasReturnType => false;

        public EventMode mode;

        public IPortModel KeyPort { get; private set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            KeyPort = AddDataInputPort<KeyCode>("key");
        }
    }
}