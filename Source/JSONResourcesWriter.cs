
using System;

namespace DotNetResourcesExtensions
{
    using Internal;
    using Internal.CustomFormatter;

    /// <summary>
    /// The <see cref="JSONResourcesWriter"/> writes a custom resources format based on structuralized JSON. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class JSONResourcesWriter : IDotNetResourcesExtensionsWriter
    {
        private ExtensibleFormatter exf;
        private System.IO.Stream targetstream;
        private System.Text.Json.Utf8JsonWriter writer;
        private System.Boolean isstreamowner;
        private StreamMixedClassManagement mgmt;

        private JSONResourcesWriter() { exf = ExtensibleFormatter.Create(); writer = null; targetstream = null; isstreamowner = false; mgmt = StreamMixedClassManagement.None; }

        /// <summary>
        /// Creates a new instance of <see cref="JSONResourcesWriter"/> with the specified stream as the data output.
        /// </summary>
        /// <param name="stream">The data stream that will be used to write data.</param>
        public JSONResourcesWriter(System.IO.Stream stream) : this()
        {
            mgmt = StreamMixedClassManagement.InitialisedWithStream;
            targetstream = stream;
            writer = new(targetstream, JSONRESOURCESCONSTANTS.JsonWriterOptions);
            CreateHeader();
        }

        /// <summary>
        /// Creates a new instance of <see cref="JSONResourcesWriter"/> with the specified file path to output.
        /// </summary>
        /// <param name="FilePath">The file path that will be used to write data.</param>
        public JSONResourcesWriter(System.String FilePath) : this()
        {
            mgmt = StreamMixedClassManagement.FileUsed;
            targetstream = new System.IO.FileStream(FilePath, System.IO.FileMode.CreateNew);
            writer = new(targetstream, JSONRESOURCESCONSTANTS.JsonWriterOptions);
            isstreamowner = true;
            CreateHeader();
        }

        /// <summary>
        /// Gets or sets a value whether this class controls the lifetime of the underlying stream.
        /// </summary>
        public System.Boolean IsStreamOwner { get => isstreamowner; set => isstreamowner = value; }

        private void CreateHeader()
        {
            writer.WriteStartObject();
            writer.WriteStartObject(JSONRESOURCESCONSTANTS.JSONHeader);
            writer.WriteNumber("Version", JSONRESOURCESCONSTANTS.Version);
            writer.WriteString("Magic", JSONRESOURCESCONSTANTS.Magic);
            writer.WriteNumber("SupportedFormatsMask", (System.Int32)JSONRESOURCESCONSTANTS.CurrentMask);
            writer.WriteNumber("CurrentHeaderVersion", JSONRESOURCESCONSTANTS.UsedHeaderVersion);
            writer.WriteEndObject();
            writer.WriteStartArray(JSONRESOURCESCONSTANTS.DataObjectName);
        }

        private void EnsureNotClosed()
        {
            if (writer is null) { throw new System.ObjectDisposedException(nameof(JSONResourcesWriter), Properties.Resources.DNTRESEXT_JSONFMT_WRITER_CLOSED); }
        }

        private void WriteResource(System.String Name, System.Object value, JSONRESResourceType rt)
        {
            EnsureNotClosed();
            if (value is null) { throw new ArgumentNullException(nameof(value)); }
            ParserHelpers.ValidateName(Name);

            switch (rt)
            {
                case JSONRESResourceType.String:
                    WriteStringResource(Name, value.ToString(), null);
                    break;
                case JSONRESResourceType.Object:
                    WriteObjectResource(Name, value, null);
                    break;
                case JSONRESResourceType.ByteArray:
                    WriteByteArrayResource(Name, (System.Byte[])value, null);
                    break;
            }
        }

        private void WriteObjectResource(System.String Name, System.Object value, System.String comment)
        {
            System.Byte[] t;
            try {
                t = exf.GetBytesFromObject(value);
            } catch (Internal.CustomFormatter.Exceptions.ConverterNotFoundException e) {
                throw new JSONFormatException(Properties.Resources.DNTRESEXT_JSONFMT_COULDNOTSER, e.Message, ParserErrorType.Serialization);
            }
            writer.WriteStartObject();
            writer.WriteNumber("HeaderVersion", JSONRESOURCESCONSTANTS.HeaderVersion);
            writer.WriteString("ResourceName", Name);
            writer.WriteString("ResourceType", JSONRESResourceType.Object.ToString());
            writer.WriteNumber("TotalLength", t.LongLength);
            writer.WriteNumber("Base64Alignment", JSONRESOURCESCONSTANTS.BASE64_SingleLineLength);
            writer.WriteString("DotnetType", value.GetType().AssemblyQualifiedName);
            if (System.String.IsNullOrEmpty(comment) == false) {
                writer.WriteString("Comment", comment);
            }
            WriteChunkBasedByteArray(t);
            t = null;
            writer.WriteEndObject();
        }

        private void WriteChunkBasedByteArray(System.Byte[] value)
        {
            System.Int64 chunks, remaining;
            System.String data = value.ToBase64(), temp;
            chunks = System.Math.DivRem(data.Length, JSONRESOURCESCONSTANTS.BASE64_SingleLineLength, out remaining);
            if (remaining > 0) { chunks++; }
            // The chunks are base64 code points and each of them provides a portion of the reconstructed information.
            // Each chunk has a length defined by BASE64_SingleLineLength. Any leftovers are covered by adding a new chunk
            // and writing to it the final information.
            writer.WriteNumber("Chunks", chunks);
            writer.WriteStartArray("Value");
            System.Int32 pos = 0;
            if (remaining > 0) {
                for (System.Int32 I = 1; I < chunks; I++)
                {
                    temp = data.Substring(pos, JSONRESOURCESCONSTANTS.BASE64_SingleLineLength);
                    writer.WriteStringValue(temp);
                    pos += JSONRESOURCESCONSTANTS.BASE64_SingleLineLength;
                }
                temp = data.Substring(pos);
                writer.WriteStringValue(temp);
            } else {
                for (System.Int32 I = 1; I <= chunks; I++)
                {
                    temp = data.Substring(pos, JSONRESOURCESCONSTANTS.BASE64_SingleLineLength);
                    writer.WriteStringValue(temp);
                    pos += JSONRESOURCESCONSTANTS.BASE64_SingleLineLength;
                }
            }
            writer.WriteEndArray();
            data = null; temp = null;
        }

        private void WriteByteArrayResource(System.String Name, System.Byte[] value, System.String comment)
        {
            writer.WriteStartObject();
            // Changing again method in Version 3 because accessing properties is really much slow...
            writer.WriteNumber("HeaderVersion", JSONRESOURCESCONSTANTS.HeaderVersion);
            writer.WriteString("ResourceName", Name);
            writer.WriteString("ResourceType", JSONRESResourceType.ByteArray.ToString());
            writer.WriteNumber("TotalLength", value.LongLength);
            writer.WriteNumber("Base64Alignment", JSONRESOURCESCONSTANTS.BASE64_SingleLineLength);
            if (System.String.IsNullOrEmpty(comment) == false) {
                writer.WriteString("Comment", comment);
            }
            WriteChunkBasedByteArray(value);
            writer.WriteEndObject();
        }

        private void WriteStringResource(System.String Name, System.String value, System.String comment)
        {
            writer.WriteStartObject();
            writer.WriteNumber("HeaderVersion", JSONRESOURCESCONSTANTS.HeaderVersion);
            writer.WriteString("ResourceName", Name);
            writer.WriteString("ResourceType", JSONRESResourceType.String.ToString());
            writer.WriteString("Value", value);
            if (System.String.IsNullOrEmpty(comment) == false) {
                writer.WriteString("Comment", comment);
            }
            writer.WriteEndObject();
        }

        private void WriteFileReference(System.String Name , IFileReference reference , System.String comment)
        {
            writer.WriteStartObject();
            writer.WriteNumber("HeaderVersion", JSONRESOURCESCONSTANTS.HeaderVersion);
            writer.WriteString("ResourceName", Name);
            writer.WriteString("ResourceType", JSONRESResourceType.FileReference.ToString());
            writer.WriteStartArray("Value");
            writer.WriteStringValue(reference.FileName);
            writer.WriteStringValue(reference.SavingType.AssemblyQualifiedName);
            writer.WriteStringValue(reference.Encoding.ToString());
            writer.WriteEndArray();
            if (System.String.IsNullOrEmpty(comment) == false) {
                writer.WriteString("Comment", comment);
            }
            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public void AddResource(string name, string value) => WriteResource(name, value, JSONRESResourceType.String);

        /// <inheritdoc />
        public void AddResource(string name, object value) => WriteResource(name, value, JSONRESResourceType.Object);

        /// <inheritdoc />
        public void AddResource(string name, byte[] value) => WriteResource(name, value, JSONRESResourceType.ByteArray);

        /// <summary>
        /// Adds a file reference as a named resource to the list of resources to be written.
        /// </summary>
        /// <param name="name">The name of the resource that </param>
        /// <param name="reference">The file reference to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="reference"/> were null.</exception>
        /// <exception cref="ArgumentException">The desired resource name was invalid to be passed as a resource name.</exception>
        public void AddFileReference(System.String name, IFileReference reference)
        {
            EnsureNotClosed();
            if (reference is null) { throw new ArgumentNullException(nameof(reference)); }
            WriteFileReference(name, reference, null);
        }

        /// <summary>
        /// Writes (Flushes actually) all the current written resources to the target file or stream.
        /// </summary>
        public void Generate() { EnsureNotClosed(); writer.Flush(); }

        /// <inheritdoc />
        public void Close()
        {
            if (writer != null)
            {
                try {
                    writer?.WriteEndArray();
                    writer?.WriteEndObject();
                    writer?.Flush();
                } catch { }
                writer?.Dispose();
                writer = null;
            }
            if ((isstreamowner && mgmt == StreamMixedClassManagement.InitialisedWithStream) || mgmt == StreamMixedClassManagement.FileUsed)
            { try { targetstream?.Flush(); } catch (ObjectDisposedException) { } targetstream?.Close(); }
        }

        /// <summary>
        /// Disposes the <see cref="JSONResourcesWriter"/> class.
        /// </summary>
        public void Dispose()
        {
            Close();
            try { if ((isstreamowner && mgmt == StreamMixedClassManagement.InitialisedWithStream) || mgmt == StreamMixedClassManagement.FileUsed)
                { targetstream?.Dispose(); } } catch (System.ObjectDisposedException) { }
            if (targetstream != null) { targetstream = null; }
            exf?.Dispose();
            exf = null;
        }

        /// <inheritdoc/>
        public void RegisterTypeResolver(ITypeResolver resolver) => exf.RegisterTypeResolver(resolver);
    }

}
