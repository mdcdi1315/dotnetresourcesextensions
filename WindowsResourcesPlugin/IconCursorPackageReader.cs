using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using DotNetResourcesExtensions.Internal;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Reads a icon package from the specified native reader and the given resource group entry which describes
    /// the icons that are contained. <br />
    /// It does then provide methods to save the resulting icon/cursor package to a stream.
    /// </summary>
    public sealed class IconCursorPackageReader : IDisposable
    {
        [StructLayout(LayoutKind.Explicit , Size = 6 , Pack = 2)]
        private unsafe struct PACKAGEHEADER
        {
            [FieldOffset(0)]
            private System.Byte pin;

            [FieldOffset(0)]
            public System.UInt16 RSVD; // Must always be zero.

            [FieldOffset(2)]
            public ImageType DirEntryType;

            [FieldOffset(4)]
            public System.UInt16 DirEntryCount;

            public readonly System.Byte[] GetBytes()
            {
                System.Byte[] ret = new System.Byte[sizeof(PACKAGEHEADER)];
                fixed (System.Byte* dst = ret)
                {
                    fixed (System.Byte* src = &Unsafe.AsRef(pin))
                    {
                        Unsafe.CopyBlockUnaligned(dst, src, ret.Length.ToUInt32());
                    }
                }
                return ret;
            }
        }

        [StructLayout(LayoutKind.Explicit , Size = 16)]
        private unsafe struct PACKAGEENTRY
        {
            [StructLayout(LayoutKind.Explicit , Size = 4 , Pack = 2)]
            public unsafe struct ICON
            {
                [FieldOffset(0)]
                public System.UInt16 ColorPlanes;

                [FieldOffset(2)]
                public System.UInt16 BitsPerPixel;
            }

            [StructLayout(LayoutKind.Explicit, Size = 4, Pack = 2)]
            public unsafe struct CURSOR
            {
                [FieldOffset(0)]
                public System.UInt16 XCoordinate;

                [FieldOffset(2)]
                public System.UInt16 YCoordinate;
            }

            [FieldOffset(0)]
            private System.Byte pin;

            [FieldOffset(0)]
            public System.Byte Width;

            [FieldOffset(1)]
            public System.Byte Height;

            [FieldOffset(2)]
            public System.Byte ColorPalleteColors;

            [FieldOffset(3)]
            public System.Byte RSVD; // Must always be zero.

            [FieldOffset(4)]
            public ICON IconEntry;

            [FieldOffset(4)]
            public CURSOR CursorEntry;

            [FieldOffset(8)]
            public System.UInt32 ImageSize;

            [FieldOffset(12)]
            public System.UInt32 ImageOffset;

            public readonly System.Byte[] GetBytes()
            {
                System.Byte[] ret = new System.Byte[sizeof(PACKAGEENTRY)];
                fixed (System.Byte* dst = ret)
                {
                    fixed (System.Byte* src = &Unsafe.AsRef(pin))
                    {
                        Unsafe.CopyBlockUnaligned(dst, src, ret.Length.ToUInt32());
                    }
                }
                return ret;
            }
        }

        private enum ImageType : System.UInt16 {
            ICON = 1,
            CURSOR = 2
        }

        private ResourceGroupInformation direntry;
        private List<KeyValuePair<System.UInt32, System.Byte[]>> icorcursors;

        /// <summary>
        /// Creates a new instance of the <see cref="IconCursorPackageReader"/> class with the specified reader and the
        /// icon/cursor package resource group to create from.
        /// </summary>
        /// <param name="reader">The native resource reader that contained the entry provided in <paramref name="entry"/>.</param>
        /// <param name="entry">The package resource group to read from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> or <paramref name="reader"/> were null.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property of <paramref name="entry"/> was not either
        /// <see cref="WindowsResourceEntryType.RT_GROUP_ICON"/> or <see cref="WindowsResourceEntryType.RT_GROUP_CURSOR"/>.</exception>
        public IconCursorPackageReader(NativeWindowsResourcesReader reader, NativeWindowsResourceEntry entry)
        {
            if (reader is null) { throw new ArgumentNullException(nameof(reader)); }
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            switch (entry.NativeType)
            {
                case WindowsResourceEntryType.RT_GROUP_CURSOR:
                case WindowsResourceEntryType.RT_GROUP_ICON:
                    break;
                default:
                    throw new ArgumentException("The native type of the given entry was not a resource group that describes a icon or cursor package.");
            }
            direntry = new(entry);
            icorcursors = new(direntry.Count);
            WindowsResourceEntryType expected = direntry.EntriesType;
            foreach (var ent in direntry.Entries)
            {
                var enumerator = reader.GetEnumerator();
                while (enumerator.MoveNext()) 
                {
                    // There is the need to check completely for the resource type due to the fact
                    // that two resources can share the same name but be completely different.
                    if (enumerator.ResourceEntry.Name is System.UInt16 num && 
                        num == ent.ResourceId && 
                        enumerator.ResourceEntry.NativeType == expected)
                    {
                        // The appropriate resource was found , the nested loop can safely exit and continue to get the next resource.
                        icorcursors.Add(new(ent.ResourceId, enumerator.ResourceEntry.Value));
                        break;
                    }
                }
                enumerator = null;
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IconCursorPackageReader"/> class with the specified reader and the
        /// icon/cursor package resource group to create from.
        /// </summary>
        /// <param name="reader">The native resource reader that contained the entry provided in <paramref name="entry"/>.</param>
        /// <param name="entry">The package resource group to read from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> or <paramref name="reader"/> were null.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property of <paramref name="entry"/> was not either
        /// <see cref="WindowsResourceEntryType.RT_GROUP_ICON"/> or <see cref="WindowsResourceEntryType.RT_GROUP_CURSOR"/>.</exception>
        public IconCursorPackageReader(NativeWindowsResFilesReader reader , NativeWindowsResourceEntry entry)
        {
            if (reader is null) { throw new ArgumentNullException(nameof(reader)); }
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            switch (entry.NativeType)
            {
                case WindowsResourceEntryType.RT_GROUP_CURSOR:
                case WindowsResourceEntryType.RT_GROUP_ICON:
                    break;
                default:
                    throw new ArgumentException("The native type of the given entry was not a resource group that describes a icon or cursor package.");
            }
            direntry = new(entry);
            icorcursors = new(direntry.Count);
            WindowsResourceEntryType expected = direntry.EntriesType;
            foreach (var ent in direntry.Entries)
            {
                var enumerator = reader.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    // There is the need to check completely for the resource type due to the fact
                    // that two resources can share the same name but be completely different.
                    if (enumerator.ResourceEntry.Name is System.UInt16 num && 
                        num == ent.ResourceId &&
                        enumerator.ResourceEntry.NativeType == expected)
                    {
                        icorcursors.Add(new(ent.ResourceId, enumerator.ResourceEntry.Value));
                    }
                }
                enumerator = null;
            }
        }

        /// <summary>
        /// Creates an icon package containing a single icon.
        /// </summary>
        /// <param name="entry">The icon resource to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> was null.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property of <paramref name="entry"/> was not <see cref="WindowsResourceEntryType.RT_ICON"/>.</exception>
        public IconCursorPackageReader(NativeWindowsResourceEntry entry)
        {
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            switch (entry.NativeType)
            {
                case WindowsResourceEntryType.RT_ICON:
                    break;
                default:
                    throw new ArgumentException("The native type of the given entry was not an entry that describes an icon.");
            }
            direntry = ResourceGroupInformation.CreateTyped(entry);
            icorcursors = new(1) {
                new(entry.NumericName, entry.Value)
            };
        }

        /// <summary>
        /// Creates an icon package containing a multiple of resource icons.
        /// </summary>
        /// <param name="entries">The icon resources to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entries"/> was null.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property of a entry in <paramref name="entries"/> array was not <see cref="WindowsResourceEntryType.RT_ICON"/>.</exception>
        public IconCursorPackageReader(params NativeWindowsResourceEntry[] entries)
        {
            List<NativeWindowsResourceEntry> fl = new(entries);
            List<System.Int32> rem = new(fl.Count);
            for (System.Int32 I = 0; I < fl.Count; I++)
            {
                if (fl[I] is null) { rem.Add(I); continue; }
                if (fl[I].NativeType != WindowsResourceEntryType.RT_ICON) { throw new ArgumentException("All resource entries must be only resource icons!"); }
            }
            foreach (var item in rem) { fl.RemoveAt(item); }
            rem.Clear();
            rem = null;
            if (fl.Count == 0) { throw new ArgumentException("At least one valid icon resource entry is required so that the package can be created."); }
            direntry = ResourceGroupInformation.CreateTyped(fl.ToArray());
            icorcursors = new(fl.Count);
            foreach (var ent in fl) { icorcursors.Add(new(ent.NumericName, ent.Value)); }
            fl.Clear();
            fl = null;
        }

        // Generates the given icons to a icon package. Each invokation creates a new package array.
        private System.Byte[] Generate()
        {
            System.IO.MemoryStream mems = null;
            try {
                mems = StreamGenerate();
                return mems.ToArray();
            } finally {
                mems?.Dispose();
                mems = null;
            }
        }

        private unsafe System.IO.MemoryStream StreamGenerate()
        {
            if (direntry is null || icorcursors is null) { throw new ObjectDisposedException(nameof(IconCursorPackageReader)); }
            System.IO.MemoryStream mems = new();
            System.Int32 finalsize = sizeof(PACKAGEHEADER) + (sizeof(PACKAGEENTRY) * icorcursors.Count);
            // The first image offset is after all the generated entries!
            System.Int32 imgoffset = finalsize;
            PACKAGEHEADER hdr = new();
            hdr.RSVD = 0;
            hdr.DirEntryType = direntry.EntriesType switch
            {
                WindowsResourceEntryType.RT_ICON => ImageType.ICON,
                WindowsResourceEntryType.RT_CURSOR => ImageType.CURSOR,
                _ => ImageType.ICON
            };
            List<PACKAGEENTRY> wrentries = new(10);
            System.Int32 size;
            foreach (var ent in direntry.Entries)
            {
                size = -1;
                foreach (var entr in icorcursors) { if (entr.Key == ent.ResourceId) { size = entr.Value.Length; break; } }
                if (size == -1) { continue; }
                PACKAGEENTRY entrynative = new();
                entrynative.Height = ent.Height.ToByte();
                entrynative.Width = ent.Width.ToByte();
                switch (hdr.DirEntryType)
                {
                    case ImageType.ICON:
                        entrynative.IconEntry = new() { BitsPerPixel = ent.BitCount, ColorPlanes = ent.Planes };
                        break;
                    case ImageType.CURSOR:
                        entrynative.CursorEntry = new() { XCoordinate = (ent.Width / 2).ToUInt16(), YCoordinate = (ent.Height / 2).ToUInt16() };
                        break;
                }
                entrynative.RSVD = 0;
                entrynative.ColorPalleteColors = ent.ColorCount.ToByte();
                entrynative.ImageOffset = imgoffset.ToUInt32();
                entrynative.ImageSize = size.ToUInt32();
                // Update the image offset
                imgoffset += size;
                wrentries.Add(entrynative);
            }
            // Start writing data...
            hdr.DirEntryCount = wrentries.Count.ToUInt16();
            mems.Write(hdr.GetBytes(), 0, sizeof(PACKAGEHEADER));
            foreach (var ent in wrentries) { mems.Write(ent.GetBytes(), 0, sizeof(PACKAGEENTRY)); }
            wrentries.Clear();
            wrentries = null;
            // Copy images one by one.
            foreach (var entry in icorcursors)
            {
                // Detect also whether the image to copy does exist...
                System.Boolean fd = false;
                foreach (var ent in direntry.Entries)
                {
                    if (ent.ResourceId == entry.Key) { fd = true; break; }
                }
                if (fd == false) { continue; }
                mems.Write(entry.Value, 0, entry.Value.Length);
            }
            // Reset position and done!
            mems.Position = 0;
            return mems;
        }

        /// <summary>
        /// Saves the read data to the specified file on disk. <br />
        /// If the file exists , it will be overwritten.
        /// </summary>
        /// <param name="filename">The file to save the data to.</param>
        public void Save(System.String filename)
        {
            if (direntry is null) { throw new ObjectDisposedException(nameof(IconCursorPackageReader)); }
            System.IO.Stream source = null;
            System.IO.FileStream FS = null;
            try {
                FS = new(filename, System.IO.FileMode.Create);
                source = StreamGenerate();
                source.CopyTo(FS);
            } finally {
                source?.Dispose();
                source = null;
                FS?.Dispose();
                FS = null;
            }
        }

        /// <summary>
        /// Saves the read data to a specified stream , starting writing from the input stream position.
        /// </summary>
        /// <param name="stream">The stream to save the data to.</param>
        public void Save(System.IO.Stream stream)
        {
            System.IO.Stream source = null;
            try {
                source = StreamGenerate();
                source.CopyTo(stream);
            } finally {
                source?.Dispose();
                source = null;
            }
        }

        /// <summary>
        /// Gets the raw icon package bytes as a byte array. <br />
        /// Be noted that this property generally allocates on call , so try to avoid using it as most as possible. <br />
        /// Other good alternatives are also the <see cref="Save(string)"/> and <see cref="Save(System.IO.Stream)"/> overloads.
        /// </summary>
        public System.Byte[] PackageBytes => Generate();

        /// <summary>
        /// Disposes this <see cref="IconCursorPackageReader"/> class instance. <br />
        /// You must dispose the instance as soon as you have finished using it!
        /// </summary>
        public void Dispose()
        {
            direntry = null;
            icorcursors?.Clear();
            icorcursors = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>Returns the class name of this instance.</summary>
        public override string ToString() => nameof(IconCursorPackageReader);
    }
}
