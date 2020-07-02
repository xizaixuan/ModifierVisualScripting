using System.Collections.Generic;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    public interface IVSGraphModel : IGraphModel, IHasVariableDeclaration
    {
        IEnumerable<IStackModel> StackModels { get; }
        IEnumerable<IVariableDeclarationModel> GraphVariableModels { get; }

        IEnumerable<IVariableDeclarationModel> GraphPortalModels { get; }
    }
}