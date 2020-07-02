using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Modifier.Runtime
{
    [NativeContainer]
    public struct VisualScriptingEventStream : IDisposable
    {
        public struct Writer
        {
            NativeStream.Writer m_Writer;

            public Writer(ref NativeStream stream)
            {
                m_Writer = stream.AsWriter();
            }

            public void Write<T>(T evt, int index) where T : struct, IVisualScriptingEvent
            {
                m_Writer.BeginForEachIndex(index);
                m_Writer.Write(TypeHash.CalculateStableTypeHash(typeof(T)));
                m_Writer.Write(UnsafeUtility.SizeOf<T>());
                m_Writer.Write(false);
                m_Writer.Write(evt);
                m_Writer.EndForEachIndex();
            }

            internal unsafe byte* Allocate(ulong hash, int size, bool isFromGraph, int index)
            {
                m_Writer.BeginForEachIndex(index);
                m_Writer.Write(hash);
                m_Writer.Write(size);
                m_Writer.Write(isFromGraph);
                var ptr = m_Writer.Allocate(size);
                m_Writer.EndForEachIndex();
                return ptr;
            }
        }

        NativeStream m_Stream;

        public VisualScriptingEventStream(int forEachCount)
        {
            m_Stream = new NativeStream(forEachCount, Allocator.Persistent);
        }

        public Writer AsWriter()
        {
            return new Writer(ref m_Stream);
        }

        public NativeStream.Reader AsReader()
        {
            return m_Stream.AsReader();
        }

        public void Dispose()
        {
            if (m_Stream.IsCreated)
                m_Stream.Dispose();
        }
    }
}
