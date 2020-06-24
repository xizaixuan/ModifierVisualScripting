using JetBrains.Annotations;

namespace Modifier.Runtime
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public enum InterpolationType : byte
    {
        Linear,
        SmoothStep
    }
}
