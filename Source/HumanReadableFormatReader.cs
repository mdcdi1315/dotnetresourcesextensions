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
                    (FileReferenceEncoding)System.Enum.Parse(typeof(FileReferenceEncoding) , strings[2]));
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

        private static System.Int64 ReadInt64OrFail(System.Object data)
        {
            if (data is System.Int64 value) { return value; }
            throw new FormatException("The value is not a numeric value.");
        }

        private static System.Int32 ReadInt32OrFail(System.Object data) => ReadInt64OrFail(data).ToInt32();

        private TypedResEntry ReadNext()
        {
            System.Byte[] ReadDataArray(Dictionary<System.String , System.Object> properties) {
                if (properties.ContainsKey("size") == false || properties.ContainsKey("alignment") == false || 
                    properties.ContainsKey("chunks") == false) { return null; }
                return reader.fr.ReadBase64ChunksValue(
                    ReadInt32OrFail(properties["size"]), 
                    ReadInt32OrFail(properties["alignment"]), 
                    ReadInt32OrFail(properties["chunks"]));
            }
            System.String ReadStringValue(Dictionary<System.String , System.Object> props) 
            {
                System.Int32 length = props.TryGetValue("length", out object? value) ? ReadInt32OrFail(value) : 0;
                System.String result = System.String.Empty , str;
                if (length > 0) {
                    reader.fr.SkipInitialTabsOrSpaces();
                    result = reader.fr.ReadExactlyAndConvertToString(length);
                    reader.fr.Position++; // Required so as to avoid the not-read \n and to help ReadLine read the next line successfully.
                    if (reader.fr.ReadLine() != "end value") {
                        throw new FormatException($"Expected the value to be ended , but the resource value does still continue. Have you forgotten to close the resource?");
                    }
                } else {
                    while ((str = reader.fr.ReadLine()) is not null)
                    {
                        if (str.Equals("end value")) {
                            // The below statement covers cases where strings 'have not newlines'.
                            if (str.FindCharOccurence('\n') == 1) { result = result.Remove(result.Length - 1); }
                            break;
                        }
                        result += str + "\n"; // The \n is omitted when reading with this way , add it explicitly.
                    }
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
                if (!str.Equals("begin value")) { HumanReadableFormatReader.AddProperty(props, str); }
                else {
                    switch (props["type"]) {
                        case "string":
                            entry = new(props["name"].ToString(), ReadStringValue(props));
                            break;
                        case "filereference":
                            // We have the file reference string , let's parse it.
                            TypedFileRef fr = TypedFileRef.ParseFromSerializedString(ReadStringValue(props));
                            if (fr.FileNameIsHttpUri()) {
                                throw new HumanReadableFormatException("The human-readable format reader does not currently support downloading data from the Internet." , ParserErrorType.Deserialization);
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
        public override void Reset()
        {
            reader.fr.Position = reader.pos;
        }
    }

    /// <summary>
    /// The Human Readable format is a enough fast method for saving any resources to a way that is
    /// enough understandable by humans. <br />
    /// The reader is adequately fast and can handle adequately fast ~200 KB of data per resource. <br />
    /// This is the reader counterpart.
    /// </summary>
    public sealed class HumanReadableFormatReader : IDotNetResourcesExtensionsReader
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

        internal static System.Boolean StringsMatch(System.String one , System.String two) => one.Equals(two , StringComparison.InvariantCultureIgnoreCase);

        internal static void AddProperty(IDictionary<System.String, System.Object> properties, System.String value)
        {
            static System.Boolean GetNumber(System.String data, out System.Int64 num)
            {
                [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                static System.Boolean IsDigit(System.Char ch) => (System.UInt32)(ch - '0') <= ('9' - '0');
                if (data.Length > 0 && IsDigit(data[0]))
                {
                    num = ParserHelpers.ToNumber(data);
                    return true;
                }
                num = 0;
                return false;
            }
            System.Int32 eqindex = value.IndexOf('=');
            if (eqindex > -1)
            {
                System.String name = value.Remove(eqindex - 1);
                if (properties.ContainsKey(name)) { return; }
                System.Object propval = null;
                System.String data = value.Substring(eqindex + 2);
                if (GetNumber(data, out System.Int64 number)) { propval = number; } else { propval = data; }
                data = null;
                properties.Add(name, propval);
                propval = null;
                name = null;
            }
        }

        private void ReadHeader() 
        {
            System.Boolean cond = true;
            System.String dt;
            Dictionary<System.String, System.Object> properties = new();
            while ((dt = fr.ReadLine()) is not null && cond) 
            {
                if (StringsMatch(dt , "begin header")) { continue; } // Correct header if this is true
                if (StringsMatch(dt , "end header")) { cond = false; break; }
                AddProperty(properties, dt);
            }
            if (properties.Count != 2) { throw new HumanReadableFormatException($"This is not the custom human-readable format. Error occured." , ParserErrorType.Header); }
            if (properties["version"] is System.Int64 number) {
                if (number > 1) {
                    throw new HumanReadableFormatException("The version read is higher than the version that this reader can read." , ParserErrorType.Versioning);
                }
            } else {
                throw new HumanReadableFormatException($"Cannot parse the numeric value defined in 'version' header. Internal error occured." , ParserErrorType.Header);
            }
            if (properties["magic"].ToString() != "mdcdi1315.HRFMT") { throw new HumanReadableFormatException("The magic value given is incorrect." , ParserErrorType.Header); }
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
