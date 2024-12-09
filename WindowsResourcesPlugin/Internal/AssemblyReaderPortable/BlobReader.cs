// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotNetResourcesExtensions.Internal.AssemblyReader
{
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    internal unsafe struct BlobReader
    {
        internal const int InvalidCompressedInteger = int.MaxValue;

        private readonly MemoryBlock _block;

        // Points right behind the last byte of the block.
        private readonly byte* _endPointer;

        private byte* _currentPointer;

        /// <summary>
        /// Creates a reader of the specified memory block.
        /// </summary>
        /// <param name="buffer">Pointer to the start of the memory block.</param>
        /// <param name="length">Length in bytes of the memory block.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null and <paramref name="length"/> is greater than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
        /// <exception cref="PlatformNotSupportedException">The current platform is not little-endian.</exception>
        public BlobReader(byte* buffer, int length)
            : this(MemoryBlock.CreateChecked(buffer, length))
        {

        }

        internal BlobReader(MemoryBlock block)
        {
            Debug.Assert(block.Length >= 0 && (block.Pointer != null || block.Length == 0));
            _block = block;
            _currentPointer = block.Pointer;
            _endPointer = block.Pointer + block.Length;
        }

        internal string GetDebuggerDisplay()
        {
            if (_block.Pointer == null)
            {
                return "<null>";
            }

            int displayedBytes;
            string display = _block.GetDebuggerDisplay(out displayedBytes);
            if (this.Offset < displayedBytes)
            {
                display = display.Insert(this.Offset * 3, "*");
            }
            else if (displayedBytes == _block.Length)
            {
                display += "*";
            }
            else
            {
                display += "*...";
            }

            return display;
        }

        #region Offset, Skipping, Marking, Alignment, Bounds Checking

        /// <summary>
        /// Pointer to the byte at the start of the underlying memory block.
        /// </summary>
        public byte* StartPointer => _block.Pointer;

        /// <summary>
        /// Pointer to the byte at the current position of the reader.
        /// </summary>
        public byte* CurrentPointer => _currentPointer;

        /// <summary>
        /// The total length of the underlying memory block.
        /// </summary>
        public int Length => _block.Length;

        /// <summary>
        /// Gets or sets the offset from start of the blob to the current position.
        /// </summary>
        /// <exception cref="BadImageFormatException">Offset is set outside the bounds of underlying reader.</exception>
        public int Offset
        {
            get
            {
                return (int)(_currentPointer - _block.Pointer);
            }
            set
            {
                if (unchecked((uint)value) > (uint)_block.Length)
                {
                    throw new BadImageFormatException("Attempted to read past the end of the memory blob.");
                }

                _currentPointer = _block.Pointer + value;
            }
        }

        /// <summary>
        /// Bytes remaining from current position to end of underlying memory block.
        /// </summary>
        public int RemainingBytes => (int)(_endPointer - _currentPointer);

        /// <summary>
        /// Repositions the reader to the start of the underlying memory block.
        /// </summary>
        public void Reset()
        {
            _currentPointer = _block.Pointer;
        }

        /// <summary>
        /// Repositions the reader forward by the number of bytes required to satisfy the given alignment.
        /// </summary>
        public void Align(byte alignment)
        {
            if (!TryAlign(alignment))
            {
                throw new BadImageFormatException("Attempted to read past the end of the memory blob.");
            }
        }

        internal bool TryAlign(byte alignment)
        {
            int remainder = this.Offset & (alignment - 1);

            Debug.Assert((alignment & (alignment - 1)) == 0, "Alignment must be a power of two.");
            Debug.Assert(remainder >= 0 && remainder < alignment);

            if (remainder != 0)
            {
                int bytesToSkip = alignment - remainder;
                if (bytesToSkip > RemainingBytes)
                {
                    return false;
                }

                _currentPointer += bytesToSkip;
            }

            return true;
        }

        internal MemoryBlock GetMemoryBlockAt(int offset, int length)
        {
            CheckBounds(offset, length);
            return new MemoryBlock(_currentPointer + offset, length);
        }

        #endregion

        #region Bounds Checking

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckBounds(int offset, int byteCount)
        {
            if (unchecked((ulong)(uint)offset + (uint)byteCount) > (ulong)(_endPointer - _currentPointer))
            {
                throw new BadImageFormatException("Attempted to read past the end of the memory blob.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckBounds(int byteCount)
        {
            if (unchecked((uint)byteCount) > (_endPointer - _currentPointer))
            {
                throw new BadImageFormatException("Attempted to read past the end of the memory blob.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte* GetCurrentPointerAndAdvance(int length)
        {
            byte* p = _currentPointer;

            if (unchecked((uint)length) > (uint)(_endPointer - p))
            {
                throw new BadImageFormatException("Attempted to read past the end of the memory blob.");
            }

            _currentPointer = p + length;
            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte* GetCurrentPointerAndAdvance1()
        {
            byte* p = _currentPointer;

            if (p == _endPointer)
            {
                throw new BadImageFormatException("Attempted to read past the end of the memory blob.");
            }

            _currentPointer = p + 1;
            return p;
        }

        #endregion

        #region Read Methods

        public bool ReadBoolean()
        {
            // It's not clear from the ECMA spec what exactly is the encoding of Boolean.
            // Some metadata writers encode "true" as 0xff, others as 1. So we treat all non-zero values as "true".
            //
            // We propose to clarify and relax the current wording in the spec as follows:
            //
            // Chapter II.16.2 "Field init metadata"
            //   ... bool '(' true | false ')' Boolean value stored in a single byte, 0 represents false, any non-zero value represents true ...
            //
            // Chapter 23.3 "Custom attributes"
            //   ... A bool is a single byte with value 0 representing false and any non-zero value representing true ...
            return ReadByte() != 0;
        }

        public sbyte ReadSByte()
        {
            return *(sbyte*)GetCurrentPointerAndAdvance1();
        }

        public byte ReadByte()
        {
            return *(byte*)GetCurrentPointerAndAdvance1();
        }

        public char ReadChar()
        {
            unchecked
            {
                byte* ptr = GetCurrentPointerAndAdvance(sizeof(char));
                return (char)(ptr[0] + (ptr[1] << 8));
            }
        }

        public short ReadInt16()
        {
            unchecked
            {
                byte* ptr = GetCurrentPointerAndAdvance(sizeof(short));
                return (short)(ptr[0] + (ptr[1] << 8));
            }
        }

        public ushort ReadUInt16()
        {
            unchecked
            {
                byte* ptr = GetCurrentPointerAndAdvance(sizeof(ushort));
                return (ushort)(ptr[0] + (ptr[1] << 8));
            }
        }

        public int ReadInt32()
        {
            unchecked
            {
                byte* ptr = GetCurrentPointerAndAdvance(sizeof(int));
                return (int)(ptr[0] + (ptr[1] << 8) + (ptr[2] << 16) + (ptr[3] << 24));
            }
        }

        public uint ReadUInt32()
        {
            unchecked
            {
                byte* ptr = GetCurrentPointerAndAdvance(sizeof(uint));
                return (uint)(ptr[0] + (ptr[1] << 8) + (ptr[2] << 16) + (ptr[3] << 24));
            }
        }

        public long ReadInt64()
        {
            unchecked
            {
                byte* ptr = GetCurrentPointerAndAdvance(sizeof(long));
                uint lo = (uint)(ptr[0] + (ptr[1] << 8) + (ptr[2] << 16) + (ptr[3] << 24));
                uint hi = (uint)(ptr[4] + (ptr[5] << 8) + (ptr[6] << 16) + (ptr[7] << 24));
                return (long)(lo + ((ulong)hi << 32));
            }
        }

        public ulong ReadUInt64()
        {
            return unchecked((ulong)ReadInt64());
        }

        public float ReadSingle()
        {
            int val = ReadInt32();
            return *(float*)&val;
        }

        public double ReadDouble()
        {
            long val = ReadInt64();
            return *(double*)&val;
        }

        public Guid ReadGuid()
        {
            const int size = 16;
            byte* ptr = GetCurrentPointerAndAdvance(size);
            if (BitConverter.IsLittleEndian)
            {
                return *(Guid*)ptr;
            }
            else
            {
                unchecked
                {
                    return new Guid(
                        (int)(ptr[0] | (ptr[1] << 8) | (ptr[2] << 16) | (ptr[3] << 24)),
                        (short)(ptr[4] | (ptr[5] << 8)),
                        (short)(ptr[6] | (ptr[7] << 8)),
                        ptr[8], ptr[9], ptr[10], ptr[11], ptr[12], ptr[13], ptr[14], ptr[15]);
                }
            }
        }

        /// <summary>
        /// Reads <see cref="decimal"/> number.
        /// </summary>
        /// <remarks>
        /// Decimal number is encoded in 13 bytes as follows:
        /// - byte 0: highest bit indicates sign (1 for negative, 0 for non-negative); the remaining 7 bits encode scale
        /// - bytes 1..12: 96-bit unsigned integer in little endian encoding.
        /// </remarks>
        /// <exception cref="BadImageFormatException">The data at the current position was not a valid <see cref="decimal"/> number.</exception>
        public decimal ReadDecimal()
        {
            byte* ptr = GetCurrentPointerAndAdvance(13);

            byte scale = (byte)(*ptr & 0x7f);
            if (scale > 28)
            {
                throw new BadImageFormatException("The value is too large so as to be a decimal value.");
            }

            unchecked
            {
                return new decimal(
                    (int)(ptr[1] | (ptr[2] << 8) | (ptr[3] << 16) | (ptr[4] << 24)),
                    (int)(ptr[5] | (ptr[6] << 8) | (ptr[7] << 16) | (ptr[8] << 24)),
                    (int)(ptr[9] | (ptr[10] << 8) | (ptr[11] << 16) | (ptr[12] << 24)),
                    isNegative: (*ptr & 0x80) != 0,
                    scale: scale);
            }
        }

        public DateTime ReadDateTime()
        {
            return new DateTime(ReadInt64());
        }

        /// <summary>
        /// Finds specified byte in the blob following the current position.
        /// </summary>
        /// <returns>
        /// Index relative to the current position, or -1 if the byte is not found in the blob following the current position.
        /// </returns>
        /// <remarks>
        /// Doesn't change the current position.
        /// </remarks>
        public int IndexOf(byte value)
        {
            int start = Offset;
            int absoluteIndex = _block.IndexOfUnchecked(value, start);
            return (absoluteIndex >= 0) ? absoluteIndex - start : -1;
        }

        /// <summary>
        /// Reads UTF-8 encoded string starting at the current position.
        /// </summary>
        /// <param name="byteCount">The number of bytes to read.</param>
        /// <returns>The string.</returns>
        /// <exception cref="BadImageFormatException"><paramref name="byteCount"/> bytes not available.</exception>
        public string ReadUTF8(int byteCount)
        {
            string s = _block.PeekUtf8(this.Offset, byteCount);
            _currentPointer += byteCount;
            return s;
        }

        /// <summary>
        /// Reads UTF-16 (little-endian) encoded string starting at the current position.
        /// </summary>
        /// <param name="byteCount">The number of bytes to read.</param>
        /// <returns>The string.</returns>
        /// <exception cref="BadImageFormatException"><paramref name="byteCount"/> bytes not available.</exception>
        public string ReadUTF16(int byteCount)
        {
            string s = _block.PeekUtf16(this.Offset, byteCount);
            _currentPointer += byteCount;
            return s;
        }

        /// <summary>
        /// Reads bytes starting at the current position.
        /// </summary>
        /// <param name="byteCount">The number of bytes to read.</param>
        /// <returns>The byte array.</returns>
        /// <exception cref="BadImageFormatException"><paramref name="byteCount"/> bytes not available.</exception>
        public byte[] ReadBytes(int byteCount)
        {
            byte[] bytes = _block.PeekBytes(this.Offset, byteCount);
            _currentPointer += byteCount;
            return bytes;
        }

        /// <summary>
        /// Reads bytes starting at the current position in to the given buffer at the given offset;
        /// </summary>
        /// <param name="byteCount">The number of bytes to read.</param>
        /// <param name="buffer">The destination buffer the bytes read will be written.</param>
        /// <param name="bufferOffset">The offset in the destination buffer where the bytes read will be written.</param>
        /// <exception cref="BadImageFormatException"><paramref name="byteCount"/> bytes not available.</exception>
        public void ReadBytes(int byteCount, byte[] buffer, int bufferOffset)
        {
            Marshal.Copy((IntPtr)GetCurrentPointerAndAdvance(byteCount), buffer, bufferOffset, byteCount);
        }

        private int ReadCompressedIntegerOrInvalid()
        {
            int bytesRead;
            int value = _block.PeekCompressedInteger(this.Offset, out bytesRead);
            _currentPointer += bytesRead;
            return value;
        }

        /// <summary>
        /// Reads an unsigned compressed integer value.
        /// See Metadata Specification section II.23.2: Blobs and signatures.
        /// </summary>
        /// <param name="value">The value of the compressed integer that was read.</param>
        /// <returns>true if the value was read successfully. false if the data at the current position was not a valid compressed integer.</returns>
        public bool TryReadCompressedInteger(out int value)
        {
            value = ReadCompressedIntegerOrInvalid();
            return value != InvalidCompressedInteger;
        }

        /// <summary>
        /// Reads an unsigned compressed integer value.
        /// See Metadata Specification section II.23.2: Blobs and signatures.
        /// </summary>
        /// <returns>The value of the compressed integer that was read.</returns>
        /// <exception cref="BadImageFormatException">The data at the current position was not a valid compressed integer.</exception>
        public int ReadCompressedInteger()
        {
            int value;
            if (!TryReadCompressedInteger(out value))
            {
                throw new BadImageFormatException("The compressed integer data were invalid.");
            }
            return value;
        }

        /// <summary>
        /// Reads a signed compressed integer value.
        /// See Metadata Specification section II.23.2: Blobs and signatures.
        /// </summary>
        /// <param name="value">The value of the compressed integer that was read.</param>
        /// <returns>true if the value was read successfully. false if the data at the current position was not a valid compressed integer.</returns>
        public bool TryReadCompressedSignedInteger(out int value)
        {
            int bytesRead;
            value = _block.PeekCompressedInteger(this.Offset, out bytesRead);

            if (value == InvalidCompressedInteger)
            {
                return false;
            }

            bool signExtend = (value & 0x1) != 0;
            value >>= 1;

            if (signExtend)
            {
                switch (bytesRead)
                {
                    case 1:
                        value |= unchecked((int)0xffffffc0);
                        break;
                    case 2:
                        value |= unchecked((int)0xffffe000);
                        break;
                    default:
                        Debug.Assert(bytesRead == 4);
                        value |= unchecked((int)0xf0000000);
                        break;
                }
            }

            _currentPointer += bytesRead;
            return true;
        }

        /// <summary>
        /// Reads a signed compressed integer value.
        /// See Metadata Specification section II.23.2: Blobs and signatures.
        /// </summary>
        /// <returns>The value of the compressed integer that was read.</returns>
        /// <exception cref="BadImageFormatException">The data at the current position was not a valid compressed integer.</exception>
        public int ReadCompressedSignedInteger()
        {
            int value;
            if (!TryReadCompressedSignedInteger(out value))
            {
                throw new BadImageFormatException("The compressed integer data were invalid.");
            }
            return value;
        }

        /// <summary>
        /// Reads a string encoded as a compressed integer containing its length followed by
        /// its contents in UTF-8. Null strings are encoded as a single 0xFF byte.
        /// </summary>
        /// <remarks>Defined as a 'SerString' in the ECMA CLI specification.</remarks>
        /// <returns>String value or null.</returns>
        /// <exception cref="BadImageFormatException">If the encoding is invalid.</exception>
        public string ReadSerializedString()
        {
            int length;
            if (TryReadCompressedInteger(out length))
            {
                return ReadUTF8(length);
            }

            if (ReadByte() != 0xFF)
            {
                throw new BadImageFormatException("The encoding of the string was invalid.");
            }

            return null;
        }

        #endregion
    }
}
