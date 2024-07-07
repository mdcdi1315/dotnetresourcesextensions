
using System;

namespace DotNetResourcesExtensions
{
    using DotNetResourcesExtensions.Internal.CustomFormatter;
    using Internal;

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
            writer.WriteNumber("CurrentHeaderVersion", JSONRESOURCESCONSTANTS.HeaderVersion);
            writer.WriteEndObject();
            writer.WriteStartArray(JSONRESOURCESCONSTANTS.DataObjectName);
        }

        private void WriteResource(System.String Name, System.Object value, JSONRESResourceType rt)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }
            ParserHelpers.ValidateName(Name);

            switch (rt)
            {
                case JSONRESResourceType.String:
                    WriteStringResource(Name, value.ToString());
                    break;
                case JSONRESResourceType.Object:
                    WriteObjectResource(Name, value);
                    break;
                case JSONRESResourceType.ByteArray:
                    WriteByteArrayResource(Name, (System.Byte[])value, JSONRESResourceType.ByteArray);
                    break;
            }
        }

        private void WriteObjectResource(System.String Name, System.Object value)
        {
            System.Byte[] t;
            try
            {
               t = exf.GetBytesFromObject(value);
            } catch (Internal.CustomFormatter.Exceptions.ConverterNotFoundException e)
            {
                throw new FormatException("Could not serialize the given object. Error occured.", e);
            }
            writer.WriteStartObject();
            writer.WriteNumber("HeaderVersion", JSONRESOURCESCONSTANTS.HeaderVersion);
            writer.WriteString("ResourceName", Name);
            writer.WriteNumber("ResourceType", (System.UInt16)JSONRESResourceType.Object);
            writer.WriteString("DotnetType" , value.GetType().AssemblyQualifiedName);
            writer.WriteBase64String("Value[0]", t);
            writer.WriteEndObject();
        }

        private void WriteByteArrayResource(System.String Name, System.Byte[] value, JSONRESResourceType rt)
        {
            writer.WriteStartObject();
            writer.WriteNumber("HeaderVersion", JSONRESOURCESCONSTANTS.HeaderVersion);
            writer.WriteString("ResourceName", Name);
            writer.WriteNumber("ResourceType", (System.UInt16)rt);
            writer.WriteBase64String("Value[0]" , value);
            writer.WriteNumber("TotalLength", value.LongLength);
            writer.WriteEndObject();
        }

        private void WriteStringResource(System.String Name, System.String value)
        {
            writer.WriteStartObject();
            writer.WriteNumber("HeaderVersion", JSONRESOURCESCONSTANTS.HeaderVersion);
            writer.WriteString("ResourceName", Name);
            writer.WriteNumber("ResourceType", (System.UInt16)JSONRESResourceType.String);
            writer.WriteString("Value[0]", value);
            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public void AddResource(string name, string value) => WriteResource(name, value, JSONRESResourceType.String);

        /// <inheritdoc />
        public void AddResource(string name, object value) => WriteResource(name, value, JSONRESResourceType.Object);

        /// <inheritdoc />
        public void AddResource(string name, byte[] value) => WriteResource(name, value, JSONRESResourceType.ByteArray);

        /// <summary>
        /// Writes (Flushes actually) all the current written resources to the target file or stream.
        /// </summary>
        public void Generate() { writer.Flush(); }

        /// <inheritdoc />
        public void Close()
        {
            if (writer != null)
            {
                try
                {
                    writer?.WriteEndArray();
                    writer?.WriteEndObject();
                    writer?.Flush();
                } catch { }
                writer?.Dispose();
                writer = null;
            }
            if (isstreamowner && (mgmt == StreamMixedClassManagement.InitialisedWithStream || 
                mgmt == StreamMixedClassManagement.FileUsed))
            { try { targetstream?.Flush(); } catch (ObjectDisposedException) { } targetstream?.Close(); }
        }

        /// <summary>
        /// Disposes the <see cref="JSONResourcesWriter"/> class.
        /// </summary>
        public void Dispose()
        {
            Close();
            try { if (isstreamowner && mgmt == StreamMixedClassManagement.InitialisedWithStream)
                { targetstream?.Dispose(); } } catch (System.ObjectDisposedException) { }
            if (targetstream != null) { targetstream = null; }
            exf?.Dispose();
            exf = null;
        }

        /// <inheritdoc/>
        public void RegisterTypeResolver(ITypeResolver resolver)
        {
            exf.RegisterTypeResolver(resolver);
        }
    }

}
