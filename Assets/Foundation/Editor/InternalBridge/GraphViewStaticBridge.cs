using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.UIR;
using UnityEngine.Yoga;
using System;

namespace Unity.Modifier.GraphToolsFoundations.Bridge
{
    public static class GraphViewStaticBridge
    {
#if !UNITY_2020_1_OR_NEWER
        public static VisualElement Instantiate(this VisualTreeAsset vta)
        {
            return vta.CloneTree();
        }
#endif

        public static Color EditorPlayModeTint => UIElementsUtility.editorPlayModeTintColor;

        public static void ShowColorPicker(Action<Color> callback, Color initialColor, bool withAlpha)
        {
            ColorPicker.Show(callback, initialColor, withAlpha);
        }

        public static string SearchField(Rect position, string text)
        {
            return EditorGUI.SearchField(position, text);
        }

        public static float RoundToPixelGrid(float v)
        {
            return GUIUtility.RoundToPixelGrid(v);
        }

        public static bool IsLayoutManual(this VisualElement ve)
        {
            return ve.isLayoutManual;
        }

        public static void SetLayout(this VisualElement ve, Rect layout)
        {
            ve.layout = layout;
        }

        public static Rect GetRect(this VisualElement ve)
        {
            return new Rect(0.0f, 0.0f, ve.layout.width, ve.layout.height);
        }

        public static void SetCheckedPseudoState(this VisualElement ve, bool set)
        {
            if (set)
            {
                ve.pseudoStates |= PseudoStates.Checked;
            }
            else
            {
                ve.pseudoStates &= ~PseudoStates.Checked;
            }
        }

        public static void SetDisabledPseudoState(this VisualElement ve, bool set)
        {
            if (set)
            {
                ve.pseudoStates |= PseudoStates.Disabled;
            }
            else
            {
                ve.pseudoStates &= ~PseudoStates.Disabled;
            }
        }

        public static bool GetDisabledPseudoState(this VisualElement ve)
        {
            return (ve.pseudoStates & PseudoStates.Disabled) == PseudoStates.Disabled;
        }

        public static object GetProperty(this VisualElement ve, PropertyName key)
        {
            return ve.GetProperty(key);
        }

        public static void SetProperty(this VisualElement ve, PropertyName key, object value)
        {
            ve.SetProperty(key, value);
        }

        public static void ResetPositionProperties(this VisualElement ve)
        {
            ve.ResetPositionProperties();
        }

        public static MeshWriteData AllocateMeshWriteData(MeshGenerationContext mgc, int vertexCount, int indexCount)
        {
            return mgc.Allocate(vertexCount, indexCount, null, null, MeshGenerationContext.MeshFlags.UVisDisplacement);
        }

        public static void SetNextVertex(this MeshWriteData md, Vector3 pos, Vector2 uv, Color32 tint)
        {
            Color32 flags = new Color32(0, 0, 0, (byte)VertexFlags.LastType);
            md.SetNextVertex(new Vertex() { position = pos, uv = uv, tint = tint, idsFlags = flags });
        }

        static void MarkYogaNodeSeen(YogaNode node)
        {
            node.MarkLayoutSeen();

            for (int i = 0; i < node.Count; i++)
            {
                MarkYogaNodeSeen(node[i]);
            }
        }

        public static void MarkYogaNodeSeen(this VisualElement ve)
        {
            MarkYogaNodeSeen(ve.yogaNode);
        }

        public static void MarkYogaNodeDirty(this VisualElement ve)
        {
            ve.yogaNode.MarkDirty();
        }

        public static void ForceComputeYogaNodeLayout(this VisualElement ve)
        {
            ve.yogaNode.CalculateLayout();
        }

        public static bool IsBoundingBoxDirty(this VisualElement ve)
        {
            return ve.isBoundingBoxDirty;
        }

        public static void SetBoundingBoxDirty(this VisualElement ve)
        {
            ve.isBoundingBoxDirty = true;
        }

        public static void SetRequireMeasureFunction(this VisualElement ve)
        {
            ve.requireMeasureFunction = true;
        }

        public static StyleLength GetComputedStyleWidth(this VisualElement ve)
        {
            return ve.computedStyle.width;
        }

        public static void SetRenderHintsForGraphView(this VisualElement ve)
        {
            ve.renderHints = RenderHints.ClipWithScissors;
        }

        public static Vector2 GetWindowScreenPoint(this VisualElement ve)
        {
            GUIView guiView = ve.elementPanel.ownerObject as GUIView;
            if (guiView == null)
                return Vector2.zero;

            return guiView.screenPosition.position;
        }
    }

    public abstract class VisualElementBridge : VisualElement
    {
        protected virtual void OnGraphElementDataReady() { }

        internal override void OnViewDataReady()
        {
            base.OnViewDataReady();
            OnGraphElementDataReady();
        }

        protected new string GetFullHierarchicalViewDataKey()
        {
            return base.GetFullHierarchicalViewDataKey();
        }

        protected new T GetOrCreateViewData<T>(object existing, string key) where T : class, new()
        {
            return base.GetOrCreateViewData<T>(existing, key);
        }

        protected new void SaveViewData()
        {
            base.SaveViewData();
        }

        protected void DisplayContextualMenu(EventBase evt)
        {
            if (elementPanel != null && elementPanel.contextualMenuManager != null)
            {
                elementPanel.contextualMenuManager.DisplayMenuIfEventMatches(evt, this);
            }
        }

        public new uint controlid => base.controlid;
    }

    public abstract class GraphViewBridge : VisualElementBridge
    {
        protected static class EventCommandNames
        {
            public const string Cut = UnityEngine.EventCommandNames.Cut;
            public const string Copy = UnityEngine.EventCommandNames.Copy;
            public const string Paste = UnityEngine.EventCommandNames.Paste;
            public const string Duplicate = UnityEngine.EventCommandNames.Duplicate;
            public const string Delete = UnityEngine.EventCommandNames.Delete;
            public const string SoftDelete = UnityEngine.EventCommandNames.SoftDelete;
            public const string FrameSelected = UnityEngine.EventCommandNames.FrameSelected;
        }

        protected void UpdateDrawChainRegistration(bool register)
        {
            var p = panel as BaseVisualElementPanel;
            if (p != null)
            {
                UIRRepaintUpdater updater = p.GetUpdater(VisualTreeUpdatePhase.Repaint) as UIRRepaintUpdater;
                if (updater != null)
                {
                    if (register)
                        updater.BeforeDrawChain += OnBeforeDrawChain;
                    else
                        updater.BeforeDrawChain -= OnBeforeDrawChain;
                }
            }
        }

        static readonly int s_EditorPixelsPerPointId = Shader.PropertyToID("_EditorPixelsPerPoint");
        static readonly int s_GraphViewScaleId = Shader.PropertyToID("_GraphViewScale");

        public VisualElement contentViewContainer { get; protected set; }

        public ITransform viewTransform => contentViewContainer.transform;

        float scale => viewTransform.scale.x;

        public ActionOnDotNetUnhandledException redrawn { get; set; }

#if UNITY_2020_1_OR_NEWEER
        void OnBeforeDrawChain(RenderChain renderChain)
#else
        void OnBeforeDrawChain(UIRenderDevice renderChain)
#endif
        {
            Material mat = renderChain.GetStandardMaterial();
            // Set global graph view shader properties (used by UIR)
            mat.SetFloat(s_EditorPixelsPerPointId, EditorGUIUtility.pixelsPerPoint);
            mat.SetFloat(s_GraphViewScaleId, scale);
        }

        static Shader graphViewShader;

        protected void OnEnterPanel()
        {
            var p = panel as BaseVisualElementPanel;
            if (p != null)
            {
                if (graphViewShader == null)
                    graphViewShader = EditorGUIUtility.LoadRequired("GraphView/GraphViewUIE.shader") as Shader;
                p.standardShader = graphViewShader;
                HostView ownerView = p.ownerObject as HostView;
                if (ownerView != null && ownerView.actualView != null)
                    ownerView.actualView.antiAliasing = 4;

                // Changing the updaters is assumed not to be a normal use case, except maybe for Unity debugging
                // purposes. For that reason, we don't track updater changes.
                Panel.BeforeUpdaterChange += OnBeforeUpdaterChange;
                Panel.AfterUpdaterChange += OnAfterUpdaterChange;
                UpdateDrawChainRegistration(true);
            }

            // Force DefaultCommonDark.uss since GraphView only has a dark style at the moment
            UIElementsEditorUtility.ForceDarkStyleSheet(this);
        }

        protected void OnLeavePanel()
        {
            // ReSharper disable once DelegateSubtraction
            Panel.BeforeUpdaterChange -= OnBeforeUpdaterChange;
            // ReSharper disable once DelegateSubtraction
            Panel.AfterUpdaterChange -= OnAfterUpdaterChange;
            UpdateDrawChainRegistration(false);
        }

        void OnBeforeUpdaterChange()
        {
            UpdateDrawChainRegistration(false);
        }

        void OnAfterUpdaterChange()
        {
            UpdateDrawChainRegistration(true);
        }
    }

    public abstract class GraphViewEditorWindowBridge : EditorWindow
    {
    }
}
