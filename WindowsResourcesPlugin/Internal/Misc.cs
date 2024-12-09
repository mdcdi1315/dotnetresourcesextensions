using System;

// Contains internal implementations that are only required for specific tasks.

namespace DotNetResourcesExtensions.Internal
{

    // Portions of the class code are from the Reference Source of .NET Framework.
    internal static class MinimalHexDecoder
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

        public static System.Byte[] GetBytes(System.String str, System.Int32 index, System.Int32 count)
        {
            System.Byte[] bytes = new System.Byte[GetByteCount(count)];
            GetBytes(str.ToCharArray(), index, count, bytes, 0);
            return bytes;
        }
    }

    // Provides a way to decode RLE compressed information.
    // The decode algorithm works correctly.
    // A transition to bitmap information was not found , so deprecating the old GDI API.
    internal static unsafe class RLEDecoder
    {
        private const System.Byte Command = 0;
        private const System.Byte EndOfLine = 0;
        private const System.Byte EndOfBitmap = 1;
        private const System.Byte Delta = 2;

        /// <summary>
        /// Decodes the given data and returns the decoded <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The full bitmap data to decode.</param>
        /// <param name="startindex">The starting index inside the <paramref name="input"/> array to start decoding.</param>
        /// <returns>The color-index decoded data.</returns>
        public static System.Byte[] Decode(System.Byte[] input, System.Int32 startindex)
        {
            Interop.BITMAPINFOHEADER bih = Interop.BITMAPINFOHEADER.ReadFromArray(input, startindex);
            System.UInt32[] pixels = DecodeRaw_Internal(bih ,input, startindex);
            System.UInt32[] colors = new System.UInt32[bih.ColorTablesCount];
            fixed (System.UInt32* dest = colors)
            {
                fixed (System.Byte* src = &input[startindex + bih.Size])
                {
                    System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(dest, src, bih.ColorTablesSize);
                }
            }
            switch (bih.BitCount)
            {
                case 4:
                    return Index4(pixels, colors);
                case 8:
                    return Index8(pixels, colors);
                default:
                    throw new NotSupportedException($"Decode operation for {bih.BitCount}-bpp bitmaps is currently not supported.");
            }
        }

        private static System.Byte[] Index4(System.UInt32[] pixels , System.UInt32[] colors)
        {
            // BitCount is 4 , so 2 pixels are saved on each byte.
            System.Byte[] result = new System.Byte[(pixels.Length / 2)];
            System.Int32 I = 0 , J = 0;
            System.Byte build, temp;
            // Read 2 pixels each time.
            for (System.Int32 idx = 0; idx < pixels.Length; idx += 2 , I++)
            {
                // Find first the color index that the pixels are matching to.
                System.Int32 cid1 = -1, cid2 = -1;
                for (System.Int32 K = 0; K < colors.Length; K++)
                {
                    if (pixels[idx] == colors[K]) { cid1 = K; }
                    if (pixels[idx+1] == colors[K]) { cid2 = K; }
                }
                // If no color was found , set them to first color in the table.
                if (cid1 == -1) { cid1 = 0; }
                if (cid2 == -1) { cid2 = 0; }
                // Save the first index on the first 4 bits , the second index on the next 4 bits.
                build = 0;
                temp = cid1.ToByte();
                for (J = 0; J < 8; J++) {
                    if (J == 4) { temp = cid2.ToByte(); }
                    if (temp.GetBit(J % 4)) { build.SetBit(J, true); }
                }
                result[I] = build;
            }
            return result;
        }

        private static System.Byte[] Index8(System.UInt32[] pixels, System.UInt32[] colors) 
        {
            // BitCount is 8 , so 1 pixel is saved on each byte.
            System.Byte[] result = new System.Byte[pixels.Length];
            System.Int32 I = 0;
            for (System.Int32 idx = 0; idx < pixels.Length;idx++ , I++)
            {
                System.Int32 cid = -1;
                for (System.Int32 K = 0; K < colors.Length; K++)
                {
                    if (pixels[idx] == colors[K]) { cid = K; break; }
                }
                if (cid == -1) { cid = 0; }
                result[I] = cid.ToByte();
            }
            return result;
        }

        /// <summary>
        /// Decodes the given data and returns the decoded <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The full bitmap data to decode.</param>
        /// <param name="startindex">The starting index inside the <paramref name="input"/> array to start decoding.</param>
        /// <returns>The ARGB decoded data. These data can be directly marshaled to System.Drawing.Color structures and set them directly to the bitmap.</returns>
        /// <exception cref="InvalidOperationException">The bitmap given is not a RLE4 or RLE8 bitmap.</exception>
        public static System.UInt32[] DecodeRaw(System.Byte[] input , System.Int32 startindex)
        {
            Interop.BITMAPINFOHEADER bih = Interop.BITMAPINFOHEADER.ReadFromArray(input, startindex);
            return DecodeRaw_Internal(bih,input ,startindex);
        }

        private static System.UInt32[] DecodeRaw_Internal(Interop.BITMAPINFOHEADER bih , System.Byte[] input , System.Int32 startindex)
        {
            System.UInt32[] colors = new System.UInt32[bih.ColorTablesCount];
            fixed (System.UInt32* dest = colors)
            {
                fixed (System.Byte* src = &input[startindex + bih.Size])
                {
                    System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(dest, src, bih.ColorTablesSize);
                }
            }
            System.UInt32[] pixels = null;
            try
            {
                System.Int64 sidx = startindex + bih.Size + bih.ColorTablesSize;
                switch (bih.Compression)
                {
                    case Interop.ImageType.BI_RLE8:
                        pixels = Decode8(input, colors, bih.Width, bih.Height, sidx.ToInt32(), bih.ImageSize);
                        break;
                    case Interop.ImageType.BI_RLE4:
                        pixels = Decode4(input, colors, bih.Width, bih.Height, sidx.ToInt32());
                        break;
                    default:
                        throw new InvalidOperationException("This method can only decode RLE bitmaps with compression by 4 or by 8.");
                }
            } catch (InvalidOperationException) {
                throw;
            } catch (System.Exception e) when (e is IndexOutOfRangeException) {
                throw new AggregateException("RLE Bitmap decode failed. The bitmap might be corrupted. Try to use the BitmapReader class instead." , e);
            } finally {
                colors = null;
            }
            return pixels;
        }

        private static System.UInt32[] Decode4(System.Byte[] encoded , System.UInt32[] colors , System.Int32 width , System.Int32 height , System.Int32 startindex)
        {
            // The size of the final array is standard.
            System.UInt32[] result = new System.UInt32[width * height];
            // Portions of the code used are adapted from https://github.com/Extender/BMPDecoder/blob/master/bmp.cpp
            // CPP code was adapted to C# by mdcdi1315 at 2024.
            System.UInt32 Y = (height > 0 ? height - 1 : 0).ToUInt32() , X = 0;
            if (height <= 0) { height *= -1; }
            System.Int32 I = startindex;
            System.Byte nextbyte , secondbyte;
            while (true)
            {
                nextbyte = encoded[I++];
                if (nextbyte > 0)
                {
                    // Encoded mode
                    // First byte: number of pixels
                    // Second byte: indexed colors (2)!
                    secondbyte = encoded[I++];
                    System.UInt32 colorA=0xFF000000|colors[(secondbyte&0xF0)>>4];
                    System.UInt32 colorB=0xFF000000|colors[secondbyte&0xF];
                    for (System.Byte pos=0; pos<nextbyte; pos++)
                    {
                        // pos & 1 has better performance than pos % 2.
                        result[Y * width + X++] = (pos & 1) > 0 ? colorB : colorA;
                    }
                } else
                {
                    secondbyte = encoded[I++];
                    if (secondbyte > 0x2)
                    {
                        // Absolute mode
                        System.Byte pixel = 0;
                        bool second;
                        for (System.Byte pos = 0; pos < secondbyte; pos++)
                        {
                            // pos & 1 has better performance than pos % 2.
                            if (!(second = (pos & 1) > 0)) { pixel = encoded[I++]; }
                            result[Y * width + (X++)] = 0xFF000000 | colors[(second ? (pixel & 0xF) : ((pixel & 0xF0) >> 4))];
                        }
                        I += ((((uint)secondbyte + 1) / 2) & 1).ToInt32(); // Run must be word-aligned.
                    } else {
                        switch (secondbyte)
                        {
                            case EndOfLine:
                                // End of line.
                                if (height > 0) { Y--; } else { Y++; }
                                X = 0;
                                break;
                            case EndOfBitmap:
                                // The bitmap ended , exit.
                                goto g_exit;
                            case Delta:
                                // Reposition X and Y appropriately.
                                X += encoded[I++];
                                Y += encoded[I++];
                                break;
                        }
                    }
                }
            }
        g_exit:
            return result;
        }

        private static System.UInt32[] Decode8(System.Byte[] encoded, System.UInt32[] colors, System.Int32 width, System.Int32 height, System.Int32 startindex , System.Int32 recsize)
        {
            // The size of the final array is standard.
            System.UInt32[] result;
            if (recsize > width * height) { result = new System.UInt32[recsize]; } else { result = new System.UInt32[width * height]; }
            // Portions of the code used are adapted from https://github.com/Extender/BMPDecoder/blob/master/bmp.cpp
            // CPP code was adapted to C# by mdcdi1315 at 2024.
            System.UInt32 Y = (height > 0 ? height - 1 : 0).ToUInt32(), X = 0;
            if (height <= 0) { height *= -1; }
            System.Int32 I = startindex;
            System.Byte nextbyte, secondbyte;
            while (true)
            {
                nextbyte = encoded[I++];
                if (nextbyte > 0) {
                    // Encoded mode
                    // First byte: number of pixels
                    // Second byte: indexed color
                    System.UInt32 color=0xFF000000|colors[encoded[I++]];
                    for (System.Byte pos=0; pos < nextbyte; pos++) { result[Y * width + X++] = color; }
                } else {
                    secondbyte = encoded[I++];
                    if (secondbyte > 0x2)
                    {
                        // Absolute mode
                        for (System.Byte pos=0; pos < secondbyte; pos++) { result[Y * width + (X++)] = 0xFF000000 | colors[encoded[I++]]; }
                        I += ((secondbyte + 1) / 2) % 2; // Run must be word-aligned.
                    } else {
                        switch (secondbyte)
                        {
                            case EndOfLine:
                                // End of line.
                                if (height > 0) { Y--; } else { Y++; }
                                X = 0;
                                break;
                            case EndOfBitmap:
                                // The bitmap ended , exit.
                                goto g_exit;
                            case Delta:
                                // Reposition X and Y appropriately.
                                X += encoded[I++];
                                Y += encoded[I++];
                                break;
                        }
                    }
                }
            }
        g_exit:
            return result;
        }

    }

}
