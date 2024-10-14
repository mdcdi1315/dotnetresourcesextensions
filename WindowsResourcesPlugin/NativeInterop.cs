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
        [DllImport(Libraries.Gdi32 , EntryPoint = "DeleteObject" , SetLastError = true , ExactSpelling = true)]
        public static extern BOOL DeleteObject(System.IntPtr handle);

        public static System.Int32 RequiredBitMapBufferSize(BITMAPHEADER header)
            => (((header.Width * header.Planes * header.BitCount + 15) >> 4) << 1) * header.Height;
    }

    [System.Security.SuppressUnmanagedCodeSecurity]
    public static class GdiPlus
    {
        public const System.Int32 Format_32bppargb = 2498570;

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

            private GDIPManager() {
                bitmaphandles = new();
                tokengdip = System.IntPtr.Zero;
                CreateGDIPlus();
                System.AppDomain.CurrentDomain.DomainUnload += DU_HandleManager;
            }

            private void DU_HandleManager(System.Object send, System.EventArgs e) => Dispose();

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
                if (header.Compression == ImageType.BI_RLE8 || header.Compression == ImageType.BI_RLE4)
                {
                    System.UInt32[] dec = RLEDecoder.DecodeRaw(data, startindex);
                    StatusToExceptionMarshaller(CreateGdipBitmapFromScan0(header.Width , header.Height,0 ,Format_32bppargb , System.IntPtr.Zero , out ret));
                    for (System.Int32 Y = 0; Y < header.Height; Y++) {
                        for (System.Int32 X = 0; X < header.Width; X++) {
                            SetGdipBitmapPixel(ret , X , Y , dec[Y * header.Width + X]);
                        }
                    }
                    dec = null;
                    goto G_end;
                }
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
            G_end:
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

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipCreateBitmapFromGdiDib", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static unsafe extern GpStatus CreateGdipBitmapFromDIB(BITMAPHEADER header, void* gdibits, out System.IntPtr bitmap);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipCreateBitmapFromScan0", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern GpStatus CreateGdipBitmapFromScan0(System.Int32 width, System.Int32 height, System.Int32 stride, System.Int32 format, System.IntPtr bytep, out System.IntPtr bitmap);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipCreateBitmapFromHICON" , CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern GpStatus CreateGdipBitmapFromHICON(System.IntPtr hicon , out System.IntPtr bitmap);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipCreateHICONFromBitmap" , CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern GpStatus GetHICONFromGdipBitmap(System.IntPtr ptr, out System.IntPtr iconhandle);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipCreateHBITMAPFromBitmap", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern GpStatus GetHBITMAPFromGdipBitmap(System.IntPtr ptr , out System.IntPtr bitmaphandle, System.Int32 background = -2894893);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipImageForceValidation", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern GpStatus ForceGdipBitmapValidation(System.IntPtr image);

        [DllImport(Libraries.GdiPlus, CharSet = CharSet.Unicode, EntryPoint = "GdipDisposeImage", ExactSpelling = true)]
        public static extern GpStatus DisposeGdipBitmap(System.IntPtr image);

        [DllImport(Libraries.GdiPlus , CharSet = CharSet.Unicode , EntryPoint = "GdipCloneImage" , ExactSpelling = true)]
        public static extern GpStatus CloneGdipBitmap(System.IntPtr bitmap , out System.IntPtr cloned);

        [DllImport(Libraries.GdiPlus , CharSet = CharSet.Unicode , EntryPoint = "GdiplusStartup" , ExactSpelling = true, SetLastError = true)]
        public static extern GpStatus CreateGDIPEnvironment(out System.IntPtr token, in GdipStartupInput si, out GdipStartupOutput so);

        [DllImport(Libraries.GdiPlus, CharSet = CharSet.Unicode , EntryPoint = "GdiplusShutdown" , ExactSpelling = true, SetLastError = true)]
        public static extern void DestroyGDIPEnvironment(IntPtr token);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageWidth" , CharSet = CharSet.Unicode ,  ExactSpelling = true)]
        public static extern GpStatus GetGdipBitmapWidth(System.IntPtr bitmaphandle , out System.UInt32 width);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageHeight" , CharSet = CharSet.Unicode , ExactSpelling = true)]
        public static extern GpStatus GetGdipBitmapHeight(System.IntPtr bitmaphandle , out System.UInt32 height);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageFlags" , CharSet = CharSet.Unicode , ExactSpelling = true)]
        public static extern GpStatus GetGdipBitmapFlags(System.IntPtr bitmaphandle, out System.UInt32 flags);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageRawFormat" , CharSet = CharSet.Unicode , ExactSpelling = true)]
        public static extern GpStatus GetGdipBitmapFormat(System.IntPtr bitmaphandle, out GUID format);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageHorizontalResolution" , CharSet = CharSet.Unicode , ExactSpelling = true)]
        public static extern GpStatus GetGdipBitmapHorizontalResolution(System.IntPtr bitmaphandle, out System.Single hres);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipGetImageVerticalResolution", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern GpStatus GetGdipBitmapVerticalResolution(System.IntPtr bitmaphandle, out System.Single vres);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipBitmapGetPixel" , CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern GpStatus GetGdipBitmapPixel(System.IntPtr bitmaphandle, System.Int32 X, System.Int32 Y , out System.UInt32 color);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipBitmapSetPixel", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern GpStatus SetGdipBitmapPixel(System.IntPtr bitmaphandle, System.Int32 X, System.Int32 Y, System.UInt32 color);
    }

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
                if (stream is null) { return System.IntPtr.Zero; }
                if (ICM) {
                    if ((st = CreateGdipBitmapFromStreamICM_Native(new COM.GPStream(stream), out ret)) != GpStatus.Ok) {
                        StatusToExceptionMarshaller(st);
                    }
                } else {
                    if ((st = CreateGdipBitmapFromStream_Native(new COM.GPStream(stream), out ret)) != GpStatus.Ok) {
                        StatusToExceptionMarshaller(st);
                    }
                }
                bitmaphandles.Add(ret);
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
                GpStatus st;
                CreateGDIPlus();
                BITMAPHEADER header = BITMAPHEADER.ReadFromArray(data, startindex);
                // If we can have color table size , it will be used;
                // Otherwise , use the old method as a valid fallback.
                System.UInt32 cssize = header.GetHeader().ColorTablesSize;
                // Do not call RequiredBitMapBufferSize unless required.
                System.Int32 bsize1 = cssize <= 0 ? Gdi32.RequiredBitMapBufferSize(header) : 0;
                System.Int32 index = (cssize > 0 ? (System.Int32)(header.Size + cssize) : data.Length - bsize1) + startindex;
                fixed (System.Byte* source = &data[index])
                {
                    if ((st = CreateGdipBitmapFromDIB(header , source , out ret)) != GpStatus.Ok) {
                        StatusToExceptionMarshaller(st);
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
        public static extern GpStatus CreateGdipBitmapFromStream_Native(COM.IStream stream , out System.IntPtr ptr);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipCreateBitmapFromStreamICM", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern GpStatus CreateGdipBitmapFromStreamICM_Native(COM.IStream stream, out System.IntPtr ptr);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipCreateBitmapFromGdiDib", ExactSpelling = true)]
        public static unsafe extern GpStatus CreateGdipBitmapFromDIB(BITMAPHEADER header, void* gdibits, out System.IntPtr bitmap);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipCreateBitmapFromHICON" , ExactSpelling = true)]
        public static extern GpStatus CreateGdipBitmapFromHICON(System.IntPtr hicon , out System.IntPtr bitmap);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipCreateHICONFromBitmap" , ExactSpelling = true)]
        public static extern GpStatus GetHICONFromGdipBitmap(System.IntPtr ptr, out System.IntPtr iconhandle);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipCreateHBITMAPFromBitmap" , ExactSpelling = true)]
        public static extern GpStatus GetHBITMAPFromGdipBitmap(System.IntPtr ptr , out System.IntPtr bitmaphandle, System.Int32 background = -2894893);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipImageForceValidation", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern GpStatus ForceGdipBitmapValidation(System.IntPtr image);

        [DllImport(Libraries.GdiPlus, CharSet = CharSet.Unicode, EntryPoint = "GdipDisposeImage", ExactSpelling = true)]
        public static extern GpStatus DisposeGdipBitmap(System.IntPtr image);

        [DllImport(Libraries.GdiPlus , CharSet = CharSet.Unicode , EntryPoint = "GdiplusStartup" , ExactSpelling = true, SetLastError = true)]
        public static extern GpStatus CreateGDIPEnvironment(out System.IntPtr token, in GdipStartupInput si, out GdipStartupOutput so);

        [DllImport(Libraries.GdiPlus, CharSet = CharSet.Unicode , EntryPoint = "GdiplusShutdown" , ExactSpelling = true, SetLastError = true)]
        public static extern void DestroyGDIPEnvironment(IntPtr token);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageWidth" , CharSet = CharSet.Unicode ,  ExactSpelling = true)]
        public static extern GpStatus GetGdipBitmapWidth(System.IntPtr bitmaphandle , out System.UInt32 width);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageHeight" , CharSet = CharSet.Unicode , ExactSpelling = true)]
        public static extern GpStatus GetGdipBitmapHeight(System.IntPtr bitmaphandle , out System.UInt32 height);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageFlags" , CharSet = CharSet.Unicode , ExactSpelling = true)]
        public static extern GpStatus GetGdipBitmapFlags(System.IntPtr bitmaphandle, out System.UInt32 flags);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageRawFormat" , CharSet = CharSet.Unicode , ExactSpelling = true)]
        public static extern GpStatus GetGdipBitmapFormat(System.IntPtr bitmaphandle, out GUID format);

        [DllImport(Libraries.GdiPlus , EntryPoint = "GdipGetImageHorizontalResolution" , CharSet = CharSet.Unicode , ExactSpelling = true)]
        public static extern GpStatus GetGdipBitmapHorizontalResolution(System.IntPtr bitmaphandle, out System.Single hres);

        [DllImport(Libraries.GdiPlus, EntryPoint = "GdipGetImageVerticalResolution", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern GpStatus GetGdipBitmapVerticalResolution(System.IntPtr bitmaphandle, out System.Single vres);
    }

    public static class COM
    {
        [ComImport]
        [Guid("0000000C-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IStream
        {
            int Read([In] IntPtr buf, [In] int len);

            int Write([In] IntPtr buf, [In] int len);

            [return: MarshalAs(UnmanagedType.I8)]
            long Seek([In][MarshalAs(UnmanagedType.I8)] long dlibMove, [In] int dwOrigin);

            void SetSize([In][MarshalAs(UnmanagedType.I8)] long libNewSize);

            [return: MarshalAs(UnmanagedType.I8)]
            long CopyTo([In][MarshalAs(UnmanagedType.Interface)] IStream pstm, [In][MarshalAs(UnmanagedType.I8)] long cb, [Out][MarshalAs(UnmanagedType.LPArray)] long[] pcbRead);

            void Commit([In] int grfCommitFlags);

            void Revert();

            void LockRegion([In][MarshalAs(UnmanagedType.I8)] long libOffset, [In][MarshalAs(UnmanagedType.I8)] long cb, [In] int dwLockType);

            void UnlockRegion([In][MarshalAs(UnmanagedType.I8)] long libOffset, [In][MarshalAs(UnmanagedType.I8)] long cb, [In] int dwLockType);

            void Stat([In] System.IntPtr pstatstg, [In] int grfStatFlag);

            [return: MarshalAs(UnmanagedType.Interface)]
            IStream Clone();
        }

        internal class GPStream : IStream
        {
            protected System.IO.Stream dataStream;

            private long virtualPosition = -1L;

            internal GPStream(System.IO.Stream stream)
            {
                if (stream is null)
                {
                    throw new ArgumentNullException(nameof(stream));
                }
                if (!stream.CanSeek)
                {
                    byte[] array = new byte[256];
                    int num = 0;
                    int num2;
                    do
                    {
                        if (array.Length < num + 256) {
                            byte[] array2 = new byte[array.Length * 2];
                            Array.Copy(array, array2, array.Length);
                            array = array2;
                        }
                        num2 = stream.Read(array, num, 256);
                        num += num2;
                    }
                    while (num2 != 0);
                    dataStream = new System.IO.MemoryStream(array);
                } else {
                    dataStream = stream;
                }
            }

            private void ActualizeVirtualPosition()
            {
                if (virtualPosition != -1)
                {
                    if (virtualPosition > dataStream.Length)
                    {
                        dataStream.SetLength(virtualPosition);
                    }
                    dataStream.Position = virtualPosition;
                    virtualPosition = -1L;
                }
            }

            public virtual IStream Clone()
            {
                NotImplemented();
                return null;
            }

            public virtual void Commit(int grfCommitFlags)
            {
                dataStream.Flush();
                ActualizeVirtualPosition();
            }

            public virtual long CopyTo(IStream pstm, long cb, long[] pcbRead)
            {
                int num = 4096;
                IntPtr intPtr = Marshal.AllocHGlobal(num);
                if (intPtr == IntPtr.Zero)
                {
                    throw new OutOfMemoryException();
                }
                long num2 = 0L;
                try
                {
                    int num4;
                    for (; num2 < cb; num2 += num4)
                    {
                        int num3 = num;
                        if (num2 + num3 > cb)
                        {
                            num3 = (int)(cb - num2);
                        }
                        num4 = Read(intPtr, num3);
                        if (num4 == 0)
                        {
                            break;
                        }
                        if (pstm.Write(intPtr, num4) != num4)
                        {
                            throw EFail("Wrote an incorrect number of bytes");
                        }
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(intPtr);
                }
                if (pcbRead != null && pcbRead.Length != 0)
                {
                    pcbRead[0] = num2;
                }
                return num2;
            }

            public virtual System.IO.Stream GetDataStream() => dataStream;

            public virtual void LockRegion(long libOffset, long cb, int dwLockType) {}

            protected static ExternalException EFail(string msg)
            {
                throw new ExternalException(msg, -2147467259);
            }

            protected static void NotImplemented()
            {
                throw new ExternalException("This method call is not implemented.", -2147467263);
            }

            public virtual int Read(IntPtr buf, int length)
            {
                byte[] array = new byte[length];
                int result = Read(array, length);
                Marshal.Copy(array, 0, buf, length);
                return result;
            }

            public virtual int Read(byte[] buffer, int length)
            {
                ActualizeVirtualPosition();
                return dataStream.Read(buffer, 0, length);
            }

            public virtual void Revert() => NotImplemented();

            public virtual long Seek(long offset, int origin)
            {
                long position = virtualPosition;
                if (virtualPosition == -1)
                {
                    position = dataStream.Position;
                }
                long length = dataStream.Length;
                switch (origin)
                {
                    case 0:
                        if (offset <= length)
                        {
                            dataStream.Position = offset;
                            virtualPosition = -1L;
                        }
                        else
                        {
                            virtualPosition = offset;
                        }
                        break;
                    case 2:
                        if (offset <= 0)
                        {
                            dataStream.Position = length + offset;
                            virtualPosition = -1L;
                        }
                        else
                        {
                            virtualPosition = length + offset;
                        }
                        break;
                    case 1:
                        if (offset + position <= length)
                        {
                            dataStream.Position = position + offset;
                            virtualPosition = -1L;
                        }
                        else
                        {
                            virtualPosition = offset + position;
                        }
                        break;
                }
                if (virtualPosition != -1)
                {
                    return virtualPosition;
                }
                return dataStream.Position;
            }

            public virtual void SetSize(long value) => dataStream.SetLength(value);

            public virtual void Stat(System.IntPtr pstatstg, int grfStatFlag)
            {
                STATSTG sTATSTG = new();
                sTATSTG.cbSize = dataStream.Length;
                sTATSTG.ctime = FILETIME.ToNative(System.DateTime.Now);
                Marshal.StructureToPtr(sTATSTG, pstatstg, pstatstg != IntPtr.Zero);
            }

            public virtual void UnlockRegion(long libOffset, long cb, int dwLockType) {}

            public virtual int Write(IntPtr buf, int length)
            {
                byte[] array = new byte[length];
                Marshal.Copy(buf, array, 0, length);
                return Write(array, length);
            }

            public virtual int Write(byte[] buffer, int length)
            {
                ActualizeVirtualPosition();
                dataStream.Write(buffer, 0, length);
                return length;
            }
        }

        [StructLayout(LayoutKind.Explicit , Size = 76)]
        public struct STATSTG
        {
            //
            // Summary:
            //     Specifies the last access time for this storage, stream, or byte array.
            [FieldOffset(0)]
            public FILETIME atime;
            //
            // Summary:
            //     Specifies the size, in bytes, of the stream or byte array.
            [FieldOffset(8)]
            public long cbSize;
            //
            // Summary:
            //     Indicates the class identifier for the storage object.
            [FieldOffset(16)]
            public GUID clsid;
            //
            // Summary:
            //     Indicates the creation time for this storage, stream, or byte array.
            [FieldOffset(32)]
            public FILETIME ctime;
            //
            // Summary:
            //     Indicates the types of region locking supported by the stream or byte array.
            [FieldOffset(40)]
            public int grfLocksSupported;
            //
            // Summary:
            //     Indicates the access mode that was specified when the object was opened.
            [FieldOffset(44)]
            public int grfMode;
            //
            // Summary:
            //     Indicates the current state bits of the storage object (the value most recently
            //     set by the IStorage::SetStateBits method).
            [FieldOffset(48)]
            public int grfStateBits;
            //
            // Summary:
            //     Indicates the last modification time for this storage, stream, or byte array.
            [FieldOffset(52)]
            public FILETIME mtime;
            //
            // Summary:
            //     Represents a pointer to a null-terminated string containing the name of the object
            //     described by this structure.
            [FieldOffset(60)]
            public System.IntPtr pwcsName;
            //
            // Summary:
            //     Reserved for future use.
            [FieldOffset(68)]
            public int reserved;
            //
            // Summary:
            //     Indicates the type of storage object, which is one of the values from the STGTY
            //     enumeration.
            [FieldOffset(72)]
            public int type;
        }

        [StructLayout(LayoutKind.Explicit , Size = 8)]
        public struct FILETIME
        {
            [FieldOffset(0)]
            public System.Int64 FileTime;
            [FieldOffset(0)]
            public int HighDateTime;
            [FieldOffset(4)]
            public int LowDateTime;

            public System.Int64 ToTicks() => (((System.UInt64)HighDateTime << 32).ToInt64() + LowDateTime);

            public System.DateTime ToDateTime() => System.DateTime.FromFileTimeUtc(FileTime);

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
                if (Compression == ImageType.BI_RGB && BitCount <= 8) {
                    if (ColorIndices == 0) { 
                        result = (System.UInt32)System.Math.Pow(2, BitCount); 
                    } else {
                        result = ColorIndices;
                    }
                // But if these are existing for 16-bit and above , return them.
                } else if (ColorIndices > 0) {
                    result = ColorIndices;
                }
                return result;
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
                // Additionally , if the important color indices is not zero , return that instead.
                if (ImportantIndices > 0 && ColorIndices > 0) {
                    result = ImportantIndices;
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
            // The below code is pretty-much based on multiple articles that specify how color tables are laid out.
            // For a basic grasp you can see https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapinfoheader#color-tables
            get {
                // Size of a single color table. (It is named that way because the original color table structure is named RGBQUAD) 
                const System.Int32 RGBQUADSIZE = 4; 
                System.UInt32 result = 0;
                if (Compression == ImageType.BI_RGB) {
                    result = ColorTablesCount * RGBQUADSIZE;
                } else if (Compression == ImageType.BI_BITFIELDS) {
                    // In this case we have 3 masks , each of which is a DWORD so it is 3 * the size of a System.UInt32.
                    // If we also have color tables , add them too!
                    // It works correctly when ColorTablesCount == 0 because all the expression will evaluate to zero.
                    result = (3 * sizeof(System.UInt32)) + (ColorTablesCount * RGBQUADSIZE);
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
