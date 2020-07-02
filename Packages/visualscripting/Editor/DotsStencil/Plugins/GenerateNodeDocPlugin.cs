using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Modifier.Elements;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.Editor.Plugins;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using UnityEngine.Assertions;

namespace Modifier.DotsStencil
{
    public class GenerateNodeDocPlugin : IPluginHandler
    {
        public void Register(Store store, VseWindow window)
        {
        }

        public void Unregister()
        {
        }

        public void OptionsMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Generate Node Documentation"), false, GenerateNodeDoc);
        }

        private void GenerateNodeDoc()
        {
            const string docPath = UIHelper.NodeDocumentationPath;
            if (Directory.Exists(docPath))
            {
                foreach (var enumerateFile in Directory.EnumerateFiles(docPath, "*.md", SearchOption.AllDirectories))
                {
                    File.Delete(enumerateFile);
                }
            }
            Directory.CreateDirectory(docPath);

            var gam = GraphAssetModel.Create("Doc", null, typeof(VSGraphAssetModel), false);
            var stateCurrentGraphModel = gam.CreateGraph<VSGraphModel>("Doc", typeof(DotsStencil), false);
            var stencil = stateCurrentGraphModel.Stencil;
            var dbProvider = stencil.GetSearcherDatabaseProvider();
            var dbs = dbProvider.GetGraphElementsSearcherDatabases();

            HashSet<string> fileNames = new HashSet<string>();
            foreach (var searcherItem in dbs[0].ItemList.OfType<GraphNodeModelSearcherItem>())
            {
                var graphElementModels = GraphNodeSearcherAdapter.CreateGraphElementModels(stateCurrentGraphModel,  searcherItem);
                if (graphElementModels.Count() != 1)
                    continue;

                var model = graphElementModels.Single();

                if (model is INodeModel nodeModel)
                {
                    string fileName;
                    if (string.IsNullOrWhiteSpace(searcherItem.Path))
                        fileName = nodeModel.GetType().Name;
                    else
                        fileName = MakePath(searcherItem, Path.DirectorySeparatorChar);
                    // fileName = fileName.Replace('/', Path.DirectorySeparatorChar);
                    Assert.IsTrue(fileNames.Add(fileName), "Duplicate filename: " + fileName);

                    var formatter = new MarkdownNodeDocumentationFormatter();
                    formatter.DocumentNode(searcherItem, nodeModel);

                    var filePath = Path.Combine(docPath, $"{fileName}.md");
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    var contents = formatter.ToString();
                    if (!File.Exists(filePath) || File.ReadAllText(filePath) != contents)
                        File.WriteAllText(filePath, contents);
                }
            }

            GenerateToc();
        }

        private string MakePath(SearcherItem searcherItem, char separator)
        {
            if (searcherItem.Parent == null)
                return searcherItem.Name;
            return MakePath(searcherItem.Parent, separator) + separator + searcherItem.Name;
        }

        static Regex FileRegex = new Regex("((?<number>\\d+)\\.)?(?<name>.+)\\.md");
        private void GenerateToc()
        {
            string file = Path.Combine(UIHelper.DocumentationPath, "TableOfContents.md");
            StringBuilder sb = new StringBuilder();
            var l = Collect(UIHelper.DocumentationPath, false);

            foreach (var valueTuple in l)
            {
                switch (valueTuple.title)
                {
                    case "index":
                    case "TableOfContents":
                        continue;
                }
                sb.AppendLine($"* [{valueTuple.title}]({valueTuple.fileName})");
                if (valueTuple.title.StartsWith("Nodes"))
                {
                    foreach (var nodeTuple in Collect(UIHelper.NodeDocumentationPath, true))
                    {
                        sb.AppendLine($"    * [{nodeTuple.title}](Nodes/{nodeTuple.fileName})");
                    }
                }
            }

            File.WriteAllText(file, sb.ToString());
        }

        private static List<(int index, string title, string fileName)> Collect(string docFolder, bool recursive)
        {
            var l = new List<(int index, string title, string fileName)>();
            foreach (var md in Directory.EnumerateFiles(docFolder, "*.md", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                var filePath = md.Substring(docFolder.Length /* end slash */);
                var fileName = Path.GetFileName(filePath);
                var matches = FileRegex.Match(fileName);
                var number = matches.Groups["number"].Value;
                int i = int.TryParse(number, out var j) ? j : 1000;
                var title = matches.Groups["name"].Value;
                l.Add((i, title, filePath.Replace(Path.DirectorySeparatorChar, '/')));
            }

            return l.OrderBy(x => x.index).ThenBy(x => x.title).ToList();
        }
    }
}
