using System;

namespace Unity.Modifier.GraphElements
{
    [Flags]
    public enum Capabilities
    {
        Selectable = 1 << 0,
        Collapsible = 1 << 1,
        Resizable = 1 << 2,
        Movable = 1 << 3,
        Deletable = 1 << 4,
        Droppable = 1 << 5,
        Ascendable = 1 << 6,
        Renamable = 1 << 7,
        Copiable = 1 << 8,
    }

    internal enum ResizeRestriction
    {
        None,
        FlexDirection
    }
}