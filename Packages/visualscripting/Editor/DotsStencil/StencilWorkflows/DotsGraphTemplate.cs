using System;
using System.Collections.Generic;
using Modifier.Runtime;
using Unity.Entities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modifier.DotsStencil
{
    public class DotsGraphTemplate : ICreatableGraphTemplate
    {
        public static readonly string k_DefaultGraphName = "Scripting Graph";
        public static readonly string k_DefaultSubGraphName = "Scripting Subgraph";

        public static DotsGraphTemplate ObjectGraphFromSelection(GameObject graphHolderParent, IEnumerable<GameObject> gameObjects)
        {
            return new DotsGraphTemplate(DotsStencil.GraphType.Object)
            {
                m_AttachToGameObject = true,
                m_GraphHolderParent = graphHolderParent,
                m_GameObjects = gameObjects != null ? new List<GameObject>(gameObjects) : null,
            };
        }

        public static DotsGraphTemplate ObjectGraphAsset()
        {
            return new DotsGraphTemplate(DotsStencil.GraphType.Object)
            {
                m_AttachToGameObject = false,
                m_GraphHolderParent = null,
                m_GameObjects = null,
            };
        }

        public static DotsGraphTemplate SubGraphAsset()
        {
            return new DotsGraphTemplate(DotsStencil.GraphType.Subgraph)
            {
                m_AttachToGameObject = false,
                m_GraphHolderParent = null,
                m_GameObjects = null,
            };
        }

        private readonly DotsStencil.GraphType m_GraphType;
        bool m_AttachToGameObject;
        GameObject m_GraphHolderParent;
        List<GameObject> m_GameObjects;

        private DotsGraphTemplate(DotsStencil.GraphType mGraphType)
        {
            m_GraphType = mGraphType;
        }

        public Type StencilType => typeof(DotsStencil);

        public string GraphTypeName => m_GraphType == DotsStencil.GraphType.Object ? k_DefaultGraphName : k_DefaultSubGraphName;

        public string DefaultAssetName => GraphTypeName;

        public void InitBasicGraph(VSGraphModel graphModel)
        {
            var dotsStencil = (DotsStencil)graphModel.Stencil;
            dotsStencil.Type = m_GraphType;
            if (dotsStencil.CompiledScriptingGraphAsset == null)
            {
                CreateDotsCompiledScriptingGraphAsset(graphModel);
            }

            if (m_AttachToGameObject)
            {
                var graphAssetModel = graphModel.AssetModel;
                var graphHolder = ObjectFactory.CreateGameObject(graphAssetModel.Name);
                Place(graphHolder, m_GraphHolderParent);

                AddScriptingGraphToObject((VSGraphAssetModel)graphModel.AssetModel, graphHolder);

                var authoringComponent = graphHolder.GetComponent<ScriptingGraphAuthoring>();

                if (m_GameObjects != null)
                {
                    Vector2 position = new Vector2(10, 10);
                    DotsStencil.CreateVariablesFromGameObjects(graphModel, authoringComponent, m_GameObjects, position, false);
                }
            }
        }

        static void Place(GameObject go, GameObject parent)
        {
            if (parent != null)
            {
                var transform = go.transform;
                Undo.SetTransformParent(transform, parent.transform, "Reparenting");
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
                go.layer = parent.layer;

                if (parent.GetComponent<RectTransform>())
                    ObjectFactory.AddComponent<RectTransform>(go);
            }
            else
            {
                go.transform.position = Vector3.zero;
                StageUtility.PlaceGameObjectInCurrentStage(go); // may change parent
            }

            // Only at this point do we know the actual parent of the object and can modify its name accordingly.
            GameObjectUtility.EnsureUniqueNameForSibling(go);
            Undo.SetCurrentGroupName("Create " + go.name);

            Selection.activeGameObject = go;
        }

        internal static void CreateDotsCompiledScriptingGraphAsset(IGraphModel graphModel)
        {
            DotsStencil stencil = (DotsStencil)graphModel.Stencil;
            stencil.CompiledScriptingGraphAsset = ScriptableObject.CreateInstance<ScriptingGraphAsset>();
            AssetDatabase.AddObjectToAsset(stencil.CompiledScriptingGraphAsset, (Object)graphModel.AssetModel);
        }

        static void AddScriptingGraphToObject(IGraphAssetModel graphAssetModel, GameObject target)
        {
            if (target.GetComponent<ConvertToEntity>() == null)
            {
                target.AddComponent<ConvertToEntity>();
            }

            ScriptingGraphAuthoring authoring = target.GetComponent<ScriptingGraphAuthoring>();
            if (authoring == null)
            {
                authoring = target.AddComponent<ScriptingGraphAuthoring>();
            }

            authoring.ScriptingGraph =
                ((DotsStencil)(graphAssetModel as GraphAssetModel)?.GraphModel.Stencil)?.CompiledScriptingGraphAsset;
        }
    }
}
