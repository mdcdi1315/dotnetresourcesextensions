using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using DotNetResourcesExtensions.Internal;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// This is the enumerator implementation for the <see cref="HumanReadableFormatReader"/>. <br />
    /// You cannot create an instance of this class; Use , instead , the <see cref="HumanReadableFormatReader.GetEnumerator"/> method
    /// which returns an instance of this class.
    /// </summary>
    public sealed class HumanReadableFormatEnumerator : Collections.AbstractDualResourceEntryEnumerator
    {
        private sealed class TypedResEntry : IResourceEntry
        {
            private readonly System.String _name;
            private readonly System.Object _value;

            public TypedResEntry(string name, object value)
            {
                _name = name;
                _value = value;
            }

            public System.String Name => _name;

            public System.Object Value => _value;

            public System.Type TypeOfValue => _value?.GetType();

            public override string ToString() => $"\'{_name}\': {_value}";
        }

        private sealed class TypedFileRef : IFileReference
        {
            private readonly System.String name;
            private readonly System.Type type;
            private readonly FileReferenceEncoding encoding;

            public TypedFileRef(string name, Type type, FileReferenceEncoding encoding)
            {
                this.name = name;
                this.type = type;
                this.encoding = encoding;
            }

            public static TypedFileRef ParseFromSerializedString(System.String dat)
            {
                System.String[] strings = dat.Split(';');
                return new(ParserHelpers.RemoveQuotes(strings[0]),
                    System.Type.GetType(strings[1] , true , true),
                    ParserHelpers.ParseEnumerationConstant<FileReferenceEncoding>(strings[2]));
            }

            public string FileName => name;
            public FileReferenceEncoding Encoding => encoding;
            public System.Type SavingType => type;
        }

        private HumanReadableFormatReader reader;
        private TypedResEntry entry;
        
        internal HumanReadableFormatEnumerator(HumanReadableFormatReader reader)
        {
            this.reader = reader;
            entry = null;
        }

        private TypedResEntry ReadNext()
        {
            System.Byte[] ReadDataArray(Dictionary<System.String , System.Object> properties) {
                System.Int32 size = 0, alignment = 0, chunks = 0;
                try {
                    size = HumanReadableFormatConstants.Property.GetProperty(properties, "size").Int32Value;
                    alignment = HumanReadableFormatConstants.Property.GetProperty(properties, "alignment").Int32Value;
                    chunks = HumanReadableFormatConstants.Property.GetProperty(properties, "chunks").Int32Value;
                } catch (System.ArgumentException) {
                    return null;
                }
                return reader.fr.ReadBase64ChunksValue(size , alignment , chunks);
            }
            System.String ReadStringValue(Dictionary<System.String , System.Object> props) 
            {
                System.Int32 length = props.TryGetValue("length", out object? value) ? HumanReadableFormatConstants.ReadInt32OrFail(value) : 0;
                System.String result = System.String.Empty , str;
                reader.fr.SkipInitialTabsOrSpaces();
                if (length > 0) {
                    result = reader.fr.ReadExactlyAndConvertToString(length);
                    reader.fr.Position++; // Required so as to avoid the not-read \n and to help ReadLine read the next line successfully.
                    if (reader.fr.ReadLine() != "end value") {
                        throw new FormatException(Properties.Resources.DNTRESEXT_HRFMT_INVALID_RESOURCEVAL_LAYOUT);
                    }
                } else {
                    List<System.String> data = new();
                    while ((str = reader.fr.ReadLiteralLine()) is not null)
                    {
                        if (str.EqualsWithoutLeadingWhiteSpace("end value")) { break; }
                        data.Add(str);
                    }
                    for (System.Int32 I = 0; I < (data.Count-1); I++) { result += $"{data[I]}\n"; }
                    result += data[data.Count-1];
                    data.Clear();
                    data = null;
                }
                return result;
            }
            System.String str;
            System.Boolean closed = false;
            TypedResEntry entry = null;
            System.Byte[] bt;
            Dictionary<System.String, System.Object> props = new();
            while ((str = reader.fr.ReadLine()) is not null) {
                if (str.Equals("begin resource")) { continue; }
                if (str.Equals("end resource")) { closed = true; break; }
                if (!str.Equals("begin value")) { HumanReadableFormatConstants.AddProperty(props, str); }
                else {
                    switch (props["type"]) {
                        case "string":
                            entry = new(props["name"].ToString(), ReadStringValue(props));
                            break;
                        case "filereference":
                            // We have the file reference string , let's parse it.
                            TypedFileRef fr = TypedFileRef.ParseFromSerializedString(ReadStringValue(props));
                            if (fr.FileNameIsHttpUri()) {
                                throw new HumanReadableFormatException(Properties.Resources.DNTRESEXT_HRFMT_FILEREF_DOWNLOAD_UNSUPPORTED, ParserErrorType.Deserialization);
                            }
                            // OK. Now read the file and get the object.
                            System.IO.FileStream FS = null;
                            try {
                                FS = fr.OpenStreamToFile();
                                bt = new System.Byte[FS.Length];
                                System.Int32 rb = FS.Read(bt, 0, bt.Length);
                                switch (fr.SavingType.FullName) {
                                    case "System.String":
                                        entry = new(props["name"].ToString(), fr.Encoding.AsEncoding().GetString(bt, 0, rb));
                                        break;
                                    case "System.Byte[]":
                                        entry = new(props["name"].ToString(), bt);
                                        break;
                                    default:
                                        entry = new(props["name"].ToString(), reader.formatter.GetObjectFromBytes(bt , fr.SavingType));
                                        break;
                                }
                            } finally {
                                FS?.Dispose();
                                fr = null;
                            }
                            break;
                        case "bytearray":
                            entry = new(props["name"].ToString(), ReadDataArray(props));
                            break;
                        case "serobject":
                            bt = ReadDataArray(props);
                            entry = new(props["name"].ToString(), reader.formatter.GetObjectFromBytes(bt,
                                System.Type.GetType(ParserHelpers.RemoveQuotes(props["dotnettype"].ToString()), true, false)));
                            bt = null;
                            break;
                    }
                    continue;
                }
            }
            if (closed == false) { return null; }
            props.Clear();
            props = null;
            return entry;
        }

        /// <inheritdoc />
        public override IResourceEntry ResourceEntry => entry;

        /// <inheritdoc />
        public override DictionaryEntry Entry => new(ResourceEntry.Name , ResourceEntry.Value);

        /// <inheritdoc />
        public override object Key => ResourceEntry.Name;

        /// <inheritdoc />
        public override object Value => ResourceEntry.Value;

        /// <inheritdoc />
        public override bool MoveNext()
        {
            System.Boolean cond = reader.fr.Position < reader.fr.Length;
            if (cond) { entry = ReadNext(); }
            if (entry is null) { cond = false; }
            return cond;
        }

        /// <inheritdoc />
        public override void Reset() { reader.fr.Position = reader.pos; }
    }

    /// <summary>
    /// The Human Readable format is a enough fast method for saving any resources to a way that is
    /// enough understandable by humans. <br />
    /// The reader is adequately fast and can handle adequately fast ~200 KB of data per resource. <br />
    /// This is the reader counterpart.
    /// </summary>
    public sealed class HumanReadableFormatReader : IDotNetResourcesExtensionsReader , Collections.IResourceEntryImplementable
    {
        private System.IO.Stream underlying;
        internal System.IO.StringableStream fr;
        private System.Boolean strmown;
        internal ExtensibleFormatter formatter;
        internal System.Int64 pos;

        /// <summary>
        /// Gets or sets a value whether this class controls the lifetime of the underlying stream.
        /// </summary>
        public bool IsStreamOwner { get => strmown; set => strmown = value; }

        private HumanReadableFormatReader() {
            strmown = false;
            formatter = ExtensibleFormatter.Create();
        }

        /// <summary>
        /// Creates a new instance of <see cref="HumanReadableFormatReader"/> by reading the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read data from.</param>
        public HumanReadableFormatReader(System.IO.Stream stream) : this() {
            underlying = stream;
            pos = underlying.Position;
            fr = new(underlying , System.Text.Encoding.UTF8);
            ReadHeader();
        }

        /// <summary>
        /// Creates a new instance of <see cref="HumanReadableFormatReader"/> by reading the specified file.
        /// </summary>
        /// <param name="path">The path to the file to read.</param>
        public HumanReadableFormatReader(System.String path) 
            : this(new System.IO.FileStream(path , FileMode.Open)) { strmown = true; }

        private void ReadHeader() 
        {
            System.Boolean cond = true;
            System.String dt;
            Dictionary<System.String, System.Object> properties = new();
            while ((dt = fr.ReadLine()) is not null && cond) 
            {
                if (HumanReadableFormatConstants.StringsMatch(dt , "begin header")) { continue; } // Correct header if this is true
                if (HumanReadableFormatConstants.StringsMatch(dt , "end header")) { cond = false; break; }
                HumanReadableFormatConstants.AddProperty(properties, dt);
            }
            if (properties.Count != 2) { throw new HumanReadableFormatException(Properties.Resources.DNTRESEXT_HRFMT_GENERIC_ERROR, ParserErrorType.Header); }
            try {
                HumanReadableFormatConstants.Property prop = HumanReadableFormatConstants.Property.GetProperty(properties, "version");
                if (prop.Int64Value > HumanReadableFormatConstants.Version.Int64Value)
                {
                    throw new HumanReadableFormatException(Properties.Resources.DNTRESEXT_HRFMT_VER_MISMATCH, ParserErrorType.Versioning);
                }
            } catch (System.Exception e) {
                throw new HumanReadableFormatException(Properties.Resources.DNTRESEXT_HRFMT_VERHDR_UNPARSEABLE, $"{e.GetType().FullName}: {e.Message}", ParserErrorType.Header);
            }
            if (HumanReadableFormatConstants.Property.GetProperty(properties , "schema").ValueEquals(HumanReadableFormatConstants.SchemaName) == false) { throw new HumanReadableFormatException($"The schema {properties["schema"]} cannot be read by this reader." , ParserErrorType.Header); }
            properties = null;
        }

        /// <inheritdoc />
        public void Close()
        {
            fr?.Close();
            if (strmown) { underlying?.Close(); }
        }

        /// <inheritdoc />
        public IDictionaryEnumerator GetEnumerator() => new HumanReadableFormatEnumerator(this);

        /// <summary>
        /// Gets an enumerator which has the ability to also get resource entries.
        /// </summary>
        public Collections.IResourceEntryEnumerator GetResourceEntryEnumerator() => new HumanReadableFormatEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public void Dispose()
        {
            fr?.Dispose();
            fr = null;
            formatter?.Dispose();
            formatter = null;
            if (strmown) { underlying?.Dispose(); underlying = null; }
        }

        /// <inheritdoc />
        public void RegisterTypeResolver(ITypeResolver resolver) => formatter.RegisterTypeResolver(resolver);
    }
}
