
using System;
using System.Collections.Generic;
using DotNetResourcesExtensions.Internal;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Gets version information from the VS_VERSION_INFO format. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class VsVersionInfoGetter
    {
        private const System.String vsverinfohdr = "VS_VERSION_INFO" , 
            stringfileinfohdr = "StringFileInfo" , varfileinfohdr = "VarFileInfo";

        private System.Byte[] data;
        private VsFileInformation info;
        private System.Text.Encoding encoding , valueencoding;
        private System.UInt16 length , vallength;
        private List<VsVersionInfoStringTable> tables;

        /// <summary>
        /// Constructs a new instance of <see cref="VsVersionInfoGetter"/> class.
        /// </summary>
        /// <param name="entry">The entry to read the version information from.</param>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property of <paramref name="entry"/> was not <see cref="WindowsResourceEntryType.RT_VERSION"/>.</exception>
        public VsVersionInfoGetter(NativeWindowsResourceEntry entry)
        {
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            if (entry.NativeType != WindowsResourceEntryType.RT_VERSION)
            {
                throw new ArgumentException("The NativeType of the resource entry must have been set to RT_VERSION.");
            }
            data = entry.Value;
            encoding = new System.Text.UnicodeEncoding(System.BitConverter.IsLittleEndian == false, false);
            valueencoding = null;
            info = new();
            tables = new();
            Decompose();
        }

        private void Decompose()
        {
            /* VS_VERSION_INFO structure is this:
             * (https://learn.microsoft.com/en-us/windows/win32/menurc/vs-versioninfo)
                typedef struct {
                  WORD             wLength;
                  WORD             wValueLength;
                  WORD             wType;
                  WCHAR           szKey;
                  WORD             Padding1;
                  VS_FIXEDFILEINFO Value;
                  WORD             Padding2;
                  WORD             Children;
                } VS_VERSIONINFO;
            WORD stands for System.UInt16.
            WCHAR is a UTF-16 string.
            VS_FIXEDFILEINFO is a recursive structure -- will be parsed in a later stage.
             */
            System.Int32 idx = 0;
            length = data.ToUInt16(idx);
            idx += 2;
            vallength = data.ToUInt16(idx);
            idx += 2;
            System.UInt16 type = data.ToUInt16(idx);
            idx += 2;
            if (type != 0) { throw new FormatException("Currently the parser can only process binary input."); }
            // OK. We know that we must read VS_VERSION_INFO exactly , but the format does not give any length for that - so we will presume that is there and
            // we will read the number of-the-what-expected value.
            System.Int32 bytelen = encoding.GetByteCount(vsverinfohdr);
            // Then decode bytelen bytes and directly test...
            if (encoding.GetString(data , idx , bytelen) != vsverinfohdr)
            {
                throw new FormatException("Could not get the VS_VERSION_INFO key. Possibly this format is corrupted.");
            }
            // Update idx to the bytes read.
            idx += bytelen;
            // Gets the padding that the structure uses. Avoid these and move to the value.
            System.UInt16 pad = data.ToUInt16(idx);
            idx += (pad * 2) + 2;
            // Read the structure 
            info = VsFileInformation.FromPlainBytes(data, idx+2);
            // Test also for the required signature
            if (info.Signature != 0xFEEF04BD) {
                throw new FormatException($"Invalid magic value. Expected 4277077181 and returned {info.Signature} .");
            }
            idx += 52; // 52 the bytes read.
            // Gets the second padding.
            pad = data.ToUInt16(idx);
            // Update again
            idx += (pad * 2) + 2;
            // OK . Now the StringFileInfo structure will be read.
            // This structure contains all the properties of this versioning info structure.
            DecomposeStringFileInfo(idx);
        }

        private void DecomposeStringFileInfo(System.Int32 indx)
        {
            System.Int32 idx = indx;
            // Read the structure length. This includes the sizes of ALL child structures.
            System.UInt16 sfilen = data.ToUInt16(idx);
            // Avoid next 2 because they do not get any value never.
            // And avoid next 2 because they are out of interest.
            idx += 6;
            // OK. The same stuff applied for VS_VERSION_INFO apply here too:
            System.Int32 bytelen = encoding.GetByteCount(stringfileinfohdr);
            if (encoding.GetString(data, idx, bytelen) != stringfileinfohdr)
            {
                throw new FormatException("Could not get the StringFileInfo key. Possibly this format is corrupted.");
            }
            // Update idx to the bytes read.
            idx += bytelen;
            // Apply padding as the format requires
            System.Int32 pad = data.ToUInt16(idx);
            // Update the index value
            idx += (pad * 2) + 2;
            // Next , head to read all the structures that this VS_VERSION_INFO defines.
            while (idx < sfilen + indx)
            {
                (VsVersionInfoStringTable , System.Int32) tuple = DecomposeStringTable(idx);
                idx += tuple.Item2;
                tables.Add(tuple.Item1);
            }
        }

        private (VsVersionInfoStringTable , System.Int32) DecomposeStringTable(System.Int32 indx)
        {
            System.Int32 idx = indx;
            // Read the StringTable length.
            System.UInt16 stllen = data.ToUInt16(idx);
            // Update index appropriately.
            idx += 6;
            // Read the Language Identifier of this string table.
            // It is always standard and does always contain 8 characters (8 * 2 = 16 bytes in total).
            VsVersionInfoStringTable stt = new(encoding.GetString(data, idx, 16));
            // Set the value encoding to a valid one...
            valueencoding = stt.Encoding;
            // Update idx again
            idx += 16;
            // Read pad value.
            System.UInt16 pad = data.ToUInt16(idx);
            // Update and pad index
            idx += (pad * 2) + 2;
            // Temporary key and value strings
            System.String key, value;
            // Read recursively all string structures.
            // A string structure is a normal structure that all the values are used.
            while (idx < stllen + indx) // Now it is very fast too because it does not use any if statements!
            {
                // This length is in bytes already
                System.UInt16 len = data.ToUInt16(idx);
                idx += 2;
                // The size is in characters , define it in bytes.
                System.Int32 vallen = data.ToUInt16(idx) * 2;
                idx += 2;
                // Gets the type of the reasource but there is no need to use it actually.
                // So just skip the type bytes.
                // System.UInt16 type = data.ToUInt16(idx);
                idx += 2;
                // OK. Now we expect to find the key at this point.
                // Detect first the structure padding by using reverse methods.
                // -2 due to the padding member.
                System.Int32 keylen = (len-6) - vallen - 2;
                // Then get the key.
                key = encoding.GetString(data, idx, keylen);
                // Update index.
                idx += keylen;
                // Read the applied padding.
                pad = data.ToUInt16(idx);
                // Apply padding by updating the index.
                idx += (pad * 2) + 2;
                value = valueencoding.GetString(data, idx, vallen);
                // Windows loves to write strings with NULL-termination character sometimes! That's awful.
                stt.Properties.Add(key.TrimEnd('\0'), value.TrimEnd('\0'));
                key = null;
                value = null;
                // Update index.
                idx += vallen;
                // If the structure was not properly aligned in multiples of 4 then add to the index the missing bytes.
                // This is what the problem was about from the beginning!
                // This will work because if the missing byte is one it will skip one byte.
                // If there are two bytes , two bytes will skip.
                // If the structure happens to just align properly , then no addition will actually happen
                // (it is like to perform idx += 0).
                idx += len % 4;
            }
            // Return the table + the new index.
            return (stt, idx);
        }

        /// <summary>
        /// Gets all the string tables that comprise this VS_VERSION_INFO structure.
        /// </summary>
        /// <returns>A new array that contains the string tables read.</returns>
        public VsVersionInfoStringTable[] Tables => tables.ToArray();

        /// <summary>
        /// Gets generic information about the version block that was read.
        /// </summary>
        public VsFileInformation Information => info;
    }
}
