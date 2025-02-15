

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// A file reference implementation shared by many classes.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal sealed class InternalFileReference : IDynamicFileReference
    {
        private System.Type type;
        private System.String filename;
        private System.Boolean iscloned;
        private FileReferenceEncoding encoding;
        private IFileReferenceVariablePropertyStore ps;
        private IFileReferenceTypeAliasResolver resolver;

        internal InternalFileReference()
        {
            iscloned = false;
            // Define an empty property store instead.
            ps = new Collections.MemoryBackingVariablePropertyStore();
            resolver = new BasicFileReferenceTypeAliasResolver();
        }

        public static InternalFileReference Clone(IFileReference reference)
            => new() { iscloned = true, encoding = reference.Encoding, filename = reference.FileName, type = reference.SavingType };

        public static InternalFileReference ParseFromSerializedString(System.String dat)
        {
            System.String[] strings = dat.Split(';');
            if (strings.Length != 3) { throw new System.ArgumentException($"The serialized string did not had 3 components. Found {strings.Length} components instead." , nameof(dat)); }
            return new() {
                FileName = Internal.ParserHelpers.RemoveQuotes(strings[0]),
                SavingType = System.Type.GetType(strings[1], true, true),
                Encoding = Internal.ParserHelpers.ParseEnumerationConstant<FileReferenceEncoding>(strings[2])
            };
        }

        public static InternalFileReference ParseFromSerializedString(System.String dat , IFileReferenceTypeAliasResolver resolver)
        {
            System.String[] strings = dat.Split(';');
            if (strings.Length != 3) { throw new System.ArgumentException($"The serialized string did not had 3 components. Found {strings.Length} components instead.", nameof(dat)); }
            System.Type st = null;
            try
            {
                st = System.Type.GetType(strings[1], true, true);
            } catch (System.TypeLoadException tle)
            {
                try {
                    st = resolver.ResolveAlias(strings[1]);
                } catch (System.ArgumentException) { throw tle; }
            }
            return new() {
                FileName = Internal.ParserHelpers.RemoveQuotes(strings[0]),
                SavingType = st,
                Encoding = Internal.ParserHelpers.ParseEnumerationConstant<FileReferenceEncoding>(strings[2])
            };
        }

#if WINDOWS10_0_19041_0_OR_GREATER || NET472_OR_GREATER
        public static InternalFileReference FromResXFileReference(System.Resources.ResXFileRef fr)
               => new() { 
                   encoding = fr.TextFileEncoding is null ? FileReferenceEncoding.Undefined : fr.TextFileEncoding.AsFileEncoding(),
                   filename = fr.FileName, type = System.Type.GetType(fr.TypeName, false, true) };
#endif

        public System.Boolean IsCloned => iscloned;

        public System.String FileName
        {
            get => filename;
            set => filename = value;
        }

        public System.Type SavingType
        {
            get => type;
            set => type = value;
        }

        public FileReferenceEncoding Encoding
        {
            get => encoding;
            set => encoding = value;
        }

        public IFileReferenceVariablePropertyStore PropertyStore
        {
            get => ps;
            set {
                if (value is null) { return; }
                ps = value;
            }
        }

        public IFileReferenceTypeAliasResolver TypeAliasResolver
        {
            get => resolver;
            set {
                if (value is null) { return; }
                resolver = value;
            }
        }

        private string GetDebuggerDisplay() => this.ToSerializedString();
    }
}