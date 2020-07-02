using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Modifier.Runtime
{
    [RequiresEntityConversion]
    public class ScriptingGraphAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [Serializable]
        public class InputBindingAuthoring
        {
            [FormerlySerializedAs("GUID")] public BindingId Id;
            public Object Object;

            public InputBindingAuthoring(BindingId strId)
            {
                Id = strId;
            }
        }

        public List<InputBindingAuthoring> Values = new List<InputBindingAuthoring>();
        public ScriptingGraphAsset ScriptingGraph;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            foreach (var valueBinding in Values.Where(valueBinding => !valueBinding.Id.IsNull))
            {
                if (!ScriptingGraph.Definition.GetInputBindingId(valueBinding.Id, out var id))
                    continue;

                if (valueBinding.Object is GameObject go && !go.scene.IsValid())
                    referencedPrefabs.Add(go);
            }
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddSharedComponentData(entity, new ScriptingGraph { ScriptingGraphAsset = ScriptingGraph });
            AddInputs(entity, dstManager, conversionSystem);
        }

        void AddInputs(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (Values == null || Values.Count == 0)
                return;

            var inputs = dstManager.AddBuffer<ValueInput>(entity);
            var bindingsToProcess = Values.ToDictionary(v => v.Id, v => v);
            foreach (var inputbinding in ScriptingGraph.Definition.Bindings)
            {
                if (!bindingsToProcess.TryGetValue(inputbinding.Id, out var valueBinding))
                {
                    Debug.LogError($"Object reference {inputbinding.Id} in the graph {ScriptingGraph} doesn't have a matching value in this {GetType().Name} component", this);
                    continue;
                }

                bindingsToProcess.Remove(inputbinding.Id);

                Value v;
                if (valueBinding.Object)
                {
                    var primaryEntity = conversionSystem.GetPrimaryEntity(valueBinding.Object);
                    if (primaryEntity == Entity.Null)
                        Debug.LogError($"Object reference {inputbinding.Id} in the graph {ScriptingGraph} references this object {valueBinding.Object}, which is not converted and doesn't have a matching entity", this);
                    v = new Value { Entity = primaryEntity };
                }
                else
                {
                    Debug.LogError($"Object reference {inputbinding.Id} references a null object", this);
                    continue;
                }

                inputs.Add(new ValueInput { Index = inputbinding.DataIndex, Value = v });
            }
        }
    }
}
