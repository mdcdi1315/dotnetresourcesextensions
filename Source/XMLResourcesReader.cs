﻿
using System;
using System.Linq;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
    using Internal;
    using System.Collections;

    /// <summary>
    /// The XML Resources Enumerator is used only with the <see cref="XMLResourcesReader"/> class. <br />
    /// You cannot create an instance of this class. Instead , to get an instance you must use the 
    /// <see cref="XMLResourcesReader.GetEnumerator()"/> method.
    /// </summary>
    public sealed class XMLResourcesEnumerator : IDictionaryEnumerator
    {
        private DictionaryEntry current;
        private System.Xml.Linq.XElement[] resources;
        private System.Int64 elementcount;
        private System.Int64 currentindex;
        private XMLRESResourceType supportedformatsmask;
        private System.UInt16 supportedheaderversion;
        private System.Boolean readresource;

        private XMLResourcesEnumerator() { readresource = false; current = default; resources = null; elementcount = 0; currentindex = -1; }

        internal XMLResourcesEnumerator(System.Xml.Linq.XElement data , XMLRESResourceType mask , System.UInt16 maxver) : this()
        {
            resources = data.Elements().ToArray();
            supportedformatsmask = mask;
            supportedheaderversion = maxver;
            elementcount = resources.LongLength;
        }

        private System.Xml.Linq.XElement GetChildElement(System.String Name) =>
            resources[currentindex].Element(System.Xml.Linq.XName.Get(Name));

        private DictionaryEntry GetResource()
        {
            DictionaryEntry result;
            // To determine what to decode , we must get first the header version.
            System.UInt16 ver = (System.UInt16)(uint)GetChildElement("HeaderVersion");
            // Throw exception if the read header has bigger version than the allowed
            if (ver > supportedheaderversion)
            {
                throw new FormatException(
                $"This header cannot be read with this version of the class. Please use a reader that supports header version {ver} or higher.");
            }
            switch (ver)
            {
                case 1:
                    result = GetV1Resource();
                    break;
                default:
                    throw new XMLFormatException($"The current version {ver} cannot be read. Internal error occured." , ParserErrorType.Versioning);
            }
            return result;
        }

        private DictionaryEntry GetV1Resource()
        {
            DictionaryEntry result = new();
            result.Key = resources[currentindex].Name.LocalName;
            XMLRESResourceType tpp = (XMLRESResourceType)(System.Int32)GetChildElement("ResourceType");
            if (tpp > supportedformatsmask)
            {
                throw new XMLFormatException($"This resource object type is not supported in version 1: {tpp}" , ParserErrorType.Deserialization);
            }
            switch (tpp)
            {
                case XMLRESResourceType.String:
                    result.Value = (System.String)GetChildElement("Value_0");
                    break;
                case XMLRESResourceType.ByteArray:
                    result.Value = System.Convert.FromBase64String((System.String)GetChildElement("Value_0"));
                    break;
                case XMLRESResourceType.Object:
                    System.Xml.Linq.XElement obj = null;
                    System.Type dotnettype = null;
                    // Using the below temp loop we find the .NET type to deserialise , and 
                    // the remaining element is our element to be deserialised.
                    foreach (var si in resources[currentindex].Elements())
                    {
                        switch (si.Name.LocalName)
                        {
                            case "HeaderVersion":
                            case "ResourceType":
                                break;
                            case "DotnetType":
                                dotnettype = System.Type.GetType(si.Value);
                                break;
                            default:
                                obj = si;
                                break;
                        }
                    }
                    if (obj == null) { throw new XMLFormatException("The resource object did not had a declared deserializer.", ParserErrorType.Deserialization); }
                    System.Runtime.Serialization.DataContractSerializer dcs = new(dotnettype);
                    try
                    {
                        result.Value = dcs.ReadObject(obj.CreateReader(), true);
                    } catch (System.Runtime.Serialization.SerializationException e)
                    {
                        throw new XMLFormatException("A resource object deserialization error occured.", e.Message, ParserErrorType.Deserialization);
                    }
                    obj = null;
                    dcs = null;
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
        public DictionaryEntry Entry {
            get
            {
                if (currentindex == -1) { throw new InvalidOperationException(); }
                if (readresource) { return current; }
                readresource = true;
                current = GetResource();
                return current;
            }
        }

        /// <inheritdoc />
        public System.Object Key => Entry.Key;

        /// <inheritdoc />
        public System.Object Value => Entry.Value;

    }

    /// <summary>
    /// The <see cref="XMLResourcesReader"/> class reads the custom resources format of <see cref="XMLResourcesWriter"/>. <br />
    /// You can use it so as to read such data. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class XMLResourcesReader : System.Resources.IResourceReader , IStreamOwnerBase
    {
        private System.IO.Stream targetstream;
        private System.Xml.XmlReader reader;
        private System.Boolean isstreamowner;
        private StreamMixedClassManagement mgmt;
        private XMLRESResourceType supportedformatsmask;
        private System.UInt16 supportedheaderversion;
        private System.Xml.Linq.XDocument xdoc;
        private System.Xml.Linq.XElement baseresnode;
        
        private XMLResourcesReader() 
        { 
            xdoc = null; 
            reader = null; 
            isstreamowner = false; 
            mgmt = StreamMixedClassManagement.None; 
            supportedformatsmask = XMLRESResourceType.String;
            supportedheaderversion = 0;
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
            mgmt = StreamMixedClassManagement.InitialisedWithStream;
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
                                    if ((System.UInt32)el > XMLResourcesConstants.Version)
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
                                    supportedheaderversion = (System.UInt16)(System.UInt32)el;
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
            if (isstreamowner && mgmt == StreamMixedClassManagement.InitialisedWithStream || 
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
        public IDictionaryEnumerator GetEnumerator() => new XMLResourcesEnumerator(baseresnode, supportedformatsmask, supportedheaderversion);
        
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
