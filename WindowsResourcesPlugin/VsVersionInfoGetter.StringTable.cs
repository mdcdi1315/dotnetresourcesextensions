using System;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Represents the .NET structure of native StringTable information.
    /// </summary>
    public sealed class VsVersionInfoStringTable
    {
        private System.String native;
        private System.Int16 langid, codepage; 
        private IDictionary<System.String, System.String> props;

        // Portions of the class code are from the Reference Source of .NET Framework.
        private static class MinimalHexDecoder
        {
            static byte[] char2val = new byte[128]
            {
                    /*    0-15 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /*   16-31 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /*   32-47 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /*   48-63 */ 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /*   64-79 */ 0xFF, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /*   80-95 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /*  96-111 */ 0xFF, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    /* 112-127 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            };

            private static int GetByteCount(int charCount)
            {
                if (charCount < 0)
                    throw new ArgumentOutOfRangeException(nameof(charCount), "charCount must not be negative.");
                if ((charCount % 2) != 0)
                    throw new FormatException("The given binary hexadecimal length is invalid.");
                return charCount / 2;
            }

            [System.Security.SecuritySafeCritical]
            public static unsafe int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
            {
                if (chars is null)
                    throw new ArgumentNullException(nameof(chars));
                if (charIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(charIndex), "charIndex must not be negative.");
                if (charIndex > chars.Length)
                    throw new ArgumentOutOfRangeException(nameof(charIndex), "charIndex must not be larger than the array length.");
                if (charCount < 0)
                    throw new ArgumentOutOfRangeException(nameof(charCount), "charCount must not be negative.");
                if (charCount > chars.Length - charIndex)
                    throw new ArgumentOutOfRangeException(nameof(charCount), "The character count exceeds the character buffer size.");
                if (bytes is null)
                    throw new ArgumentNullException(nameof(bytes));
                if (byteIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(byteIndex), "byteIndex must not be negative.");
                if (byteIndex > bytes.Length)
                    throw new ArgumentOutOfRangeException(nameof(byteIndex), "The byte index must not excceed the array size.");
                int byteCount = GetByteCount(charCount);
                if (byteCount < 0 || byteCount > bytes.Length - byteIndex)
                    throw new ArgumentException("The byte array is too small to fit the resulting data.", nameof(bytes));
                if (charCount > 0)
                {
                    fixed (byte* _char2val = char2val)
                    {
                        fixed (byte* _bytes = &bytes[byteIndex])
                        {
                            fixed (char* _chars = &chars[charIndex])
                            {
                                char* pch = _chars;
                                char* pchMax = _chars + charCount;
                                byte* pb = _bytes;
                                while (pch < pchMax)
                                {
                                    System.Diagnostics.Debug.Assert(pch + 2 <= pchMax, "");
                                    char pch0 = pch[0];
                                    char pch1 = pch[1];
                                    if ((pch0 | pch1) >= 128)
                                        throw new FormatException($"An invalid HEX sequence was found: {new System.String(pch, 0, 2)}");
                                    byte d1 = _char2val[pch0];
                                    byte d2 = _char2val[pch1];
                                    if ((d1 | d2) == 0xFF)
                                        throw new FormatException($"An invalid HEX sequence was found: {new System.String(pch, 0, 2)}");
                                    pb[0] = (byte)((d1 << 4) + d2);
                                    pch += 2;
                                    pb++;
                                }
                            }
                        }
                    }
                }
                return byteCount;
            }
        
            public static System.Byte[] GetBytes(System.String str , System.Int32 index , System.Int32 count)
            {
                System.Byte[] bytes = new System.Byte[GetByteCount(count)];
                GetBytes(str.ToCharArray() , index , count , bytes , 0);
                return bytes;
            }
        }

        /// <summary>
        /// Creates a default instance of <see cref="VsVersionInfoStringTable"/>.
        /// </summary>
        public VsVersionInfoStringTable()
        {
            langid = 0;
            codepage = 0;
            native = null;
            props = new Dictionary<System.String , System.String>();
        }

        internal VsVersionInfoStringTable(System.String lgid) : this()
        {
            native = lgid;
            System.Byte[] temp = MinimalHexDecoder.GetBytes(lgid, 0, 4);
            System.Array.Reverse(temp);
            langid = System.BitConverter.ToInt16(temp, 0);
            temp = MinimalHexDecoder.GetBytes(lgid, 4, 4);
            System.Array.Reverse(temp);
            codepage = System.BitConverter.ToInt16(temp, 0);
            temp = null;
        }

        /// <summary>
        /// Gets the full identification string (header) for this <see cref="VsVersionInfoStringTable"/>.
        /// </summary>
        public System.String NativeIdentifier => native;

        /// <summary>
        /// Gets the encoding defined for the resource values of this <see cref="VsVersionInfoStringTable"/> class.
        /// </summary>
        public System.Text.Encoding Encoding => System.Text.Encoding.GetEncoding(codepage);

        /// <summary>
        /// Gets the langauge that the resource values do represent.
        /// </summary>
        public System.Globalization.CultureInfo Culture => new(langid);

        /// <summary>
        /// Gets all the properties that comprise this string table.
        /// </summary>
        public IDictionary<System.String , System.String> Properties => props;
    }
}
