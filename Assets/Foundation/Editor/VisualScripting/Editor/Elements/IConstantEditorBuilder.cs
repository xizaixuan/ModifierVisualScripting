using System;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor.ConstantEditor
{
    public interface IConstantEditorBuilder
    {
        Action<IChangeEvent> OnValueChanged { get; }
    }
}