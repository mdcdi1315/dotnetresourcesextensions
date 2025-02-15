
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DotNetResourcesExtensions.Internal;

namespace DotNetResourcesExtensions.Localization
{
    [StructLayout(LayoutKind.Explicit , Size = 6 , Pack = 1)]
    internal struct LOCALIZATIONENTRY
    {
        [FieldOffset(0)]
        public System.Byte Version;

        [FieldOffset(1)]
        public LocalizationSpecialTypeFlags Flags;

        [FieldOffset(2)]
        public System.Int32 LCID;

        public LOCALIZATIONENTRY()
        {
            Version = 1;
            Flags = 0;
            LCID = 0;
        }
    }

    internal static class LocalizationIndexFormatConstants
    {
        public const System.String HeaderMagic = "LIF0";
        public const System.UInt16 Version = 0;
    }

    /// <summary>
    /// A localization index file aids in loading cultures for file-based resources and it automates most of the loading process. <br />
    /// This class is optional; but it is useful enough to automate how the resource loader will load it's referenced localization readers.
    /// </summary>
    public sealed class LocalizationIndexWriter : IDisposable, IStreamOwnerBase
    {
        private System.Int64 origpos;
        private System.IO.Stream stream;
        private List<LocalizationEntry> entriestoadd;
        private System.Boolean strmown , defcdecl , dgenerated;

        /// <summary>
        /// Creates a new instance of the <see cref="LocalizationIndexWriter"/> class with the specified stream to write the contents to.
        /// </summary>
        /// <param name="stream">The stream to write the current localization index.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> is not writeable.</exception>
        public LocalizationIndexWriter(System.IO.Stream stream)
        {
            if (stream is null) { throw new ArgumentNullException(nameof(stream)); }
            if (stream.CanWrite == false) { throw new ArgumentException("The stream given must be a writeable stream." , nameof(stream)); }
            this.stream = stream;
            origpos = stream.Position;
            strmown = false;
            defcdecl = false;
            dgenerated = false;
            entriestoadd = new(10);
        }

        /// <summary>
        /// Gets or sets a value whether the <see cref="LocalizationIndexWriter"/> class should dispose the underlying stream too when calling <see cref="Dispose"/>.
        /// </summary>
        public bool IsStreamOwner { get => strmown; set => strmown = value; }

        /// <summary>
        /// Adds a localization reader footprint to the current localization index stream.
        /// </summary>
        /// <param name="ent">The localization reader entry to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="ent"/> was null.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="LocalizationEntry.Culture"/> field was null.</exception>
        public void AddLocalizationReaderEntry(LocalizationEntry ent)
        {
            if (stream is null) { throw new ObjectDisposedException(nameof(LocalizationIndexWriter)); }
            if (ent is null) { throw new ArgumentNullException(nameof(ent)); }
            if (ent.Culture is null) { throw new InvalidOperationException("The localization reader entry object was invalid."); }
            if (ent.Flags.HasFlag(LocalizationSpecialTypeFlags.IsFallbackCulture)) { 
                if (defcdecl) {
                    // A fallback culture has already been declared!!!
                    throw new ArgumentException("A fallback culture has already been declared." , nameof(ent));
                }
                defcdecl = true; 
            }
            entriestoadd.Add(ent);
        }

        /// <summary>
        /// Clears all the localization reader entries. You can use this to correct mistakes.
        /// </summary>
        public void Clear()
        {
            entriestoadd?.Clear();
            defcdecl = false;
            dgenerated = false;
            stream.Position = origpos;
        }

        /// <summary>
        /// Generate the localization file based on the current localization entries.
        /// </summary>
        public void Generate()
        {
            if (dgenerated) {
                stream.Position = origpos;
                dgenerated = false;
            }
            stream.WriteASCIIString(LocalizationIndexFormatConstants.HeaderMagic);
            stream.WriteUInt16(LocalizationIndexFormatConstants.Version);
            LOCALIZATIONENTRY temp;
            foreach (var ent in entriestoadd)
            {
                temp = new();
                temp.Flags = ent.Flags;
                temp.LCID = ent.Culture.LCID;
                stream.WriteStructure(temp);
            }
            dgenerated = true;
        }

        /// <summary>
        /// Disposes all the resources that the <see cref="LocalizationIndexWriter"/> class does currently utilize.
        /// </summary>
        public void Dispose()
        {
            if (stream is not null)
            {
                if (dgenerated == false) { Generate(); }
                if (strmown) { stream.Dispose(); }
                stream = null;
            }
            dgenerated = false;
            entriestoadd?.Clear();
            entriestoadd = null;
            origpos = 0;
        }
    }

    /// <summary>
    /// The reader counterpart of the <see cref="LocalizationIndexWriter"/> class.
    /// </summary>
    public sealed class LocalizationIndexReader :  IDisposable , IStreamOwnerBase
    {
        private System.Int64 original;
        private System.Boolean strmown;
        private System.IO.Stream stream;

        /// <summary>
        /// Creates a new instance of <see cref="LocalizationIndexReader"/> class with the specified stream to read localization entries from.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> was null.</exception>
        /// <exception cref="ArgumentException">The specified <paramref name="stream"/> was not readable.</exception>
        public LocalizationIndexReader(System.IO.Stream stream)
        {
            if (stream is null) { throw new ArgumentNullException(nameof(stream)); }
            if (stream.CanRead == false) { throw new ArgumentException("The stream given must be a readable stream.", nameof(stream)); }
            this.stream = stream;
            strmown = false;
            System.String hdr = stream.ReadASCIIString(LocalizationIndexFormatConstants.HeaderMagic.Length);
            if (hdr != LocalizationIndexFormatConstants.HeaderMagic) { throw new ArgumentException("The given stream was not a Localization Format stream." , nameof(stream)); }
            System.UInt16 ver = stream.ReadUInt16();
            if (ver > LocalizationIndexFormatConstants.Version) { throw new ArgumentException($"The given version {ver} is not supported with this reader - use a reader that can only support up to version {LocalizationIndexFormatConstants.Version} formats." , nameof(stream)); }
            original = stream.Position;
        }

        /// <summary>
        /// Gets the next entry that is defined on the stream.
        /// </summary>
        /// <returns>The next localization entry.</returns>
        public LocalizationEntry GetNextEntry() => new(stream.ReadStructure<LOCALIZATIONENTRY>());

        /// <summary>
        /// Gets all the entries that are defined on the stream.
        /// </summary>
        /// <returns>All the localization entries that comprise this stream.</returns>
        public unsafe LocalizationEntry[] GetAllEntries()
        {
            stream.Position = original;
            System.Int64 size = stream.Length - stream.Position;
            LocalizationEntry[] ret = new LocalizationEntry[size / sizeof(LOCALIZATIONENTRY)];
            for (System.Int32 I = 0; I < ret.Length; I++) 
            {
                ret[I] = new(stream.ReadStructure<LOCALIZATIONENTRY>());
            }
            return ret;
        }

        /// <summary>
        /// Gets or sets a value whether the <see cref="LocalizationIndexReader"/> class should dispose the underlying stream too when calling <see cref="Dispose"/>.
        /// </summary>
        public bool IsStreamOwner { get => strmown; set => strmown = value; }

        /// <summary>
        /// Disposes any allocated data that the instance of the <see cref="LocalizationIndexReader"/> class does use.
        /// </summary>
        public void Dispose()
        {
            if (stream is not null) 
            {
                if (strmown) { stream.Dispose(); }
                stream = null;
            }
            original = 0;
        }
    }
}

