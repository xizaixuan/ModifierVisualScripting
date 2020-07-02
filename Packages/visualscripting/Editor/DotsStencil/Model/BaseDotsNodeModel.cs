using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;
using ValueType = Modifier.Runtime.ValueType;

namespace Modifier.DotsStencil
{
    public abstract class BaseDotsNodeModel : NodeModel
    {
        [SerializeReference]
        protected INode m_Node;
        public INode Node
        {
            get => m_Node;
            set => m_Node = value;
        }

        public struct PortCountProperties
        {
            public string Name;
            public int Min;
            public int Max;
        }

        static Dictionary<string, List<PortMetaData>> s_NoCustomData = new Dictionary<string, List<PortMetaData>>();
        static Dictionary<string, PortCountProperties> s_NoCustomPortCount = new Dictionary<string, PortCountProperties>();

        public virtual IReadOnlyDictionary<string, List<PortMetaData>> PortCustomData => s_NoCustomData;
        public virtual IReadOnlyDictionary<string, PortCountProperties> PortCountData => s_NoCustomPortCount;

        public struct PortMetaData
        {
            public string Name;
            public ValueType Type;
            public object DefaultValue;
            public PortModel.PortModelOptions PortModelOptions;

            /// <summary>
            /// Get PortMetadata for a each field in a type that can be matched to a ValueType
            /// </summary>
            /// <param name="componentType">type to parse fields from</param>
            /// <param name="stencil">current stencil</param>
            /// <returns>Port Metadata corresponding to each valid field found in the type</returns>
            internal static IEnumerable<PortMetaData> FromValidTypeFields(Type componentType, Stencil stencil)
            {
                return componentType == null ? Enumerable.Empty<PortMetaData>() : componentType.GetFields()
                    .Select(f => new PortMetaData { Name = f.Name, Type = f.FieldType.GenerateTypeHandle(stencil).ToValueType(out var t) ? t : ValueType.Unknown });
            }
        }

        internal static IEnumerable<FieldInfo> GetNodePorts(Type t)
        {
            return t.GetFields().Where(f => typeof(IPort).IsAssignableFrom(f.FieldType));
        }

        public static PortMetaData GetPortMetadata(INode node, FieldInfo portField)
        {
            PortMetaData data = new PortMetaData()
            {
                Name = portField.Name,
                Type = ValueType.Unknown,
                DefaultValue = default
            };

            var executionType = NodeDocumentationFormatter.GetNodeExecutionType(node.GetType(), node);
            var attr = NodeDocumentationFormatter.GetMatchingPortDescription(executionType, portField);

            if (attr != null)
            {
                data.Type = attr.Type;
                if (attr.Name != null)
                    data.Name = attr.Name;
                data.DefaultValue = attr.DefaultValue;
            }

            return data;
        }
    }
}
