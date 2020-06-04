﻿using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    [Flags]
    public enum ResizerDirection
    {
        Top = 1 << 0,
        Bottom = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
    }

    public class ResizableElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ResizableElement> {}

        public ResizableElement() : this("Resizable.uxml")
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("resizableElement");
        }

        public ResizableElement(string uiFile)
        {
            var tpl = Resources.Load<VisualTreeAsset>(uiFile);
            if (tpl == null)
                tpl = GraphElementsHelper.LoadUXML(uiFile);

            this.AddStylesheet("Resizable.uss");

            tpl.CloneTree(this);

            foreach (ResizerDirection value in System.Enum.GetValues(typeof(ResizerDirection)))
            {
                VisualElement resizer = this.Q(value.ToString().ToLower() + "-resize");
                if (resizer != null)
                    resizer.AddManipulator(new ElementResizer(this, value));
            }

            foreach (ResizerDirection vertical in new[] { ResizerDirection.Top, ResizerDirection.Bottom })
                foreach (ResizerDirection horizontal in new[] { ResizerDirection.Left, ResizerDirection.Right })
                {
                    VisualElement resizer = this.Q(vertical.ToString().ToLower() + "-" + horizontal.ToString().ToLower() + "-resize");
                    if (resizer != null)
                        resizer.AddManipulator(new ElementResizer(this, vertical | horizontal));
                }
        }
    }
}