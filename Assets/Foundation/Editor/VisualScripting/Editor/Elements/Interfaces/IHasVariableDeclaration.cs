
using System.Collections.Generic;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    public interface IHasVariableDeclaration
    {
        IList<VariableDeclarationModel> VariableDeclarations { get; }
    }
}