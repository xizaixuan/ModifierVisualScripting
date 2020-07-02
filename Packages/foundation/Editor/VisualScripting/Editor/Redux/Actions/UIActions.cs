using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class RefreshUIAction : IAction
    {
        public readonly UpdateFlags UpdateFlags;
        public readonly List<IGraphElementModel> ChangedModels;

        public RefreshUIAction(UpdateFlags updateFlags, List<IGraphElementModel> changedModels = null)
        {
            UpdateFlags = updateFlags;
            ChangedModels = changedModels;
        }

        public RefreshUIAction(List<IGraphElementModel> changedModels) : this(UpdateFlags.None, changedModels)
        {
        }
    }

    [PublicAPI]
    public class OpenDocumentationAction : IAction
    {
        public readonly INodeModel[] NodeModels;

        public OpenDocumentationAction(params INodeModel[] nodeModels)
        {
            NodeModels = nodeModels;
        }
    }
}