using JetBrains.Annotations;
using System;

namespace Modifier.Runtime
{
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public class NodeDescriptionAttribute : Attribute
    {
        public string Description { get; }
        public object Type { get; }

        public string Example { get; set; }
        public string DataSetup { get; set; }

        public NodeDescriptionAttribute(string description)
        {
            Description = description;
        }

        public NodeDescriptionAttribute(object type, string description)
        {
            Type = type;
            Description = description;
        }
    }
}
