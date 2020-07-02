using System;
using Unity.Entities;

namespace UnityEditor.Modifier.VisualScripting.Runtime
{
    [Flags]
    public enum ComponentOptions
    {
        AnyComponent = 0,
        OnlyAuthoringComponents = 1,
    }

    public class ComponentSearcherAttribute : TypeSearcherAttribute
    {
        public ComponentOptions ComponentOptions { get; }

        public ComponentSearcherAttribute(ComponentOptions options = ComponentOptions.OnlyAuthoringComponents)
            : base(typeof(IComponentData))
        {
            ComponentOptions = options;
        }
    }
}
