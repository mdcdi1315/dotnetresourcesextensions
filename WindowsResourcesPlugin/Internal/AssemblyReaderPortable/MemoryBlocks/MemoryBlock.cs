// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetResourcesExtensions.Internal.AssemblyReader
{
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    internal readonly unsafe struct MemoryBlock
    {
        internal readonly byte* Pointer;
        internal readonly int Length;

        internal MemoryBlock(byte* buffer, int length)
        {
            Debug.Assert(length >= 0 && (buffer != null || length == 0));
            this.Pointer = buffer;
            this.Length = length;
        }

        internal static MemoryBlock CreateChecked(byte* buffer, int length)
        {
            if (length < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(length));
            }

            if (buffer == null && length != 0)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new MemoryBlock(buffer, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckBounds(int offset, int byteCount)
        {
            if (unchecked((ulong)(uint)offset + (uint)byteCount) > (ulong)Length)
            {
                throw new System.BadImageFormatException("Attempted to read out of the memory block bounds.");
            }
        }

        internal byte[] ToArray()
        {
            return Pointer == null ? null : PeekBytes(0, Length);
        }

        private string GetDebuggerDisplay()
        {
            if (Pointer == null)
            {
                return "<null>";
            }

            return GetDebuggerDisplay(out _);
        }

        internal string GetDebuggerDisplay(out int displayedBytes)
        {
            displayedBytes = System.Math.Min(Length, 64);
            string result = System.BitConverter.ToString(PeekBytes(0, displayedBytes));
            if (displayedBytes < Length)
            {
                result += "-...";
            }

            return result;
        }

        internal string GetDebuggerDisplay(int offset)
        {
            if (Pointer == null)
            {
                return "<null>";
            }

            int displayedBytes;
            string display = GetDebuggerDisplay(out displayedBytes);
            if (offset < displayedBytes)
            {
                display = display.Insert(offset * 3, "*");
            }
            else if (displayedBytes == Length)
            {
                display += "*";
            }
            else
            {
                display += "*...";
            }

            return display;
        }

        internal MemoryBlock GetMemoryBlockAt(int offset, int length)
        {
            CheckBounds(offset, length);
            return new MemoryBlock(Pointer + offset, length);
        }

        internal byte PeekByte(int offset)
        {
            CheckBounds(offset, sizeof(byte));
            return Pointer[offset];
        }

        internal int PeekInt32(int offset)
        {
            uint result = PeekUInt32(offset);
            if (unchecked((int)result != result))
            {
                throw new System.BadImageFormatException("The value is too large so as to be an Int32.");
            }

            return (int)result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint PeekUInt32(int offset)
        {
            CheckBounds(offset, sizeof(uint));

            uint result = Unsafe.ReadUnaligned<uint>(Pointer + offset);
            return System.BitConverter.IsLittleEndian ? result : BinaryPrimitives.ReverseEndianness(result);
        }

        /// <summary>
        /// Decodes a compressed integer value starting at offset.
        /// See Metadata Specification section II.23.2: Blobs and signatures.
        /// </summary>
        /// <param name="offset">Offset to the start of the compressed data.</param>
        /// <param name="numberOfBytesRead">Bytes actually read.</param>
        /// <returns>
        /// Value between 0 and 0x1fffffff, or <see cref="BlobReader.InvalidCompressedInteger"/> if the value encoding is invalid.
        /// </returns>
        internal int PeekCompressedInteger(int offset, out int numberOfBytesRead)
        {
            CheckBounds(offset, 0);

            byte* ptr = Pointer + offset;
            long limit = Length - offset;

            if (limit == 0)
            {
                numberOfBytesRead = 0;
                return BlobReader.InvalidCompressedInteger;
            }

            byte headerByte = ptr[0];
            if ((headerByte & 0x80) == 0)
            {
                numberOfBytesRead = 1;
                return headerByte;
            }
            else if ((headerByte & 0x40) == 0)
            {
                if (limit >= 2)
                {
                    numberOfBytesRead = 2;
                    return ((headerByte & 0x3f) << 8) | ptr[1];
                }
            }
            else if ((headerByte & 0x20) == 0)
            {
                if (limit >= 4)
                {
                    numberOfBytesRead = 4;
                    return ((headerByte & 0x1f) << 24) | (ptr[1] << 16) | (ptr[2] << 8) | ptr[3];
                }
            }

            numberOfBytesRead = 0;
            return BlobReader.InvalidCompressedInteger;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort PeekUInt16(int offset)
        {
            CheckBounds(offset, sizeof(ushort));

            ushort result = Unsafe.ReadUnaligned<ushort>(Pointer + offset);
            return BitConverter.IsLittleEndian ? result : BinaryPrimitives.ReverseEndianness(result);
        }

        // When reference has tag bits.
        internal uint PeekTaggedReference(int offset, bool smallRefSize)
        {
            return PeekReferenceUnchecked(offset, smallRefSize);
        }

        // Use when searching for a tagged or non-tagged reference.
        // The result may be an invalid reference and shall only be used to compare with a valid reference.
        internal uint PeekReferenceUnchecked(int offset, bool smallRefSize)
        {
            return smallRefSize ? PeekUInt16(offset) : PeekUInt32(offset);
        }

        internal Guid PeekGuid(int offset)
        {
            CheckBounds(offset, sizeof(Guid));

            byte* ptr = Pointer + offset;
            if (BitConverter.IsLittleEndian)
            {
                return Unsafe.ReadUnaligned<Guid>(ptr);
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

        internal string PeekUtf16(int offset, int byteCount)
        {
            CheckBounds(offset, byteCount);

            byte* ptr = Pointer + offset;
            if (BitConverter.IsLittleEndian)
            {
                // doesn't allocate a new string if byteCount == 0
                return new string((char*)ptr, 0, byteCount / sizeof(char));
            }
            else
            {
                return Encoding.Unicode.GetString(ptr, byteCount);
            }
        }

        internal string PeekUtf8(int offset, int byteCount)
        {
            CheckBounds(offset, byteCount);
            return Encoding.UTF8.GetString(Pointer + offset, byteCount);
        }

        /// <summary>
        /// Get number of bytes from offset to given terminator, null terminator, or end-of-block (whichever comes first).
        /// Returned length does not include the terminator, but numberOfBytesRead out parameter does.
        /// </summary>
        /// <param name="offset">Offset in to the block where the UTF-8 bytes start.</param>
        /// <param name="terminator">A character in the ASCII range that marks the end of the string.
        /// If a value other than '\0' is passed we still stop at the null terminator if encountered first.</param>
        /// <param name="numberOfBytesRead">The number of bytes read, which includes the terminator if we did not hit the end of the block.</param>
        /// <returns>Length (byte count) not including terminator.</returns>
        internal int GetUtf8NullTerminatedLength(int offset, out int numberOfBytesRead, char terminator)
        {
            CheckBounds(offset, 0);

            Debug.Assert(terminator <= 0x7f);

            System.ReadOnlySpan<byte> span = new(Pointer + offset, Length - offset);
            int length = terminator != '\0' ?
                span.IndexOfAny((byte)0, (byte)terminator) :
                span.IndexOf((byte)0);
            if (length >= 0)
            {
                numberOfBytesRead = length + 1; // we also read the terminator
            }
            else
            {
                numberOfBytesRead = length = span.Length;
            }

            return length;
        }

        internal byte[] PeekBytes(int offset, int byteCount)
        {
            CheckBounds(offset, byteCount);
            return new ReadOnlySpan<byte>(Pointer + offset, byteCount).ToArray();
        }

        internal int IndexOf(byte b, int start)
        {
            CheckBounds(start, 0);
            return IndexOfUnchecked(b, start);
        }

        internal int IndexOfUnchecked(byte b, int start)
        {
            int i = new ReadOnlySpan<byte>(Pointer + start, Length - start).IndexOf(b);
            return i >= 0 ?
                i + start :
                -1;
        }

        /// <summary>
        /// In a table that specifies children via a list field (e.g. TypeDef.FieldList, TypeDef.MethodList),
        /// searches for the parent given a reference to a child.
        /// </summary>
        /// <returns>Returns row number [0..RowCount).</returns>
        internal int BinarySearchForSlot(
            int rowCount,
            int rowSize,
            int referenceListOffset,
            uint referenceValue,
            bool isReferenceSmall)
        {
            int startRowNumber = 0;
            int endRowNumber = rowCount - 1;
            uint startValue = PeekReferenceUnchecked(startRowNumber * rowSize + referenceListOffset, isReferenceSmall);
            uint endValue = PeekReferenceUnchecked(endRowNumber * rowSize + referenceListOffset, isReferenceSmall);
            if (endRowNumber == 1)
            {
                if (referenceValue >= endValue)
                {
                    return endRowNumber;
                }

                return startRowNumber;
            }

            while (endRowNumber - startRowNumber > 1)
            {
                if (referenceValue <= startValue)
                {
                    return referenceValue == startValue ? startRowNumber : startRowNumber - 1;
                }

                if (referenceValue >= endValue)
                {
                    return referenceValue == endValue ? endRowNumber : endRowNumber + 1;
                }

                int midRowNumber = (startRowNumber + endRowNumber) / 2;
                uint midReferenceValue = PeekReferenceUnchecked(midRowNumber * rowSize + referenceListOffset, isReferenceSmall);
                if (referenceValue > midReferenceValue)
                {
                    startRowNumber = midRowNumber;
                    startValue = midReferenceValue;
                }
                else if (referenceValue < midReferenceValue)
                {
                    endRowNumber = midRowNumber;
                    endValue = midReferenceValue;
                }
                else
                {
                    return midRowNumber;
                }
            }

            return startRowNumber;
        }

        /// <summary>
        /// In a table ordered by a column containing entity references searches for a row with the specified reference.
        /// </summary>
        /// <returns>Returns row number [0..RowCount) or -1 if not found.</returns>
        internal int BinarySearchReference(
            int rowCount,
            int rowSize,
            int referenceOffset,
            uint referenceValue,
            bool isReferenceSmall)
        {
            int startRowNumber = 0;
            int endRowNumber = rowCount - 1;
            while (startRowNumber <= endRowNumber)
            {
                int midRowNumber = (startRowNumber + endRowNumber) / 2;
                uint midReferenceValue = PeekReferenceUnchecked(midRowNumber * rowSize + referenceOffset, isReferenceSmall);
                if (referenceValue > midReferenceValue)
                {
                    startRowNumber = midRowNumber + 1;
                }
                else if (referenceValue < midReferenceValue)
                {
                    endRowNumber = midRowNumber - 1;
                }
                else
                {
                    return midRowNumber;
                }
            }

            return -1;
        }

        // Row number [0, ptrTable.Length) or -1 if not found.
        internal int BinarySearchReference(
            int[] ptrTable,
            int rowSize,
            int referenceOffset,
            uint referenceValue,
            bool isReferenceSmall)
        {
            int startRowNumber = 0;
            int endRowNumber = ptrTable.Length - 1;
            while (startRowNumber <= endRowNumber)
            {
                int midRowNumber = (startRowNumber + endRowNumber) / 2;
                uint midReferenceValue = PeekReferenceUnchecked((ptrTable[midRowNumber] - 1) * rowSize + referenceOffset, isReferenceSmall);
                if (referenceValue > midReferenceValue)
                {
                    startRowNumber = midRowNumber + 1;
                }
                else if (referenceValue < midReferenceValue)
                {
                    endRowNumber = midRowNumber - 1;
                }
                else
                {
                    return midRowNumber;
                }
            }

            return -1;
        }

        /// <summary>
        /// Calculates a range of rows that have specified value in the specified column in a table that is sorted by that column.
        /// </summary>
        internal void BinarySearchReferenceRange(
            int rowCount,
            int rowSize,
            int referenceOffset,
            uint referenceValue,
            bool isReferenceSmall,
            out int startRowNumber, // [0, rowCount) or -1
            out int endRowNumber)   // [0, rowCount) or -1
        {
            int foundRowNumber = BinarySearchReference(
                rowCount,
                rowSize,
                referenceOffset,
                referenceValue,
                isReferenceSmall
            );

            if (foundRowNumber == -1)
            {
                startRowNumber = -1;
                endRowNumber = -1;
                return;
            }

            startRowNumber = foundRowNumber;
            while (startRowNumber > 0 &&
                   PeekReferenceUnchecked((startRowNumber - 1) * rowSize + referenceOffset, isReferenceSmall) == referenceValue)
            {
                startRowNumber--;
            }

            endRowNumber = foundRowNumber;
            while (endRowNumber + 1 < rowCount &&
                   PeekReferenceUnchecked((endRowNumber + 1) * rowSize + referenceOffset, isReferenceSmall) == referenceValue)
            {
                endRowNumber++;
            }
        }

        /// <summary>
        /// Calculates a range of rows that have specified value in the specified column in a table that is sorted by that column.
        /// </summary>
        internal void BinarySearchReferenceRange(
            int[] ptrTable,
            int rowSize,
            int referenceOffset,
            uint referenceValue,
            bool isReferenceSmall,
            out int startRowNumber, // [0, ptrTable.Length) or -1
            out int endRowNumber)   // [0, ptrTable.Length) or -1
        {
            int foundRowNumber = BinarySearchReference(
                ptrTable,
                rowSize,
                referenceOffset,
                referenceValue,
                isReferenceSmall
            );

            if (foundRowNumber == -1)
            {
                startRowNumber = -1;
                endRowNumber = -1;
                return;
            }

            startRowNumber = foundRowNumber;
            while (startRowNumber > 0 &&
                   PeekReferenceUnchecked((ptrTable[startRowNumber - 1] - 1) * rowSize + referenceOffset, isReferenceSmall) == referenceValue)
            {
                startRowNumber--;
            }

            endRowNumber = foundRowNumber;
            while (endRowNumber + 1 < ptrTable.Length &&
                   PeekReferenceUnchecked((ptrTable[endRowNumber + 1] - 1) * rowSize + referenceOffset, isReferenceSmall) == referenceValue)
            {
                endRowNumber++;
            }
        }

        // Always RowNumber....
        internal int LinearSearchReference(
            int rowSize,
            int referenceOffset,
            uint referenceValue,
            bool isReferenceSmall)
        {
            int currOffset = referenceOffset;
            int totalSize = this.Length;
            while (currOffset < totalSize)
            {
                uint currReference = PeekReferenceUnchecked(currOffset, isReferenceSmall);
                if (currReference == referenceValue)
                {
                    return currOffset / rowSize;
                }

                currOffset += rowSize;
            }

            return -1;
        }

        internal bool IsOrderedByReferenceAscending(
            int rowSize,
            int referenceOffset,
            bool isReferenceSmall)
        {
            int offset = referenceOffset;
            int totalSize = this.Length;

            uint previous = 0;
            while (offset < totalSize)
            {
                uint current = PeekReferenceUnchecked(offset, isReferenceSmall);
                if (current < previous)
                {
                    return false;
                }

                previous = current;
                offset += rowSize;
            }

            return true;
        }

        internal int[] BuildPtrTable(
            int numberOfRows,
            int rowSize,
            int referenceOffset,
            bool isReferenceSmall)
        {
            int[] ptrTable = new int[numberOfRows];
            uint[] unsortedReferences = new uint[numberOfRows];

            for (int i = 0; i < ptrTable.Length; i++)
            {
                ptrTable[i] = i + 1;
            }

            ReadColumn(unsortedReferences, rowSize, referenceOffset, isReferenceSmall);
            Array.Sort(ptrTable, (int a, int b) => { return unsortedReferences[a - 1].CompareTo(unsortedReferences[b - 1]); });
            return ptrTable;
        }

        private void ReadColumn(
            uint[] result,
            int rowSize,
            int referenceOffset,
            bool isReferenceSmall)
        {
            int offset = referenceOffset;
            int totalSize = this.Length;

            int i = 0;
            while (offset < totalSize)
            {
                result[i] = PeekReferenceUnchecked(offset, isReferenceSmall);
                offset += rowSize;
                i++;
            }

            Debug.Assert(i == result.Length);
        }

        internal bool PeekHeapValueOffsetAndSize(int index, out int offset, out int size)
        {
            int bytesRead;
            int numberOfBytes = PeekCompressedInteger(index, out bytesRead);
            if (numberOfBytes == BlobReader.InvalidCompressedInteger)
            {
                offset = 0;
                size = 0;
                return false;
            }

            offset = index + bytesRead;
            size = numberOfBytes;
            return true;
        }
    }
}
