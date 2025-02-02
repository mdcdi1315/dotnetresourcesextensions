// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Threading;
using System.Diagnostics;

namespace DotNetResourcesExtensions.Internal.AssemblyReader.MemoryBlocks
{
    internal sealed class ByteArrayMemoryProvider : MemoryBlockProvider
    {
        private readonly System.Byte[] _array;
        private PinnedObject _pinned;

        public ByteArrayMemoryProvider(System.Byte[] array)
        {
            Debug.Assert(array is not null);
            _array = array;
        }

        protected override void Dispose(bool disposing)
        {
            Debug.Assert(disposing);
            Interlocked.Exchange(ref _pinned, null)?.Dispose();
        }

        public override int Size => _array.Length;

        public System.Byte[] Array => _array;

        protected override AbstractMemoryBlock GetMemoryBlockImpl(int start, int size)
        {
            return new ByteArrayMemoryBlock(this, start, size);
        }

        public override Stream GetStream(out StreamConstraints constraints)
        {
            constraints = new StreamConstraints(null, 0, Size);
            return new ImmutableMemoryStream(_array);
        }

        internal unsafe byte* Pointer
        {
            get
            {
                if (_pinned == null)
                {
                    var newPinned = new PinnedObject(_array);

                    if (Interlocked.CompareExchange(ref _pinned, newPinned, null) != null)
                    {
                        // another thread has already allocated the handle:
                        newPinned.Dispose();
                    }
                }

                return _pinned.Pointer;
            }
        }
    }
}
