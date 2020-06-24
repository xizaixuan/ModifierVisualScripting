using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modifier.Runtime;
using Modifier.Runtime.Nodes;
using UnityEditor.Searcher;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using ValueType = Modifier.Runtime.ValueType;

namespace Modifier.DotsStencil
{
    internal abstract class NodeDocumentationFormatter
    {
        protected abstract void Paragraph(string paragraph);
        protected abstract void SectionTitle(string title, int titleImportance);
        protected abstract void PortDescription(string portName, string type, string defaultValue, string attrDescription);
        protected abstract void PortsHeader(string sectionName);

        public void DocumentNode(SearcherItem searcherItem, INodeModel nodeModel)
        {
            INode node;
            switch (nodeModel)
            {
                case IDotsNodeModel baseDotsNodeModel:
                    node = baseDotsNodeModel.Node;
                    break;
                case StringConstantModel _:
                    node = new ConstantString();
                    break;
                case BooleanConstantNodeModel _:
                    node = new ConstantBool();
                    break;
                case IntConstantModel _:
                    node = new ConstantInt();
                    break;
                case FloatConstantModel _:
                    node = new ConstantFloat();
                    break;
                case Vector2ConstantModel _:
                    node = new ConstantFloat2();
                    break;
                case Vector3ConstantModel _:
                    node = new ConstantFloat3();
                    break;
                case Vector4ConstantModel _:
                    node = new ConstantFloat4();
                    break;
                case QuaternionConstantModel _:
                    node = new ConstantQuaternion();
                    break;
                default:
                    return;
            }
            var nodeType = node.GetType();
            var executionType = GetNodeExecutionType(nodeType, node);
            var title = Attribute.IsDefined(node.GetType(), typeof(WorkInProgressAttribute))
                ? $"{searcherItem.Name} [WIP]"
                : searcherItem.Name;

            SectionTitle(title, 1);

            var nodeDescription = GetNodeDescription(nodeType, executionType, out string exampleText, out string dataSetup);
            if (!String.IsNullOrEmpty(nodeDescription))
                Paragraph(nodeDescription);

            GetPortsDescription(executionType, node);

            if (exampleText != null)
            {
                SectionTitle("Examples", 2);
                Paragraph(exampleText);
            }

            if (dataSetup != null)
            {
                SectionTitle("Data Setup", 2);
                Paragraph(dataSetup);
            }
        }

        public static object GetNodeExecutionType(Type nodeType, INode node)
        {
            object executionType = null;
            if (nodeType.GetInterfaces().Any(x =>
                x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHasExecutionType<>)))
            {
                var getType = nodeType.GetProperty("Type")?.GetMethod;
                executionType = getType?.Invoke(node, new object[] {});
            }

            return executionType;
        }

        /// <summary>
        /// Generates ports' description in the following format: "Name [Type]: Description (Default value)"
        /// </summary>
        /// <param name="executionType"></param>
        /// <param name="node"></param>
        /// <returns>The description of all the ports in the node</returns>
        private void GetPortsDescription(object executionType, INode node)
        {
            var runtimeNode = node;
            var nodeType = runtimeNode.GetType();
            var fields = nodeType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var inputs = fields.Where(f => typeof(IInputDataPort).IsAssignableFrom(f.FieldType)
                || typeof(IInputTriggerPort).IsAssignableFrom(f.FieldType)).ToList();
            var outputs = fields.Where(f => typeof(IOutputDataPort).IsAssignableFrom(f.FieldType)
                || typeof(IOutputTriggerPort).IsAssignableFrom(f.FieldType)).ToList();

            SectionTitle("Ports", 2);

            AppendSection("Inputs", inputs, this, executionType);
            AppendSection("Outputs", outputs, this, executionType);
        }

        private static string GetNodeDescription(Type runtimeNodeType, object executionType, out string exampleText,
            out string dataSetup)
        {
            NodeDescriptionAttribute nodeDescription;
            if (executionType != null)
            {
                var attrs = runtimeNodeType.GetCustomAttributes<NodeDescriptionAttribute>();
                nodeDescription = attrs.FirstOrDefault(a => a.Type != null && a.Type.Equals(executionType));
            }
            else
            {
                nodeDescription = runtimeNodeType.GetCustomAttribute<NodeDescriptionAttribute>();
            }

            if (nodeDescription != null)
            {
                exampleText = nodeDescription.Example;
                dataSetup = nodeDescription.DataSetup;
                return nodeDescription.Description;
            }

            exampleText = null;
            dataSetup = null;
            return null;
        }

        private static void AppendSection(string sectionName, IReadOnlyCollection<FieldInfo> infos,
            NodeDocumentationFormatter formatter, object executionType)
        {
            if (infos.Any())
            {
                formatter.PortsHeader(sectionName);

                foreach (var info in infos)
                {
                    var attr = GetMatchingPortDescription(executionType, info);
                    var portName = String.IsNullOrEmpty(attr?.Name) ? info.Name : attr.Name;
                    var type = typeof(ITriggerPort).IsAssignableFrom(info.FieldType) ? "Trigger" : attr?.Type != ValueType.Unknown ? $"[{attr?.Type.FriendlyName()}]" : String.Empty;
                    var defaultValue = attr?.DefaultValue?.ToString();
                    formatter.PortDescription(portName, type, defaultValue, attr?.Description);
                }
            }
        }

        public static PortDescriptionAttribute GetMatchingPortDescription(object executionType, FieldInfo info)
        {
            var attrs = info.GetCustomAttributes<PortDescriptionAttribute>();
            var attr = attrs.FirstOrDefault(a => executionType == null || executionType.Equals(a.ExecutionType));
            if (executionType != null && attr == null) // fallback on a non specialized port description
                attr = attrs.FirstOrDefault(a => a.ExecutionType == null);
            return attr;
        }
    }
}
