using JetBrains.Annotations;

namespace Modifier.Runtime.Nodes
{
    public interface IHasExecutionType<T>
    {
        [UsedImplicitly]
        T Type { get; set; }
    }
}
