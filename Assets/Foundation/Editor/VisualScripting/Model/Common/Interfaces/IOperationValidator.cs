using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    public interface IOperationValidator
    {
        bool HasValidOperationForInput(IPortModel inputPort, TypeHandle typeHandle);
    }
}