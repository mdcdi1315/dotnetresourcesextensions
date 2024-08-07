
using System;

namespace DotNetResourcesExtensions
{
    using Internal;
    using Internal.CustomFormatter;

    /// <summary>
    /// The <see cref="XMLResourcesWriter"/> writes a custom resources format based on structuralized XML. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class XMLResourcesWriter : IDotNetResourcesExtensionsWriter
    {
        private System.IO.Stream targetstream;
        private ExtensibleFormatter formatter;
        private System.Xml.XmlWriter writer;
        private System.Boolean isstreamowner;
        private StreamMixedClassManagement mgmt;

        private XMLResourcesWriter() { formatter = new(); targetstream = null; writer = null; isstreamowner = false; mgmt = StreamMixedClassManagement.None; }

        /// <summary>
        /// Gets or sets a value whether this class controls the lifetime of the underlying stream.
        /// </summary>
        public bool IsStreamOwner { get => isstreamowner; set => isstreamowner = value; }

        /// <summary>
        /// Create a new instance of <see cref="XMLResourcesWriter"/> with the specified stream as the resulting output.
        /// </summary>
        /// <param name="stream">The resulting output stream.</param>
        public XMLResourcesWriter(System.IO.Stream stream) : this() 
        {
            targetstream = stream;
            mgmt = StreamMixedClassManagement.InitialisedWithStream;
            writer = System.Xml.XmlWriter.Create(targetstream , XMLResourcesConstants.GlobalWriterSettings);
            isstreamowner = false;
            CreateHeader();
        }

        /// <summary>
        /// Create a new instance of <see cref="XMLResourcesWriter"/> with the specified file path as the resulting output.
        /// </summary>
        /// <param name="file">The file path to save the generated resources.</param>
        public XMLResourcesWriter(System.String file) : this()
        {
            targetstream = new System.IO.FileStream(file , System.IO.FileMode.Create);
            mgmt = StreamMixedClassManagement.FileUsed;
            writer = System.Xml.XmlWriter.Create(targetstream, XMLResourcesConstants.GlobalWriterSettings);
            isstreamowner = true;
            CreateHeader();
        }

        private void CreateHeader()
        {
            writer.WriteStartDocument(true);
            writer.WriteStartElement(XMLResourcesConstants.GlobalNameSpaceName);
            writer.WriteStartElement(XMLResourcesConstants.XMLHeader);
            writer.WriteElementString("Version" , XMLResourcesConstants.Version.ToString());
            writer.WriteElementString("Magic" , XMLResourcesConstants.Magic);
            writer.WriteElementString("SupportedFormatsMask", ((System.Byte)XMLResourcesConstants.CurrentMask).ToString());
            writer.WriteElementString("CurrentHeaderVersion", XMLResourcesConstants.HeaderVersion.ToString());
            writer.WriteEndElement();
            writer.WriteStartElement(XMLResourcesConstants.DataObjectName);
        }

        private void WriteResource(System.String Name, System.Object value, XMLRESResourceType rt)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }
            ParserHelpers.ValidateName(Name);

            switch (rt)
            {
                case XMLRESResourceType.String:
                    WriteStringResource(Name, value.ToString());
                    break;
                case XMLRESResourceType.Object:
                    WriteObjectResource(Name, value);
                    break;
                case XMLRESResourceType.ByteArray:
                    WriteByteArrayResource(Name, (System.Byte[])value, XMLRESResourceType.ByteArray);
                    break;
            }
        }

        private void WriteStringResource(System.String Name , System.String Value)
        {
            writer.WriteStartElement(Name);
            writer.WriteElementString("HeaderVersion", XMLResourcesConstants.HeaderVersion.ToString());
            writer.WriteElementString("ResourceType" , ((System.Byte)XMLRESResourceType.String).ToString());
            writer.WriteElementString("Value_0" , Value);
            writer.WriteEndElement();
        }

        private void WriteByteArrayResource(System.String Name, System.Byte[] value, XMLRESResourceType rt)
        {
            writer.WriteStartElement(Name);
            writer.WriteElementString("HeaderVersion", XMLResourcesConstants.HeaderVersion.ToString());
            writer.WriteElementString("ResourceType", ((System.Byte)rt).ToString());
            writer.WriteStartElement("Value_0");
            writer.WriteBase64(value , 0 , value.Length);
            writer.WriteEndElement();
            writer.WriteElementString("TotalLength", value.LongLength.ToString());
            writer.WriteEndElement();
        }

        private void WriteObjectResource(System.String Name, System.Object value)
        {
            writer.WriteStartElement(Name);
            writer.WriteElementString("HeaderVersion", XMLResourcesConstants.HeaderVersion.ToString());
            writer.WriteElementString("ResourceType", ((System.Byte)XMLRESResourceType.Object).ToString());
            writer.WriteElementString("DotnetType", value.GetType().AssemblyQualifiedName);
            writer.WriteStartElement("Value_0");
            System.Byte[] data = formatter.GetBytesFromObject(value);
            writer.WriteBase64(data, 0, data.Length);
            writer.WriteEndElement();
            writer.WriteElementString("TotalLength", data.LongLength.ToString());
            writer.WriteEndElement();
        }

        /// <inheritdoc />
        public void AddResource(string name, string value) => WriteResource(name, value, XMLRESResourceType.String);

        /// <inheritdoc />
        public void AddResource(string name, object value) => WriteResource(name, value, XMLRESResourceType.Object);

        /// <inheritdoc />
        public void AddResource(string name, byte[] value) => WriteResource(name, value, XMLRESResourceType.ByteArray);

        /// <inheritdoc />
        public void Close()
        {
            try
            {
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
            } catch (System.InvalidOperationException) { }
            if (isstreamowner && (mgmt == StreamMixedClassManagement.FileUsed || 
                mgmt == StreamMixedClassManagement.InitialisedWithStream))
            {
                try { targetstream?.Flush(); } catch (ObjectDisposedException) { }
                targetstream?.Close();
            }
        }

        /// <summary>
        /// Disposes the <see cref="XMLResourcesWriter"/> class.
        /// </summary>
        public void Dispose()
        {
            Close();
            try { writer?.Dispose(); } catch (ObjectDisposedException) { }
            try
            {
                if (isstreamowner && (mgmt == StreamMixedClassManagement.FileUsed ||
                mgmt == StreamMixedClassManagement.InitialisedWithStream))
                {
                    targetstream?.Dispose(); 
                    targetstream = null;
                }
            } catch (ObjectDisposedException) { }
            writer = null;
            formatter?.Dispose();
            formatter = null;
        }

        /// <summary>
        /// Writes (Flushes actually) all the current written resources to the target file or stream.
        /// </summary>
        public void Generate() { writer.Flush(); }

        /// <inheritdoc />
        public void RegisterTypeResolver(ITypeResolver resolver) => formatter.RegisterTypeResolver(resolver);
    }

}
