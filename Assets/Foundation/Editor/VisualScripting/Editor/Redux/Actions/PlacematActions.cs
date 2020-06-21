using System.Collections.Generic;
using System.Linq;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class CreatePlacematAction : IAction
    {
        public string Title;
        public Rect Position;

        public CreatePlacematAction(string title, Rect position)
        {
            Title = title;
            Position = position;
        }
    }
}