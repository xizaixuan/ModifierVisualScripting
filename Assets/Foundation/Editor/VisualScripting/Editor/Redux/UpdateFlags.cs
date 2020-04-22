namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public enum UpdateFlags
    {
        None                = 0,
        Selection           = 1 << 0,

        All = Selection,
    }
}