using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    public class VSGraphModel : GraphModel, IVSGraphModel
    {
        public string SourceFilePath => Stencil.GetSourceFilePath(this);

        public string TypeName => TypeSystem.CodifyString(AssetModel.Name);
    }
}