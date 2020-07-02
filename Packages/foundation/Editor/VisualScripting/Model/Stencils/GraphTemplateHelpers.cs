using JetBrains.Annotations;
using System;
using System.IO;
using UnityEditor.Modifier.VisualScripting.Model;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    [PublicAPI]
    public static class GraphTemplateHelpers
    {
        public static void PromptToCreate(this ICreatableGraphTemplate template, Store store)
        {
            PromptToCreate(template.StencilType, store, template.GraphTypeName, template.DefaultAssetName, template);
        }

        public static void PromptToCreate(Type stencilType, Store store, string graphTitle, string assetName, IGraphTemplate template = null)
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create scripting graph",
                assetName,
                "asset", "Create a new scripting graph for " + graphTitle);

            if (path.Length != 0)
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                store.Dispatch(new CreateGraphAssetAction(stencilType, fileName, path, graphTemplate: template));
            }
        }
    }
}