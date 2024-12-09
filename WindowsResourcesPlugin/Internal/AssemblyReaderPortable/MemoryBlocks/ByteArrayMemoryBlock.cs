// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace DotNetResourcesExtensions.Internal.AssemblyReader.MemoryBlocks
{ 
    /// <summary>
    /// Represents a memory block backed by an array of bytes.
    /// </summary>
    internal sealed class ByteArrayMemoryBlock : AbstractMemoryBlock
    {
        private ByteArrayMemoryProvider _provider;
        private readonly int _start;
        private readonly int _size;

        internal ByteArrayMemoryBlock(ByteArrayMemoryProvider provider, int start, int size)
        {
            _provider = provider;
            _size = size;
            _start = start;
        }

        public override void Dispose()
        {
            _provider = null!;
        }

        public override unsafe byte* Pointer => _provider.Pointer + _start;
        public override int Size => _size;

        public override System.Byte[] GetContentUnchecked(int start, int length)
        {
            System.Byte[] ret = new System.Byte[length];
            System.Array.Copy(_provider.Array, _start + start , ret, start, length);
            return ret;
        }
    }
}
