using System;
using System.IO;
using System.Collections.Generic;
using DotNetResourcesExtensions.Internal;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions
{

    namespace Internal 
    {
        internal static class HumanReadableFormatConstants
        {
            public sealed record class Property
            {
                private readonly string name;
                private readonly object value;

                public Property(System.String name, object value) 
                {
                    this.name = name;
                    this.value = value;
                }

                public static Property GetProperty(IDictionary<System.String , System.Object> data , System.String Name)
                {
                    if (data is null) { throw new ArgumentNullException(nameof(data)); }
                    if (Name is null) { throw new ArgumentNullException(nameof(Name)); }

                    if (!data.TryGetValue(Name, out System.Object? value)) { throw new ArgumentException($"The {Name} property does not exist in the dictionary."); }

                    return new Property(Name , value);
                }

                public System.String Name { get => name; }

                public System.String StringValue => value?.ToString();

                public System.Int64 Int64Value => ReadInt64OrFail(value);

                public System.Int32 Int32Value => ReadInt32OrFail(value);

                public void WriteToStreamAsTabbed(System.IO.StringableStream stream , System.Byte tabs)
                {
                    stream.WriteTabbedStringLine(tabs , $"{name} = {value}");
                }

                public override int GetHashCode() => base.GetHashCode();

                public System.Boolean Equals(Property other) => name == other.name;

                public System.Boolean ValueEquals(Property other) => value.Equals(other.value);

                public override string ToString() => $"Property: {{ Name: {name} Value: {value} }}";
            }

            static HumanReadableFormatConstants() {
                Version = new("version", (System.Int64)1);
                SchemaName = new("schema" , "mdcdi1315.HRFMT");
                TypeIsString = new("type" , "string");
                TypeIsByteArray = new("type", "bytearray");
                TypeIsFileRef = new("type", "filereference");
                TypeIsSerObj = new("type", "serobject");
            }

            public static readonly Property Version , SchemaName , TypeIsString , TypeIsSerObj , TypeIsByteArray , TypeIsFileRef;

            public static System.Int64 ReadInt64OrFail(System.Object data)
            {
                if (data is System.Int64 value) { return value; }
                throw new FormatException("The value is not a numeric value.");
            }

            public static System.Int32 ReadInt32OrFail(System.Object data) => ReadInt64OrFail(data).ToInt32();

            internal static System.Boolean StringsMatch(System.String one, System.String two) => one.Equals(two, StringComparison.InvariantCultureIgnoreCase);

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
        }
    }

    /// <summary>
    /// Defines a writer for human-readable resource format. <br />
    /// This format will be even closer to human understanding and reading by defining some simple constructs.
    /// </summary>
    public sealed class HumanReadableFormatWriter : IDotNetResourcesExtensionsWriter
    {
        private System.IO.StringableStream fw;
        private System.IO.Stream stream;
        private System.Text.Encoding encoding;
        private ExtensibleFormatter formatter;
        private System.Boolean strmown;

        /// <summary>
        /// Gets or sets a value whether this class controls the lifetime of the underlying stream.
        /// </summary>
        public bool IsStreamOwner { get => strmown; set => strmown = value; }

        private HumanReadableFormatWriter() {
            formatter = ExtensibleFormatter.Create();
            encoding = System.Text.Encoding.UTF8;
            strmown = false;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="HumanReadableFormatWriter"/> class using the specified stream as the underlying stream for writing data.
        /// </summary>
        /// <param name="stream">The stream to save the created resources to.</param>
        public HumanReadableFormatWriter(System.IO.Stream stream) : this() {
            this.stream = stream;
            fw = new(this.stream , encoding);
            WriteHeader();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="HumanReadableFormatWriter"/> class using the specified file where the resulting data will be saved.
        /// </summary>
        /// <param name="path">The file path to save the resulting data. The file will be overwritten if it does already exist.</param>
        public HumanReadableFormatWriter(System.String path)
         : this(new System.IO.FileStream(path , System.IO.FileMode.Create)) { strmown = true; }

        private void BeginResource(System.String name)
        {
            ParserHelpers.ValidateName(name);
            fw.WriteStringLine("begin resource");
            new HumanReadableFormatConstants.Property("name", name).WriteToStreamAsTabbed(fw, 1);
        }

        private void EndResource() => fw.WriteStringLine("end resource");

        private void WriteHeader()
        {
            fw.WriteStringLine("begin header");
            HumanReadableFormatConstants.Version.WriteToStreamAsTabbed(fw, 1);
            HumanReadableFormatConstants.SchemaName.WriteToStreamAsTabbed(fw, 1);
            fw.WriteStringLine("end header");
        }

        /// <inheritdoc/>
        public void AddResource(string name, byte[]? value)
        {
            if (value is null) { throw new System.ArgumentNullException(nameof(value)); }
            BeginResource(name);
            HumanReadableFormatConstants.TypeIsByteArray.WriteToStreamAsTabbed(fw, 1);
            fw.WriteBase64ChunksValue(value);
            EndResource();
        }

        /// <inheritdoc/>
        public void AddResource(string name, object? value)
        {
            if (value is null) { throw new System.ArgumentNullException(nameof(value)); }
            else if (value is System.String str) {
                AddResource(name, str);
                return;
            } else if (value is byte[] bytes) {
                AddResource(name, bytes);
                return;
            }
            BeginResource(name);
            HumanReadableFormatConstants.TypeIsSerObj.WriteToStreamAsTabbed(fw, 1);
            fw.WriteTabbedStringLine(1, $"dotnettype = \"{value.GetType().AssemblyQualifiedName}\"");
            fw.WriteBase64ChunksValue(formatter.GetBytesFromObject(value));
            EndResource();
        }

        /// <inheritdoc/>
        public void AddResource(string name, string? value)
        {
            if (value is null) { throw new System.ArgumentNullException(nameof(value)); }
            BeginResource(name);
            HumanReadableFormatConstants.TypeIsString.WriteToStreamAsTabbed(fw, 1);
            System.Byte[] bt = encoding.GetBytes(value);
            new HumanReadableFormatConstants.Property("length", bt.LongLength).WriteToStreamAsTabbed(fw, 1);
            fw.WriteTabbedStringLine(1, "begin value");
            fw.WriteTabs(2);
            fw.Write(bt , 0 , bt.Length);
            fw.WriteNewLine();
            fw.WriteTabbedStringLine(1, "end value");
            EndResource();
        }

        /// <summary>
        /// Adds a file reference that when it will be read , it's data will be returned.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <param name="reference">The reference to add.</param>
        public void AddFileReference(System.String name , IFileReference reference)
        {
            BeginResource(name);
            System.Byte[] serialized = encoding.GetBytes(reference.ToSerializedString());
            HumanReadableFormatConstants.TypeIsFileRef.WriteToStreamAsTabbed(fw, 1);
            new HumanReadableFormatConstants.Property("length", serialized.Length).WriteToStreamAsTabbed(fw , 1);
            fw.WriteTabbedStringLine(1, "begin value");
            fw.WriteTabs(2);
            fw.Write(serialized, 0, serialized.Length);
            fw.WriteTabbedStringLine(1, "end value");
            EndResource();
        }

        /// <inheritdoc/>
        public void Close() { Generate(); fw?.Close(); if (strmown) { stream?.Close(); } }

        /// <inheritdoc/>
        public void Generate() { fw?.Flush(); stream?.Flush(); }

        /// <inheritdoc/>
        public void Dispose()
        {
            Close();
            try { fw?.Dispose(); } catch { }
            fw = null;
            if (strmown) { stream?.Dispose(); }
            stream = null;
            encoding = null;
            formatter?.Dispose();
            formatter = null;
        }

        /// <inheritdoc/>
        public void RegisterTypeResolver(ITypeResolver resolver) => formatter.RegisterTypeResolver(resolver);
    }
}
