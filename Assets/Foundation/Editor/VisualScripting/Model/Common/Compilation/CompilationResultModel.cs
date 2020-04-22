
namespace UnityEditor.Modifier.VisualScripting.Model
{
    public class CompilationResultModel : ICompilationResultModel
    {
        public CompilationResultModel lastResult;

        public CompilationResultModel GetLastResult()
        {
            return lastResult;
        }
    }
}