using System;
using System.Collections.Generic;
using System.Linq;
using Modifier.Runtime;
using Unity.Entities;

namespace Modifier.DotsStencil
{
    [Serializable, ComponentNodeSearcher("Get")]
    class GetComponentNodeModel : DotsNodeModel<GetComponent>, IReferenceComponentTypes
    {
        Type ComponentType => TypedNode.Type.TypeIndex == -1 ? null : TypeManager.GetType(TypedNode.Type.TypeIndex);
        public override string Title => "Get " + (TypedNode.Type.TypeIndex == -1 ? "<Unknown Component>" : ComponentType.Name);
        public override IReadOnlyDictionary<string, List<PortMetaData>> PortCustomData =>
            new Dictionary<string, List<PortMetaData>>
        {
            {
                nameof(GetComponent.ComponentData),
                TypedNode.Type.TypeIndex == -1 ?
                new List<PortMetaData>() :
                PortMetaData.FromValidTypeFields(ComponentType, Stencil).ToList()
            }
        };

        public IEnumerable<TypeReference> ReferencedTypes => Enumerable.Repeat(TypedNode.Type, 1);
    }
}
