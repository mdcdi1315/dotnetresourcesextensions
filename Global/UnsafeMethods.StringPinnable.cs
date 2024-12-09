using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace DotNetResourcesExtensions.Internal
{
    /// <summary>
    /// Represents the private data of the official <see cref="System.String"/> structure. <br />
    /// Use this pinnable when you want to do pointer operations and avoid allocating unnecessarily new arrays.
    /// </summary>
    // A pinnable that gets the offset to data when we are on any platform.
    // Seems that the String has not been structurally changed during the time ...
    // Required for UnsafeMethods class.
    [StructLayout(LayoutKind.Explicit, Size = 8)] // As the size of System.String is
    internal sealed unsafe class StringPinnable
    {
        /// <summary>
        /// Gets a new instance of <see cref="StringPinnable"/> from the specified data.
        /// </summary>
        /// <param name="data">The data to get from.</param>
        /// <returns>A new <see cref="StringPinnable"/> that is the unmanaged surface of <paramref name="data"/>.</returns>
        public static StringPinnable GetAsPinnable(System.String data) => Unsafe.As<StringPinnable>(data);

        public static unsafe StringPinnable ReadFromArray(System.Char[] chars, System.Int32 startindex)
        {
            if (chars is null) { throw new ArgumentNullException(nameof(chars)); }
            if (startindex < 0 || startindex >= chars.Length) { throw new ArgumentOutOfRangeException(nameof(startindex), "startindex must not be negative and be less than the array length."); }
            StringPinnable pinnable = new();
            pinnable.Length = chars.Length - startindex;
            pinnable.Data = (System.Char*)Unsafe.AsPointer(ref chars[startindex]);
            return pinnable;
        }

        /*
        public static unsafe StringPinnable Create(System.Int32 length)
        {
            StringPinnable pinnable = new();
            if (length < 0) { throw new ArgumentOutOfRangeException(nameof(length), "length must not be negative."); }
            pinnable.Length = length;
            fixed (System.Char* target = &pinnable.Data)
            {
                Unsafe.InitBlock(target, 48, (pinnable.Length * Unsafe.SizeOf<System.Char>()).ToUInt32());
            }
            return pinnable;
        }
        */

        // Avoiding direct construction - use the GetAsPinnable method.
        private StringPinnable() { }

        /// <summary>
        /// The length of the provided string.
        /// </summary>
        [FieldOffset(0)] public System.Int32 Length;
        /// <summary>
        /// The original pointer of the string. Use the &amp; operator to get a pointer to the whole data array.
        /// </summary>
        [FieldOffset(4)] public System.Char* Data;
        /// <summary>
        /// The first element of the string.
        /// </summary>
        [FieldOffset(4)] public System.Char First;

        /// <summary>
        /// Reconverts back to the original string if you want it again...
        /// </summary>
        public override string ToString() => new(Data);
    }

    internal static unsafe partial class UnsafeMethods
    {
        /// <summary>
        /// Uses unsafe schemes to copy a string to a new character array , starting from the specified index and copying <paramref name="count"/> charaters to target.
        /// </summary>
        /// <param name="str">The string to copy.</param>
        /// <param name="index">The index to start copying characters from.</param>
        /// <param name="count">The number of characters to copy.</param>
        /// <returns>A new array which has characters copied from <paramref name="str"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> or <paramref name="count"/> were less than zero , or <paramref name="count"/> was greater than the input string.</exception>
        public static System.Char[] ToCharArrayUnsafe(this System.String str, System.Int32 index, System.Int32 count)
        {
            if (str is null) { return System.Array.Empty<System.Char>(); }
            if (index < 0) { throw new ArgumentOutOfRangeException(nameof(index), "index must not be negative."); }
            if (count < 0) { throw new ArgumentOutOfRangeException(nameof(index), "count must be positive."); }
            StringPinnable pnt = StringPinnable.GetAsPinnable(str);
            if (count == 0 || pnt.Length == 0) { return System.Array.Empty<System.Char>(); }
            if (count > pnt.Length - index)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"Count must be less than or equal to {pnt.Length - index} .");
            }
            System.Char[] result = new System.Char[count];
            fixed (System.Char* ptr = result)
            {
                // Gets the starting reference of pnt.Data where the user prefers to start copying from.
                // If index == 0 (might be the most common case btw) , then pnt.Data is returned from Unsafe.Add as-it-is.
                System.Char* src = (System.Char*)Unsafe.Add<System.Char>(pnt.Data, index);
                // Uses cpblk , fast and efficient.
                // Also be noted that we need byte count , and not character count , so we convert it to bytes at run-time.
                Unsafe.CopyBlockUnaligned(ptr, src, (count * Unsafe.SizeOf<System.Char>()).ToUInt32());
            }
            return result;
        }

        /// <summary>
        /// Uses unsafe schemes to copy a string to a new character array.
        /// </summary>
        /// <param name="str">The string to copy.</param>
        /// <returns>A new array which has characters copied from <paramref name="str"/>.</returns>
        public static System.Char[] ToCharArrayUnsafe(this System.String str) => ToCharArrayUnsafe(str, 0, str.Length);

    }
}
