using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;

namespace Modifier.DotsStencil
{
    public static class DotsTypeHandle
    {
        public static TypeHandle Float2 { get; } = CSharpTypeSerializer.GenerateTypeHandle<float2>();
        public static TypeHandle Float3 { get; } = CSharpTypeSerializer.GenerateTypeHandle<float3>();
        public static TypeHandle Float4 { get; } = CSharpTypeSerializer.GenerateTypeHandle<float4>();
        public static TypeHandle Quaternion { get; } = CSharpTypeSerializer.GenerateTypeHandle<quaternion>();
        public static TypeHandle Entity { get; } = CSharpTypeSerializer.GenerateTypeHandle<Entity>();
        public static TypeHandle NativeString32 { get; } = CSharpTypeSerializer.GenerateTypeHandle<NativeString32>();
        public static TypeHandle NativeString64 { get; } = CSharpTypeSerializer.GenerateTypeHandle<NativeString64>();
        public static TypeHandle NativeString128 { get; } = CSharpTypeSerializer.GenerateTypeHandle<NativeString128>();
        public static TypeHandle NativeString512 { get; } = CSharpTypeSerializer.GenerateTypeHandle<NativeString512>();
        public static TypeHandle NativeString4096 { get; } = CSharpTypeSerializer.GenerateTypeHandle<NativeString4096>();
    }
}