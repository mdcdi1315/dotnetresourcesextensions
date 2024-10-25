namespace DotNetResourcesExtensions
{
    // Values took from the Win32 header WinUser.h .
    /// <summary>
    /// Defines constants that specify the resource type that Windows understand as a common/valid type. <br />
    /// These constants are located in WinUser.h C/C++ header.
    /// </summary>
    public enum WindowsResourceEntryType : System.Int32
    {
        /// <summary>A special constant that indicates that the resource type could not be determined.</summary>
        Unknown = -1,
        /// <summary>Defines the RT_CURSOR constant. When this is defined , then the resource value represents a cursor file.</summary>
        RT_CURSOR = 1,
        /// <summary>Defines the RT_BITMAP constant. When this is defined , then the resource value represents a bitmap file.</summary>
        RT_BITMAP,
        /// <summary>Defines the RT_ICON constant. When this is defined , then the resource value represents a Windows icon file.</summary>
        RT_ICON,
        /// <summary>Defines the RT_MENU constant.</summary>
        RT_MENU,
        /// <summary>Defines the RT_DIALOG constant.</summary>
        RT_DIALOG,
        /// <summary>
        /// Defines the RT_STRING constant. When this is defined , then the resource value represents a section of string table resources. <br />
        /// If you have such a resource entry , you can learn it's contents by constructing a new instance of <see cref="NativeStringsCollection"/> class.
        /// </summary>
        RT_STRING,
        /// <summary>Defines the RT_FONTDIR constant.</summary>
        RT_FONTDIR,
        /// <summary>Defines the RT_FONT constant. When this is defined , then the resource value represents a font file.</summary>
        RT_FONT,
        /// <summary>
        /// Defines the RT_ACCELERATOR constant. <br />
        /// Entries marked with this type contain keyboard acclerators. <br />
        /// To read these , use the <see cref="AcceleratorTable"/> class.
        /// </summary>
        RT_ACCELERATOR,
        /// <summary>Defines the RT_RCDATA constant. Usually the resource value is just the byte array itself.</summary>
        RT_RCDATA,
        /// <summary>
        /// Defines the RT_MESSAGETABLE constant. <br />
        /// Entries marked with this type do contain format message strings for message boxes. <br />
        /// To read such entries , use the <see cref="MessageTable"/> class.
        /// </summary>
        RT_MESSAGETABLE,
        /// <summary>
        /// Defines the RT_GROUP_CURSOR constant. <br />
        /// This constant specifies that the data contain multiple cursor entries that map to resources , and all these do belong
        /// to the group that are specified. To read such a resource , use the <see cref="ResourceGroupInformation"/> class.
        /// </summary>
        RT_GROUP_CURSOR = 12,
        /// <summary>
        /// Defines the RT_GROUP_ICON constant. <br />
        /// This constant specifies that the data contain multiple icon entries that map to resources , and all these do belong
        /// to the group that are specified. To read such a resource , use the <see cref="ResourceGroupInformation"/> class.
        /// </summary>
        RT_GROUP_ICON = 14,
        /// <summary>
        /// Defines the RT_VERSION constant. <br />
        /// When this is defined , then the resource value represents the version block of the app. <br />
        /// In most cases , there will be no or one resource entries of this type in a PE file. <br />
        /// Use the <see cref="VsVersionInfoGetter"/> class to read such an entry.
        /// </summary>
        RT_VERSION = 16,
        /// <summary>Defines the RT_DLGINCLUDE constant.</summary>
        RT_DLGINCLUDE,
        /// <summary>Defines the RT_PLUGPLAY constant.</summary>
        RT_PLUGPLAY = 19,
        /// <summary>Defines the RT_VXD constant.</summary>
        RT_VXD,
        /// <summary>Defines the RT_ANICURSOR constant. This is an animated cursor.</summary>
        RT_ANICURSOR,
        /// <summary>Defines the RT_ANIICON constant. This is an animated icon.</summary>
        RT_ANIICON,
        /// <summary>Defines the RT_HTML constant.</summary>
        RT_HTML,
        /// <summary>
        /// Defines the RT_MANIFEST constant. <br />
        /// This constant indicates that the value contains the embedded manifest for the application. <br />
        /// The value does usually contain XML document information for advanced startup settings. <br />
        /// You can pass the resource value to a XML reader to see what it contains!
        /// </summary>
        RT_MANIFEST = 24,
    }
}
