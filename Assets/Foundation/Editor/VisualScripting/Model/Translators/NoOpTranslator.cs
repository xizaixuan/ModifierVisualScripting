using System;
using UnityEngine.Modifier.VisualScripting;

namespace UnityEditor.Modifier.VisualScripting.Model.Translators
{
    public class NoOpTranslator : ITranslator
    {
        public bool SupportsCompilation() => false;
        public CompilationResult TranslateAndCompile(VSGraphModel graphModel, AssemblyType assemblyType, CompilationOptions compilationOptions)
        {
            throw new NotImplementedException();
        }
    }
}