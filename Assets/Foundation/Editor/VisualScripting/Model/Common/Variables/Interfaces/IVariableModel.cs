using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    public interface IVariableModel : IHasMainOutputPort, IHasSingleInputPort, IHasSingleOutputPort
    {
        IVariableDeclarationModel DeclarationModel { get; }
    }
}