using System;
using System.Collections.Generic;
using System.Linq;

namespace Modifier.Runtime
{
    public struct Ref<T> : IEquatable<Ref<T>> where T : unmanaged, IEquatable<T>
    {
        public readonly T Value;
        public int RefCount;

        public Ref(T value)
        {
            Value = value;
            RefCount = 0;
        }

        public static bool operator ==(Ref<T> lhs, Ref<T> rhs)
        {
            return lhs.Value.Equals(rhs.Value) && lhs.RefCount == rhs.RefCount;
        }

        public static bool operator !=(Ref<T> lhs, Ref<T> rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(Ref<T> other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is Ref<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Value.GetHashCode() * 397) ^ RefCount;
            }
        }
    }

    public class RefPool<T> where T : unmanaged, IEquatable<T>
    {
        List<Ref<T>> m_Pool = new List<Ref<T>>();
        Queue<int> m_RecyclableIndices = new Queue<int>();

        public Ref<T> this[int i]
        {
            get => m_Pool[i];
            set
            {
                m_Pool[i] = value;
                if (value.RefCount <= 0)
                    m_RecyclableIndices.Enqueue(i);
            }
        }

        public int Add(Ref<T> value)
        {
            if (m_RecyclableIndices.Any())
            {
                var index = m_RecyclableIndices.Dequeue();
                m_Pool[index] = value;
                return index;
            }
            else
            {
                var index = m_Pool.Count;
                m_Pool.Add(value);
                return index;
            }
        }
    }
}