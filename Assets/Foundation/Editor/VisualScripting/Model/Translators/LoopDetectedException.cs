using System;

namespace UnityEditor.Modifier.VisualScripting.Model.Translators
{
    public class LoopDetectedException : Exception
    {
        public LoopDetectedException(string message)
            : base(message) { }
    }
}