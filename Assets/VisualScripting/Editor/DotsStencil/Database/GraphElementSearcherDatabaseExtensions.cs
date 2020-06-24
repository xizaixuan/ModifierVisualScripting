using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modifier.Runtime;
using Modifier.Runtime.Nodes;
using UnityEditor;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine.Assertions;
using UnityEngine.Modifier.VisualScripting;

namespace Modifier.DotsStencil
{
    static class GraphElementSearcherDatabaseExtensions
    {
        internal static GraphElementSearcherDatabase AddDotsEvents(this GraphElementSearcherDatabase self)
        {
            var eventTypes = TypeCache.GetTypesDerivedFrom<IVisualScriptingEvent>()
                .Where(t => !Attribute.IsDefined(t, typeof(HiddenAttribute)));
            var sendEventNodeType = typeof(SendEventNodeModel);
            var onEventNodeType = typeof(OnEventNodeModel);

            foreach (var eventType in eventTypes)
            {
                var typeHandle = eventType.GenerateTypeHandle(self.Stencil);

                self.Items.AddAtPath(new GraphNodeModelSearcherItem(
                    new NodeSearcherItemData(sendEventNodeType),
                    data => data.CreateNode(
                        sendEventNodeType,
                        preDefineSetup: n => ((IEventNodeModel)n).TypeHandle = typeHandle),
                    $"Send {eventType.FriendlyName().Nicify()}"),
                    "Events");

                self.Items.AddAtPath(new GraphNodeModelSearcherItem(
                    new NodeSearcherItemData(onEventNodeType),
                    data => data.CreateNode(
                        onEventNodeType,
                        preDefineSetup: n => ((IEventNodeModel)n).TypeHandle = typeHandle),
                    $"On {eventType.FriendlyName().Nicify()}"),
                    "Events");
            }

            return self;
        }

        internal static GraphElementSearcherDatabase AddDotsConstants(this GraphElementSearcherDatabase self)
        {
            var constants = new Dictionary<string, Type>
            {
                { "Boolean Constant", typeof(BooleanConstantNodeModel) },
                { "Integer Constant", typeof(IntConstantModel) },
                { "Float Constant", typeof(FloatConstantModel) },
                { "Vector 2 Constant", typeof(Vector2ConstantModel) },
                { "Vector 3 Constant", typeof(Vector3ConstantModel) },
                { "Vector 4 Constant", typeof(Vector4ConstantModel) },
                { "Quaternion Constant", typeof(QuaternionConstantModel) },
                { "String Constant", typeof(StringConstantModel) },
            };

            foreach (var constant in constants)
            {
                self.Items.AddAtPath(new GraphNodeModelSearcherItem(
                    new NodeSearcherItemData(constant.Value),
                    data => data.CreateNode(constant.Value),
                    constant.Key),
                    "Constants");
            }

            return self;
        }

        // TODO temp while developing. Will not be created from searcher in the long run.
        internal static GraphElementSearcherDatabase AddEdgePortals(this GraphElementSearcherDatabase self)
        {
            var portals = new List<(string name, Type type)>
            {
                ("Data Portal", typeof(DataEdgePortalEntryModel)),
                ("Trigger Portal", typeof(ExecutionEdgePortalEntryModel)),
                ("Data Portal", typeof(DataEdgePortalExitModel)),
                ("Trigger Portal", typeof(ExecutionEdgePortalExitModel))
            };

            foreach (var portal in portals)
            {
                self.Items.AddAtPath(
                    new GraphNodeModelSearcherItem(
                        new NodeSearcherItemData(portal.type),
                        data =>
                        {
                            var p = (EdgePortalModel)data.CreateNode(portal.type);
                            p.DeclarationModel = ((VSGraphModel)data.GraphModel).CreateGraphPortalDeclaration(portal.name);
                            ((GraphModel)data.GraphModel).CreateOppositePortal(p, data.SpawnFlags);
                            return p;
                        },
                        (typeof(IEdgePortalEntryModel).IsAssignableFrom(portal.type) ? "Entry " : "Exit ") + portal.name),
                    "Portals");
            }

            return self;
        }

        internal static GraphElementSearcherDatabase AddNodesWithSearcherItemCollectionAttribute(
            this GraphElementSearcherDatabase self)
        {
            var types = TypeCache.GetTypesWithAttribute<DotsSearcherItemCollectionAttribute>();
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<DotsSearcherItemCollectionAttribute>();
                Assert.IsTrue(typeof(BaseDotsNodeModel).IsAssignableFrom(type));
                foreach (var objectData in attribute.ObjectData)
                {
                    var name = string.IsNullOrEmpty(attribute.NameFormat)
                        ? objectData.SearcherTitle
                        : string.Format(attribute.NameFormat, objectData.SearcherTitle);
                    var path = string.IsNullOrEmpty(objectData.Path) ? attribute.Path : attribute.Path + "/" + objectData.Path;

                    self.Items.AddAtPath(new GraphNodeModelSearcherItem(
                        new NodeSearcherItemData(type),
                        data => data.CreateNode(
                            type,
                            preDefineSetup: n =>
                            {
                                var baseDotsNodeModel = (BaseDotsNodeModel)n;
                                var runtimeNode = baseDotsNodeModel.Node;

                                if (runtimeNode == null || !runtimeNode.GetType().GetInterfaces().Any(x =>
                                    x.IsGenericType &&
                                    x.GetGenericTypeDefinition() == typeof(IHasExecutionType<>)))
                                    return;

                                var setNode = runtimeNode.GetType().GetProperty("Type")?.SetMethod;
                                setNode?.Invoke(runtimeNode, new[] { objectData.Value });

                                baseDotsNodeModel.Node = runtimeNode;
                            }),
                        name),
                        path);
                }
            }

            return self;
        }
    }
}