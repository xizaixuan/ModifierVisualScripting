using System.Collections.Generic;
using Unity.Entities;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine.Modifier.VisualScripting;

namespace Modifier.DotsStencil
{
    public class GraphTrace : IGraphTrace
    {
        public Entity Entity;
        public readonly string EntityName;
        public CircularBuffer<EntityFrameData> Frames;

        public GraphTrace(Entity entity, string entityName)
        {
            Entity = entity;
            EntityName = entityName;
            Frames = new CircularBuffer<EntityFrameData>(100);
        }

        public IReadOnlyList<IFrameData> AllFrames => Frames;
    }
}
