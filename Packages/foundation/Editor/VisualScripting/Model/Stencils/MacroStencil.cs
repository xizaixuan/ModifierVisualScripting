﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Compilation;
using UnityEditor.Modifier.VisualScripting.Model.Translators;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Modifier.VisualScripting;

namespace UnityEditor.Modifier.VisualScripting.Model.Stencils
{
    public class MacroStencil : Stencil
    {
        [SerializeReference]
        Stencil m_Parent;

        ISearcherFilterProvider m_MacroSearcherFilterProvider;

        public override StencilCapabilityFlags Capabilities => StencilCapabilityFlags.SupportsMacros;
        public override IBuilder Builder => m_Parent?.Builder;
        public override IRuntimeStencilReference RuntimeReference => m_Parent.RuntimeReference;

        public override void PreProcessGraph(VSGraphModel graphModel)
        {
            m_Parent.PreProcessGraph(graphModel);
        }

        public override IEnumerable<INodeModel> GetEntryPoints(VSGraphModel vsGraphModel)
        {
            return vsGraphModel.VariableDeclarations.Where(v => v.Modifiers == ModifierFlags.WriteOnly).SelectMany(vsGraphModel.FindUsages<VariableNodeModel>);
        }

        public void SetParent(Type type, VSGraphModel graphModel)
        {
            Assert.IsTrue(typeof(Stencil).IsAssignableFrom(type));
            m_Parent = (Stencil)Activator.CreateInstance(type);
            graphModel.AssetModel.SetAssetDirty();
        }

        internal Type ParentType => m_Parent.GetType();

        public override IBlackboardProvider GetBlackboardProvider()
        {
            return m_BlackboardProvider ?? (m_BlackboardProvider = new BlackboardMacroProvider(this));
        }

        public override ISearcherFilterProvider GetSearcherFilterProvider()
        {
            return m_MacroSearcherFilterProvider ?? (m_MacroSearcherFilterProvider = new MacroSearcherFilterProvider(this));
        }

        public override ISearcherDatabaseProvider GetSearcherDatabaseProvider()
        {
            return m_Parent.GetSearcherDatabaseProvider();
        }

        public override List<ITypeMetadata> GetAssembliesTypesMetadata()
        {
            return m_Parent.GetAssembliesTypesMetadata();
        }

        public override ITranslator CreateTranslator()
        {
            return new NoOpTranslator();
        }
    }
}