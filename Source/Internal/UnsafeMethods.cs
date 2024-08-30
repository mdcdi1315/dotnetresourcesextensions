using System;
using System.Runtime.CompilerServices;

// The below warning is disabled because all calls that evolve it are sizeof's which only get sizes of unmanaged types.
#pragma warning disable 8500 // Declares a pointer to , takes the address of , or gets the size of a managed type

namespace DotNetResourcesExtensions.Internal
{
    /// <summary>
    /// Provides unsafe extension methods for the <see cref="DotNetResourcesExtensions"/> project
    /// for manipulating primitive information.
    /// </summary>
    internal static unsafe class UnsafeMethods
    {
        // The unsafe calls allow us to bypass definite runtime checks for conversions.
        [System.Diagnostics.DebuggerHidden]
        private static TResult WideningConversion<TInput, TResult>(TInput input)
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

        [System.Diagnostics.DebuggerHidden]
        private static TResult NarrowingConversion<TInput, TResult>(TInput input)
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

        // NOTE: The method implementation is the same as the Unsafe.BitCast class.
        [System.Diagnostics.DebuggerHidden]
        private static TTo LinearConversion<TFrom, TTo>(TFrom source)
            where TFrom : struct
            where TTo : struct
        {
            if (sizeof(TFrom) != sizeof(TTo)) { throw new NotSupportedException("The structures must have the same size so that the linear conversion can be performed."); }
            return Unsafe.ReadUnaligned<TTo>(ref Unsafe.As<TFrom, byte>(ref source));
        }

        [System.Diagnostics.DebuggerHidden]
        private static System.Byte[] GetBytesTemplate<T>(T input) where T : unmanaged
        {
            System.Byte[] bytes = new System.Byte[sizeof(T)];
            Unsafe.As<System.Byte, T>(ref bytes[0]) = input;
            return bytes;
        }

        [System.Diagnostics.DebuggerHidden]
        private static System.Span<System.Byte> GetBytesTemplate2<T>(T input) where T : unmanaged
        {
            System.Span<System.Byte> result = new(new System.Byte[sizeof(T)]);
            Unsafe.As<System.Byte, T>(ref result[0]) = input;
            return result;
        }

        [System.Diagnostics.DebuggerHidden]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208",
            Justification = "This method is always hidden by the debugger and any methods that use this pass the sidx parameter as StartIndex.")]
        private static T GetFromBytesTemplate<T>(System.Byte[] bytes , System.Int32 sidx) where T : unmanaged
        {
            if (sidx < 0 || (sidx + sizeof(T)) > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("StartIndex" , "StartIndex must be more or equal to zero and smaller than the array length plus the size of the structure.");
            }
            return Unsafe.ReadUnaligned<T>(ref bytes[sidx]);
        }

        [System.Diagnostics.DebuggerHidden]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage" , "CA2208" , 
            Justification = "This method is always hidden by the debugger and any methods that use this pass the sidx parameter as StartIndex.")]
        private static T GetFromBytesTemplate2<T>(System.Span<System.Byte> span , System.Int32 sidx) where T : unmanaged
        {
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
        public static T ReverseEndianess<T>(this T input) where T : unmanaged
        {
            System.Byte[] bt = GetBytesTemplate(input);
            bt.Reverse();
            return GetFromBytesTemplate<T>(bt , 0);
        }

        /// <summary>
        /// Reverses the entire sequence of the array elements.
        /// </summary>
        /// <param name="array">The array to be reversed.</param>
        /// <typeparam name="T">The type of a single element inside the array , the array type.</typeparam>
        public static void Reverse<T>(this T[] array) where T : notnull
        {
            // This allows an optimized method for doing reversals faster in .NET Framework.
            // For Core runtimes it could be said that their perf is better than this ,
            // but this is also acceptable , due to the fact that it uses pinned pointers (which all runtimes can easily manipulate them).
            ((System.Span<T>)array).Reverse();
        }

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int32"/> to a <see cref="System.Int64"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int64 ToInt64(this System.Int32 number) => WideningConversion<System.Int32, System.Int64>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int64"/> to a <see cref="System.UInt64"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.UInt64 ToUInt64(this System.Int64 number) => LinearConversion<System.Int64 , System.UInt64>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.UInt32"/> to a <see cref="System.UInt64"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.UInt64 ToUInt64(this System.UInt32 number) => WideningConversion<System.UInt32, System.UInt64>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int16"/> to a <see cref="System.Int32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int32 ToInt32(this System.Int16 number) => WideningConversion<System.Int16, System.Int32>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int32"/> to a <see cref="System.UInt32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.UInt32 ToUInt32(this System.Int32 number) => LinearConversion<System.Int32 , System.UInt32>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Byte"/> to a <see cref="System.Int16"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
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
        /// Uses unsafe schemes to convert a <see cref="System.Int16"/> to a <see cref="System.UInt16"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.UInt16 ToUInt16(this System.Int16 number) => LinearConversion<System.Int16, System.UInt16>(number);

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

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int64"/> to a <see cref="System.Char"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Char ToChar(this System.Int64 number) => NarrowingConversion<System.Int64, System.Char>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int32"/> to a <see cref="System.Char"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Char ToChar(this System.Int32 number) => NarrowingConversion<System.Int32, System.Char>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Char"/> to a <see cref="System.Int64"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="character">The character to convert.</param>
        public static System.Int64 ToInt64(this System.Char character) => WideningConversion<System.Char, System.Int64>(character);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Char"/> to a <see cref="System.Int32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="character">The character to convert.</param>
        public static System.Int32 ToInt32(this System.Char character) => WideningConversion<System.Char, System.Int32>(character);

        /// <summary>
        /// Returns the equivalent byte array representation of this number.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <returns>The equivalent array representation of <paramref name="number"/>.</returns>
        public static System.Byte[] GetBytes(this System.Int64 number) => GetBytesTemplate(number);

        /// <summary>
        /// Returns the equivalent byte array representation of this number.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <returns>The equivalent array representation of <paramref name="number"/>.</returns>
        public static System.Byte[] GetBytes(this System.UInt64 number) => GetBytesTemplate(number);

        /// <summary>
        /// Returns the equivalent byte array representation of this number.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <returns>The equivalent array representation of <paramref name="number"/>.</returns>
        public static System.Byte[] GetBytes(this System.UInt32 number) => GetBytesTemplate(number);

        /// <summary>
        /// Returns the equivalent byte array representation of this number.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <returns>The equivalent array representation of <paramref name="number"/>.</returns>
        public static System.Byte[] GetBytes(this System.UInt16 number) => GetBytesTemplate(number);

        /// <summary>
        /// Returns the equivalent byte array representation of this number.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <returns>The equivalent array representation of <paramref name="number"/>.</returns>
        public static System.Byte[] GetBytes(this System.Int16 number) => GetBytesTemplate(number);

        /// <summary>
        /// Returns the equivalent byte array representation of this number.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <returns>The equivalent array representation of <paramref name="number"/>.</returns>
        public static System.Byte[] GetBytes(this System.Int32 number) => GetBytesTemplate(number);

        /// <summary>
        /// Gets a <see cref="System.Int16"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.Int16"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.Int16 ToInt16(this System.Byte[] array, System.Int32 StartIndex)
            => GetFromBytesTemplate<System.Int16>(array, StartIndex);

        /// <summary>
        /// Gets a <see cref="System.Int32"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.Int32"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.Int32 ToInt32(this System.Byte[] array, System.Int32 StartIndex)
            => GetFromBytesTemplate<System.Int32>(array, StartIndex);

        /// <summary>
        /// Gets a <see cref="System.Int64"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.Int64"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.Int64 ToInt64(this System.Byte[] array, System.Int32 StartIndex)
            => GetFromBytesTemplate<System.Int64>(array, StartIndex);

        /// <summary>
        /// Gets a <see cref="System.UInt64"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.UInt64"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.UInt64 ToUInt64(this System.Byte[] array, System.Int32 StartIndex)
            => GetFromBytesTemplate<System.UInt64>(array, StartIndex);

        /// <summary>
        /// Gets a <see cref="System.UInt32"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.UInt32"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.UInt32 ToUInt32(this System.Byte[] array, System.Int32 StartIndex)
            => GetFromBytesTemplate<System.UInt32>(array, StartIndex);

        /// <summary>
        /// Gets a <see cref="System.UInt16"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.UInt16"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.UInt16 ToUInt16(this System.Byte[] array, System.Int32 StartIndex)
            => GetFromBytesTemplate<System.UInt16>(array, StartIndex);

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
    }
}
