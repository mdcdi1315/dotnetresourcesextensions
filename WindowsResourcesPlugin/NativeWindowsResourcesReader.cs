using System;
using System.IO;
using System.Collections;
using DotNetResourcesExtensions.Win32Resources;
using DotNetResourcesExtensions.Internal.AssemblyReader;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Native Windows Resources reader class. <br />
    /// These resources can be only be acquired from any Windows and .NET executable containing valid 
    /// Win32 resources files. <br />
    /// Be noted that this class can work not only on Windows , but also on every single supported platform that .NET currently supports!
    /// </summary>
    public sealed class NativeWindowsResourcesReader : IDotNetResourcesExtensionsReader
    {
        private enum StreamOpenMode : System.Byte { Unknown , WithFile , Stream , StreamAndDispose }

        private PEReader rdr;
        private Stream backstream;
        private ResourceData data;
        private StreamOpenMode mode;

        private NativeWindowsResourcesReader()
        {
            mode = StreamOpenMode.Unknown;
            data = null;
            backstream = null;
            rdr = null;
        }

        /// <summary>
        /// Creates a new <see cref="NativeWindowsResourcesReader"/> class from the specified data stream that contains a valid PE image.
        /// </summary>
        /// <param name="stream">The stream to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> was null.</exception>
        public NativeWindowsResourcesReader(System.IO.Stream stream) : this()
        {
            if (stream is null) { throw new ArgumentNullException(nameof(stream)); }
            backstream = stream;
            mode = StreamOpenMode.Stream;
            Initialize();
        }

        /// <summary>
        /// Creates a new <see cref="NativeWindowsResourcesReader"/> class from the specified PE file path.
        /// </summary>
        /// <param name="file">The PE File path to use to read for resources.</param>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> was null or empty.</exception>
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="file"/> does not define a valid path.</exception>
        public NativeWindowsResourcesReader(System.String file) : this()
        {
            if (System.String.IsNullOrEmpty(file)) {
                throw new ArgumentNullException(nameof(file));
            }
            if (System.IO.File.Exists(file) == false) {
                throw new System.IO.FileNotFoundException("The system cannot find the image specified so as to be loaded." , file);
            }
            backstream = new System.IO.FileStream(file , FileMode.Open);
            mode = StreamOpenMode.WithFile;
            Initialize();
        }

        /// <summary>
        /// Creates a new <see cref="NativeWindowsResourcesReader"/> class from the specified file information that is a PE image.
        /// </summary>
        /// <param name="info">The file information to use so as to read the PE file for resources.</param>
        public NativeWindowsResourcesReader(System.IO.FileInfo info) : this(info.OpenRead()) 
        {
            mode = StreamOpenMode.WithFile;
        }

        private void Initialize()
        {
            // The stream will be elsewise managed by the class , no need to be handled by PEReader.
            try {
                if (backstream.CanRead == false) { throw new System.IO.IOException("The stream given must be readable stream."); }
                rdr = new(backstream, PEStreamOptions.LeaveOpen);
                data = new(rdr);
                if (data.IsEmpty) { throw new NoNativeResourcesFoundException(); }
            } catch {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Gets or sets a value whether the <see cref="NativeWindowsResourcesReader"/> controls the lifetime of the underlying stream.
        /// </summary>
        public System.Boolean IsStreamOwner
        {
            get {
                if (mode == StreamOpenMode.WithFile) { return true; }
                return mode == StreamOpenMode.StreamAndDispose;
            }
            set {
                if (mode == StreamOpenMode.WithFile) { return; }
                if (value)
                {
                    mode = StreamOpenMode.StreamAndDispose;
                } else {
                    mode = StreamOpenMode.Stream;
                }
            }
        }

        /// <inheritdoc />
        public void Close()
        {
            data = null;
        }

        /// <summary>
        /// Disposes all associated information used by this instance.
        /// </summary>
        public void Dispose()
        {
            if (rdr is not null)
            {
                rdr.Dispose();
                rdr = null;
            }
            if (IsStreamOwner) 
            {
                backstream?.Dispose();
                backstream = null;
            }
            data = null;
        }

        /// <inheritdoc cref="System.Resources.IResourceReader.GetEnumerator"/>
        public NativeWindowsResourcesEnumerator GetEnumerator()
        {
            if (data is null) { throw new ObjectDisposedException(nameof(NativeWindowsResourcesReader)); }
            return new(data.GetAllResources());
        }

        IDictionaryEnumerator System.Resources.IResourceReader.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void IUsingCustomFormatter.RegisterTypeResolver(ITypeResolver resolver)
        {
            throw new NotSupportedException("Currently this method is not supported for this kind of reader.");
        }
    }

    /// <summary>
    /// Native Windows Resources reader class. <br />
    /// This class variant is able to read any .RES file produced by the RC tool or any other compatible tool too. <br />
    /// Also platform-independent.
    /// </summary>
    public sealed class NativeWindowsResFilesReader : IDotNetResourcesExtensionsReader
    {
        private enum StreamOpenMode : System.Byte { Unknown, WithFile, Stream, StreamAndDispose }

        private System.Int64 sp;
        private System.IO.Stream strm;
        private StreamOpenMode mode;

        private NativeWindowsResFilesReader()
        {
            strm = null;
            mode = StreamOpenMode.Unknown;
        }

        /// <summary>
        /// Creates a new <see cref="NativeWindowsResFilesReader"/> class from the specified RES file path.
        /// </summary>
        /// <param name="file">The RES File path to use to read for resources.</param>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> was null or empty.</exception>
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="file"/> does not define a valid path.</exception>
        public NativeWindowsResFilesReader(System.String file) : this()
        {
            if (System.String.IsNullOrEmpty(file)) {
                throw new ArgumentNullException(nameof(file));
            }
            if (System.IO.File.Exists(file) == false) {
                throw new System.IO.FileNotFoundException("The system cannot find the image specified so as to be loaded.", file);
            }
            strm = new System.IO.FileStream(file, FileMode.Open);
            mode = StreamOpenMode.WithFile;
            Initialize();
        }

        /// <summary>
        /// Creates a new <see cref="NativeWindowsResFilesReader"/> class from the specified file information that is a RES file.
        /// </summary>
        /// <param name="info">The file information to use so as to read the RES file for resources.</param>
        public NativeWindowsResFilesReader(System.IO.FileInfo info) : this(info.OpenRead())
        {
            mode = StreamOpenMode.WithFile;
        }

        /// <summary>
        /// Creates a new <see cref="NativeWindowsResFilesReader"/> class from the specified data stream that contains valid RC RES data.
        /// </summary>
        /// <param name="stream">The stream to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> was null.</exception>
        public NativeWindowsResFilesReader(System.IO.Stream stream) : this()
        {
            if (stream is null) { throw new ArgumentNullException(nameof(stream)); }
            strm = stream;
            mode = StreamOpenMode.Stream;
            Initialize();
        }

        private void Initialize()
        {
            try {
                if (strm.CanRead == false) { throw new System.IO.IOException("The stream given must be readable stream."); }
                // At least 64 bytes must exist on the stream to consider it a valid .RES stream.
                if (strm.Length < 64) { throw new NoNativeResourcesFoundException(); }
                sp = strm.Position;
            } catch {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Gets or sets a value whether this instance controls or not the lifetime of the opened stream.
        /// </summary>
        public System.Boolean IsStreamOwner
        {
            get
            {
                if (mode == StreamOpenMode.WithFile) { return true; }
                return mode == StreamOpenMode.StreamAndDispose;
            }
            set
            {
                if (mode == StreamOpenMode.WithFile) { return; }
                if (value)
                {
                    mode = StreamOpenMode.StreamAndDispose;
                }
                else
                {
                    mode = StreamOpenMode.Stream;
                }
            }
        }

        /// <summary>Closes and invalidates this reader.</summary>
        public void Close()
        {
            if (IsStreamOwner) { strm?.Close(); }
        }

        /// <summary>
        /// Disposes all the allocated data used by this class instance.
        /// </summary>
        public void Dispose()
        {
            if (IsStreamOwner) { strm?.Dispose(); }
            strm = null;
        }

        /// <summary>
        /// Returns an enumerator that can enumerate all the resource entries of the given stream.
        /// </summary>
        /// <returns>A new enumerator instance that is able to enumerate the entries of the current resource stream.</returns>
        public NativeWindowsResFilesEnumerator GetEnumerator() => new(strm , sp);

        IDictionaryEnumerator System.Resources.IResourceReader.GetEnumerator() => GetEnumerator();

        void IUsingCustomFormatter.RegisterTypeResolver(ITypeResolver resolver)
        {
            throw new NotSupportedException("Currently this method is not supported for this kind of reader.");
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
