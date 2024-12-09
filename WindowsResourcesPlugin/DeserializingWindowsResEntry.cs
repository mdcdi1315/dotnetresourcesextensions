
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
        /// <exception cref="System.ArgumentNullException"><paramref name="entry"/> was <see langword="null"/>.</exception>
        public DeserializingWindowsResourceEntry(NativeWindowsResourceEntry entry)
        {
            if (entry is null) { throw new System.ArgumentNullException(nameof(entry)); }
            name = entry.Name.ToString();
            switch (entry.NativeType)
            {
#if NET472_OR_GREATER || WINDOWS10_0_19041_0_OR_GREATER
                case WindowsResourceEntryType.RT_BITMAP:
                    SafeDeviceIndependentBitmapHandle SD = null;
                    try {
                        SD = new(entry);
                        value = System.Drawing.Bitmap.FromHbitmap(SD.DangerousGetHandle());
                    } finally {
                        SD?.Dispose();
                        SD = null;
                    }
                    break;
                case WindowsResourceEntryType.RT_ICON:
                    SafeIconHandle sih = null;
                    try {  
                        sih = new SafeIconHandle(entry);
                        value = System.Drawing.Icon.FromHandle(sih.DangerousGetHandle());
                    } finally { sih?.Dispose(); sih = null; }
                    break;
#endif
                // Return RT_MANIFEST as plain array so that XML processors can process it if the user demands it.
                case WindowsResourceEntryType.RT_MANIFEST:
                // Animated cursors and HTML files are just files that have been directly saved and they do not require any additional processing.
                case WindowsResourceEntryType.RT_ANICURSOR:
                case WindowsResourceEntryType.RT_HTML:
                // Represents a plain byte array , usually coming from a custom file
                case WindowsResourceEntryType.RT_RCDATA:
                    value = entry.Value;
                    break;
                case WindowsResourceEntryType.RT_STRING:
                    value = new NativeStringsCollection(entry);
                    break;
                case WindowsResourceEntryType.RT_VERSION:
                    value = new VsVersionInfoGetter(entry);
                    break;
                case WindowsResourceEntryType.RT_GROUP_ICON:
                case WindowsResourceEntryType.RT_GROUP_CURSOR:
                    value = new ResourceGroupInformation(entry);
                    break;
                case WindowsResourceEntryType.RT_ACCELERATOR:
                    value = new AcceleratorTable(entry);
                    break;
                case WindowsResourceEntryType.RT_MESSAGETABLE:
                    value = new MessageTable(entry);
                    break;
                default:
                case WindowsResourceEntryType.Unknown:
                    throw new System.FormatException("Cannot create a DeserializingWindowsResourceEntry instance from this entry.");
            }
        }

        /// <inheritdoc />
        public System.String Name => name;

        /// <inheritdoc />
        public System.Object Value => value;

        /// <inheritdoc />
        public System.Type TypeOfValue => value?.GetType();
    }
}
