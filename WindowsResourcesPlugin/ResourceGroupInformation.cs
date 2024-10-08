using System;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Provides basic information about a resource group (icons or cursors) , which is either a <see cref="WindowsResourceEntryType.RT_GROUP_ICON"/> or 
    /// a <see cref="WindowsResourceEntryType.RT_GROUP_CURSOR"/> resource.
    /// </summary>
    public sealed class ResourceGroupInformation
    {
        private System.Int32 entrycount;
        private WindowsResourceEntryType entriestype;
        private List<IconCursorInformationEntry> entries;

        /// <summary>
        /// Constructs a new instance of <see cref="ResourceGroupInformation"/> class from the specified entry
        /// that contains icon or cursor directory entries.
        /// </summary>
        /// <param name="entry">The resource entry to read the data from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> was null.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property of <paramref name="entry"/>
        /// was not <see cref="WindowsResourceEntryType.RT_GROUP_CURSOR"/> or <see cref="WindowsResourceEntryType.RT_GROUP_ICON"/>.
        /// </exception>
        public ResourceGroupInformation(NativeWindowsResourceEntry entry)
        {
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            if (entry.NativeType != WindowsResourceEntryType.RT_GROUP_CURSOR &&
                entry.NativeType != WindowsResourceEntryType.RT_GROUP_ICON)
            {
                throw new ArgumentException("The NativeType of the resource entry must have been either set to RT_GROUP_ICON or RT_GROUP_CURSOR.");
            }
            Interop.NEWHEADER header = Interop.NEWHEADER.ReadFromArray(entry.Value, 0);
            entrycount = header.DirectoryCount;
            entries = new(entrycount);
            entriestype = header.ResourceType switch {
                1 => WindowsResourceEntryType.RT_ICON,
                2 => WindowsResourceEntryType.RT_CURSOR,
                _ => WindowsResourceEntryType.Unknown
            };
            ReadEntries(entry.Value);
        }

        private void ReadEntries(System.Byte[] data)
        {
            System.Int32 idx = 6; // Remember that we have already read 6 bytes!
            // Read all RESDIR structures , and at the same time save them to entries.
            while (idx + 1 < data.Length) 
            {
                entries.Add(new(data, idx, entriestype));
                // 14 is the length of a single RESDIR.
                idx += 14; // Update the index by adding the size of the structure.
            }
            if (entries.Count != entrycount) { throw new FormatException($"The resource directories read were not equal to the expected directories (Read {entries.Count} while expecting {entrycount} entries)."); }
        }

        /// <summary>
        /// Gets an enumerable of icon or cursor entries to examine.
        /// </summary>
        public IEnumerable<IconCursorInformationEntry> Entries => entries;

        /// <summary>
        /// Specifies the type of entries contained in <see cref="Entries"/> property. <br />
        /// This property does only take two distinct values - either <see cref="WindowsResourceEntryType.RT_ICON"/> or <see cref="WindowsResourceEntryType.RT_CURSOR"/>.
        /// </summary>
        public WindowsResourceEntryType EntriesType => entriestype;

        /// <summary>
        /// Gets the number of entries contained in <see cref="Entries"/> property.
        /// </summary>
        public System.Int32 Count => entrycount;

        /// <summary>
        /// Cleans up all the resources used by the current instance if it falls out of scope.
        /// </summary>
        ~ResourceGroupInformation() { entries?.Clear(); entries = null; }
    }

    /// <summary>
    /// Defines a icon or cursor entry , depending on the resource fed to the constructor of <see cref="ResourceGroupInformation"/> class. <br />
    /// You cannot create an instance of this class; instead , access the <see cref="ResourceGroupInformation.Entries"/> property to get an instance of this class.
    /// </summary>
    public sealed class IconCursorInformationEntry
    {
        private Interop.RESDIR native;
        private WindowsResourceEntryType entrytype;

        internal IconCursorInformationEntry(System.Byte[] data , System.Int32 idx, WindowsResourceEntryType entrytype) {
            native = Interop.RESDIR.ReadFromArray(data, idx);
            this.entrytype = entrytype;
        }

        /// <summary>
        /// Gets the width of the current icon or cursor. <br />
        /// A value 0 could also mean the value 256.
        /// </summary>
        public System.UInt16 Width => entrytype == WindowsResourceEntryType.RT_ICON ? native.Icon.Width : native.Cursor.Width;

        /// <summary>
        /// Gets the height of the current icon or cursor. <br />
        /// A value 0 could also mean the value 256.
        /// </summary>
        public System.UInt16 Height => entrytype == WindowsResourceEntryType.RT_ICON ? native.Icon.Height : native.Cursor.Height;

        /// <summary>
        /// Gets the current resource size.
        /// </summary>
        public System.UInt32 Size => native.BytesInRes;

        /// <summary>
        /// Gets the icon or cursor image planes count.
        /// </summary>
        public System.UInt16 Planes => native.Planes;

        /// <summary>
        /// Gets the icon or cursor image bit count , or as known as bits-per-pixel (bpp) value.
        /// </summary>
        public System.UInt16 BitCount => native.BitCount;

        /// <summary>
        /// Gets the resource ID of the icon or cursor directory entry that is referenced to. 
        /// </summary>
        public System.UInt16 ResourceId => native.IconCursorId;
    }
}
