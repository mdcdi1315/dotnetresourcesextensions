// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Threading;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Diagnostics.CodeAnalysis;

namespace DotNetResourcesExtensions.Internal.AssemblyReader.MemoryBlocks
{
    /// <summary>
    /// Represents data read from a stream.
    /// </summary>
    /// <remarks>
    /// Uses memory map to load data from streams backed by files that are bigger than <see cref="MemoryMapThreshold"/>.
    /// </remarks>
    internal sealed class StreamMemoryBlockProvider : MemoryBlockProvider
    {
        // From System.IO.Stream.CopyTo:
        // We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
        // The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
        // improvement in Copy performance.
        internal const int StreamCopyBufferSize = 81920;

        // We're trying to balance total VM usage (which is a minimum of 64KB for a memory mapped file)
        // with private working set (since heap memory will be backed by the paging file and non-sharable).
        // Internal for testing.
        internal const int MemoryMapThreshold = 16 * 1024;

        // The stream is user specified and might not be thread-safe.
        // Any read from the stream must be protected by streamGuard.
        private Stream _stream;
        private readonly object _streamGuard;

        private readonly bool _leaveOpen;
        private bool _useMemoryMap;

        private readonly long _imageStart;
        private readonly int _imageSize;

        private MemoryMappedFile _lazyMemoryMap;

        public StreamMemoryBlockProvider(Stream stream, long imageStart, int imageSize, bool leaveOpen)
        {
            Debug.Assert(stream.CanSeek && stream.CanRead);
            _stream = stream;
            _streamGuard = new object();
            _imageStart = imageStart;
            _imageSize = imageSize;
            _leaveOpen = leaveOpen;
            _useMemoryMap = stream is FileStream;
        }

        protected override void Dispose(bool disposing)
        {
            Debug.Assert(disposing);
            if (!_leaveOpen)
            {
                Interlocked.Exchange(ref _stream, null!)?.Dispose();
            }

            Interlocked.Exchange(ref _lazyMemoryMap, null)?.Dispose();
        }

        public override int Size
        {
            get
            {
                return _imageSize;
            }
        }

        /// <exception cref="IOException">Error reading from the stream.</exception>
        internal static unsafe NativeHeapMemoryBlock ReadMemoryBlockNoLock(Stream stream, long start, int size)
        {
            static unsafe void CopyTo(Stream source, byte* destination, int size)
            {
                byte[] buffer = new byte[System.Math.Min(StreamCopyBufferSize, size)];
                while (size > 0)
                {
                    int readSize = System.Math.Min(size, buffer.Length);
                    int bytesRead = source.Read(buffer, 0, readSize);

                    if (bytesRead <= 0 || bytesRead > readSize)
                    {
                        throw new IOException("Unexpected end of stream has occured.");
                    }

                    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, (System.IntPtr)destination, bytesRead);

                    destination += bytesRead;
                    size -= bytesRead;
                }
            }

            var block = new NativeHeapMemoryBlock(size);
            bool fault = true;
            try
            {
                stream.Seek(start, SeekOrigin.Begin);

                CopyTo(stream, block.Pointer, size);

                fault = false;
            }
            finally
            {
                if (fault)
                {
                    block.Dispose();
                }
            }

            return block;
        }

        /// <exception cref="IOException">Error while reading from the stream.</exception>
        protected override AbstractMemoryBlock GetMemoryBlockImpl(int start, int size)
        {
            long absoluteStart = _imageStart + start;

            if (_useMemoryMap && size > MemoryMapThreshold)
            {
                if (TryCreateMemoryMappedFileBlock(absoluteStart, size, out MemoryMappedFileBlock block))
                {
                    return block;
                }

                _useMemoryMap = false;
            }

            lock (_streamGuard)
            {
                return ReadMemoryBlockNoLock(_stream!, absoluteStart, size);
            }
        }

        public override Stream GetStream(out StreamConstraints constraints)
        {
            constraints = new StreamConstraints(_streamGuard, _imageStart, _imageSize);
            return _stream;
        }

        /// <exception cref="IOException">IO error while mapping memory or not enough memory to create the mapping.</exception>
        private unsafe bool TryCreateMemoryMappedFileBlock(long start, int size, [NotNullWhen(true)] out MemoryMappedFileBlock block)
        {
            if (_lazyMemoryMap == null)
            {
                // leave the underlying stream open. It will be closed by the Dispose method.
                MemoryMappedFile newMemoryMap;

                // CreateMemoryMap might modify the stream (calls FileStream.Flush)
                lock (_streamGuard)
                {
                    try
                    {
                        newMemoryMap =
                            MemoryMappedFile.CreateFromFile(
                                fileStream: (FileStream)_stream,
                                mapName: null,
                                capacity: 0,
                                access: MemoryMappedFileAccess.Read,
                                inheritability: HandleInheritability.None,
                                leaveOpen: true);
                    }
                    catch (System.UnauthorizedAccessException e)
                    {
                        throw new IOException(e.Message, e);
                    }
                }

                if (newMemoryMap == null)
                {
                    block = null;
                    return false;
                }

                if (Interlocked.CompareExchange(ref _lazyMemoryMap, newMemoryMap, null) != null)
                {
                    newMemoryMap.Dispose();
                }
            }

            MemoryMappedViewAccessor accessor;

            lock (_streamGuard)
            {
                accessor = _lazyMemoryMap.CreateViewAccessor(start, size, MemoryMappedFileAccess.Read);
            }

            if (accessor == null)
            {
                block = null;
                return false;
            }

            block = new MemoryMappedFileBlock(accessor, accessor.SafeMemoryMappedViewHandle, accessor.PointerOffset, size);
            return true;
        }
    }
}
