using System;
using System.Runtime.InteropServices;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Specifies the class for managing the icon handle creation and disposal. <br />
    /// The icon handle provided can be then subsequently used in System.Drawing.Icon.FromHandle method , which will give you the .NET Icon class. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class SafeIconHandle : SafeHandle , ICloneable
    {
        private SafeIconHandle() : base(IntPtr.Zero , true) { }

        private SafeIconHandle(System.IntPtr handle) : this() { this.handle = handle; }

        /// <summary>
        /// Creates a new instance of the <see cref="SafeIconHandle"/> class by using the specified native entry that contains the icon.
        /// </summary>
        /// <param name="entry">The entry to get the icon handle from.</param>
        /// <exception cref="PlatformNotSupportedException">This API was invoked in other platform than Windows.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> was null.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property of <paramref name="entry"/> was not <see cref="WindowsResourceEntryType.RT_ICON"/>.</exception>
        public unsafe SafeIconHandle(NativeWindowsResourceEntry entry) : this()
        {
            if (Interop.ApisSupported() == false)
            {
                throw new PlatformNotSupportedException("The SafeIconHandle class can be only instantiated from Windows.");
            }
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            if (entry.NativeType != WindowsResourceEntryType.RT_ICON)
            {
                throw new ArgumentException("The entry's native type must be RT_ICON.");
            }
            fixed (System.Byte* ptr = entry.Value)
            {
                handle = Interop.User32.CreateIconOrCursorExtended(ptr, 
                    (System.UInt32)entry.Value.Length, 
                    Interop.BOOL.TRUE,
                    0x00030000, 0, 0, 
                    Interop.IconCreationFlags.LR_DEFAULTCOLOR);
            }
        }

        /// <summary>
        /// Clones the current icon handle to a new instance of <see cref="SafeIconHandle"/>. <br />
        /// Although that the handle will be different than the current , they both represent the same icon.
        /// </summary>
        /// <returns>A cloned instance of this instance.</returns>
        /// <exception cref="InvalidOperationException">The handle was invalid.</exception>
        /// <exception cref="ObjectDisposedException">The handle was disposed. Disposed handles cannot be copied.</exception>
        public SafeIconHandle Clone()
        {
            if (IsInvalid) { throw new InvalidOperationException("Cannot copy a handle which is invalid."); }
            if (IsClosed) { throw new ObjectDisposedException(nameof(SafeIconHandle), "Cannot copy from a disposed handle."); }
            System.IntPtr copy = Interop.User32.CopyImage(handle , Interop.ImageCopyType.IMAGE_ICON , 0 , 0 , Interop.ImageCopyFlags.LR_DEFAULTCOLOR);
            if (copy == IntPtr.Zero) { Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error()); }
            return new SafeIconHandle(copy);
        }

        System.Object ICloneable.Clone() => Clone();

        /// <summary>
        /// A <see cref="SafeIconHandle"/> is invalid when it's handle value is zero.
        /// </summary>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <summary>
        /// Releases the icon handle.
        /// </summary>
        protected override bool ReleaseHandle() => Interop.User32.DestroyIcon(handle) != Interop.BOOL.FALSE;
    }

    /// <summary>
    /// Specifies the class for managing the cursor handle creation and disposal. <br />
    /// The cursor handle provided can be then subsequently used in System.Windows.Forms.Cursor.Cursor(System.IntPtr) constructor , which will give you the .NET Cursor class. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class SafeCursorHandle : SafeHandle , ICloneable
    {
        private SafeCursorHandle() : base(IntPtr.Zero , true) { }

        private SafeCursorHandle(System.IntPtr copied) : this() { handle = copied; }

        /// <summary>
        /// Creates a new instance of the <see cref="SafeCursorHandle"/> class by using the specified native entry that contains the cursor.
        /// </summary>
        /// <param name="entry">The entry to get the cursor handle from.</param>
        /// <exception cref="PlatformNotSupportedException">This API was invoked in other platform than Windows.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> was null.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property of <paramref name="entry"/> was not <see cref="WindowsResourceEntryType.RT_CURSOR"/>.</exception>
        public unsafe SafeCursorHandle(NativeWindowsResourceEntry entry) : this()
        {
            if (Interop.ApisSupported() == false)
            {
                throw new PlatformNotSupportedException("The SafeCursorHandle class can be only instantiated from Windows.");
            }
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            if (entry.NativeType != WindowsResourceEntryType.RT_CURSOR)
            {
                throw new ArgumentException("The entry's native type must be RT_CURSOR.");
            }
            fixed (System.Byte* ptr = entry.Value)
            {
                handle = Interop.User32.CreateIconOrCursorExtended(ptr,
                    (System.UInt32)entry.Value.Length,
                    Interop.BOOL.FALSE,
                    0x00030000, 0, 0,
                    Interop.IconCreationFlags.LR_DEFAULTCOLOR);
            }
        }

        /// <summary>
        /// Clones the current cursor handle to a new instance of <see cref="SafeCursorHandle"/>. <br />
        /// Although that the handle will be different than the current , they both represent the same cursor.
        /// </summary>
        /// <returns>A cloned instance of this instance.</returns>
        /// <exception cref="InvalidOperationException">The handle was invalid.</exception>
        /// <exception cref="ObjectDisposedException">The handle was disposed. Disposed handles cannot be copied.</exception>
        public SafeCursorHandle Clone()
        {
            if (IsInvalid) { throw new InvalidOperationException("Cannot copy a handle which is invalid."); }
            if (IsClosed) { throw new ObjectDisposedException(nameof(SafeCursorHandle), "Cannot copy from a disposed handle."); }
            System.IntPtr copy = Interop.User32.CopyImage(handle, Interop.ImageCopyType.IMAGE_CURSOR, 0, 0, Interop.ImageCopyFlags.LR_DEFAULTCOLOR);
            if (copy == IntPtr.Zero) { Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error()); }
            return new SafeCursorHandle(copy);
        }

        System.Object ICloneable.Clone() => Clone();

        /// <summary>
        /// A <see cref="SafeCursorHandle"/> is invalid when it's handle value is zero.
        /// </summary>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <summary>
        /// Releases the cursor handle.
        /// </summary>
        protected override bool ReleaseHandle() => Interop.User32.DestroyCursor(handle) != Interop.BOOL.FALSE;
    }

    /// <summary>
    /// Specifies the class for managing the device-dependent bitmap creation and disposal. <br />
    /// Unlike the other safe handles , this handle cannot be used in System.Drawing.Bitmap.FromHbitmap(System.IntPtr) method , because
    /// you need to pass also there a HDC handle and the class does not support such case. <br />
    /// If you want to use the resource with the System.Drawing.Bitmap class , use either the <see cref="BitmapReader"/> or the <see cref="SafeDeviceIndependentBitmapHandle"/> classes. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class SafeDeviceDependentBitmapHandle : SafeHandle , ICloneable
    {
        private SafeDeviceDependentBitmapHandle() : base(IntPtr.Zero , true) { }

        private SafeDeviceDependentBitmapHandle(System.IntPtr copy) : this() { handle = copy; }

        /// <summary>
        /// Creates a new instance of the <see cref="SafeDeviceDependentBitmapHandle"/> class by using the specified native entry that contains the bitmap to load.
        /// </summary>
        /// <param name="entry">The native entry to load the bitmap from.</param>
        /// <exception cref="PlatformNotSupportedException">This API was invoked in other platform than Windows.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> was null.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property of <paramref name="entry"/> was not <see cref="WindowsResourceEntryType.RT_BITMAP"/>.</exception>
        public unsafe SafeDeviceDependentBitmapHandle(NativeWindowsResourceEntry entry) : this()
        {
            if (Interop.ApisSupported() == false)
            {
                throw new PlatformNotSupportedException("The SafeDeviceDependentBitmapHandle class can be only instantiated from Windows.");
            }
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            if (entry.NativeType != WindowsResourceEntryType.RT_BITMAP)
            {
                throw new ArgumentException("The entry's native type must be RT_BITMAP.");
            }
            handle = Interop.Gdi32.LoadBitmap(entry.Value);
        }

        /// <summary>
        /// Clones the current bitmap handle to a new instance of <see cref="SafeDeviceDependentBitmapHandle"/>. <br />
        /// Although that the handle will be different than the current , they both represent the same bitmap.
        /// </summary>
        /// <returns>A cloned instance of this instance.</returns>
        /// <exception cref="InvalidOperationException">The handle was invalid.</exception>
        /// <exception cref="ObjectDisposedException">The handle was disposed. Disposed handles cannot be copied.</exception>
        public SafeDeviceDependentBitmapHandle Clone()
        {
            if (IsInvalid) { throw new InvalidOperationException("Cannot copy a handle which is invalid."); }
            if (IsClosed) { throw new ObjectDisposedException(nameof(SafeDeviceDependentBitmapHandle), "Cannot copy from a disposed handle."); }
            System.IntPtr copy = Interop.User32.CopyImage(handle, Interop.ImageCopyType.IMAGE_BITMAP, 0, 0, Interop.ImageCopyFlags.LR_DEFAULTCOLOR);
            if (copy == IntPtr.Zero) { Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error()); }
            return new SafeDeviceDependentBitmapHandle(copy);
        }

        System.Object ICloneable.Clone() => Clone();

        /// <summary>
        /// A <see cref="SafeDeviceDependentBitmapHandle"/> is invalid when it's handle value is zero.
        /// </summary>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <summary>
        /// Releases the device-dependent bitmap handle.
        /// </summary>
        protected override bool ReleaseHandle() => Interop.Gdi32.DeleteObject(handle) != Interop.BOOL.FALSE;
    }

    /// <summary>
    /// Specifies the class for managing the device-independent bitmap creation and disposal. <br />
    /// Unlike the <see cref="SafeDeviceDependentBitmapHandle"/> class , this handle can be used in System.Drawing.Bitmap.FromHbitmap(System.IntPtr) method. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class SafeDeviceIndependentBitmapHandle : SafeHandle , ICloneable
    {
        private SafeDeviceIndependentBitmapHandle() : base(IntPtr.Zero, true) { }

        private SafeDeviceIndependentBitmapHandle(System.IntPtr copy) : this() { handle = copy; }

        /// <summary>
        /// Creates a new instance of the <see cref="SafeDeviceIndependentBitmapHandle"/> class by using the specified native entry that contains the bitmap to load.
        /// </summary>
        /// <param name="entry">The native entry to load the bitmap from.</param>
        /// <exception cref="PlatformNotSupportedException">This API was invoked in other platform than Windows.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> was null.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property of <paramref name="entry"/> was not <see cref="WindowsResourceEntryType.RT_BITMAP"/>.</exception>
        public unsafe SafeDeviceIndependentBitmapHandle(NativeWindowsResourceEntry entry) : this()
        {
            if (Interop.ApisSupported() == false)
            {
                throw new PlatformNotSupportedException("The SafeDeviceIndependentBitmapHandle class can be only instantiated from Windows.");
            }
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            if (entry.NativeType != WindowsResourceEntryType.RT_BITMAP)
            {
                throw new ArgumentException("The entry's native type must be RT_BITMAP.");
            }
            handle = Interop.Gdi32.LoadDIBitmap(entry.Value);
            if (IsInvalid) { Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error()); }
        }

        /// <summary>
        /// Clones the current bitmap handle to a new instance of <see cref="SafeDeviceIndependentBitmapHandle"/>. <br />
        /// Although that the handle will be different than the current , they both represent the same bitmap.
        /// </summary>
        /// <returns>A cloned instance of this instance.</returns>
        /// <exception cref="InvalidOperationException">The handle was invalid.</exception>
        /// <exception cref="ObjectDisposedException">The handle was disposed. Disposed handles cannot be copied.</exception>
        public SafeDeviceIndependentBitmapHandle Clone()
        {
            if (IsInvalid) { throw new InvalidOperationException("Cannot copy a handle which is invalid."); }
            if (IsClosed) { throw new ObjectDisposedException(nameof(SafeDeviceIndependentBitmapHandle), "Cannot copy from a disposed handle."); }
            System.IntPtr copy = Interop.User32.CopyImage(handle, Interop.ImageCopyType.IMAGE_BITMAP, 0, 0, Interop.ImageCopyFlags.LR_DEFAULTCOLOR | Interop.ImageCopyFlags.LR_CREATEDIBSECTION);
            if (copy == IntPtr.Zero) { Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error()); }
            return new SafeDeviceIndependentBitmapHandle(copy);
        }

        System.Object ICloneable.Clone() => Clone();

        /// <summary>
        /// A <see cref="SafeDeviceIndependentBitmapHandle"/> is invalid when it's handle value is zero.
        /// </summary>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <summary>
        /// Releases the device-dependent bitmap handle.
        /// </summary>
        protected override bool ReleaseHandle() => Interop.Gdi32.DeleteObject(handle) != Interop.BOOL.FALSE;
    }
}
