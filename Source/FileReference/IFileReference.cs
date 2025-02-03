
namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Represents an abstract version of a file reference inside a resource data stream. <br />
    /// Any users of this interface must know that any implementation that will use this interface must
    /// support code for these cases. <br />
    /// Unlike the original ResX classes , these file references are expected their resulting type to be given
    /// when these are read out by any reader , and not returning the original read file reference. <br />
    /// To implement and use the <see cref="IFileReference"/> to your own resource reader , you should consult the Docs first.
    /// </summary>
    public interface IFileReference
    {
        /// <summary>
        /// Gets the filename of this reference. The filename can be a relative path too.
        /// </summary>
        public System.String FileName { get; }

        /// <summary>
        /// The final resource type that it will be represented when this reference will be read.
        /// </summary>
        public System.Type SavingType { get; }

        /// <summary>
        /// Gets the encoding which this file has been written at.
        /// </summary>
        public FileReferenceEncoding Encoding { get; }
    }

    /// <summary>
    /// Defines common constants for file encodings for a given <see cref="IFileReference"/>. 
    /// </summary>
    public enum FileReferenceEncoding : System.Byte
    {
        /// <summary>
        /// A special value that indicates that user has not defined the file encoding. <br />
        /// When any writer encounters this value , it must treat it as it was the <see cref="Binary"/> constant.
        /// </summary>
        Undefined,
        /// <summary>
        /// Defines that the file does not have any specific encoding (That is a file that contains binary data).
        /// </summary>
        Binary,
        /// <summary>
        /// The file has used the UTF-8 encoding.
        /// </summary>
        UTF8,
        /// <summary>
        /// The file has used the little-endian UTF-16 encoding.
        /// </summary>
        UTF16LE,
        /// <summary>
        /// The file has used the big-endian UTF-16 encoding.
        /// </summary>
        UTF16BE,
        /// <summary>
        /// The file has used the little-endian UTF-32 encoding.
        /// </summary>
        UTF32LE,
        /// <summary>
        /// The file has used the big-endian UTF-16 encoding.
        /// </summary>
        UTF32BE
    }
}
