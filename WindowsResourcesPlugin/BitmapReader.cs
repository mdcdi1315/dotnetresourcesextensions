
using System;
using DotNetResourcesExtensions.Internal;

#pragma warning disable CA1416

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Reads bitmap data out-of-the-box for any resource that defines them. <br />
    /// And in select platforms , the reader also exposes a System.Drawing.Bitmap class to read the data.
    /// </summary>
    public sealed class BitmapReader : IDisposable
    {
        private System.Byte[] transformed;

        /// <summary>
        /// Gets a new instance of <see cref="BitmapReader"/> class by reading the specified native entry.
        /// </summary>
        /// <param name="entry">The resource entry to read.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> was null.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property of <paramref name="entry"/> was not set to <see cref="WindowsResourceEntryType.RT_BITMAP"/>.</exception>
        public BitmapReader(NativeWindowsResourceEntry entry)
        {
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            if (entry.NativeType != WindowsResourceEntryType.RT_BITMAP)
            {
                throw new ArgumentException("The entry native type must have been set to RT_BITMAP.");
            }
            transformed = Interop.BITMAPFILEHEADER.CreateBitmap(entry.Value);
        }

        /// <summary>
        /// Gets the transformed bitmap bytes which can be directly saved to a file.
        /// </summary>
        public System.Byte[] BitmapBytes => transformed;

        /// <summary>
        /// Saves the read data to a stream.
        /// </summary>
        /// <param name="stream">The stream to save the data to.</param>
        public void Save(System.IO.Stream stream)
        {
            if (transformed is null)
            {
                throw new ObjectDisposedException(nameof(BitmapReader));
            }
            stream.Write(transformed, 0, transformed.Length);
        }

        /// <summary>
        /// Saves the read data to the specified file on disk. <br />
        /// If the file exists , it will be overwritten.
        /// </summary>
        /// <param name="File">The file to save the data to.</param>
        public void Save(System.String File)
        {
            if (transformed is null) {
                throw new ObjectDisposedException(nameof(BitmapReader));
            }
            System.IO.FileStream FS = null;
            try {
                FS = new(File, System.IO.FileMode.Create);
                FS.Write(transformed, 0, transformed.Length);
            } finally {
                FS?.Dispose();
                FS = null;
            }
        }

#if NET472_OR_GREATER || WINDOWS10_0_19041_0_OR_GREATER
        
        /// <summary>
        /// Gets the image data as a new <see cref="System.Drawing.Bitmap"/> class.
        /// </summary>
        public System.Drawing.Bitmap AsBitmap
        {
            get {
                if (transformed is null)
                {
                    throw new ObjectDisposedException(nameof(BitmapReader));
                }
                System.IO.MemoryStream mem = null;
                try
                {
                    mem = new(transformed);
                    return new(mem);
                } finally {
                    mem?.Dispose();
                    mem = null;
                }
            }
        }

#endif

        /// <summary>
        /// Disposes this class instance.
        /// </summary>
        public void Dispose() {
            transformed = null;
        }
    }

    /// <summary>
    /// Defines the GDI+ Bitmap native class in C#. <br />
    /// A bridge option is also provided to cast this object to a System.Drawing.Bitmap class 
    /// , when the Windows Desktop platform is available on run-time.
    /// </summary>
    public sealed class GdiPlusBitmap : IDisposable
    {
        private System.IntPtr bitmapinst;

        private GdiPlusBitmap() 
        {
            bitmapinst = IntPtr.Zero;
            if (Interop.ApisSupported() == false)
            {
                throw new PlatformNotSupportedException("The GdiPlusBitmap class can be only instantiated from Windows.");
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="GdiPlusBitmap"/> class from the specified native entry. <br />
        /// The entry native type can either be <see cref="WindowsResourceEntryType.RT_BITMAP"/> or
        /// <see cref="WindowsResourceEntryType.RT_ICON"/>.
        /// </summary>
        /// <param name="entry">The native entry to create the GDI+ bitmap from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> was null.</exception>
        /// <exception cref="ArgumentException">The resource entry type was not an icon or a bitmap.</exception>
        public GdiPlusBitmap(NativeWindowsResourceEntry entry) : this()
        {
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            if (entry.NativeType == WindowsResourceEntryType.RT_BITMAP) {
                bitmapinst = Interop.GdiPlus.GDIPManager.Default.GetBitmapHandleFromDIB(entry.Value, 0);
            } else if (entry.NativeType == WindowsResourceEntryType.RT_ICON) {
                SafeIconHandle sih = new(entry);
                try {
                    bitmapinst = Interop.GdiPlus.GDIPManager.Default.GetBitmapHandleFromHICON(sih.DangerousGetHandle());
                } finally {
                    sih?.Dispose();
                    sih = null;
                }
            } else {
                throw new ArgumentException("The resource entry was not either a RT_ICON or a RT_BITMAP. Creation failed.");
            }
            ValidateBitmap();
        }

        /// <summary>
        /// Creates a new instance of <see cref="GdiPlusBitmap"/> class from the specified safe icon handle.
        /// </summary>
        /// <param name="iconhandle">The safe icon handle to create the bitmap from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="iconhandle"/> was null.</exception>
        /// <exception cref="ArgumentException"><paramref name="iconhandle"/> was invalid or a disposed handle.</exception>
        public GdiPlusBitmap(SafeIconHandle iconhandle) : this()
        {
            if (iconhandle is null) { throw new ArgumentNullException(nameof(iconhandle)); }
            if (iconhandle.IsInvalid || iconhandle.IsClosed) {
                throw new ArgumentException("Cannot create a GDI+ bitmap from a invalid or disposed handle.");
            }
            bitmapinst = Interop.GdiPlus.GDIPManager.Default.GetBitmapHandleFromHICON(iconhandle.DangerousGetHandle());
            ValidateBitmap();
        }

        /// <summary>
        /// Creates a new instance of <see cref="GdiPlusBitmap"/> class from the specified bitmap reader.
        /// </summary>
        /// <param name="reader">The bitmap reader to use.</param>
        /// <param name="UseICM">An optional value whether to use ICM (Image Color Management) or not.</param>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> was null.</exception>
        public GdiPlusBitmap(BitmapReader reader , System.Boolean UseICM = false) : this()
        {
            if (reader is null) { throw new ArgumentNullException(nameof(reader)); }
            System.IO.MemoryStream ms = new(reader.BitmapBytes);
            try {
                bitmapinst = Interop.GdiPlus.GDIPManager.Default.GetBitmapHandleFromStream(ms, UseICM);
                ValidateBitmap();
            } finally {
                ms?.Dispose();
                ms = null;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="GdiPlusBitmap"/> class from the specified data stream.
        /// </summary>
        /// <param name="stream">The data stream to read from.</param>
        /// <param name="UseICM">An optional value whether to use ICM (Image Color Management) or not.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> was null.</exception>
        public GdiPlusBitmap(System.IO.Stream stream , System.Boolean UseICM = false) : this()
        {
            if (stream is null) { throw new ArgumentNullException(nameof(stream)); }
            bitmapinst = Interop.GdiPlus.GDIPManager.Default.GetBitmapHandleFromStream(stream, UseICM);
            ValidateBitmap();
        }

        /// <summary>
        /// Gets the equivalent device-independent safe handle for this bitmap object. <br />
        /// Note: Do not call this property unless you need it! 
        /// </summary>
        public SafeDeviceIndependentBitmapHandle SafeHandle
        {
            get {
                if (bitmapinst == IntPtr.Zero) { throw new ObjectDisposedException(nameof(GdiPlusBitmap)); }
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetHBITMAPFromGdipBitmap(bitmapinst, out System.IntPtr hbm));
                return new(hbm);
            }
        }

        /// <summary>
        /// Gets a safe icon handle for this bitmap object. <br />
        /// Note: Do not call this property unless you need it! 
        /// </summary>
        public SafeIconHandle IconHandle
        {
            get {
                if (bitmapinst == IntPtr.Zero) { throw new ObjectDisposedException(nameof(GdiPlusBitmap)); }
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetHICONFromGdipBitmap(bitmapinst, out System.IntPtr hicon));
                return new(hicon);
            }
        }

#if NET472_OR_GREATER || WINDOWS10_0_19041_0_OR_GREATER
        /// <summary>
        /// Gets a <see cref="System.Drawing.Bitmap"/> object that is equal to this bitmap instance.
        /// </summary>
        /// <exception cref="NotImplementedException">The internal implementation required to 
        /// create the bitmap class has been altered or removed , so it is not implemented.</exception>
        public System.Drawing.Bitmap Bitmap
        {
            get {
                if (bitmapinst == IntPtr.Zero) { throw new ObjectDisposedException(nameof(GdiPlusBitmap)); }
                // CreateImageObject seems to exist in all .NET platforms , so use it to get the desired result.
                // NOTE: This uses private reflection and thus the signature of CreateImageObject can be changed at any time without notice.
                foreach (System.Reflection.MethodInfo info in typeof(System.Drawing.Image).GetMember("CreateImageObject", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
                {
                    System.Int32 count = 0;
                    foreach (var param in info.GetParameters()) 
                    {
                        if (param.ParameterType == typeof(System.IntPtr) && count == 0)
                        {
                            try {
                                return (System.Drawing.Bitmap)info.Invoke(null, new System.Object[] { bitmapinst });
                            } catch (System.Reflection.TargetInvocationException tie) {
                                throw tie.InnerException ?? new ArgumentException("Method invokation failed.");
                            }
                        }
                        count++;
                    }
                }
                // If the CreateImageObject has been altered in a .NET release , throw this exception.
                throw new NotImplementedException("The internal implementation might have been altered for this .NET release. " +
                    "Check whether this .NET release has an internal CreateImageObject method in the Image class. " +
                    "If not , then you should file a bug for it.");
            }
        }

        /// <summary>
        /// Casts (actually maps) the current bitmap class to the real <see cref="System.Drawing.Bitmap"/> class.
        /// </summary>
        /// <param name="bm">The bitmap object to map.</param>
        public static explicit operator System.Drawing.Bitmap(GdiPlusBitmap bm) => bm.Bitmap;
#endif

        /// <summary>
        /// Gets the width of this bitmap in pixels.
        /// </summary>
        public System.Int32 Width
        {
            get {
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetGdipBitmapWidth(bitmapinst, out System.UInt32 width));
                return width.ToInt32();
            }
        }

        /// <summary>
        /// Gets the height of this bitmap in pixels.
        /// </summary>
        public System.Int32 Height
        {
            get {
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetGdipBitmapHeight(bitmapinst, out System.UInt32 height));
                return height.ToInt32();
            }
        }

        /// <summary>
        /// Gets the format of this image. This property maps to the <c>RawFormat</c> property of the System.Drawing.Bitmap class.
        /// </summary>
        public System.Guid ImageFormat
        {
            get {
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetGdipBitmapFormat(bitmapinst, out Interop.GUID raw));
                return raw.GetGuid();
            }
        }

        /// <summary>
        /// Gets the image flags for this bitmap.
        /// </summary>
        public System.UInt32 Flags
        {
            get {
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetGdipBitmapFlags(bitmapinst, out System.UInt32 flags));
                return flags;
            }
        }

        /// <summary>
        /// Returns the horizontal resolution, in dots per inch, of this bitmap.
        /// </summary>
        public System.Single HorizontalResolution
        {
            get {
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetGdipBitmapHorizontalResolution(bitmapinst , out System.Single hr));
                return hr;
            }
        }

        /// <summary>
        /// Returns the vertical resolution, in dots per inch, of this bitmap.
        /// </summary>
        public System.Single VerticalResolution
        {
            get {
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetGdipBitmapVerticalResolution(bitmapinst, out System.Single vr));
                return vr;
            }
        }

        private void ValidateBitmap()
        {
            Interop.GpStatus st = Interop.GdiPlus.ForceGdipBitmapValidation(bitmapinst);
            if (st != Interop.GpStatus.Ok) {
                Dispose();
                Interop.GdiPlus.StatusToExceptionMarshaller(st);
            }
        }

        /// <summary>
        /// Disposes this <see cref="GdiPlusBitmap"/> instance.
        /// </summary>
        public void Dispose()
        {
            if (bitmapinst != IntPtr.Zero && Interop.GdiPlus.GDIPManager.Default.RemoveBitmapHandle(bitmapinst))
            {
                System.Diagnostics.Debug.Fail($"Cannot find the GDI+ handle with value {bitmapinst.ToInt64():x2}.");
            }
            bitmapinst = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets a string that describes the current bitmap instance and state.
        /// </summary>
        public override string ToString() => $"GdiPlusBitmap {{ Flags={Flags} , ImageFormat={ImageFormat} , HorizontalResolution={HorizontalResolution} , VerticalResolution={VerticalResolution} , Width={Width} , Height={Height} }}";

        /// <summary>
        /// Disposes immediately this instance if it falls out of scope.
        /// </summary>
        ~GdiPlusBitmap() => Dispose();
    }

}
