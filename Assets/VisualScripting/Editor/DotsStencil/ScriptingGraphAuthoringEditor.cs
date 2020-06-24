using System;
using System.Collections.Generic;
using System.Linq;
using Modifier.Runtime;
using UnityEditor;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using ValueType = Modifier.Runtime.ValueType;

namespace Modifier.DotsStencil
{
    [CustomEditor(typeof(ScriptingGraphAuthoring))]
    class ScriptingGraphAuthoringEditor : Editor
    {
        private HashSet<BindingId> m_ProcessedBindings;
        public override void OnInspectorGUI()
        {
            bool dirty = false;
            ScriptingGraphAuthoring authoring = target as ScriptingGraphAuthoring;
            VSGraphAssetModel assetModel = null;
            if (authoring.ScriptingGraph)
            {
                var path = AssetDatabase.GetAssetPath(authoring.ScriptingGraph);
                assetModel = AssetDatabase.LoadAssetAtPath<VSGraphAssetModel>(path);
            }

            var newAssetModel =
                EditorGUILayout.ObjectField("Scripting Graph", assetModel, typeof(VSGraphAssetModel), false) as
                VSGraphAssetModel;
            if (assetModel != newAssetModel)
            {
                dirty = true;
                if (newAssetModel)
                    authoring.ScriptingGraph = ((DotsStencil)newAssetModel.GraphModel.Stencil).CompiledScriptingGraphAsset;
                else
                    authoring.ScriptingGraph = null;
            }

            // I/O
            if (!(newAssetModel?.GraphModel is VSGraphModel graph))
                return;
            if (m_ProcessedBindings == null)
                m_ProcessedBindings = new HashSet<BindingId>();
            else
                m_ProcessedBindings.Clear();

            foreach (var graphVariableModel in graph.GraphVariableModels)
            {
                var variableType = GraphBuilder.GetVariableType(graphVariableModel);
                switch (variableType)
                {
                    case GraphBuilder.VariableType.SmartObject:
                    case GraphBuilder.VariableType.ObjectReference:
                        BindingId id = GetExistingBinding(graphVariableModel, authoring, out var binding);
                        m_ProcessedBindings.Add(id);
                        if (binding == null)
                        {
                            dirty = true;
                            authoring.Values.Add(binding = new ScriptingGraphAuthoring.InputBindingAuthoring(id));
                        }

                        var valueType = graphVariableModel.DataType.ToValueType();
                        EditorGUI.BeginChangeCheck();
                        switch (valueType)
                        {
                            case ValueType.Entity:
                                binding.Object = EditorGUILayout.ObjectField(graphVariableModel.Name, binding.Object,
                                    typeof(GameObject), true);
                                break;
                            default:
                                EditorGUILayout.LabelField(graphVariableModel.Name, valueType.ToString());
                                break;
                        }

                        if (EditorGUI.EndChangeCheck())
                            dirty = true;
                        break;
                    case GraphBuilder.VariableType.InputOutput:
                    case GraphBuilder.VariableType.Variable:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (authoring.Values != null)
                for (var index = authoring.Values.Count - 1; index >= 0; index--)
                {
                    var binding = authoring.Values[index];
                    if (!m_ProcessedBindings.Contains(binding.Id))
                    {
                        authoring.Values.RemoveAt(index);
                        dirty = true;
                    }
                }

            if (dirty)
                EditorUtility.SetDirty(authoring);
        }

        private static BindingId GetExistingBinding(IVariableDeclarationModel graphVariableModel,
            ScriptingGraphAuthoring authoring, out ScriptingGraphAuthoring.InputBindingAuthoring binding)
        {
            var id = GraphBuilder.GetBindingId(graphVariableModel);

            binding = GetExistingBinding(authoring, id);
            return id;
        }

        private static ScriptingGraphAuthoring.InputBindingAuthoring GetExistingBinding(
            ScriptingGraphAuthoring authoring, BindingId id)
        {
            var binding = authoring.Values?.FirstOrDefault(v => v.Id.Equals(id));
            return binding;
        }

        public static void BindInput(ScriptingGraphAuthoring authoring, VariableDeclarationModel graphVariableModel,
            GameObject gameObject)
        {
            var id = GraphBuilder.GetBindingId(graphVariableModel);
            var binding = GetExistingBinding(authoring, id);
            if (binding == null)
            {
                if (authoring.Values == null)
                    authoring.Values = new List<ScriptingGraphAuthoring.InputBindingAuthoring>();
                authoring.Values.Add(binding = new ScriptingGraphAuthoring.InputBindingAuthoring(id));
            }
            binding.Object = gameObject;
            EditorUtility.SetDirty(authoring);
        }
    }
}
