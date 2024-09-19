using System;
using System.Runtime.CompilerServices;

// The below warning is disabled because all calls that evolve it are sizeof's which only get sizes of unmanaged types.
#pragma warning disable 8500 // Declares a pointer to , takes the address of , or gets the size of a managed type

namespace DotNetResourcesExtensions.Internal
{
    /// <summary>
    /// Provides unsafe extension methods for the <see cref="DotNetResourcesExtensions"/> project
    /// for manipulating primitive information and byte arrays.
    /// </summary>
    internal static unsafe class UnsafeMethods
    {
        // The unsafe calls allow us to bypass definite runtime checks for conversions.
        // These are also implemented in pure MSIL - which means that these can work in any possible .NET platform.
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

        // Reinterprets the given unmanaged type to an equivalent and recreatable representation 
        // in a plain byte array.
        [System.Diagnostics.DebuggerHidden]
        private static System.Byte[] GetBytesTemplate<T>(T input) where T : unmanaged
        {
            System.Byte[] bytes = new System.Byte[sizeof(T)];
            Unsafe.As<System.Byte, T>(ref bytes[0]) = input;
            return bytes;
        }

        // Same as above but also pins the data to a span.
        [System.Diagnostics.DebuggerHidden]
        private static System.Span<System.Byte> GetBytesTemplate2<T>(T input) where T : unmanaged => new(GetBytesTemplate(input));

        // Converts the given plain data to a new structure with type T.
        // Be noted that the method only requires the minimum amount of bytes in order to recreate the structure;
        // the method does not test against exact size.
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

        // Same as above.
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
        /// Gets the managed reference of the first array element in <paramref name="array"/>.
        /// </summary>
        /// <typeparam name="T">The array type.</typeparam>
        /// <param name="array">The array to retrieve the first element from.</param>
        /// <returns>The reference of the first element in <paramref name="array"/>.</returns>
        public static ref T GetFirstArrayElementRef<T>(this T[] array) where T : notnull => ref array[0];

        /// <summary>
        /// Copies the value of the primitive type <typeparamref name="T"/> to a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The primitive type to copy.</typeparam>
        /// <param name="input">The input value type to copy.</param>
        /// <returns>A copied version of <paramref name="input"/>.</returns>
        public static T Copy<T>(this T input) where T : unmanaged
        {
            System.Span<System.Byte> temp = GetBytesTemplate2(input);
            System.Span<System.Byte> copied = new(new System.Byte[temp.Length]);
            Unsafe.CopyBlockUnaligned(ref copied[0] , ref temp[0] , temp.Length.ToUInt32());
            return Unsafe.ReadUnaligned<T>(ref copied[0]);
        }

        /// <summary>
        /// Reverses endianess for the primitive type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The primitive type to reverse endianess for.</typeparam>
        /// <param name="input">The input type to reverse.</param>
        /// <returns>The reversed endianess result of <paramref name="input"/>.</returns>
        // The method 
        public static T ReverseEndianess<T>(this T input) where T : unmanaged
        {
            System.Span<System.Byte> bt = GetBytesTemplate2(input);
            bt.Reverse();
            return GetFromBytesTemplate2<T>(bt , 0);
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
        /// Uses unsafe schemes to convert a <see cref="System.Char"/> to a <see cref="System.Byte"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="character">The character to convert.</param>
        public static System.Byte ToByte(this System.Char character) => NarrowingConversion<System.Char , System.Byte>(character);

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
        /// Uses unsafe schemes to convert a <see cref="System.Byte"/> to a <see cref="System.Char"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Char ToChar(this System.Byte number) => WideningConversion<System.Byte , System.Char>(number);

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
        /// Returns the equivalent byte array representation of this number.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <returns>The equivalent array representation of <paramref name="number"/>.</returns>
        public static System.Byte[] GetBytes(this System.Single number)
        {
            System.Int32 cnv = LinearConversion<System.Single , System.Int32>(number);
            return GetBytesTemplate(cnv);
        }

        /// <summary>
        /// Returns the equivalent byte array representation of this number.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <returns>The equivalent array representation of <paramref name="number"/>.</returns>
        public static System.Byte[] GetBytes(this System.Double number)
        {
            System.Int64 cnv = LinearConversion<System.Double , System.Int64>(number);
            return GetBytesTemplate(cnv);
        }

        /// <summary>
        /// Converts the double-precision floating <paramref name="number"/> given to it's equivalent 64 bits , stored in a <see cref="System.Int64"/>. 
        /// </summary>
        /// <param name="number">The double-precision floating <paramref name="number"/> to convert.</param>
        /// <returns>The equivalent 64 bits returned as a <see cref="System.Int64"/>.</returns>
        public static System.Int64 ToInt64Bits(this System.Double number) => LinearConversion<System.Double , System.Int64>(number);

        /// <summary>
        /// Converts the single-precision floating <paramref name="number"/> given to it's equivalent 32 bits , stored in a <see cref="System.Int32"/>. 
        /// </summary>
        /// <param name="number">The single-precision floating <paramref name="number"/> to convert.</param>
        /// <returns>The equivalent 32 bits returned as a <see cref="System.Int32"/>.</returns>
        public static System.Int32 ToInt32Bits(this System.Single number) => LinearConversion<System.Single , System.Int32>(number);

        /// <summary>
        /// Gets a <see cref="System.Single"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.Single"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.Single ToSingle(this System.Byte[] array , System.Int32 StartIndex)
        {
            System.Int32 rn = GetFromBytesTemplate<System.Int32>(array , StartIndex);
            return LinearConversion<System.Int32 , System.Single>(rn);
        }

        /// <summary>
        /// Gets a <see cref="System.Double"/> from a byte array returned from <c>GetBytes</c> method.
        /// </summary>
        /// <param name="array">The array that contains the information to create a <see cref="System.Double"/>.</param>
        /// <param name="StartIndex">The zero-based index position to start reading from.</param>
        /// <returns>The read number.</returns>
        public static System.Double ToDouble(this System.Byte[] array , System.Int32 StartIndex)
        {
            System.Int64 rn = GetFromBytesTemplate<System.Int64>(array , StartIndex);
            return LinearConversion<System.Int64 , System.Double>(rn);
        }

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

        /// <summary>
        /// Converts the given byte array to a base64 sequence.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>The base64 data that are equivalent to <paramref name="bytes"/>.</returns>
        // Note that the base64 code implementation uses unsafe code , so it is okay to include it here.
        public static System.String ToBase64(this System.Byte[] bytes) => Base64Encoding.Singleton.GetString(bytes);
    
        /// <summary>
        /// Converts the given byte array to a base64 sequence.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>The base64 data that are equivalent to <paramref name="bytes"/>.</returns>
        /// <param name="startindex">The index to start computing the base64 information.</param>
        /// <param name="count">The elements to produce the base64 equivalent data.</param>
        public static System.String ToBase64Selected(this System.Byte[] bytes , System.Int32 startindex , System.Int32 count) => Base64Encoding.Singleton.GetString(bytes , startindex , count);
    
        /// <summary>
        /// Converts the given base64 string to the equivalent byte array representation.
        /// </summary>
        /// <param name="base64">The base64 string to convert.</param>
        /// <returns>The decoded byte information.</returns>
        public static System.Byte[] FromBase64(this System.String base64) => Base64Encoding.Singleton.GetBytes(base64);

        /// <summary>
        /// Converts the given base64 string to the equivalent byte array representation.
        /// </summary>
        /// <param name="base64">The base64 string to convert.</param>
        /// <param name="index">The character index inside <paramref name="base64"/> to start decoding from.</param>
        /// <param name="count">The number of characters to decode.</param>
        /// <returns>The decoded byte information.</returns>
        public static System.Byte[] FromBase64Selected(this System.String base64 , System.Int32 index, System.Int32 count) 
#if NETSTANDARD || NET472_OR_GREATER
        => Base64Encoding.Singleton.GetBytes(base64.ToCharArray() , index , count);
#else
        => Base64Encoding.Singleton.GetBytes(base64 , index , count);
#endif

        // Parts of bit conversion code also belong from referencesource.microsoft.com/en-us !.
        private static System.Byte GetBitValue(System.Int32 bitidx) => (1 << (bitidx & 7)).ToByte();

        /// <summary>
        /// Gets the bit on the specified index inside the byte. <br />
        /// The index is a zero-based number up to 7.
        /// </summary>
        /// <param name="bt">The byte to retrieve the bit from.</param>
        /// <param name="bitindex">The bit index to retrieve.</param>
        /// <returns>The bit value. <see langword="true"/> means that the bit is set.</returns>
        public static System.Boolean GetBit(this System.Byte bt , System.Int32 bitindex)
        {
            System.Byte bitval = GetBitValue(bitindex);
            return (bt & bitval) == bitval;
        }

        /// <summary>
        /// Sets the bit at the specified index of the specified byte. The byte given must be a reference to it
        /// so that the operation can succeed.
        /// </summary>
        /// <param name="bt">The byte to set the bit to.</param>
        /// <param name="bitindex">The bit index inside the byte to set.</param>
        /// <param name="value">The value to set on the specified bit (<see langword="true"/> means 1 , and <see langword="false"/> means 0).</param>
        [System.Security.SecuritySafeCritical] // This operation is undefined if the pointer is invalid
        public static void SetBit(this ref System.Byte bt , System.Int32 bitindex , System.Boolean value)
        {
            if (value) {
                bt |= GetBitValue(bitindex);
            } else {
                System.Byte result = 0;
                for (System.Int32 I = 0; I < 8; I++)
                {
                    // Add only all the bits that are set , and omit the target bit which we want to unset.
                    if (I != bitindex && GetBit(bt , I))
                    {
                        result |= GetBitValue(I);
                    }
                }
                bt = result;
            }
        }

        /// <summary>
        /// Returns the byte as a fully recreatable and representable bit array of <see cref="System.Boolean"/>s.
        /// </summary>
        /// <param name="bt">The byte to convert.</param>
        /// <returns>It's binary representation.</returns>
        public static System.Boolean[] ToBinary(this System.Byte bt)
        {
            // Create a Boolean array that it's size is equal to a byte.
            System.Boolean[] result = new System.Boolean[8];
            for (System.Int32 I = 0; I < 8; I++)
            {
                result[I] = GetBit(bt , I);
            }
            return result;
        }

        /// <summary>
        /// Converts the given number to a binary string in the .NET Core format.
        /// </summary>
        /// <typeparam name="T">The number type to convert.</typeparam>
        /// <param name="number">The number to convert.</param>
        /// <returns>The converted binary string that is equal to <paramref name="number"/>.</returns>
        public static System.String ToBinaryString<T>(this T number) where T : unmanaged
        {
            System.String result = System.String.Empty;
            System.Byte[] data = GetBytesTemplate(number);
            for (System.Int32 I = 0; I < data.Length; I++)
            {
                for (System.Int32 J = 0; J < 8; J++)
                {
                    result += GetBit(data[I] , J) ? "1" : "0";
                }
                if (I+1 < data.Length) { result += "_"; }
            }
            return result;
        }

    }
}
