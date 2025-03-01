﻿
using System;
using DotNetResourcesExtensions.Internal;

#pragma warning disable CA1416

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Reads bitmap data out-of-the-box for any resource that defines them. <br />
    /// Additionally , it converts cursors to bitmaps if these are passed to the constructor. <br />
    /// And in select platforms , the reader also exposes a System.Drawing.Bitmap class to read the data. <br />
    /// Note that always the resulting data will be a bitmap!
    /// </summary>
    public sealed class BitmapReader : IDisposable
    {
        private System.Byte[] transformed;

        /// <summary>
        /// Gets a new instance of <see cref="BitmapReader"/> class by reading the specified native entry.
        /// </summary>
        /// <param name="entry">The resource entry to read bitmap data from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> was null.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property of 
        /// <paramref name="entry"/> was not set to <see cref="WindowsResourceEntryType.RT_BITMAP"/> or 
        /// <see cref="WindowsResourceEntryType.RT_CURSOR"/>.</exception>
        public BitmapReader(NativeWindowsResourceEntry entry)
        {
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            switch (entry.NativeType)
            {
                case WindowsResourceEntryType.RT_BITMAP:
                    transformed = Interop.BITMAPFILEHEADER.CreateBitmap(entry.Value);
                    break;
                case WindowsResourceEntryType.RT_CURSOR:
                    transformed = Interop.BITMAPFILEHEADER.CreateCursor(entry.Value);
                    break;
                default:
                    throw new ArgumentException("The entry native type must have been set either to RT_BITMAP or RT_CURSOR.");
            }
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
        public void Dispose() { transformed = null; }

        /// <summary>Returns the name of this type.</summary>
        public override string ToString() => "BitmapReader";
    }

    /// <summary>
    /// Defines the GDI+ Bitmap native class in C#. <br />
    /// A bridge option is also provided to cast this object to a System.Drawing.Bitmap class 
    /// , when the Windows Desktop platform is available on run-time.
    /// </summary>
    public sealed class GdiPlusBitmap : IDisposable , ICloneable
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
        /// <exception cref="PlatformNotSupportedException">This API was invoked in other platform than Windows.</exception>
        public GdiPlusBitmap(NativeWindowsResourceEntry entry) : this()
        {
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            if (entry.NativeType == WindowsResourceEntryType.RT_BITMAP) {
                bitmapinst = Interop.GdiPlus.GDIPManager.Default.GetBitmapHandleFromDIB(entry.Value, 0);
            } else if (entry.NativeType == WindowsResourceEntryType.RT_ICON) {
                SafeIconHandle sih = new(entry);
                if (sih.IsInvalid || sih.IsClosed) {
                    throw new ArgumentException("Cannot create a GDI+ bitmap from a invalid or disposed handle.");
                }
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
        /// <exception cref="PlatformNotSupportedException">This API was invoked in other platform than Windows.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> was null.</exception>
        /// <exception cref="PlatformNotSupportedException">This API was invoked in other platform than Windows.</exception>
        public GdiPlusBitmap(BitmapReader reader) : this() {
            if (reader is null) { throw new ArgumentNullException(nameof(reader)); }
            bitmapinst = Interop.GdiPlus.GDIPManager.Default.GetBitmapHandleFromDIB(reader.BitmapBytes, 14);
            ValidateBitmap();
        }

        /// <summary>
        /// Creates a new instance of <see cref="GdiPlusBitmap"/> class from the specified data stream.
        /// </summary>
        /// <param name="stream">The data stream to read from.</param>
        /// <param name="UseICM">An optional value whether to use ICM (Image Color Management) or not.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> was null.</exception>
        [System.Obsolete("Stream constructor does not work well and is obsoleted." , true)]
        public GdiPlusBitmap(System.IO.Stream stream , System.Boolean UseICM = false) : this()
        {
            throw new NotSupportedException("This constructor overload does not work well with all streams and it is deprecated.");
        }

        /// <summary>
        /// Gets the equivalent device-independent safe handle for this bitmap object. <br />
        /// Note: Do not call this property unless you need it! 
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
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
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
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
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        /// <exception cref="NotImplementedException">The internal implementation required to 
        /// create the bitmap class has been altered or removed , so it is not implemented.</exception>
        /// <exception cref="AggregateException">The reflected call failed , see the inner exceptions for more information.</exception>
        public System.Drawing.Bitmap Bitmap
        {
            [System.Security.SecurityCritical]
            get {
                if (bitmapinst == IntPtr.Zero) { throw new ObjectDisposedException(nameof(GdiPlusBitmap)); }
                // So cloning the image will avoid the past issues existing with access violation exceptions.
                System.IntPtr instance;
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.CloneGdipBitmap(bitmapinst, out instance));
                // CreateImageObject seems to exist in all .NET platforms , so use it to get the desired result.
                // NOTE: This uses private reflection and thus the signature of CreateImageObject can be changed at any time without notice.
                try {
                    foreach (System.Reflection.MethodInfo info in typeof(System.Drawing.Image).GetMember(
                        "CreateImageObject", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
                    {
                        System.Int32 count = 0;
                        foreach (var param in info.GetParameters())
                        {
                            if (param.ParameterType == typeof(System.IntPtr) && count == 0)
                            {
                                try {
                                    // Now we do not care about disposal issues since the handle will be explicitly handled by the created instance.
                                    return (System.Drawing.Bitmap)info.Invoke(null, new System.Object[] { instance });
                                } catch (System.Reflection.TargetInvocationException tie) {
                                    // Dispose the cloned image if an error occured.
                                    Interop.GdiPlus.DisposeGdipBitmap(instance);
                                    throw new AggregateException("Internal Error occured while doing reflection. The bitmap creation failed.", tie.InnerException);
                                }
                            }
                            count++;
                        }
                    }
                } catch (System.InvalidCastException)  {
                    // Dispose the cloned image if a casting error occured.
                    Interop.GdiPlus.DisposeGdipBitmap(instance);
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
        public static explicit operator System.Drawing.Bitmap(GdiPlusBitmap bm)
        {
            try {
                return bm.Bitmap;
            } catch (NotImplementedException e) {
                throw new InvalidCastException("Invalid attempt to cast to a Bitmap class." , e);
            } catch (ObjectDisposedException) { throw; }
        }
#endif

        /// <summary>
        /// Clones this GDI+ bitmap instance to a newly created instance.
        /// </summary>
        /// <returns>The cloned result that is created from this bitmap instance.</returns>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public GdiPlusBitmap Clone()
        {
            if (bitmapinst == IntPtr.Zero) { throw new ObjectDisposedException(nameof(GdiPlusBitmap)); }
            GdiPlusBitmap result = new();
            Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.CloneGdipBitmap(bitmapinst, out result.bitmapinst));
            return result;
        }

        System.Object ICloneable.Clone() => Clone();

        /// <summary>
        /// Gets the width of this bitmap in pixels.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public System.Int32 Width
        {
            get {
                if (bitmapinst == IntPtr.Zero) { throw new ObjectDisposedException(nameof(GdiPlusBitmap)); }
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetGdipBitmapWidth(bitmapinst, out System.UInt32 width));
                return width.ToInt32();
            }
        }

        /// <summary>
        /// Gets the height of this bitmap in pixels.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public System.Int32 Height
        {
            get {
                if (bitmapinst == IntPtr.Zero) { throw new ObjectDisposedException(nameof(GdiPlusBitmap)); }
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetGdipBitmapHeight(bitmapinst, out System.UInt32 height));
                return height.ToInt32();
            }
        }

        /// <summary>
        /// Gets the format of this bitmap. This property maps to the <c>RawFormat</c> property of the System.Drawing.Bitmap class.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public System.Guid ImageFormat
        {
            get {
                if (bitmapinst == IntPtr.Zero) { throw new ObjectDisposedException(nameof(GdiPlusBitmap)); }
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetGdipBitmapFormat(bitmapinst, out Interop.GUID raw));
                return raw.GetGuid();
            }
        }

        /// <summary>
        /// Gets the flags for this bitmap.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public System.UInt32 Flags
        {
            get {
                if (bitmapinst == IntPtr.Zero) { throw new ObjectDisposedException(nameof(GdiPlusBitmap)); }
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetGdipBitmapFlags(bitmapinst, out System.UInt32 flags));
                return flags;
            }
        }

        /// <summary>
        /// Returns the horizontal resolution, in dots per inch, of this bitmap.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public System.Single HorizontalResolution
        {
            get {
                if (bitmapinst == IntPtr.Zero) { throw new ObjectDisposedException(nameof(GdiPlusBitmap)); }
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetGdipBitmapHorizontalResolution(bitmapinst , out System.Single hr));
                return hr;
            }
        }

        /// <summary>
        /// Returns the vertical resolution, in dots per inch, of this bitmap.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public System.Single VerticalResolution
        {
            get {
                if (bitmapinst == IntPtr.Zero) { throw new ObjectDisposedException(nameof(GdiPlusBitmap)); }
                Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetGdipBitmapVerticalResolution(bitmapinst, out System.Single vr));
                return vr;
            }
        }

        /// <summary>
        /// Returns the operating system handle that holds this bitmap.
        /// </summary>
        public System.IntPtr Handle => bitmapinst;

        /// <summary>
        /// Gets an ARGB pixel from the specified coordinates.
        /// </summary>
        /// <param name="X">The X-coordinate of the bitmap to retrieve the pixel.</param>
        /// <param name="Y">The Y-coordinate of the bitmap to retrieve the pixel.</param>
        /// <returns>The ARGB pixel encoded as a <see cref="System.UInt32"/>.</returns>
        /// <exception cref="ObjectDisposedException">The instance is disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="X"/> or/and <paramref name="Y"/> are out of the image bounds.</exception>
        public System.UInt32 GetPixel(System.Int32 X , System.Int32 Y)
        {
            if (bitmapinst == IntPtr.Zero) { throw new ObjectDisposedException(nameof(GdiPlusBitmap)); }
            if (X < 0 || Y < 0 || X >= Width || Y >= Height)
            {
                throw new ArgumentOutOfRangeException("X and Y parameters must be zero-index numbers with upper bound their widths and heights.");
            }
            Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.GetGdipBitmapPixel(bitmapinst, X, Y, out System.UInt32 ret));
            return ret;
        }

        /// <summary>
        /// Sets the specified pixel on the image on the specified coordinates.
        /// </summary>
        /// <param name="X">The X-coordinate of the bitmap to retrieve the pixel.</param>
        /// <param name="Y">The Y-coordinate of the bitmap to retrieve the pixel.</param>
        /// <param name="Color">The color to set on the specified coordinates.</param>
        /// <exception cref="ObjectDisposedException">The instance is disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="X"/> or/and <paramref name="Y"/> are out of the image bounds.</exception>
        public void SetPixel(System.Int32 X , System.Int32 Y , System.UInt32 Color)
        {
            if (bitmapinst == IntPtr.Zero) { throw new ObjectDisposedException(nameof(GdiPlusBitmap)); }
            if (X < 0 || Y < 0 || X >= Width || Y >= Height)
            {
                throw new ArgumentOutOfRangeException("X and Y parameters must be zero-index numbers with upper bound their widths and heights.");
            }
            Interop.GdiPlus.StatusToExceptionMarshaller(Interop.GdiPlus.SetGdipBitmapPixel(bitmapinst, X, Y, Color));
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
        /// <exception cref="ObjectDisposedException">This instance is disposed. The instance data are required to form the information.</exception>
        public override string ToString() => $"GdiPlusBitmap {{ Flags={Flags} , Handle={Handle} , ImageFormat={ImageFormat} , HorizontalResolution={HorizontalResolution} , VerticalResolution={VerticalResolution} , Width={Width} , Height={Height} }}";

        /// <summary>
        /// Disposes immediately this instance if it falls out of scope.
        /// </summary>
        ~GdiPlusBitmap() => Dispose();
    }

}
