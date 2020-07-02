using System.Collections.Generic;
using Modifier.Runtime;

namespace Modifier.DotsStencil
{
    public interface IReferenceComponentTypes : IDotsNodeModel
    {
        IEnumerable<TypeReference> ReferencedTypes { get; }
    }
}