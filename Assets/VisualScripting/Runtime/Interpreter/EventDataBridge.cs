using System.Collections.Generic;
using Unity.Collections;

namespace Modifier.Runtime
{
    static class EventDataBridge
    {
        public static List<NativeString128> NativeStrings128 { get; } = new List<NativeString128>();
    }
}
