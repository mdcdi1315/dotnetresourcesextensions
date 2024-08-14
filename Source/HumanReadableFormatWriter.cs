using System.IO;
using DotNetResourcesExtensions.Internal;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions
{
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
            fw.WriteTabbedStringLine(1, $"name = {name}");
        }

        private void EndResource() => fw.WriteStringLine("end resource");

        private void WriteHeader()
        {
            fw.WriteStringLine("begin header");
            fw.WriteTabbedStringLine(1, "version = 1");
            fw.WriteTabbedStringLine(1, "magic = mdcdi1315.HRFMT");
            fw.WriteStringLine("end header");
        }

        /// <inheritdoc/>
        public void AddResource(string name, byte[]? value)
        {
            if (value is null) { throw new System.ArgumentNullException(nameof(value)); }
            BeginResource(name);
            fw.WriteTabbedStringLine(1, "type = bytearray");
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
            fw.WriteTabbedStringLine(1, "type = serobject");
            fw.WriteTabbedStringLine(1, $"dotnettype = \"{value.GetType().AssemblyQualifiedName}\"");
            fw.WriteBase64ChunksValue(formatter.GetBytesFromObject(value));
            EndResource();
        }

        /// <inheritdoc/>
        public void AddResource(string name, string? value)
        {
            if (value is null) { throw new System.ArgumentNullException(nameof(value)); }
            BeginResource(name);
            fw.WriteTabbedStringLine(1, "type = string");
            System.Byte[] bt = encoding.GetBytes(value);
            fw.WriteTabbedStringLine(1, $"length = {bt.LongLength}");
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
            fw.WriteTabbedStringLine(1, "type = filereference");
            fw.WriteTabbedStringLine(1, $"length = {serialized.Length}");
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
