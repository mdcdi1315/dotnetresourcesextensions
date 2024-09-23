
using System;

namespace DotNetResourcesExtensions
{
    using Internal;
    using Internal.CustomFormatter;
    using System.Xml.Linq;

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
            if (value is null) { throw new ArgumentNullException(nameof(value)); }
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
                    WriteByteArrayResource(Name, (System.Byte[])value);
                    break;
            }
        }

        private void WriteStringResource(System.String Name , System.String Value)
        {
            writer.WriteStartElement(XMLResourcesConstants.SingleResourceObjName);
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type" , XMLRESResourceType.String.ToString());
            writer.WriteElementString("Value" , Value);
            writer.WriteEndElement();
        }

        private void WriteByteArrayResource(System.String Name, System.Byte[] value)
        {
            writer.WriteStartElement(XMLResourcesConstants.SingleResourceObjName);
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", XMLRESResourceType.ByteArray.ToString());
            writer.WriteAttributeString("length", value.LongLength.ToString());
            WriteChunkedBase64(value);
            writer.WriteEndElement();
        }

        private void WriteChunkedBase64(System.Byte[] array)
        {
            System.String str = array.ToBase64();
            System.Int32 chunks = str.Length / XMLResourcesConstants.SingleBase64ChunkCharacterLength , 
                rem = str.Length % XMLResourcesConstants.SingleBase64ChunkCharacterLength;
            writer.WriteElementString("Chunks", (rem > 0 ? chunks+1 : chunks).ToString());
            writer.WriteElementString("Alignment", XMLResourcesConstants.SingleBase64ChunkCharacterLength.ToString());
            System.Int32 idx = 0;
            System.String nl = writer.Settings.NewLineChars;
            System.Text.StringBuilder sb = new(str.Length + (chunks * nl.Length));
            writer.WriteStartElement("Value");
            for (System.Int32 I = 1; I <= chunks; I++)
            {
                sb.Append(str.Substring(idx , XMLResourcesConstants.SingleBase64ChunkCharacterLength) + nl);
                idx += XMLResourcesConstants.SingleBase64ChunkCharacterLength;
                // Important so as all the data have been written successfully!
                writer.Flush();
            }
            nl = null;
            if (rem > 0) {
                sb.Append(str.Substring(idx));
            } else {
                sb.Remove(sb.Length - 1 , 1);
            }
            str = null;
            writer.WriteValue(sb.ToString());
            sb.Clear();
            sb = null;
            writer.WriteEndElement();
        }

        private void WriteObjectResource(System.String Name, System.Object value)
        {
            writer.WriteStartElement(XMLResourcesConstants.SingleResourceObjName);
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", XMLRESResourceType.Object.ToString());
            System.Byte[] data = formatter.GetBytesFromObject(value);
            writer.WriteAttributeString("length", data.LongLength.ToString());
            writer.WriteAttributeString("dotnettype", value.GetType().AssemblyQualifiedName);
            WriteChunkedBase64(data);
            writer.WriteEndElement();
        }

        /// <inheritdoc />
        public void AddResource(string name, string value) => WriteResource(name, value, XMLRESResourceType.String);

        /// <inheritdoc />
        public void AddResource(string name, object value) => WriteResource(name, value, XMLRESResourceType.Object);

        /// <inheritdoc />
        public void AddResource(string name, byte[] value) => WriteResource(name, value, XMLRESResourceType.ByteArray);

        /// <summary>
        /// Adds a file reference that when it will be read , it's data will be returned.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <param name="reference">The reference to add.</param>
        public void AddFileReference(System.String name, IFileReference reference)
        {
            if (reference is null) { throw new ArgumentNullException(nameof(reference)); }
            ParserHelpers.ValidateName(name);
            writer.WriteStartElement(XMLResourcesConstants.SingleResourceObjName);
            writer.WriteAttributeString("name", name);
            writer.WriteAttributeString("type", XMLRESResourceType.FileReference.ToString());
            writer.WriteElementString("Value", reference.ToSerializedString());
            writer.WriteEndElement();
        }

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
