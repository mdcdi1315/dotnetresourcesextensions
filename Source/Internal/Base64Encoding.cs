//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Text;
using System.Security;
using System.Runtime.CompilerServices;

namespace DotNetResourcesExtensions.Internal
{
    // Parts of the below class belong to the reference source of .NET Framework , which is located at referencesource.microsoft.com/en-us/ .
    // Modified by mdcdi1315 at 12/9/2024 for the needs of DotNetResourcesExtensions.
    // This source is originating from the WCF internals.
    // Update for 1.0.4 : Add a specialized method for getting the base64 data from a string but avoiding to allocate new character array for reading the string.
    internal unsafe class Base64Encoding : Encoding
    {
        private static Base64Encoding single;

        /// <summary>
        /// Provides a single instance of the <see cref="Base64Encoding"/> class.
        /// </summary>
        public static Base64Encoding Singleton => single ??= new();

        static readonly byte[] char2val = new byte[128]
        {
            /*    0-15 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            /*   16-31 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            /*   32-47 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,   62, 0xFF, 0xFF, 0xFF,   63,
            /*   48-63 */   52,   53,   54,   55,   56,   57,   58,   59,   60,   61, 0xFF, 0xFF, 0xFF,   64, 0xFF, 0xFF,
            /*   64-79 */ 0xFF,    0,    1,    2,    3,    4,    5,    6,    7,    8,    9,   10,   11,   12,   13,   14,
            /*   80-95 */   15,   16,   17,   18,   19,   20,   21,   22,   23,   24,   25, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            /*  96-111 */ 0xFF,   26,   27,   28,   29,   30,   31,   32,   33,   34,   35,   36,   37,   38,   39,   40,
            /* 112-127 */   41,   42,   43,   44,   45,   46,   47,   48,   49,   50,   51, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        };

        static readonly string val2char = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        static readonly byte[] val2byte = new byte[] 
        { 
            (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G', (byte)'H', (byte)'I', (byte)'J', (byte)'K', (byte)'L', (byte)'M', (byte)'N', (byte)'O', (byte)'P',
            (byte)'Q', (byte)'R', (byte)'S', (byte)'T', (byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y', (byte)'Z', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f',
            (byte)'g', (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p', (byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v',
            (byte)'w', (byte)'x', (byte)'y', (byte)'z', (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'+', (byte)'/'
        };

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
                throw new ArgumentOutOfRangeException(nameof(charCount), "charCount must not be negative.");
            if ((charCount % 4) != 0)
                throw new FormatException($"This base64 length is invalid: {charCount}");
            return charCount / 4 * 3;
        }

        // First two chars of a four char base64 sequence can't be ==, and must be valid
        private static bool IsValidLeadBytes(int v1, int v2, int v3, int v4) => ((v1 | v2) < 64) && ((v3 | v4) != 0xFF);

        // If the third char is = then the fourth char must be =
        private static bool IsValidTailBytes(int v3, int v4) => (v3 == 64 && v4 != 64) == false;

        // Default constructor that is only called by the Singleton property once.
        private Base64Encoding() {}

        [SecuritySafeCritical]
        public override int GetByteCount(char[] chars, int index, int count)
        {
            if (chars is null)
                throw new ArgumentNullException(nameof(chars));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "index must not be negative.");
            if (index > chars.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "The index offset must be less than the array length.");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count must not be negative.");
            if (count > chars.Length - index)
                throw new ArgumentOutOfRangeException(nameof(count), "Computed size excceds the buffer size.");

            if (count == 0)
                return 0;
            if ((count % 4) != 0)
                throw new FormatException($"This base64 length is invalid: {count}");
            fixed (byte* _char2val = char2val)
            {
                fixed (char* _chars = &chars[index])
                {
                    int totalCount = 0;
                    char* pch = _chars;
                    char* pchMax = _chars + count;
                    while (pch < pchMax)
                    {
                        System.Diagnostics.Debug.Assert(pch + 4 <= pchMax, "");
                        char pch0 = pch[0];
                        char pch1 = pch[1];
                        char pch2 = pch[2];
                        char pch3 = pch[3];

                        if ((pch0 | pch1 | pch2 | pch3) >= 128)
                            throw new FormatException($"The given base64 sequence is invalid: {new System.String(pch , 0 , 4)}");

                        // xx765432 xx107654 xx321076 xx543210
                        // 76543210 76543210 76543210
                        int v1 = _char2val[pch0];
                        int v2 = _char2val[pch1];
                        int v3 = _char2val[pch2];
                        int v4 = _char2val[pch3];

                        if (!IsValidLeadBytes(v1, v2, v3, v4) || !IsValidTailBytes(v3, v4))
                            throw new FormatException($"The given base64 sequence is invalid: {new System.String(pch, 0, 4)}");

                        int byteCount = (v4 != 64 ? 3 : (v3 != 64 ? 2 : 1));
                        totalCount += byteCount;
                        pch += 4;
                    }
                    return totalCount;
                }
            }
        }

#if NET7_0_OR_GREATER
        public new virtual int GetByteCount(System.String str , System.Int32 index , System.Int32 count)
#else
        public virtual int GetByteCount(System.String str, System.Int32 index, System.Int32 count)
#endif
        {
            if (str is null) { throw new ArgumentNullException(nameof(str)); }
            if (index < 0) { throw new ArgumentOutOfRangeException(nameof(index)); }
            if (count < 0) { throw new ArgumentOutOfRangeException(nameof(count)); }
            StringPinnable sp = StringPinnable.GetAsPinnable(str);
            if (count > sp.Length - index)
                throw new ArgumentOutOfRangeException(nameof(count), "Computed size excceds the buffer size.");

            if (count == 0)
                return 0;
            if ((count % 4) != 0)
                throw new FormatException($"This base64 length is invalid: {count}");
            fixed (byte* _char2val = char2val)
            {
                fixed (char* _chars = &Unsafe.Add(ref sp.Data, index))
                {
                    int totalCount = 0;
                    char* pch = _chars;
                    char* pchMax = _chars + count;
                    while (pch < pchMax)
                    {
                        System.Diagnostics.Debug.Assert(pch + 4 <= pchMax, "");
                        char pch0 = pch[0];
                        char pch1 = pch[1];
                        char pch2 = pch[2];
                        char pch3 = pch[3];

                        if ((pch0 | pch1 | pch2 | pch3) >= 128)
                            throw new FormatException($"The given base64 sequence is invalid: {new System.String(pch, 0, 4)}");

                        // xx765432 xx107654 xx321076 xx543210
                        // 76543210 76543210 76543210
                        int v1 = _char2val[pch0];
                        int v2 = _char2val[pch1];
                        int v3 = _char2val[pch2];
                        int v4 = _char2val[pch3];

                        if (!IsValidLeadBytes(v1, v2, v3, v4) || !IsValidTailBytes(v3, v4))
                            throw new FormatException($"The given base64 sequence is invalid: {new System.String(pch, 0, 4)}");

                        int byteCount = (v4 != 64 ? 3 : (v3 != 64 ? 2 : 1));
                        totalCount += byteCount;
                        pch += 4;
                    }
                    return totalCount;
                }
            }
        }

        [SecuritySafeCritical]
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (chars is null)
                throw new ArgumentNullException(nameof(chars));

            if (charIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(charIndex), "charIndex must be negative.");
            if (charIndex > chars.Length)
                throw new ArgumentOutOfRangeException(nameof(charIndex), "charIndex must be less than the array length.");

            if (charCount < 0)
                throw new ArgumentOutOfRangeException(nameof(charCount), "charCount must not be negative.");
            if (charCount > chars.Length - charIndex)
                throw new ArgumentOutOfRangeException(nameof(charCount), $"charCount must be less or equal than {chars.Length - charIndex} .");

            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes));
            if (byteIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(byteIndex), "byteIndex must not be negative.");
            if (byteIndex > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(byteIndex), "byteIndex must not excceed the array length.");

            if (charCount == 0)
                return 0;
            if ((charCount % 4) != 0)
                throw new FormatException($"This base64 length is invalid: {charCount}");
            fixed (byte* _char2val = char2val)
            {
                fixed (char* _chars = &chars[charIndex])
                {
                    fixed (byte* _bytes = &bytes[byteIndex])
                    {
                        char* pch = _chars;
                        char* pchMax = _chars + charCount;
                        byte* pb = _bytes;
                        byte* pbMax = _bytes + bytes.Length - byteIndex;
                        while (pch < pchMax)
                        {
                            System.Diagnostics.Debug.Assert(pch + 4 <= pchMax, "");
                            char pch0 = pch[0];
                            char pch1 = pch[1];
                            char pch2 = pch[2];
                            char pch3 = pch[3];

                            if ((pch0 | pch1 | pch2 | pch3) >= 128)
                                throw new FormatException($"Invalid base64 sequence detected: {new System.String(pch , 0 , 4)}");
                            // xx765432 xx107654 xx321076 xx543210
                            // 76543210 76543210 76543210

                            int v1 = _char2val[pch0];
                            int v2 = _char2val[pch1];
                            int v3 = _char2val[pch2];
                            int v4 = _char2val[pch3];

                            if (!IsValidLeadBytes(v1, v2, v3, v4) || !IsValidTailBytes(v3, v4))
                                throw new FormatException($"Invalid base64 sequence detected: {new System.String(pch, 0, 4)}");

                            int byteCount = (v4 != 64 ? 3 : (v3 != 64 ? 2 : 1));
                            if (pb + byteCount > pbMax)
                                throw new ArgumentException("This array is too small to fit all bytes.", "bytes");

                            pb[0] = ((v1 << 2) | ((v2 >> 4) & 0x03)).ToByte();
                            if (byteCount > 1)
                            {
                                pb[1] = ((v2 << 4) | ((v3 >> 2) & 0x0F)).ToByte();
                                if (byteCount > 2)
                                {
                                    pb[2] = ((v3 << 6) | ((v4 >> 0) & 0x3F)).ToByte();
                                }
                            }
                            pb += byteCount;
                            pch += 4;
                        }
                        return (pb - _bytes).ToInt32();
                    }
                }
            }
        }

#if NET7_0_OR_GREATER
        public new System.Byte[] GetBytes(System.String Str , System.Int32 index , System.Int32 count)
#else
        public virtual System.Byte[] GetBytes(System.String Str , System.Int32 index , System.Int32 count)
#endif
        {
            if (Str is null) { throw new System.ArgumentNullException(nameof(Str)); }
            if (index < 0) { throw new System.ArgumentOutOfRangeException(nameof(index)); }
            if (count < 0) { throw new System.ArgumentOutOfRangeException(nameof(count)); }
            System.Int32 bc = GetByteCount(Str , index , count);
            StringPinnable sp = StringPinnable.GetAsPinnable(Str);
            if (count > sp.Length - index)
                throw new ArgumentOutOfRangeException(nameof(count), "Computed size excceds the buffer size.");

            if (count == 0)
                return new System.Byte[0];
            if ((count % 4) != 0)
                throw new FormatException($"This base64 length is invalid: {count}");
            System.Byte[] result = new System.Byte[bc];
            fixed (byte* _char2val = char2val)
            {
                fixed (char* _chars = &Unsafe.Add(ref sp.Data , index)) // Even if 0 is fed , the call will return what is expected.
                {
                    fixed (byte* _bytes = &result[0])
                    {
                        char* pch = _chars;
                        char* pchMax = _chars + count;
                        byte* pb = _bytes;
                        byte* pbMax = _bytes + bc;
                        while (pch < pchMax)
                        {
                            System.Diagnostics.Debug.Assert(pch + 4 <= pchMax, "");
                            char pch0 = pch[0];
                            char pch1 = pch[1];
                            char pch2 = pch[2];
                            char pch3 = pch[3];

                            if ((pch0 | pch1 | pch2 | pch3) >= 128)
                                throw new FormatException($"Invalid base64 sequence detected: {new System.String(pch, 0, 4)}");
                            // xx765432 xx107654 xx321076 xx543210
                            // 76543210 76543210 76543210

                            int v1 = _char2val[pch0];
                            int v2 = _char2val[pch1];
                            int v3 = _char2val[pch2];
                            int v4 = _char2val[pch3];

                            if (!IsValidLeadBytes(v1, v2, v3, v4) || !IsValidTailBytes(v3, v4))
                                throw new FormatException($"Invalid base64 sequence detected: {new System.String(pch, 0, 4)}");

                            int byteCount = (v4 != 64 ? 3 : (v3 != 64 ? 2 : 1));
                            if (pb + byteCount > pbMax)
                                throw new ArgumentException("This array is too small to fit all bytes.", "bytes");

                            pb[0] = ((v1 << 2) | ((v2 >> 4) & 0x03)).ToByte();
                            if (byteCount > 1)
                            {
                                pb[1] = ((v2 << 4) | ((v3 >> 2) & 0x0F)).ToByte();
                                if (byteCount > 2)
                                {
                                    pb[2] = ((v3 << 6) | ((v4 >> 0) & 0x3F)).ToByte();
                                }
                            }
                            pb += byteCount;
                            pch += 4;
                        }
                        return result; // We do care to just take the bytes array back.
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public virtual int GetBytes(byte[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (chars is null)
                throw new ArgumentNullException(nameof(chars));
            if (charIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(charIndex), "charIndex must not be negative.");
            if (charIndex > chars.Length)
                throw new ArgumentOutOfRangeException(nameof(charIndex), "charIndex must not excceed the array length.");

            if (charCount < 0)
                throw new ArgumentOutOfRangeException(nameof(charCount), "charCount must not be negative.");
            if (charCount > chars.Length - charIndex)
                throw new ArgumentOutOfRangeException(nameof(charCount), $"charCount must be less or equal than {chars.Length - charIndex} .");

            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (byteIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(byteIndex), "byteIndex must not be negative.");
            if (byteIndex > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(byteIndex), "byteIndex must not excceed the array length.");

            if (charCount == 0)
                return 0;
            if ((charCount % 4) != 0)
                throw new FormatException($"The given base64 length is invalid: {charCount} .");
            fixed (byte* _char2val = char2val)
            {
                fixed (byte* _chars = &chars[charIndex])
                {
                    fixed (byte* _bytes = &bytes[byteIndex])
                    {
                        byte* pch = _chars;
                        byte* pchMax = _chars + charCount;
                        byte* pb = _bytes;
                        byte* pbMax = _bytes + bytes.Length - byteIndex;
                        while (pch < pchMax)
                        {
                            System.Diagnostics.Debug.Assert(pch + 4 <= pchMax, "");
                            byte pch0 = pch[0];
                            byte pch1 = pch[1];
                            byte pch2 = pch[2];
                            byte pch3 = pch[3];
                            if ((pch0 | pch1 | pch2 | pch3) >= 128)
                                throw new FormatException($"The given base64 sequence is invalid: {new string((sbyte*)pch, 0, 4)}");
                            // xx765432 xx107654 xx321076 xx543210
                            // 76543210 76543210 76543210

                            int v1 = _char2val[pch0];
                            int v2 = _char2val[pch1];
                            int v3 = _char2val[pch2];
                            int v4 = _char2val[pch3];

                            if (!IsValidLeadBytes(v1, v2, v3, v4) || !IsValidTailBytes(v3, v4))
                                throw new FormatException($"The given base64 sequence is invalid: {new string((sbyte*)pch, 0, 4)}");

                            int byteCount = (v4 != 64 ? 3 : (v3 != 64 ? 2 : 1));
                            if (pb + byteCount > pbMax)
                                throw new ArgumentException("The buffer is too small to fit all target data.", nameof(bytes));

                            pb[0] = ((v1 << 2) | ((v2 >> 4) & 0x03)).ToByte();
                            if (byteCount > 1)
                            {
                                pb[1] = ((v2 << 4) | ((v3 >> 2) & 0x0F)).ToByte();
                                if (byteCount > 2)
                                {
                                    pb[2] = ((v3 << 6) | ((v4 >> 0) & 0x3F)).ToByte();
                                }
                            }
                            pb += byteCount;
                            pch += 4;
                        }
                        return (pb - _bytes).ToInt32();
                    }
                }
            }
        }

        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0 || byteCount > int.MaxValue / 4 * 3 - 2)
                throw new ArgumentOutOfRangeException(nameof(byteCount), $"byteCount must be in range from 0 to {int.MaxValue / 4 * 3 - 2} .");
            return ((byteCount + 2) / 3) * 4;
        }

        public override int GetCharCount(byte[] bytes, int index, int count) => GetMaxCharCount(count);

        [SecuritySafeCritical]
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes));
            if (byteIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(byteIndex), "byteIndex must not be negative.");
            if (byteIndex > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(byteIndex), "byteIndex must not excceed the array length.");
            if (byteCount < 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount), "byteCount must not be negative.");
            if (byteCount > bytes.Length - byteIndex)
                throw new ArgumentOutOfRangeException(nameof(byteCount), $"byteCount must be less or equal to {bytes.Length - byteIndex}.");

            int charCount = GetCharCount(bytes, byteIndex, byteCount);
            if (chars is null)
                throw new ArgumentNullException(nameof(chars));
            if (charIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(charIndex), "charIndex must not be negative.");
            if (charIndex > chars.Length)
                throw new ArgumentOutOfRangeException(nameof(charIndex), "charIndex must not excceed the array length.");
            if (charCount < 0 || charCount > chars.Length - charIndex)
                throw new ArgumentException($"charCount must not be negative and less or equal than {chars.Length - charIndex} .", nameof(chars));

            // We've computed exactly how many chars there are and verified that
            // there's enough space in the char buffer, so we can proceed without
            // checking the charCount.

            if (byteCount > 0)
            {
                fixed (char* _val2char = val2char)
                {
                    fixed (byte* _bytes = &bytes[byteIndex])
                    {
                        fixed (char* _chars = &chars[charIndex])
                        {
                            byte* pb = _bytes;
                            byte* pbMax = pb + byteCount - 3;
                            char* pch = _chars;

                            // Convert chunks of 3 bytes to 4 chars
                            while (pb <= pbMax)
                            {
                                // 76543210 76543210 76543210
                                // xx765432 xx107654 xx321076 xx543210

                                // Inspect the code carefully before you change this
                                pch[0] = _val2char[(pb[0] >> 2)];
                                pch[1] = _val2char[((pb[0] & 0x03) << 4) | (pb[1] >> 4)];
                                pch[2] = _val2char[((pb[1] & 0x0F) << 2) | (pb[2] >> 6)];
                                pch[3] = _val2char[pb[2] & 0x3F];

                                pb += 3;
                                pch += 4;
                            }

                            // Handle 1 or 2 trailing bytes
                            if (pb - pbMax == 2)
                            {
                                // 1 trailing byte
                                // 76543210 xxxxxxxx xxxxxxxx
                                // xx765432 xx10xxxx xxxxxxxx xxxxxxxx
                                pch[0] = _val2char[(pb[0] >> 2)];
                                pch[1] = _val2char[((pb[0] & 0x03) << 4)];
                                pch[2] = '=';
                                pch[3] = '=';
                            }
                            else if (pb - pbMax == 1)
                            {
                                // 2 trailing bytes
                                // 76543210 76543210 xxxxxxxx
                                // xx765432 xx107654 xx3210xx xxxxxxxx
                                pch[0] = _val2char[(pb[0] >> 2)];
                                pch[1] = _val2char[((pb[0] & 0x03) << 4) | (pb[1] >> 4)];
                                pch[2] = _val2char[((pb[1] & 0x0F) << 2)];
                                pch[3] = '=';
                            } else {
                                // 0 trailing bytes
                                System.Diagnostics.Debug.Assert(pb - pbMax == 3, "");
                            }
                        }
                    }
                }
            }

            return charCount;
        }

        [SecuritySafeCritical]
        public virtual int GetChars(byte[] bytes, int byteIndex, int byteCount, byte[] chars, int charIndex)
        {
            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes));
            if (byteIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(byteIndex), "byteIndex must not be negative.");
            if (byteIndex > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(byteIndex), "byteIndex must not excceed the array length.");
            if (byteCount < 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount), "byteCount must not be negative.");
            if (byteCount > bytes.Length - byteIndex)
                throw new ArgumentOutOfRangeException(nameof(byteCount), $"byteCount must be less or equal to {bytes.Length - byteIndex} .");

            int charCount = GetCharCount(bytes, byteIndex, byteCount);
            if (chars is null)
                throw new ArgumentNullException(nameof(chars));
            if (charIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(charIndex), "charIndex must not be negative.");
            if (charIndex > chars.Length)
                throw new ArgumentOutOfRangeException(nameof(charIndex), "charIndex must not excceed the array length.");

            if (charCount < 0 || charCount > chars.Length - charIndex)
                throw new ArgumentException("This array is too small to fit all the resulting characters.", nameof(chars));

            // We've computed exactly how many chars there are and verified that
            // there's enough space in the char buffer, so we can proceed without
            // checking the charCount.

            if (byteCount > 0)
            {
                fixed (byte* _val2byte = val2byte)
                {
                    fixed (byte* _bytes = &bytes[byteIndex])
                    {
                        fixed (byte* _chars = &chars[charIndex])
                        {
                            byte* pb = _bytes;
                            byte* pbMax = pb + byteCount - 3;
                            byte* pch = _chars;

                            // Convert chunks of 3 bytes to 4 chars
                            while (pb <= pbMax)
                            {
                                // 76543210 76543210 76543210
                                // xx765432 xx107654 xx321076 xx543210

                                // Inspect the code carefully before you change this
                                pch[0] = _val2byte[(pb[0] >> 2)];
                                pch[1] = _val2byte[((pb[0] & 0x03) << 4) | (pb[1] >> 4)];
                                pch[2] = _val2byte[((pb[1] & 0x0F) << 2) | (pb[2] >> 6)];
                                pch[3] = _val2byte[pb[2] & 0x3F];

                                pb += 3;
                                pch += 4;
                            }

                            // Handle 1 or 2 trailing bytes
                            if (pb - pbMax == 2)
                            {
                                // 1 trailing byte
                                // 76543210 xxxxxxxx xxxxxxxx
                                // xx765432 xx10xxxx xxxxxxxx xxxxxxxx
                                pch[0] = _val2byte[(pb[0] >> 2)];
                                pch[1] = _val2byte[((pb[0] & 0x03) << 4)];
                                pch[2] = (byte)'=';
                                pch[3] = (byte)'=';
                            }
                            else if (pb - pbMax == 1)
                            {
                                // 2 trailing bytes
                                // 76543210 76543210 xxxxxxxx
                                // xx765432 xx107654 xx3210xx xxxxxxxx
                                pch[0] = _val2byte[(pb[0] >> 2)];
                                pch[1] = _val2byte[((pb[0] & 0x03) << 4) | (pb[1] >> 4)];
                                pch[2] = _val2byte[((pb[1] & 0x0F) << 2)];
                                pch[3] = (byte)'=';
                            }
                            else
                            {
                                // 0 trailing bytes
                                System.Diagnostics.Debug.Assert(pb - pbMax == 3, "");
                            }
                        }
                    }
                }
            }

            return charCount;
        }
    }
}
