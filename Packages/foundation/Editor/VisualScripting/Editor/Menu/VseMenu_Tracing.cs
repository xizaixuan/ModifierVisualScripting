using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEditor.Searcher;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    partial class VseMenu
    {
        public Action<ChangeEvent<bool>> OnToggleTracing;

        ToolbarToggle m_EnableTracingButton;

        void CreateTracingMenu()
        {
            m_EnableTracingButton = this.MandatoryQ<ToolbarToggle>("enableTracingButton");
            m_EnableTracingButton.tooltip = "Toggle Tracing For Current Instance";
            m_EnableTracingButton.SetValueWithoutNotify(m_Store.GetState().EditorDataModel.TracingEnabled);
            m_EnableTracingButton.RegisterValueChangedCallback(e => OnToggleTracing?.Invoke(e));
        }
    }
}