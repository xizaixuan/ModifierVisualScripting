
using System.IO;

namespace UnityEditor.Modifier.VisualScripting.Model.Stencils
{
    public abstract class Stencil
    {
        int test;
        public virtual string GetSourceFilePath(VSGraphModel graphModel)
        {
            return Path.Combine(ModelUtility.GetAssemblyRelativePath(), graphModel.TypeName + ".cs");
        }
    }
}