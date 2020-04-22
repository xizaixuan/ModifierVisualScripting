﻿
using System;
using Modifier.Runtime;
using UnityEditor;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ModifierStencil
{
    public class ModifierGraphTemplate : ICreatableGraphTemplate
    {
        public static readonly string k_DefaultGraphName = "Scripting Graph";

        public static ModifierGraphTemplate ObjectGraphAsset()
        {
            return new ModifierGraphTemplate(ModifierStencil.GraphType.Object);
        }

        private readonly ModifierStencil.GraphType m_GraphType;

        private ModifierGraphTemplate(ModifierStencil.GraphType graphType)
        {
            m_GraphType = graphType;
        }

        public Type StencilType => typeof(ModifierStencil);

        public string GraphTypeName => k_DefaultGraphName;

        public string DefaultAssetName => GraphTypeName;

        public void InitBasicGraph(VSGraphModel graphModel)
        {
            var modifierStencil = (ModifierStencil)graphModel.Stencil;
            modifierStencil.Type = m_GraphType;

            if (modifierStencil.CompiledScriptingGraphAsset == null)
            {
                CreateModifierCompiledScriptingGraphAsset(graphModel);
            }
        }

        internal static void CreateModifierCompiledScriptingGraphAsset(IGraphModel graphModel)
        {
            ModifierStencil stencil = (ModifierStencil)graphModel.Stencil;
            stencil.CompiledScriptingGraphAsset = ScriptableObject.CreateInstance<ScriptingGraphAsset>();
            AssetDatabase.AddObjectToAsset(stencil.CompiledScriptingGraphAsset, (Object)graphModel.AssetModel);
        }
    }
}