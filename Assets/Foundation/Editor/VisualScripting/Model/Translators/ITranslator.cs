using UnityEngine.Modifier.VisualScripting;

namespace UnityEditor.Modifier.VisualScripting.Model.Translators
{
    public interface ITranslator
    {
        bool SupportsCompilation();
        CompilationResult TranslateAndCompile(VSGraphModel graphModel, AssemblyType assemblyType, CompilationOptions compilationOptions);
    }
}