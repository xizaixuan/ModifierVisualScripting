using System;
using Modifier.DotsStencil;
using UnityEditor;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEngine;

namespace Modifier.VisualScripting.Editor
{
    public static class DotsGraphCreator
    {
        [MenuItem("GameObject/VisualScripting/Scripting Graph", priority = 10)]
        public static void CreateGraphObject(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            var initialState = new State(null);
            var store = new Store(initialState);
            CreateGraphOnNewGameObject(store, parent, false);
        }

        public static void CreateGraphOnNewGameObject(Store store, GameObject parent, bool useSelection)
        {
            var selection = Selection.gameObjects;
            var template = DotsGraphTemplate.ObjectGraphFromSelection(parent, useSelection ? selection : null);
            template.PromptToCreate(store);
        }

        [MenuItem("Assets/Create/VisualScripting/Scripting Graph")]
        public static void CreateGraph(MenuCommand menuCommand)
        {
            var initialState = new State(null);
            var store = new Store(initialState);
            var template = DotsGraphTemplate.ObjectGraphAsset();
            template.PromptToCreate(store);
        }

        public static void CreateSubgraph(Store store)
        {
            var template = DotsGraphTemplate.SubGraphAsset();
            template.PromptToCreate(store);
        }
    }
}
