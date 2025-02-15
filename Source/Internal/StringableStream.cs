
using System;
using System.IO;

namespace DotNetResourcesExtensions.Internal
{
    /// <summary>
    /// Data finding state flags.
    /// </summary>
    [Flags]
    public enum FindDataState : System.Byte
    {
        /// <summary>
        /// When no flags are defined , this is the default operation. <br />
        /// Continue instructs that the loop should continue to the next buffer if that is available; <br />
        /// If the end of stream has been reached , then the method should not return valid results , meaning that the requested data were not found.
        /// </summary>
        Continue = 0x00,
        /// <summary>
        /// When this flag is defined , it means that there were not enough data in the buffer to determine whether the method has found something or not. <br />
        /// If the end of stream has been reached at this state , an exception must be thrown to the caller for corrupted data.
        /// </summary>
        NeedsMoreInformation = 0x02,
        /// <summary>
        /// The method has found everything it requires and can exit safely , returning the results.
        /// </summary>
        Return = 0x04
    }

    /// <summary>
    /// Specialized delegate for finding any nature of data represented by an implementing 
    /// method of this delegate. <br />
    /// This is used for finding text into the data.
    /// </summary>
    /// <param name="index">The loop byte index.</param>
    /// <param name="total">The total bytes that are accessed in this pass.</param>
    /// <param name="step">The step of the loop. <paramref name="index"/> is always updated by this number.</param>
    /// <param name="buffer">The buffer that is being searched for. Using <paramref name="buffer"/>[<paramref name="index"/>] gets the current element.</param>
    /// <returns>A instance of the <see cref="DelegateDataState"/> structure.</returns>
    public delegate DelegateDataState FindDataDelegate(System.Int32 index, System.Int32 total, System.Int32 step , System.Byte[] buffer);

    /// <summary>
    /// Used by the <see cref="StringableStream"/> class to determine the results of a 
    /// found data operation. <br />
    /// Can be overriden so that it can return find-specific information determined by your <see cref="FindDataDelegate"/> implementation.
    /// </summary>
    public class FindDataInformation
    {
        /// <summary>
        /// The starting position that the search had begun from.
        /// </summary>
        public System.Int64 Start;
        /// <summary>
        /// The first byte of the location where the desired data were found.
        /// </summary>
        public System.Int64 FoundIndex;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FindDataInformation()
        {
            Start = 0;
            FoundIndex = 0;
        }
    }

    /// <summary>
    /// Represents the return value of the <see cref="FindDataDelegate"/> delegate.
    /// </summary>
    public struct DelegateDataState
    {
        /// <summary>
        /// The index inside the delegate's buffer to return , if <see cref="DataState"/> has the value 
        /// <see cref="FindDataState.Return"/>.
        /// </summary>
        public System.Int32 BufferIndex;
        /// <summary>
        /// The current reader state.
        /// </summary>
        public FindDataState DataState;

        /// <summary>
        /// Creates a new delegate data snapshot state.
        /// </summary>
        public DelegateDataState()
        {
            BufferIndex = 0;
            DataState = FindDataState.Continue;
        }
    }

    /// <summary>
    /// Represents a stream wrapper that supports reading and writing strings under a specific encoding. <br />
    /// The stream wrapper is also thread-safe: it means that it may be accessed from any thread. <br />
    /// Finally , note that the stream must be seekable in order for everything defined here can work actually.
    /// </summary>
    public sealed class StringableStream : System.IO.Stream, IStreamOwnerBase
    {
        private sealed class CharacterEncodingInfo
        {
            public System.Boolean IsBigEndian;
            public System.Int32 CharacterByteSize;

            public CharacterEncodingInfo(System.Text.Encoding enc)
            {
                System.Byte[] data = enc.GetBytes("\n");
                CharacterByteSize = data.Length;
                if (CharacterByteSize <= 1)
                {
                    IsBigEndian = false;
                }
                else
                {
                    IsBigEndian = data[data.Length - 1] == 10;
                }
                data = null;
            }
        }

        [Flags]
        private enum NewLineFlags : System.Byte
        {
            None = 0,
            CR = 0x01,
            LF = 0x02,
            CRLF = CR | LF,
            CRLFUndetermined = 0x08,
            EndOfUnderlyingStream = 0x16,
        }

        private sealed class NewLineData : FindDataInformation
        {
            public NewLineFlags NType;
        }

        private System.Boolean strmown;
        private System.IO.Stream _stream;
        private System.Text.Encoding encoding;
        private CharacterEncodingInfo encdetails;
        private System.Threading.SemaphoreSlim thracc;

        private StringableStream()
        {
            encoding = System.Text.Encoding.UTF8;
            thracc = new(1);
            strmown = false;
        }

        /// <summary>
        /// Creates a new instance of <see cref="StringableStream"/> class from the specified stream that will be wrapped.
        /// </summary>
        /// <param name="underlying">The stream to wrap.</param>
        public StringableStream(System.IO.Stream underlying) : this() { _stream = underlying; PrepareStream(); }

        /// <summary>
        /// Creates a new instance of <see cref="StringableStream"/> class from the specified stream that will be wrapped.
        /// </summary>
        /// <param name="underlying">The stream object to wrap.</param>
        /// <param name="encoding">The character encoding to use for reading and writing strings.</param>
        public StringableStream(System.IO.Stream underlying, System.Text.Encoding encoding) : this()
        {
            _stream = underlying;
            this.encoding = encoding;
            PrepareStream();
        }

        private void PrepareStream()
        {
            if (_stream is null) { throw new ArgumentNullException("underlying"); }
            if (_stream.CanSeek == false) { throw new NotSupportedException(Properties.Resources.DNTRESEXT_SSM_UNDERLYING_UNSEEKABLE); }
            // The encoding details give useful information and insight on how a character is encoded with this encoding.
            encdetails = new(encoding);
        }

        /// <summary>
        /// Gets or sets the character encoding that some methods will use to read and write lines.
        /// </summary>
        public System.Text.Encoding Encoding
        {
            get => encoding;
            set
            {
                if (value is null) { throw new ArgumentNullException(nameof(value)); }
                encoding = value;
                // The encoding details must be updated!!!
                encdetails = new(encoding);
            }
        }

        /// <summary>
        /// Gets the underlying stream object , the stream that this instance is safely wrapping.
        /// </summary>
        public System.IO.Stream BaseStream => _stream;

        /// <summary>
        /// Gets or sets a value whether the <see cref="StringableStream "/> instance should dispose the underlying stream too when calling 
        /// <see cref="Stream.Dispose()"/>.
        /// </summary>
        public System.Boolean IsStreamOwner
        {
            get => strmown;
            set => strmown = value;
        }

        /// <summary>
        /// Gets a value whether the underlying stream supports reading.
        /// </summary>
        public override bool CanRead => _stream.CanRead;

        /// <summary>
        /// Gets a value whether the underlying stream supports seeking.
        /// </summary>
        public override bool CanSeek => _stream.CanSeek;

        /// <summary>
        /// If the underlying stream supports this property , it gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A <see cref="System.Int64"/> value representing the stream's length in bytes.</returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public override long Length => _stream.Length;

        /// <summary>
        /// Gets a value whether the underlying stream supports writing.
        /// </summary>
        public override bool CanWrite => _stream.CanWrite;

        /// <summary>
        /// Gets a value whether the underlying stream has the ability to time out.
        /// </summary>
        public override bool CanTimeout => _stream.CanTimeout;

        /// <summary>
        /// If the underlying stream supports this property , it gets or sets the position in the stream.
        /// </summary>
        /// <returns>The current position within the underlying stream.</returns>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public override long Position
        {
            get => _stream.Position;  // Reading a stream's position does not pose any thread-safety issue.
            set
            {
                // However, changing position is a thread-unsafe operation
                thracc.Wait();
                try
                {
                    _stream.Position = value;
                }
                finally
                {
                    thracc.Release();
                }
            }
        }

        /// <summary>
        /// If the underlying stream implements this method , clears all it's buffers and causes any buffered data to be written to the underlying stream.
        /// </summary>
        /// <exception cref="System.IO.IOException"></exception>
        public override void Flush() => _stream.Flush();

        /// <summary>
        /// Reads a sequence of bytes from the underlying stream and advances the position within the stream by the number of read bytes.
        /// </summary>
        /// <param name="buffer">The buffer to place the read data into.</param>
        /// <param name="offset">The element index inside the <paramref name="buffer"/> to begin placing data read from the stream.</param>
        /// <param name="count">The number of elements to read from the stream.</param>
        /// <returns>
        /// The actual bytes read from the stream and saved into <paramref name="buffer"/>. <br />
        /// This can be less than the bytes requested if that many bytes are not currently available , 
        /// or zero (0) if the end of stream has been reached.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            thracc.Wait();
            try
            {
                return _stream.Read(buffer, offset, count);
            }
            finally
            {
                thracc.Release();
            }
        }

        /// <summary>
        /// Writes a sequence of bytes to the underlying stream and advances the position within the stream by the number of written bytes.
        /// </summary>
        /// <param name="buffer">The buffer which it's contents will be written to the stream.</param>
        /// <param name="offset">The element index inside the <paramref name="buffer"/> to begin placing data from the buffer to the stream.</param>
        /// <param name="count">The number of elements to write to the stream.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            thracc.Wait();
            try
            {
                _stream.Write(buffer, offset, count);
            }
            finally
            {
                thracc.Release();
            }
        }

        /// <summary>
        /// Reads a byte from the underlying stream and advances the position by one byte. 
        /// Returns -1 if the end of the stream has been reached.
        /// </summary>
        /// <returns>The unsigned byte cast to an <see cref="System.Int32"/> , or -1 if at the end of the underlying stream.</returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public override int ReadByte()
        {
            thracc.Wait();
            try
            {
                return _stream.ReadByte();
            }
            finally
            {
                thracc.Release();
            }
        }

        /// <summary>
        /// Writes a byte to the current position within the underlying stream and advances the position by one byte.
        /// </summary>
        /// <param name="value">The byte to write.</param>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public override void WriteByte(byte value)
        {
            thracc.Wait();
            try
            {
                _stream.WriteByte(value);
            }
            finally { thracc.Release(); }
        }

        /// <summary>
        /// If the underlying stream implements this method , it sets the length of the underlying stream.
        /// </summary>
        /// <param name="value">The new length of the underlying stream.</param>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public override void SetLength(long value)
        {
            thracc.Wait();
            try
            {
                _stream.SetLength(value);
            }
            finally
            {
                thracc.Release();
            }
        }

        /// <summary>
        /// If the underlying stream implements this method, sets the position within the underlying stream.
        /// </summary>
        /// <param name="offset">The offest to seek into the underlying stream.</param>
        /// <param name="origin">The seek origin where the <paramref name="offset"/> will be applied.</param>
        /// <returns>The new position within the underlying stream.</returns>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            thracc.Wait();
            try
            {
                return _stream.Seek(offset, origin);
            }
            finally
            {
                thracc.Release();
            }
        }

        /// <summary>
        /// Writes a single character after encoded with the specified encoding given in <see cref="Encoding"/> property.
        /// </summary>
        /// <param name="ch">The character to write to the stream.</param>
        public void WriteChar(System.Char ch)
        {
            System.Byte[] bt = encoding.GetBytes(new System.Char[] { ch });
            Write(bt, 0, bt.Length);
            bt = null;
        }

        /// <summary>
        /// Writes a string sequence to the stream after encoded with the specified encoding given in <see cref="Encoding"/> property.
        /// </summary>
        /// <param name="data">The string sequence to write.</param>
        public void WriteString(System.String data)
        {
            System.Byte[] bt = encoding.GetBytes(data);
            Write(bt, 0, bt.Length);
            bt = null;
        }

        /// <summary>
        /// Writes a string sequence to the stream after encoded with the specified encoding given in <see cref="Encoding"/> property.
        /// </summary>
        /// <param name="data">The string sequence to write.</param>
        /// <param name="offset">The first character index in <paramref name="data"/> parameter that will be written to the stream.</param>
        /// <param name="count">The number of characters that will be written to the stream.</param>
        public unsafe void WriteString(System.String data, int offset, int count)
        {
            if (data is null) { throw new ArgumentNullException(nameof(data)); }
            if (offset < 0 || offset > data.Length - count) { throw new ArgumentOutOfRangeException(nameof(offset), "The character offset must be into the string's bounds."); }
            if (count < 0) { throw new ArgumentOutOfRangeException(nameof(count), "Invalid value for the number of characters to write."); }
            fixed (System.Char* p = data)
            {
                System.Char* index = p + offset;
                System.Int32 bc = encoding.GetByteCount(index, count);
                System.Byte[] wb = new System.Byte[bc];
                System.Int32 enc;
                fixed (System.Byte* dst = wb)
                {
                    enc = encoding.GetBytes(index, count, dst, bc);
                }
                Write(wb, 0, enc);
                wb = null;
            }
        }

        /// <summary>
        /// Writes a string sequence and a new line sequence to the stream after encoded with the specified encoding given in <see cref="Encoding"/> property.
        /// </summary>
        /// <param name="data">The string sequence to write.</param>
        public void WriteStringLine(System.String data)
        {
            WriteString(data);
            WriteMacNewLine();
        }

        /// <summary>
        /// Writes the Mac line termination sequence to the stream.
        /// </summary>
        public void WriteMacNewLine() => WriteChar('\r');

        /// <summary>
        /// Writes the Unix line termination sequence to the stream.
        /// </summary>
        public void WriteUnixNewLine() => WriteChar('\n');

        /// <summary>
        /// Writes the Windows line termination sequence to the stream.
        /// </summary>
        public void WriteWindowsNewLine() => WriteString("\r\n");

        /// <summary>
        /// Writes the specified number of tabs to the stream.
        /// </summary>
        /// <param name="tabs">The number of tabs to write.</param>
        public void WriteTabs(System.Byte tabs)
        {
            System.Byte[] data = encoding.GetBytes(new System.String('\t', tabs));
            Write(data, 0, data.Length);
            data = null;
        }

        /// <summary>
        /// Writes a string sequence to the stream after writing the specified number of tab characters.
        /// </summary>
        /// <param name="tabs">The number of tab characters to prefix the <paramref name="data"/> string sequence.</param>
        /// <param name="data">The string sequence to write to the stream.</param>
        public void WriteTabbedString(System.Byte tabs, System.String data)
        {
            WriteTabs(tabs);
            WriteString(data);
        }

        /// <summary>
        /// Writes a string sequence to the stream after writing the specified number of tab characters. <br />
        /// Finally , it writes a string line termination sequence at the end.
        /// </summary>
        /// <param name="tabs">The number of tab characters to prefix the <paramref name="data"/> string sequence.</param>
        /// <param name="data">The string sequence to write to the stream.</param>
        public void WriteTabbedStringLine(System.Byte tabs, System.String data)
        {
            WriteTabs(tabs);
            WriteStringLine(data);
        }

        /// <summary>
        /// A common utility which you can use in the current stream to test if an ASCII
        /// character is found in the current encoded buffer , coming from this stream. <br />
        /// Usually used together with the <see cref="FindTextData{T}(FindDataDelegate, int)"/> method.
        /// </summary>
        /// <param name="data">The buffer to be tested.</param>
        /// <param name="index">The data index to test against.</param>
        /// <param name="c">The character to find.</param>
        /// <returns><see langword="true"/> when this character was found; otherwise , <see langword="false"/>.</returns>
        public System.Boolean IsCommonCharacter(System.Byte[] data, System.Int32 index, System.Char c)
        {
            if (encdetails.IsBigEndian) {
                return data[index + encdetails.CharacterByteSize] == c;
            } else {
                return data[index] == c;
            }
        }

        /// <summary>
        /// Reads the next line of characters in the stream , omitting any tabs that are in the beginning. <br />
        /// Returns null when no more characters can be read (stream end).
        /// </summary>
        public System.String ReadLine()
        {
            System.String dt = ReadLiteralLine();
            if (dt is null) { return null; }
            System.Text.StringBuilder sb = new(dt);
            dt = null;
            System.Boolean fd = false;
            for (System.Int32 I = 0; I < sb.Length && fd == false; I++)
            {
                switch (sb[I])
                {
                    case ' ':
                    case '\t':
                        sb.Remove(I, 1); // Remove the character.
                        // re-start the search until all characters are processed.
                        I = -1;
                        break;
                    default:
                        fd = true;
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Reads the next line of characters as-it-is , and including even leading tabs or spaces. <br />
        /// The character reading is based on a fast acquiring method that predicts the results based on: <br />
        /// <list type="bullet">
        ///     <item>The selected character encoding.</item>
        ///     <item>The line's new line sequence. Corrupted files may have CRLF but may return LF and the opposite.</item>
        /// </list>
        /// Additionally note that if correct reading has been performed , the seek pointer <br />
        /// will always point to the first character of the next line. <br />
        /// A further wrinkle of this implementation is that it allows to read plain data <br />
        /// even if moving or messing around with the seek pointer. <br />
        /// Returns null when no more characters can be read (stream end).
        /// </summary>
        public System.String ReadLiteralLine()
        {
            NewLineData end = FindNLExactly();
            if (end is null) { return null; }
            thracc.Wait();
            try {
                // Note the seek pointer has not returned back , so do it now.
                System.Int64 len = end.FoundIndex - end.Start;
                _stream.Seek(-len, SeekOrigin.Current);
                // Now we can read the stream.
                System.Byte[] data = new System.Byte[len];
                System.Int32 rb = _stream.Read(data, 0, data.Length);
                // It is not impossible.
                if (rb == 0) { return null; }
                // The stream will normally read the bytes ,
                // but we must strip out the newline chars.
                // This is pretty much straightforward:
                switch (end.NType)
                {
                    case NewLineFlags.CR:
                    case NewLineFlags.LF:
                        rb -= 1 * encdetails.CharacterByteSize;
                        break;
                    case NewLineFlags.CRLF:
                        rb -= 2 * encdetails.CharacterByteSize;
                        break;
                    case NewLineFlags.EndOfUnderlyingStream:
                        // Special-case the EOF.
                        break;
                }
                end = null;
                System.String str = encoding.GetString(data, 0, rb);
                data = null;
                return str;
            } finally {
                thracc.Release();
            }
        }

        /// <summary>
        /// Finds text data in the current stream. <br />
        /// How the data will be found is determined by the provided delegate. <br />
        /// The seek pointer will stop to the point where the found index was found.
        /// </summary>
        /// <typeparam name="T">The more-specific type of <see cref="FindDataInformation"/> class to return.</typeparam>
        /// <param name="dlg">The delegate to execute in each pass.</param>
        /// <param name="numberofbufchars">The number of characters to fetch during each read pass.</param>
        /// <returns>The found data if any , or in event that the reader state was clean while stream end occured and no data found , <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dlg"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.IO.EndOfStreamException">The stream ended prematurely. The find data are corrupted at this state.</exception>
        public T FindTextData<T>(FindDataDelegate dlg , System.Int32 numberofbufchars) where T : FindDataInformation , new()
        {
            if (dlg is null) { throw new ArgumentNullException(nameof(dlg)); }
            if (numberofbufchars < 3) { throw new ArgumentException(Properties.Resources.DNTRESEXT_SSM_FINDOP_BUFCHARS_INV, nameof(numberofbufchars)); }
            T data = new();
            thracc.Wait();
            try {
                // Save the position where the stream has begun to search.
                data.Start = _stream.Position;
                System.Int32 cbs = encdetails.CharacterByteSize;
                System.Byte[] buf = new System.Byte[numberofbufchars * cbs]; // Fetch the number of characters requested each time;
                System.Int32 rb;
                DelegateDataState FDS = default;
                while ((rb = _stream.Read(buf, 0, buf.Length)) > 0)
                {
                    System.Int32 rem = rb % cbs;
                    for (System.Int32 I = 0; I < rb; I += cbs) {
                        // Call the delegate and get data.
                        FDS = dlg(I, rb, cbs, buf);
                        // If the reader has found something , return it
                        if (FDS.DataState == FindDataState.Return) { break; }
                    }
                    // The bytes fetched might be less than the exact decoded characters , address that with this if statement.
                    if (rem > 0) { _stream.Seek(-rem, SeekOrigin.Current); }
                    // For unknown states , fetch more data.
                    if (FDS.DataState == FindDataState.NeedsMoreInformation) {
                        continue;
                    } else if (FDS.DataState == FindDataState.Return) {
                        // First seek back ...
                        // -rem because rem bytes have already been rewinded.
                        _stream.Seek(-(rb - rem), SeekOrigin.Current);
                        // Then apply the final seek pointer.
                        data.FoundIndex = _stream.Seek(FDS.BufferIndex, SeekOrigin.Current);
                        return data;
                    }
                }
                if (FDS.DataState == FindDataState.NeedsMoreInformation) {
                    throw new System.IO.EndOfStreamException(Properties.Resources.DNTRESEXT_SSM_FINDOP_UEND);
                } else {
                    return null;
                }
            } finally {
                thracc.Release();
            }
        }

        // Finds the next line on the stream based on the selected encoding , 
        // the selected new line sequence , and the stream's current position.
        private NewLineData FindNLExactly()
        {
            NewLineData nld = null;
            NewLineFlags nlf = NewLineFlags.None;
            nld = FindTextData<NewLineData>((System.Int32 I , System.Int32 rb , System.Int32 cbs , System.Byte[] buf) => {
                DelegateDataState dds = new();
                dds.DataState = FindDataState.Continue;
                if (nlf.HasFlag(NewLineFlags.CRLFUndetermined) && IsCommonCharacter(buf, I, '\n'))
                {
                    nlf = NewLineFlags.CRLF;
                    dds.BufferIndex = I + cbs;
                    dds.DataState = FindDataState.Return;
                    return dds;
                } else {
                    nlf = NewLineFlags.None;
                }
                if (IsCommonCharacter(buf, I, '\r'))
                {
                    nlf |= NewLineFlags.CR;
                    if (I + cbs >= rb) { 
                        nlf |= NewLineFlags.CRLFUndetermined; 
                        dds.DataState = FindDataState.NeedsMoreInformation;
                        return dds; 
                    }
                    if (IsCommonCharacter(buf, I + cbs, '\n')) {
                        nlf |= NewLineFlags.CRLF;
                        dds.BufferIndex = I + (cbs * 2);
                        dds.DataState = FindDataState.Return;
                        return dds;
                    } else {
                        nlf |= NewLineFlags.LF;
                        dds.BufferIndex = I + cbs;
                        dds.DataState = FindDataState.Return;
                        return dds;
                    }
                }
                if (IsCommonCharacter(buf, I, '\n'))
                {
                    nlf |= NewLineFlags.LF;
                    dds.BufferIndex = I + cbs;
                    dds.DataState = FindDataState.Return;
                    return dds;
                }
                return dds;
            } , 16);
            if (nld is null && _stream.Position >= _stream.Length)
            {
                nld.FoundIndex = _stream.Position;
                nld.NType = NewLineFlags.EndOfUnderlyingStream;
                return nld;
            }
            nld.NType = nlf;
            return nld;
        }

        /// <summary>
        /// Skips the string data to the next line , without reading any characters.
        /// </summary>
        public void SkipToNextLine()
        {
            if (_stream.Position >= _stream.Length) { return; }
            thracc.Wait();
            try
            {
                FindNLExactly();
            }
            finally
            {
                thracc.Release();
            }
        }

        /// <summary>
        /// Skips any initial tab or space characters found at the beggining of a line.
        /// </summary>
        public void SkipInitialTabsOrSpaces()
        {
            if (_stream.Position >= _stream.Length) { return; }
            thracc.Wait();
            try {
                System.Byte[] chd = new System.Byte[encdetails.CharacterByteSize];
                while (_stream.Read(chd, 0, chd.Length) > 0)
                {
                    if (IsCommonCharacter(chd, 0, ' ')) { continue; }
                    if (IsCommonCharacter(chd, 0, '\t')) { continue; }
                    break;
                }
                chd = null;
                if (_stream.Position < _stream.Length) { _stream.Position--; }
            } finally {
                thracc.Release();
            }
        }

        /// <summary>
        /// Closes the stream , after ensured that all buffered data are flushed.
        /// </summary>
        public sealed override void Close()
        {
            if (strmown) { _stream?.Flush(); _stream?.Close(); }
            base.Close();
        }

        /// <summary>
        /// Finds whether the string given in <paramref name="data"/> parameter , <br />
        /// and the next bytes match the string given.
        /// </summary>
        /// <param name="data">The string to test against.</param>
        /// <returns><see langword="true"/> when the string data are equal to the next bytes; otherwise , <see langword="false"/>.</returns>
        public System.Boolean ByteByByteEqual(System.String data) => NextBytesEqualTo(encoding.GetBytes(data));

        /// <summary>
        /// Finds whether the string given in <paramref name="data"/> parameter ,
        /// and the next bytes match the string given. <br />
        /// The search is performed after any initial tabs or spaces are removed.
        /// </summary>
        /// <param name="data">The string to test against.</param>
        /// <returns><see langword="true"/> when the string data are equal to the next bytes; otherwise , <see langword="false"/>.</returns>
        public System.Boolean LazyByteToByteEqual(System.String data) => LazyNextBytesEqualTo(encoding.GetBytes(data));

        /// <summary>
        /// Finds whether the given array data are equal to the following stream bytes.
        /// </summary>
        /// <param name="bytes">The array to test against.</param>
        /// <returns><see langword="true"/> when the array data are equal to the next bytes; otherwise , <see langword="false"/>.</returns>
        public System.Boolean NextBytesEqualTo(System.Byte[] bytes)
        {
            if (bytes is null) { return false; }
            System.Byte[] temp = new System.Byte[bytes.Length];
            System.Int32 rb = Read(temp, 0, temp.Length);
            if (rb < temp.Length) { return false; }
            for (System.Int64 I = 0; I < bytes.LongLength; I++)
            {
                if (temp[I] != bytes[I]) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Finds whether the given array data are equal to the following stream bytes. <br />
        /// The search is performed after any initial tabs or spaces are removed.
        /// </summary>
        /// <param name="bytes">The array to test against.</param>
        /// <returns><see langword="true"/> when the array data are equal to the next bytes; otherwise , <see langword="false"/>.</returns>
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
            thracc.Wait();
            try
            { // There is no any special usage for the try block here , it is here to just ensure that the semaphore will be released.
                while ((rb = _stream.ReadByte()) > -1 && cond)
                {
                    switch (rb)
                    {
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
            finally
            {
                thracc.Release();
            }
        }

        /// <summary>
        /// Gets a string describing the stream's current state.
        /// </summary>
        public override System.String ToString() => $"StringableStream<{_stream.GetType().Name}> {{ Position={Position} , Length={Length} , CanRead={CanRead} , CanWrite={CanWrite} }}";

        /// <summary></summary>
        protected sealed override void Dispose(bool disposing)
        {
            encoding = null;
            encdetails = null;
            thracc?.Dispose();
            thracc = null;
            if (strmown) { _stream?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}