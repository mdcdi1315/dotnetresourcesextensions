using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using DotNetResourcesExtensions.Internal;

internal static class Interop
{
    public static class Libraries
    {
        public const System.String User32 = "user32.dll";

        public const System.String Gdi32 = "gdi32.dll";

        public const System.String GdiPlus = "gdiplus.dll";
    }

    /// <summary>
    /// Blittable version of Windows BOOL type. It is convenient in situations where
    /// manual marshalling is required, or to avoid overhead of regular bool marshalling.
    /// </summary>
    /// <remarks>
    /// Some Windows APIs return arbitrary integer values although the return type is defined
    /// as BOOL. It is best to never compare BOOL to TRUE. Always use bResult != BOOL.FALSE
    /// or bResult == BOOL.FALSE .
    /// </remarks>
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

    [Flags]
    public enum VirtualKeyFlags : System.UInt16
    {
        /// <summary>
        /// The ALT key must be held down when the accelerator key is pressed.
        /// </summary>
        FALT = 0x10,
        /// <summary>
        /// The CTRL key must be held down when the accelerator key is pressed.
        /// </summary>
        FCONTROL = 0x08,
        /// <summary>
        /// No top-level menu item is highlighted when the accelerator is used.  <br />
        /// If this flag is not specified, a top-level menu item will be highlighted, if possible, when the accelerator is used. <br />
        /// This attribute is obsolete and retained only for backward compatibility with resource files designed for 16-bit Windows.
        /// </summary>
        FNOINVERT = 0x02,
        /// <summary>
        /// The SHIFT key must be held down when the accelerator key is pressed.
        /// </summary>
        FSHIFT = 0x04,
        /// <summary>
        /// The key member specifies a virtual-key code. <br />
        /// If this flag is not specified, key is assumed to specify a character code.
        /// </summary>
        FVIRTKEY = 1,
        /// <summary>
        /// When this flag is specified , it means that it is the last entry in the accelerator table.
        /// </summary>
        LastEntry = 0x80
    }

    public enum GpStatus : System.Int32
    {
        Ok = 0,
        GenericError = 1,
        InvalidParameter = 2,
        OutOfMemory = 3,
        ObjectBusy = 4,
        InsufficientBuffer = 5,
        NotImplemented = 6,
        Win32Error = 7,
        WrongState = 8,
        Aborted = 9,
        FileNotFound = 10,
        ValueOverflow = 11,
        AccessDenied = 12,
        UnknownImageFormat = 13,
        FontFamilyNotFound = 14,
        FontStyleNotFound = 15,
        NotTrueTypeFont = 16,
        UnsupportedGdiplusVersion = 17,
        GdiplusNotInitialized = 18,
        PropertyNotFound = 19,
        PropertyNotSupported = 20,
        ProfileNotFound = 21
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GdipStartupInput
    {
        public int GdiplusVersion;

        public IntPtr DebugEventCallback;

        public bool SuppressBackgroundThread;

        public bool SuppressExternalCodecs;

        public static GdipStartupInput GetDefault()
        {
            GdipStartupInput result = default;
            result.GdiplusVersion = 1;
            result.SuppressBackgroundThread = false;
            result.SuppressExternalCodecs = false;
            return result;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GdipStartupOutput
    {
        public IntPtr hook;

        public IntPtr unhook;
    }

    [System.Security.SuppressUnmanagedCodeSecurity]
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

        [DllImport(Libraries.User32, EntryPoint = "CreateAcceleratorTableA", SetLastError = true)]
        public static unsafe extern System.IntPtr CreateAcceleratorTable_Native(void* table, System.Int32 count);

        public static unsafe System.IntPtr CreateAcceleratorTable(ACCEL[] data)
        {
            void* ptr = ACCEL.CreateUnmanagedArray(data);
            try {
                return CreateAcceleratorTable_Native(ptr, data.Length);
            } finally {
                Marshal.FreeHGlobal(new IntPtr(ptr));
            }
        }

        [DllImport(Libraries.User32, EntryPoint = "DestroyAcceleratorTable")]
        public static extern BOOL DestroyAcceleratorTable(System.IntPtr acceltable);
    }

    [System.Security.SuppressUnmanagedCodeSecurity]
    public static class Gdi32
    {
        // Here are two distinct API's that work in the same manner , output same results , but:
        // The CreateBitmap API creates a device-dependent handle , which it does mean that the image could only be directly rendered.
        // The CreateDeviceIndependentBitmap API creates a device-independent handle , which can be handled by the System.Drawing.Bitmap class.
        // Note that WinForms do only support the second API so for .NET developers that operate at WinForms level the second API is preferred.
        // For other .NET developers that work in lower manipulation layer , using the device-dependent API might be seem useful.
        [DllImport(Libraries.Gdi32 , EntryPoint = "CreateBitmap" ,SetLastError = true)]
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

        public static unsafe System.IntPtr LoadBitmap(System.Byte[] raw,  System.Int32 startindex)
        {
            BITMAPINFOHEADER header = BITMAPINFOHEADER.ReadFromArray(raw, startindex);
            System.UInt32 ctsize = header.ColorTablesSize;
            System.Int32 index = (ctsize > 0 ? (System.Int32)(header.Size + ctsize) : raw.Length - RequiredBitMapBufferSize(header)) + startindex;
            fixed (byte* ptr = &raw[index])
            {
                System.IntPtr hbitmap = CreateBitmap(header.Width, header.Height, header.Planes, header.BitCount, ptr);
                if (hbitmap == IntPtr.Zero) { throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()); }
                return hbitmap;
            }
        }

        public static unsafe System.IntPtr LoadDIBitmap(System.Byte[] raw , System.Int32 startindex)
        {
            BITMAPHEADER header = BITMAPHEADER.ReadFromArray(raw, startindex);
            System.Boolean wasrle = false;
            if (header.Compression == ImageType.BI_RLE8 || header.Compression == ImageType.BI_RLE4)
            {
                System.Int32 hdrsize = (header.Size + header.GetHeader().ColorTablesSize).ToInt32();
                System.Byte[] dec = RLEDecoder.Decode(raw, startindex);
                wasrle = true;
                raw = dec;
                dec = null;
                header.Compression = ImageType.BI_RGB;
                header.ImageSize = 0;
                // Directly set the decoded data to the raw array , then special case the native call.
                // Header does not need reconstruction since it remained the same.
                // We are ready for the native call!
            }
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
                // Otherwise the Unsafe.CopyBlockUnaligned call would have indefinitely failed.
                if (dib == IntPtr.Zero) { error = Marshal.GetLastWin32Error(); goto _done; }
                if (wasrle) {
                    fixed (System.Byte* source = raw)
                    {
                        // Just freely use the raw.Length property .. no problem.
                        Unsafe.CopyBlockUnaligned(target, source, raw.Length.ToUInt32());
                    }
                } else {
                    // If we can have color table size , it will be used;
                    // Otherwise , use the old method as a valid fallback.
                    System.UInt32 cssize = header.GetHeader().ColorTablesSize;
                    // Do not call RequiredBitMapBufferSize unless required.
                    System.Int32 bsize1 = cssize <= 0 ? RequiredBitMapBufferSize(header) : 0;
                    System.Int32 index = (cssize > 0 ? (System.Int32)(header.Size + cssize) : raw.Length - bsize1) + startindex;
                    System.UInt32 bsize2 = (cssize > 0 ? raw.Length - index : bsize1).ToUInt32();
                    fixed (System.Byte* source = &raw[index])
                    {
                        Unsafe.CopyBlockUnaligned(target, source, bsize2);
                    }
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

    [System.Security.SuppressUnmanagedCodeSecurity]
    public static class GdiPlus
    {
        [System.Diagnostics.DebuggerHidden]
        public static void StatusToExceptionMarshaller(GpStatus status)
        {
            switch (status)
            {
                case GpStatus.Ok:
                    return;
                case GpStatus.GdiplusNotInitialized:
                    throw new InvalidOperationException("GDI+ is uninitialized.");
                case GpStatus.UnsupportedGdiplusVersion:
                    throw new PlatformNotSupportedException("The version that the DLL requires as a GDI+ version is unavailable on this OS version.");
                case GpStatus.FileNotFound:
                    throw new System.IO.FileNotFoundException("A file required by GDI+ was not found.");
                case GpStatus.GenericError:
                    throw new ExternalException("A generic error occured in GDI+.");
                case GpStatus.OutOfMemory:
                    throw new OutOfMemoryException("GDI+ did not have enough memory to complete the operation.");
                case GpStatus.Win32Error:
                    throw new System.ComponentModel.Win32Exception("GDI+ failed because of a Win32 error.");
                case GpStatus.InvalidParameter:
                    throw new ArgumentException("GDI+ failed because an invalid parameter was passed in the call.");
                case GpStatus.ObjectBusy:
                    throw new InvalidOperationException("Invalid attempt to perform a method call which is performed already in another thread.");
                default:
                    throw new ExternalException($"GDI+ failed due to {status} .");
            }
        }

        /// <summary>
        /// Handles the usage and startup/shutdown of GDI+.
        /// </summary>
        [System.Security.SuppressUnmanagedCodeSecurity]
        public sealed class GDIPManager : IDisposable
        {
            private static GDIPManager instance;

            /// <summary>
            /// Gets the only and default handle manager.
            /// </summary>
            public static GDIPManager Default => instance ??= new();

            private System.Collections.Generic.List<System.IntPtr> bitmaphandles;
            private System.IntPtr tokengdip;

            public GDIPManager() {
                bitmaphandles = new();
                tokengdip = System.IntPtr.Zero;
                CreateGDIPlus();
                System.AppDomain.CurrentDomain.DomainUnload += DU_HandleManager;
            }

            private void DU_HandleManager(System.Object send, System.EventArgs e) => Dispose();

            public System.IntPtr GetBitmapHandleFromStream(System.IO.Stream stream , System.Boolean ICM)
            {
                System.IntPtr ret = System.IntPtr.Zero;
                GpStatus st;
                CreateGDIPlus();
                COM.ComStream nativestream = new(stream);
                if (stream is null) { return System.IntPtr.Zero; }
                if (ICM) {
                    st = CreateGdipBitmapFromStreamICM_Native(nativestream, out ret);
                } else {
                    st = CreateGdipBitmapFromStream_Native(nativestream, out ret);
                }
                StatusToExceptionMarshaller(st);
                bitmaphandles.Add(ret);
                nativestream = null;
                return ret;
            }

            public System.IntPtr GetBitmapHandleFromHICON(System.IntPtr hicon)
            {
                System.IntPtr ret = System.IntPtr.Zero;
                GpStatus st;
                CreateGDIPlus();
                if ((st = CreateGdipBitmapFromHICON(hicon , out ret)) != GpStatus.Ok) {
                    StatusToExceptionMarshaller(st);
                }
                bitmaphandles.Add(ret);
                return ret;
            }

            public unsafe System.IntPtr GetBitmapHandleFromDIB(System.Byte[] data , System.Int32 startindex)
            {
                System.IntPtr ret = System.IntPtr.Zero;
                CreateGDIPlus();
                BITMAPHEADER header = BITMAPHEADER.ReadFromArray(data, startindex);
                System.Boolean wasrle = false;
                if (header.Compression == ImageType.BI_RLE8 || header.Compression == ImageType.BI_RLE4)
                {
                    System.Byte[] dec = RLEDecoder.Decode(data, startindex);
                    wasrle = true;
                    header.Compression = ImageType.BI_RGB;
                    header.ImageSize = 0;
                    // Directly set the decoded data to the raw array , then special case the native call.
                    // Header does not need reconstruction since it remained the same.
                    // We are ready for the native call!
                    fixed (System.Byte* source = dec)
                    {
                        StatusToExceptionMarshaller(CreateGdipBitmapFromDIB(header, source, out ret));
                    }
                    dec = null;
                }
                if (wasrle == false) {
                    // If we can have color table size , it will be used;
                    // Otherwise , use the old method as a valid fallback.
                    System.UInt32 cssize = header.GetHeader().ColorTablesSize;
                    // Do not call RequiredBitMapBufferSize unless required.
                    System.Int32 bsize1 = cssize <= 0 ? Gdi32.RequiredBitMapBufferSize(header) : 0;
                    System.Int32 index = (cssize > 0 ? (System.Int32)(header.Size + cssize) : data.Length - bsize1) + startindex;
                    fixed (System.Byte* source = &data[index])
                    {
                        StatusToExceptionMarshaller(CreateGdipBitmapFromDIB(header, source, out ret));
                    }
                }
                bitmaphandles.Add(ret);
                return ret;
            }

            public bool IsAlive(System.IntPtr hnd) => bitmaphandles.Contains(hnd);

            public bool RemoveBitmapHandle(System.IntPtr hnd)
            {
                GpStatus st;
                try
                {
                    if (bitmaphandles.Contains(hnd) && (st = DisposeGdipBitmap(hnd)) != GpStatus.Ok)
                    {
                        StatusToExceptionMarshaller(st);
                    }
                } catch (Exception ex) when 
                    (ex is System.Security.SecurityException || ex is System.OutOfMemoryException ||
                     ex is System.IndexOutOfRangeException || ex is System.NullReferenceException) {
                    throw;
                }
                bool ret = bitmaphandles.Remove(hnd);
                if (ret && bitmaphandles.Count == 0)
                {
                    // The GDI+ context can be safely disposed.
                    DisposeGDIPlus();
                }
                return ret;
            }

            public void Clear()
            {
                GpStatus st;
                foreach (var hnd in bitmaphandles)
                {
                    if ((st = DisposeGdipBitmap(hnd)) != GpStatus.Ok) {
                        StatusToExceptionMarshaller(st);
                    }
                }
                bitmaphandles.Clear();
            }

            private void CreateGDIPlus()
            {
                if (tokengdip == System.IntPtr.Zero)
                {
                    GpStatus st;
                    if ((st = CreateGDIPEnvironment(out tokengdip, GdipStartupInput.GetDefault(), out GdipStartupOutput _)) != GpStatus.Ok)
                    {
                        StatusToExceptionMarshaller(st);
                    }
                }
            }

            private void DisposeGDIPlus()
            {
                if (tokengdip != IntPtr.Zero)
                {
                    DestroyGDIPEnvironment(tokengdip);
                    tokengdip = IntPtr.Zero;
                }
            }

            /// <summary>
            /// Call this when GDI+ can be uninitialized , or when you want to dispose all GDI+ data.
            /// </summary>
            public void Dispose()
            {
                if (bitmaphandles is not null)
                {
                    Clear();
                    bitmaphandles = null;
                }
                DisposeGDIPlus();
                System.AppDomain.CurrentDomain.DomainUnload -= DU_HandleManager;
            }
        }

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipCreateBitmapFromStream", CharSet = CharSet.Unicode, ExactSpelling = true)]
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.Machine)]
        public static extern GpStatus CreateGdipBitmapFromStream_Native(COM.IStream stream , out System.IntPtr ptr);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipCreateBitmapFromStreamICM", CharSet = CharSet.Unicode, ExactSpelling = true)]
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.Machine)]
        public static extern GpStatus CreateGdipBitmapFromStreamICM_Native(COM.IStream stream, out System.IntPtr ptr);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipCreateBitmapFromGdiDib", CharSet = CharSet.Unicode, ExactSpelling = true)]
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.Machine)]
        public static unsafe extern GpStatus CreateGdipBitmapFromDIB(BITMAPHEADER header, void* gdibits, out System.IntPtr bitmap);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipCreateBitmapFromHICON" , CharSet = CharSet.Unicode, ExactSpelling = true)]
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.Machine)]
        public static extern GpStatus CreateGdipBitmapFromHICON(System.IntPtr hicon , out System.IntPtr bitmap);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipCreateHICONFromBitmap" , CharSet = CharSet.Unicode, ExactSpelling = true)]
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.Machine)]
        public static extern GpStatus GetHICONFromGdipBitmap(System.IntPtr ptr, out System.IntPtr iconhandle);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipCreateHBITMAPFromBitmap", CharSet = CharSet.Unicode, ExactSpelling = true)]
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.Machine)]
        public static extern GpStatus GetHBITMAPFromGdipBitmap(System.IntPtr ptr , out System.IntPtr bitmaphandle, System.Int32 background = -2894893);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipImageForceValidation", CharSet = CharSet.Unicode, ExactSpelling = true)]
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.None)]
        public static extern GpStatus ForceGdipBitmapValidation(System.IntPtr image);

        [DllImport(Libraries.GdiPlus, CharSet = CharSet.Unicode, EntryPoint = "GdipDisposeImage", ExactSpelling = true)]
        public static extern GpStatus DisposeGdipBitmap(System.IntPtr image);

        [DllImport(Libraries.GdiPlus , CharSet = CharSet.Unicode , EntryPoint = "GdiplusStartup" , ExactSpelling = true, SetLastError = true)]
        public static extern GpStatus CreateGDIPEnvironment(out System.IntPtr token, in GdipStartupInput si, out GdipStartupOutput so);

        [DllImport(Libraries.GdiPlus, CharSet = CharSet.Unicode , EntryPoint = "GdiplusShutdown" , ExactSpelling = true, SetLastError = true)]
        public static extern void DestroyGDIPEnvironment(IntPtr token);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageWidth" , CharSet = CharSet.Unicode ,  ExactSpelling = true)]
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.None)]
        public static extern GpStatus GetGdipBitmapWidth(System.IntPtr bitmaphandle , out System.UInt32 width);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageHeight" , CharSet = CharSet.Unicode , ExactSpelling = true)]
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.None)]
        public static extern GpStatus GetGdipBitmapHeight(System.IntPtr bitmaphandle , out System.UInt32 height);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageFlags" , CharSet = CharSet.Unicode , ExactSpelling = true)]
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.None)]
        public static extern GpStatus GetGdipBitmapFlags(System.IntPtr bitmaphandle, out System.UInt32 flags);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageRawFormat" , CharSet = CharSet.Unicode , ExactSpelling = true)]
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.None)]
        public static extern GpStatus GetGdipBitmapFormat(System.IntPtr bitmaphandle, out GUID format);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageHorizontalResolution" , CharSet = CharSet.Unicode , ExactSpelling = true)]
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.None)]
        public static extern GpStatus GetGdipBitmapHorizontalResolution(System.IntPtr bitmaphandle, out System.Single hres);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipGetImageVerticalResolution", CharSet = CharSet.Unicode, ExactSpelling = true)]
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.None)]
        public static extern GpStatus GetGdipBitmapVerticalResolution(System.IntPtr bitmaphandle, out System.Single vres);
    }

    public static class COM
    {
        public class IStreamConsts
        {
            public const int LOCK_WRITE = 1;

            public const int LOCK_EXCLUSIVE = 2;

            public const int LOCK_ONLYONCE = 4;

            public const int STATFLAG_DEFAULT = 0;

            public const int STATFLAG_NONAME = 1;

            public const int STATFLAG_NOOPEN = 2;

            public const int STGC_DEFAULT = 0;

            public const int STGC_OVERWRITE = 1;

            public const int STGC_ONLYIFCURRENT = 2;

            public const int STGC_DANGEROUSLYCOMMITMERELYTODISKCACHE = 4;

            public const int STREAM_SEEK_SET = 0;

            public const int STREAM_SEEK_CUR = 1;

            public const int STREAM_SEEK_END = 2;
        }

        /// <summary>Provides the managed definition of the <see langword="IStream" /> interface, with <see langword="ISequentialStream" /> functionality.</summary>
        [ComImport]
        [Guid("0000000c-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IStream
        {
            /// <summary>Reads a specified number of bytes from the stream object into memory starting at the current seek pointer.</summary>
            /// <param name="pv">When this method returns, contains the data read from the stream. This parameter is passed uninitialized.</param>
            /// <param name="cb">The number of bytes to read from the stream object.</param>
            /// <param name="pcbRead">A pointer to a <see langword="ULONG" /> variable that receives the actual number of bytes read from the stream object.</param>
            void Read([Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pv, int cb, IntPtr pcbRead);

            /// <summary>Writes a specified number of bytes into the stream object starting at the current seek pointer.</summary>
            /// <param name="pv">The buffer to write this stream to.</param>
            /// <param name="cb">The number of bytes to write to the stream.</param>
            /// <param name="pcbWritten">On successful return, contains the actual number of bytes written to the stream object. If the caller sets this pointer to <see cref="F:System.IntPtr.Zero" />, this method does not provide the actual number of bytes written.</param>
            void Write([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pv, int cb, IntPtr pcbWritten);

            /// <summary>Changes the seek pointer to a new location relative to the beginning of the stream, to the end of the stream, or to the current seek pointer.</summary>
            /// <param name="dlibMove">The displacement to add to <paramref name="dwOrigin" />.</param>
            /// <param name="dwOrigin">The origin of the seek. The origin can be the beginning of the file, the current seek pointer, or the end of the file.</param>
            /// <param name="plibNewPosition">On successful return, contains the offset of the seek pointer from the beginning of the stream.</param>
            void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition);

            /// <summary>Changes the size of the stream object.</summary>
            /// <param name="libNewSize">The new size of the stream as a number of bytes.</param>
            void SetSize(long libNewSize);

            /// <summary>Copies a specified number of bytes from the current seek pointer in the stream to the current seek pointer in another stream.</summary>
            /// <param name="pstm">A reference to the destination stream.</param>
            /// <param name="cb">The number of bytes to copy from the source stream.</param>
            /// <param name="pcbRead">On successful return, contains the actual number of bytes read from the source.</param>
            /// <param name="pcbWritten">On successful return, contains the actual number of bytes written to the destination.</param>
            void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten);

            /// <summary>Ensures that any changes made to a stream object that is open in transacted mode are reflected in the parent storage.</summary>
            /// <param name="grfCommitFlags">A value that controls how the changes for the stream object are committed.</param>
            void Commit(int grfCommitFlags);

            /// <summary>Discards all changes that have been made to a transacted stream since the last <see cref="M:System.Runtime.InteropServices.ComTypes.IStream.Commit(System.Int32)" /> call.</summary>
            void Revert();

            /// <summary>Restricts access to a specified range of bytes in the stream.</summary>
            /// <param name="libOffset">The byte offset for the beginning of the range.</param>
            /// <param name="cb">The length of the range, in bytes, to restrict.</param>
            /// <param name="dwLockType">The requested restrictions on accessing the range.</param>
            void LockRegion(long libOffset, long cb, int dwLockType);

            /// <summary>Removes the access restriction on a range of bytes previously restricted with the <see cref="M:System.Runtime.InteropServices.ComTypes.IStream.LockRegion(System.Int64,System.Int64,System.Int32)" /> method.</summary>
            /// <param name="libOffset">The byte offset for the beginning of the range.</param>
            /// <param name="cb">The length, in bytes, of the range to restrict.</param>
            /// <param name="dwLockType">The access restrictions previously placed on the range.</param>
            void UnlockRegion(long libOffset, long cb, int dwLockType);

            /// <summary>Retrieves the <see cref="T:System.Runtime.InteropServices.STATSTG" /> structure for this stream.</summary>
            /// <param name="pstatstg">When this method returns, contains a <see langword="STATSTG" /> structure that describes this stream object. This parameter is passed uninitialized.</param>
            /// <param name="grfStatFlag">Members in the <see langword="STATSTG" /> structure that this method does not return, thus saving some memory allocation operations.</param>
            void Stat(out STATSTG pstatstg, int grfStatFlag);

            /// <summary>Creates a new stream object with its own seek pointer that references the same bytes as the original stream.</summary>
            /// <param name="ppstm">When this method returns, contains the new stream object. This parameter is passed uninitialized.</param>
            void Clone(out IStream ppstm);
        }

        /// <summary>
        /// Represents a stream that is a wrapper to a COM Stream.
        /// </summary>
        internal class ComStream : System.IO.Stream, IStream
        {
            private System.IO.Stream stream;
            /// <summary>
            /// Gets a value whether this stream is syncronized.
            /// </summary>
            protected System.Boolean sync;

            public override bool CanRead => stream.CanRead;

            public override bool CanSeek => stream.CanSeek;

            public override bool CanWrite => stream.CanWrite;

            public override long Length => stream.Length;

            public override long Position
            {
                get
                {
                    return stream.Position;
                }
                set
                {
                    stream.Position = value;
                }
            }

            public ComStream(System.IO.Stream stream)
                : this(stream, synchronizeStream: true)
            {
            }

            private ComStream(System.IO.Stream stream, bool synchronizeStream)
            {
                if (stream is null)
                {
                    throw new ArgumentNullException(nameof(stream));
                }
                if (sync = synchronizeStream)
                {
                    stream = System.IO.Stream.Synchronized(stream);
                }
                this.stream = stream;
            }

            void IStream.Clone(out IStream ppstm) { ppstm = null; }

            void IStream.Commit(int grfCommitFlags) => stream.Flush();

            void IStream.CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
            {
                const System.Int32 buffersize = 2048;
                if (cb > System.Int32.MaxValue)
                {
                    throw new NotSupportedException("Cannot copy the given number of bytes because it is extravagantly large to be handled by .NET.");
                }
                System.Int32 br = 0, bw = 0, blocks = (System.Int32)(cb / buffersize), remaining = (System.Int32)(cb % buffersize), trb;
                System.IntPtr TBW = new(1);
                System.Byte[] temp = new System.Byte[buffersize];
                while (blocks > 0)
                {
                    trb = stream.Read(temp, 0, buffersize);
                    pstm.Write(temp, trb, TBW);
                    bw += Marshal.ReadInt32(TBW);
                    br += trb;
                    blocks--;
                }
                if (remaining > 0)
                {
                    trb = stream.Read(temp, 0, buffersize);
                    pstm.Write(temp, trb, TBW);
                    bw += Marshal.ReadInt32(TBW);
                    br += trb;
                }
                if (pcbRead != System.IntPtr.Zero)
                {
                    Marshal.WriteInt32(pcbRead, br);
                }
                if (pcbWritten != System.IntPtr.Zero)
                {
                    Marshal.WriteInt32(pcbWritten, bw);
                }
            }

            void IStream.LockRegion(long libOffset, long cb, int dwLockType) { }

            void IStream.Read(byte[] pv, int cb, IntPtr pcbRead)
            {
                if (!CanRead)
                {
                    throw new InvalidOperationException("Stream is not readable.");
                }
                int val = Read(pv, 0, cb);
                if (pcbRead != IntPtr.Zero)
                {
                    Marshal.WriteInt32(pcbRead, val);
                }
            }

            void IStream.Revert() { }

            void IStream.Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
            {
                long val = Seek(dlibMove, (System.IO.SeekOrigin)dwOrigin);
                if (plibNewPosition != IntPtr.Zero)
                {
                    Marshal.WriteInt64(plibNewPosition, val);
                }
            }

            void IStream.SetSize(long libNewSize) => SetLength(libNewSize);

            void IStream.Stat(out STATSTG pstatstg, int grfStatFlag)
            {
                STATSTG sTATSTG = default;
                sTATSTG.type = 2;
                sTATSTG.cbSize = Length;
                sTATSTG.grfMode = 0;
                STATSTG ret = sTATSTG;
                if (CanWrite && CanRead)
                {
                    ret.grfMode |= 2;
                }
                else if (CanRead)
                {
                    ret.grfMode |= 0;
                }
                else
                {
                    if (!CanWrite)
                    {
                        throw new ObjectDisposedException("Stream");
                    }
                    ret.grfMode |= 1;
                }
                ret.ctime = FILETIME.ToNative(System.DateTime.Now);
                pstatstg = ret;
            }

            void IStream.UnlockRegion(long libOffset, long cb, int dwLockType) { }

            void IStream.Write(byte[] pv, int cb, IntPtr pcbWritten)
            {
                if (!CanWrite)
                {
                    throw new InvalidOperationException("Stream is not writeable.");
                }
                Write(pv, 0, cb);
                if (pcbWritten != IntPtr.Zero)
                {
                    Marshal.WriteInt32(pcbWritten, cb);
                }
            }

            public override void Flush() => stream.Flush();

            public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);

            public override long Seek(long offset, System.IO.SeekOrigin origin) => stream.Seek(offset, origin);

            public override void SetLength(long value) => stream.SetLength(value);

            public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }
            }

            public override void Close()
            {
                base.Close();
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }
        }

        // The older statstg had issues , using the one provided with System.Drawing instead.
        [StructLayout(LayoutKind.Sequential)]
        public struct STATSTG
        {
            public IntPtr pwcsName;
            public int type;
            [MarshalAs(UnmanagedType.I8)]
            public long cbSize;
            [MarshalAs(UnmanagedType.I8)]
            public FILETIME mtime;
            [MarshalAs(UnmanagedType.I8)]
            public FILETIME ctime;
            [MarshalAs(UnmanagedType.I8)]
            public FILETIME atime;
            [MarshalAs(UnmanagedType.I4)]
            public int grfMode;
            [MarshalAs(UnmanagedType.I4)]
            public int grfLocksSupported;
            public GUID clsid;
            [MarshalAs(UnmanagedType.I4)]
            public int grfStateBits;
            [MarshalAs(UnmanagedType.I4)]
            public int reserved;
        }

        [StructLayout(LayoutKind.Explicit , Size = 8 , Pack = 4)]
        public struct FILETIME
        {
            [FieldOffset(0)]
            private System.Int64 FileTime;
            [FieldOffset(0)]
            public int HighDateTime;
            [FieldOffset(4)]
            public int LowDateTime;

            public readonly System.Int64 ToTicks() => (((System.UInt64)HighDateTime << 32).ToInt64() + LowDateTime);

            // This is a bit redundant operation but does worth to have it this way.
            public readonly System.Int64 ToFileTime() => FileTime;

            public readonly System.DateTime ToDateTime() => System.DateTime.FromFileTimeUtc(FileTime);

            public static unsafe FILETIME ToNative(System.DateTime datetime) => new() { FileTime = datetime.ToFileTimeUtc() };
        }

    }

    // Native Bitmap Information header so as to get the bitmap information.
    [StructLayout(LayoutKind.Explicit , Size = 36)]
    public unsafe struct BITMAPINFOHEADER
    {
        public static BITMAPINFOHEADER ReadFromArray(System.Byte[] bytes , System.Int32 startindex)
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
                    Unsafe.CopyBlockUnaligned(ptr, array, sizeof(BITMAPINFOHEADER).ToUInt32());
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

        /// <summary>
        /// Gets a value of how many color tables must/or exist in the image. <br />
        /// A value of 0 means that the image type is invalid to compute color indices , 
        /// or the image does not require color tables.
        /// </summary>
        public readonly System.UInt32 ColorTablesCount
        {
            get {
                System.UInt32 result = 0;
                // Color tables are not required for 16-bit bitmaps and above.
                switch (Compression)
                {
                    case ImageType.BI_RGB:
                    case ImageType.BI_RLE8:
                    case ImageType.BI_RLE4:
                        // No meaning to compute the color tables if the bpp is more than 8!
                        if (BitCount <= 8) {
                            if (ColorIndices == 0) {
                                result = (System.UInt32)System.Math.Pow(2, BitCount);
                            } else {
                                result = ColorIndices;
                            }
                        }
                        break;
                    default:
                        // But if these are existing for 16-bit and above , return them.
                        if (ColorIndices > 0) { result = ColorIndices; }
                        break;
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the final size of the color tables in bytes. <br />
        /// This value can be zero when this value is not required or when the image type is invalid.
        /// </summary>
        public readonly System.UInt32 ColorTablesSize
        {
            // The below code is pretty-much based on multiple articles that specify how color tables are laid out.
            // For a basic grasp you can see https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapinfoheader#color-tables
            get {
                // Size of a single color table. (It is named that way because the original color table structure is named RGBQUAD) 
                const System.Int32 RGBQUADSIZE = 4; 
                System.UInt32 result = 0;
                switch (Compression)
                {
                    case ImageType.BI_RGB:
                    case ImageType.BI_RLE8:
                    case ImageType.BI_RLE4:
                        result = ColorTablesCount * RGBQUADSIZE;
                        break;
                    case ImageType.BI_BITFIELDS:
                        // In this case we have 3 masks , each of which is a DWORD so it is 3 * the size of a System.UInt32.
                        // If we also have color tables , add them too!
                        // It works correctly when ColorTablesCount == 0 because all the expression will evaluate to zero.
                        result = (3 * sizeof(System.UInt32)) + (ColorTablesCount * RGBQUADSIZE);
                        break;
                }
                return result;
            }
        }

        public readonly System.Byte[] GetBytes()
        {
            System.Byte[] result = new System.Byte[sizeof(BITMAPINFOHEADER)];
            fixed (byte* dest = result)
            {
                fixed (byte* src = &Unsafe.AsRef(pin))
                {
                    Unsafe.CopyBlockUnaligned(dest, src, sizeof(BITMAPINFOHEADER).ToUInt32());
                }
            }
            return result;
        }
    }

    // This structure is not used in any native operation , it is just used to create or read typed bitmaps.
    [StructLayout(LayoutKind.Explicit , Size = 14)]
    public unsafe struct BITMAPFILEHEADER
    {
        private static System.Byte[] CreateHeaderData(System.Byte[] withoutheader , System.Int32 startindex , BITMAPFILEHEADER header)
        {
            System.UInt32 filehdrsize = sizeof(BITMAPFILEHEADER).ToUInt32();
            // Combine the data and return them.
            System.Byte[] result = new System.Byte[filehdrsize + (withoutheader.Length - startindex)];
            // The below unsafe calls avoid to create a new byte array ,
            // and improve performance.
            fixed (System.Byte* dst = result)
            {
                fixed (System.Byte* hdrptr = &Unsafe.AsRef(header.pin))
                {
                    Unsafe.CopyBlockUnaligned(dst, hdrptr, filehdrsize);
                }
                fixed (System.Byte* src = &withoutheader[startindex])
                {
                    Unsafe.CopyBlockUnaligned(Unsafe.Add<System.Byte>(dst, filehdrsize.ToInt32()), 
                        src, (withoutheader.Length - startindex).ToUInt32());
                }
            }
            return result;
        }

        public static System.Byte[] CreateBitmap(System.Byte[] withoutheader)
        {
            // The below variable is constant but keep it this way for compat.
            System.UInt32 filehdrsize = sizeof(BITMAPFILEHEADER).ToUInt32();
            BITMAPINFOHEADER data = BITMAPINFOHEADER.ReadFromArray(withoutheader, 0);
            BITMAPFILEHEADER header = new();
            header.Type = 0x4d42; // Is the 'BM' string in ASCII.
            // Because the final size of the entire bitmap data is known due to the array ,
            // it is possible to just add the header size plus the raw data length themselves.
            header.Size = (filehdrsize + withoutheader.Length).ToUInt32();
            // The below field is set based on this article: https://learn.microsoft.com/en-us/windows/win32/gdi/storing-an-image
            header.Offset = filehdrsize + data.Size + data.ColorTablesSize;
            // Set reserved fields to zero (although that setting to them other values would not be problem)
            header.RSVD1 = 0; header.RSVD2 = 0;
            return CreateHeaderData(withoutheader, 0, header);
        }

        public static System.Byte[] CreateCursor(System.Byte[] withoutheader)
        {
            // Same thing happens with the cursors , but the first 4 bytes must be ignored because they do contain the hotspot values.
            // In this way , the cursor is marshalled as a bitmap.
            // The below variable is constant but keep it this way for compat.
            System.UInt32 filehdrsize = sizeof(BITMAPFILEHEADER).ToUInt32();
            BITMAPINFOHEADER data = BITMAPINFOHEADER.ReadFromArray(withoutheader, 4);
            BITMAPFILEHEADER header = new();
            header.Type = 0x4d42; // Is the 'BM' string in ASCII.
            // Because the final size of the entire bitmap data is known due to the array ,
            // it is possible to just add the header size plus the raw data length themselves.
            header.Size = (filehdrsize + (withoutheader.Length - 4)).ToUInt32();
            // The below field is set based on this article: https://learn.microsoft.com/en-us/windows/win32/gdi/storing-an-image
            header.Offset = filehdrsize + data.Size + data.ColorTablesSize;
            // Set reserved fields to zero (although that setting to them other values would not be problem)
            header.RSVD1 = 0; header.RSVD2 = 0;
            return CreateHeaderData(withoutheader, 4, header);
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
                    Unsafe.CopyBlockUnaligned(ptr, array, sizeof(BITMAPFILEHEADER).ToUInt32());
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
                    Unsafe.CopyBlockUnaligned(dest, src , sizeof(BITMAPFILEHEADER).ToUInt32());
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
            System.UInt32 reqsize = sizeof(BITMAPHEADER).ToUInt32();
            BITMAPINFOHEADER hdr = BITMAPINFOHEADER.ReadFromArray(bytes, startindex);
            // There are some cases that bytes do not contain at least 1060 bytes , so possibly the bitmap does not contain 256 tables.
            // The below code checks if we have less size than the required and if so , the structure is initialized with only the required size.
            if (reqsize > bytes.Length - startindex)
            {
                // Color table is not required for above 8bpp but if it does require them , get them. 
                // Note that RLE4 and 8 must be processed on the bitmap bits - but that will be performed in the calling method.
                reqsize = hdr.Size + hdr.ColorTablesSize;
            }
            // We have the required size , we can initialize BITMAPHEADER without losing information.
            if (reqsize > bytes.Length - startindex)
            {
                throw new ArgumentException("There are not enough elements to copy so that the BITMAPHEADER structure can be initialized.");
            }
            hdr = default;
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
                Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref result), source, sizeof(BITMAPINFOHEADER).ToUInt32());
            }
            return result;
        }
    }

    // The starting header when reading resources of type RT_GROUP_ICON or RT_GROUP_CURSOR.
    [StructLayout(LayoutKind.Explicit , Size = 6)]
    public unsafe struct NEWHEADER
    {
        public static NEWHEADER ReadFromArray(System.Byte[] bytes , System.Int32 startindex)
        {
            if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
            if (sizeof(NEWHEADER) > bytes.Length - startindex)
            {
                throw new ArgumentException("There are not enough elements to copy so that the NEWHEADER structure can be initialized.");
            }
            NEWHEADER result = new();
            fixed (System.Byte* ptr = &Unsafe.AsRef(result.pin))
            {
                fixed (System.Byte* array = &bytes[startindex])
                {
                    Unsafe.CopyBlockUnaligned(ptr, array, sizeof(NEWHEADER).ToUInt32());
                }
            }
            return result;
        }

        [FieldOffset(0)]
        private System.Byte pin;

        // Reserved field , do not modify.
        [FieldOffset(0)]
        public System.UInt16 RSVD;

        /// <summary>
        /// If this structure contains icon information , the value 1 has been assigned; otherwise the value 2 is assigned , which means that it contains cursors.
        /// </summary>
        [FieldOffset(2)]
        public System.UInt16 ResourceType;

        /// <summary>
        /// Gets the number of <see cref="RESDIR"/> structures contained in this RT_GROUP_XXXX resource.
        /// </summary>
        [FieldOffset(4)]
        public System.UInt16 DirectoryCount;
    }

    // The main information structure. Someone can consider it a 'link' between a resource and this information.
    [StructLayout(LayoutKind.Explicit , Size = 14)]
    public unsafe struct RESDIR
    {
        // Helper structure for RESDIR
        [StructLayout(LayoutKind.Explicit, Size = 4)]
        public struct ICONRESDIR
        {
            [FieldOffset(0)]
            public System.Byte Width;

            [FieldOffset(1)]
            public System.Byte Height;

            [FieldOffset(2)]
            public System.Byte ColorCount;

            [FieldOffset(3)]
            public System.Byte Reserved;
        }

        // Helper structure for RESDIR
        [StructLayout(LayoutKind.Explicit, Size = 4)]
        public struct CURSORDIR
        {
            [FieldOffset(0)]
            public System.UInt16 Width;

            [FieldOffset(2)]
            public System.UInt16 Height;
        }

        public static RESDIR ReadFromArray(System.Byte[] bytes, System.Int32 startindex)
        {
            if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
            if (sizeof(RESDIR) > bytes.Length - startindex)
            {
                throw new ArgumentException("There are not enough elements to copy so that the RESDIR structure can be initialized.");
            }
            RESDIR result = new();
            fixed (System.Byte* ptr = &Unsafe.AsRef(result.pin))
            {
                fixed (System.Byte* array = &bytes[startindex])
                {
                    Unsafe.CopyBlockUnaligned(ptr, array, sizeof(RESDIR).ToUInt32());
                }
            }
            return result;
        }

        [FieldOffset(0)]
        private System.Byte pin;

        // The actual RESDIR defines the below two fields as a union , but since both are being aligned on position 0 , 
        // these two fields act as if it was the union here.

        [FieldOffset(0)]
        public ICONRESDIR Icon;

        [FieldOffset(0)]
        public CURSORDIR Cursor;

        [FieldOffset(4)]
        public System.UInt16 Planes;

        [FieldOffset(6)]
        public System.UInt16 BitCount;

        [FieldOffset(8)]
        public System.UInt32 BytesInRes;

        [FieldOffset(12)]
        public System.UInt16 IconCursorId;
    }

    // Defines the accelerator table structure.
    [StructLayout(LayoutKind.Explicit, Size = 6, Pack = 2)]
    public unsafe struct ACCEL
    {
        public static void* CreateUnmanagedArray(ACCEL[] accelerators)
        {
            if (accelerators is null) { throw new ArgumentNullException(nameof(accelerators)); }
            if (accelerators.Length == 0) { throw new ArgumentException("At least one element is required to create the unmanaged array."); }
            System.Int32 bc = accelerators.Length * sizeof(ACCEL);
            System.IntPtr ptr = Marshal.AllocHGlobal(bc);
            fixed (ACCEL* src = accelerators)
            {
                Unsafe.CopyBlockUnaligned((void*)ptr, src, bc.ToUInt32());
            }
            return (void*)ptr;
        }

        public static ACCEL ReadFromArray(System.Byte[] bytes, System.Int32 startindex)
        {
            if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
            System.Int32 sdt = sizeof(ACCEL);
            if (sdt > bytes.Length - startindex)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), "There are not enough elements so that the ACCEL structure can be initialized.");
            }
            ACCEL ret = new();
            fixed (System.Byte* target = &Unsafe.AsRef(ret.pin))
            {
                fixed (System.Byte* source = &bytes[startindex])
                {
                    Unsafe.CopyBlockUnaligned(target, source, sdt.ToUInt32());
                }
            }
            return ret;
        }

        [FieldOffset(0)]
        private System.Byte pin;

        [FieldOffset(0)]
        public VirtualKeyFlags FVirtual;

        [FieldOffset(1)]
        public System.Char Key;

        [FieldOffset(3)]
        public System.UInt16 IdOrCommand;
    }

    // Although that this structure does not exist in any header , this is a single accelerator table entry in RC format.
    [StructLayout(LayoutKind.Explicit , Size = 8)]
    public unsafe struct ACCELTABLEENTRY
    {
        public static ACCELTABLEENTRY ReadFromArray(System.Byte[] bytes, System.Int32 startindex)
        {
            if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
            System.Int32 sdt = sizeof(ACCELTABLEENTRY);
            if (sdt > bytes.Length - startindex)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), "There are not enough elements so that the ACCEL structure can be initialized.");
            }
            ACCELTABLEENTRY ret = new();
            fixed (System.Byte* target = &Unsafe.AsRef(ret.pin))
            {
                fixed (System.Byte* source = &bytes[startindex])
                {
                    Unsafe.CopyBlockUnaligned(target, source, sdt.ToUInt32());
                }
            }
            return ret;
        }

        [FieldOffset(0)]
        private System.Byte pin;

        [FieldOffset(0)]
        public VirtualKeyFlags FVirtual;

        [FieldOffset(2)]
        public System.Char KeyCode;

        [FieldOffset(4)]
        public System.UInt16 IdOrCommand;

        /// <summary>
        /// The number of bytes inserted to ensure that the structure is aligned on a System.UInt32 boundary.
        /// </summary>
        [FieldOffset(6)]
        public System.UInt16 Padding;

        public readonly ACCEL ToAccelerator() {
            ACCEL ret = new();
            ret.FVirtual = FVirtual;
            ret.Key = KeyCode;
            ret.IdOrCommand = IdOrCommand;
            return ret;
        }
    }

    /// <summary>
    /// GUID native marshalling type. <br />
    /// Provides also methods to convert from , and to , a <see cref="System.Guid"/> structure.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16, Pack = 4)]
    public unsafe struct GUID
    {
        [FieldOffset(0)]
        private System.Byte pin;
        [FieldOffset(0)]
        public System.UInt32 Data1;
        [FieldOffset(4)]
        public System.UInt16 Data2;
        [FieldOffset(6)]
        public System.UInt16 Data3;
        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public System.Byte[] Data4;

        public GUID()
        {
            pin = 0;
            Data1 = 0;
            Data2 = 0;
            Data3 = 0;
            Data4 = new System.Byte[8];
        }

        public readonly System.Guid GetGuid()
        {
            System.Byte[] temp = new System.Byte[16];
            fixed (System.Byte* source = &Unsafe.AsRef(pin))
            {
                fixed (System.Byte* target = temp)
                {
                    Unsafe.CopyBlockUnaligned(target, source, 16);
                }
            }
            return new(temp);
        }

        public static GUID FromGUID(System.Guid guid)
        {
            GUID result = new();
            System.Byte[] bytes = guid.ToByteArray();
            fixed (System.Byte* source = bytes)
            {
                fixed (System.Byte* target = &Unsafe.AsRef(result.pin)) 
                {
                    Unsafe.CopyBlockUnaligned(target, source, 16);
                }
            }
            bytes = null;
            return result;
        }

        public static GUID Empty = new();

        public static GUID FromString(System.String str) => FromGUID(new(str));

        /// <summary>
        /// Returns the fully constructed GUID.
        /// </summary>
        public override readonly System.String ToString() => GetGuid().ToString();
    }

    public static System.UInt16 MAKEFOURCC(System.Char ch0, System.Char ch1, System.Char ch2, System.Char ch3)
        => (System.UInt16)((System.UInt16)(System.Byte)ch0 | (((System.UInt16)(System.Byte)ch1) << 8) | (((System.UInt16)(System.Byte)ch2) << 16) | (((System.UInt16)(System.Byte)ch3) << 24));

    // Gets a value whether whether we are from Windows so as to invoke the required API's.
    public static System.Boolean ApisSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}

// Type mappings:
// DWORD corresponds to System.UInt32
// WORD corresponds to System.UInt16
// LONG corresponds to System.Int32
// UINT corresponds to System.UInt32
// BYTE corresponds to System.Byte
