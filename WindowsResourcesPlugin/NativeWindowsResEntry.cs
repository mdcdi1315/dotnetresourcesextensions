using System.Globalization;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Defines a native Windows Resource Entry implementation. <br />
    /// This class cannot be inherited nor can be directly created; you need an instance of <see cref="NativeWindowsResourcesEnumerator"/> to get one.
    /// </summary>
    public sealed class NativeWindowsResourceEntry : IResourceEntry
    {
        private System.String nativetypestr;
        private WindowsResourceEntryType nativetype;
        private System.Byte[] value;
        private CultureInfo culture;
        private System.Object name;

        internal NativeWindowsResourceEntry() {
            nativetype = WindowsResourceEntryType.Unknown;
            nativetypestr = null;
            value = null;
            culture = null;
            name = null;
        }

        System.String IResourceEntry.Name => name.ToString();

        System.Object IResourceEntry.Value => value;

        /// <summary>
        /// Gets the resource name as a numeric value , if that is valid for this entry.
        /// </summary>
        public System.UInt16 NumericName 
        { 
            get {
                if (name is System.UInt16 num) { return num; }
                throw new System.InvalidCastException("Cannot cast because the name object is a string.");
            } 
        }

        /// <summary>
        /// Gets the name of this resource entry. It can be a string or a numeric value.
        /// </summary>
        public System.Object Name { get => name; internal set => name = value; }

        /// <summary>
        /// Gets the value of this resource entry. It is always a bytearray.
        /// </summary>
        public System.Byte[] Value { get => value; internal set => this.value = value; }

        /// <summary>
        /// Gets the language that this resource entry has been defined in.
        /// </summary>
        public CultureInfo Culture { get => culture; internal set => culture = value; }
        
        /// <summary>
        /// Gets the Windows native type specification for this entry , as defined in WinUser.h . If the value is not valid , the actual type number is returned.
        /// </summary>
        public WindowsResourceEntryType NativeType { get => nativetype; internal set => nativetype = value; }

        /// <summary>
        /// Gets the native type specification as a string.
        /// </summary>
        public System.String NativeTypeString 
        { 
            get => System.String.IsNullOrEmpty(nativetypestr) ? nativetype.ToString() : nativetypestr;
            internal set => nativetypestr = value; 
        }

        /// <inheritdoc />
        public System.Type TypeOfValue => typeof(System.Byte[]);
    }
}
