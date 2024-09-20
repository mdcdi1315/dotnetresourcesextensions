using DotNetResourcesExtensions.Internal;

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

    /// <summary>
    /// Defines common extensions for <see cref="IFileReference"/> instances.
    /// </summary>
    public static class IFileReferenceExtensions
    {
        [System.Diagnostics.DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
        private sealed class TypedFileReference : IFileReference
        {
            private System.String filename;
            private System.Type type;
            private FileReferenceEncoding encoding;
            private System.Boolean iscloned;

            private TypedFileReference() { iscloned = false; }

            public static TypedFileReference Clone(IFileReference reference)
                => new() { iscloned = true, encoding = reference.Encoding , filename = reference.FileName , type = reference.SavingType };

#if WINDOWS10_0_19041_0_OR_GREATER || NET472_OR_GREATER
            public static TypedFileReference FromResXFileReference(System.Resources.ResXFileRef fr)
               => new() { 
                   encoding = fr.TextFileEncoding is null ? FileReferenceEncoding.Undefined : fr.TextFileEncoding.AsFileEncoding(),
                   filename = fr.FileName, type = System.Type.GetType(fr.TypeName, false, true) };
#endif

            public System.Boolean IsCloned => iscloned;

            public System.String FileName => filename;

            public System.Type SavingType => type;

            public FileReferenceEncoding Encoding => encoding;

            private string GetDebuggerDisplay() => this.ToSerializedString();
        }

        /// <summary>
        /// Defines a simple encoding for binary files. This is meant to be returned by the extension methods only.
        /// </summary>
        private sealed class BinaryEncoding : System.Text.Encoding
        {
            public override int GetByteCount(char[] chars, int index, int count) => count;

            public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
            {
                System.Int32 bi = byteIndex , wb = 0;
                try {
                    for (System.Int32 I = charIndex; I < charCount + charIndex; I++, bi++)
                    {
                        bytes[bi] = chars[I].ToByte();
                        wb++;
                    }
                } catch { }
                return wb - 1;
            }

            public override int GetCharCount(byte[] bytes, int index, int count)
            {
                for (int I = 0; I < count; I++)
                {
                    if (bytes[index + I] == 0) { return I; }
                }
                return count;
            }

            public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
            {
                System.Int32 J = byteIndex;
                try {
                    for (System.Int32 I = charIndex; I < chars.Length && J < byteIndex + byteCount; I++, J++)
                    {
                        chars[I] = bytes[J].ToChar(); 
                    }
                } catch { }
                return J - 1;
            }

            public override int GetMaxByteCount(int charCount) => charCount;

            public override int GetMaxCharCount(int byteCount) => byteCount;
        }

        /// <summary>
        /// If the <see cref="IFileReference.FileName"/> represents a relative path , this method returns an absolute path to the represented file.
        /// </summary>
        /// <param name="reference">The file reference to get the file name.</param>
        /// <returns>The <see cref="IFileReference.FileName"/> , but represented as a fully qualified path.</returns>
        public static System.String AsFullPath(this IFileReference reference)
        {
            System.String ret = System.String.Empty;
            if (System.String.IsNullOrEmpty(reference.FileName)) { return ret; }
            if (reference.FileName.Length == 0) { return ret; }
            ret = Internal.ParserHelpers.RemoveQuotes(reference.FileName);
            return System.IO.Path.GetFullPath(ret);
        }

        /// <summary>
        /// Opens a <see cref="System.IO.FileStream"/> to the specified file name so as to read it.
        /// </summary>
        /// <param name="reference">The file reference to use.</param>
        /// <returns>The opened read-only file stream.</returns>
        /// <exception cref="System.IO.FileNotFoundException">The file name specified in this reference was not found.</exception>
        public static System.IO.FileStream OpenStreamToFile(this IFileReference reference) {
            System.IO.FileInfo info = GetFileNameInfo(reference);
            if (info.Exists == false) { throw new System.IO.FileNotFoundException(System.String.Format(Properties.Resources.DNTRESEXT_FILEREFEXT_OPENFILE_FILENOTFOUND , info.Name) , reference.FileName); }
            return info.OpenRead();
        }

        /// <summary>
        /// Gets the file information for the file specified in the <see cref="IFileReference.FileName"/> property.
        /// </summary>
        /// <param name="reference">The file reference to use.</param>
        /// <returns>The file information on disk.</returns>
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public static System.IO.FileInfo GetFileNameInfo(this IFileReference reference) => new(AsFullPath(reference));

        /// <summary>
        /// Returns the <see cref="IFileReference.Encoding"/> property as a instance of a <see cref="System.Text.Encoding"/>.
        /// </summary>
        /// <param name="reference">The file reference to get the encoding information.</param>
        /// <returns>The equivalent instance that can represent the <see cref="IFileReference.Encoding"/> property.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">The <see cref="IFileReference.Encoding"/> property was out of the range of the currently defined constants.</exception>
        public static System.Text.Encoding AsEncoding(this IFileReference reference) => AsEncoding(reference.Encoding);

        /// <summary>
        /// Returns a equivalent encoding for any valid constant inside the <see cref="FileReferenceEncoding"/> enumeration.
        /// </summary>
        /// <param name="encoding">The constant which to return a valid encoding for.</param>
        /// <returns>The equivalent instance that can represent the <paramref name="encoding"/> parameter.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">The <paramref name="encoding"/> parameter was out of the range of the currently defined constants.</exception>
        public static System.Text.Encoding AsEncoding(this FileReferenceEncoding encoding)
          => encoding switch {
                FileReferenceEncoding.Undefined => new BinaryEncoding(),
                FileReferenceEncoding.Binary => new BinaryEncoding(),
                FileReferenceEncoding.UTF8 => System.Text.Encoding.UTF8,
                FileReferenceEncoding.UTF16LE => new System.Text.UnicodeEncoding(false, false),
                FileReferenceEncoding.UTF16BE => new System.Text.UnicodeEncoding(true, false),
                FileReferenceEncoding.UTF32LE => new System.Text.UTF32Encoding(false, false),
                FileReferenceEncoding.UTF32BE => new System.Text.UTF32Encoding(true, false),
                _ => throw new System.ArgumentOutOfRangeException(nameof(encoding) , Properties.Resources.DNTRESEXT_FILEREFENC_OOR)
        };

        /// <summary>
        /// If the <see cref="IFileReference.FileName"/> property contains a valid HTTP URI , it returns <see langword="true"/>.
        /// </summary>
        /// <param name="reference">The file reference to test for the condition.</param>
        /// <returns><see langword="true"/> when the <see cref="IFileReference.FileName"/> property is a valid HTTP URI; otherwise , <see langword="false"/>.</returns>
        public static System.Boolean FileNameIsHttpUri(this IFileReference reference) => reference.FileName.StartsWith("http://") || reference.FileName.StartsWith("https://");

        /// <summary>
        /// Directly serializes the data of this file reference to a human-readable string.
        /// </summary>
        /// <param name="reference">The file reference to return the serialized string for.</param>
        /// <returns>A human-readable serialzed string that represents this file reference.</returns>
        /// <exception cref="System.AggregateException">One or more unexpected exceptions occured during serialization. See the <see cref="System.AggregateException.InnerExceptions"/> property's contents at runtime for more information.</exception>
        public static System.String ToSerializedString(this IFileReference reference)
        {
            System.String result = System.String.Empty;
            try {
                if (FileNameIsHttpUri(reference)) { 
                    result = reference.FileName;
                } else { result = $"\"{AsFullPath(reference)}\""; }
                result += $";{reference.SavingType.AssemblyQualifiedName};{(reference.Encoding == FileReferenceEncoding.Undefined ? FileReferenceEncoding.Binary : reference.Encoding)}";
            } catch (System.Exception e) {
                throw new System.AggregateException(Properties.Resources.DNTRESEXT_FILEREFEXT_SERSTRING_FAILED, e);
            }
            return result;
        }

        /// <summary>
        /// Gets an equivalent enumerated constant of <see cref="FileReferenceEncoding"/> from any valid <see cref="System.Text.Encoding"/> instance.
        /// </summary>
        /// <param name="encoding">The encoding instance to get the equivalent constant of <see cref="FileReferenceEncoding"/>.</param>
        /// <returns>A equivalent enumerated constant that is equal to <paramref name="encoding"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="encoding"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="encoding"/> had an instance which the <see cref="FileReferenceEncoding"/> enumeration does not contain.</exception>
        public static FileReferenceEncoding AsFileEncoding(this System.Text.Encoding encoding)
        {
            switch (encoding)
            {
                case null:
                    throw new System.ArgumentNullException(nameof(encoding));
                case System.Text.UTF8Encoding:
                    return FileReferenceEncoding.UTF8;
                case BinaryEncoding:
                    return FileReferenceEncoding.Binary;
                case System.Text.UnicodeEncoding en:
                    if (en.CodePage == 1201) { return FileReferenceEncoding.UTF16BE; } else { return FileReferenceEncoding.UTF16LE; }
                case System.Text.UTF32Encoding en:
                    if (en.CodePage == 12001) { return FileReferenceEncoding.UTF32BE; } else { return FileReferenceEncoding.UTF32LE; }
                default:
                    throw new System.ArgumentException(System.String.Format(Properties.Resources.DNTRESEXT_FILEREFENC_INVALIDENC , encoding.EncodingName));
            }
        }

        /// <summary>
        /// Clones the specified file <paramref name="reference"/> to a new file reference.
        /// </summary>
        /// <param name="reference">The file reference to clone.</param>
        /// <returns>The cloned file reference.</returns>
        public static IFileReference Clone(this IFileReference reference) => TypedFileReference.Clone(reference);

        /// <summary>
        /// Gets a value whether the current file reference is a cloned reference.
        /// </summary>
        /// <param name="reference">The file reference to test.</param>
        /// <returns><see langword="true"/> if this file reference is a cloned instance; otherwise , <see langword="false"/>.</returns>
        public static System.Boolean IsCloned(this IFileReference reference) => reference is TypedFileReference dt && dt.IsCloned;

#if WINDOWS10_0_19041_0_OR_GREATER || NET472_OR_GREATER
        /// <summary>
        /// Reinterprets the specified <see cref="System.Resources.ResXFileRef"/> as a instance of the <see cref="IFileReference"/> interface.
        /// </summary>
        /// <param name="reference">The ResX file reference to reinterpret.</param>
        /// <returns>An equivalent <see cref="IFileReference"/> instance.</returns>
        public static IFileReference AsFileReference(this System.Resources.ResXFileRef reference) => TypedFileReference.FromResXFileReference(reference);
#endif
    }
}
