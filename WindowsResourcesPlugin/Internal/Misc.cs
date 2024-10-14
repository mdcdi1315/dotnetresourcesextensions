using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// Contains internal implementations that are only required for specific tasks.

namespace DotNetResourcesExtensions.Internal
{

    // Portions of the class code are from the Reference Source of .NET Framework.
    internal static class MinimalHexDecoder
    {
        static byte[] char2val = new byte[128]
        {
                    /*    0-15 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /*   16-31 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /*   32-47 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /*   48-63 */ 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /*   64-79 */ 0xFF, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /*   80-95 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /*  96-111 */ 0xFF, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /* 112-127 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        };

        private static int GetByteCount(int charCount)
        {
            if (charCount < 0)
                throw new ArgumentOutOfRangeException(nameof(charCount), "charCount must not be negative.");
            if ((charCount % 2) != 0)
                throw new FormatException("The given binary hexadecimal length is invalid.");
            return charCount / 2;
        }

        [System.Security.SecuritySafeCritical]
        public static unsafe int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (chars is null)
                throw new ArgumentNullException(nameof(chars));
            if (charIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(charIndex), "charIndex must not be negative.");
            if (charIndex > chars.Length)
                throw new ArgumentOutOfRangeException(nameof(charIndex), "charIndex must not be larger than the array length.");
            if (charCount < 0)
                throw new ArgumentOutOfRangeException(nameof(charCount), "charCount must not be negative.");
            if (charCount > chars.Length - charIndex)
                throw new ArgumentOutOfRangeException(nameof(charCount), "The character count exceeds the character buffer size.");
            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes));
            if (byteIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(byteIndex), "byteIndex must not be negative.");
            if (byteIndex > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(byteIndex), "The byte index must not excceed the array size.");
            int byteCount = GetByteCount(charCount);
            if (byteCount < 0 || byteCount > bytes.Length - byteIndex)
                throw new ArgumentException("The byte array is too small to fit the resulting data.", nameof(bytes));
            if (charCount > 0)
            {
                fixed (byte* _char2val = char2val)
                {
                    fixed (byte* _bytes = &bytes[byteIndex])
                    {
                        fixed (char* _chars = &chars[charIndex])
                        {
                            char* pch = _chars;
                            char* pchMax = _chars + charCount;
                            byte* pb = _bytes;
                            while (pch < pchMax)
                            {
                                System.Diagnostics.Debug.Assert(pch + 2 <= pchMax, "");
                                char pch0 = pch[0];
                                char pch1 = pch[1];
                                if ((pch0 | pch1) >= 128)
                                    throw new FormatException($"An invalid HEX sequence was found: {new System.String(pch, 0, 2)}");
                                byte d1 = _char2val[pch0];
                                byte d2 = _char2val[pch1];
                                if ((d1 | d2) == 0xFF)
                                    throw new FormatException($"An invalid HEX sequence was found: {new System.String(pch, 0, 2)}");
                                pb[0] = (byte)((d1 << 4) + d2);
                                pch += 2;
                                pb++;
                            }
                        }
                    }
                }
            }
            return byteCount;
        }

        public static System.Byte[] GetBytes(System.String str, System.Int32 index, System.Int32 count)
        {
            System.Byte[] bytes = new System.Byte[GetByteCount(count)];
            GetBytes(str.ToCharArray(), index, count, bytes, 0);
            return bytes;
        }
    }

    // The below classes are retrieved from the WinForms project:
    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    /// <summary>
    ///  Fast stack based <see cref="ReadOnlySpan{T}"/> reader.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Care must be used when reading struct values that depend on a specific field state for members to work
    ///   correctly. For example, <see cref="DateTime"/> has a very specific set of valid values for its packed
    ///   <see langword="ulong"/> field.
    ///  </para>
    ///  <para>
    ///   Inspired by SequenceReader patterns.
    ///  </para>
    /// </remarks>
    internal unsafe ref struct SpanReader<T> where T : unmanaged, IEquatable<T>
    {
        private ReadOnlySpan<T> _unread;

        public ReadOnlySpan<T> Span { get; }

        public SpanReader(ReadOnlySpan<T> span)
        {
            Span = span;
            _unread = span;
        }

        public int Position
        {
            readonly get => Span.Length - _unread.Length;
            set => _unread = Span[value..];
        }

        /// <summary>
        ///  Try to read everything up to the given <paramref name="delimiter"/>. Advances the reader past the
        ///  <paramref name="delimiter"/> if found.
        /// </summary>
        /// <inheritdoc cref="TryReadTo(T, bool, out ReadOnlySpan{T})"/>
        public bool TryReadTo(T delimiter, out ReadOnlySpan<T> span) =>
            TryReadTo(delimiter, advancePastDelimiter: true, out span);

        /// <summary>
        ///  Try to read everything up to the given <paramref name="delimiter"/>.
        /// </summary>
        /// <param name="span">The read data, if any.</param>
        /// <param name="delimiter">The delimiter to look for.</param>
        /// <param name="advancePastDelimiter"><see langword="true"/> to move past the <paramref name="delimiter"/> if found.</param>
        /// <returns><see langword="true"/> if the <paramref name="delimiter"/> was found.</returns>
        public bool TryReadTo(T delimiter, bool advancePastDelimiter, out ReadOnlySpan<T> span)
        {
            bool found = false;
            int index = _unread.IndexOf(delimiter);
            span = default;

            if (index != -1)
            {
                found = true;
                if (index > 0)
                {
                    span = _unread;
                    UncheckedSliceTo(ref span, index);
                    if (advancePastDelimiter)
                    {
                        index++;
                    }

                    UnsafeAdvance(index);
                }
            }

            return found;
        }

        /// <summary>
        ///  Try to read the next value.
        /// </summary>
        public bool TryRead(out T value)
        {
            bool success;

            if (_unread.IsEmpty)
            {
                value = default;
                success = false;
            }
            else
            {
                success = true;
                value = _unread[0];
                UnsafeAdvance(1);
            }

            return success;
        }

        /// <summary>
        ///  Try to read a span of the given <paramref name="count"/>.
        /// </summary>
        public bool TryRead(int count, out ReadOnlySpan<T> span)
        {
            bool success;

            if (count > _unread.Length)
            {
                span = default;
                success = false;
            }
            else
            {
                success = true;
                span = _unread[..count];
                UnsafeAdvance(count);
            }

            return success;
        }

        /// <summary>
        ///  Try to read a value of the given type. The size of the value must be evenly divisible by the size of
        ///  <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   This is just a straight copy of bits. If <typeparamref name="TValue"/> has methods that depend on
        ///   specific field value constraints this could be unsafe.
        ///  </para>
        ///  <para>
        ///   The compiler will often optimize away the struct copy if you only read from the value.
        ///  </para>
        /// </remarks>
        public bool TryRead<TValue>(out TValue value) where TValue : unmanaged
        {
            if (sizeof(TValue) < sizeof(T) || sizeof(TValue) % sizeof(T) != 0)
            {
                throw new ArgumentException($"The size of {nameof(TValue)} must be evenly divisible by the size of {nameof(T)}.");
            }

            bool success;

            if (sizeof(TValue) > _unread.Length * sizeof(T))
            {
                value = default;
                success = false;
            }
            else
            {
                success = true;
                value = Unsafe.ReadUnaligned<TValue>(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(_unread)));
                UnsafeAdvance(sizeof(TValue) / sizeof(T));
            }

            return success;
        }

        /// <summary>
        ///  Try to read a span of values of the given type. The size of the value must be evenly divisible by the size of
        ///  <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   This effectively does a <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/> and the same
        ///   caveats apply about safety.
        ///  </para>
        /// </remarks>
        public bool TryRead<TValue>(int count, out ReadOnlySpan<TValue> value) where TValue : unmanaged
        {
            if (sizeof(TValue) < sizeof(T) || sizeof(TValue) % sizeof(T) != 0)
            {
                throw new ArgumentException($"The size of {nameof(TValue)} must be evenly divisible by the size of {nameof(T)}.");
            }

            bool success;

            if (sizeof(TValue) * count > _unread.Length * sizeof(T))
            {
                value = default;
                success = false;
            }
            else
            {
                success = true;
                value = new ReadOnlySpan<TValue>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(_unread)) , count);
                UnsafeAdvance((sizeof(TValue) / sizeof(T)) * count);
            }

            return success;
        }

        /// <summary>
        ///  Check to see if the given <paramref name="next"/> values are next.
        /// </summary>
        /// <param name="next">The span to compare the next items to.</param>
        public readonly bool IsNext(ReadOnlySpan<T> next) => _unread.StartsWith(next);

        /// <summary>
        ///  Advance the reader if the given <paramref name="next"/> values are next.
        /// </summary>
        /// <param name="next">The span to compare the next items to.</param>
        /// <returns><see langword="true"/> if the values were found and the reader advanced.</returns>
        public bool TryAdvancePast(ReadOnlySpan<T> next)
        {
            bool success = false;
            if (_unread.StartsWith(next))
            {
                UnsafeAdvance(next.Length);
                success = true;
            }

            return success;
        }

        private System.Int32 _private_IndexOfAnyExcept<TI>(ReadOnlySpan<TI> span ,TI value)
        {
            static int Implementation<TD>(ref TD searchSpace, TD value0, int length)
            {
                Debug.Assert(length >= 0, "Expected non-negative length");
                System.Collections.Generic.EqualityComparer<TD> eqc = System.Collections.Generic.EqualityComparer<TD>.Default;

                for (int i = 0; i < length; i++)
                {
                    if (eqc.Equals(Unsafe.Add(ref searchSpace, i), value0) == false)
                    {
                        eqc = null;
                        return i;
                    }
                }
                eqc = null;
                return -1;
            }
            return Implementation(ref MemoryMarshal.GetReference(span), value, span.Length);
        }

        /// <summary>
        ///  Advance the reader past consecutive instances of the given <paramref name="value"/>.
        /// </summary>
        /// <returns>How many positions the reader has been advanced</returns>
        public int AdvancePast(T value)
        {
            int count = 0;

            int index = _private_IndexOfAnyExcept(_unread, value);
            if (index == -1)
            {
                // Everything left is the value
                count = _unread.Length;
                _unread = default;
            }
            else if (index != 0)
            {
                count = index;
                UnsafeAdvance(index);
            }

            return count;
        }

        /// <summary>
        ///  Advance the reader by the given <paramref name="count"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count) => _unread = _unread[count..];

        /// <summary>
        ///  Rewind the reader by the given <paramref name="count"/>.
        /// </summary>
        public void Rewind(int count) => _unread = Span[(Span.Length - _unread.Length - count)..];

        /// <summary>
        ///  Reset the reader to the beginning of the span.
        /// </summary>
        public void Reset() => _unread = Span;

        /// <summary>
        ///  Advance the reader without bounds checking.
        /// </summary>
        /// <param name="count"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UnsafeAdvance(int count)
        {
            Debug.Assert((uint)count <= (uint)_unread.Length);
            UncheckedSlice(ref _unread, count, _unread.Length - count);
        }

        /// <summary>
        ///  Slicing without bounds checking.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UncheckedSliceTo(ref ReadOnlySpan<T> span, int length)
        {
            Debug.Assert((uint)length <= (uint)span.Length);
            span = new ReadOnlySpan<T>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), length);
        }

        /// <summary>
        ///  Slicing without bounds checking.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UncheckedSlice(ref ReadOnlySpan<T> span, int start, int length)
        {
            Debug.Assert((uint)start <= (uint)span.Length && (uint)length <= (uint)(span.Length - start));
            span = new System.ReadOnlySpan<T>(Unsafe.Add<T>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), start), length);
        }
    }

    /// <summary>
    ///  Fast stack based <see cref="Span{T}"/> writer.
    /// </summary>
    internal unsafe ref struct SpanWriter<T> where T : unmanaged, IEquatable<T>
    {
        private Span<T> _unwritten;
        public Span<T> Span { get; }

        public SpanWriter(Span<T> span)
        {
            _unwritten = span;
            Span = span;
        }

        public int Position
        {
            readonly get => Span.Length - _unwritten.Length;
            set => _unwritten = Span[value..];
        }

        public readonly int Length => Span.Length;

        /// <summary>
        ///  Try to write the given value.
        /// </summary>
        public bool TryWrite(T value)
        {
            bool success = false;

            if (!_unwritten.IsEmpty)
            {
                success = true;
                _unwritten[0] = value;
                UnsafeAdvance(1);
            }

            return success;
        }

        /// <summary>
        ///  Try to write the given value.
        /// </summary>
        public bool TryWrite(ReadOnlySpan<T> values)
        {
            bool success = false;

            if (_unwritten.Length >= values.Length)
            {
                success = true;
                values.CopyTo(_unwritten);
                UnsafeAdvance(values.Length);
            }

            return success;
        }

        /// <summary>
        ///  Try to write the given value <paramref name="count"/> times.
        /// </summary>
        public bool TryWriteCount(int count, T value)
        {
            bool success = false;

            if (_unwritten.Length >= count)
            {
                success = true;
                _unwritten[..count].Fill(value);
                UnsafeAdvance(count);
            }

            return success;
        }

        /// <summary>
        ///  Advance the writer by the given <paramref name="count"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count) => _unwritten = _unwritten[count..];

        /// <summary>
        ///  Rewind the writer by the given <paramref name="count"/>.
        /// </summary>
        public void Rewind(int count) => _unwritten = Span[(Span.Length - _unwritten.Length - count)..];

        /// <summary>
        ///  Reset the reader to the beginning of the span.
        /// </summary>
        public void Reset() => _unwritten = Span;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UnsafeAdvance(int count)
        {
            Debug.Assert((uint)count <= (uint)_unwritten.Length);
            UncheckedSlice(ref _unwritten, count, _unwritten.Length - count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UncheckedSlice(ref Span<T> span, int start, int length)
        {
            Debug.Assert((uint)start <= (uint)span.Length && (uint)length <= (uint)(span.Length - start));
            span = new System.Span<T>(Unsafe.Add<T>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), start) , length);
        }
    }

    /// <summary>
    ///  Simple run length encoder (RLE) that works on spans.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Format used is a byte for the count, followed by a byte for the value.
    ///  </para>
    /// </remarks>
    internal static class RunLengthEncoder
    {
        /// <summary>
        ///  Get the encoded length, in bytes, of the given data.
        /// </summary>
        public static int GetEncodedLength(System.ReadOnlySpan<System.Byte> data)
        {
            SpanReader<byte> reader = new(data);

            int length = 0;
            while (reader.TryRead(out byte value))
            {
                int count = reader.AdvancePast(value) + 1;
                while (count > 0)
                {
                    // 1 byte for the count, 1 byte for the value
                    length += 2;
                    count -= 0xFF;
                }
            }

            return length;
        }

        /// <summary>
        ///  Get the decoded length, in bytes, of the given encoded data.
        /// </summary>
        public static int GetDecodedLength(ReadOnlySpan<System.Byte> encoded)
        {
            int length = 0;
            for (int i = 0; i < encoded.Length; i += 2)
            {
                length += encoded[i];
            }

            return length;
        }

        /// <summary>
        ///  Encode the given data into the given <paramref name="encoded"/> span.
        /// </summary>
        /// <returns>
        ///  <see langword="false"/> if the <paramref name="encoded"/> span was not large enough to hold the encoded data.
        /// </returns>
        public static bool TryEncode(ReadOnlySpan<byte> data, Span<byte> encoded, out int written)
        {
            SpanReader<byte> reader = new(data);
            SpanWriter<byte> writer = new(encoded);

            while (reader.TryRead(out byte value))
            {
                int count = reader.AdvancePast(value) + 1;
                while (count > 0)
                {
                    if (!writer.TryWrite((byte)Math.Min(count, 0xFF)) || !writer.TryWrite(value))
                    {
                        written = writer.Position;
                        return false;
                    }

                    count -= 0xFF;
                }
            }

            written = writer.Position;
            return true;
        }

        public static bool TryDecode(ReadOnlySpan<byte> encoded, Span<byte> data, out int written)
        {
            SpanReader<byte> reader = new(encoded);
            SpanWriter<byte> writer = new(data);

            while (reader.TryRead(out byte count))
            {
                if (!reader.TryRead(out byte value) || !writer.TryWriteCount(count, value))
                {
                    written = writer.Position;
                    return false;
                }
            }

            written = writer.Position;
            return true;
        }
    }


}
