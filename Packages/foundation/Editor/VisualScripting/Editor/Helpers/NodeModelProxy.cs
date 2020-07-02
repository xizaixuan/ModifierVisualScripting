using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class NodeModelProxy<T> : ScriptableObject, INodeModelProxy where T : IGraphElementModel
    {
        public ScriptableObject ScriptableObject() { return this; }

        public void SetModel(IGraphElementModel model) { Model = (T)model; }

        [SerializeReference]
        public T Model;
    }
}