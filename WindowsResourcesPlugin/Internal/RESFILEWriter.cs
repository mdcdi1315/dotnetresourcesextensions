using System;

namespace DotNetResourcesExtensions.Internal
{
    /// <summary>
    /// Defines a simple writer that can write .RES files in a mean and lean way. <br />
    /// The following code was created from the doc on <see href="https://learn.microsoft.com/en-us/windows/win32/menurc/resourceheader"/>.
    /// </summary>
    internal sealed class RESFILEWriter : IDisposable
    {
        private System.IO.Stream stream;

        public RESFILEWriter(System.IO.Stream stream)
        {
            this.stream = stream;
            // In the .RES files , there is always a blank header of all zeroes.
            // It is excluded when it is getting on a assembly though.
            WriteHeader(0, (System.UInt16)0, (WindowsResourceEntryType)0, 0, 0, 0, 0, 0);
        }

        /*
         A .RES file is a list that contains concatened resource entries.
         The base structure that all resource entries use is:

        typedef struct {
          DWORD DataSize; // Data size without any applied padding.
          DWORD HeaderSize; // Header size with all paddings applied.
          DWORD TYPE; // Type number or string. Will be aligned together with the NAME field.
          DWORD NAME; // resource name or as numeric. Aligned by 4 together with the NAME field.
          DWORD DataVersion; // data version
          WORD  MemoryFlags; // DNU anymore
          WORD  LanguageId; // Usually this expresses a culture but not necessarily.
          DWORD Version; // Version that tools like this class can use to increment.
          DWORD Characteristics; // DNU for tools.
        } RESOURCEHEADER;

        Although that this structure is the full representation , some special cases do exist.
        See on the Write* methods below.
         */

        private void WriteDWORD(System.UInt32 val)
        {
            System.Byte[] dt = val.GetBytes();
            stream.Write(dt, 0, dt.Length);
            dt = null;
        }

        private void WriteWORD(System.UInt16 val) 
        {
            System.Byte[] dt = val.GetBytes();
            stream.Write(dt, 0, dt.Length);
            dt = null;
        }

        private void WriteWCHAR(System.Char ch) 
        {
            System.Byte[] dt = ch.GetBytes();
            stream.Write(dt, 0, dt.Length);
            dt = null;
        }

        // Crafts a header and writes it to the stream.
        private void WriteHeader(
            System.UInt32 datsize, System.Object name, 
            System.Object type,  System.UInt32 datversion , 
            System.UInt16 memflags , System.UInt16 langid , 
            System.UInt32 version , System.UInt32 characteristics)
        {
            System.Int64 posdw = stream.Position;
            System.Int32 hdrsize = (sizeof(System.UInt32) * 5) + (sizeof(System.UInt16) * 2); // Will hold the header size.
            // The padding value is applied after all byte lengths for both name and type fields have been computed.
            System.Int32 typelen = 0;
            if (type is System.String tm2) {
                // If the resource entry type is a string , it is written as LE-UTF16 unicode 
                // and the header size is increased by it's length in bytes plus the \0 character.
                typelen = (tm2.Length + 1) * sizeof(System.Char);
                hdrsize += typelen;
            } else { 
                hdrsize += (typelen = sizeof(System.UInt32));
            }
            System.Int32 namelen = 0;
            if (name is System.String tm1) {
                // If the resource entry name is a string , it is written as LE-UTF16 unicode 
                // and the header size is increased by it's length in bytes plus the \0 character.
                namelen = (tm1.Length + 1) * sizeof(System.Char);
                hdrsize += namelen;
            } else {
                hdrsize += (namelen = sizeof(System.UInt32));
            }
            hdrsize += GetNamePadBytes(namelen + typelen);
            // Ready to write the header.
            WriteDWORD(datsize);
            WriteDWORD(hdrsize.ToUInt32());
            if (type is System.String tdat)
            {
                foreach (System.Char c in tdat) { WriteWCHAR(c); }
                // Write terminating character
                WriteWCHAR('\0');
            } else if (type is WindowsResourceEntryType tnum)
            {
                WriteWORD(0xffff);
                WriteWORD((System.UInt16)tnum);
            }
            if (name is System.String ndat)
            {
                foreach (System.Char c in ndat) { WriteWCHAR(c); }
                // Write terminating character
                WriteWCHAR('\0');
            } else if (name is System.UInt16 nval) {
                WriteWORD(0xffff);
                WriteWORD(nval);
            }
            // Doc was unclear about that the padding must be computed after both the TYPE and NAME field lengths have been computed.
            System.Int32 pd = GetNamePadBytes(namelen + typelen);
            for (System.Int32 I = 1; I <= pd; I++) { stream.WriteByte(0); }
            WriteDWORD(datversion);
            WriteWORD(memflags);
            WriteWORD(langid);
            WriteDWORD(version);
            WriteDWORD(characteristics);
            // validation routine to ensure that the padding will be always correct.
            if ((stream.Position - posdw) != hdrsize) { throw new AggregateException($"Expected {hdrsize} bytes while written {stream.Position - posdw} bytes."); }
        }

        public void Write(NativeWindowsResourceEntry entry)
        {
            WriteHeader(entry.Value.Length.ToUInt32(), 
            entry.Name, 
            entry.NativeType switch { 
                WindowsResourceEntryType.Unknown => entry.NativeTypeString,
                _ => entry.NativeType
            }, 0 , 0 , (entry.Culture.LCID switch { 
                127 => 0,
                _ => entry.Culture.LCID
            }).ToUInt16(), 0 , 0);
            stream.Write(entry.Value , 0 , entry.Value.Length);
            // The resource value , on the other hand , must be padded by 4.
            PadAlignment(4 , entry.Value.Length);
        }

        private static System.Int32 GetNamePadBytes(System.Int32 lengthbytes) => lengthbytes % 4;

        private static System.Int32 GetPadBytes(System.Int32 align , System.Int32 length)
        {
            if ((align & 1) == 1) { throw new ArgumentOutOfRangeException(nameof(align), "The align parameter must always be multiples of 2."); }
            int misalignment = length & (align - 1);
            if (misalignment != 0) { return align - misalignment; }
            return 0;
        }

        // Verifies that always the file alignment will be correct.
        private void PadAlignment(int align, int length)
        {
            System.Int32 pd = GetPadBytes(align, length);
            for (System.Int32 I = 1; I <= pd; I++) { stream.WriteByte(0); }
        }

        public void Dispose() {
            stream?.Dispose();
            stream = null;
        }
    }
}
