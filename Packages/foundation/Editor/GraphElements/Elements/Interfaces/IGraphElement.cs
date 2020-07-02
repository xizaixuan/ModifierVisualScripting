using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEditor.Modifier.EditorCommon.Redux;

namespace Unity.Modifier.GraphElements
{
    public interface IGraphElement
    {
        IGTFGraphElementModel Model { get; }
        void Setup(IGTFGraphElementModel model, IStore store, GraphView graphView);
        void UpdateFromModel();
    }
}