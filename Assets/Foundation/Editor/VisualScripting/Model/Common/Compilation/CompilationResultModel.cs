
namespace UnityEditor.Modifier.VisualScripting.Model
{
    public class CompilationResultModel : ICompilationResultModel
    {
        public CompilationResult lastResult;

        public CompilationResult GetLastResult()
        {
            return lastResult;
        }
    }
}