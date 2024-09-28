using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

internal static class Interop
{
    public static class Libraries
    {
        public const System.String User32 = "user32.dll";

        public const System.String Gdi32 = "gdi32.dll";
    }

    public enum BOOL : System.Int32
    {
        FALSE = 0,
        TRUE = 1,
    }

    [Flags]
    public enum IconCreationFlags : System.UInt32
    {
        /// <summary>
        /// Uses the default color format.
        /// </summary>
        LR_DEFAULTCOLOR = 0x00000000,
        /// <summary>
        /// Uses the width or height specified by the system metric values for cursors or icons, if the cxDesired or cyDesired values are set to zero. 
        /// If this flag is not specified and cxDesired and cyDesired are set to zero, the function uses the actual resource size.
        /// </summary>
        LR_DEFAULTSIZE = 0x00000040,
        /// <summary>
        /// Creates a monochrome icon or cursor.
        /// </summary>
        LR_MONOCHROME = 0x00000001,
        /// <summary>
        /// Shares the icon or cursor handle if the icon or cursor is created multiple times. <br />
        /// If <see cref="LR_SHARED"/> is not set, a second call to <see cref="User32.CreateIconOrCursorExtended"/> for the same resource will create the icon or cursor again and return a different handle. <br />
        /// When you use this flag, the system will destroy the resource when it is no longer needed. <br />
        /// Do not use <see cref="LR_SHARED"/> for icons or cursors that have non-standard sizes, that may change after loading, or that are loaded from a file.
        /// </summary>
        LR_SHARED = 0x00008000
    }

    public enum DIBColorType : System.UInt32
    {
        DIB_RGB_COLORS = 0,
        DIB_PAL_COLORS = 1,
    }

    public enum ImageType : System.UInt16
    {
        BI_RGB = 0,
        BI_RLE8 = 1,
        BI_RLE4 = 2,
        BI_BITFIELDS = 3,
        BI_JPEG = 4,
        BI_PNG = 5
    }

    [Flags]
    public enum ImageCopyFlags : System.UInt32
    {
        /// <summary>
        /// Deletes the original image after creating the copy.
        /// </summary>
        LR_COPYDELETEORG = 0x00000008,
        /// <summary>
        /// Tries to reload an icon or cursor resource from the original resource file rather than simply copying the current image. <br />
        /// This is useful for creating a different-sized copy when the resource file contains multiple sizes of the resource. <br />
        /// Without this flag, CopyImage stretches the original image to the new size. <br />
        /// If this flag is set, CopyImage uses the size in the resource file closest to the desired size. <br />
        /// This will succeed only if hImage was loaded by LoadIcon or LoadCursor, or by LoadImage with the LR_SHARED flag. <br />
        /// </summary>
        LR_COPYFROMRESOURCE = 0x00004000,
        /// <summary>
        /// Returns the original hImage if it satisfies the criteria for the copy—that is, correct dimensions and color depth—in which case the <see cref="LR_COPYDELETEORG"/> flag is ignored. 
        /// If this flag is not specified, a new object is always created.
        /// </summary>
        LR_COPYRETURNORG = 0x00000004,
        /// <summary>
        /// If this is set and a new bitmap is created, the bitmap is created as a DIB section.
        /// Otherwise, the bitmap image is created as a device-dependent bitmap.
        /// This flag is only valid if uType is <see cref="ImageCopyType.IMAGE_BITMAP"/>.
        /// </summary>
        LR_CREATEDIBSECTION = 0x00002000,
        /// <summary>
        /// Uses the default color format.
        /// </summary>
        LR_DEFAULTCOLOR = 0x00000000,
        /// <summary>
        /// Uses the width or height specified by the system metric values for cursors or icons, if the cxDesired or cyDesired values are set to zero.  <br />
        /// If this flag is not specified and cxDesired and cyDesired are set to zero, the function uses the actual resource size. <br />
        /// If the resource contains multiple images, the function uses the size of the first image.
        /// </summary>
        LR_DEFAULTSIZE = 0x00000040,
        /// <summary>
        /// Creates a new monochrome image.
        /// </summary>
        LR_MONOCHROME = 0x00000001
    }

    public enum ImageCopyType : System.UInt32
    {
        IMAGE_BITMAP = 0,
        IMAGE_ICON,
        IMAGE_CURSOR
    }

    public static class User32
    {
        // Note that the version parameter must be greater than or equal to 0x00020000 and less or equal than 0x00030000.
        // For iconorcursor parameter , define TRUE to create icons ,
        // or FALSE to create a cursor instead.
        [DllImport(Libraries.User32, EntryPoint = "CreateIconFromResource", SetLastError = true)]
        public static unsafe extern System.IntPtr CreateIconOrCursor(
            System.Byte* pdata, 
            System.UInt32 size, 
            BOOL iconorcursor, 
            System.UInt32 version = 0x00030000);

        [DllImport(Libraries.User32 , EntryPoint = "CreateIconFromResourceEx" , SetLastError = true)]
        public static unsafe extern System.IntPtr CreateIconOrCursorExtended(
            System.Byte* pdata,
            System.UInt32 size,
            BOOL iconorcursor ,
            System.UInt32 version, 
            System.Int32 cxdesired,
            System.Int32 cydesired,
            IconCreationFlags flags);

        // The below method is not used , but left here if anyone wants it to use in their sourcecode sometime...
        [DllImport(Libraries.User32 , EntryPoint = "CopyIcon" , SetLastError = true)]
        public static extern System.IntPtr CopyIconOrCursorHandle(System.IntPtr original);

        [DllImport(Libraries.User32 , EntryPoint = "CopyImage" , SetLastError = true)]
        public static extern System.IntPtr CopyImage(
            System.IntPtr image , 
            ImageCopyType type ,
            System.Int32 cx,
            System.Int32 cy,
            ImageCopyFlags flags);

        [DllImport(Libraries.User32 , EntryPoint = "DestroyIcon" , SetLastError = true)]
        public static extern BOOL DestroyIcon(System.IntPtr handle);

        [DllImport(Libraries.User32 , EntryPoint = "DestroyCursor" , SetLastError = true)]
        public static extern BOOL DestroyCursor(System.IntPtr handle);

        [DllImport(Libraries.User32 , EntryPoint = "GetDC" , SetLastError = false)]
        public static extern System.IntPtr GetDC(System.IntPtr hwndtarget);

        [DllImport(Libraries.User32 , EntryPoint = "ReleaseDC")]
        public static extern System.Int32 ReleaseDC(System.IntPtr hwnd, System.IntPtr hdc);
    }

    public static class Gdi32
    {
        // Here are two distinct API's that work in the same manner , output same results , but:
        // The CreateBitmap API creates a device-dependent handle , which it does mean that the image could only be directly rendered.
        // The CreateDeviceIndependentBitmap API creates a device-independent handle , which can be handled by the System.Drawing.Bitmap class.
        // Note that WinForms do only support the second API so for .NET developers that operate at WinForms level the second API is preferred.
        // For other .NET developers that work in lower manipulation layer , using the device-dependent API might be seem useful.
        [DllImport(Libraries.Gdi32 , EntryPoint = "CreateBitmap")]
        public static unsafe extern System.IntPtr CreateBitmap(System.Int32 Width , System.Int32 Height , System.UInt32 Planes , System.UInt32 BitCount , System.Byte* pbytes);

        [DllImport(Libraries.Gdi32 , EntryPoint = "DeleteObject" , SetLastError = true , ExactSpelling = true)]
        public static extern BOOL DeleteObject(System.IntPtr handle);

        public static System.Int32 RequiredBitMapBufferSize(BITMAPINFOHEADER header)
            => (((header.Width * header.Planes * header.BitCount + 15) >> 4) << 1) * header.Height;

        public static System.Int32 RequiredBitMapBufferSize(BITMAPHEADER header)
            => (((header.Width * header.Planes * header.BitCount + 15) >> 4) << 1) * header.Height;

        public static unsafe System.Int32 ColorTableEntriesInBytes(BITMAPINFOHEADER header , System.Int32 arraysize)
        {
            System.Int32 size = RequiredBitMapBufferSize(header);
            System.Int32 hdrsize = sizeof(BITMAPINFOHEADER);
            // We know the total size of the header + the data size , so find the color table bytes
            // by using the arraysize parameter.
            return arraysize - (hdrsize + size);
        }

        public static unsafe System.IntPtr LoadBitmap(System.Byte[] raw)
        {
            BITMAPINFOHEADER header = BITMAPINFOHEADER.ReadFromArray(raw, 0);
            fixed (byte* ptr = &raw[raw.Length - RequiredBitMapBufferSize(header)]) 
            {
                return CreateBitmap(header.Width, header.Height, header.Planes, header.BitCount, ptr);
            }
        }

        public static unsafe System.IntPtr LoadDIBitmap(System.Byte[] raw)
        {
            BITMAPHEADER header = BITMAPHEADER.ReadFromArray(raw, 0);
            System.IntPtr dc = User32.GetDC(System.IntPtr.Zero);
            if (dc == IntPtr.Zero) { throw new ArgumentException("Could not inject device context. Creation failed."); }
            System.IntPtr dib;
            System.Int32 error = 0;
            // The 'target' pointer must be NullRef at the beginning.
            // The CreateDeviceIndpendentBitmap will give us then a valid 'target'
            // which can be fed to CopyBlockUnaligned to copy the bitmap data into it.
            fixed (System.Byte* target = &Unsafe.NullRef<System.Byte>())
            {
                dib = CreateDeviceIndependentBitmap(dc, header, DIBColorType.DIB_RGB_COLORS, (void**)&target, System.IntPtr.Zero, 0);
                // Do not allocate any data if is invalid , return handle as-it-is so that native interop can detect that.
                // Otherwise the Unsafe.CopyBlockUnaligned would have indefinitely failed.
                if (dib == IntPtr.Zero) { error = Marshal.GetLastWin32Error(); goto _done; }
                System.Int32 size = RequiredBitMapBufferSize(header);
                fixed (System.Byte* source = &raw[raw.Length - size])
                {
                    Unsafe.CopyBlockUnaligned(target, source, (System.UInt32)size);
                }
            }
         _done:
            // These handles are not 'exactly' released. See the remarks on https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-releasedc.
            _ = User32.ReleaseDC(System.IntPtr.Zero, dc);
            // Throw the native exception here, I cannot save the error on the thread of older frameworks.
            if (error != 0) { throw new System.ComponentModel.Win32Exception(error); }
            return dib;
        }

        [DllImport(Libraries.Gdi32 , EntryPoint = "CreateDIBSection" , SetLastError = true)]
        public static unsafe extern System.IntPtr CreateDeviceIndependentBitmap(
            System.IntPtr hdc , 
            BITMAPHEADER header,
            DIBColorType usage,
            void** target,
            System.IntPtr sectionnotused,
            System.UInt32 sectionsizenotused);

        [DllImport(Libraries.Gdi32 , EntryPoint = "CreateCompatibleBitmap", SetLastError = true)]
        public static extern System.IntPtr CreateDeviceDependentBitmap(System.IntPtr hdc, System.Int32 x, System.Int32 y);

        [DllImport(Libraries.Gdi32 , EntryPoint = "GetDIBits" , SetLastError = false)]
        public static unsafe extern System.Int32 GetBits_DI(System.IntPtr hdc , System.IntPtr bitmap , System.UInt32 firstline , System.UInt32 linecount , void* buffer , ref BITMAPHEADER header , DIBColorType usage);
    }

    // DWORD corresponds to System.UInt32
    // WORD corresponds to System.UInt16
    // LONG corresponds to System.Int32
    // UINT corresponds to System.UInt32

    // Native Bitmap Information header so as to get the bitmap information.
    [StructLayout(LayoutKind.Explicit , Size = 36)]
    public struct BITMAPINFOHEADER
    {
        public static unsafe BITMAPINFOHEADER ReadFromArray(System.Byte[] bytes , System.Int32 startindex)
        {
            if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
            if (sizeof(BITMAPINFOHEADER) > bytes.Length - startindex)
            {
                throw new ArgumentException("There are not enough elements to copy so that the BITMAPINFOHEADER structure can be initialized.");
            }
            BITMAPINFOHEADER result = new();
            fixed (System.Byte* ptr = &Unsafe.AsRef(result.pin))
            {
                fixed (System.Byte* array = &bytes[startindex])
                {
                    Unsafe.CopyBlockUnaligned(ptr, array, (System.UInt32)sizeof(BITMAPINFOHEADER));
                }
            }
            return result;
        }

        [FieldOffset(0)]
        private System.Byte pin;

        [FieldOffset(0)]
        public System.UInt32 Size;

        [FieldOffset(4)]
        public System.Int32 Width;

        [FieldOffset(8)]
        public System.Int32 Height;

        [FieldOffset(12)]
        public System.UInt16 Planes;

        [FieldOffset(14)]
        public System.UInt16 BitCount;

        [FieldOffset(16)]
        public ImageType Compression;

        [FieldOffset(18)]
        public System.UInt16 ImageSize;

        [FieldOffset(20)]
        public System.Int32 HorizontalResolution;

        [FieldOffset(24)]
        public System.Int32 VerticalResolution;

        [FieldOffset(28)]
        public System.UInt32 ColorIndices;

        [FieldOffset(32)]
        public System.UInt32 ImportantIndices;

        /// <summary>
        /// Gets the image size in bytes.
        /// </summary>
        public readonly System.Int32 DataSize
        {
            get {
                if (ImageSize > 0) { return ImageSize; }
                System.Int32 stride = ((((Width * BitCount) + 31) & ~31) >> 3);
                return System.Math.Abs(Height) * stride;
            }
        }

        public unsafe readonly System.Byte[] GetBytes()
        {
            System.Byte[] result = new System.Byte[sizeof(BITMAPINFOHEADER)];
            fixed (byte* dest = result)
            {
                fixed (byte* src = &Unsafe.AsRef(pin))
                {
                    Unsafe.CopyBlockUnaligned(dest, src, (System.UInt32)sizeof(BITMAPINFOHEADER));
                }
            }
            return result;
        }
    }

    // This structure is not used in any native operation , it is just used to create or read typed bitmaps.
    [StructLayout(LayoutKind.Explicit , Size = 14)]
    public unsafe struct BITMAPFILEHEADER
    {
        public static System.Byte[] CreateBitmap(System.Byte[] withoutheader)
        {
            const System.Int32 RGBQUADSIZE = 4; // Size of RGBQUAD structure
            BITMAPINFOHEADER data = BITMAPINFOHEADER.ReadFromArray(withoutheader, 0);
            BITMAPFILEHEADER header = new();
            header.Type = 0x4d42; // Is the 'BM' string in ASCII.
            // The below two fields are set based on this article: https://learn.microsoft.com/en-us/windows/win32/gdi/storing-an-image
            header.Size = (System.UInt32)(sizeof(BITMAPFILEHEADER) + data.Size + (data.ColorIndices * RGBQUADSIZE) + data.ImageSize);
            header.Offset = (System.UInt32)(sizeof(BITMAPFILEHEADER) + data.Size + (data.ColorIndices * RGBQUADSIZE));
            // Set reserved fields to zero (although that setting to them other values would not be problem)
            header.RSVD1 = 0; header.RSVD2 = 0;
            // destroy the data structure we do not need it.
            data = default;
            // Get header bytes so as to write them.
            System.Byte[] headerdat = header.GetBytes();
            // destroy the header structure we do not need it.
            header = default; 
            // Combine the data and return them.
            System.Byte[] result = new System.Byte[headerdat.Length + withoutheader.Length];
            Array.ConstrainedCopy(headerdat, 0, result, 0, headerdat.Length);
            Array.ConstrainedCopy(withoutheader, 0, result, headerdat.Length, withoutheader.Length);
            headerdat = null;
            return result;
        }

        public static BITMAPFILEHEADER ReadFromArray(System.Byte[] bytes , System.Int32 startindex) 
        {
            if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
            if (sizeof(BITMAPFILEHEADER) > bytes.Length - startindex)
            {
                throw new ArgumentException("There are not enough elements to copy so that the BITMAPFILEHEADER structure can be initialized.");
            }
            BITMAPFILEHEADER result = new();
            fixed (System.Byte* ptr = &Unsafe.AsRef(result.pin))
            {
                fixed (System.Byte* array = &bytes[startindex])
                {
                    Unsafe.CopyBlockUnaligned(ptr, array, (System.UInt32)sizeof(BITMAPFILEHEADER));
                }
            }
            return result;
        }

        [FieldOffset(0)]
        private System.Byte pin;

        [FieldOffset(0)]
        public System.UInt16 Type;

        [FieldOffset(2)]
        public System.UInt32 Size;

        [FieldOffset(6)]
        public System.UInt16 RSVD1;

        [FieldOffset(8)]
        public System.UInt16 RSVD2;

        [FieldOffset(10)]
        public System.UInt32 Offset;

        public readonly System.Byte[] GetBytes()
        {
            System.Byte[] result = new System.Byte[sizeof(BITMAPFILEHEADER)];
            fixed (byte* dest = result)
            {
                fixed (byte* src = &Unsafe.AsRef(pin))
                {
                    Unsafe.CopyBlockUnaligned(dest, src , (System.UInt32)sizeof(BITMAPFILEHEADER));
                }
            }
            return result;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 40)]
    public unsafe struct BITMAPHEADER
    {
        public static BITMAPHEADER ReadFromArray(System.Byte[] bytes, System.Int32 startindex)
        {
            if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
            System.UInt32 reqsize = (System.UInt32)sizeof(BITMAPHEADER);
            // There are some cases that bytes do not contain at least 1060 bytes , so possibly the bitmap does not contain 256 tables.
            // A helper function will help to calculate the exact required size to init the structure...
            if (reqsize > bytes.Length - startindex)
            {
                reqsize = (System.UInt32)(sizeof(BITMAPINFOHEADER) + 
                    Gdi32.ColorTableEntriesInBytes(
                        BITMAPINFOHEADER.ReadFromArray(bytes , startindex) , 
                        bytes.Length - startindex));
            }
            // We have the required size , we can initialize BITMAPHEADER without losing information.
            if (reqsize > bytes.Length - startindex)
            {
                throw new ArgumentException("There are not enough elements to copy so that the BITMAPHEADER structure can be initialized.");
            }
            BITMAPHEADER result = new();
            fixed (System.Byte* ptr = &Unsafe.AsRef(result.pin))
            {
                fixed (System.Byte* array = &bytes[startindex])
                {
                    Unsafe.CopyBlockUnaligned(ptr, array, reqsize);
                }
            }
            return result;
        }

        [FieldOffset(0)]
        private System.Byte pin;

        [FieldOffset(0)]
        public System.UInt32 Size;

        [FieldOffset(4)]
        public System.Int32 Width;

        [FieldOffset(8)]
        public System.Int32 Height;

        [FieldOffset(12)]
        public System.UInt16 Planes;

        [FieldOffset(14)]
        public System.UInt16 BitCount;

        [FieldOffset(16)]
        public ImageType Compression;

        [FieldOffset(18)]
        public System.UInt16 ImageSize;

        [FieldOffset(20)]
        public System.Int32 HorizontalResolution;

        [FieldOffset(24)]
        public System.Int32 VerticalResolution;

        [FieldOffset(28)]
        public System.UInt32 ColorIndices;

        [FieldOffset(32)]
        public System.UInt32 ImportantIndices;

        [FieldOffset(36)]
        public fixed System.Byte ColorTable[1024];

        public readonly BITMAPINFOHEADER GetHeader()
        {
            BITMAPINFOHEADER result = new();
            fixed (System.Byte* source = &Unsafe.AsRef(pin))
            {
                Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref result), source, (System.UInt32)sizeof(BITMAPINFOHEADER));
            }
            return result;
        }
    }

    public static System.UInt16 MAKEFOURCC(System.Char ch0, System.Char ch1, System.Char ch2, System.Char ch3)
        => (System.UInt16)((System.UInt16)(System.Byte)ch0 | (((System.UInt16)(System.Byte)ch1) << 8) | (((System.UInt16)(System.Byte)ch2) << 16) | (((System.UInt16)(System.Byte)ch3) << 24));

    // Gets a value whether whether we are from Windows so as to invoke the required API's.
    public static System.Boolean ApisSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
