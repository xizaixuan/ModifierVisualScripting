using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.UIR;

namespace Unity.Modifier.GraphToolsFoundations.Bridge
{
    public abstract class GraphViewEditorWindowBridge : EditorWindow
    {
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
}
