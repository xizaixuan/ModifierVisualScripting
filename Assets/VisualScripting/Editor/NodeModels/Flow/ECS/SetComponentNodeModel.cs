using System;
using System.Collections.Generic;
using System.Linq;
using Modifier.Runtime;
using Unity.Entities;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    [Serializable, ComponentNodeSearcher("Set")]
    class SetComponentNodeModel : DotsNodeModel<SetComponent>, IReferenceComponentTypes
    {
        Type ComponentType => TypedNode.Type.TypeIndex == -1 ? null : TypeManager.GetType(TypedNode.Type.TypeIndex);
        public override string Title => "Set " + (TypedNode.Type.TypeIndex == -1 ? "<Unknown Component>" : ComponentType.Name);

        public override IReadOnlyDictionary<string, List<PortMetaData>> PortCustomData =>
            new Dictionary<string, List<PortMetaData>>
        {
            {
                nameof(SetComponent.ComponentData),
                TypedNode.Type.TypeIndex == -1 ?
                new List<PortMetaData>() :
                PortMetaData.FromValidTypeFields(ComponentType, Stencil).Select(x =>
                {
                    x.PortModelOptions = PortModel.PortModelOptions.NoEmbeddedConstant;
                    return x;
                }).ToList()
            }
        };
        public IEnumerable<TypeReference> ReferencedTypes => Enumerable.Repeat(TypedNode.Type, 1);
    }
}
