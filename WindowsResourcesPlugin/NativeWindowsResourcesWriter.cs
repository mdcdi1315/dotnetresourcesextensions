
using System;
using System.Resources;
using DotNetResourcesExtensions.Internal;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Native Windows Resources writer class. <br />
    /// The class can write RC-like resources to a specified stream.
    /// </summary>
    public sealed class NativeWindowsResFilesWriter : IDotNetResourcesExtensionsWriter
    {
        private RESFILEWriter writer;
        private System.IO.Stream stream;
        private System.Boolean strmown;

        /// <summary>
        /// Creates a new instance of <see cref="NativeWindowsResFilesWriter"/> class by using the specified
        /// stream to write the resulting data to.
        /// </summary>
        /// <param name="str">The stream instance to write data to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="str"/> was null.</exception>
        /// <exception cref="ArgumentException">The stream was not writeable.</exception>
        public NativeWindowsResFilesWriter(System.IO.Stream str)
        {
            if (str is null) { throw new ArgumentNullException(nameof(str)); }
            if (str.CanWrite == false) { throw new ArgumentException("The stream given must be a writeable stream." , nameof(str)); }
            strmown = false;
            writer = new(str);
            stream = str;
        }

        /// <summary>
        /// Gets or sets a value whether the <see cref="NativeWindowsResFilesWriter"/> class instance controls the lifetime of the underlying stream.
        /// </summary>
        public System.Boolean IsStreamOwner 
        { 
            get => strmown; 
            set => strmown = value; 
        }

        /// <summary>
        /// Adds a native resource entry to the list of the resources to be written.
        /// </summary>
        /// <param name="entry">The native resource entry to write to the underlying resource stream.</param>
        public void AddResource(NativeWindowsResourceEntry entry) => writer?.Write(entry);

        void IResourceWriter.AddResource(string name, byte[] value)
        {
            throw new NotSupportedException("The NativeWindowsResFilesWriter class does not support writing resource entries on the fly.");
        }

        void IResourceWriter.AddResource(string name, object value)
        {
            if (value is NativeWindowsResourceEntry entry) { AddResource(entry); return; }
#if NET472_OR_GREATER || WINDOWS10_0_19041_0_OR_GREATER
            if (value is System.Drawing.Bitmap bm)
            {
                NativeWindowsResourceEntry ent = new();
                ent.Name = name;
                ent.Culture = System.Globalization.CultureInfo.InvariantCulture;
                ent.NativeType = WindowsResourceEntryType.RT_BITMAP;
                if (bm.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb) {
                    throw new ArgumentException("The image file must be always 32bpp in ARGB color scheme.");
                }
                System.IO.MemoryStream temp = new();
                Interop.BITMAPINFOHEADER bih = new();
                bih.Size = 36;
                bih.ImportantIndices = 0;
                bih.ColorIndices = 0;
                bih.Width = bm.Width;
                bih.Height = bm.Height;
                bih.BitCount = 32;
                bih.Planes = 1;
                bih.ColorIndices = 0;
                bih.ImageSize = 0;
                bih.Compression = Interop.ImageType.BI_PNG;
                bih.HorizontalResolution = (System.Int32)bm.HorizontalResolution;
                bih.VerticalResolution = (System.Int32)bm.VerticalResolution;
                temp.Write(bih.GetBytes(), 0, 36);
                try {
                    bm.Save(temp, System.Drawing.Imaging.ImageFormat.Png);
                } finally {
                    temp?.Dispose();
                    temp = null;
                }
                ent.Value = temp.ToArray();
                temp.Dispose();
                temp = null;
                AddResource(ent);
            }
#endif
            throw new NotSupportedException("The NativeWindowsResFilesWriter class does not support writing resource entries on the fly.");
        }

        void IResourceWriter.AddResource(string name, string value)
        {
            throw new NotSupportedException("The NativeWindowsResFilesWriter class does not support writing resource entries on the fly.");
        }

        /// <summary>
        /// Attempts to close the open underlying stream that this writer uses to write data.
        /// </summary>
        public void Close()
        {
            writer?.Dispose();
            stream?.Close();
        }

        /// <summary>
        /// Disposes this <see cref="NativeWindowsResFilesWriter"/> class instance.
        /// </summary>
        public void Dispose()
        {
            writer?.Dispose();
            writer = null;
            if (strmown) { stream?.Dispose(); }
            stream = null;
        }

        /// <summary>
        /// Ensures that all written resources have been correctly written and saved to the stream.
        /// </summary>
        public void Generate() => stream?.Flush();

        void IUsingCustomFormatter.RegisterTypeResolver(ITypeResolver resolver) {
            throw new NotSupportedException("The NativeWindowsResFilesWriter class does not support using custom data formatters.");
        }
    }
}
