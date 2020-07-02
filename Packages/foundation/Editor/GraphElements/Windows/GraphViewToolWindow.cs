﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public abstract class GraphViewToolWindow : GraphViewToolWindowBridge
    {
        struct GraphViewChoice
        {
            public EditorWindow window;
            public GraphView graphView;
            public int idx;
            public bool canUse;
        }

        const string k_DefaultSelectorName = "Select a panel";

        UnityEditor.UIElements.Toolbar m_Toolbar;
        protected VisualElement m_ToolbarContainer;
        ToolbarMenu m_SelectorMenu;

        [SerializeField]
        EditorWindow m_SelectedWindow;

        [SerializeField]
        int m_SelectedGraphViewIdx;

        protected GraphView m_SelectedGraphView;
        List<GraphViewChoice> m_GraphViewChoices;

        bool m_FirstUpdate;

        protected abstract string ToolName { get; }

        public override IEnumerable<Type> GetExtraPaneTypes()
        {
            return Assembly
                .GetAssembly(typeof(GraphViewToolWindow))
                .GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(GraphViewToolWindow)));
        }

        public override void SelectGraphViewFromWindow(GraphViewEditorWindowBridge window, GraphViewBridge graphView, int graphViewIndexInWindow = 0)
        {
            var gvChoice = new GraphViewChoice { window = window, graphView = graphView as GraphView, idx = graphViewIndexInWindow };
            SelectGraphView(gvChoice);
        }

        protected void OnEnable()
        {
            var root = rootVisualElement;

            this.SetAntiAliasing(4);

            m_Toolbar = new UnityEditor.UIElements.Toolbar();

            // Register panel choice refresh on the toolbar so the event
            // is received before the ToolbarPopup clickable handle it.
            m_Toolbar.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.target == m_SelectorMenu)
                    RefreshPanelChoices();
            }, TrickleDown.TrickleDown);
            m_GraphViewChoices = new List<GraphViewChoice>();
            m_SelectorMenu = new ToolbarMenu { name = "panelSelectPopup", text = "Select a panel" };

            var menu = m_SelectorMenu.menu;
            menu.AppendAction("None", OnSelectGraphView,
                a => m_SelectedGraphView == null ?
                DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            m_Toolbar.Add(m_SelectorMenu);
            m_Toolbar.style.flexGrow = 1;
            m_Toolbar.style.overflow = Overflow.Hidden;
            m_ToolbarContainer = new VisualElement();
            m_ToolbarContainer.style.flexDirection = FlexDirection.Row;
            m_ToolbarContainer.Add(m_Toolbar);

            root.Add(m_ToolbarContainer);

            m_FirstUpdate = true;

            titleContent.text = ToolName;
        }

        void Update()
        {
            // We need to wait until all the windows are created to re-assign a potential graphView.
            if (m_FirstUpdate)
            {
                m_FirstUpdate = false;
                if (m_SelectedWindow != null)
                {
                    var graphViewEditor = m_SelectedWindow as GraphViewEditorWindow;
                    if (graphViewEditor != null && m_SelectedGraphViewIdx >= 0 && m_SelectedGraphView == null)
                    {
                        m_SelectedGraphView = graphViewEditor.graphViews.ElementAt(m_SelectedGraphViewIdx);
                        m_SelectorMenu.text = m_SelectedGraphView.name;
                        OnGraphViewChanged();
                    }
                }
            }
            else
            {
                if (!m_SelectedWindow && m_SelectedGraphView != null)
                    SelectGraphView(null);
            }

            UpdateGraphViewName();
        }

        void RefreshPanelChoices()
        {
            m_GraphViewChoices.Clear();

            var usedGraphViews = new HashSet<GraphView>();

            foreach (var toolWindow in GraphViewStaticBridge.GetGraphViewWindows<GraphViewToolWindow>(GetType()))
            {
                if (toolWindow.m_SelectedGraphView != null)
                {
                    usedGraphViews.Add(toolWindow.m_SelectedGraphView);
                }
            }

            foreach (var window in GraphViewStaticBridge.GetGraphViewWindows<GraphViewEditorWindow>(null))
            {
                int idx = 0;
                foreach (var graphView in window.graphViews.Where(IsGraphViewSupported))
                {
                    m_GraphViewChoices.Add(new GraphViewChoice { window = window, idx = idx++, graphView = graphView, canUse = !usedGraphViews.Contains(graphView) });
                }
            }

            var menu = m_SelectorMenu.menu;
            var menuItemsCount = menu.MenuItems().Count;

            // Clear previous items (but not the "none" one at the top of the list)
            for (int i = menuItemsCount - 1; i > 0; i--)
                menu.RemoveItemAt(i);

            foreach (var graphView in m_GraphViewChoices)
            {
                menu.AppendAction(graphView.graphView.name, OnSelectGraphView,
                    a =>
                    {
                        var gvc = (GraphViewChoice)a.userData;
                        return (gvc.graphView == m_SelectedGraphView
                            ? DropdownMenuAction.Status.Checked
                            : (gvc.canUse
                                ? DropdownMenuAction.Status.Normal
                                : DropdownMenuAction.Status.Disabled));
                    },
                    graphView);
            }
        }

        void OnSelectGraphView(DropdownMenuAction action)
        {
            var choice = (GraphViewChoice?)action.userData;
            var newlySelectedGraphView = choice?.graphView;
            if (newlySelectedGraphView == m_SelectedGraphView)
                return;

            SelectGraphView(choice);
        }

        void SelectGraphView(GraphViewChoice? choice)
        {
            OnGraphViewChanging();
            m_SelectedGraphView = choice?.graphView;
            m_SelectedWindow = choice?.window;
            m_SelectedGraphViewIdx = choice?.idx ?? -1;
            OnGraphViewChanged();
            UpdateGraphViewName();
        }

        // Called just before the change.
        protected abstract void OnGraphViewChanging();

        // Called just after the change.
        protected abstract void OnGraphViewChanged();

        protected virtual bool IsGraphViewSupported(GraphView gv)
        {
            return false;
        }

        void UpdateGraphViewName()
        {
            string updatedName = k_DefaultSelectorName;
            if (m_SelectedGraphView != null)
                updatedName = m_SelectedGraphView.name;

            if (m_SelectorMenu.text != updatedName)
                m_SelectorMenu.text = updatedName;
        }
    }
}