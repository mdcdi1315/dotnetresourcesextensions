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
    // This does not still work well , but it is expected to be fixed soon.
    internal static unsafe class RLEDecoder
    {
        private const System.Byte Command = 0;
        private const System.Byte EndOfLine = 0;
        private const System.Byte EndOfBitmap = 1;
        private const System.Byte Delta = 2;

        // Provides a decode array builder for RLE bitmaps.
        private unsafe sealed class RLEBuilder
        {
            private System.Int32 index;
            private System.Byte[] result;

            public RLEBuilder()
            {
                index = -1;
                result = new System.Byte[1];
            }

            public RLEBuilder(System.Int32 cap)
            {
                index = 0;
                result = new System.Byte[cap];
            }

            private void Expand(System.Int32 reqindex , System.Boolean iscount = false)
            {
                // Set the required elements multiplied by 3 so that this is not called at all times.
                System.Int32 reqelements = iscount ? (reqindex+1) * 3 : ((reqindex+2) - result.Length) * 3;
                // Do not expand the array if we already have that number of bytes!
                if (reqelements+index <= result.Length || reqelements < 0) { return; }
                System.Byte[] temp = new System.Byte[reqelements + result.Length];
                System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref temp[0], ref result[0], (System.UInt32)result.Length);
                result = temp;
            }

            public void Add(System.Int32 index , System.Byte value)
            {
                if (index < 0) { return; }
                // Expand auto-validates if there is the required index.
                Expand(index, false);
                result[index] = value;
                this.index = index;
            }

            public void Add(System.Byte value) => Add(++index, value);

            public void AddRange(System.Byte[] data , System.Int32 startindex , System.Int32 count)
            {
                if (data is null) { return; }
                if (data.Length == 0 || startindex < 0 || count < 0 || count > data.Length - startindex) { return; }
                // Expand auto-validates if there are these requested elements.
                Expand(count - startindex, true);
                for (System.Int32 I = 0; I < count; I++) { Add(data[I+startindex]); }
            }

            public void SkipLineData(System.Int32 width)
            {
                System.Int32 skip = 0;
                int remainingPixelsInRow = (index+1) % width;
                if (remainingPixelsInRow > 0) { skip = width - remainingPixelsInRow; }
                // Pad it up , actually when the image will be decoded in RGB these pads will be the color index 0.
                Pad(skip);
            }

            public void SkipDeltaData(System.Int32 dx , System.Int32 dy , System.Int32 width)
            {
                System.Int32 delta = (width * dy) + dx;
                Pad(delta);
            }

            public void Pad(System.Int32 padbytes)
            {
                // Ensure capacity
                Expand(padbytes, true);
                for (System.Int32 I = 0; I < padbytes; I++) { Add(0); }
            }

            /// <summary>
            /// Index equal to -1 means that the index value is uninitialized.
            /// Additionally provides the bytes that are truly written into the array if you do +1 to this property.
            /// </summary>
            public System.Int32 Index => index;

            /// <summary>
            /// Gets the internal array capacity.
            /// </summary>
            public System.Int32 Capacity => result.Length;

            /// <summary>
            /// Returns the bytes produced after all the add operations.
            /// Note that this method will return the exact number of bytes but not much more than the required.
            /// </summary>
            public System.Byte[] Bytes
            {
                get {
                    System.UInt32 cidx = (System.UInt32)index;
                    System.Byte[] allocated = new System.Byte[index == -1 ? 1 : cidx+1];
                    System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref allocated[0], ref result[0], cidx+1);
                    return allocated;
                }
            }
        }

        /// <summary>
        /// Decodes the given data and returns the decoded <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The full bitmap data to decode.</param>
        /// <param name="startindex">The starting index inside the <paramref name="input"/> array to start decoding.</param>
        /// <returns>The RGB decoded data.</returns>
        public static System.Byte[] Decode(System.Byte[] input, System.Int32 startindex)
        {
            Interop.BITMAPINFOHEADER bih = Interop.BITMAPINFOHEADER.ReadFromArray(input, startindex);
            System.Byte[] indexes = null;
            if (bih.Compression == Interop.ImageType.BI_RLE4) 
            { indexes = Decode4(input ,bih.Width ,(startindex+bih.Size+bih.ColorTablesSize).ToInt32()); }
            if (bih.Compression == Interop.ImageType.BI_RLE8)
            { indexes = Decode8(input, bih.Width, (startindex + bih.Size + bih.ColorTablesSize).ToInt32()); }
            if (indexes is null) { throw new InvalidOperationException("This method can only decode RLE bitmaps with compression by 4 or by 8."); }
            System.Byte[] colors = new System.Byte[bih.ColorTablesSize];
            System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref colors[0], ref input[startindex + bih.Size] , bih.ColorTablesSize);
            return DecodeRGB(indexes , colors , bih.Width , bih.Height);
        }

        private static System.Byte[] Decode4(System.Byte[] encoded , System.Int32 width , System.Int32 startindex)
        {
            // Let's say the array size * 3 to be sure about the capacity.
            RLEBuilder result = new((encoded.Length - startindex) * 3);
            // Portions of the code used are adapted from the ImageSharp project. The original source file can be found at https://github.com/SixLabors/ImageSharp/blob/main/src/ImageSharp/Formats/Bmp/BmpDecoderCore.cs
            // += 2 because the data are read into pairs.
            for (System.Int32 I = startindex; I < encoded.Length; I += 2)
            {
                if (encoded[I] == Command)
                {
                    switch (encoded[I+1]) 
                    {
                        case EndOfLine:
                            result.SkipLineData(width);
                            break;
                        case EndOfBitmap:
                            goto g_exit;
                        case Delta:
                            System.Int32 dx = encoded[I+2];
                            System.Int32 dy = encoded[I+3];
                            result.SkipDeltaData(dx, dy , width);
                            // Skip two more bytes
                            I += 2;
                            break;
                        default:
                            // If the second byte > 2, we are in 'absolute mode'.
                            // The second byte contains the number of color indexes that follow.
                            int max = encoded[I+1];
                            int bytesToRead = (((uint)max + 1) / 2).ToInt32();

                            int idx = I+2;
                            for (int i = 0; i < max; i++)
                            {
                                byte twoPixels = encoded[idx];
                                if (i % 2 == 0) {
                                    result.Add(((twoPixels >> 4) & 0xF).ToByte());
                                } else {
                                    result.Add((twoPixels & 0xF).ToByte());
                                    idx++;
                                }
                            }

                            // Absolute mode data is aligned to two-byte word-boundary.
                            int padding = bytesToRead & 1;
                            // Skip the pad data.
                            I += padding;
                            // Skip to BytesToRead.
                            I += bytesToRead;
                            break;
                    }
                } else {
                    int max = encoded[I];

                    // The second byte contains two color indexes, one in its high-order 4 bits and one in its low-order 4 bits.
                    byte twoPixels = encoded[I+1];
                    byte rightPixel = (twoPixels & 0xF).ToByte();
                    byte leftPixel = ((twoPixels >> 4) & 0xF).ToByte();

                    for (int idx = 0; idx < max; idx++)
                    {
                        if (idx % 2 == 0) {
                            result.Add(leftPixel);
                        } else {
                            result.Add(rightPixel);
                        }
                    }
                    I += max;
                }
            }
            g_exit:
            return result.Bytes;
        }

        private static System.Byte[] Decode8(System.Byte[] encoded , System.Int32 width , System.Int32 startindex)
        {
            // Let's say the array size * 3 to be sure about the capacity.
            RLEBuilder result = new((encoded.Length - startindex) * 3);
            // Portions of the code used are adapted from the ImageSharp project. The original source file can be found at https://github.com/SixLabors/ImageSharp/blob/main/src/ImageSharp/Formats/Bmp/BmpDecoderCore.cs
            // += 2 because the data are read into pairs.
            for (System.Int32 I = startindex; I < encoded.Length; I += 2)
            {
                if (encoded[I] == Command)
                {
                    switch (encoded[I + 1]) {
                        case EndOfBitmap:
                            goto g_exit;
                        case EndOfLine:
                            result.SkipLineData(width);
                            break;
                        case Delta:
                            System.Int32 dx = encoded[I + 2];
                            System.Int32 dy = encoded[I + 3];
                            result.SkipDeltaData(dx, dy, width);
                            // Skip two more bytes
                            I += 2;
                            break;
                        default:
                            // If the second byte > 2, we are in 'absolute mode'.
                            // Take this number of bytes from the stream as uncompressed data.
                            int length = encoded[I+1];

                            // Copy all the bytes to the target data.
                            result.AddRange(encoded, I + 2, length);

                            // Absolute mode data is aligned to two-byte word-boundary. (Expression length & 1).
                            // Apply padding , update index with the length data.
                            I += (length & 1) + length;
                            break;
                    }
                } else {
                    int max = encoded[I];
                    byte colorIdx = encoded[I+1]; // store the value to avoid the repeated indexer access inside the loop.

                    for (System.Int32 c = 0; c < max; c++) { result.Add(colorIdx); }
                    I += max;
                }
            }
            g_exit:
            return result.Bytes;
        }

        private static System.Byte[] DecodeRGB(System.Byte[] indexes, System.Byte[] colors , System.Int32 width , System.Int32 height)
        {
            // Read RGB data and decode!
            System.Byte[] decoded = new System.Byte[indexes.Length * 3];
            for (System.Int32 I = 0; I < indexes.Length; I++)
            {
                // The color table is defined as a table with 4 bytes that represent each color.
                // Each index read represents a color index.
                System.Int32 ci = indexes[I] * 4 , cd = I * 3;
                // Map decoded colors...
                // The data are decoded as the blue color first , the green and the red repectively.
                decoded[cd] = colors[ci];
                decoded[cd+1] = colors[ci+1];
                decoded[cd+2] = colors[ci+2];
            }
            return decoded;
        }
    }

}
