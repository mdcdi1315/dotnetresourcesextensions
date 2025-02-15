
using DotNetResourcesExtensions.Internal;


namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Defines common extensions for <see cref="IFileReference"/> instances.
    /// </summary>
    public static class IFileReferenceExtensions
    {
        /// <summary>
        /// Defines a simple encoding for binary files. This is meant to be returned by the extension methods only.
        /// </summary>
        private sealed class BinaryEncoding : System.Text.Encoding
        {
            public override int GetByteCount(char[] chars, int index, int count) => count;

            public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
            {
                System.Int32 bi = byteIndex, wb = 0;
                try
                {
                    for (System.Int32 I = charIndex; I < charCount + charIndex; I++, bi++)
                    {
                        bytes[bi] = chars[I].ToByte();
                        wb++;
                    }
                }
                catch { }
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
                try
                {
                    for (System.Int32 I = charIndex; I < chars.Length && J < byteIndex + byteCount; I++, J++)
                    {
                        chars[I] = bytes[J].ToChar();
                    }
                }
                catch { }
                return J - 1;
            }

            public override int GetMaxByteCount(int charCount) => charCount;

            public override int GetMaxCharCount(int byteCount) => byteCount;
        }

        /// <summary>
        /// Returns a default alias resolver that provides basic default alias mappings for critical .NET types.
        /// </summary>
        public static IFileReferenceTypeAliasResolver DefaultResolver => new BasicFileReferenceTypeAliasResolver();

        /// <summary>
        /// If the <see cref="IFileReference.FileName"/> represents a relative path , 
        /// this method returns an absolute path to the represented file. <br />
        /// If the path contains variables , these are expanded.
        /// </summary>
        /// <param name="reference">The file reference to get the file name.</param>
        /// <returns>The <see cref="IFileReference.FileName"/> , but represented as a fully qualified path , if possible.</returns>
        public static System.String AsFullPath(this IFileReference reference)
        {
            System.String ret = System.String.Empty;
            if (System.String.IsNullOrEmpty(reference.FileName)) { return ret; }
            ret = ParserHelpers.RemoveQuotes(reference.FileName);
            if (reference is InternalFileReference ifr)
            {
                ret = DecodeVarsFromIFR(ifr);
            }
            return System.IO.Path.GetFullPath(ret);
        }

        [System.Flags]
        private enum DecodeStateFlags : System.Byte
        {
            None = 0x00,
            DetectedDollarSign = 0x01,
            SaveVariable = 0x02
        }

        private static System.String DecodeVarsFromIFR(InternalFileReference reference)
        {
            // IFR reference variables will be decoded as follows:
            // $(Your-Reference-Variable) , so an MSBuild-like syntax.
            // To include the literal dollar sign , make sure to not use any opening parenthesis after the sign,
            // or include a double dollar sign if you need the next character to be a parenthesis anyways.
            System.Text.StringBuilder sb = new(reference.FileName.Length); // Initial capacity.
            DecodeStateFlags dsf = DecodeStateFlags.None;
            System.String tmpvar = System.String.Empty;
            foreach (var c in reference.FileName)
            {
                if (dsf.HasFlag(DecodeStateFlags.DetectedDollarSign))
                {
                    if (c == '$') {
                        sb.Append(c);
                        dsf = DecodeStateFlags.None;
                        continue;
                    } else if (c != '(') {
                        dsf = DecodeStateFlags.None;
                        sb.Append('$');
                        sb.Append(c);
                        continue;
                    } else {
                        dsf |= DecodeStateFlags.SaveVariable;
                        continue;
                    }
                }
                if (dsf.HasFlag(DecodeStateFlags.SaveVariable)) 
                {
                    if (c == ')') {
                        sb.Append(reference.PropertyStore.GetVariable(tmpvar));
                        tmpvar = null;
                        dsf = DecodeStateFlags.None;
                        continue;
                    }
                    tmpvar += c;
                } else if (c == '$') {
                    dsf |= DecodeStateFlags.DetectedDollarSign;
                } else {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets a fully resolved file reference saved type , from the given alias.
        /// </summary>
        /// <param name="reference">The file reference for the alias to be resolved.</param>
        /// <param name="alias">The type alias to be resolved against.</param>
        /// <returns>The fully qualified type that <paramref name="alias"/> represents.</returns>
        /// <exception cref="System.InvalidOperationException">The final type alias resolver was not valid.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="alias"/> was null or empty.</exception>
        public static System.Type GetFullyResolvedType(this IFileReference reference , System.String alias)
        {
            if (reference is null) { throw new System.NullReferenceException("Attempted to dereference a null file reference."); }
            if (System.String.IsNullOrEmpty(alias)) { throw new System.ArgumentNullException(nameof(alias)); }
            IFileReferenceTypeAliasResolver resolver;
            if (reference is IDynamicFileReference dfr) {
                resolver = dfr.TypeAliasResolver;
            } else {
                resolver = DefaultResolver;
            }
            if (resolver is null) { throw new System.InvalidOperationException("Cannot invoke the type alias resolver because it is null."); }
            return resolver.ResolveAlias(alias);
        }

        /// <summary>
        /// Gets the fully qualified type of a given type string to be set dynamically to a new file reference. <br />
        /// The type string is also tested if it is an alias , if loading fails.
        /// </summary>
        /// <param name="reference">The file reference that acts as a resolving base if an alias string is detected.</param>
        /// <param name="typestring">The type string or type alias to resolve.</param>
        /// <returns>The fully qualified type that <paramref name="typestring"/> represents.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="typestring"/> was null or empty.</exception>
        /// <exception cref="System.TypeLoadException">The type cannot be found , it is unresolvable.</exception>
        public static System.Type ResolveSavingTypeString(this IFileReference reference , System.String typestring)
        {
            if (reference is null) { throw new System.NullReferenceException("Attempted to dereference a null file reference."); }
            if (System.String.IsNullOrEmpty(typestring)) { throw new System.ArgumentNullException(nameof(typestring)); }
            System.Type type = null;
            try {
                type = System.Type.GetType(typestring, true, true);
            } catch (System.TypeLoadException tle) {
                try {
                    type = reference.GetFullyResolvedType(typestring);
                } catch {
                    throw new System.TypeLoadException($"The given type with name \'{typestring}\' cannot be identified , either as a fully qualified type string or as a type alias." , tle);
                }
            }
            return type;
        }

        /// <summary>
        /// Opens a <see cref="System.IO.FileStream"/> to the specified file name so as to read it.
        /// </summary>
        /// <param name="reference">The file reference to use.</param>
        /// <returns>The opened read-only file stream.</returns>
        /// <exception cref="System.IO.FileNotFoundException">The file name specified in this reference was not found.</exception>
        public static System.IO.FileStream OpenStreamToFile(this IFileReference reference)
        {
            System.IO.FileInfo info = GetFileNameInfo(reference);
            if (info.Exists == false) { throw new System.IO.FileNotFoundException(System.String.Format(Properties.Resources.DNTRESEXT_FILEREFEXT_OPENFILE_FILENOTFOUND, info.Name), reference.FileName); }
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
          => encoding switch
          {
              FileReferenceEncoding.Undefined => new BinaryEncoding(),
              FileReferenceEncoding.Binary => new BinaryEncoding(),
              FileReferenceEncoding.UTF8 => System.Text.Encoding.UTF8,
              FileReferenceEncoding.UTF16LE => new System.Text.UnicodeEncoding(false, false),
              FileReferenceEncoding.UTF16BE => new System.Text.UnicodeEncoding(true, false),
              FileReferenceEncoding.UTF32LE => new System.Text.UTF32Encoding(false, false),
              FileReferenceEncoding.UTF32BE => new System.Text.UTF32Encoding(true, false),
              _ => throw new System.ArgumentOutOfRangeException(nameof(encoding), Properties.Resources.DNTRESEXT_FILEREFENC_OOR)
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
                if (FileNameIsHttpUri(reference)) { result = reference.FileName; } else { result = $"\"{reference.FileName}\""; }
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
                    throw new System.ArgumentException(System.String.Format(Properties.Resources.DNTRESEXT_FILEREFENC_INVALIDENC, encoding.EncodingName));
            }
        }

        /// <summary>
        /// Clones the specified file <paramref name="reference"/> to a new file reference.
        /// </summary>
        /// <param name="reference">The file reference to clone.</param>
        /// <returns>The cloned file reference.</returns>
        public static IFileReference Clone(this IFileReference reference) => InternalFileReference.Clone(reference);

        /// <summary>
        /// Gets a value whether the current file reference is a cloned reference.
        /// </summary>
        /// <param name="reference">The file reference to test.</param>
        /// <returns><see langword="true"/> if this file reference is a cloned instance; otherwise , <see langword="false"/>.</returns>
        public static System.Boolean IsCloned(this IFileReference reference) => reference is InternalFileReference dt && dt.IsCloned;

#if WINDOWS10_0_19041_0_OR_GREATER || NET472_OR_GREATER
        /// <summary>
        /// Reinterprets the specified <see cref="System.Resources.ResXFileRef"/> as a instance of the <see cref="IFileReference"/> interface.
        /// </summary>
        /// <param name="reference">The ResX file reference to reinterpret.</param>
        /// <returns>An equivalent <see cref="IFileReference"/> instance.</returns>
        public static IFileReference AsFileReference(this System.Resources.ResXFileRef reference) => InternalFileReference.FromResXFileReference(reference);
#endif
    }

}