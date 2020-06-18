using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    public interface IVariableModel : IHasMainOutputPort
    {
        IVariableDeclarationModel DeclarationModel { get; }
    }
}