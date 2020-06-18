using System;
using Modifier.Runtime;
using UnityEditor;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modifier.DotsStencil
{
    public class ModifierGraphTemplate : ICreatableGraphTemplate
    {
        public static readonly string k_DefaultGraphName = "Scripting Graph";

        public static ModifierGraphTemplate ObjectGraphAsset()
        {
            return new ModifierGraphTemplate(DotsStencil.GraphType.Object);
        }

        private readonly DotsStencil.GraphType m_GraphType;

        private ModifierGraphTemplate(DotsStencil.GraphType graphType)
        {
            m_GraphType = graphType;
        }

        public Type StencilType => typeof(DotsStencil);

        public string GraphTypeName => k_DefaultGraphName;

        public string DefaultAssetName => GraphTypeName;

        public void InitBasicGraph(VSGraphModel graphModel)
        {
            var DotsStencil = (DotsStencil)graphModel.Stencil;
            DotsStencil.Type = m_GraphType;

            if (DotsStencil.CompiledScriptingGraphAsset == null)
            {
                CreateModifierCompiledScriptingGraphAsset(graphModel);
            }
        }

        internal static void CreateModifierCompiledScriptingGraphAsset(IGraphModel graphModel)
        {
            DotsStencil stencil = (DotsStencil)graphModel.Stencil;
            stencil.CompiledScriptingGraphAsset = ScriptableObject.CreateInstance<ScriptingGraphAsset>();
            AssetDatabase.AddObjectToAsset(stencil.CompiledScriptingGraphAsset, (Object)graphModel.AssetModel);
        }
    }
}