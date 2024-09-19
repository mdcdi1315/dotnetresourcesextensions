
using System;
using System.Linq;

namespace DotNetResourcesExtensions
{
    using Internal;
    using System.Collections;
    using Internal.CustomFormatter;

    /// <summary>
    /// The XML Resources Enumerator is used only with the <see cref="XMLResourcesReader"/> class. <br />
    /// You cannot create an instance of this class. Instead , to get an instance you must use the 
    /// <see cref="XMLResourcesReader.GetEnumerator()"/> method and cast to this class.
    /// </summary>
    public sealed class XMLResourcesEnumerator : IDictionaryEnumerator
    {
        private DictionaryEntry current;
        private XMLResourcesReader reader;
        private System.Xml.Linq.XElement[] resources;
        private System.Int64 elementcount;
        private System.Int64 currentindex;
        private System.Boolean readresource;

        private sealed class XMLTypedFileReference : IFileReference
        {
            private readonly System.String name;
            private readonly System.Type type;
            private readonly FileReferenceEncoding encoding;

            public XMLTypedFileReference(string name, Type type, FileReferenceEncoding encoding)
            {
                this.name = name;
                this.type = type;
                this.encoding = encoding;
            }

            public static XMLTypedFileReference ParseFromSerializedString(System.String dat)
            {
                System.String[] strings = dat.Split(';');
                return new(ParserHelpers.RemoveQuotes(strings[0]),
                    System.Type.GetType(strings[1], true, true),
                    ParserHelpers.ParseEnumerationConstant<FileReferenceEncoding>(strings[2]));
            }

            public string FileName => name;
            public FileReferenceEncoding Encoding => encoding;
            public System.Type SavingType => type;
        }

        private XMLResourcesEnumerator() { readresource = false; current = default; resources = null; elementcount = 0; currentindex = -1; }

        internal XMLResourcesEnumerator(XMLResourcesReader rd) : this()
        {
            reader = rd;
            if (rd.versionread == 2)
            {
                // V2 has just all the resource nodes named the same way. An attribute identifies the resource name anymore.
                // This design allows more characters to be enclosed in a resource name!
                resources = rd.baseresnode.Elements(System.Xml.Linq.XName.Get(XMLResourcesConstants.SingleResourceObjName)).ToArray();
            } else if (rd.versionread == 1) 
            {
                // V1 is just supported by doing this if statement
                resources = rd.baseresnode.Elements().ToArray();
            }
            elementcount = resources.LongLength;
        }

        private System.Xml.Linq.XElement GetChildElement(System.String Name) =>
            resources[currentindex].Element(System.Xml.Linq.XName.Get(Name));

        private System.Xml.Linq.XAttribute GetElementAttribute(System.String Name)
            => resources[currentindex].Attribute(System.Xml.Linq.XName.Get(Name));

        private DictionaryEntry GetResource()
        {
            if (currentindex <= -1) { throw new InvalidOperationException("The enumeration has not started yet."); }
            if (currentindex >= elementcount) { throw new InvalidOperationException("The enumeration has been finished. If you want to re-enumerate , use Reset() method."); }
            if (readresource) { return current; }
            DictionaryEntry result = default;
            // The older code is anymore useless and the header version is deprecated.
            // So , still though remains the same and the V1 resource will be normally retrieved.
            // For V2 , a different method takes action.
            switch (reader.versionread)
            {
                case 1:
                    result = GetV1Resource();
                    break;
                case 2:
                    result = GetV2Resource();
                    break;
                // We do not need the default case anymore
            }
            readresource = true;
            current = result;
            return result;
        }

        private DictionaryEntry GetV2Resource()
        {
            System.Byte[] GetBytes()
            {
                System.Int32 Chunks = (System.Int32)GetChildElement("Chunks") , 
                    Alignment = (System.Int32)GetChildElement("Alignment");
                System.String dat = GetChildElement("Value").Value;
                System.Text.StringBuilder sb = new(dat.Length , dat.Length);
                System.Int32 rc = 0;
                foreach (System.String dt in ParserHelpers.GetStringSplittedData(dat , '\n')) {
                    if (dt.Length == Alignment) { rc++; }
                    sb.Append(dt);
                }
                dat = null;
                if (rc < Chunks - 1) // Chunks - 1 to avoid the rem issue
                {
                    throw new XMLFormatException("The byte array contents could not be read because not all expected chunks were read." , ParserErrorType.Deserialization);
                }
                System.Byte[] bt = sb.ToString().FromBase64();
                sb.Clear();
                sb = null;
                rc = (System.Int32)GetElementAttribute("length");
                if (bt.Length != rc) {
                    throw new XMLFormatException($"Corrupted byte array detected. Expected to read {rc} bytes while read {bt.Length} bytes." , ParserErrorType.Deserialization);
                }
                return bt;
            }
            DictionaryEntry result = new();
            result.Key = GetElementAttribute("name").Value;
            XMLRESResourceType tpp = (XMLRESResourceType)(System.Int32)GetElementAttribute("type");
            if (tpp > reader.supportedformatsmask) {
                // The supported formats mask is still true!
                throw new XMLFormatException($"This resource object type is not supported in version 2: {tpp}", ParserErrorType.Deserialization);
            }
            switch (tpp)
            {
                case XMLRESResourceType.String:
                    result.Value = GetChildElement("Value").Value;
                    break;
                case XMLRESResourceType.ByteArray:
                    result.Value = GetBytes();
                    break;
                case XMLRESResourceType.Object:
                    System.Type dnttype = System.Type.GetType(GetElementAttribute("dotnettype").Value);
                    try {
                        result.Value = reader.exf.GetObjectFromBytes(GetBytes(), dnttype);
                    } catch (Internal.CustomFormatter.Exceptions.ConverterNotFoundException e) {
                        throw new XMLFormatException("A resource object deserialization error occured.", e.Message, ParserErrorType.Deserialization);
                    }
                    break;
                case XMLRESResourceType.FileReference:
                    XMLTypedFileReference tfr = XMLTypedFileReference.ParseFromSerializedString(GetChildElement("Value").Value);
                    System.IO.FileStream FS = null;
                    try {
                        FS = tfr.OpenStreamToFile();
                        System.Byte[] data = ParserHelpers.ReadBuffered(FS , FS.Length);
                        result.Value = tfr.SavingType.FullName switch
                        {
                            "System.String" => tfr.AsEncoding().GetString(data),
                            "System.Byte[]" => data,
                            _ => reader.exf.GetObjectFromBytes(data, tfr.SavingType),
                        };
                        data = null;
                    } finally {
                        FS?.Dispose();
                        FS = null;
                    }
                    tfr = null;
                    break;
            }
            return result;
        }

        private DictionaryEntry GetV1Resource()
        {
            DictionaryEntry result = new();
            result.Key = resources[currentindex].Name.LocalName;
            XMLRESResourceType tpp = (XMLRESResourceType)(System.Int32)GetChildElement("ResourceType");
            if (tpp > reader.supportedformatsmask) {
                throw new XMLFormatException($"This resource object type is not supported in version 1: {tpp}" , ParserErrorType.Deserialization);
            }
            switch (tpp)
            {
                case XMLRESResourceType.String:
                    result.Value = GetChildElement("Value_0").Value;
                    break;
                case XMLRESResourceType.ByteArray:
                    result.Value = GetChildElement("Value_0").Value.FromBase64();
                    break;
                case XMLRESResourceType.Object:
                    // We will not use the DataContractSerializer. Instead , we will depend on ExtensibleFormatter.
                    // Why? To avoid complexity and having XML and code transparency.
                    System.Type dotnettype = System.Type.GetType(GetChildElement("DotnetType").Value);
                    try {
                        result.Value = reader.exf.GetObjectFromBytes(GetChildElement("Value_0").Value.FromBase64(), dotnettype);
                    } catch (Internal.CustomFormatter.Exceptions.ConverterNotFoundException e) {
                        throw new XMLFormatException("A resource object deserialization error occured.", e.Message, ParserErrorType.Deserialization);
                    }
                    break;
            }
            return result;
        }        

        /// <inheritdoc />
        public System.Boolean MoveNext()
        {
            readresource = false;
            currentindex++;
            return currentindex < elementcount;
        }

        /// <inheritdoc />
        public System.Object Current => Entry;

        /// <inheritdoc />
        public void Reset() => currentindex = -1;

        /// <inheritdoc />
        public DictionaryEntry Entry => GetResource();

        /// <inheritdoc />
        public System.Object Key => Entry.Key;

        /// <inheritdoc />
        public System.Object Value => Entry.Value;
    }

    /// <summary>
    /// The <see cref="XMLResourcesReader"/> class reads the custom resources format of <see cref="XMLResourcesWriter"/>. <br />
    /// You can use it so as to read such data. <br />
    /// Can also read the V1 older format. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class XMLResourcesReader : IDotNetResourcesExtensionsReader
    {
        private System.IO.Stream targetstream;
        private System.Xml.XmlReader reader;
        private System.Boolean isstreamowner;
        private StreamMixedClassManagement mgmt;
        internal XMLRESResourceType supportedformatsmask;
        // supportedheaderversion is deprecated but is used by V1 , so still remains here.
        internal System.UInt16 supportedheaderversion , versionread;
        internal ExtensibleFormatter exf;
        internal System.Xml.Linq.XElement baseresnode;
        private System.Xml.Linq.XDocument xdoc;
        
        private XMLResourcesReader() 
        {
            exf = new();
            xdoc = null; 
            reader = null; 
            isstreamowner = false; 
            mgmt = StreamMixedClassManagement.None; 
            supportedformatsmask = XMLRESResourceType.String;
            versionread = supportedheaderversion = 0;
        }

        /// <summary>
        /// Creates a new instance of <see cref="XMLResourcesReader"/> from the specified data stream.
        /// </summary>
        /// <param name="stream">The data stream to read resources from.</param>
        public XMLResourcesReader(System.IO.Stream stream) : this()
        {
            targetstream = stream;
            mgmt = StreamMixedClassManagement.InitialisedWithStream;
            reader = System.Xml.XmlReader.Create(targetstream , XMLResourcesConstants.GlobalReaderSettings);
            xdoc = System.Xml.Linq.XDocument.Load(reader);
            ValidateHeader();
        }

        /// <summary>
        /// Creates a new instance of <see cref="XMLResourcesReader"/> from the specified file.
        /// </summary>
        /// <param name="path">The path of the file to read. The file must have been written using the <see cref="XMLResourcesWriter"/> class.</param>
        public XMLResourcesReader(System.String path) : this()
        {
            targetstream = new System.IO.FileStream(path , System.IO.FileMode.Open);
            mgmt = StreamMixedClassManagement.FileUsed;
            isstreamowner = true;
            reader = System.Xml.XmlReader.Create(targetstream, XMLResourcesConstants.GlobalReaderSettings);
            xdoc = System.Xml.Linq.XDocument.Load(reader);
            ValidateHeader();
        }

        private void ValidateHeader()
        {
            if (xdoc.Root.Name != XMLResourcesConstants.GlobalNameSpaceName) 
            { throw new XMLFormatException(XMLResourcesConstants.DefaultExceptionMsg , 
                "The XML Resources header was not found." , ParserErrorType.Header); }
            System.Boolean found = false;
            foreach (var di in xdoc.Root.Nodes())
            {
                if (di is System.Xml.Linq.XElement d && d.Name.LocalName == XMLResourcesConstants.XMLHeader)
                {
                    foreach (System.Xml.Linq.XNode node in d.Nodes())
                    {
                        if (node.GetType() == typeof(System.Xml.Linq.XElement))
                        {
                            System.Xml.Linq.XElement el = (System.Xml.Linq.XElement)node;
                            switch (el.Name.LocalName)
                            {
                                case "Version":
                                    if ((versionread = ((System.UInt32)el).ToUInt16()) > XMLResourcesConstants.Version)
                                    { throw new XMLFormatException(XMLResourcesConstants.DefaultExceptionMsg , 
                                        $"You attempted to load a version not supported by this class. Please use a reader that supports version {el.Value} or later."
                                        , ParserErrorType.Header); }
                                    break;
                                case "Magic":
                                    if (el.Value != XMLResourcesConstants.Magic)
                                    {
                                        throw new XMLFormatException(XMLResourcesConstants.DefaultExceptionMsg,
                                        "The magic value of the header is either unsupported or represents data corruption."
                                        , ParserErrorType.Header);
                                    }
                                    break;
                                case "SupportedFormatsMask":
                                    supportedformatsmask = (XMLRESResourceType)(System.Int32)el;
                                    break;
                                case "CurrentHeaderVersion":
                                    // Although that this field is still read , it does not effectively be used in version >= 2.
                                    supportedheaderversion = ((System.UInt32)el).ToUInt16();
                                    break;
                            }
                        }
                    }
                    baseresnode = d.Parent.Element(System.Xml.Linq.XName.Get(XMLResourcesConstants.DataObjectName));
                    found = true;
                    break;
                }
            }
            if (found == false) {
                throw new XMLFormatException(XMLResourcesConstants.DefaultExceptionMsg,
                "The XML Resources header was not found.", ParserErrorType.Header);
            }
        }

        /// <summary>
        /// Gets or sets a value whether this class controls the lifetime of the underlying stream.
        /// </summary>
        public bool IsStreamOwner { get => isstreamowner; set => isstreamowner = value; }

        /// <inheritdoc />
        public void Close()
        {
            xdoc = null;
            baseresnode = null;
            reader?.Close();
            if ((isstreamowner && mgmt == StreamMixedClassManagement.InitialisedWithStream) || 
                mgmt == StreamMixedClassManagement.FileUsed)
            {
                targetstream?.Close();
            }
        }

        /// <summary>
        /// Disposes the <see cref="XMLResourcesReader"/> class.
        /// </summary>
        public void Dispose()
        {
            Close();
            targetstream?.Dispose();
            targetstream = null;
            reader?.Dispose();
            reader = null;
        }

        /// <inheritdoc />
        public IDictionaryEnumerator GetEnumerator() => new XMLResourcesEnumerator(this);
        
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public void RegisterTypeResolver(ITypeResolver resolver)
        {
            exf.RegisterTypeResolver(resolver);
        }
    }
}
