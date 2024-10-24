﻿using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// The below warning is disabled because all calls that evolve it are sizeof's which only get sizes of unmanaged types.
#pragma warning disable 8500 // Declares a pointer to , takes the address of , or gets the size of a managed type

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
    internal sealed class StringPinnable
    {
        /// <summary>
        /// Gets a new instance of <see cref="StringPinnable"/> from the specified data.
        /// </summary>
        /// <param name="data">The data to get from.</param>
        /// <returns>A new <see cref="StringPinnable"/> that is the unmanaged surface of <paramref name="data"/>.</returns>
        public static StringPinnable GetAsPinnable(System.String data) => Unsafe.As<StringPinnable>(data);

        public static unsafe StringPinnable ReadFromArray(System.Char[] chars , System.Int32 startindex)
        {
            if (chars is null) { throw new ArgumentNullException(nameof(chars)); }
            if (startindex < 0 || startindex >= chars.Length) { throw new ArgumentOutOfRangeException(nameof(startindex) ,"startindex must not be negative and be less than the array length."); }
            StringPinnable pinnable = new();
            pinnable.Length = chars.Length - startindex;            
            fixed (System.Char* src = &chars[startindex])
            {
                fixed (System.Char* dst = &pinnable.Data)
                {
                    Unsafe.CopyBlockUnaligned(dst, src, (pinnable.Length * Unsafe.SizeOf<System.Char>()).ToUInt32());
                }
            }
            return pinnable;
        }

        public static unsafe StringPinnable Create(System.Int32 length)
        {
            StringPinnable pinnable = new();
            if (length < 0) { throw new ArgumentOutOfRangeException(nameof(length) , "length must not be negative."); }
            pinnable.Length = length;
            fixed (System.Char* target = &pinnable.Data)
            {
                Unsafe.InitBlock(target, 48, (pinnable.Length * Unsafe.SizeOf<System.Char>()).ToUInt32());
            }
            return pinnable;
        }


        // Avoiding direct construction - use the GetAsPinnable method.
        private StringPinnable() { }

        /// <summary>
        /// The length of the provided string.
        /// </summary>
        [FieldOffset(0)] public System.Int32 Length;
        /// <summary>
        /// The first element of the string. Use the &amp; operator to get a pointer to the whole data array.
        /// </summary>
        [FieldOffset(4)] public System.Char Data;

        /// <summary>
        /// Reconverts back to the original string if you want it again...
        /// </summary>
        public override string ToString() => Unsafe.As<System.String>(this);
    }

    /// <summary>
    /// Provides unsafe extension methods for the <see cref="DotNetResourcesExtensions"/> project
    /// for manipulating primitive information and byte arrays.
    /// </summary>
    // The purpose of the UnsafeMethods class is to provide faster alternatives for .NET Framework on raw primitive information , 
    // while providing a unified and reliable api surface for the developers of the library.
    internal static unsafe partial class UnsafeMethods
    {
        // The unsafe calls allow us to bypass definite runtime checks for conversions.
        // These are also implemented in pure MSIL - which means that these can work in any possible .NET platform.
        // All these operations work on checked mode but note that actually work as if the conversion was performed in unchecked mode.
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
            if (input is System.Byte b) {
                // Endianess swap in byte level is now possible because a decomposal method to binary was found.
                b = ReverseEndianess_Byte(b);
                return Unsafe.As<System.Byte , T>(ref b);
            }
            System.Span<System.Byte> bt = GetBytesTemplate2(input);
            bt.Reverse();
            return GetFromBytesTemplate2<T>(bt , 0);
        }

        private static System.Byte ReverseEndianess_Byte(System.Byte b)
        {
            System.Boolean[] booleans = b.ToBinary();
            // Reverse bin data
            booleans.Reverse();
            System.Byte result = 0;
            // Then assign the reversed result
            for (System.Int32 I = 0; I < booleans.Length; I++) {
                if (booleans[I]) { result.SetBit(I, true); }
            }
            booleans = null;
            return result;
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

        #region Conversions to Int64
        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int32"/> to a <see cref="System.Int64"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int64 ToInt64(this System.Int32 number) => WideningConversion<System.Int32, System.Int64>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.UInt64"/> to a <see cref="System.Int64"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int64 ToInt64(this System.UInt64 number) => LinearConversion<System.UInt64 , System.Int64>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Char"/> to a <see cref="System.Int64"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="character">The character to convert.</param>
        public static System.Int64 ToInt64(this System.Char character) => WideningConversion<System.Char, System.Int64>(character);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.UInt32"/> to a <see cref="System.Int64"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int64 ToInt64(this System.UInt32 number) => WideningConversion<System.UInt32 , System.Int64>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.UInt16"/> to a <see cref="System.Int64"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int64 ToInt64(this System.UInt16 number) => WideningConversion<System.UInt16, System.Int64>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int16"/> to a <see cref="System.Int64"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int64 ToInt64(this System.Int16 number) => WideningConversion<System.Int16, System.Int64>(number);
        #endregion

        #region Conversions to UInt64
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
        #endregion

        #region Conversions to Int32
        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int16"/> to a <see cref="System.Int32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int32 ToInt32(this System.Int16 number) => WideningConversion<System.Int16, System.Int32>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int64"/> to a <see cref="System.Int32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int32 ToInt32(this System.Int64 number) => NarrowingConversion<System.Int64, System.Int32>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.UInt32"/> to a <see cref="System.Int32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int32 ToInt32(this System.UInt32 number) => LinearConversion<System.UInt32, System.Int32>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Char"/> to a <see cref="System.Int32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="character">The character to convert.</param>
        public static System.Int32 ToInt32(this System.Char character) => WideningConversion<System.Char, System.Int32>(character);
        #endregion

        #region Conversions to UInt32
        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int32"/> to a <see cref="System.UInt32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.UInt32 ToUInt32(this System.Int32 number) => LinearConversion<System.Int32 , System.UInt32>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.UInt64"/> to a <see cref="System.UInt32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.UInt32 ToUInt32(this System.UInt64 number) => NarrowingConversion<System.UInt64, System.UInt32>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int64"/> to a <see cref="System.UInt32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.UInt32 ToUInt32(this System.Int64 number) => NarrowingConversion<System.Int64, System.UInt32>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.UInt16"/> to a <see cref="System.UInt32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.UInt32 ToUInt32(this System.UInt16 number) => WideningConversion<System.UInt16 , System.UInt32>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int16"/> to a <see cref="System.UInt32"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.UInt32 ToUInt32(this System.Int16 number) => WideningConversion<System.Int16, System.UInt32>(number);
        #endregion

        #region Conversions to Int16
        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Byte"/> to a <see cref="System.Int16"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int16 ToInt16(this System.Byte number) => WideningConversion<System.Byte, System.Int16>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.UInt16"/> to a <see cref="System.Int16"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int16 ToInt16(this System.UInt16 number) => LinearConversion<System.UInt16, System.Int16>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int64"/> to a <see cref="System.Int16"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int16 ToInt16(this System.Int64 number) => NarrowingConversion<System.Int64 , System.Int16>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int32"/> to a <see cref="System.Int16"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Int16 ToInt16(this System.Int32 number) => NarrowingConversion<System.Int32, System.Int16>(number);
        #endregion

        #region Conversions to UInt16
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
        /// Uses unsafe schemes to convert a <see cref="System.Int64"/> to a <see cref="System.UInt16"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.UInt16 ToUInt16(this System.Int64 number) => NarrowingConversion<System.Int64 , System.UInt16>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int32"/> to a <see cref="System.UInt16"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.UInt16 ToUInt16(this System.Int32 number) => NarrowingConversion<System.Int32, System.UInt16>(number);
        #endregion

        #region Conversions to Byte
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
        #endregion

        #region Conversions to Char
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
        /// Uses unsafe schemes to convert a <see cref="System.UInt32"/> to a <see cref="System.Char"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Char ToChar(this System.UInt32 number) => NarrowingConversion<System.UInt32, System.Char>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Int16"/> to a <see cref="System.Char"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Char ToChar(this System.Int16 number) => LinearConversion<System.Int16, System.Char>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.UInt16"/> to a <see cref="System.Char"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Char ToChar(this System.UInt16 number) => LinearConversion<System.UInt16, System.Char>(number);

        /// <summary>
        /// Uses unsafe schemes to convert a <see cref="System.Byte"/> to a <see cref="System.Char"/>. 
        /// The conversion is only performed with less checks during runtime.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        public static System.Char ToChar(this System.Byte number) => WideningConversion<System.Byte , System.Char>(number);
        #endregion

        #region Get Bytes from numeric types
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
        /// Returns the equivalent byte array representation of this number.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <returns>The equivalent array representation of <paramref name="number"/>.</returns>
        public static System.Byte[] GetBytes(this System.Decimal number) => GetBytesTemplate(number);
        #endregion

        #region Convert to numeric types from bytes
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
        public static System.Double ToDouble(this System.Byte[] array , System.Int32 StartIndex)
        {
            System.Int64 rn = GetFromBytesTemplate<System.Int64>(array , StartIndex);
            return LinearConversion<System.Int64 , System.Double>(rn);
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
        public static System.Decimal ToDecimal(this System.Byte[] array, System.Int32 StartIndex) => GetFromBytesTemplate<System.Decimal>(array, StartIndex);

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
        #endregion

        /// <summary>
        /// Uses unsafe schemes to copy a string to a new character array , starting from the specified index and copying <paramref name="count"/> charaters to target.
        /// </summary>
        /// <param name="str">The string to copy.</param>
        /// <param name="index">The index to start copying characters from.</param>
        /// <param name="count">The number of characters to copy.</param>
        /// <returns>A new array which has characters copied from <paramref name="str"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> or <paramref name="count"/> were less than zero , or <paramref name="count"/> was greater than the input string.</exception>
        public static System.Char[] ToCharArrayUnsafe(this System.String str , System.Int32 index , System.Int32 count)
        {
            if (str is null) { return System.Array.Empty<System.Char>(); }
            if (index < 0) { throw new ArgumentOutOfRangeException(nameof(index), "index must not be negative."); }
            if (count < 0) { throw new ArgumentOutOfRangeException(nameof(index), "count must be positive."); }
            StringPinnable pnt = StringPinnable.GetAsPinnable(str);
            if (count == 0 || pnt.Length == 0) { return System.Array.Empty<System.Char>(); }
            if (count > pnt.Length - index) {
                throw new ArgumentOutOfRangeException(nameof(count) , $"Count must be less than or equal to {pnt.Length - index} .");
            }
            System.Char[] result = new System.Char[count];
            fixed (System.Char* ptr = result)
            {
                // Gets the starting reference of pnt.Data where the user prefers to start copying from.
                // If index == 0 (might be the most common case btw) , then pnt.Data is returned from Unsafe.Add as-it-is.
                fixed (System.Char* src = &Unsafe.Add(ref pnt.Data , index))
                {
                    // Uses cpblk , fast and efficient.
                    // Also be noted that we need byte count , and not character count , so we convert it to bytes at run-time.
                    Unsafe.CopyBlockUnaligned(ptr, src, (count * Unsafe.SizeOf<System.Char>()).ToUInt32());
                }
            }
            return result;
        }

        /// <summary>
        /// Uses unsafe schemes to copy a string to a new character array.
        /// </summary>
        /// <param name="str">The string to copy.</param>
        /// <returns>A new array which has characters copied from <paramref name="str"/>.</returns>
        public static System.Char[] ToCharArrayUnsafe(this System.String str) => ToCharArrayUnsafe(str, 0, str.Length);

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
                    result += data[I].GetBit(J) ? "1" : "0";
                }
                if (I+1 < data.Length) { result += "_"; }
            }
            return result;
        }

    }
}
