using System.Linq;
using Modifier.Runtime;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using UnityEngine.Assertions;
using State = UnityEditor.Modifier.VisualScripting.Editor.State;

namespace Modifier.DotsStencil
{
    public static class DragAndDropReducers
    {
        public static void Register(Store store)
        {
            store.Register<CreateObjectReferenceAction>(CreateObjectReference);
            store.Register<DotsCreateGetSetVariableNodesAction>(DotsCreateGetSetVariableNodes);
        }

        static State CreateObjectReference(State prevState, CreateObjectReferenceAction action)
        {
            var graph = action.GraphModel as VSGraphModel;
            if (action.Type == CreateObjectReferenceAction.ReferenceType.Subgraph)
            {
                DotsStencil.CreateSubGraphReference(graph, action.Objects.OfType<VSGraphAssetModel>(),
                    action.GraphSpacePosition);
            }
            else
            {
                if (prevState.EditorDataModel.BoundObject == null)
                {
                    Debug.LogError(
                        "Cannot create object references when a graph is opened in asset mode. Select a game object referencing this graph to do that.");
                    return prevState;
                }

                var authoringComponent = (prevState.EditorDataModel.BoundObject as GameObject)
                    ?.GetComponent<ScriptingGraphAuthoring>();
                Assert.IsNotNull(authoringComponent,
                    "The currently bound object has no ScriptingGraphAuthoring component. This is impossible.");
                DotsStencil.CreateVariablesFromGameObjects(graph, authoringComponent,
                    action.Objects.OfType<GameObject>(), action.GraphSpacePosition,
                    action.Type == CreateObjectReferenceAction.ReferenceType.ObjectGraph);
            }

            prevState.MarkForUpdate(UpdateFlags.GraphTopology);
            return prevState;
        }

        static State DotsCreateGetSetVariableNodes(State prevState, DotsCreateGetSetVariableNodesAction action)
        {
            VSGraphModel vsGraphModel = ((VSGraphModel)prevState.CurrentGraphModel);
            foreach (var tuple in action.VariablesToCreate)
            {
                vsGraphModel.CreateNode<SetVariableNodeModel>(tuple.Item1.Title, tuple.Item2, SpawnFlags.Default, v =>
                {
                    v.DeclarationModel = tuple.Item1;
                    v.IsGetter = action.CreateGetters;
                });
            }
            return prevState;
        }
    }
}
