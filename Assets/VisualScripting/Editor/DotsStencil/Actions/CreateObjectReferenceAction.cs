using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEngine;

namespace Modifier.DotsStencil
{
    public class CreateObjectReferenceAction : IAction
    {
        public enum ReferenceType
        {
            Object,
            ObjectGraph,
            Subgraph,
        }
        public readonly Vector2 GraphSpacePosition;
        public readonly IGraphModel GraphModel;
        public readonly Object[] Objects;
        public readonly ReferenceType Type;

        public CreateObjectReferenceAction(Vector2 graphSpacePosition, IGraphModel graphModel, Object[] objects, ReferenceType referenceType)
        {
            GraphSpacePosition = graphSpacePosition;
            GraphModel = graphModel;
            Objects = objects;
            Type = referenceType;
        }
    }
}
