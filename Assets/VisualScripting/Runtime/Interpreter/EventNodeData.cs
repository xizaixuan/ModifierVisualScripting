using System.Collections.Generic;
using Unity.Entities;

namespace Modifier.Runtime
{
    public readonly struct EventNodeData
    {
        public ulong Id { get; }
        public Entity Source { get; }
        public Entity Target { get; }
        public IEnumerable<Value> Values { get; }

        public EventNodeData(ulong id, IEnumerable<Value> values, Entity target = default, Entity source = default)
        {
            Id = id;
            Values = values;
            Target = target;
            Source = source;
        }
    }
}
