using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    public struct StringReference : IEquatable<StringReference>
    {
        public enum Storage
        {
            None,
            Managed,
            Unmanaged32,
            Unmanaged64,
            Unmanaged128,
            Unmanaged512,
            Unmanaged4096
        }

        [SerializeField]
        internal int Index;

        [SerializeField]
        internal Storage StorageType;

        public bool IsUnmanaged => StorageType != Storage.None && StorageType != Storage.Managed;

        public StringReference(int index, Storage storage)
        {
            Index = index;
            StorageType = storage;
        }

        public bool Equals(StringReference other)
        {
            return Index == other.Index && StorageType == other.StorageType;
        }

        public override bool Equals(object obj)
        {
            return obj is StringReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Index;
        }
    }

    [StructLayout(LayoutKind.Explicit), Serializable]
    public struct Value : IEquatable<Value>
    {
        [FieldOffset(0)] public ValueType Type;
        [FieldOffset(sizeof(ValueType))] private bool _bool;
        [FieldOffset(sizeof(ValueType))] private int _int;
        [FieldOffset(sizeof(ValueType))] private float _float;
        [FieldOffset(sizeof(ValueType))] private float2 _float2;
        [FieldOffset(sizeof(ValueType))] private float3 _float3;
        [FieldOffset(sizeof(ValueType)), SerializeField] private float4 _float4;
        [FieldOffset(sizeof(ValueType))] private quaternion _quaternion;
        [FieldOffset(sizeof(ValueType))] private Entity _entity;
        [FieldOffset(sizeof(ValueType))] private StringReference _string;

        public static bool CanConvert(ValueType from, ValueType to, bool allowFloatToIntRounding)
        {
            // extracted from each property getter down this file
            if (from == to)
                return true;
            switch (to)
            {
                case ValueType.StringReference:
                case ValueType.Quaternion:
                case ValueType.Entity:
                case ValueType.Bool:
                    return false;
                case ValueType.Int:
                    return from == ValueType.Bool || (allowFloatToIntRounding && from == ValueType.Float);
                case ValueType.Float:
                    return from == ValueType.Int;
                case ValueType.Float2:
                case ValueType.Float3:
                case ValueType.Float4:
                    return from == ValueType.Int || from == ValueType.Float || from == ValueType.Float2 ||
                        from == ValueType.Float3 || from == ValueType.Float4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(to), to, null);
            }
        }

        public bool Bool { get { Assert.AreEqual(Type, ValueType.Bool); return _bool; } set { Type = ValueType.Bool; _bool = value; } }
        public int Int
        {
            get
            {
                switch (Type)
                {
                    case ValueType.Bool:
                        return _bool ? 1 : 0;
                    case ValueType.Float:
                        return (int)_float;
                    case ValueType.Int:
                        return _int;
                    default: throw new InvalidDataException();
                }
            }
            set { Type = ValueType.Int; _int = value; }
        }
        public float Float
        {
            get
            {
                switch (Type)
                {
                    case ValueType.Float:
                        return _float;
                    case ValueType.Int:
                        return _int;
                    default: throw new InvalidDataException();
                }
            }
            set { Type = ValueType.Float; _float = value; }
        }

        public float2 Float2
        {
            get
            {
                switch (Type)
                {
                    case ValueType.Int:
                        return _int;
                    case ValueType.Float:
                        return _float;
                    case ValueType.Float2:
                        return _float2;
                    case ValueType.Float3:
                        return _float3.xy;
                    case ValueType.Float4:
                        return _float4.xy;
                    default: throw new InvalidDataException();
                }
            }
            set
            {
                Type = ValueType.Float2;
                _float2 = value;
            }
        }

        public float3 Float3
        {
            get
            {
                switch (Type)
                {
                    case ValueType.Int:
                        return _int;
                    case ValueType.Float:
                        return _float;
                    case ValueType.Float2:
                        return new float3(_float2, 0);
                    case ValueType.Float3:
                        return _float3;
                    case ValueType.Float4:
                        return _float4.xyz;
                    default: throw new InvalidDataException();
                }
            }
            set
            {
                Type = ValueType.Float3;
                _float3 = value;
            }
        }

        public float4 Float4
        {
            get
            {
                switch (Type)
                {
                    case ValueType.Int:
                        return _int;
                    case ValueType.Float:
                        return _float;
                    case ValueType.Float2:
                        return new float4(_float2, 0, 0);
                    case ValueType.Float3:
                        return new float4(_float3, 0);
                    case ValueType.Float4:
                        return _float4;
                    default: throw new InvalidDataException();
                }
            }
            set
            {
                Type = ValueType.Float4;
                _float4 = value;
            }
        }

        public quaternion Quaternion { get { Assert.AreEqual(Type, ValueType.Quaternion); return _quaternion; } set { Type = ValueType.Quaternion; _quaternion = value; } }
        public Entity Entity { get { Assert.AreEqual(Type, ValueType.Entity); return _entity; } set { Type = ValueType.Entity; _entity = value; } }
        public StringReference StringReference { get { Assert.AreEqual(Type, ValueType.StringReference); return _string; } set { Type = ValueType.StringReference; _string = value; } }

        public static implicit operator Value(bool f)
        {
            return new Value { Bool = f };
        }

        public static implicit operator Value(int f)
        {
            return new Value { Int = f };
        }

        public static implicit operator Value(float f)
        {
            return new Value { Float = f };
        }

        public static implicit operator Value(float2 f)
        {
            return new Value { Float2 = f };
        }

        public static implicit operator Value(float3 f)
        {
            return new Value { Float3 = f };
        }

        public static implicit operator Value(float4 f)
        {
            return new Value { Float4 = f };
        }

        public static implicit operator Value(quaternion f)
        {
            return new Value { Quaternion = f };
        }

        public static implicit operator Value(Entity f)
        {
            return new Value { Entity = f };
        }

        public static implicit operator Value(StringReference f)
        {
            return new Value { StringReference = f };
        }

        public override string ToString()
        {
            switch (Type)
            {
                case ValueType.Unknown:
                    return ValueType.Unknown.ToString();
                case ValueType.Bool:
                    return Bool.ToString(CultureInfo.InvariantCulture);
                case ValueType.Int:
                    return Int.ToString(CultureInfo.InvariantCulture);
                case ValueType.Float:
                    return Float.ToString(CultureInfo.InvariantCulture);
                case ValueType.Float2:
                    return Float2.ToString();
                case ValueType.Float3:
                    return Float3.ToString();
                case ValueType.Float4:
                    return Float4.ToString();
                case ValueType.Quaternion:
                    return Quaternion.ToString();
                case ValueType.Entity:
                    return Entity.ToString();
                case ValueType.StringReference:
                    return _string.ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string ToPrettyString()
        {
            switch (Type)
            {
                case ValueType.Unknown:
                    return ValueType.Unknown.ToString();
                case ValueType.Bool:
                    return Bool.ToString(CultureInfo.InvariantCulture);
                case ValueType.Int:
                    return Int.ToString(CultureInfo.InvariantCulture);
                case ValueType.Float:
                    return Float.ToString("F2");
                case ValueType.Float2:
                    return Float2.ToString("F2", CultureInfo.InvariantCulture);
                case ValueType.Float3:
                    return Float3.ToString("F2", CultureInfo.InvariantCulture);
                case ValueType.Float4:
                    return Float4.ToString("F2", CultureInfo.InvariantCulture);
                case ValueType.Quaternion:
                    return Quaternion.ToString("F2", CultureInfo.InvariantCulture);
                case ValueType.Entity:
                    return Entity.ToString();
                case ValueType.StringReference:
                    return _string.ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool Equals(Value other)
        {
            if (Type != other.Type)
            {
                if (Type == ValueType.Float && other.Type == ValueType.Int || Type == ValueType.Int && other.Type == ValueType.Float)
                    return Float.Equals(other.Float);
            }
            switch (Type)
            {
                case ValueType.Unknown:
                    return false;
                case ValueType.Bool:
                    return Bool == other.Bool;
                case ValueType.Int:
                    return Int == other.Int;
                case ValueType.Float:
                    return Float.Equals(other.Float);
                case ValueType.Float2:
                    return Float2.Equals(other.Float2);
                case ValueType.Float3:
                    return Float3.Equals(other.Float3);
                case ValueType.Float4:
                    return Float4.Equals(other.Float4);
                case ValueType.Quaternion:
                    return Quaternion.Equals(other.Quaternion);
                case ValueType.Entity:
                    return Entity == other.Entity;
                case ValueType.StringReference:
                    return StringReference.Equals(other.StringReference);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Value other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Type;
        }

        public static unsafe Value FromPtr(void* voidPtr, ValueType type)
        {
            Assert.IsFalse(type == ValueType.StringReference, "String not handled");

            switch (type)
            {
                case ValueType.Bool:
                    return new Value { Bool = *(bool*)voidPtr };
                case ValueType.Int:
                    return new Value { Int = *(int*)voidPtr };
                case ValueType.Float:
                    return new Value { Float = *(float*)voidPtr };
                case ValueType.Float2:
                    return new Value { Float2 = *(float2*)voidPtr };
                case ValueType.Float3:
                    return new Value { Float3 = *(float3*)voidPtr };
                case ValueType.Float4:
                    return new Value { Float4 = *(float4*)voidPtr };
                case ValueType.Quaternion:
                    return new Value { Quaternion = *(quaternion*)voidPtr };
                case ValueType.Entity:
                    return new Value { Entity = *(Entity*)voidPtr };
            }
            return new Value();
        }

        public static unsafe void SetPtrToValue(void* voidPtr, ValueType type, Value setValue)
        {
            Assert.IsFalse(type == ValueType.StringReference, "String not handled");

            if (type == setValue.Type)
            {
                switch (type)
                {
                    case ValueType.Bool:
                        *(bool*)voidPtr = setValue.Bool;
                        break;
                    case ValueType.Int:
                        *(int*)voidPtr = setValue.Int;
                        break;
                    case ValueType.Float:
                        *(float*)voidPtr = setValue.Float;
                        break;
                    case ValueType.Float2:
                        *(float2*)voidPtr = setValue.Float2;
                        break;
                    case ValueType.Float3:
                        *(float3*)voidPtr = setValue.Float3;
                        break;
                    case ValueType.Float4:
                        *(float4*)voidPtr = setValue.Float4;
                        break;
                    case ValueType.Quaternion:
                        *(quaternion*)voidPtr = setValue.Quaternion;
                        break;
                    case ValueType.Entity:
                        *(Entity*)voidPtr = setValue.Entity;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
        }
    }

    [Serializable]
    public enum ValueType : byte
    {
        Unknown,
        Bool,
        Int,
        Float,
        Float2,
        Float3,
        Float4,
        Quaternion,
        Entity,
        StringReference,
    }

    public static class ValueTypeExtensions
    {
        public static string FriendlyName(this ValueType self)
        {
            switch (self)
            {
                case ValueType.Bool: return "Boolean";
                case ValueType.Int: return "Integer";
                case ValueType.Entity: return "GameObject";
                case ValueType.StringReference: return "String";

                case ValueType.Unknown:
                    return "Unknown";
                case ValueType.Float2:
                    return "Vector 2";
                case ValueType.Float3:
                    return "Vector 3";
                case ValueType.Float4:
                    return "Vector 4";
                case ValueType.Quaternion:
                default: return self.ToString();
            }
        }
    }
}