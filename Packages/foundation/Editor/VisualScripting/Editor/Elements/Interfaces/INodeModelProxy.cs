using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    internal interface INodeModelProxy
    {
        ScriptableObject ScriptableObject();
        void SetModel(IGraphElementModel model);
    }
}