using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    internal struct InsertInfo
    {
        public static readonly InsertInfo nil = new InsertInfo { target = null, index = -1, locationPosition = Vector2.zero};
        public VisualElement target;
        public int index;
        public Vector2 locationPosition;
    }

    internal interface IInsertLocation
    {
        void GetInsertInfo(Vector2 worldPosition, out InsertInfo insertInfo);
    }
}