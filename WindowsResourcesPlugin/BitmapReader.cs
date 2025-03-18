
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
                default:
                    throw new ArgumentException("The entry native type must have been set to RT_BITMAP.");
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
}
