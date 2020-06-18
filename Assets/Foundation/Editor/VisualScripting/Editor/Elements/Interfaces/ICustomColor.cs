using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public interface ICustomColor
    {
        void ResetColor();

        void SetColor(Color c);
    }
}