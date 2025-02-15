
using System.Runtime.CompilerServices;

namespace DotNetResourcesExtensions.Internal
{
    /// <summary>
    /// Provides extension methods for fast reading and writing data to streams.
    /// </summary>
    internal unsafe static partial class StreamMethods
    {
        // A typed length for temporary buffers.
        private const System.Int32 BUFSIZE = 2048;
        // A typed length for ReadFixedLengthString method's internal byte buffer.
        private const System.Int32 MaxCharBytesSize = 128;
        private const System.String DefaultPadString = "PAD";

        /// <summary>
        /// Defines a temporary buffer size for operations that must use intermediate buffers.
        /// </summary>
        public const System.Int32 MaxTypicalBufferSize = BUFSIZE;

        /// <summary>
        /// Writes an ASCII-encoded string to the specified stream. <br />
        /// For Unicode values bigger than 255 , the question mark character '?' is written.
        /// </summary>
        /// <param name="stream">The stream to write the specified string.</param>
        /// <param name="str">The string to write to the stream.</param>
        public static void WriteASCIIString(this System.IO.Stream stream, System.String str)
        {
            if (System.String.IsNullOrEmpty(str)) { return; }
            System.Byte[] data = new System.Byte[str.Length];
            System.Char temp;
            for (System.Int32 I = 0; I < str.Length; I++)
            {
                temp = str[I];
                if (temp > 255) { data[I] = 63; continue; }
                data[I] = temp.ToByte();
            }
            stream.Write(data, 0, data.Length);
            data = null;
        }

        /// <summary>
        /// Writes an UTF16-encoded string with little endianess to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write the specified string.</param>
        /// <param name="str">The string to write to the stream.</param>
        public static void WriteUTF16LEString(this System.IO.Stream stream, System.String str)
        {
            if (System.String.IsNullOrEmpty(str)) { return; }
            System.Byte[] data = new System.Byte[str.Length * sizeof(System.Char)];
            System.Byte[] temp;
            for (System.Int32 I = 0 , J = 0; I < str.Length; I++ , J += 2)
            {
                temp = str[I].GetBytes();
                data[J] = temp[0];
                data[J + 1] = temp[1];
                temp = null;
            }
            stream.Write(data , 0 , data.Length);
            stream = null;
        }

        /// <summary>
        /// Writes a fixed-length string of the specified encoding to the stream.
        /// </summary>
        /// <param name="stream">The stream where the fixed-length string will be written to.</param>
        /// <param name="str">The string to write.</param>
        /// <param name="enc">The character encoding under which <paramref name="str"/> will be saved.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="enc"/> was null.</exception>
        public static void WriteFixedLengthString(this System.IO.Stream stream, System.String str , System.Text.Encoding enc)
        {
            if (enc is null) { throw new System.ArgumentNullException(nameof(enc)); }
            if (System.String.IsNullOrEmpty(str))
            {
                Write7BitEncodedInt(stream, 0);
                return;
            }
            System.Int32 len = enc.GetByteCount(str);
            System.Byte[] bt = System.Buffers.ArrayPool<System.Byte>.Shared.Rent(len);
            System.Int32 actual = enc.GetBytes(str, 0, str.Length, bt, 0);
            Write7BitEncodedInt(stream , actual);
            stream.Write(bt , 0 , actual);
            System.Buffers.ArrayPool<System.Byte>.Shared.Return(bt);
            bt = null;
        }

        /// <summary>
        /// Writes characters from a given pad string as many times as it is required by the <paramref name="pads"/> parameter. <br />
        /// If <paramref name="pads"/> has a value greater than the <paramref name="pad"/> parameter length , the string is repeated.
        /// </summary>
        /// <param name="stream">The stream to write the pad string to.</param>
        /// <param name="pad">The pad string to use to write the pads. Can have any content , as long as it is does only use ASCII characters.</param>
        /// <param name="pads">The number of pad bytes to write.</param>
        public static void WritePadString(this System.IO.Stream stream , System.String pad, System.Int32 pads)
        {
            // If no pads required , do not throw an exception. Consider it as a valid call and instead
            // do not write anyting on the target stream.
            // For safety , the method will not also write a padding that is very large (e.g. 8000 pads) , 
            // which a padding will never be such large in a real format.
            if (pads <= 0 || pads > 512) { return; }
            // Assign a default pad string if the user failed to give a proper one.
            if (System.String.IsNullOrEmpty(pad)) { pad = DefaultPadString; } 
            System.Int32 padidx = 0;
            // Create a padding array which will be written to the final target.
            // We do not care about severe memory allocations since this method
            // will only get some random pads which will be usually less than 512 bytes in total.
            System.Byte[] pds = new System.Byte[pads];
            for (System.Int32 I = 0; I < pads; I++)
            {
                pds[I] = pad[padidx].ToByte();
                padidx++;
                if (padidx >= pad.Length) { padidx = 0; }
            }
            // Write all the generated pads at once.
            stream.Write(pds, 0, pds.Length);
            pds = null;
        }

        /// <summary>
        /// Reads an ASCII-encoded string from the stream. 
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="length">The length, in bytes , of the string to read.</param>
        /// <returns>The read string contents.</returns>
        public static System.String ReadASCIIString(this System.IO.Stream stream , System.Int32 length) 
        {
            if (length < 0) { return null; }
            System.Byte[] data = new System.Byte[length];
            System.Int32 rb = stream.Read(data, 0, data.Length);
            fixed (System.Byte* src = data)
            {
                return new((System.SByte*)src, 0, rb);
            }
        }

        /// <summary>
        /// Reads an UTF16-encoded string with little endianess from the stream. 
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="length">The length, in bytes , of the string to be read.</param>
        public static System.String ReadUTF16LEString(this System.IO.Stream stream , System.Int32 length)
        {
            if (length < 0) { return null; }
            System.Byte[] data = new System.Byte[length];
            System.Int32 rb = stream.Read(data, 0, data.Length);
            fixed (System.Byte* src = data)
            {
                return new((System.Char*)src, 0, rb);
            }
        }

        // Code portions of below method belong from BinaryReader from .NET Foundation: 
        // Licensed to the .NET Foundation under one or more agreements.
        // The .NET Foundation licenses this file to you under the MIT license.
        /// <summary>
        /// Reads a fixed-length string from the specified string , with the specified text encoding.
        /// </summary>
        /// <param name="stream">The stream to read the fixed-length string from.</param>
        /// <param name="enc">The encoding under the string was written.</param>
        /// <returns>The decoded string.</returns>
        public static System.String ReadFixedLengthString(this System.IO.Stream stream , System.Text.Encoding enc)
        {
            if (enc is null) { throw new System.ArgumentNullException(nameof(enc)); }
            System.Text.Decoder dec = enc.GetDecoder();

            int currPos = 0;
            int n;
            int stringLength;
            int readLength;
            int charsRead;

            // Length of the string in bytes, not chars
            stringLength = Read7BitEncodedInt(stream);
            if (stringLength < 0) { throw new System.FormatException($"Invalid fixed string length: {stringLength}."); }
            if (stringLength == 0) { return System.String.Empty; }

            System.Byte[] CharBytes = System.Buffers.ArrayPool<System.Byte>.Shared.Rent(MaxCharBytesSize);
            System.Char[] CharBuffer = System.Buffers.ArrayPool<System.Char>.Shared.Rent(enc.GetMaxCharCount(CharBytes.Length));

            System.Text.StringBuilder sb = new(2048); // Give an arbitrary capacity of 2048 characters.

            do {
                readLength = ((stringLength - currPos) > MaxCharBytesSize) ? MaxCharBytesSize : (stringLength - currPos);
                
                n = stream.Read(CharBytes, 0, readLength);
                if (n == 0) { throw new System.IO.EndOfStreamException("The stream was ended prematurely."); }

                charsRead = dec.GetChars(CharBytes, 0, n, CharBuffer, 0);

                if (currPos == 0 && n == stringLength)
                {
                    return new string(CharBuffer, 0, charsRead);
                }

                sb.Append(CharBuffer, 0, charsRead);
                currPos += n;
            } while (currPos < stringLength);
            dec = null;
            System.Buffers.ArrayPool<System.Byte>.Shared.Return(CharBytes);
            System.Buffers.ArrayPool<System.Char>.Shared.Return(CharBuffer);
            CharBuffer = null;
            CharBytes = null;
            return sb.ToString();
        }

        /// <summary>
        /// Writes a signed byte to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the signed byte.</param>
        /// <param name="signedbyte">The signed byte to write.</param>
        public static void WriteSByte(this System.IO.Stream stream, System.SByte signedbyte) 
            => stream.WriteByte(signedbyte.ToByte());

        /// <summary>
        /// Writes a signed short integer to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the signed integer.</param>
        /// <param name="value">The signed short integer to write.</param>
        public static void WriteInt16(this System.IO.Stream stream , System.Int16 value)
        {
            System.Byte[] dt = value.GetBytes();
            stream.Write(dt, 0, dt.Length);
            dt = null;
        }

        /// <summary>
        /// Writes an unsigned short integer to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the unsigned integer.</param>
        /// <param name="value">The unsigned short integer to write.</param>
        public static void WriteUInt16(this System.IO.Stream stream, System.UInt16 value)
        {
            System.Byte[] dt = value.GetBytes();
            stream.Write(dt, 0, dt.Length);
            dt = null;
        }

        /// <summary>
        /// Writes an unsigned integer to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the unsigned integer.</param>
        /// <param name="value">The unsigned integer to write.</param>
        public static void WriteUInt32(this System.IO.Stream stream, System.UInt32 value)
        {
            System.Byte[] dt = value.GetBytes();
            stream.Write(dt, 0, dt.Length);
            dt = null;
        }

        /// <summary>
        /// Writes a signed integer to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the signed integer.</param>
        /// <param name="value">The signed integer to write.</param>
        public static void WriteInt32(this System.IO.Stream stream, System.Int32 value)
        {
            System.Byte[] dt = value.GetBytes();
            stream.Write(dt, 0, dt.Length);
            dt = null;
        }

        /// <summary>
        /// Writes a signed long integer to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the signed integer.</param>
        /// <param name="value">The signed integer to write.</param>
        public static void WriteInt64(this System.IO.Stream stream, System.Int64 value)
        {
            System.Byte[] dt = value.GetBytes();
            stream.Write(dt, 0, dt.Length);
            dt = null;
        }

        /// <summary>
        /// Writes an unsigned long integer to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the unsigned integer.</param>
        /// <param name="value">The unsigned long integer to write.</param>
        public static void WriteUInt64(this System.IO.Stream stream, System.UInt64 value)
        {
            System.Byte[] dt = value.GetBytes();
            stream.Write(dt, 0, dt.Length);
            dt = null;
        }

        /// <summary>
        /// Reads a signed byte from the stream. 
        /// </summary>
        /// <param name="stream">The stream to read the signed byte.</param>
        /// <returns>The read signed byte.</returns>
        public static System.SByte ReadSByte(this System.IO.Stream stream) => stream.ReadByte().ToSByte();

        /// <summary>
        /// Reads a signed short integer from the stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The read signed short integer.</returns>
        public static System.Int16 ReadInt16(this System.IO.Stream stream)
        {
            System.Int32 size = sizeof(System.Int16);
            System.Byte[] data = new System.Byte[size];
            stream.Read(data, 0, size);
            return data.ToInt16(0);
        }

        /// <summary>
        /// Reads a signed integer from the stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The read signed integer.</returns>
        public static System.Int32 ReadInt32(this System.IO.Stream stream)
        {
            System.Int32 size = sizeof(System.Int32);
            System.Byte[] data = new System.Byte[size];
            stream.Read(data, 0, size);
            return data.ToInt32(0);
        }

        /// <summary>
        /// Reads a signed long integer from the stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The read signed long integer.</returns>
        public static System.Int64 ReadInt64(this System.IO.Stream stream)
        {
            System.Int32 size = sizeof(System.Int64);
            System.Byte[] data = new System.Byte[size];
            stream.Read(data, 0, size);
            return data.ToInt64(0);
        }

        /// <summary>
        /// Reads an unsigned short integer from the stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The read unsigned short integer.</returns>
        public static System.UInt16 ReadUInt16(this System.IO.Stream stream)
        {
            System.Int32 size = sizeof(System.UInt16);
            System.Byte[] data = new System.Byte[size];
            stream.Read(data, 0, size);
            return data.ToUInt16(0);
        }

        /// <summary>
        /// Reads an unsigned integer from the stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The read unsigned integer.</returns>
        public static System.UInt32 ReadUInt32(this System.IO.Stream stream)
        {
            System.Int32 size = sizeof(System.UInt32);
            System.Byte[] data = new System.Byte[size];
            stream.Read(data, 0, size);
            return data.ToUInt32(0);
        }

        /// <summary>
        /// Reads an unsigned integer from the stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The read unsigned integer.</returns>
        public static System.UInt64 ReadUInt64(this System.IO.Stream stream)
        {
            System.Int32 size = sizeof(System.UInt64);
            System.Byte[] data = new System.Byte[size];
            stream.Read(data, 0, size);
            return data.ToUInt64(0);
        }

        /// <summary>
        /// Writes a boolean to the stream.
        /// </summary>
        /// <param name="stream">The stream to write.</param>
        /// <param name="value">The boolean value to write.</param>
        public static void WriteBoolean(this System.IO.Stream stream, System.Boolean value) => stream.WriteByte((value ? 1 : 0).ToByte());

        /// <summary>
        /// Reads a boolean from the stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The read boolean value.</returns>
        public static System.Boolean ReadBoolean(this System.IO.Stream stream) => stream.ReadByte() != 0;

        /// <summary>
        /// Writes a decimal to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the decimal.</param>
        /// <param name="dec">The decimal to write.</param>
        public static void WriteDecimal(this System.IO.Stream stream , System.Decimal dec)
        {
            System.Byte[] dt = dec.GetBytes();
            stream.Write(dt, 0, dt.Length);
            dt = null;
        }

        /// <summary>
        /// Reads a decimal from the stream.
        /// </summary>
        /// <param name="stream">The stream to read the decimal from.</param>
        /// <returns>The read decimal value.</returns>
        public static System.Decimal ReadDecimal(this System.IO.Stream stream)
        {
            System.Int32 size = sizeof(System.Decimal);
            System.Byte[] data = new System.Byte[size];
            stream.Read(data, 0, size);
            return data.ToDecimal(0);
        }

        /// <summary>
        /// Writes a double-precision floating-point value to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the number to.</param>
        /// <param name="value">The double-precision floating-point value to write.</param>
        public static void WriteDouble(this System.IO.Stream stream , System.Double value)
        {
            System.Byte[] dt = value.GetBytes();
            stream.Write(dt, 0, dt.Length);
            dt = null;
        }

        /// <summary>
        /// Writes a single-precision floating-point value to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the number to.</param>
        /// <param name="value">The single-precision floating-point value to write.</param>
        public static void WriteSingle(this System.IO.Stream stream , System.Single value)
        {
            System.Byte[] dt = value.GetBytes();
            stream.Write(dt , 0 , dt.Length);
            dt = null;
        }

        /// <summary>
        /// Reads a double-precision floating-point value from the stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <returns>The read double-precision floating-point value.</returns>
        public static System.Double ReadDouble(this System.IO.Stream stream)
        {
            System.Int32 size = sizeof(System.Double);
            System.Byte[] data = new System.Byte[size];
            stream.Read(data, 0, size);
            return data.ToDouble(0);
        }

        /// <summary>
        /// Reads a single-precision floating-point value from the stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <returns>The read single-precision floating-point value.</returns>
        public static System.Single ReadSingle(this System.IO.Stream stream)
        {
            System.Int32 size = sizeof(System.Single);
            System.Byte[] data = new System.Byte[size];
            stream.Read(data, 0, size);
            return data.ToSingle(0);
        }

        /// <summary>
        /// Writes a structure of type <typeparamref name="T"/> to the stream.
        /// </summary>
        /// <typeparam name="T">The type of the structure to write.</typeparam>
        /// <param name="stream">The stream to write the structure.</param>
        /// <param name="structure">The structure to write.</param>
        public static void WriteStructure<T>(this System.IO.Stream stream, T structure) where T : struct 
        {
            System.Byte[] temp = new System.Byte[Unsafe.SizeOf<T>()];
            temp.WriteStructure(0 , structure);
            stream.Write(temp , 0 , temp.Length);
            temp = null;
        }

        /// <summary>
        /// Reads a structure of type <typeparamref name="T"/> from the stream.
        /// </summary>
        /// <typeparam name="T">The type of the structure to read.</typeparam>
        /// <param name="stream">The stream to read the structure from.</param>
        /// <returns>The read structure.</returns>
        /// <exception cref="System.IO.EndOfStreamException">The structure could not be read.</exception>
        public static T ReadStructure<T>(this System.IO.Stream stream) where T : struct
        {
            System.Byte[] temp = new System.Byte[Unsafe.SizeOf<T>()];
            System.Int32 rb = stream.Read(temp , 0 , temp.Length);
            if (rb != temp.Length) { throw new System.IO.EndOfStreamException($"Cannot read the structure of type {typeof(T).FullName}: Expected to read {temp.Length} bytes while read {rb} bytes."); }
            return temp.ReadStructure<T>(0);
        }

        /// <summary>
        /// Reads a 7-bit encoded integer from the stream.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <returns>The given 7-bit encoded integer.</returns>
        /// <exception cref="System.FormatException"></exception>
        /// <exception cref="System.IO.EndOfStreamException"></exception>
        public static System.Int32 Read7BitEncodedInt(this System.IO.Stream reader)
        {
            System.Int32 rb , num = 0 , num2 = 0;
            byte b;
            do {
                if (num2 == 35) {
                    throw new System.FormatException("Too many bytes of what should have been a 7-bit encoded Int32.");
                }
                rb = reader.ReadByte();
                if (rb == -1) { throw new System.IO.EndOfStreamException("The stream ended prematurely."); }
                b = rb.ToByte();
                num |= (b & 0x7F) << num2;
                num2 += 7;
            } while ((b & 0x80u) != 0);
            return num;
        }

        /// <summary>
        /// Writes a 7-bit encoded integer to the specified stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        /// <param name="value">The integer to write.</param>
        public static void Write7BitEncodedInt(this System.IO.Stream writer, int value)
        {
            System.UInt32 num;
            for (num = value.ToUInt32(); num >= 128; num >>= 7)
            {
                writer.WriteByte((byte)(num | 0x80u));
            }
            writer.WriteByte((System.Byte)num);
        }

        /// <summary>
        /// Reads <paramref name="count"/> bytes from the stream.
        /// </summary>
        /// <param name="stream">The stream to read the bytes from.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The read array.</returns>
        public static System.Byte[] ReadBytes(this System.IO.Stream stream , System.Int32 count)
        {
            if (count == 0) { return System.Array.Empty<System.Byte>(); }
            byte[] bytes = new byte[count];
            System.Int32 index = 0 , rb;
            do {
                rb = stream.Read(bytes, index, count);
                if (rb == 0) { break; }

                index += rb;
                count -= rb;
            } while (count > 0);

            if (index != bytes.Length)
            {
                System.Byte[] bret = new System.Byte[index];
                bytes.Copy(0 , bret, 0, index.ToUInt32());
                bytes = bret;
            }

            return bytes;
        }

        /// <summary>
        /// Reads <paramref name="count"/> bytes from the stream.
        /// </summary>
        /// <param name="stream">The stream to read the bytes from.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The read array.</returns>
        public static System.Byte[] ReadBytes(this System.IO.Stream stream , System.Int64 count)
        {
            if (count == 0) { return System.Array.Empty<System.Byte>(); }
            System.Byte[] ret = new System.Byte[count];
            // Read 'count' bytes from the stream. To achieve that , use a second temp buffer which will copy the stream data incrementally to the result buffer.
            // This is done to achieve offset indexes longer than 2147483647.
            System.Byte[] tempbuf = new System.Byte[BUFSIZE];
            // cb variable: Consumed bytes.
            // rbb variable: Factually read bytes. Used as an index in the copy operation.
            System.Int64 cb = count , rbb = 0; 
            System.Int32 rb; // Read bytes from the stream.
            do {
                // The condition specifies that if we have bufferable data , the entire buffer will be used;
                // otherwise , read only the required bytes. Do that in order for the stream's position to
                // only advance by count bytes.
                rb = stream.Read(tempbuf , 0 , (cb >= BUFSIZE) ? tempbuf.Length : cb.ToInt32());
                // No more data to read , exit and return whatever we found.
                if (rb == 0) { break; }

                // Bump the read bytes into the final buffer.
                tempbuf.Copy(0, ret, rbb, rb.ToUInt32());

                // Update index and consumed bytes.
                rbb += rb;
                cb -= rb;
            } while (cb > 0); // Do this until the entire buffer has been fetched.
            tempbuf = null;
            return ret;
        }

        /// <summary>
        /// Writes all the data directly to the stream by writing these sequentially.
        /// </summary>
        /// <param name="stream">The stream to write the bytes to.</param>
        /// <param name="data">The bytes to write to the stream.</param>
        // Copied from ParserHelpers.cs - WriteBuffered method.
        public static void WriteBytes(this System.IO.Stream stream, System.Byte[] data)
        {
            // Abstract: Writes bytes to a stream with a 'buffered' method.
            // Calculate the blocks that will be raw-copied. 
            // Also , calculate the remaining data that will be plainly passed.
            System.Int64 blocks = data.LongLength / BUFSIZE, c = data.LongLength % BUFSIZE;
            System.Int32 pos = 0;
            // Copy all data to the stream
            while (blocks > 0)
            {
                stream.Write(data, pos, BUFSIZE);
                pos += BUFSIZE;
                blocks--;
            }
            // If the input array size is not exactly a multiple of BUFSIZE , the rest data will be copied as-it-is.
            // This even works for cases that data.LongLength < BUFSIZE because the while loop
            // will never be entered.
            if (c > 0) { stream.Write(data, pos, c.ToInt32()); }
        }

        /// <summary>
        /// Directly copies over all the bytes contained from the current to another stream. <br />
        /// Both positions will be repsectively updated.
        /// </summary>
        /// <param name="input">The source stream.</param>
        /// <param name="output">The target stream.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="output"/> parameter was null.</exception>
        public static void DirectCopyToStream(this System.IO.Stream input, System.IO.Stream output)
            => DirectCopyToStream(input, output, BUFSIZE);

        /// <summary>
        /// Directly copies over all the bytes contained from the current to another stream. <br />
        /// Both positions will be repsectively updated.
        /// </summary>
        /// <param name="input">The source stream.</param>
        /// <param name="output">The target stream.</param>
        /// <param name="buffersize">The buffer size in bytes that the method should allocate. This buffer will be used to copy data from the one stream to the another.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="output"/> parameter was null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The <paramref name="buffersize"/> parameter was less than 1024 bytes.</exception>
        public static void DirectCopyToStream(this System.IO.Stream input , System.IO.Stream output , System.Int32 buffersize)
        {
            if (output is null) { throw new System.ArgumentNullException(nameof(output)); }
            if (buffersize < 1024) { throw new System.ArgumentOutOfRangeException(nameof(buffersize) , "The buffersize parameter is too small and could degrade performance."); }
            System.Byte[] buffer = new System.Byte[buffersize];
            System.Int32 readin;
            while ((readin = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, readin);
            }
        }
    }
}