using System;
using System.Runtime.CompilerServices;

namespace DotNetResourcesExtensions.Internal
{
    // Defines unsafe methods for manipulating primitive data.

    internal static class UnsafeMethods
    {
        // The unsafe calls allow us to bypass definite runtime checks for conversions.
        private static unsafe TResult WideningConversion<TInput, TResult>(TInput input)
            where TResult : unmanaged
            where TInput : unmanaged
        {
            // Defines a numeric conversion between two numbers that is widening.
            System.Int32 resultsize = sizeof(TResult), inputsize = sizeof(TInput);
            if (resultsize < inputsize) { throw new ArgumentException("The input type given must be smaller than the output type."); }
            System.Byte[] bytes = new System.Byte[resultsize];
            Unsafe.As<System.Byte, TInput>(ref bytes[0]) = input;
            return Unsafe.ReadUnaligned<TResult>(ref bytes[0]);
        }

        private static unsafe TResult NarrowingConversion<TInput, TResult>(TInput input)
            where TResult : unmanaged
            where TInput : unmanaged
        {
            // Defines a numeric conversion between two numbers that is narrowing.
            System.Int32 resultsize = sizeof(TResult), inputsize = sizeof(TInput);
            if (resultsize >= inputsize) { throw new ArgumentException("The input type given must be larger than the output type."); }
            // If we fed to the array size the resultsize variable , the unsafe calls would cause a buffer overrun.
            // Instead , the array size will be determined by inputsize , and then the ReadUnaligned call will decide how many bytes it will use.
            System.Byte[] bytes = new System.Byte[inputsize];
            Unsafe.As<System.Byte, TInput>(ref bytes[0]) = input;
            return Unsafe.ReadUnaligned<TResult>(ref bytes[0]);
        }

        private static unsafe System.Byte[] GetBytesTemplate<T>(T input) where T : unmanaged
        {
            System.Byte[] bytes = new System.Byte[sizeof(T)];
            Unsafe.As<System.Byte, T>(ref bytes[0]) = input;
            return bytes;
        }

        private static unsafe System.Span<System.Byte> GetBytesTemplate2<T>(T input) where T : unmanaged
        {
            System.Span<System.Byte> result = new(new System.Byte[sizeof(T)]);
            Unsafe.As<System.Byte, T>(ref result[0]) = input;
            return result;
        }

        private static unsafe T GetFromBytesTemplate<T>(System.Byte[] bytes , System.Int32 sidx) where T : unmanaged
        {
            if (sizeof(T) != bytes.Length)
            {
                throw new ArgumentException("bytes must have the same length as the resulting structure's size." , nameof(bytes));
            }
            if (sidx < 0 || (sidx + sizeof(T)) > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("StartIndex" , "StartIndex must be more or equal to zero and smaller than the array length plus the size of the structure.");
            }
            return Unsafe.ReadUnaligned<T>(ref bytes[sidx]);
        }

        private static unsafe T GetFromBytesTemplate2<T>(System.Span<System.Byte> span , System.Int32 sidx) where T : unmanaged
        {
            if (sizeof(T) != span.Length)
            {
                throw new ArgumentException("span must have the same length as the resulting structure's size.", nameof(span));
            }
            if (sidx < 0 || (sidx + sizeof(T)) > span.Length)
            {
                throw new ArgumentOutOfRangeException("StartIndex", "StartIndex must be more or equal to zero and smaller than the array length minus the size of the structure.");
            }
            return Unsafe.ReadUnaligned<T>(ref span[sidx]);
        }

        /// <summary>
        /// Reverses endianess for the primitive type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The primitive type to reverse endianess for.</typeparam>
        /// <param name="input">The input type to reverse.</param>
        /// <returns>The reversed endianess result of <paramref name="input"/>.</returns>
        public static unsafe T ReverseEndianess<T>(T input) where T : unmanaged
        {
            System.Byte[] bt = GetBytesTemplate(input);
            System.Array.Reverse(bt);
            return GetFromBytesTemplate<T>(bt , 0);
        }

        public static System.Int64 ToInt64(this System.Int32 number) => WideningConversion<System.Int32, System.Int64>(number);

        public static System.UInt64 ToUInt64(this System.UInt32 number) => WideningConversion<System.UInt32, System.UInt64>(number);

        public static System.Int32 ToInt32(this System.Int16 number) => WideningConversion<System.Int16, System.Int32>(number);

        public static System.Int16 ToInt16(this System.Byte number) => WideningConversion<System.Byte, System.Int16>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int64"/> to a <see cref="System.Int32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int32 ToInt32(this System.Int64 number) => NarrowingConversion<System.Int64, System.Int32>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.UInt64"/> to a <see cref="System.UInt32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.UInt32 ToUInt32(this System.UInt64 number) => NarrowingConversion<System.UInt64, System.UInt32>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int32"/> to a <see cref="System.Int16"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int16 ToInt16(this System.Int32 number) => NarrowingConversion<System.Int32, System.Int16>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.UInt32"/> to a <see cref="System.UInt16"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.UInt16 ToUInt16(this System.UInt32 number) => NarrowingConversion<System.UInt32, System.UInt16>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int16"/> to a <see cref="System.Byte"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Byte ToByte(this System.Int16 number) => NarrowingConversion<System.Int16, System.Byte>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int32"/> to a <see cref="System.Byte"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Byte ToByte(this System.Int32 number) => NarrowingConversion<System.Int32 , System.Byte>(number);

        public static System.Byte[] GetBytes(this System.Int64 number) => GetBytesTemplate(number);

        public static System.Byte[] GetBytes(this System.UInt64 number) => GetBytesTemplate(number);

        public static System.Byte[] GetBytes(this System.UInt32 number) => GetBytesTemplate(number);

        public static System.Byte[] GetBytes(this System.UInt16 number) => GetBytesTemplate(number);

        public static System.Byte[] GetBytes(this System.Int16 number) => GetBytesTemplate(number);

        public static System.Byte[] GetBytes(this System.Int32 number) => GetBytesTemplate(number);

        public static System.Int16 ToInt16(this System.Byte[] array, System.Int32 StartIndex)
            => GetFromBytesTemplate<System.Int16>(array, StartIndex);

        public static System.Int32 ToInt32(this System.Byte[] array, System.Int32 StartIndex)
            => GetFromBytesTemplate<System.Int32>(array, StartIndex);

        public static System.Int64 ToInt64(this System.Byte[] array, System.Int32 StartIndex)
            => GetFromBytesTemplate<System.Int64>(array, StartIndex);

        public static System.UInt64 ToUInt64(this System.Byte[] array, System.Int32 StartIndex)
            => GetFromBytesTemplate<System.UInt64>(array, StartIndex);

        public static System.UInt32 ToUInt32(this System.Byte[] array, System.Int32 StartIndex)
            => GetFromBytesTemplate<System.UInt32>(array, StartIndex);

        public static System.UInt16 ToUInt16(this System.Byte[] array, System.Int32 StartIndex)
            => GetFromBytesTemplate<System.UInt16>(array, StartIndex);
    }
}
