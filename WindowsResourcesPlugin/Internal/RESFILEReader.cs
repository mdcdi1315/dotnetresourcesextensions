using System;
using System.Runtime.CompilerServices;

namespace DotNetResourcesExtensions.Internal
{
    /// <summary>
    /// Reads directly the contents of a .res file. <br />
    /// This allows you to read directly streams of this format! <br />
    /// The following code was created from the doc on <see href="https://learn.microsoft.com/en-us/windows/win32/menurc/resourceheader"/>.
    /// </summary>
    internal unsafe sealed class RESFILEReader : IDisposable
    {
        /*
         Win32 .RES files are generally consisting of concatened resource entries , 
         containing the central header and it's data.

        The structure is defined as follows:
        typedef struct {
          DWORD DataSize;
          DWORD HeaderSize;
          DWORD TYPE;
          DWORD NAME;
          DWORD DataVersion;
          WORD  MemoryFlags;
          WORD  LanguageId;
          DWORD Version;
          DWORD Characteristics;
        } RESOURCEHEADER;

        Be noted that this is not exactly the actual header structure due to string embedabillity;
        See notes on the reading methods.
         */

        private sealed class ResourceHeader
        {
            public System.Object restype;
            public System.Object resname;
            public System.UInt32 datver;
            public System.UInt16 memflags;
            public System.UInt16 langid;
            public System.UInt32 version;
            public System.UInt32 characteristics;
            private System.Int32 cb;
            private System.Text.StringBuilder sharedbuilder;

            // Reads a native resource header. The position pointer must point before the first byte of the HeaderSize header.
            public ResourceHeader(System.IO.Stream stream)
            {
                System.UInt32 ReadDWORD()
                {
                    System.Byte[] temp = new System.Byte[sizeof(System.UInt32)];
                    stream.Read(temp, 0, temp.Length);
                    return Unsafe.ReadUnaligned<System.UInt32>(ref temp[0]);
                }
                sharedbuilder = new(100);
                System.UInt32 size;
                System.Byte[] temp;
                try
                {
                    size = ReadDWORD() - 8;
                    temp = new System.Byte[size]; // The resource header size includes all the members in the structure - omit 8 bytes so.
                }
                catch (System.OverflowException)
                {
                    throw new System.AccessViolationException("ALLOCATION ERROR: Cannot read the header correctly. Maybe the file is corrupted?");
                }
                stream.Read(temp, 0, temp.Length);
                Read(temp);
                temp = null;
                sharedbuilder.Clear();
                sharedbuilder = null;
            }

            public System.Int32 ConsumedBytes => cb;

            private void Read(System.Byte[] data)
            {
                System.Int32 I = 0;
                // Although that the TYPE member is a System.UInt32 we will read a System.UInt16 to determine with what we have to deal with.
                System.Int32 typelen = 0;
                System.UInt16 strornumber = data.ToUInt16(I);
                I += 2;
                if (strornumber == 0xffff) {
                    // The resource type is a number. Let's read it and save it.
                    restype = data.ToUInt16(I);
                    I += 2;
                    typelen = 4;
                } else {
                    // The resource type is a string. 
                    // The end of string is determined by the C's \0 termination sequence.
                    System.Char temp;
                    sharedbuilder.Append(strornumber.ToChar());
                    do {
                        temp = data.ToChar(I);
                        I += 2;
                        if (temp != '\0') { sharedbuilder.Append(temp); }
                    } while (temp != '\0');
                    restype = sharedbuilder.ToString();
                    typelen = (sharedbuilder.Length + 1) * 2;
                    sharedbuilder.Clear(); // Clear the contents of the builder.
                }
                strornumber = data.ToUInt16(I);
                I += 2;
                System.Int32 namelen = 0;
                if (strornumber == 0xffff)
                {
                    // The resource name is a number. Let's read it and save it.
                    resname = data.ToUInt16(I);
                    namelen = 4;
                    I += 2;
                } else {
                    // The resource name is a string. 
                    // The end of string is determined by the C's \0 termination sequence.
                    System.Char temp;
                    sharedbuilder.Append(strornumber.ToChar());
                    do {
                        temp = data.ToChar(I);
                        I += 2;
                        if (temp != '\0') { sharedbuilder.Append(temp); }
                    } while (temp != '\0');
                    resname = sharedbuilder.ToString();
                    namelen = (sharedbuilder.Length + 1) * 2;
                    sharedbuilder.Clear(); // Clear the contents of the builder.
                }
                // MSFT says that there might be needed to add a WORD of padding to align the header.
                // But , it was unclear that the padding must be computed from the TYPE and NAME fields together!
                I += (namelen + typelen) % 4;
                // The rest of data are read sequentially from the array.
                datver = data.ToUInt32(I);
                I += 4;
                memflags = data.ToUInt16(I);
                I += 2;
                langid = data.ToUInt16(I);
                I += 2;
                version = data.ToUInt32(I);
                I += 4;
                characteristics = data.ToUInt32(I);
                cb = I + 4;
                // Finished reading the header.
            }
        }

        private System.IO.Stream stream;
        private System.Int64 finalendoffset , startpos , currentpos;
        private NativeWindowsResourceEntry entrytemp;

        public RESFILEReader(System.IO.Stream strm , System.Int64 ofs)
        {
            stream = System.IO.Stream.Synchronized(strm);
            finalendoffset = ofs;
            startpos = strm.Position;
            currentpos = startpos;
            entrytemp = new();
        }

        private T ReadNumber<T>() where T : struct
        {
            System.Byte[] temp = new System.Byte[Unsafe.SizeOf<T>()];
            stream.Read(temp, 0, temp.Length);
            return Unsafe.ReadUnaligned<T>(ref temp[0]);
        }

        private System.UInt32 ReadDWORD() => ReadNumber<System.UInt32>();

        /// <summary>
        /// Reads the next resource that is available on the stream. <br />
        /// Returns null if no more resource entries do exist.
        /// </summary>
        public NativeWindowsResourceEntry ReadNext()
        {
            stream.Position = currentpos;
            if (stream.Position >= finalendoffset) { return null; }
            entrytemp = new();
            // DataSize implies the resource value length , without any file alignment.
            entrytemp.Value = new System.Byte[ReadDWORD()];
            var hdr = new ResourceHeader(stream);
            try {
                entrytemp.Culture = new(hdr.langid);
            } catch (System.Globalization.CultureNotFoundException) {
                entrytemp.Culture = System.Globalization.CultureInfo.InvariantCulture;
            } catch (System.ArgumentOutOfRangeException) {
                entrytemp.Culture = System.Globalization.CultureInfo.InvariantCulture;
            }
            entrytemp.Name = hdr.resname;
            if (hdr.restype is System.UInt16 typenum) {
                entrytemp.NativeType = (WindowsResourceEntryType)typenum;
            } else {
                entrytemp.NativeTypeString = hdr.restype.ToString();
            }
            stream.Read(entrytemp.Value, 0, entrytemp.Value.Length);
            // Align the stream pointer so that it is on a DWORD boundary based on the resource bytes that were read.
            PadAlignment(4 , entrytemp.Value.Length);
            System.Threading.Interlocked.Exchange(ref currentpos, stream.Position);
            return entrytemp;
        }

        /// <summary>
        /// Rewinds back the reader before the first resource.
        /// </summary>
        public void Reset() => currentpos = startpos;

        // Verifies that always the file alignment will be correct.
        private void PadAlignment(int align , int length)
        {
            if ((align & 1) == 1) { throw new ArgumentOutOfRangeException(nameof(align) ,"The align parameter must always be multiples of 2."); }
            int misalignment = length & (align - 1);
            if (misalignment != 0) { stream.Position += align - misalignment; }
        }

        public void Dispose() { 
            entrytemp = null;
            stream = null;
        }
    }
}
