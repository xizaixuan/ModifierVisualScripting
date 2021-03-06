﻿using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    static class ToolbarExtensions
    {
        public static void CreateOrUpdateItem(this ToolbarBreadcrumbs breadcrumbs, int index, string itemLabel, Action<int> clickedEvent)
        {
            if (index >= breadcrumbs.childCount)
            {
                breadcrumbs.PushItem(itemLabel, () =>
                {
                    int i = index;
                    clickedEvent?.Invoke(i);
                });
            }
            else
            {
                if (breadcrumbs.ElementAt(index) is ToolbarButton item)
                {
                    item.text = itemLabel;
                }
                else
                {
                    Debug.LogError("Trying to update an element that is not a ToolbarButton");
                }
            }
        }

        public static void TrimItems(this ToolbarBreadcrumbs breadcrumbs, int countToKeep)
        {
            while (breadcrumbs.childCount > countToKeep)
                breadcrumbs.PopItem();
        }

        public static void ChangeClickEvent(this ToolbarButton button, Action newClickEvent)
        {
            button.RemoveManipulator(button.clickable);
            button.clickable = new Clickable(newClickEvent);
            button.AddManipulator(button.clickable);
        }
    }
}