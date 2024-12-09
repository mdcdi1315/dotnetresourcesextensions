using System;
using System.Runtime.CompilerServices;


namespace DotNetResourcesExtensions.Internal
{
    internal static unsafe partial class UnsafeMethods
    {
        // Same as GetBytesTemplate but also pins the data to a span.
        [System.Diagnostics.DebuggerHidden]
        private static System.Span<System.Byte> GetBytesTemplate2<T>(T input) where T : unmanaged => new(GetBytesTemplate(input));

        // Same as GetFromBytesTemplate.
        [System.Diagnostics.DebuggerHidden]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208",
            Justification = "This method is always hidden by the debugger and any methods that use this pass the sidx parameter as StartIndex.")]
        private static T GetFromBytesTemplate2<T>(System.Span<System.Byte> span, System.Int32 sidx) where T : unmanaged
        {
            if (sidx < 0 || (sidx + sizeof(T)) > span.Length)
            {
                throw new ArgumentOutOfRangeException("StartIndex", "StartIndex must be more or equal to zero and smaller than the array length minus the size of the structure.");
            }
            return Unsafe.ReadUnaligned<T>(ref span[sidx]);
        }

        #region Convert to numeric types from bytes

        /// <summary>
        /// Gets a <see cref="System.Single"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.Single"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.Single ToSingle(this System.Span<System.Byte> array, System.Int32 StartIndex)
        {
            System.Int32 rn = GetFromBytesTemplate2<System.Int32>(array, StartIndex);
            return LinearConversion<System.Int32, System.Single>(rn);
        }

        /// <summary>
        /// Gets a <see cref="System.Double"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.Double"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.Double ToDouble(this System.Span<System.Byte> array, System.Int32 StartIndex)
        {
            System.Int64 rn = GetFromBytesTemplate2<System.Int64>(array, StartIndex);
            return LinearConversion<System.Int64, System.Double>(rn);
        }

        /// <summary>
        /// Gets a <see cref="System.Decimal"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.Decimal"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.Decimal ToDecimal(this System.Span<System.Byte> array, System.Int32 StartIndex) => GetFromBytesTemplate2<System.Decimal>(array, StartIndex);

        /// <summary>
        /// Gets a <see cref="System.Int16"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.Int16"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.Int16 ToInt16(this System.Span<System.Byte> array, System.Int32 StartIndex)
            => GetFromBytesTemplate2<System.Int16>(array, StartIndex);

        /// <summary>
        /// Gets a <see cref="System.Int32"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.Int32"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.Int32 ToInt32(this System.Span<System.Byte> array, System.Int32 StartIndex)
            => GetFromBytesTemplate2<System.Int32>(array, StartIndex);

        /// <summary>
        /// Gets a <see cref="System.Int64"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.Int64"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.Int64 ToInt64(this System.Span<System.Byte> array, System.Int32 StartIndex)
            => GetFromBytesTemplate2<System.Int64>(array, StartIndex);

        /// <summary>
        /// Gets a <see cref="System.UInt64"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.UInt64"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.UInt64 ToUInt64(this System.Span<System.Byte> array, System.Int32 StartIndex)
            => GetFromBytesTemplate2<System.UInt64>(array, StartIndex);

        /// <summary>
        /// Gets a <see cref="System.UInt32"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.UInt32"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.UInt32 ToUInt32(this System.Span<System.Byte> array, System.Int32 StartIndex)
            => GetFromBytesTemplate2<System.UInt32>(array, StartIndex);

        /// <summary>
        /// Gets a <see cref="System.UInt16"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.UInt16"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.UInt16 ToUInt16(this System.Span<System.Byte> array, System.Int32 StartIndex)
            => GetFromBytesTemplate2<System.UInt16>(array, StartIndex);
        #endregion

        /// <summary>
        /// Casts the given byte span to a span of character array. <br />
        /// The bytes can only contain ANSI characters (ranging 0..255).
        /// </summary>
        /// <param name="span">The byte span to cast.</param>
        /// <param name="startindex">The starting index to start copying from.</param>
        /// <returns>A new span that has been casted.</returns>
        public static System.Span<System.Char> CastToChars(this System.Span<System.Byte> span, System.Int32 startindex)
        {
            System.Span<System.Char> ret = new(new System.Char[span.Length - startindex]);
            for (System.Int32 I = startindex , J = 0; I < span.Length; I++ , J++)
            {
                ret[J] = span[I].ToChar();
            }
            return ret;
        }
    }
}
