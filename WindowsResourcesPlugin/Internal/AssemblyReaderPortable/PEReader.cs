// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace DotNetResourcesExtensions.Internal.AssemblyReader
{
    using MemoryBlocks;

    /// <summary>
    /// Portable Executable format reader.
    /// </summary>
    /// <remarks>
    /// The implementation is thread-safe, that is multiple threads can read data from the reader in parallel.
    /// Disposal of the reader is not thread-safe (see <see cref="Dispose"/>).
    /// </remarks>
    internal sealed partial class PEReader : IDisposable
    {
        /// <summary>
        /// True if the PE image has been loaded into memory by the OS loader.
        /// </summary>
        public bool IsLoadedImage { get; }

        // May be null in the event that the entire image is not
        // deemed necessary and we have been instructed to read
        // the image contents without being lazy.
        //
        // _lazyPEHeaders are not null in that case.
        private MemoryBlockProvider _peImage;

        // If we read the data from the image lazily (peImage != null) we defer reading the PE headers.
        private PEHeaders _lazyPEHeaders;

        private AbstractMemoryBlock _lazyMetadataBlock;
        private AbstractMemoryBlock _lazyImageBlock;
        private AbstractMemoryBlock[] _lazyPESectionBlocks;

        /// <summary>
        /// Resolve image size as either the given user-specified size or distance from current position to end-of-stream.
        /// Also performs the relevant argument validation and publicly visible caller has same argument names.
        /// </summary>
        /// <exception cref="ArgumentException">size is 0 and distance from current position to end-of-stream can't fit in Int32.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Size is negative or extends past the end-of-stream from current position.</exception>
        internal static int GetAndValidateSize(Stream stream, int size, string streamParameterName)
        {
            long maxSize = stream.Length - stream.Position;

            if (size < 0 || size > maxSize)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if (size != 0)
            {
                return size;
            }

            if (maxSize > int.MaxValue)
            {
                throw new ArgumentException("The stream is too large so as to be a PE image.", streamParameterName);
            }

            return (int)maxSize;
        }

        /// <summary>
        /// Creates a Portable Executable reader over a PE image stored in memory.
        /// </summary>
        /// <param name="peImage">Pointer to the start of the PE image.</param>
        /// <param name="size">The size of the PE image.</param>
        /// <exception cref="ArgumentNullException"><paramref name="peImage"/> is <see cref="IntPtr.Zero"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is negative.</exception>
        /// <remarks>
        /// The memory is owned by the caller and not released on disposal of the <see cref="PEReader"/>.
        /// The caller is responsible for keeping the memory alive and unmodified throughout the lifetime of the <see cref="PEReader"/>.
        /// The content of the image is not read during the construction of the <see cref="PEReader"/>
        /// </remarks>
        public unsafe PEReader(byte* peImage, int size)
            : this(peImage, size, isLoadedImage: false)
        {
        }

        /// <summary>
        /// Creates a Portable Executable reader over a PE image stored in memory.
        /// </summary>
        /// <param name="peImage">Pointer to the start of the PE image.</param>
        /// <param name="size">The size of the PE image.</param>
        /// <param name="isLoadedImage">True if the PE image has been loaded into memory by the OS loader.</param>
        /// <exception cref="ArgumentNullException"><paramref name="peImage"/> is <see cref="IntPtr.Zero"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is negative.</exception>
        /// <remarks>
        /// The memory is owned by the caller and not released on disposal of the <see cref="PEReader"/>.
        /// The caller is responsible for keeping the memory alive and unmodified throughout the lifetime of the <see cref="PEReader"/>.
        /// The content of the image is not read during the construction of the <see cref="PEReader"/>
        /// </remarks>
        public unsafe PEReader(byte* peImage, int size, bool isLoadedImage)
        {
            if (peImage is null)
            {
                throw new ArgumentNullException(nameof(peImage));
            }

            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            _peImage = new ExternalMemoryBlockProvider(peImage, size);
            IsLoadedImage = isLoadedImage;
        }

        /// <summary>
        /// Creates a Portable Executable reader over a PE image stored in a stream.
        /// </summary>
        /// <param name="peStream">PE image stream.</param>
        /// <exception cref="ArgumentNullException"><paramref name="peStream"/> is null.</exception>
        /// <remarks>
        /// Ownership of the stream is transferred to the <see cref="PEReader"/> upon successful validation of constructor arguments. It will be
        /// disposed by the <see cref="PEReader"/> and the caller must not manipulate it.
        /// </remarks>
        public PEReader(Stream peStream)
            : this(peStream, PEStreamOptions.Default)
        {
        }

        /// <summary>
        /// Creates a Portable Executable reader over a PE image stored in a stream beginning at its current position and ending at the end of the stream.
        /// </summary>
        /// <param name="peStream">PE image stream.</param>
        /// <param name="options">
        /// Options specifying how sections of the PE image are read from the stream.
        ///
        /// Unless <see cref="PEStreamOptions.LeaveOpen"/> is specified, ownership of the stream is transferred to the <see cref="PEReader"/>
        /// upon successful argument validation. It will be disposed by the <see cref="PEReader"/> and the caller must not manipulate it.
        ///
        /// Unless <see cref="PEStreamOptions.PrefetchMetadata"/> or <see cref="PEStreamOptions.PrefetchEntireImage"/> is specified no data
        /// is read from the stream during the construction of the <see cref="PEReader"/>. Furthermore, the stream must not be manipulated
        /// by caller while the <see cref="PEReader"/> is alive and undisposed.
        ///
        /// If <see cref="PEStreamOptions.PrefetchMetadata"/> or <see cref="PEStreamOptions.PrefetchEntireImage"/>, the <see cref="PEReader"/>
        /// will have read all of the data requested during construction. As such, if <see cref="PEStreamOptions.LeaveOpen"/> is also
        /// specified, the caller retains full ownership of the stream and is assured that it will not be manipulated by the <see cref="PEReader"/>
        /// after construction.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="peStream"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="options"/> has an invalid value.</exception>
        /// <exception cref="IOException">Error reading from the stream (only when prefetching data).</exception>
        /// <exception cref="BadImageFormatException"><see cref="PEStreamOptions.PrefetchMetadata"/> is specified and the PE headers of the image are invalid.</exception>
        public PEReader(Stream peStream, PEStreamOptions options)
            : this(peStream, options, 0)
        {
        }

        /// <summary>
        /// Creates a Portable Executable reader over a PE image of the given size beginning at the stream's current position.
        /// </summary>
        /// <param name="peStream">PE image stream.</param>
        /// <param name="size">PE image size.</param>
        /// <param name="options">
        /// Options specifying how sections of the PE image are read from the stream.
        ///
        /// Unless <see cref="PEStreamOptions.LeaveOpen"/> is specified, ownership of the stream is transferred to the <see cref="PEReader"/>
        /// upon successful argument validation. It will be disposed by the <see cref="PEReader"/> and the caller must not manipulate it.
        ///
        /// Unless <see cref="PEStreamOptions.PrefetchMetadata"/> or <see cref="PEStreamOptions.PrefetchEntireImage"/> is specified no data
        /// is read from the stream during the construction of the <see cref="PEReader"/>. Furthermore, the stream must not be manipulated
        /// by caller while the <see cref="PEReader"/> is alive and undisposed.
        ///
        /// If <see cref="PEStreamOptions.PrefetchMetadata"/> or <see cref="PEStreamOptions.PrefetchEntireImage"/>, the <see cref="PEReader"/>
        /// will have read all of the data requested during construction. As such, if <see cref="PEStreamOptions.LeaveOpen"/> is also
        /// specified, the caller retains full ownership of the stream and is assured that it will not be manipulated by the <see cref="PEReader"/>
        /// after construction.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Size is negative or extends past the end of the stream.</exception>
        /// <exception cref="IOException">Error reading from the stream (only when prefetching data).</exception>
        /// <exception cref="BadImageFormatException"><see cref="PEStreamOptions.PrefetchMetadata"/> is specified and the PE headers of the image are invalid.</exception>
        public unsafe PEReader(Stream peStream, PEStreamOptions options, int size)
        {
            if (peStream is null)
            {
                throw new ArgumentNullException(nameof(peStream));
            }

            if (!peStream.CanRead || !peStream.CanSeek)
            {
                throw new ArgumentException("The given stream must be readable and seekable.", nameof(peStream));
            }

            if (!options.IsValid())
            {
                throw new ArgumentOutOfRangeException(nameof(options));
            }

            IsLoadedImage = (options & PEStreamOptions.IsLoadedImage) != 0;

            long start = peStream.Position;
            int actualSize = GetAndValidateSize(peStream, size, nameof(peStream));

            bool closeStream = true;
            try
            {
                if ((options & (PEStreamOptions.PrefetchMetadata | PEStreamOptions.PrefetchEntireImage)) == 0)
                {
                    _peImage = new StreamMemoryBlockProvider(peStream, start, actualSize, (options & PEStreamOptions.LeaveOpen) != 0);
                    closeStream = false;
                }
                else
                {
                    // Read in the entire image or metadata blob:
                    if ((options & PEStreamOptions.PrefetchEntireImage) != 0)
                    {
                        var imageBlock = StreamMemoryBlockProvider.ReadMemoryBlockNoLock(peStream, start, actualSize);
                        _lazyImageBlock = imageBlock;
                        _peImage = new ExternalMemoryBlockProvider(imageBlock.Pointer, imageBlock.Size);

                        // if the caller asked for metadata initialize the PE headers (calculates metadata offset):
                        if ((options & PEStreamOptions.PrefetchMetadata) != 0)
                        {
                            InitializePEHeaders();
                        }
                    }
                    else
                    {
                        // The peImage is left null, but the lazyMetadataBlock is initialized up front.
                        _lazyPEHeaders = new PEHeaders(peStream, actualSize, IsLoadedImage);
                        _lazyMetadataBlock = StreamMemoryBlockProvider.ReadMemoryBlockNoLock(peStream, _lazyPEHeaders.MetadataStartOffset, _lazyPEHeaders.MetadataSize);
                    }
                    // We read all we need, the stream is going to be closed.
                }
            }
            finally
            {
                if (closeStream && (options & PEStreamOptions.LeaveOpen) == 0)
                {
                    peStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Creates a Portable Executable reader over a PE image stored in a byte array.
        /// </summary>
        /// <param name="peImage">PE image.</param>
        /// <remarks>
        /// The content of the image is not read during the construction of the <see cref="PEReader"/>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="peImage"/> is null.</exception>
        public PEReader(System.Byte[] peImage)
        {
            if (peImage is null)
            {
                throw new ArgumentNullException(nameof(peImage));
            }

            _peImage = new ByteArrayMemoryProvider(peImage);
        }

        /// <summary>
        /// Disposes all memory allocated by the reader.
        /// </summary>
        /// <remarks>
        /// <see cref="Dispose"/>  can be called multiple times (but not in parallel).
        /// It is not safe to call <see cref="Dispose"/> in parallel with any other operation on the <see cref="PEReader"/>
        /// or reading from <see cref="PEMemoryBlock"/>s retrieved from the reader.
        /// </remarks>
        public void Dispose()
        {
            _lazyPEHeaders = null;

            _peImage?.Dispose();
            _peImage = null;

            _lazyImageBlock?.Dispose();
            _lazyImageBlock = null;

            _lazyMetadataBlock?.Dispose();
            _lazyMetadataBlock = null;

            var peSectionBlocks = _lazyPESectionBlocks;
            if (peSectionBlocks != null)
            {
                foreach (var block in peSectionBlocks)
                {
                    block?.Dispose();
                }

                _lazyPESectionBlocks = null;
            }
        }

        private MemoryBlockProvider GetPEImage()
        {
            var peImage = _peImage;
            if (peImage == null)
            {
                if (_lazyPEHeaders == null)
                {
                    throw new ObjectDisposedException(nameof(PEReader));
                }

                throw new InvalidOperationException("The PE image is currently not available.");
            }

            return peImage;
        }

        /// <summary>
        /// Gets the PE headers.
        /// </summary>
        /// <exception cref="BadImageFormatException">The headers contain invalid data.</exception>
        /// <exception cref="IOException">Error reading from the stream.</exception>
        public PEHeaders PEHeaders
        {
            get
            {
                if (_lazyPEHeaders == null)
                {
                    InitializePEHeaders();
                    Debug.Assert(_lazyPEHeaders != null);
                }

                return _lazyPEHeaders;
            }
        }

        /// <exception cref="IOException">Error reading from the stream.</exception>
        private void InitializePEHeaders()
        {
            StreamConstraints constraints;
            Stream stream = GetPEImage().GetStream(out constraints);

            PEHeaders headers;
            if (constraints.GuardOpt != null)
            {
                lock (constraints.GuardOpt)
                {
                    headers = ReadPEHeadersNoLock(stream, constraints.ImageStart, constraints.ImageSize, IsLoadedImage);
                }
            }
            else
            {
                headers = ReadPEHeadersNoLock(stream, constraints.ImageStart, constraints.ImageSize, IsLoadedImage);
            }

            Interlocked.CompareExchange(ref _lazyPEHeaders, headers, null);
        }

        /// <exception cref="IOException">Error reading from the stream.</exception>
        private static PEHeaders ReadPEHeadersNoLock(Stream stream, long imageStartPosition, int imageSize, bool isLoadedImage)
        {
            Debug.Assert(imageStartPosition >= 0 && imageStartPosition <= stream.Length);
            stream.Seek(imageStartPosition, SeekOrigin.Begin);
            return new PEHeaders(stream, imageSize, isLoadedImage);
        }

        /// <summary>
        /// Returns a view of the entire image as a pointer and length.
        /// </summary>
        /// <exception cref="InvalidOperationException">PE image not available.</exception>
        private AbstractMemoryBlock GetEntireImageBlock()
        {
            if (_lazyImageBlock == null)
            {
                var newBlock = GetPEImage().GetMemoryBlock();
                if (Interlocked.CompareExchange(ref _lazyImageBlock, newBlock, null) != null)
                {
                    // another thread created the block already, we need to dispose ours:
                    newBlock.Dispose();
                }
            }

            return _lazyImageBlock;
        }

        /// <exception cref="IOException">IO error while reading from the underlying stream.</exception>
        /// <exception cref="InvalidOperationException">PE image doesn't have metadata.</exception>
        private AbstractMemoryBlock GetMetadataBlock()
        {
            if (!HasMetadata)
            {
                throw new InvalidOperationException("The PE image does not contain any metadata.");
            }

            if (_lazyMetadataBlock == null)
            {
                var newBlock = GetPEImage().GetMemoryBlock(PEHeaders.MetadataStartOffset, PEHeaders.MetadataSize);
                if (Interlocked.CompareExchange(ref _lazyMetadataBlock, newBlock, null) != null)
                {
                    // another thread created the block already, we need to dispose ours:
                    newBlock.Dispose();
                }
            }

            return _lazyMetadataBlock;
        }

        /// <exception cref="IOException">IO error while reading from the underlying stream.</exception>
        /// <exception cref="InvalidOperationException">PE image not available.</exception>
        private AbstractMemoryBlock GetPESectionBlock(int index)
        {
            Debug.Assert(index >= 0 && index < PEHeaders.SectionHeaders.Length);

            var peImage = GetPEImage();

            if (_lazyPESectionBlocks == null)
            {
                Interlocked.CompareExchange(ref _lazyPESectionBlocks, new AbstractMemoryBlock[PEHeaders.SectionHeaders.Length], null);
            }

            AbstractMemoryBlock existingBlock = Volatile.Read(ref _lazyPESectionBlocks[index]);
            if (existingBlock != null)
            {
                return existingBlock;
            }

            AbstractMemoryBlock newBlock;
            if (IsLoadedImage)
            {
                newBlock = peImage.GetMemoryBlock(
                    PEHeaders.SectionHeaders[index].VirtualAddress,
                    PEHeaders.SectionHeaders[index].VirtualSize);
            }
            else
            {
                // Virtual size can be smaller than size in the image
                // since the size in the image is aligned.
                // Trim the alignment.
                //
                // Virtual size can also be larger than size in the image.
                // When loaded sizeInImage bytes are mapped from the image
                // and the rest of the bytes are zeroed out.
                // Only return data stored in the image.

                int size = Math.Min(
                    PEHeaders.SectionHeaders[index].VirtualSize,
                    PEHeaders.SectionHeaders[index].SizeOfRawData);

                newBlock = peImage.GetMemoryBlock(PEHeaders.SectionHeaders[index].PointerToRawData, size);
            }

            if (Interlocked.CompareExchange(ref _lazyPESectionBlocks[index], newBlock, null) != null)
            {
                // another thread created the block already, we need to dispose ours:
                newBlock.Dispose();
            }

            return _lazyPESectionBlocks[index]!;
        }

        /// <summary>
        /// Return true if the reader can access the entire PE image.
        /// </summary>
        /// <remarks>
        /// Returns false if the <see cref="PEReader"/> is constructed from a stream and only part of it is prefetched into memory.
        /// </remarks>
        public bool IsEntireImageAvailable => _lazyImageBlock != null || _peImage != null;

        /// <summary>
        /// Gets a pointer to and size of the PE image if available (<see cref="IsEntireImageAvailable"/>).
        /// </summary>
        /// <exception cref="InvalidOperationException">The entire PE image is not available.</exception>
        public PEMemoryBlock GetEntireImage()
        {
            return new PEMemoryBlock(GetEntireImageBlock());
        }

        /// <summary>
        /// Returns true if the PE image contains CLI metadata.
        /// </summary>
        /// <exception cref="BadImageFormatException">The PE headers contain invalid data.</exception>
        /// <exception cref="IOException">Error reading from the underlying stream.</exception>
        public bool HasMetadata
        {
            get { return PEHeaders.MetadataSize > 0; }
        }

        /// <summary>
        /// Loads PE section that contains CLI metadata.
        /// </summary>
        /// <exception cref="InvalidOperationException">The PE image doesn't contain metadata (<see cref="HasMetadata"/> returns false).</exception>
        /// <exception cref="BadImageFormatException">The PE headers contain invalid data.</exception>
        /// <exception cref="IOException">IO error while reading from the underlying stream.</exception>
        public PEMemoryBlock GetMetadata()
        {
            return new PEMemoryBlock(GetMetadataBlock());
        }

        /// <summary>
        /// Loads PE section that contains the specified <paramref name="relativeVirtualAddress"/> into memory
        /// and returns a memory block that starts at <paramref name="relativeVirtualAddress"/> and ends at the end of the containing section.
        /// </summary>
        /// <param name="relativeVirtualAddress">Relative Virtual Address of the data to read.</param>
        /// <returns>
        /// An empty block if <paramref name="relativeVirtualAddress"/> doesn't represent a location in any of the PE sections of this PE image.
        /// </returns>
        /// <exception cref="BadImageFormatException">The PE headers contain invalid data.</exception>
        /// <exception cref="IOException">IO error while reading from the underlying stream.</exception>
        /// <exception cref="InvalidOperationException">PE image not available.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="relativeVirtualAddress"/> is negative.</exception>
        public PEMemoryBlock GetSectionData(int relativeVirtualAddress)
        {
            if (relativeVirtualAddress < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(relativeVirtualAddress));
            }

            int sectionIndex = PEHeaders.GetContainingSectionIndex(relativeVirtualAddress);
            if (sectionIndex < 0)
            {
                return default(PEMemoryBlock);
            }

            var block = GetPESectionBlock(sectionIndex);

            int relativeOffset = relativeVirtualAddress - PEHeaders.SectionHeaders[sectionIndex].VirtualAddress;
            if (relativeOffset > block.Size)
            {
                return default(PEMemoryBlock);
            }

            return new PEMemoryBlock(block, relativeOffset);
        }

        /// <summary>
        /// Loads PE section of the specified name into memory and returns a memory block that spans the section.
        /// </summary>
        /// <param name="sectionName">Name of the section.</param>
        /// <returns>
        /// An empty block if no section of the given <paramref name="sectionName"/> exists in this PE image.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="sectionName"/> is null.</exception>
        /// <exception cref="InvalidOperationException">PE image not available.</exception>
        public PEMemoryBlock GetSectionData(string sectionName)
        {
            if (sectionName is null)
            {
                throw new ArgumentNullException(nameof(sectionName));
            }

            int sectionIndex = PEHeaders.IndexOfSection(sectionName);
            if (sectionIndex < 0)
            {
                return default(PEMemoryBlock);
            }

            return new PEMemoryBlock(GetPESectionBlock(sectionIndex));
        }

    }
}
