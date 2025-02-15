
using System;
using System.IO;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace DotNetResourcesExtensions.Internal.CustomFormatter.Converters
{
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;

#if WINDOWS10_0_17763_0_OR_GREATER || NET471_OR_GREATER
    using System.Drawing.Imaging;
#endif

    internal sealed class DoubleRepresentation : DefaultArrayRepresentation<System.Double>
    {
        public override Converter<double, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Double value) => value.GetBytes();
            return Method;
        }

        public override Converter<byte[], double> GetUntransformMethod()
        {
            System.Double Method(System.Byte[] data) => data.ToDouble(0);
            return Method;
        }
    }

    internal sealed class IntRepresentation : DefaultArrayRepresentation<System.Int32>
    {
        public override Converter<int, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(int value) => value.GetBytes();
            return Method;
        }

        public override Converter<byte[], int> GetUntransformMethod()
        {
            System.Int32 Method(System.Byte[] bytes) => bytes.ToInt32(0);
            return Method;
        }
    }

    internal sealed class SingleRepresentation : DefaultArrayRepresentation<System.Single>
    {
        public override Converter<float, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Single value) => value.GetBytes();
            return Method;
        }

        public override Converter<byte[], float> GetUntransformMethod()
        {
            System.Single Method(System.Byte[] bytes) => bytes.ToSingle(0);
            return Method;
        }
    }

    internal sealed class UIntRepresentation : DefaultArrayRepresentation<System.UInt32>
    {
        public override Converter<uint, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.UInt32 value) => value.GetBytes();
            return Method;
        }

        public override Converter<byte[], uint> GetUntransformMethod()
        {
            System.UInt32 Method(System.Byte[] bytes) => bytes.ToUInt32(0);
            return Method;
        }
    }

    internal sealed class ShortRepresentation : DefaultArrayRepresentation<System.Int16>
    {
        public override Converter<short, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Int16 value) => value.GetBytes();
            return Method;
        }

        public override Converter<byte[], short> GetUntransformMethod()
        {
            System.Int16 Method(System.Byte[] bytes) => bytes.ToInt16(0);
            return Method;
        }
    }

    internal sealed class UShortRepresentation : DefaultArrayRepresentation<System.UInt16>
    {
        public override Converter<ushort, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.UInt16 value) => value.GetBytes();
            return Method;
        }

        public override Converter<byte[], ushort> GetUntransformMethod()
        {
            System.UInt16 Method(System.Byte[] bytes) => bytes.ToUInt16(0);
            return Method;
        }
    }

    internal sealed class LongRepresentation : DefaultArrayRepresentation<System.Int64>
    {
        public override Converter<System.Int64, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Int64 value) => value.GetBytes();
            return Method;
        }

        public override Converter<byte[], System.Int64> GetUntransformMethod()
        {
            System.Int64 Method(System.Byte[] bytes) => bytes.ToInt64(0);
            return Method;
        }
    }

    internal sealed class ULongRepresentation : DefaultArrayRepresentation<System.UInt64>
    {
        public override Converter<System.UInt64, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.UInt64 value) => value.GetBytes();
            return Method;
        }

        public override Converter<byte[], System.UInt64> GetUntransformMethod()
        {
            System.UInt64 Method(System.Byte[] bytes) => bytes.ToUInt64(0);
            return Method;
        }
    }

    /// <summary>
    /// Exposes methods to convert a <see cref="Stream"/> to a byte array and the reverse.
    /// </summary>
    internal sealed class StreamRepresentation : DefaultArrayRepresentation<System.IO.Stream>
    {
        private const System.Int32 MAXARRAYLENGTH = 0x100000;

        public override Converter<Stream, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.IO.Stream stream)
            {
                if (stream is null) { throw new ArgumentNullException(nameof(stream)); }
                if (stream.CanRead == false) { throw new ArgumentException("The stream must be a readable stream." , nameof(stream)); }
                if (stream.Length > MAXARRAYLENGTH) { throw new ArgumentException(
                    $"The stream length cannot be more than {MAXARRAYLENGTH} bytes ." , nameof(stream)); }
                return stream.ReadBytes(stream.Length);
            }
            return Method;
        }

        public override Converter<byte[], Stream> GetUntransformMethod()
        {
            System.IO.Stream Method(System.Byte[] bytes)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.Length <= 0) { throw new ArgumentException("The byte array length must not be zero.", nameof(bytes)); }
                System.IO.MemoryStream MS = new(bytes);
                bytes = null;
                MS.Position = 0;
                return MS;
            }
            return Method;
        }
    }

    internal sealed class DateTimeRepresentation : DefaultArrayRepresentation<System.DateTime>
    {
        public override Converter<DateTime, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.DateTime datetime)
            {
                // For serializing and getting the original DateTime , we need to call the ToBinary method.
                // This is enough to encode all required data to represent this structure to bytes.
                return datetime.ToBinary().GetBytes();
            }
            return Method;
        }

        public override Converter<byte[], DateTime> GetUntransformMethod()
        {
            System.DateTime Method(System.Byte[] bytes) 
            {
                // The binary data is just a long , we need a length of 8 bytes , so.
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.LongLength != 8) { throw new ArgumentException("The array size must be exactly 8 bytes." , nameof(bytes)); }
                return System.DateTime.FromBinary(bytes.ToInt64(0));
            }
            return Method;
        }
    }

    internal sealed class DateTimeOffsetRepresentation : DefaultArrayRepresentation<System.DateTimeOffset>
    {
        public override Converter<DateTimeOffset, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.DateTimeOffset offset)
            {
                // For the DateTimeOffset , we need to serialize two longs , because the one will represent the 
                // ticks for the DateTimeOffset , while the second it refers to the offset itself , which is a TimeSpan.
                // The TimeSpan itself can also in turn be serialized just with some ticks too.
                System.Byte[] result = new System.Byte[16];
                System.Byte[] temp = offset.Ticks.GetBytes();
                Array.ConstrainedCopy(temp, 0, result, 0, 8);
                temp = offset.Offset.Ticks.GetBytes();
                Array.ConstrainedCopy(temp, 0, result, 8, 8);
                return result;
            }
            return Method;
        }

        public override Converter<byte[], DateTimeOffset> GetUntransformMethod()
        {
            System.DateTimeOffset Method(System.Byte[] bytes)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.LongLength != 16) { throw new ArgumentException("The array size must be exactly 16 bytes.", nameof(bytes)); }
                return new(bytes.ToInt64(0), new TimeSpan(bytes.ToInt64(8)));
            }
            return Method;
        }
    }

    internal sealed class TimeSpanRepresentation : DefaultArrayRepresentation<System.TimeSpan>
    {
        public override Converter<TimeSpan, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.TimeSpan timespan)
            {
                // TimeSpan can be just initialised with ticks , so only saving the ticks does the work for us.
                return timespan.Ticks.GetBytes();
            }
            return Method;
        }

        public override Converter<byte[], TimeSpan> GetUntransformMethod()
        {
            TimeSpan Method(System.Byte[] bytes)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.LongLength != 8) { throw new ArgumentException("The array size must be exactly 8 bytes.", nameof(bytes)); }
                return new(bytes.ToInt64(0));
            }
            return Method;
        }
    }

    internal sealed class TypeRepresentation : DefaultArrayRepresentation<System.Type>
    {
        public override Converter<Type, byte[]> GetTransformMethod()
        {
            // Serializing a System.Type is also easy: we just get the
            // AssemblyQualifiedTypeName for that instance , convert it to UTF-8 bytes and we are done.
            System.Byte[] Method(System.Type type) => Encoding.UTF8.GetBytes(type.AssemblyQualifiedName);
            return Method;
        }

        public override Converter<byte[], Type> GetUntransformMethod()
        {
            [RequiresUnreferencedCode("The type mentioned in bytes might have not been loaded in the runtime during retrieval and the method might fail to return the original serialized object.")]
            System.Type Method(System.Byte[] bytes)
            {
                // Any exceptions about the array is thrown by GetString , we will only handle the case if the bytes array is null.
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                return System.Type.GetType(Encoding.UTF8.GetString(bytes));
            }
            return Method;
        }
    }

    internal sealed class GuidRepresentation : DefaultArrayRepresentation<System.Guid>
    {
        public override Converter<Guid, byte[]> GetTransformMethod()
        {
            // Thankfully , the Guid structure has already a byte array method that is it's value. 
            // Use that instead.
            System.Byte[] Method(System.Guid guid) => guid.ToByteArray();
            return Method;
        }

        public override Converter<byte[], Guid> GetUntransformMethod()
        {
            System.Guid Method(System.Byte[] bytes)
            {
                // Any exceptions about the array is thrown by the ctor itself , we will only handle the case if the bytes array is null.
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                return new(bytes);
            }
            return Method;
        }
    }

    internal sealed class DecimalRepresentation : DefaultArrayRepresentation<System.Decimal>
    {
        // A pinnable structure that can cast directly to a byte array and the reverse
        // This was created for System.Decimal.
        // Purpose of this is to create a independent and simpler version for converting decimals.
        // Although that there are already methods in UnsafeMethods class that convert decimals , 
        // this structure acts just like the same manner as converting to bytes , but does that much more efficiently.
        // Note that the decimal is not clearly an unmanaged type; just it is because it does exist as a native datatype.
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        private unsafe struct DecimalPinnable
        {
            public static DecimalPinnable GetPinnable(System.Decimal dec) => Unsafe.As<System.Decimal, DecimalPinnable>(ref dec);

            public static DecimalPinnable GetFromBytes(System.Byte[] bytes , System.Int32 startindex)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (sizeof(DecimalPinnable) > bytes.Length - startindex) {
                    throw new ArgumentException("There are not enough elements to copy so that the DecimalPinnable can be initialized.");
                }
                DecimalPinnable result = new();
                fixed (System.Byte* ptr = &Unsafe.AsRef(result.pin))
                {
                    fixed (System.Byte* array = &bytes[startindex])
                    {
                        Unsafe.CopyBlockUnaligned(ptr, array, sizeof(DecimalPinnable).ToUInt32());
                    }
                }
                return result;
            }

            // Field that is used by unsafe operations to get the structure pointer directly. Do not make it read-only nor set any values to it.
            [FieldOffset(0)]
            private System.Byte pin;

            [FieldOffset(0)]
            public System.Int32 Flags;

            [FieldOffset(4)]
            public System.UInt32 Low32;

            [FieldOffset(8)]
            public System.UInt64 High32;

            public readonly System.Span<System.Byte> GetBytes()
            {
                fixed (System.Byte* ptr = &Unsafe.AsRef(pin))
                {
                    return new System.Span<System.Byte>(ptr, sizeof(DecimalPinnable));
                }
            }

            public readonly System.Decimal GetDecimal() => Unsafe.As<DecimalPinnable, System.Decimal>(ref Unsafe.AsRef(this));
        }

        public override Converter<decimal, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Decimal dec) => DecimalPinnable.GetPinnable(dec).GetBytes().ToArray();
            return Method;
        }

        public override Converter<byte[], decimal> GetUntransformMethod()
        {
            System.Decimal Method(System.Byte[] bytes) => DecimalPinnable.GetFromBytes(bytes, 0).GetDecimal();
            return Method;
        }
    }

    internal sealed class BooleanRepresentation : DefaultArrayRepresentation<System.Boolean>
    {
        public override Converter<bool, byte[]> GetTransformMethod()
        {
            // The below inline method performs this task:
            // Gets a boolean , checks for true.
            // If boolean is true , stores 1 in the byte array.
            // If boolean is false , stores 0 in the byte array.
            System.Byte[] Method(System.Boolean boolean) => 
                new System.Byte[1] { (boolean ? 1 : 0).ToByte() };
            return Method;
        }

        public override Converter<byte[], bool> GetUntransformMethod()
        {
            System.Boolean Method(System.Byte[] bytes) => bytes[0] == 1;
            return Method;
        }
    }

    internal sealed class UriRepresentation : DefaultArrayRepresentation<System.Uri>
    {
        public override Converter<Uri, byte[]> GetTransformMethod()
        {
            System.Byte[] Method([NotNull] Uri uri) => Encoding.UTF8.GetBytes(
                System.String.IsNullOrEmpty(
                    // This null check is a bit dangerous...
                    uri?.OriginalString) ? System.String.Empty : uri.OriginalString
                );
            return Method;
        }

        public override Converter<byte[], Uri> GetUntransformMethod()
        {
            System.Uri Method(System.Byte[] bytes) => new(Encoding.UTF8.GetString(bytes));
            return Method;
        }
    }

    internal sealed class VersionRepresentation : DefaultArrayRepresentation<System.Version>
    {
        private const System.Int32 LENGTH = 16;

        public override Converter<Version, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Version ver)
            {
                System.Byte[] result = new System.Byte[LENGTH];
                // The Version object is encoded as follows:
                // MajorNumber -> MinorNumber -> BuildNumber -> RevisionNumber.
                System.Byte[] temp = ver.Major.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 0, 4);
                temp = ver.Minor.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 4, 4);
                temp = ver.Build.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 8, 4);
                temp = ver.Revision.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 12, 4);
                return result;
            }
            return Method;
        }

        public override Converter<byte[], Version> GetUntransformMethod()
        {
            Version Method(System.Byte[] bytes)
            {
                // To decode the object , we have a small issue:
                // If any of encoded Build or Revision numbers are -1 , then we must call the appropriate constructor for that..
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.LongLength != LENGTH) { throw new ArgumentException($"Array length must be exactly {LENGTH} bytes.", nameof(bytes)); }
                System.Int32 VMJ, VMI, VB, VR;
                VMJ = bytes.ToInt32(0);
                VMI = bytes.ToInt32(4);
                VB = bytes.ToInt32(8);
                VR = bytes.ToInt32(12);
                Version result;
                if (VB == -1)
                {
                    // Just make sure to initalise the Version using Major and Minor numbers only.
                    result = new(VMJ , VMI);
                } else if (VR == -1)
                {
                    // Initialise using the build number too.
                    result = new(VMJ, VMI , VB);
                } else
                {
                    // Everything can be used as valid numbers! Use all so.
                    result = new(VMJ, VMI, VB , VR);
                }
                // So we finally get the equal object as the encoded one. Return that object.
                return result;
            }
            return Method;
        }
    }

    internal sealed class ApplicationIdRepresentation : DefaultArrayRepresentation<System.ApplicationId>
    {
        private const System.Int32 MINLEN = 16 + 8;

        public override Converter<ApplicationId, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(ApplicationId id)
            {
                if (id is null) { throw new ArgumentNullException(nameof(id)); }
                System.Int32 fl = MINLEN;
                if (id.Culture is not null) {
                    fl += Encoding.Unicode.GetByteCount(id.Culture);
                    fl += 4;
                }
                if (id.Name is not null) {
                    fl += Encoding.Unicode.GetByteCount(id.Name);
                    fl += 4;
                }
                if (id.ProcessorArchitecture is not null) {
                    fl += Encoding.Unicode.GetByteCount(id.ProcessorArchitecture);
                    fl += 4;
                }
                // Application ID will be transformed as:
                // (PublicKeyToken-8bytes) -> (Version-16bytes) -> [CultureLength]Culture -> [NameLength]Name -> [ProcArchLength]ProcessorArchitecture
                System.Byte[] result = new System.Byte[fl] , temp = id.PublicKeyToken , tlen;
                System.Array.ConstrainedCopy(temp , 0 , result , 0, 8);
                temp = System.BitConverter.GetBytes(id.Version.Major);
                System.Array.ConstrainedCopy(temp, 0, result, 8, 4);
                temp = System.BitConverter.GetBytes(id.Version.Minor);
                System.Array.ConstrainedCopy(temp, 0, result, 12, 4);
                temp = System.BitConverter.GetBytes(id.Version.Build);
                System.Array.ConstrainedCopy(temp, 0, result, 16, 4);
                temp = System.BitConverter.GetBytes(id.Version.Revision);
                System.Array.ConstrainedCopy(temp, 0, result, 20, 4);
                // From now on we have variable lengths
                System.Int32 idx = 24;
                if (id.Culture is not null) {
                    // Compute culture length.
                    temp = Encoding.Unicode.GetBytes(id.Culture);
                    tlen = System.BitConverter.GetBytes(temp.Length);
                    System.Array.ConstrainedCopy(tlen, 0, result, idx, 4);
                    idx += 4;
                    System.Array.ConstrainedCopy(temp , 0 , result , idx , temp.Length);
                    idx += temp.Length;
                } else {
                    tlen = System.BitConverter.GetBytes(-4); // Encode -4 to indicate that the field is null , and no data exist.
                    System.Array.ConstrainedCopy(tlen, 0, result, idx, 4);
                    idx += 4;
                }
                if (id.Name is not null) {
                    // Compute name length.
                    temp = Encoding.Unicode.GetBytes(id.Name);
                    tlen = System.BitConverter.GetBytes(temp.Length);
                    System.Array.ConstrainedCopy(tlen, 0, result, idx, 4);
                    idx += 4;
                    System.Array.ConstrainedCopy(temp, 0, result, idx, temp.Length);
                    idx += temp.Length;
                } else {
                    tlen = System.BitConverter.GetBytes(-4); // Encode -4 to indicate that the field is null , and no data exist.
                    System.Array.ConstrainedCopy(tlen, 0, result, idx, 4);
                    idx += 4;
                }
                if (id.ProcessorArchitecture is not null)
                {
                    // Compute ProcArch length.
                    temp = Encoding.Unicode.GetBytes(id.ProcessorArchitecture);
                    tlen = System.BitConverter.GetBytes(temp.Length);
                    System.Array.ConstrainedCopy(tlen, 0, result, idx, 4);
                    idx += 4;
                    System.Array.ConstrainedCopy(temp, 0, result, idx, temp.Length);
                    idx += temp.Length;
                } else {
                    tlen = System.BitConverter.GetBytes(-4); // Encode -4 to indicate that the field is null , and no data exist.
                    System.Array.ConstrainedCopy(tlen, 0, result, idx, 4);
                    idx += 4;
                }
                return result;
            }
            return Method;
        }

        public override Converter<byte[], ApplicationId> GetUntransformMethod()
        {
            ApplicationId Method(System.Byte[] bytes)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.Length < MINLEN) { throw new ArgumentException($"The array length must be at least {MINLEN} bytes."); }
                System.Byte[] pkt = new System.Byte[8]; // PublicKeyToken
                System.Array.ConstrainedCopy(bytes , 0 , pkt , 0, 8);
                System.Int32 VMJ, VMI, VB, VR;
                System.String ct, parch, name;
                ct = parch = name = System.String.Empty;
                VMJ = System.BitConverter.ToInt32(bytes, 8);
                VMI = System.BitConverter.ToInt32(bytes, 12);
                VB = System.BitConverter.ToInt32(bytes, 16);
                VR = System.BitConverter.ToInt32(bytes, 20);
                Version ver;
                if (VB == -1) {
                    // Just make sure to initalise the Version using Major and Minor numbers only.
                    ver = new(VMJ, VMI);
                } else if (VR == -1) {
                    // Initialise using the build number too.
                    ver = new(VMJ, VMI, VB);
                } else {
                    // Everything can be used as valid numbers! Use all so.
                    ver = new(VMJ, VMI, VB, VR);
                }
                // We have now the version object too. All the rest are variable length fields.
                System.Int32 idx = 24;
                // The first field is the culture field.
                System.Int32 rb = System.BitConverter.ToInt32(bytes , idx);
                idx += 4; // Skip 4 bytes to read the culture field if exists.
                if (rb > 0) { // Although I have defined -4 as the invalid value , we need the length to be > 0 in any way.
                    ct = Encoding.Unicode.GetString(bytes, idx, rb);
                    idx += rb;
                }
                // The second field is the name field.
                rb = System.BitConverter.ToInt32(bytes, idx);
                idx += 4; // Skip 4 bytes to read the name field if exists.
                if (rb > 0) { // Although I have defined -4 as the invalid value , we need the length to be > 0 in any way.
                    name = Encoding.Unicode.GetString(bytes, idx, rb);
                    idx += rb;
                }
                // The third field is the process architecture field.
                rb = System.BitConverter.ToInt32(bytes, idx);
                idx += 4; // Skip 4 bytes to read the process architecture field if exists.
                if (rb > 0) { // Although I have defined -4 as the invalid value , we need the length to be > 0 in any way.
                    parch = Encoding.Unicode.GetString(bytes, idx, rb);
                    idx += rb;
                }
                return new(pkt , name , ver, parch , ct);
            }
            return Method;
        }
    }

#if NET7_0_OR_GREATER // Array representations built for CoreCLR

    internal sealed class HalfRepresentation : DefaultArrayRepresentation<System.Half>
    {
        public override Converter<Half, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Half half) => System.BitConverter.GetBytes(half);
            return Method;
        }

        public override Converter<byte[], Half> GetUntransformMethod()
        {
            System.Half Method(System.Byte[] bytes) => System.BitConverter.ToHalf(bytes , 0);
            return Method;
        }
    }

    internal sealed class Int128Representation : DefaultArrayRepresentation<System.Int128>
    {
        [StructLayout(LayoutKind.Explicit , Size = 16)]
        private unsafe struct PinnableInt128
        {
            public static PinnableInt128 GetPinnable(System.Int128 dec) => Unsafe.As<System.Int128, PinnableInt128>(ref dec);

            public static PinnableInt128 GetFromBytes(System.Byte[] bytes, System.Int32 startindex)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (sizeof(PinnableInt128) > bytes.Length - startindex)
                {
                    throw new ArgumentException("There are not enough elements to copy so that the pinnable can be initialized.");
                }
                PinnableInt128 result = new();
                fixed (System.Byte* ptr = &Unsafe.AsRef(result.pin))
                {
                    fixed (System.Byte* array = &bytes[startindex])
                    {
                        Unsafe.CopyBlockUnaligned(ptr, array, sizeof(PinnableInt128).ToUInt32());
                    }
                }
                return result;
            }

            // Field that is used by unsafe operations to get the structure pointer directly. Do not make it read-only nor set any values to it.
            [FieldOffset(0)]
            private System.Byte pin;

            [FieldOffset(0)]
            public System.Int64 Low64;

            [FieldOffset(8)]
            public System.Int64 High64;

            public readonly System.Span<System.Byte> GetBytes()
            {
                fixed (System.Byte* ptr = &Unsafe.AsRef(pin))
                {
                    return new System.Span<System.Byte>(ptr, sizeof(PinnableInt128));
                }
            }

            public readonly System.Int128 GetInt128() => Unsafe.As<PinnableInt128, System.Int128>(ref Unsafe.AsRef(this));
        }

        private static System.Int128 OldDecodeMethod(System.Byte[] bytes)
        {
            if (bytes.LongLength != DigitLength) { throw new ArgumentException($"The array length must be exactly {DigitLength} bytes."); }
            Int128 result = 0, prg = 1;
            if (bytes[0] == 0) { return result; } // If zero return as it is
            for (System.Int32 I = bytes[1] + 1; I > 1; I--)
            { // bytes[1] contain the number of digits , +1 to access the associated index
                result += bytes[I] * prg;
                prg *= 10;
            }
            if (bytes[0] == 1) { result = -result; } // When negative convert it to negative value.
            return result;
        }

        private const System.Int32 ByteEncodedInt128Len = 16;
        private const System.Int32 DigitLength = 40 + 1; // 40 are the most characters produced by a negative Int128 number , plus it's minus sign , plus 1 for counting the number of digits.

        public override Converter<Int128, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Int128 largeint) => PinnableInt128.GetPinnable(largeint).GetBytes().ToArray();
            return Method;
        }

        public override Converter<byte[], Int128> GetUntransformMethod()
        {
            Int128 Method(System.Byte[] bytes)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.Length == DigitLength) { return OldDecodeMethod(bytes); }
                if (bytes.Length == ByteEncodedInt128Len) { return PinnableInt128.GetFromBytes(bytes, 0).GetInt128(); }
                throw new ArgumentException($"The array size must be exactly {ByteEncodedInt128Len} bytes." , nameof(bytes));
            }
            return Method;
        }
    }

    internal sealed class UInt128Representation : DefaultArrayRepresentation<System.UInt128>
    {
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        private unsafe struct PinnableUInt128
        {
            public static PinnableUInt128 GetPinnable(System.UInt128 dec) => Unsafe.As<System.UInt128, PinnableUInt128>(ref dec);

            public static PinnableUInt128 GetFromBytes(System.Byte[] bytes, System.Int32 startindex)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (sizeof(PinnableUInt128) > bytes.Length - startindex)
                {
                    throw new ArgumentException("There are not enough elements to copy so that the pinnable can be initialized.");
                }
                PinnableUInt128 result = new();
                fixed (System.Byte* ptr = &Unsafe.AsRef(result.pin))
                {
                    fixed (System.Byte* array = &bytes[startindex])
                    {
                        Unsafe.CopyBlockUnaligned(ptr, array, sizeof(PinnableUInt128).ToUInt32());
                    }
                }
                return result;
            }

            // Field that is used by unsafe operations to get the structure pointer directly. Do not make it read-only nor set any values to it.
            [FieldOffset(0)]
            private System.Byte pin;

            [FieldOffset(0)]
            public System.UInt64 Low64;

            [FieldOffset(8)]
            public System.UInt64 High64;

            public readonly System.Span<System.Byte> GetBytes()
            {
                fixed (System.Byte* ptr = &Unsafe.AsRef(pin))
                {
                    return new System.Span<System.Byte>(ptr, sizeof(PinnableUInt128));
                }
            }

            public readonly System.UInt128 GetUInt128() => Unsafe.As<PinnableUInt128, System.UInt128>(ref Unsafe.AsRef(this));
        }

        private static System.UInt128 OldDecodeMethod(System.Byte[] bytes)
        {
            if (bytes.LongLength != DigitLength) { throw new ArgumentException($"The array length must be exactly {DigitLength} bytes."); }
            UInt128 result = 0, prg = 1;
            for (System.Int32 I = bytes[0]; I > 0; I--)
            {
                result += bytes[I] * prg;
                prg *= 10;
            }
            return result; // Even if it would be zero , zero it will return.
        }

        private const System.Int32 ByteEncodedUInt128Len = 16;
        private const System.Int32 DigitLength = 39 + 1; // 39 are the most characters produced by a UInt128 number , plus 1 for counting the number of digits.

        public override Converter<UInt128, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.UInt128 number) => PinnableUInt128.GetPinnable(number).GetBytes().ToArray();
            return Method;
        }

        public override Converter<byte[], UInt128> GetUntransformMethod()
        {
            System.UInt128 Method(System.Byte[] bytes)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.Length == DigitLength) { return OldDecodeMethod(bytes); }
                if (bytes.Length == ByteEncodedUInt128Len) { return PinnableUInt128.GetFromBytes(bytes, 0).GetUInt128(); }
                throw new ArgumentException($"The array size must be exactly {ByteEncodedUInt128Len} bytes.", nameof(bytes));
            }
            return Method;
        }
    }

    internal sealed class DateOnlyRepresentation : DefaultArrayRepresentation<System.DateOnly>
    {
        [StructLayout(LayoutKind.Explicit, Size = 4)]
        private unsafe struct PinnableDateOnly
        {
            public static PinnableDateOnly GetPinnable(System.DateOnly dec) => Unsafe.As<System.DateOnly, PinnableDateOnly>(ref dec);

            public static PinnableDateOnly GetFromBytes(System.Byte[] bytes, System.Int32 startindex)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (sizeof(PinnableDateOnly) > bytes.Length - startindex)
                {
                    throw new ArgumentException("There are not enough elements to copy so that the pinnable can be initialized.");
                }
                PinnableDateOnly result = new();
                fixed (System.Byte* ptr = &Unsafe.AsRef(result.pin))
                {
                    fixed (System.Byte* array = &bytes[startindex])
                    {
                        Unsafe.CopyBlockUnaligned(ptr, array, sizeof(PinnableDateOnly).ToUInt32());
                    }
                }
                return result;
            }

            // Field that is used by unsafe operations to get the structure pointer directly. Do not make it read-only nor set any values to it.
            [FieldOffset(0)]
            private System.Byte pin;

            [FieldOffset(0)]
            public System.Int32 Day;

            public readonly System.Span<System.Byte> GetBytes()
            {
                fixed (System.Byte* ptr = &Unsafe.AsRef(pin))
                {
                    return new System.Span<System.Byte>(ptr, sizeof(PinnableDateOnly));
                }
            }

            public readonly System.DateOnly GetDateOnly() => Unsafe.As<PinnableDateOnly, System.DateOnly>(ref Unsafe.AsRef(this));
        }

        private const System.Int32 ByteSize = 4;
        private const System.Int32 OldSize = 6;

        public override Converter<DateOnly, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.DateOnly dt) => dt.DayNumber.GetBytes();
            return Method;
        }

        public override Converter<byte[], DateOnly> GetUntransformMethod()
        {
            DateOnly Method(System.Byte[] bytes)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.Length == ByteSize) { return PinnableDateOnly.GetFromBytes(bytes , 0).GetDateOnly(); }
                if (bytes.Length != OldSize) { throw new ArgumentException($"Array length must be exactly {ByteSize} bytes."); }
                return new(bytes.ToInt32(2), bytes[1] , bytes[0]);
            }
            return Method;
        }
    }

    internal sealed class TimeOnlyRepresentation : DefaultArrayRepresentation<System.TimeOnly>
    {
        private const System.Int32 Size = 8;

        public override Converter<TimeOnly, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.TimeOnly time) => time.Ticks.GetBytes();
            return Method;
        }

        public override Converter<byte[], TimeOnly> GetUntransformMethod()
        {
            System.TimeOnly Method(System.Byte[] bytes)
            {
                if (bytes is null) { throw new System.ArgumentNullException(nameof(bytes)); }
                if (bytes.LongLength != Size) { throw new System.ArgumentException($"The array length must be exactly {Size} bytes."); }
                return new(bytes.ToInt64(0));
            }
            return Method;
        }
    }

#endif

    internal sealed class PointRepresentation : DefaultArrayRepresentation<System.Drawing.Point>
    {
        public override Converter<Point, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Drawing.Point point)
            {
                if (point.IsEmpty) { throw new ArgumentNullException(nameof(point)); }
                // We know that the int as bytes array length is 4. 
                // During write , the resulting array will be 8 bytes , 4 for the X point , and 4 for the Y point.
                System.Byte[] result = new System.Byte[8];
                System.Byte[] temp = point.X.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 0, 4);
                temp = point.Y.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 4, 4);
                return result;
            }
            return Method;
        }

        public override Converter<byte[], Point> GetUntransformMethod()
        {
            System.Drawing.Point Method(System.Byte[] bytes)
            {
                // The array size must be exactly 8.
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.LongLength != 8) { throw new ArgumentException("The array size must be exactly 8."); }
                // Create our resulting structure.
                System.Drawing.Point point = new();
                point.X = bytes.ToInt32(0);
                point.Y = bytes.ToInt32(4);
                return point;
            }
            return Method;
        }
    }

    internal sealed class PointFRepresentation : DefaultArrayRepresentation<System.Drawing.PointF>
    {
        public override Converter<PointF, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Drawing.PointF point)
            {
                if (point.IsEmpty) { throw new ArgumentNullException(nameof(point)); }
                // We know that the float as bytes array length is 4. 
                // During write , the resulting array will be 8 bytes , 4 for the X point , and 4 for the Y point.
                System.Byte[] result = new System.Byte[8];
                System.Byte[] temp = System.BitConverter.GetBytes(point.X);
                System.Array.ConstrainedCopy(temp, 0, result, 0, 4);
                temp = System.BitConverter.GetBytes(point.Y);
                System.Array.ConstrainedCopy(temp, 0, result, 4, 4);
                return result;
            }
            return Method;
        }

        public override Converter<byte[], PointF> GetUntransformMethod()
        {
            System.Drawing.PointF Method(System.Byte[] bytes)
            {
                // The array size must be exactly 8.
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.LongLength != 8) { throw new ArgumentException("The array size must be exactly 8."); }
                // Create our resulting structure.
                System.Drawing.PointF point = new();
                point.X = System.BitConverter.ToSingle(bytes, 0);
                point.Y = System.BitConverter.ToSingle(bytes, 4);
                return point;
            }
            return Method;
        }
    }

    internal sealed class SizeRepresentation : DefaultArrayRepresentation<System.Drawing.Size>
    {
        public override Converter<Size, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Drawing.Size size)
            {
                if (size.IsEmpty) { throw new ArgumentNullException(nameof(size)); }
                // We know that the int as bytes array length is 4. 
                // During write , the resulting array will be 8 bytes , 4 for the width and 4 for the height.
                System.Byte[] result = new System.Byte[8];
                System.Byte[] temp = size.Width.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 0, 4);
                temp = size.Height.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 4, 4);
                return result;
            }
            return Method;
        }

        public override Converter<byte[], Size> GetUntransformMethod()
        {
            System.Drawing.Size Method(System.Byte[] bytes)
            {
                // The array size must be exactly 8.
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.LongLength != 8) { throw new ArgumentException("The array size must be exactly 8."); }
                System.Drawing.Size result = new();
                result.Width = bytes.ToInt32(0);
                result.Height = bytes.ToInt32(4);
                return result;
            }
            return Method;
        }
    }

    internal sealed class SizeFRepresentation : DefaultArrayRepresentation<System.Drawing.SizeF>
    {
        public override Converter<SizeF, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Drawing.SizeF size)
            {
                if (size.IsEmpty) { throw new ArgumentNullException(nameof(size)); }
                // We know that the int as bytes array length is 4. 
                // During write , the resulting array will be 8 bytes , 4 for the width and 4 for the height.
                System.Byte[] result = new System.Byte[8];
                System.Byte[] temp = size.Width.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 0, 4);
                temp = size.Height.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 4, 4);
                return result;
            }
            return Method;
        }

        public override Converter<byte[], SizeF> GetUntransformMethod()
        {
            System.Drawing.SizeF Method(System.Byte[] bytes)
            {
                // The array size must be exactly 8.
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.LongLength != 8) { throw new ArgumentException("The array size must be exactly 8."); }
                return new(bytes.ToSingle(0) , bytes.ToSingle(4));
            }
            return Method;
        }
    }

    internal sealed class RectangleRepresentation : DefaultArrayRepresentation<System.Drawing.Rectangle>
    {
        public override Converter<Rectangle, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(Rectangle rectangle)
            {
                if (rectangle.IsEmpty) { throw new ArgumentNullException(nameof(rectangle)); }
                // We know that the int as bytes array length is 4. 
                // During write , the resulting array will be 16 bytes , 4 for the width
                // , 4 for the height , 4 for the X point and 4 for the Y point.
                // We do not need to encode other properties , because these are only part of the
                // Rectangle.
                System.Byte[] result = new System.Byte[16];
                System.Byte[] temp = rectangle.Width.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 0, 4);
                temp = rectangle.Height.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 4, 4);
                temp = rectangle.X.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 8, 4);
                temp = rectangle.Y.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 12, 4);
                return result;
            }
            return Method;
        }

        public override Converter<byte[], Rectangle> GetUntransformMethod()
        {
            Rectangle Method(System.Byte[] bytes)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.LongLength != 16) { throw new ArgumentException("The array size must be exactly 16."); }
                return new(bytes.ToInt32(8) , bytes.ToInt32(12) , bytes.ToInt32(0) , bytes.ToInt32(4));
            }
            return Method;
        }
    }

    internal sealed class RectangleFRepresentation : DefaultArrayRepresentation<System.Drawing.RectangleF>
    {
        public override Converter<RectangleF, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(RectangleF rectangle)
            {
                if (rectangle.IsEmpty) { throw new ArgumentNullException(nameof(rectangle)); }
                // We know that the float as bytes array length is 4. 
                // During write , the resulting array will be 16 bytes , 4 for the width
                // , 4 for the height , 4 for the X point and 4 for the Y point.
                // We do not need to encode other properties ,
                // because the Rectangle object only depends on these
                // 4 properties.
                System.Byte[] result = new System.Byte[16];
                System.Byte[] temp = rectangle.Width.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 0, 4);
                temp = rectangle.Height.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 4, 4);
                temp = rectangle.X.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 8, 4);
                temp = rectangle.Y.GetBytes();
                System.Array.ConstrainedCopy(temp, 0, result, 12, 4);
                return result;
            }
            return Method;
        }

        public override Converter<byte[], RectangleF> GetUntransformMethod()
        {
            RectangleF Method(System.Byte[] bytes)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.LongLength != 16) { throw new ArgumentException("The array size must be exactly 16."); }
                return new(bytes.ToSingle(8) , bytes.ToSingle(12) , bytes.ToSingle(0) , bytes.ToSingle(4));
            }
            return Method;
        }
    }

#if WINDOWS10_0_17763_0_OR_GREATER || NET471_OR_GREATER

    // These types are for Windows only , but when they are available ,
    // these convert some critical classes of System.Drawing namespace.

    internal sealed class BitmapRepresentation : DefaultArrayRepresentation<System.Drawing.Bitmap>
    {
        // The below boilerplate serialization code was acquired from a decompiled .NET Framework flavor
        // of System.Drawing. They are seem to use the below technique to serialize images...
        private struct OBJECTHEADER
        {
            public short signature;

            public short headersize;

            public short objectType;

            public short nameLen;

            public short classLen;

            public short nameOffset;

            public short classOffset;

            public short width;

            public short height;

            public IntPtr pInfo;

            public OBJECTHEADER()
            {
                signature = 0;
                headersize = 0;
                objectType = 0;
                nameLen = 0;
                classLen = 0;
                nameOffset = 0;
                classOffset = 0;
                width = 0;
                height = 0;
                pInfo = IntPtr.Zero;
            }
        }

        internal static ImageCodecInfo FindEncoder(ImageFormat fmt)
        {
            ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo ici in imageEncoders)
            {
                if (ici.FormatID.Equals(fmt.Guid)) { return ici; }
            }
            return null;
        }

        internal static void Save(System.Drawing.Image img, MemoryStream stream)
        {
            ImageFormat imageFormat = img.RawFormat;
            if (imageFormat == ImageFormat.Jpeg)
            {
                imageFormat = ImageFormat.Png;
            }
            ImageCodecInfo imageCodecInfo = FindEncoder(imageFormat);
            imageCodecInfo ??= FindEncoder(ImageFormat.Png);
            img.Save(stream, imageCodecInfo, null);
        }

        internal static System.Byte[] GetBytes(System.Drawing.Image value)
        {
            if (value is null) { throw new ArgumentNullException(nameof(value)); }
            bool flag = false;
            MemoryStream memoryStream = null;
            Image image2 = null;
            try
            {
                memoryStream = new MemoryStream();
                image2 = value;
                if (image2.RawFormat.Equals(ImageFormat.Icon))
                {
                    flag = true;
                    image2 = new Bitmap(image2, image2.Width, image2.Height);
                }
                Save(image2, memoryStream);
            } finally {
                memoryStream?.Close();
                if (flag)
                {
                    image2?.Dispose();
                }
            }
            return memoryStream?.ToArray();
        }

        internal static unsafe Stream GetBitmapStream(byte[] rawData)
        {
            try
            {
                fixed (byte* ptr = rawData)
                {
                    IntPtr intPtr = (IntPtr)ptr;
                    if (intPtr == IntPtr.Zero)
                    {
                        return null;
                    }
                    if (rawData.Length <= sizeof(OBJECTHEADER) || Marshal.ReadInt16(intPtr) != 7189)
                    {
                        return null;
                    }
                    OBJECTHEADER oBJECTHEADER = (OBJECTHEADER)Marshal.PtrToStructure(intPtr, typeof(OBJECTHEADER));
                    if (rawData.Length <= oBJECTHEADER.headersize + 18)
                    {
                        return null;
                    }
                    string @string = Encoding.ASCII.GetString(rawData, oBJECTHEADER.headersize + 12, 6);
                    if (@string != "PBrush")
                    {
                        return null;
                    }
                    byte[] bytes = Encoding.ASCII.GetBytes("BM");
                    for (int i = oBJECTHEADER.headersize + 18; i < oBJECTHEADER.headersize + 510 && i + 1 < rawData.Length; i++)
                    {
                        if (bytes[0] == ptr[i] && bytes[1] == ptr[i + 1])
                        {
                            return new MemoryStream(rawData, i, rawData.Length - i);
                        }
                    }
                }
            } catch (OutOfMemoryException) { }
            catch (ArgumentException) { }
            return null;
        }

        internal static System.Drawing.Image GetImage(System.Byte[] array)
        {
            Stream stream = GetBitmapStream(array);
            if (stream == null)
            {
                stream = new MemoryStream(array);
            }
            return Bitmap.FromStream(stream);
        }

        public override Converter<Bitmap, byte[]> GetTransformMethod() => GetBytes;

        public override Converter<byte[], Bitmap> GetUntransformMethod()
        {
            System.Drawing.Bitmap Method(System.Byte[] bytes) => (System.Drawing.Bitmap)GetImage(bytes);
            return Method;
        }
    }

    internal sealed class IconRepresentation : DefaultArrayRepresentation<System.Drawing.Icon>
    {
        public override Converter<Icon, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Drawing.Icon icon)
            {
                if (icon is null) { throw new ArgumentNullException(nameof(icon)); }
                System.IO.MemoryStream MS = null;
                try
                {
                    MS = new();
                    icon.Save(MS);
                    return MS.ToArray();
                } finally { MS?.Dispose(); }
            }
            return Method;
        }

        public override Converter<byte[], Icon> GetUntransformMethod()
        {
            System.Drawing.Icon Method(System.Byte[] bytes)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                System.IO.MemoryStream MS = null;
                try
                {
                    MS = new(bytes);
                    return new(MS);
                } finally { MS?.Dispose(); }
            }
            return Method;
        }
    }

    internal sealed class ColorRepresentation : DefaultArrayRepresentation<System.Drawing.Color>
    {
        public override Converter<Color, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(System.Drawing.Color color)
            {
                if (color.IsEmpty) { throw new ArgumentNullException(nameof(color)); }
                // The Color structure defines the ARGB values as bytes.
                // Though , the BitConverter does not provide encoding to bytes
                // because there are already bytes.
                // So , we can easily encode/decode the Color structure.
                // Note that the encoded Color will not save information related to naming or if it is a known color.
                // But the below is enough to save the Color structure to resources format.
                return new System.Byte[] { color.A , color.R , color.G , color.B };
            }
            return Method;
        }

        public override Converter<byte[], Color> GetUntransformMethod()
        {
            System.Drawing.Color Method(System.Byte[] bytes)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.LongLength != 4) { throw new ArgumentException("The array size must be exactly 4."); }
                return System.Drawing.Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
            }
            return Method;
        }
    }

    internal sealed class SolidBrushRepresentation : DefaultArrayRepresentation<System.Drawing.SolidBrush>
    {
        public override Converter<SolidBrush, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(SolidBrush sb)
            {
                if (sb.Color.IsEmpty) { throw new ArgumentNullException(nameof(sb.Color)); }
                // The Color structure defines the ARGB values as bytes.
                // Though , the BitConverter does not provide encoding to bytes
                // because there are already bytes.
                // So , we can easily encode/decode the Color structure.
                // Note that the encoded Color will not save information related to naming or if it is a known color.
                // But the below is enough to save the Color structure to resources format.
                return new System.Byte[] { sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B };
            }
            return Method;
        }

        public override Converter<byte[], SolidBrush> GetUntransformMethod()
        {
            SolidBrush Method(System.Byte[] bytes)
            {
                if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
                if (bytes.LongLength != 4) { throw new ArgumentException("The array size must be exactly 4."); }
                return new(System.Drawing.Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]));
            }
            return Method;
        }
    }

    internal sealed class StringFormatRepresentation : DefaultArrayRepresentation<System.Drawing.StringFormat>
    {
        private const System.Int32 LENGTH = 28;

        public override Converter<StringFormat, byte[]> GetTransformMethod()
        {
            System.Byte[] Method(StringFormat sf)
            {
                if (sf is null) { throw new ArgumentNullException(nameof(sf)); }
                // Now we have to encode a StringFormat class.
                // To do that:
                // -> Encode the language substitution property.
                // -> Encode the string format flags.
                // -> Encode the string alignment.
                // -> Encode the string line alignment.
                // -> Encode the hotkey prefix.
                // -> Encode the digit substitution method.
                // -> Encode the string trimming.
                // First , initialise our array using LENGTH bytes..
                System.Byte[] result = new System.Byte[LENGTH];
                // Initialise a temporary array. The language substitution property size is 4 bytes.
                System.Byte[] temp;
                // Copy language substitution property.
                temp = System.BitConverter.GetBytes(sf.DigitSubstitutionLanguage);
                System.Array.ConstrainedCopy(temp, 0, result, 0, 4);
                // Copy string format flags.
                temp = System.BitConverter.GetBytes((System.Int32)sf.FormatFlags);
                System.Array.ConstrainedCopy(temp, 0, result, 4, 4);
                // Copy string alignment flags. Although that enum values specify values inside a byte value range , the rest 3 are future-reserved.
                temp = System.BitConverter.GetBytes((System.Int32)sf.Alignment);
                System.Array.ConstrainedCopy(temp, 0, result, 8, 4);
                // Copy string line alignment flags. Although that enum values specify values inside a byte value range , the rest 3 are future-reserved.
                temp = System.BitConverter.GetBytes((System.Int32)sf.LineAlignment);
                System.Array.ConstrainedCopy(temp, 0, result, 12, 4);
                // Copy hotkey prefix. Although that enum values specify values inside a byte value range , the rest 3 are future-reserved.
                temp = System.BitConverter.GetBytes((System.Int32)sf.HotkeyPrefix);
                System.Array.ConstrainedCopy(temp, 0, result, 16, 4);
                // Copy digit substitution method. Although that enum values specify values inside a byte value range , the rest 3 are future-reserved.
                temp = System.BitConverter.GetBytes((System.Int32)sf.DigitSubstitutionMethod);
                System.Array.ConstrainedCopy(temp, 0, result, 20, 4);
                // Copy string trimming. Although that enum values specify values inside a byte value range , the rest 3 are future-reserved.
                temp = System.BitConverter.GetBytes((System.Int32)sf.Trimming);
                System.Array.ConstrainedCopy(temp, 0, result, 24, 4);
                // We are done here. Return the resulting array.
                return result;
            }
            return Method;
        }

        public override Converter<byte[], StringFormat> GetUntransformMethod()
        {
            StringFormat Method(System.Byte[] bytes)
            {
                // Decoding does the exact reverse actions.
                if (bytes is null) { throw new ArgumentNullException("bytes"); }
                if (bytes.LongLength != LENGTH) { throw new ArgumentException($"Array length must be exactly {LENGTH} bytes." , nameof(bytes)); }
                // Decode language and string format flags to pass to the constructor.
                StringFormat result = new((StringFormatFlags)bytes.ToInt32(4), bytes.ToInt32(0));
                // Then , everything else are set through properties and methods...
                result.Alignment = (StringAlignment)bytes.ToInt32(8);
                result.LineAlignment = (StringAlignment)bytes.ToInt32(12);
                result.HotkeyPrefix = (System.Drawing.Text.HotkeyPrefix)bytes.ToInt32(16);
                result.Trimming = (StringTrimming)bytes.ToInt32(24);
                result.SetDigitSubstitution(bytes.ToInt32(0), (StringDigitSubstitute)bytes.ToInt32(20));
                return result;
            }
            return Method;
        }
    }

#endif

}