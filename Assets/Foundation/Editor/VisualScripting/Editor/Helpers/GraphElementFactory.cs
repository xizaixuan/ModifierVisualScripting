using JetBrains.Annotations;
using System;
using System.Reflection;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;

namespace Unity.Modifier.GraphElements
{
    public static class GraphElementFactory
    {
        static Dictionary<ValueTuple<GraphView, IGTFGraphElementModel>, IGraphElement> s_UIForModel = new Dictionary<ValueTuple<GraphView, IGTFGraphElementModel>, IGraphElement>();

        [CanBeNull]
        public static T GetUI<T>(this IGTFGraphElementModel model, GraphView graphView) where T : class, IGraphElement
        {
            return s_UIForModel.TryGetValue(new ValueTuple<GraphView, IGTFGraphElementModel>(graphView, model), out var ui) ? ui as T : null;
        }

        [CanBeNull]
        public static T CreateUI<T>(this IGTFGraphElementModel model, GraphView graphView, IStore store) where T : class, IGraphElement
        {
            return CreateUI<T>(graphView, store, model);
        }

        public static T CreateUI<T>(GraphView graphView, IStore store, IGTFGraphElementModel model) where T : class, IGraphElement
        {
            if (model == null)
            {
                Debug.LogError("GraphElementFactory could not create element because model is null.");
                return null;
            }

            if (graphView == null)
            {
                Debug.LogError("GraphElementFactory could not create element because graphView is null.");
                return null;
            }

            var ext = ExtensionMethodCache<ElementBuilder>.GetExtensionMethod(
                model.GetType(),
                ElementBuilder.FilterMethods,
                ElementBuilder.KeySelector
            );

            T newElem = null;
            if (ext != null)
            {
                var nodeBuilder = new ElementBuilder { GraphView = graphView };
                newElem = ext.Invoke(null, new object[] { nodeBuilder, store, model }) as T;
            }

            if (newElem == null)
            {
                Debug.LogError($"GraphElementFactory doesn't know how to create a UI for element of type: {model.GetType()}");
                return null;
            }

            s_UIForModel[new ValueTuple<GraphView, IGTFGraphElementModel>(graphView, model)] = newElem;

            return newElem;
        }

        public static void RemoveAll(GraphView graphView)
        {
            var toRemove = s_UIForModel.Where(pair => pair.Key.Item1 == graphView).Select(pair => pair.Key).ToList();

            foreach (var key in toRemove)
            {
                s_UIForModel.Remove(key);
            }
        }
    }
}