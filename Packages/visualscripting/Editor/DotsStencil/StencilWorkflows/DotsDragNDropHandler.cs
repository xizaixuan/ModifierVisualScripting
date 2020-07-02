using System.Linq;
using Modifier.Runtime;
using UnityEditor;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using UnityEngine.UIElements;

namespace Modifier.DotsStencil
{
    public class DotsDragNDropHandler : IExternalDragNDropHandler
    {
        public void HandleDragUpdated(DragUpdatedEvent e, DragNDropContext ctx)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
        }

        public void HandleDragPerform(DragPerformEvent e, Store store, DragNDropContext ctx, VisualElement element)
        {
            var state = store.GetState();
            if (state?.CurrentGraphModel == null)
                return;
            DotsStencil stencil = (DotsStencil)state.CurrentGraphModel.Stencil;

            if (stencil.Type == DotsStencil.GraphType.Subgraph)
            {
                Debug.LogError("Cannot create object references in a subgraph. Create a Data Input of type Game Object instead and feed it from an object graph in the scene.");
                return;
            }

            if (!(state?.EditorDataModel?.BoundObject as UnityEngine.Object))
            {
                Debug.LogError("Cannot create object references when a graph is opened in asset mode. Select a game object referencing this graph to do that.");
                return;
            }

            Vector2 graphSpacePosition = element.WorldToLocal(e.mousePosition);

            if (DragAndDrop.objectReferences.OfType<VSGraphAssetModel>().Any())
            {
                if (!DragAndDrop.objectReferences.All(x => x is VSGraphAssetModel assetModel && assetModel.GraphModel?.Stencil is DotsStencil dotsStencil && dotsStencil.Type == DotsStencil.GraphType.Subgraph))
                {
                    Debug.LogError("Object graph references must be created from their Game Object in the scene hierarchy. Only subgraphs can be referenced using their asset");
                    return;
                }
                store.Dispatch(new CreateObjectReferenceAction(graphSpacePosition, state?.CurrentGraphModel, DragAndDrop.objectReferences, CreateObjectReferenceAction.ReferenceType.Subgraph));
                return;
            }

            if (!DragAndDrop.objectReferences.Any(x => x is GameObject go && go.GetComponent<ScriptingGraphAuthoring>()))
            {
                store.Dispatch(new CreateObjectReferenceAction(graphSpacePosition, state?.CurrentGraphModel, DragAndDrop.objectReferences, CreateObjectReferenceAction.ReferenceType.Object));
                return;
            }

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Object Reference"), false, () =>
            {
                store.Dispatch(new CreateObjectReferenceAction(graphSpacePosition, state?.CurrentGraphModel, DragAndDrop.objectReferences, CreateObjectReferenceAction.ReferenceType.Object));
            });
            menu.AddItem(new GUIContent("Smart Object"), false, () =>
            {
                store.Dispatch(new CreateObjectReferenceAction(graphSpacePosition, state?.CurrentGraphModel, DragAndDrop.objectReferences, CreateObjectReferenceAction.ReferenceType.ObjectGraph));
            });
            menu.DropDown(new Rect(e.originalMousePosition, Vector2.zero));
        }
    }
}
