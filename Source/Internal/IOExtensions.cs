using System.Collections.Generic;
using System.Runtime.InteropServices;
using DotNetResourcesExtensions.Internal;

namespace System.IO
{

	internal static class StreamReaderExtensions
	{
		public static int Read7BitEncodedInt(this BinaryReader reader)
		{
			int num = 0;
			int num2 = 0;
			byte b;
			do
			{
				if (num2 == 35)
				{
					throw new FormatException(
						DotNetResourcesExtensions.Properties.Resources.Format_Bad7BitInt32);
				}
				b = reader.ReadByte();
				num |= (b & 0x7F) << num2;
				num2 += 7;
			} while ((b & 0x80u) != 0);
			return num;
		}

        public static System.Byte[] ReadBase64ChunksValue(this StringableStream reader , System.Int32 expected , System.Int32 alignment , System.Int32 chunks)
        {
            System.String data = System.String.Empty , temp;
            System.Boolean cond = true;
            System.Int32 chwithalignment = 0;
            while (cond && (temp = reader.ReadLine()) is not null) {
                if (temp.StartsWith("chunk")) {
                    if (GetChunkIndex(temp) > chunks) { cond = false; break; }
                    temp = reader.ReadLine();
                    data += temp;
                    if (temp.Length == alignment) { chwithalignment++; }
                    if (reader.ReadLine() != "end chunk") { throw new FormatException("Expected the chunk to be closed but instead it is malformed."); }
                }
                if (temp.Equals("end value")) { cond = false; }
            }
            if (chwithalignment < (chunks - 1)) {
                throw new FormatException($"Corrupted byte array was read. Expected to read at least {chunks-1} chunks but instead only the {chwithalignment} chunks were successfully retrieved.");
            }
            System.Byte[] decoded = data.FromBase64();
            data = null;
            temp = null;
            if (decoded.Length != expected) { throw new FormatException($"Expected to read {expected} bytes but read {decoded.Length} bytes."); }
            return decoded;
        }

        public static System.String ReadExactlyAndConvertToString(this StringableStream reader , System.Int32 rb)
        {
            System.Byte[] bytes = new System.Byte[rb];
            System.Int32 arb = reader.Read(bytes , 0 , rb);
            return reader.Encoding.GetString(bytes , 0 , arb);
        }

        private static System.Int32 GetChunkIndex(System.String data) {
            System.Int32 indxs = data.IndexOf('[') , indxe = data.IndexOf(']');
            if (indxe == -1 || indxs == -1) { return -1; }
            System.Int32 ret = 0 , prg = 1;
            for (System.Int32 I = indxe-1; I > indxs; I--) {
                ret += (data[I].ToInt32() - 48) * prg;
                prg *= 10;
            }
            return ret;
        }
	}

    internal static class StreamWriterExtensions
    {
        const System.Int32 stringlinesize = 512;

        public static void Write7BitEncodedInt(this BinaryWriter writer, int value)
        {
            uint num;
            for (num = (uint)value; num >= 128; num >>= 7)
            {
                writer.Write((byte)(num | 0x80u));
            }
            writer.Write((byte)num);
        }

        public static void WriteBase64ChunksValue(this StringableStream writer, byte[] data)
        {
            System.String base64 = data.ToBase64();
            System.Int32 chunks = base64.Length / stringlinesize;
            System.Int32 lastrem = base64.Length % stringlinesize;
            writer.WriteTabbedStringLine(1, $"alignment = {stringlinesize}");
            writer.WriteTabbedStringLine(1, $"size = {data.LongLength}");
            writer.WriteTabbedStringLine(1, $"chunks = {(lastrem > 0 ? chunks + 1 : chunks)}");
            writer.WriteTabbedStringLine(1, "begin value");
            System.Int32 cgs = 0;
            System.String dt;
            for (System.Int32 I = 1; I <= chunks; I++) {
                dt = base64.Substring(cgs);
                dt = dt.Remove(stringlinesize);
                writer.WriteTabbedStringLine(2, $"chunk[{I}]");
                writer.WriteTabbedStringLine(3, dt);
                writer.WriteTabbedStringLine(2, "end chunk");
                cgs += stringlinesize;
            }
            if (lastrem > 0) {
                dt = base64.Substring(cgs);
                writer.WriteTabbedStringLine(2, $"chunk[{chunks+1}]");
                writer.WriteTabbedStringLine(3, dt);
                writer.WriteTabbedStringLine(2, "end chunk");
            }
            base64 = null;
            dt = null;
            writer.WriteTabbedStringLine(1 , "end value");
        }
    }

    internal sealed class PinnedBufferMemoryStream : UnmanagedMemoryStream
    {
        private readonly byte[] _array;

        private GCHandle _pinningHandle;

        public unsafe PinnedBufferMemoryStream(System.Byte[] array)
        {
            _array = array;
            _pinningHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            int num = array.Length;
            fixed (byte* pointer = &MemoryMarshal.GetReference<System.Byte>(array))
            {
                Initialize(pointer, num, num, FileAccess.Read);
            }
        }

        ~PinnedBufferMemoryStream()
        {
            Dispose(disposing: false);
        }

        protected override void Dispose(bool disposing)
        {
            if (_pinningHandle.IsAllocated)
            {
                _pinningHandle.Free();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Represents a stream wrapper that supports reading and writing strings under a specific encoding.
    /// </summary>
    internal sealed class StringableStream : Stream , DotNetResourcesExtensions.Internal.IStreamOwnerBase
    {
        private System.Boolean strmown;
        private System.IO.Stream _stream;
        private System.Text.Encoding encoding;

        private StringableStream() { encoding = System.Text.Encoding.UTF8; strmown = false; }

        public StringableStream(System.IO.Stream underlying) : this() { _stream = underlying; }

        public StringableStream(System.IO.Stream underlying , System.Text.Encoding encoding) { _stream = underlying; this.encoding = encoding; }

        public System.Text.Encoding Encoding { get => encoding; set => encoding = value; }

        public System.IO.Stream BaseStream => _stream;

        public System.Boolean IsStreamOwner { get => strmown; set => strmown = value; }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override long Length => _stream.Length;

        public override bool CanWrite => _stream.CanWrite;

        public override bool CanTimeout => _stream.CanTimeout;

        public override long Position { get => _stream.Position; set => _stream.Position = value; }

        public override void Flush() => _stream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

        public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

        public override int ReadByte() => _stream.ReadByte();

        public override void WriteByte(byte value) => _stream.WriteByte(value);

        public override void SetLength(long value) => _stream.SetLength(value);

        public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

        public void WriteChar(System.Char ch) 
        {
            System.Byte[] bt = encoding.GetBytes(new System.Char[] { ch });
            Write(bt, 0, bt.Length);
        }

        public void WriteString(System.String data)
        {
            System.Byte[] bt = encoding.GetBytes(data);
            Write(bt, 0, bt.Length);
        }

        public void WriteStringLine(System.String data)
        {
            WriteString(data);
            WriteNewLine();
        }

        public void WriteNewLine() => WriteByte(10);

        public void WriteTabs(System.Byte tabs)
        {
            for (System.Byte I = 1; I <= tabs; I++) { WriteByte(9); }
        }

        public void WriteTabbedString(System.Byte tabs , System.String data)
        {
            WriteTabs(tabs);
            WriteString(data);
        }

        public void WriteTabbedStringLine(System.Byte tabs, System.String data)
        {
            WriteTabs(tabs);
            WriteStringLine(data);
        }

        /// <summary>
        /// Reads the next line of characters in the stream , omitting any tabs that are in the beginning. <br />
        /// Returns null when no more characters can be read (stream end).
        /// </summary>
        public System.String ReadLine()
        {
            if (Position >= Length) { return null; }
            System.Int32 rb;
            System.Boolean cond = true;
            List<System.Byte> byteview = new();
            while (cond && (rb = ReadByte()) > -1)
            {
                switch (rb) 
                {
                    // The newline characters signify end of line.
                    case 10:
                    case 13:
                        cond = false; 
                        break;
                    // Tab or space cases. For tabs or spaces that are in the beginning are omitted.
                    case 9:
                    case 32:
                        if (byteview.Count == 0) { break; }
                        goto default;
                    default:
                        byteview.Add((System.Byte)rb);
                        break;
                }
            }
            // Convert the byte view to a string and return it
            return encoding.GetString(byteview.ToArray());
        }

        /// <summary>
        /// Reads the next line of characters as-it-is , and including even leading tabs or spaces.
        /// </summary>
        /// <returns></returns>
        public System.String ReadLiteralLine()
        {
            if (Position >= Length) { return null; }
            System.Int32 rb;
            System.Boolean cond = true;
            List<System.Byte> byteview = new();
            while (cond && (rb = ReadByte()) > -1)
            {
                switch (rb)
                {
                    // The newline characters signify end of line.
                    case 10:
                    case 13:
                        cond = false;
                        break;
                    default:
                        byteview.Add((System.Byte)rb);
                        break;
                }
            }
            // Convert the byte view to a string and return it
            return encoding.GetString(byteview.ToArray());
        }

        public void SkipToNextLine()
        {
            if (Position >= Length) { return; }
            System.Int32 rb;
            System.Boolean cond = true;
            while (cond && (rb = ReadByte()) > -1)
            {
                switch (rb)
                {
                    case 10:
                    case 13:
                        cond = false;
                        break;
                }
            }
        }

        public void SkipInitialTabsOrSpaces()
        {
            if (Position >= Length) { return; }
            System.Int32 rb;
            System.Boolean cond = true , cond2 = false;
            while (cond && (rb = ReadByte()) > -1) {
                switch (rb) {
                    default:
                    case 10:
                    case 13:
                        cond = false;
                        break;
                    case 9:
                    case 32:
                        cond2 = true;
                        break;
                }
            }
            // We were stopped by EOL or 9 or 32's were ended.
            if ((cond == false) & cond2) { Position--; }
        }

        public System.Boolean ByteByByteEqual(System.String data) => NextBytesEqualTo(encoding.GetBytes(data));

        public System.Boolean LazyByteToByteEqual(System.String data) => LazyNextBytesEqualTo(encoding.GetBytes(data));

        public System.Boolean NextBytesEqualTo(System.Byte[] bytes)
        {
            if (bytes is null) { return false; }
            System.Byte[] temp = new System.Byte[bytes.Length];
            System.Int32 rb = Read(temp , 0 , temp.Length);
            if (rb < temp.Length) { return false; }
            for (System.Int64 I = 0; I < bytes.LongLength; I++) {
                if (temp[I] != bytes[I]) { return false; }
            }
            return true;
        }

        public System.Boolean LazyNextBytesEqualTo(System.Byte[] bytes) 
        {
            if (bytes is null) { return false; }
            AvoidInvalidChars();
            return NextBytesEqualTo(bytes);
        }

        private void AvoidInvalidChars()
        {
            System.Int32 rb;
            System.Boolean cond = true;
            while ((rb = ReadByte()) > -1 && cond) {
                switch (rb) {
                    case -1:
                        return;
                    case 9:
                    case 32:
                        break;
                    default:
                        cond = false;
                        break;
                }
            }
            Position--;
        }

        public override System.String ToString() => $"StringableStream<{_stream.GetType().Name}> {{ Position={Position} , Length={Length} , CanRead={CanRead} , CanWrite={CanWrite} }}";

        protected override void Dispose(bool disposing)
        {
            encoding = null;
            if (strmown) { _stream?.Dispose(); }
            base.Dispose(disposing);
        }

        public override void Close()
        {
            if (strmown) { _stream?.Flush();  _stream?.Close(); }
            base.Close();
        }
    }

}
