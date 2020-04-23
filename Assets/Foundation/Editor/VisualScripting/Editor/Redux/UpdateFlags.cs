using System;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    [Flags]
    public enum UpdateFlags
    {
        None                = 0,
        Selection           = 1 << 0,

        All = Selection,
    }
}