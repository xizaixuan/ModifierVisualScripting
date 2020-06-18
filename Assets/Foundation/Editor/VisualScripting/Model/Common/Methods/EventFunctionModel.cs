using JetBrains.Annotations;
using System;
using UnityEditor.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    [PublicAPI]
    [Serializable]
    public abstract class EventFunctionModel : FunctionModel, IEventFunctionModel
    {
        public override bool IsInstanceMethod => true;
        public override bool HasReturnType => false;

        public override bool AllowMultipleInstances => false;

        public override string IconTypeString => "typeEventFunction";
    }
}