// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace DotNetResourcesExtensions.Internal.AssemblyReader.MemoryBlocks
{
    /// <summary>
    /// Represents a disposable blob of memory accessed via unsafe pointer.
    /// </summary>
    internal abstract class AbstractMemoryBlock : System.IDisposable
    {
        /// <summary>
        /// Pointer to the underlying data (not valid after disposal).
        /// </summary>
        public abstract unsafe byte* Pointer { get; }

        /// <summary>
        /// Size of the block.
        /// </summary>
        public abstract int Size { get; }

        public unsafe BlobReader GetReader() => new BlobReader(Pointer, Size);

        /// <summary>
        /// Returns the content of the entire memory block.
        /// </summary>
        /// <remarks>
        /// Does not check bounds.
        ///
        /// Only creates a copy of the data if they are not represented by a managed byte array,
        /// or if the specified range doesn't span the entire block.
        /// </remarks>
        public virtual unsafe System.Byte[] GetContentUnchecked(int start, int length)
        {
            var result = new System.ReadOnlySpan<byte>(Pointer + start, length);
            System.GC.KeepAlive(this);
            return result.ToArray();
        }

        /// <summary>
        /// Disposes the block.
        /// </summary>
        /// <remarks>
        /// The operation is idempotent, but must not be called concurrently with any other operations on the block.
        ///
        /// Using the block after dispose is an error in our code and therefore no effort is made to throw a tidy
        /// ObjectDisposedException and null ref or AV is possible.
        /// </remarks>
        public abstract void Dispose();
    }
}
