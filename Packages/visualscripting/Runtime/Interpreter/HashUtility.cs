using System;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Modifier.Runtime
{
    static class HashUtility
    {
        public static uint HashCollection<T>(IList<T> coll, Func<T, uint, uint> hashFunction, uint hash)
        {
            for (var index = 0; index < coll.Count; index++)
                hash = hashFunction(coll[index], hash);

            return hash;
        }

        public static unsafe uint HashUnmanagedStruct<T>(T obj, uint seed) where T : unmanaged
        {
            void* ptr = &obj;
            seed = math.hash((byte*)ptr, UnsafeUtility.SizeOf(obj.GetType()), seed);
            return seed;
        }

        /// <summary>
        /// Hash a struct that has been boxed. Given a struct Node : INode, this allows to hash a value of static type INode and actual type Node. The actual type of obj must be unmanaged
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="seed"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static unsafe uint HashBoxedUnmanagedStruct<T>(T obj, uint seed)
        {
            const int managedObjectHeaderSize = 16;
            Assert.IsTrue(UnsafeUtility.IsUnmanaged(obj.GetType()), $"Type {obj.GetType().Name} is managed");

            var ptr = UnsafeUtility.PinGCObjectAndGetAddress(obj, out var handle);
            seed = math.hash((byte*)ptr + managedObjectHeaderSize, UnsafeUtility.SizeOf(obj.GetType()), seed);
            UnsafeUtility.ReleaseGCObject(handle);
            return seed;
        }

        /// <summary>
        /// Hash anything else. Relies on object.GetHashCode()
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="seed"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static uint HashManaged<T>(T obj, uint seed)
        {
            return (uint)((obj?.GetHashCode() ?? 0) ^ (seed * 11));
        }
    }
}