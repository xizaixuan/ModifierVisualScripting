﻿namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public interface IMovable
    {
        void UpdatePinning();

        bool NeedStoreDispatch { get; }
    }
}