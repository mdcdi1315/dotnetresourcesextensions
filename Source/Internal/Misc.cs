using System;
using System.Runtime.CompilerServices;

// This file contains all miscellaneous types.

namespace DotNetResourcesExtensions.Internal
{
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
    internal enum StreamMixedClassManagement : System.Byte { None, NotStream, InitialisedWithStream, FileUsed }

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
            finally { RD = null; }
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
