using System;

namespace Modifier.Runtime
{
    [Serializable]
    public struct FieldDescription
    {
        public ulong DeclaringTypeHash;
        public ValueType FieldValueType;
        public int Offset;
        public StringReference.Storage Storage;
    }
}