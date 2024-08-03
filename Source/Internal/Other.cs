
using System;
using System.Security;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions.Internal
{
    /// <summary>
    /// For classes that use streams , this simple interface defines whether 
    /// the class has control of the lifetime of the supplied stream.
    /// </summary>
    public interface IStreamOwnerBase
    {
        /// <summary>
        /// Gets or sets a value whether the implementing class controls the lifetime of the underlying stream.
        /// </summary>
        public System.Boolean IsStreamOwner { get; set; }
    }

    /// <summary>
    /// Internal enumeration so as to cope with classes that either use or do not use streams.
    /// </summary>
    internal enum StreamMixedClassManagement : System.Byte { None , NotStream , InitialisedWithStream , FileUsed }

    // Parser helpers.
    internal static class ParserHelpers
    {
        private const System.Int32 BUFSIZE = 4096;
        public const System.String InvalidNameCharacters = "!@#$%^&*()`~,./?\\|\"\';:{}[]+=©§¥¢ž®¯";
        public const System.String InvalidFirstCharacterNameCharacters = "1234567890!@#$%^&*()`~,./?\\|\"';:{}[]+=-_©§¥¢ž®¯";

        // Simple function to convert a simple number string to a numeric value. 
        public static System.Int64 ToNumber(System.String value)
        {
            System.Int64 result = 0 , prg = 1;
            for (System.Int32 I = value.Length - 1; I > -1; I--) {
                result += (((System.Int64)value[I]) - 48) * prg;
                prg *= 10;
            }
            return result;
        }

        // Simple function to convert a simple number to a number string.
        // Only works currently for numbers that are positive.
        public static System.String NumberToString(System.Int64 value)
        {
            List<System.Char> chars = new();
            System.Int64 prg = 1;
            System.Int64 rt;
            System.Byte I = 19;
            do
            {
                I--;
                rt = (value / prg) % 10;
                chars.Add((System.Char)(rt + 48));
                prg *= 10;
            } while (I > 0);
            chars.Reverse();
            return new(chars.ToArray()); // Although it will return a value like of full zeroes (If it is zero) , we are not being bothered by that.
        }

        /// <summary>
        /// Tests if the resource name given is valid.
        /// </summary>
        /// <param name="Name">The resource name to test.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        [System.Diagnostics.DebuggerHidden]
        public static void ValidateName(System.String Name)
        {
            if (Name == null || System.String.IsNullOrEmpty(Name))
            {
                throw new ArgumentNullException("Name");
            }

            if (Name.Length > 530)
            {
                throw new ArgumentException("A resource name must not have more than 530 characters.");
            }

            for (System.Int32 J = 0; J < InvalidFirstCharacterNameCharacters.Length; J++)
            {
                if (Name[0] == InvalidFirstCharacterNameCharacters[J])
                {
                    throw new ArgumentException($"The first character of a resource name must not have all the following characters: {InvalidFirstCharacterNameCharacters}.", "Name");
                }
            }

            for (System.Int32 I = 1; I < Name.Length; I++) 
            {
                for (System.Int32 J = 0; J < InvalidNameCharacters.Length;J++)
                {
                    if (Name[I] == InvalidNameCharacters[J]) 
                    {
                        throw new ArgumentException($"A resource name must not have all the following characters: {InvalidNameCharacters}.", "Name");
                    }
                }
            }
        }

        public static System.Byte[] ReadBuffered(System.IO.Stream Stream, System.Int64 RequestedBytes)
        {
            // Check for null conditions or whether we can read from this stream
            if (Stream == null) { return null; }
            if (Stream.CanRead == false) { return null; }
            // Create a new byte array with the requested size.
            System.Byte[] Contents = new System.Byte[RequestedBytes];
            if (RequestedBytes <= BUFSIZE)
            {
                // Read all bytes directly , if the requested bytes are less than the buffer limit.
                // Otherwise we don't care here; we do not read thousands or millions of bytes.
                Stream.Read(Contents, 0, Contents.Length);
            }
            else
            {
                System.Int32 Count;
                System.Int32 Offset = 0;
                // Read all bytes with buffered mode.
                do
                {
                    Count = Stream.Read(Contents, Offset, BUFSIZE);
                    Offset += BUFSIZE;
                    // Condition specifies that the loop will continue to run when the read bytes are
                    // more or equal than the buffer limit , plus make sure that the next read will not
                    // surpass the bytes that the final array can hold.
                } while ((Count >= BUFSIZE) && (Offset + BUFSIZE <= Contents.Length));
                // In case that the bytes were surpassed in the above condition , pass all the rest bytes again normally.
                if (Contents.Length - Offset > 0) { Stream.Read(Contents, Offset, Contents.Length - Offset); }
            }
            return Contents;
        }

        public static void WriteBuffered(System.IO.Stream stream , System.Byte[] data)
        {
            // Abstract: Writes bytes to a stream with a 'buffered' method.
            // Calculate the blocks that will be raw-copied. 
            // Also , calculate the remaining data that will be plainly passed.
            System.Int64 blocks = data.LongLength / BUFSIZE , 
                c = data.LongLength % BUFSIZE;
            System.Int32 pos = 0;
            // Copy all data to the stream
            while (blocks > 0) {
                stream.Write(data, pos, BUFSIZE);
                pos += BUFSIZE;
                blocks--;
            }
            // If the input array size is not exactly a multiple of BUFSIZE , the rest data will be copied as-it-is.
            // This even works for cases that data.LongLength < BUFSIZE because the while loop
            // will never be entered.
            if (c > 0) { stream.Write(data, pos, (System.Int32)c); }
        }

        public static void BlockCopy(System.IO.Stream input, System.IO.Stream output)
        {
            System.Byte[] buffer = new System.Byte[BUFSIZE];
            System.Int32 readin = 0;
            while ((readin = input.Read(buffer, 0, BUFSIZE)) > 0)
            {
                output.Write(buffer, 0, readin);
            }
        }

        public static string GetAssemblyQualifiedName(Type type, Func<Type, string> typeNameConverter)
        {
            string text = null;
            if (type != null)
            {
                if (typeNameConverter != null)
                {
                    try
                    {
                        text = typeNameConverter(type);
                    }
                    catch (Exception ex)
                    {
                        if (IsSecurityOrCriticalException(ex))
                        {
                            throw;
                        }
                    }
                }
                if (string.IsNullOrEmpty(text))
                {
                    text = type.AssemblyQualifiedName;
                }
            }
            return text;
        }

#pragma warning disable 0618
        public static bool IsSecurityOrCriticalException(Exception ex)
        {
            if (!(ex is NullReferenceException) && !(ex is StackOverflowException) && !(ex is OutOfMemoryException) && !(ex is ThreadAbortException) && !(ex is ExecutionEngineException) && !(ex is IndexOutOfRangeException) && !(ex is AccessViolationException))
            {
                return ex is SecurityException;
            }
            return true;
        }

        public static bool IsCriticalException(Exception ex)
        {
            if (!(ex is NullReferenceException) && !(ex is StackOverflowException) && !(ex is OutOfMemoryException) && !(ex is ThreadAbortException) && !(ex is ExecutionEngineException) && !(ex is IndexOutOfRangeException))
            {
                return ex is AccessViolationException;
            }
            return true;
        }
#pragma warning restore 0618

        public static System.Byte[] GetBytes(System.Byte[] bytes, System.Int64 idx, System.Int64 count)
        {
            System.Int64 len = count <= bytes.LongLength ? count : (count - bytes.LongLength);
            System.Byte[] res = new System.Byte[len];
            System.Int64 I = idx, J = 0;
            for (; I < idx + count && I < bytes.LongLength; I++)
            {
                res[J] = bytes[I];
                J++;
            }
            // Recursive call when the bytes read were less than the expected.
            // In that case , the array length will be fixed to the number of elements read.
            if (J < len) { return GetBytes(res, 0, J); }
            return res;
        }

        public static System.Byte[] GetBytes(System.Span<System.Byte> span, System.Int32 idx, System.Int32 count)
        {
            System.Int32 len = count <= span.Length ? count : (count - span.Length);
            System.Byte[] res = new System.Byte[len];
            System.Int32 I = idx, J = 0;
            for (; I < idx + count && I < span.Length; I++)
            {
                res[J] = span[I];
                J++;
            }
            if (J < len) { return GetBytes(bytes: res, 0, J); }
            return res;
        }

        public static System.Byte[] GetBytes(System.Byte[] bytes, System.Int64 idx) => GetBytes(bytes, idx, bytes.LongLength - idx);
    
    }

    /// <summary>
    /// Defines base properties for all resource parser exceptions.
    /// </summary>
    public interface IResourceParsersExceptionBase
    {
        /// <summary>
        /// Gets the parser error category of the implemented exception.
        /// </summary>
        public ParserErrorType ErrorCategory { get; }

        /// <summary>
        /// Gets the original parser error message. This might not be always available.
        /// </summary>
        public System.String ParseErrorMessage { get; set; }
    }

    /// <summary>
    /// Serves as the base implementation for resources-related exception types.
    /// </summary>
    public interface IResourceExceptionBase
    {
        /// <summary>
        /// Gets the resource name that caused the implemented exception.
        /// </summary>
        public System.String ResourceName { get; }
    }

    /// <summary>
    /// Defines special constants that categorize the parser errors.
    /// </summary>
    public enum ParserErrorType : System.Byte
    {
        /// <summary>Internal dummy field. Do not use.</summary>
        Default,
        /// <summary>The error refers to header conformance deserialization.</summary>
        Header,
        /// <summary>The error refers to versioning mistake.</summary>
        Versioning,
        /// <summary>The error refers to deserialization error.</summary>
        Deserialization,
        /// <summary>The error refers to serialization error.</summary>
        Serialization
    }

    /// <summary>
    /// Defines a format to save serialized objects using the <see cref="ICustomFormatter"/> interface
    /// for resource classes which did not had support or was not considered such support. <br />
    /// Additionally it provides two extension methods that extend the <see cref="ICustomFormatter"/>
    /// interface for supporting using this format directly from the formatter instance.
    /// </summary>
    public static class ResourceInterchargeFormat
    {
        private const System.Int32 HDRLEN = 3;
        private const System.Int32 SIZEIDLEN = 4;
        /// <summary>
        /// Defines the header as bytes. Use it to recognize the format in the resource classes.
        /// </summary>
        public static System.Byte[] RifHeaderAsBytes = new System.Byte[3] { 82, 73, 70 };
        // This format is defined as follows: 
        // RIF(Header) -> 0000(Size of .NET type string) -> .NET type string -> 0000(Size of data to save) -> Data
        // For LE cases , the arrays will be reversed.

        /// <summary>
        /// Get a new Intercharge format from the specified formatter and the object to serialize.
        /// </summary>
        /// <param name="formatter">The formatter which will serialize the specified object.</param>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A new intercharge format array.</returns>
        public static System.Byte[] GetFromObject(CustomFormatter.ICustomFormatter formatter, System.Object obj)
        {
            System.Byte[] formatteddata = formatter.GetBytesFromObject(obj);
            System.Byte[] nameasbytes = System.Text.Encoding.UTF8.GetBytes(obj.GetType().AssemblyQualifiedName);
            System.Byte[] Final = new System.Byte[HDRLEN + SIZEIDLEN + nameasbytes.Length + SIZEIDLEN + formatteddata.Length];
            System.Int32 idx = 0;
            System.Array.ConstrainedCopy(RifHeaderAsBytes , 0 , Final , idx , HDRLEN);
            idx += HDRLEN;
            System.Byte[] temp = System.BitConverter.GetBytes(nameasbytes.Length);
            if (System.BitConverter.IsLittleEndian) { Array.Reverse(temp); }
            Array.ConstrainedCopy(temp, 0, Final, idx, SIZEIDLEN);
            idx += SIZEIDLEN;
            Array.ConstrainedCopy(nameasbytes, 0, Final, idx, nameasbytes.Length);
            idx += nameasbytes.Length;
            nameasbytes = null;
            temp = System.BitConverter.GetBytes(formatteddata.Length);
            if (System.BitConverter.IsLittleEndian) { Array.Reverse(temp); }
            Array.ConstrainedCopy(temp, 0, Final, idx, SIZEIDLEN);
            idx += SIZEIDLEN;
            Array.ConstrainedCopy(formatteddata , 0 , Final , idx , formatteddata.Length);
            return Final;
        }

        /// <summary>
        /// Get the original object from the produced array from <see cref="GetFromObject(CustomFormatter.ICustomFormatter, object)"/> 
        /// method and the formatter to deserialize data.
        /// </summary>
        /// <param name="formatter">The formatter which will deserialize the object back to it's original state.</param>
        /// <param name="bytes">The byte array acquired from <see cref="GetFromObject(CustomFormatter.ICustomFormatter, object)"/> method.</param>
        /// <returns>The original object.</returns>
        /// <exception cref="FormatException">The array given was not the array returned from <see cref="GetFromObject(CustomFormatter.ICustomFormatter, object)"/>.</exception>
        public static System.Object GetFromBytes(CustomFormatter.ICustomFormatter formatter , byte[] bytes)
        {
            System.Int32 I = 0;
            for (; I < HDRLEN;I++)
            {
                if (bytes[I] != RifHeaderAsBytes[I]) { throw new FormatException("This is not the intercharge format."); }
            }
            // Is at 3.
            // Read next 4 bytes , reverse them and call BitConverter.
            System.Byte[] temp = new System.Byte[SIZEIDLEN];
            Array.ConstrainedCopy(bytes , I , temp , 0 , SIZEIDLEN);
            if (System.BitConverter.IsLittleEndian) { Array.Reverse(temp); }
            System.Int32 len = BitConverter.ToInt32(temp, 0);
            I += SIZEIDLEN; // Skip the 4 bytes read previously.
            // Read the string.
            System.String typestring = System.Text.Encoding.UTF8.GetString(ParserHelpers.GetBytes(bytes: bytes, I, len));
            // We will feed it to the type in a bit , keep it for now.
            // Skip the read bytes.
            I += len;
            // Read next 4 bytes.
            temp = ParserHelpers.GetBytes(bytes: bytes, I, SIZEIDLEN);
            if (System.BitConverter.IsLittleEndian) { Array.Reverse(temp); }
            len = BitConverter.ToInt32(temp, 0);
            // Skip the 4 read bytes read previously.
            I += SIZEIDLEN;
            // Return the object.
            return formatter.GetObjectFromBytes(ParserHelpers.GetBytes(bytes: bytes , I , len) , System.Type.GetType(typestring , true , true));
        }

        /// <summary>
        /// Returns the serialized bytes for <paramref name="obj"/> but also directly saves it under the Resource Intercharge Format for directly saving it.
        /// </summary>
        /// <param name="formatter">The formatter which will serialize the specified object.</param>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A new intercharge format array containing <paramref name="obj"/>.</returns>
        public static System.Byte[] GetObjectAsResourceInterchargeFormatBytes(this CustomFormatter.ICustomFormatter formatter , System.Object obj) => GetFromObject(formatter , obj);

        /// <summary>
        /// Gets the original object defined in the Resource Intercharge Format from the specified <paramref name="bytes"/>.
        /// </summary>
        /// <param name="formatter">The formatter which will deserialize the object back to it's original state.</param>
        /// <param name="bytes">The byte array acquired from <see cref="GetFromObject(CustomFormatter.ICustomFormatter, object)"/> or <see cref="GetObjectAsResourceInterchargeFormatBytes(CustomFormatter.ICustomFormatter, object)"/> methods.</param>
        /// <returns>The original object.</returns>
        /// <exception cref="FormatException">The array given was not the array returned from <see cref="GetFromObject(CustomFormatter.ICustomFormatter, object)"/> or <see cref="GetObjectAsResourceInterchargeFormatBytes(CustomFormatter.ICustomFormatter, object)"/>.</exception>
        public static System.Object GetFromResourceInterchargeFormatBytes(this CustomFormatter.ICustomFormatter formatter, System.Byte[] bytes) => GetFromBytes(formatter, bytes);
    }
}

namespace System.Numerics.Hashing
{
    internal static class HashHelpers
    {
        private readonly static System.Func<System.Int32> RS1 = () =>
        {
            System.Random RD = null;
            try
            {
                RD = new System.Random();
                return RD.Next(System.Int32.MinValue, System.Int32.MaxValue);
            }
            catch (System.Exception EX)
            {
                // Rethrow the exception , but as an invalidoperation one , because actually calling unintialised RD is illegal.
                throw new InvalidOperationException("Could not call Rand.Next. More than one errors occured.", EX);
            }
            finally { if (RD != null) { RD = null; } }
        };

        public static readonly int RandomSeed = RS1();

        /// <summary>
        /// Combines two hash codes and returns their combined result.
        /// </summary>
        /// <param name="h1">The first hash code.</param>
        /// <param name="h2">The second hash code.</param>
        /// <returns>The combined result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Combine(int h1, int h2)
        {
            // RyuJIT optimizes this to use the ROL instruction
            // Related GitHub pull request: dotnet/coreclr#1830
            uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)rol5 + h1) ^ h2;
        }

        /// <summary>
        /// Combines a multiple of hash codes and returns their combined result. <br />
        /// This method works better when combining four or more hash codes!
        /// </summary>
        /// <param name="nums">An array of hash codes.</param>
        /// <returns>The combined result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Int32 Combine(params int[] nums)
        {
            // What this code does? 
            // Let's imagine a case that we have to calculate 
            // 30 hash codes and combine them.
            // Someone would use this scheme:
            // <-- // .NET C# Code
            // System.Int32 result = 0;
            // HashHelpers.Combine(result , this.one);
            // HashHelpers.Combine(result , this.two);
            // HashHelpers.Combine(result , this.three);
            // // and goes on...
            // -->
            // However , such code produces some sort of heavy IL (creates 30 call schemes in IL and pushes to the stack 60 times?!)
            // and programmer pain to write such a lot of code.
            // The System.Numerics.Vector structure , produces it's hash code by Combine , by calling it around 76? times.
            // Some code that utilizes Combine is: 
            /* <-- IL code
             *   
IL_0627:  stloc.0
IL_0628:  ldloc.0
IL_0629:  ldarg.0
IL_062a:  ldflda     valuetype System.Numerics.Register valuetype System.Numerics.Vector`1<!T>::register
IL_062f:  ldflda     int8 System.Numerics.Register::sbyte_4
IL_0634:  call       instance int32 [mscorlib]System.SByte::GetHashCode()
IL_0639:  call       int32 System.Numerics.Hashing.HashHelpers::Combine(int32,
                                                                  int32)
IL_063e:  stloc.0
IL_063f:  ldloc.0
IL_0640:  ldarg.0
IL_0641:  ldflda     valuetype System.Numerics.Register valuetype System.Numerics.Vector`1<!T>::register
IL_0646:  ldflda     int8 System.Numerics.Register::sbyte_5
IL_064b:  call       instance int32 [mscorlib]System.SByte::GetHashCode()
IL_0650:  call       int32 System.Numerics.Hashing.HashHelpers::Combine(int32,
                                                                  int32)
IL_0655:  stloc.0
IL_0656:  ldloc.0
IL_0657:  ldarg.0
IL_0658:  ldflda     valuetype System.Numerics.Register valuetype System.Numerics.Vector`1<!T>::register
IL_065d:  ldflda     int8 System.Numerics.Register::sbyte_6
IL_0662:  call       instance int32 [mscorlib]System.SByte::GetHashCode()
IL_0667:  call       int32 System.Numerics.Hashing.HashHelpers::Combine(int32,
                                                                  int32)
            -->
            */
            // which is a bit of heavy for our JIT compiler , because it pushes:
            // -> 3 times the result variable (ldloc.0)
            // -> 6 times the register field and then it's fields (ldflda)
            // -> 6 times the call instruction is called
            // -> 3 times the set to the result variable (stloc.0)
            // Total: 12 pushes for 3 times? don't you consider that a bit heavy?
            // It is sure that stloc can be called repeatedly , and that makes the compiler more flexible in handling the values.
            // When at least this method is called for such cases , the array passed here will have ready for us the codes 
            // (Since the JIT will have done the most hard work calling GetHashCode and getting it's result)
            // So the values will be pushed only once while the array is created , but there are not actually pushed , they
            // are allocated to the managed heap! ldflda calls do not happen here , plus the result is returned once done.
            // If we inspect this function's loop code , we will see: 
            /* <-- // IL Code
IL_0008:  nop // Start of loop code : for (System.Int64 I = 0; I < nums.LongLength; I++)
IL_0009:  ldloc.0 <- The 'result' variable (Our result variable!)
IL_000a:  ldarg.0 <- The 'nums' array (The hashcodes!)
IL_000b:  ldloc.1 <- The 'I' iterator of the array
IL_000c:  conv.ovf.i <- Get the element at 'I' position in 'nums' array.
IL_000d:  ldelem.i4 <- Push it to the second parameter
IL_000e:  call       int32 System.Numerics.Hashing.HashHelpers::Combine(int32, <- Call Combine here
                                                                             int32)
IL_0013:  stloc.0 <- Set the result to the 'result' variable
IL_0014:  nop // End of the code inside the loop -->
            */
            // Here , we gathered 4 pushes which are destroyed after Combine is called.
            // This happens for all the elements.
            // We have saved 2 times from happening about the same thing? Yes. It became loop code.
            // Here you can see the optimizations done:
            /*
IL_06a5:  ldc.i4.s   13 <- New element in array , Number 13
IL_06a7:  ldarg.0
IL_06a8:  ldflda     valuetype System.Numerics.Register valuetype System.Numerics.Vector`1<!T>::register <- ldflda instructions 
IL_06ad:  ldflda     int8 System.Numerics.Register::sbyte_13
IL_06b2:  call       instance int32 [mscorlib]System.SByte::GetHashCode() <- The hash code result is directly set to the array's element 
IL_06b7:  stelem.i4
IL_06b8:  dup
IL_06b9:  ldc.i4.s   14
IL_06bb:  ldarg.0
IL_06bc:  ldflda     valuetype System.Numerics.Register valuetype System.Numerics.Vector`1<!T>::register
IL_06c1:  ldflda     int8 System.Numerics.Register::sbyte_14
IL_06c6:  call       instance int32 [mscorlib]System.SByte::GetHashCode()
IL_06cb:  stelem.i4
IL_06cc:  dup
IL_06cd:  ldc.i4.s   15
IL_06cf:  ldarg.0
IL_06d0:  ldflda     valuetype System.Numerics.Register valuetype System.Numerics.Vector`1<!T>::register
IL_06d5:  ldflda     int8 System.Numerics.Register::sbyte_15
IL_06da:  call       instance int32 [mscorlib]System.SByte::GetHashCode()
IL_06df:  stelem.i4 <- Done adding the last one. Very good optimization here is that directly our custom Combine is called.
IL_06e0:  call       int32 System.Numerics.Hashing.HashHelpers::Combine(int32[])
            */
            System.Int32 result = 0;
            // Combine one-by-one the elements.
            for (System.Int64 I = 0; I < nums.LongLength; I++) { result = Combine(result, nums[I]); }
            return result;
        }

        /// <summary>
        /// Combines the hash codes taken from specified objects and returns their combined result. <br />
        /// This method works better when combining four or more hash codes! <br />
        /// NOTE: This method is unsafe if an implementer has overriden 
        /// <see cref="System.Object.GetHashCode()"/> and throws unconditionally an exception !
        /// </summary>
        /// <param name="objects">An array of objects to take the hash codes from.</param>
        /// <returns>The combined result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Int32 Combine(params System.Object[] objects)
        {
            System.Int32 result = 0;
            // Combine one-by-one the elements.
            for (System.Int64 I = 0; I < objects.LongLength; I++) { result = Combine(result, objects[I].GetHashCode()); }
            return result;
        }

        /// <summary>
        /// Combines three hash codes and returns their combined result.
        /// </summary>
        /// <param name="one">The first hash code.</param>
        /// <param name="two">The second hash code.</param>
        /// <param name="three">The third hash code.</param>
        /// <returns>The combined result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Int32 Combine(int one, int two, int three)
         => Combine(Combine(one, two), three);
    }
}