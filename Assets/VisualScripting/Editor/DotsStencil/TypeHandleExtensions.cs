using System;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using ValueType = Modifier.Runtime.ValueType;

namespace Modifier.DotsStencil
{
    public static class TypeHandleExtensions
    {
        public static ValueType ToValueType(this TypeHandle handle)
        {
            if (handle.ToValueType(out var typeHandleToValueType))
                return typeHandleToValueType;
            throw new ArgumentOutOfRangeException(nameof(handle), "Unknown TypeHandle");
        }

        public static ValueType ToValueTypeOrUnknown(this TypeHandle handle)
        {
            return handle.ToValueType(out var typeHandleToValueType) ? typeHandleToValueType : ValueType.Unknown;
        }

        public static bool ToValueType(this TypeHandle handle, out ValueType typeHandleToValueType)
        {
            typeHandleToValueType = ValueType.Unknown;

            if (handle == TypeHandle.Unknown)
            {
                typeHandleToValueType = ValueType.Unknown;
                return true;
            }

            if (handle == TypeHandle.Bool)
            {
                typeHandleToValueType = ValueType.Bool;
                return true;
            }

            if (handle == TypeHandle.Int)
            {
                typeHandleToValueType = ValueType.Int;
                return true;
            }

            if (handle == TypeHandle.Float)
            {
                typeHandleToValueType = ValueType.Float;
                return true;
            }

            if (handle == TypeHandle.Vector2 || handle == DotsTypeHandle.Float2)
            {
                typeHandleToValueType = ValueType.Float2;
                return true;
            }

            if (handle == TypeHandle.Vector3 || handle == DotsTypeHandle.Float3)
            {
                typeHandleToValueType = ValueType.Float3;
                return true;
            }

            if (handle == TypeHandle.Vector4 || handle == DotsTypeHandle.Float4)
            {
                typeHandleToValueType = ValueType.Float4;
                return true;
            }

            if (handle == TypeHandle.Quaternion || handle == DotsTypeHandle.Quaternion)
            {
                typeHandleToValueType = ValueType.Quaternion;
                return true;
            }

            if (handle == TypeHandle.GameObject || handle == DotsTypeHandle.Entity)
            {
                typeHandleToValueType = ValueType.Entity;
                return true;
            }

            if (handle == TypeHandle.String || handle.IsNativeString())
            {
                typeHandleToValueType = ValueType.StringReference;
                return true;
            }

            return false;
        }

        static bool IsNativeString(this TypeHandle handle)
        {
            return handle == DotsTypeHandle.NativeString32
                || handle == DotsTypeHandle.NativeString64
                || handle == DotsTypeHandle.NativeString128
                || handle == DotsTypeHandle.NativeString512
                || handle == DotsTypeHandle.NativeString4096;
        }
    }
}
