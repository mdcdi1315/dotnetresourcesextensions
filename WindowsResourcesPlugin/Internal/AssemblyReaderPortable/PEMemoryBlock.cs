// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace DotNetResourcesExtensions.Internal.AssemblyReader
{
    using MemoryBlocks;

    internal readonly struct PEMemoryBlock
    {
        private readonly AbstractMemoryBlock _block;
        private readonly int _offset;

        internal PEMemoryBlock(AbstractMemoryBlock block, int offset = 0)
        {
            Debug.Assert(block != null);
            Debug.Assert(offset >= 0 && offset <= block.Size);

            _block = block;
            _offset = offset;
        }

        /// <summary>
        /// Pointer to the first byte of the block.
        /// </summary>
        public unsafe byte* Pointer => (_block != null) ? _block.Pointer + _offset : null;

        /// <summary>
        /// Length of the block.
        /// </summary>
        public int Length => _block?.Size - _offset ?? 0;

        /// <summary>
        /// Creates <see cref="BlobReader"/> for a blob spanning the entire block.
        /// </summary>
        public unsafe BlobReader GetReader()
        {
            return new BlobReader(Pointer, Length);
        }

        /// <summary>
        /// Creates <see cref="BlobReader"/> for a blob spanning a part of the block.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Specified range is not contained within the block.</exception>
        public unsafe BlobReader GetReader(int start, int length)
        {
           // BlobUtilities.ValidateRange(Length, start, length, nameof(length));
            return new BlobReader(Pointer + start, length);
        }

        /// <summary>
        /// Reads the content of the entire block into an array.
        /// </summary>
        public System.Byte[] GetContent() => _block?.GetContentUnchecked(_offset, Length) ?? System.Array.Empty<System.Byte>();

        /// <summary>
        /// Reads the content of a part of the block into an array.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Specified range is not contained within the block.</exception>
        public System.Byte[] GetContent(int start, int length)
        {
           // BlobUtilities.ValidateRange(Length, start, length, nameof(length));
            return _block?.GetContentUnchecked(_offset + start, length) ?? System.Array.Empty<System.Byte>();
        }
    }
}
