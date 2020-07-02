
namespace UnityEditor.Modifier.EditorCommon.Redux
{
    public delegate TState Reducer<TState, in TAction>(TState previousState, TAction action) where TAction : IAction;
}