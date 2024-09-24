﻿using DotNetResourcesExtensions.Internal.CustomFormatter;

#pragma warning disable CA1416

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Defines a resource entry that tries to be as most close as possible with the original implementation of <see cref="IResourceEntry"/>. <br />
    /// The class was created so that the user can decide whether he wants a resource entry much more closer to the DotNetResourcesExtensions
    /// conventions. This class can only be constructed from a valid <see cref="NativeWindowsResourceEntry"/> instance.
    /// </summary>
    public sealed class DeserializingWindowsResourceEntry : IResourceEntry
    {
        private System.String name;
        private System.Object value;

        /// <summary>
        /// Creates a new instance of <see cref="DeserializingWindowsResourceEntry"/> from the specified native resource entry.
        /// </summary>
        /// <param name="entry">The entry to create the deserializing entry from.</param>
        /// <exception cref="System.FormatException">The entry cannot be created.</exception>
        public DeserializingWindowsResourceEntry(NativeWindowsResourceEntry entry)
        {
            ExtensibleFormatter exf = new();
            try
            {
                name = entry.Name.ToString();
                switch (entry.NativeType)
                {
#if NET472_OR_GREATER || WINDOWS10_0_19041_0_OR_GREATER
                    case WindowsResourceEntryType.RT_BITMAP:
                        value = exf.GetObject<System.Drawing.Bitmap>(entry.Value);
                        break;
                    case WindowsResourceEntryType.RT_ICON:
                        value = exf.GetObject<System.Drawing.Icon>(entry.Value);
                        break;
#endif
                    case WindowsResourceEntryType.RT_RCDATA:
                        value = entry.Value;
                        break;
                    case WindowsResourceEntryType.RT_STRING:
                        value = new NativeStringsCollection(entry);
                        break;
                    case WindowsResourceEntryType.RT_VERSION:
                        value = new VsVersionInfoGetter(entry);
                        break;
                    default:
                    case WindowsResourceEntryType.Unknown:
                        throw new System.FormatException("Cannot create a DeserializingWindowsResourceEntry instance from this entry.");
                }
            } finally { exf.Dispose(); exf = null; }
        }

        /// <inheritdoc />
        public System.String Name => name;

        /// <inheritdoc />
        public System.Object Value => value;

        /// <inheritdoc />
        public System.Type TypeOfValue => value?.GetType();
    }
}