using Modifier.Runtime;
using Modifier.VisualScripting.Editor.Elements.Interfaces;
using System;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Compilation;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEditor.Modifier.VisualScripting.Model.Translators;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.Modifier.VisualScripting;

namespace Modifier.DotsStencil
{
    public class DotsStencil : Stencil
    {
        public enum GraphType { Object };

        [HideInInspector, SerializeField]
        public GraphType Type;

        public ScriptingGraphAsset CompiledScriptingGraphAsset;

        public override IBuilder Builder => throw new NotImplementedException();

        public override ISearcherDatabaseProvider GetSearcherDatabaseProvider()
        {
            throw new NotImplementedException();
        }
    }
}
