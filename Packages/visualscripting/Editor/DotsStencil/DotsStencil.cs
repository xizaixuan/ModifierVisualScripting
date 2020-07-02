using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modifier.Runtime;
using UnityEditor;
using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEditor.Compilation;
using Unity.Modifier.GraphElements;
using UnityEditor.Searcher;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.Editor.Plugins;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Compilation;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEditor.Modifier.VisualScripting.Model.Translators;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Modifier.VisualScripting;
using Modifier.VisualScripting.Editor.Elements.Interfaces;
using PortType = UnityEditor.Modifier.VisualScripting.Model.PortType;

namespace Modifier.DotsStencil
{
    public class DotsStencil : Stencil
    {
        public enum GraphType { Object, Subgraph, }

        private const string k_DuplicateInputOutputWarning = "Inputs and outputs can only be present once in a graph, skipping the creation of duplicates";

        class DotsBuilder : IBuilder
        {
            public void Build(IEnumerable<GraphAssetModel> vsGraphAssetModels, Action<string, CompilerMessage[]> roslynCompilationOnBuildFinished)
            {
                foreach (var graphAssetModel in vsGraphAssetModels)
                {
                    var vsGraphModel = (VSGraphModel)graphAssetModel.GraphModel;
                    var translator = vsGraphModel.CreateTranslator();
                    if (!translator.SupportsCompilation())
                        continue;

                    vsGraphModel.Compile(AssemblyType.Source, translator,
                        CompilationOptions.Default | CompilationOptions.LiveEditing);
                }
            }
        }

        public ScriptingGraphAsset CompiledScriptingGraphAsset;

        ISearcherDatabaseProvider m_SearcherDatabaseProvider;
        ISearcherFilterProvider m_SearcherFilterProvider;
        IDebugger _debugger = new DotsDebugger();
        private DotsDragNDropHandler m_DragNDropHandler;
        static DotsBuilder s_DotsBuilder = new DotsBuilder();

        [HideInInspector, SerializeField]
        public GraphType Type;

        public override IBlackboardProvider GetBlackboardProvider()
        {
            return m_BlackboardProvider ?? (m_BlackboardProvider = new DotsBlackboardProvider(this));
        }

        public override IToolbarProvider GetToolbarProvider()
        {
            return m_ToolbarProvider ?? (m_ToolbarProvider = new DotsToolbarProvider());
        }

        public override bool CanPasteNode(NodeModel originalModel, VSGraphModel graph)
        {
            switch (originalModel)
            {
                case VariableNodeModel variableNodeModel when variableNodeModel.DeclarationModel?.IsInputOrOutput() == true:
                    {
                        var canPasteNode = !graph.FindUsages((VariableDeclarationModel)variableNodeModel.DeclarationModel).Any();
                        if (!canPasteNode)
                            Debug.LogWarning(k_DuplicateInputOutputWarning);
                        return canPasteNode;
                    }
                case EdgePortalModel edgePortalModel:
                    return graph.PortalDeclarations.Contains(edgePortalModel.DeclarationModel);
                default:
                    return base.CanPasteNode(originalModel, graph);
            }
        }

        public override void OnDragAndDropVariableDeclarations(Store store, List<Tuple<IVariableDeclarationModel, Vector2>> variablesToCreate)
        {
            if (variablesToCreate.Any(x => !x.Item1.IsGraphVariable()))
            {
                int inputOutputsAlreadyPresentInGraph = variablesToCreate.RemoveAll(x =>
                    x.Item1.IsInputOrOutput() && ((VSGraphModel)store.GetState().CurrentGraphModel)
                        .FindUsages((VariableDeclarationModel)x.Item1).Any());
                if (inputOutputsAlreadyPresentInGraph > 0)
                    Debug.LogWarning(k_DuplicateInputOutputWarning);
                base.OnDragAndDropVariableDeclarations(store, variablesToCreate);
            }
            else if (Event.current?.shift == true)
                store.Dispatch(new DotsCreateGetSetVariableNodesAction(variablesToCreate, createGetters: false)); // set
            else if (Event.current?.alt == true)
                store.Dispatch(new DotsCreateGetSetVariableNodesAction(variablesToCreate, createGetters: true)); // get
            else
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Set Variable (Hold Shift)"), false, () =>
                {
                    store.Dispatch(new DotsCreateGetSetVariableNodesAction(variablesToCreate, createGetters: false)); // set
                });
                menu.AddItem(new GUIContent("Get Variable (Hold Alt)"), false, () =>
                {
                    store.Dispatch(new DotsCreateGetSetVariableNodesAction(variablesToCreate, createGetters: true)); // get
                });
                menu.ShowAsContext();
            }
        }

        public override IVariableModel CreateVariableModelForDeclaration(IGraphModel graphModel, IVariableDeclarationModel declarationModel, Vector2 position, SpawnFlags spawnFlags = SpawnFlags.Default, GUID? guid = null)
        {
            // Custom smart object reference node
            if (declarationModel.IsSmartObject())
                return graphModel.CreateNode<SmartObjectReferenceNodeModel>(declarationModel.Title, position, spawnFlags, n => n.DeclarationModel = declarationModel);
            // For Object references and inputs, keep the base VariableNodeModel
            if (declarationModel.IsObjectReference() || (declarationModel.IsInputOrOutput() && !declarationModel.IsDataOutput()))
                return base.CreateVariableModelForDeclaration(graphModel, declarationModel, position, spawnFlags, guid);
            // Graph Variables: custom SetVariableNodeModel
            return graphModel.CreateNode<SetVariableNodeModel>(declarationModel.Title, position, spawnFlags, v =>
            {
                v.IsGetter = declarationModel.IsDataInput();
                v.DeclarationModel = declarationModel;
            }, guid);
        }

        public override ISearcherFilterProvider GetSearcherFilterProvider()
        {
            return m_SearcherFilterProvider ?? (m_SearcherFilterProvider = new DotsSearcherFilterProvider(this));
        }

        public override ISearcherDatabaseProvider GetSearcherDatabaseProvider()
        {
            return m_SearcherDatabaseProvider ?? (m_SearcherDatabaseProvider = new DotsSearcherDatabaseProvider(this));
        }

        public override ISearcherAdapter GetSearcherAdapter(IGraphModel graphModel, string title)
        {
            return new DotsNodeSearcherAdapter(graphModel, title);
        }

        public override IExternalDragNDropHandler DragNDropHandler => m_DragNDropHandler ?? (m_DragNDropHandler = new DotsDragNDropHandler());

        public override ITranslator CreateTranslator()
        {
            return new DotsTranslator();
        }

        public override bool MoveNodeDependenciesByDefault => false;

        public override IBuilder Builder => s_DotsBuilder;


        public override string GetSourceFilePath(VSGraphModel graphModel)
        {
            return Path.Combine(ModelUtility.GetAssemblyRelativePath(), graphModel.TypeName + ".asset");
        }

        public override IDebugger Debugger => _debugger ?? (_debugger = new DotsDebugger());

        public override void OnCompilationSucceeded(VSGraphModel graphModel, CompilationResult results)
        {
            var hash = ((DotsTranslator.DotsCompilationResult)results).GraphDefinition.ComputeHash();
            if (hash == CompiledScriptingGraphAsset.HashCode)
                return;
            CompiledScriptingGraphAsset.HashCode = hash;
            if (!EditorApplication.isPlaying)
                return;
            ScriptingGraphRuntime.LastVersion++;
        }

        public override IEnumerable<IPluginHandler> GetCompilationPluginHandlers(CompilationOptions getCompilationOptions)
        {
            foreach (var compilationPluginHandler in base.GetCompilationPluginHandlers(getCompilationOptions))
                yield return compilationPluginHandler;
            yield return new DotsBoundObjectPlugin();
            if (Unsupported.IsDeveloperMode())
                yield return new GenerateNodeDocPlugin();
        }

        public override void CreateNodesFromPort(Store store, IPortModel portModel, Vector2 localPosition, Vector2 worldPosition,
            IEnumerable<IGTFEdgeModel> edgesToDelete, IStackModel stackModel, int index)
        {
            switch (portModel.Direction)
            {
                case Direction.Output:
                    SearcherService.ShowOutputToGraphNodes(store.GetState(), portModel, worldPosition, item =>
                        store.Dispatch(new CreateNodeFromOutputPortAction(portModel, localPosition, item, edgesToDelete)));
                    break;

                case Direction.Input:
                    SearcherService.ShowInputToGraphNodes(store.GetState(), portModel, worldPosition, item =>
                        store.Dispatch(new CreateNodeFromInputPortAction(portModel, localPosition, item, edgesToDelete)));
                    break;
            }
        }

        public static void CreateVariablesFromGameObjects(VSGraphModel graph, ScriptingGraphAuthoring authoringComponent, IEnumerable<GameObject> gameObjects, Vector2 position, bool actionSmartObjects)
        {
            foreach (var obj in gameObjects)
            {
                var decl = graph.CreateGraphVariableDeclaration(obj.name, TypeHandle.GameObject, true);
                decl.MakeObjectReference();
                ScriptingGraphAuthoring scriptingGraphAuthoring = null;
                if (actionSmartObjects && (scriptingGraphAuthoring = obj.GetComponent<ScriptingGraphAuthoring>()))
                    decl.MakeSmartObject();
                var variableNode = graph.CreateVariableNode(decl, position);
                if (scriptingGraphAuthoring != null)
                {
                    var path = AssetDatabase.GetAssetPath(scriptingGraphAuthoring.ScriptingGraph);
                    var referencedAssetModel = AssetDatabase.LoadAssetAtPath<VSGraphAssetModel>(path);
                    ((SmartObjectReferenceNodeModel)variableNode).GraphReference = referencedAssetModel;
                }
                ScriptingGraphAuthoringEditor.BindInput(authoringComponent, decl, obj);
                position += new Vector2(20, 25);
            }
        }

        public static void CreateSubGraphReference(VSGraphModel graphModel, IEnumerable<VSGraphAssetModel> subgraphAssets, Vector2 position)
        {
            foreach (var subgraphModel in subgraphAssets)
            {
                var subgraphReference = graphModel.CreateNode<SubgraphReferenceNodeModel>(subgraphModel.Name, position,
                    SpawnFlags.Default,
                    v => { v.GraphReference = subgraphModel; });
                position += new Vector2(20, 25);
            }
        }

        public override IEnumerable<INodeModel> GetEntryPoints(VSGraphModel vsGraphModel)
        {
            return vsGraphModel.NodeModels.OfType<IDotsNodeModel>().Where(n => typeof(IEntryPointNode).IsAssignableFrom(n.NodeType));
        }

        public override bool CreateDependencyFromEdge(IEdgeModel model, out LinkedNodesDependency linkedNodesDependency, out INodeModel parent)
        {
            var outputNode = model.OutputPortModel.NodeModel;
            var inputNode = model.InputPortModel.NodeModel;
            bool outputIsData = outputNode.IsDataNode();
            bool inputIsData = inputNode.IsDataNode();
            if (outputIsData)
            {
                parent = inputNode;
                linkedNodesDependency = new LinkedNodesDependency
                {
                    count = 1,
                    DependentPort = model.OutputPortModel,
                    ParentPort = model.InputPortModel,
                };
                return true;
            }
            if (!inputIsData)
            {
                parent = outputNode;
                linkedNodesDependency = new LinkedNodesDependency
                {
                    count = 1,
                    DependentPort = model.InputPortModel,
                    ParentPort = model.OutputPortModel,
                };
                return true;
            }

            linkedNodesDependency = default;
            parent = default;
            return false;
        }

        public override bool GetPortCapacity(PortModel portModel, out PortCapacity capacity)
        {
            if (portModel.PortType == PortType.Execution)
                capacity = PortCapacity.Multi;
            else
            {
                Assert.AreEqual(portModel.PortType, PortType.Data);
                capacity = portModel.Direction == Direction.Input
                    ? PortCapacity.Single
                    : PortCapacity.Multi;
            }
            return true;
        }

        public override void RegisterReducers(Store store)
        {
            DragAndDropReducers.Register(store);
        }
    }
}
