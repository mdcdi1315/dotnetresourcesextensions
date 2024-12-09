using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using DotNetResourcesExtensions.Internal;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Provides information about a native Windows assembly.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 52)]
    public unsafe struct VsFileInformation
    {
        /// <summary>
        /// Reads an instance of this structure from plain bytes.
        /// </summary>
        /// <param name="bytes">The byte array to read from.</param>
        /// <param name="startindex">The index to start reading the structure from.</param>
        /// <returns>A new instance of <see cref="VsFileInformation"/> structure read from <paramref name="bytes"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> were effectively null.</exception>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> do not have the requested elements , or <paramref name="startindex"/> overlaps the array length.</exception>
        public static VsFileInformation FromPlainBytes(System.Byte[] bytes, System.Int32 startindex)
        {
            if (bytes is null) { throw new ArgumentNullException(nameof(bytes)); }
            if (sizeof(VsFileInformation) > bytes.Length - startindex) {
                throw new ArgumentException("There are not enough elements to copy so that the VsFileInformation structure can be initialized.");
            }
            VsFileInformation result = new();
            fixed (System.Byte* ptr = &Unsafe.AsRef(result.pin))
            {
                fixed (System.Byte* array = &bytes[startindex])
                {
                    Unsafe.CopyBlockUnaligned(ptr, array, (System.UInt32)sizeof(VsFileInformation));
                }
            }
            return result;
        }

        // pinnable reference for reading the structure , do not modify!
        [FieldOffset(0)]
        private System.Byte pin;

        /// <summary>
        /// The file information signature. It must be always the value 0xFEEF04BD.
        /// </summary>
        [FieldOffset(0)]
        public System.UInt32 Signature;

        /// <summary>
        /// The file information structure version. 
        /// The high-order word of this member contains the major version number, and the low-order word contains the minor version number.
        /// </summary>
        [FieldOffset(4)]
        public System.UInt32 StructVersion;

        /// <summary>
        /// The most significant 32 bits of the file's binary version number.
        /// </summary>
        [FieldOffset(8)]
        public System.UInt32 FileVersionHigh;

        /// <summary>
        /// The least significant 32 bits of the file's binary version number.
        /// </summary>
        [FieldOffset(12)]
        public System.UInt32 FileVersionLow;

        /// <summary>
        /// The most significant 32 bits of the binary version number of the product with which this file was distributed.
        /// </summary>
        [FieldOffset(16)]
        public System.UInt32 ProductVersionHigh;

        /// <summary>
        /// The least significant 32 bits of the binary version number of the product with which this file was distributed.
        /// </summary>
        [FieldOffset(20)]
        public System.UInt32 ProductVersionLow;

        /// <summary>
        /// Contains a bitmask that specifies the valid bits in <see cref="FileFlags"/>. A bit is valid only if it was defined when the file was created.
        /// </summary>
        [FieldOffset(24)]
        public VsFileInfoFlags FileFlagsMask;

        /// <summary>
        /// Contains a bitmask that specifies the Boolean attributes of the file. 
        /// </summary>
        [FieldOffset(28)]
        public VsFileInfoFlags FileFlags;

        /// <summary>
        /// The operating system for which this file was designed.
        /// </summary>
        [FieldOffset(32)]
        public VsFileOperatingSystemFlags FileOS;

        /// <summary>
        /// The general type of file.
        /// </summary>
        [FieldOffset(36)]
        public VsFileTypeFlags FileType;

        /// <summary>
        /// The function of the file. 
        /// The possible values depend on the value of <see cref="FileType"/>. 
        /// For all values of <see cref="FileType"/> not described in the following list, <see cref="FileSubType"/> is zero.
        /// </summary>
        [FieldOffset(40)]
        public VsFileSubTypeFlags FileSubType;

        /// <summary>
        /// The most significant 32 bits of the file's 64-bit binary creation date and time stamp.
        /// </summary>
        [FieldOffset(44)]
        public System.UInt32 FileCreationDateHigh;

        /// <summary>
        /// The least significant 32 bits of the file's 64-bit binary creation date and time stamp.
        /// </summary>
        [FieldOffset(48)]
        public System.UInt32 FileCreationDateLow;

        private readonly System.Int64 FileCreationTimeToTicks() => (FileCreationDateHigh.ToInt64() << 32) + FileCreationDateLow;

        /// <summary>
        /// Gets a <see cref="DateTime"/> that represents the result of the two <see cref="FileCreationDateHigh"/> and <see cref="FileCreationDateLow"/> fields.
        /// </summary>
        public readonly DateTime FileCreationDate => new(FileCreationTimeToTicks());

        /// <summary>
        /// Gets the structure data back to a byte array.
        /// </summary>
        /// <returns>The structure data represented as a byte array.</returns>
        public readonly System.Byte[] GetBytes()
        {
            System.Byte[] result = new System.Byte[sizeof(VsFileInformation)];
            fixed (System.Byte* ptr = &Unsafe.AsRef(pin))
            {
                fixed (System.Byte* presult = &result[0])
                {
                    Unsafe.CopyBlockUnaligned(presult, ptr, (System.UInt32)sizeof(VsFileInformation));
                }
            }
            return result;
        }
        
        /// <summary>
        /// Gets the version number defined from the 
        /// <see cref="FileVersionHigh"/> and <see cref="FileVersionLow"/> fields.
        /// </summary>
        public readonly System.Version Version
        {
            get {
                System.Byte[] high = FileVersionHigh.GetBytes() , low = FileVersionLow.GetBytes();
                return new(
                    high.ToUInt16(2),
                    high.ToUInt16(0),
                    low.ToUInt16(2),
                    low.ToUInt16(0));
            }
        }

        /// <summary>
        /// Gets the product version number defined from the 
        /// <see cref="ProductVersionHigh"/> and <see cref="ProductVersionLow"/> fields.
        /// </summary>
        public readonly System.Version ProductVersion
        {
            get {
                System.Byte[] high = ProductVersionHigh.GetBytes(),
                    low = ProductVersionLow.GetBytes();
                return new(
                    high.ToUInt16(2),
                    high.ToUInt16(0),
                    low.ToUInt16(2),
                    low.ToUInt16(0));
            }
        }

        /// <summary>
        /// Gets the structure version field (<see cref="StructVersion"/>) as a <see cref="System.Version"/> instance.
        /// </summary>
        public readonly System.Version StructureVersion
        {
            get {
                System.Byte[] bytes = StructVersion.GetBytes();
                return new(bytes.ToUInt16(2), bytes.ToUInt16(0));
            }
        }

        /// <summary>
        /// Gets a string which describes the structure details in short form.
        /// </summary>
        public readonly override System.String ToString()
            => $"VsFileInformation {{ FileFlagsMask={FileFlagsMask} , FileFlags={FileFlags} , FileOS={FileOS} , FileType={FileType} , FileSubType={FileSubType} , FileCreationDate={FileCreationDate} }}";
    }

    /// <summary>
    /// Specifies constants for file information flags.
    /// </summary>
    [Flags]
    public enum VsFileInfoFlags : System.UInt32
    {
        /// <summary>
        /// The file does not contain any special flags.
        /// </summary>
        None = 0,
        /// <summary>
        /// The file contains debugging information or is compiled with debugging features enabled.
        /// </summary>
        VS_FF_DEBUG = 1,
        /// <summary>
        /// The file's version structure was created dynamically; 
        /// therefore, some of the members in this structure may be empty or incorrect. 
        /// This flag should never be set in a file's <see cref="VsFileInformation"/> data.
        /// </summary>
        VS_FF_INFOINFERRED = 16,
        /// <summary>
        /// The file has been modified and is not identical to the original shipping file of the same version number.
        /// </summary>
        VS_FF_PATCHED = 4,
        /// <summary>
        /// The file is a development version, not a commercially released product.
        /// </summary>
        VS_FF_PRERELEASE = 2,
        /// <summary>
        /// The file was not built using standard release procedures. 
        /// If this flag is set, the StringFileInfo structure should contain a PrivateBuild entry.
        /// </summary>
        VS_FF_PRIVATEBUILD = 8,
        /// <summary>
        /// The file was built by the original company using standard release procedures but is a variation of the normal file of the same version number. 
        /// If this flag is set, the StringFileInfo structure should contain a SpecialBuild entry.
        /// </summary>
        VS_FF_SPECIALBUILD = 32
    }

    /// <summary>
    /// Specifies the operating environment this app must run on.
    /// </summary>
    [Flags]
    public enum VsFileOperatingSystemFlags : System.UInt32
    {
        /// <summary>
        /// The file was designed for MS-DOS.
        /// </summary>
        VOS_DOS = 65536,
        /// <summary>
        /// The file was designed for Windows NT.
        /// </summary>
        VOS_NT = 262144,
        /// <summary>
        /// The file was designed for 16-bit Windows.
        /// </summary>
        VOS__WINDOWS16 = 1,
        /// <summary>
        /// The file was designed for 32-bit Windows.
        /// </summary>
        VOS__WINDOWS32 = 4,
        /// <summary>
        /// The file was designed for 16-bit OS/2.
        /// </summary>
        VOS_OS216 = 131072,
        /// <summary>
        /// The file was designed for 32-bit OS/2.
        /// </summary>
        VOS_OS232 = 196608,
        /// <summary>
        /// The file was designed for 16-bit Presentation Manager.
        /// </summary>
        VOS__PM16 = 2,
        /// <summary>
        /// The file was designed for 32-bit Presentation Manager.
        /// </summary>
        VOS__PM32 = 3,
        /// <summary>
        /// The operating system for which the file was designed is unknown to the system.
        /// </summary>
        VOS_UNKNOWN = 0,
        /// <summary>
        /// The file was designed for 16-bit Windows running on MS-DOS.
        /// </summary>
        VOS_DOS_WINDOWS16 = 65537,
        /// <summary>
        /// The file was designed for 32-bit Windows running on MS-DOS.
        /// </summary>
        VOS_DOS_WINDOWS32 = 65540,
        /// <summary>
        /// The file was designed for Windows NT.
        /// </summary>
        VOS_NT_WINDOWS32 = 262148,
        /// <summary>
        /// The file was designed for 16-bit Presentation Manager running on 16-bit OS/2.
        /// </summary>
        VOS_OS216_PM16 = 131074,
        /// <summary>
        /// The file was designed for 32-bit Presentation Manager running on 32-bit OS/2.
        /// </summary>
        VOS_OS232_PM32 = 196611
    }

    /// <summary>
    /// Specifies constants about the file type.
    /// </summary>
    [Flags]
    public enum VsFileTypeFlags : System.UInt32
    {
        /// <summary>
        /// The file contains an application.
        /// </summary>
        VFT_APP = 1,
        /// <summary>
        /// The file contains a DLL.
        /// </summary>
        VFT_DLL = 2,
        /// <summary>
        /// The file contains a device driver. 
        /// If <see cref="VsFileInformation.FileType"/> is <see cref="VFT_DRV"/>, <see cref="VsFileInformation.FileSubType"/> contains a more specific description of the driver.
        /// </summary>
        VFT_DRV = 3,
        /// <summary>
        /// The file contains a font. 
        /// If <see cref="VsFileInformation.FileType"/> is <see cref="VFT_FONT"/>, <see cref="VsFileInformation.FileSubType"/> contains a more specific description of the font file.
        /// </summary>
        VFT_FONT = 4,
        /// <summary>
        /// The file contains a static-link library.
        /// </summary>
        VFT_STATIC_LIB = 7,
        /// <summary>
        /// The file type is unknown to the system.
        /// </summary>
        VFT_UNKNOWN = 0,
        /// <summary>
        /// The file contains a virtual device.
        /// </summary>
        VFT_VXD = 5,
    }

    /// <summary>
    /// Specifies additional miscellaneous constants about the file type.
    /// </summary>
    [Flags]
    public enum VsFileSubTypeFlags : System.UInt32
    {
        /// <summary>
        /// The file contains a communications driver.
        /// </summary>
        VFT2_DRV_COMM = 10,
        /// <summary>
        /// The file contains a display driver.
        /// </summary>
        VFT2_DRV_DISPLAY = 4,
        /// <summary>
        /// The file contains an installable driver.
        /// </summary>
        VFT2_DRV_INSTALLABLE = 8,
        /// <summary>
        /// The file contains a keyboard driver.
        /// </summary>
        VFT2_DRV_KEYBOARD = 2,
        /// <summary>
        /// The file contains a language driver.
        /// </summary>
        VFT2_DRV_LANGUAGE = 3,
        /// <summary>
        /// The file contains a mouse driver.
        /// </summary>
        VFT2_DRV_MOUSE = 5,
        /// <summary>
        /// The file contains a network driver.
        /// </summary>
        VFT2_DRV_NETWORK = 6,
        /// <summary>
        /// The file contains a printer driver.
        /// </summary>
        VFT2_DRV_PRINTER = 1,
        /// <summary>
        /// The file contains a sound driver.
        /// </summary>
        VFT2_DRV_SOUND = 9,
        /// <summary>
        /// The file contains a system driver.
        /// </summary>
        VFT2_DRV_SYSTEM = 7,
        /// <summary>
        /// The file contains a versioned printer driver.
        /// </summary>
        VFT2_DRV_VERSIONED_PRINTER = 12,
        /// <summary>
        /// The driver type is unknown by the system.
        /// </summary>
        VFT2_UNKNOWN = 0,
        /// <summary>
        /// The file contains a raster font.
        /// </summary>
        VFT2_FONT_RASTER = 1,
        /// <summary>
        /// The file contains a TrueType font.
        /// </summary>
        VFT2_FONT_TRUETYPE = 3,
        /// <summary>
        /// The file contains a vector font.
        /// </summary>
        VFT2_FONT_VECTOR = 2
    }

}
