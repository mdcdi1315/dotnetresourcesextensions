// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Resources;
using System.Xml;
using System.Diagnostics.CodeAnalysis;
using DotNetResourcesExtensions.Properties;

namespace DotNetResourcesExtensions.Internal.ResX;

/// <summary>
///  ResX resource writer. See the text in "ResourceSchema" for more information. <br />
///  Note: It writes custom ResX data and thus , the default ResX readers cannot read the results taken from this
///  ResX Writer. You can only read the result data using the <see cref="ResXResourceReader"/> class provided.
/// </summary>
public class ResXResourceWriter : IResourceWriter
{
    internal const string TypeStr = "type";
    internal const string NameStr = "name";
    internal const string DataStr = "data";
    internal const string MetadataStr = "metadata";
    internal const string MimeTypeStr = "mimetype";
    internal const string ValueStr = "value";
    internal const string ResHeaderStr = "resheader";
    internal const string VersionStr = "version";
    internal const string ResMimeTypeStr = "resmimetype";
    internal const string ReaderStr = "reader";
    internal const string WriterStr = "writer";
    internal const string CommentStr = "comment";
    internal const string AssemblyStr = "assembly";
    internal const string AliasStr = "alias";

    private Dictionary<string, string?>? _cachedAliases;

    private static readonly TraceSwitch s_resValueProviderSwitch = new("ResX", "Debug the resource value provider");

#pragma warning disable IDE1006 // Naming Styles (Shipped public API)
    /// <summary>
    /// Gets the binary serialized object mime type.
    /// </summary>
    public static readonly string BinSerializedObjectMimeType = "application/x-microsoft.net.object.binary.base64";
    /// <summary>
    /// Gets the sopa formatted object mime type.
    /// </summary>
    public static readonly string SoapSerializedObjectMimeType = "application/x-microsoft.net.object.soap.base64";
    /// <summary>
    /// Gets the default serialized mime type. It's current value is <see cref="BinSerializedObjectMimeType"/>.
    /// </summary>
    public static readonly string DefaultSerializedObjectMimeType = BinSerializedObjectMimeType;
    // My addition so as to split the normal formatted resources from my custom binary formatter.
    /// <summary>
    /// The custom formatter mime type used for this writer.
    /// </summary>
    public const System.String Mdcdi1315SerializedObjectMimeType = "application/x-mdcdi1315-dotnetresourcesextensions.binary.base64";
    /// <summary>
    /// The plain byte array mime type.
    /// </summary>
    public static readonly string ByteArraySerializedObjectMimeType = "application/x-microsoft.net.object.bytearray.base64";
    /// <summary>
    /// The current ResX mime type. It is a custom identifier.
    /// </summary>
    public static readonly string ResMimeType = "text/mdcdi1315-resx";
    /// <summary>
    /// The current version that this writer can write.
    /// </summary>
    public static readonly string Version = "1.1-initial";

    /// <summary>
    /// Specifies the schema to use in writing the XML file. This field is read-only.
    /// </summary>
    /// <remarks>The schema provides details on how to encode the resources based on the type of resource being written.</remarks>
    public static readonly string ResourceSchema = """
            <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
                <xsd:import namespace="http://www.w3.org/XML/1998/namespace"/>
                <xsd:element name="root" msdata:IsDataSet="true">
                    <xsd:complexType>
                        <xsd:choice maxOccurs="unbounded">
                            <xsd:element name="metadata">
                                <xsd:complexType>
                                    <xsd:sequence>
                                        <xsd:element name="value" type="xsd:string" minOccurs="0"/>
                                    </xsd:sequence>
                                    <xsd:attribute name="name" use="required" type="xsd:string"/>
                                    <xsd:attribute name="type" type="xsd:string"/>
                                    <xsd:attribute name="mimetype" type="xsd:string"/>
                                    <xsd:attribute ref="xml:space"/>
                                </xsd:complexType>
                            </xsd:element>
                            <xsd:element name="assembly">
                                <xsd:complexType>
                                    <xsd:attribute name="alias" type="xsd:string"/>
                                    <xsd:attribute name="name" type="xsd:string"/>
                                </xsd:complexType>
                            </xsd:element>
                            <xsd:element name="data">
                                <xsd:complexType>
                                    <xsd:sequence>
                                        <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                                        <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
                                    </xsd:sequence>
                                    <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
                                    <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
                                    <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
                                    <xsd:attribute ref="xml:space"/>
                                </xsd:complexType>
                            </xsd:element>
                            <xsd:element name="resheader">
                                <xsd:complexType>
                                    <xsd:sequence>
                                        <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                                    </xsd:sequence>
                                    <xsd:attribute name="name" type="xsd:string" use="required" />
                                </xsd:complexType>
                            </xsd:element>
                        </xsd:choice>
                    </xsd:complexType>
                </xsd:element>
            </xsd:schema>
            """;
#pragma warning restore IDE1006 // Naming Styles

    private readonly string? _fileName;
    private Stream? _stream;
    private TextWriter? _textWriter;
    private XmlTextWriter? _xmlTextWriter;

    private bool _hasBeenSaved;

    private readonly Func<Type?, string>? _typeNameConverter; // no public property to be consistent with ResXDataNode class.

    /// <summary>
    ///  A path that, if prepended to the relative file path specified in a <see cref="ResXFileRef"/> object,
    ///  yields an absolute path to an XML resource file.
    /// </summary>
    public string? BasePath { get; set; }

    /// <summary>
    ///  Creates a new instance of <see cref="ResXResourceWriter"/> class that will write to the specified file.
    /// </summary>
    public ResXResourceWriter(string fileName) => _fileName = fileName;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResXResourceWriter"/> class that writes the resources to a specified file and sets a delegate that enables resource assemblies to be written that target versions of the .NET Framework before .NET Framework 4 by using qualified assembly names.
    /// </summary>
    /// <param name="fileName">The file to send output to.</param>
    /// <param name="typeNameConverter">The delegate that is used to target earlier versions of the .NET Framework.</param>
    public ResXResourceWriter(string fileName, Func<Type?, string> typeNameConverter)
    {
        _fileName = fileName;
        _typeNameConverter = typeNameConverter;
    }

    /// <summary>
    ///  Creates a new ResXResourceWriter that will write to the specified stream.
    /// </summary>
    public ResXResourceWriter(Stream stream) => _stream = stream;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResXResourceWriter"/> class that writes the resources to a specified stream object and sets a converter delegate. This delegate enables resource assemblies to be written that target versions of the .NET Framework before .NET Framework 4 by using qualified assembly names.
    /// </summary>
    /// <param name="stream">The stream to send the output to.</param>
    /// <param name="typeNameConverter">The delegate that is used to target earlier versions of the .NET Framework.</param>
    public ResXResourceWriter(Stream stream, Func<Type?, string> typeNameConverter)
    {
        _stream = stream;
        _typeNameConverter = typeNameConverter;
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="ResXResourceWriter"/> class that writes to the specified
    ///  <see cref="TextWriter"/> object.
    /// </summary>
    public ResXResourceWriter(TextWriter textWriter) => _textWriter = textWriter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResXResourceWriter"/> class that writes the resources to a specified <see cref="TextWriter"/> object and sets a delegate that enables resource assemblies to be written that target versions of the .NET Framework before .NET Framework 4 by using qualified assembly names.
    /// </summary>
    /// <param name="textWriter">The object to send output to.</param>
    /// <param name="typeNameConverter">The delegate that is used to target earlier versions of the .NET Framework.</param>
    public ResXResourceWriter(TextWriter textWriter, Func<Type?, string> typeNameConverter)
    {
        _textWriter = textWriter;
        _typeNameConverter = typeNameConverter;
    }

    /// <summary>
    /// This member overrides the <see cref="System.Object.Finalize()"/> method.
    /// </summary>
    ~ResXResourceWriter() { Dispose(disposing: false); }

    [MemberNotNull(nameof(_xmlTextWriter))]
    private void InitializeWriter()
    {
        if (_xmlTextWriter is not null)
        {
            return;
        }

        bool writeHeaderRequired = false;

        if (_textWriter is not null)
        {
            _textWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            writeHeaderRequired = true;

            _xmlTextWriter = new XmlTextWriter(_textWriter);
        }
        else if (_stream is not null)
        {
            _xmlTextWriter = new XmlTextWriter(_stream, Encoding.UTF8);
        }
        else
        {
            Debug.Assert(_fileName is not null, "Nothing to output to");
            _xmlTextWriter = new XmlTextWriter(_fileName, Encoding.UTF8);
        }

        _xmlTextWriter.Formatting = Formatting.Indented;
        _xmlTextWriter.Indentation = 2;

        if (!writeHeaderRequired)
        {
            _xmlTextWriter.WriteStartDocument(); // writes <?xml version="1.0" encoding="utf-8"?>
        }

        _xmlTextWriter.WriteStartElement("root");
        XmlTextReader reader = new XmlTextReader(new StringReader(ResourceSchema))
        {
            WhitespaceHandling = WhitespaceHandling.None
        };
        _xmlTextWriter.WriteNode(reader, true);

        _xmlTextWriter.WriteStartElement(ResHeaderStr);
        {
            _xmlTextWriter.WriteAttributeString(NameStr, ResMimeTypeStr);
            _xmlTextWriter.WriteStartElement(ValueStr);
            {
                _xmlTextWriter.WriteString(ResMimeType);
            }

            _xmlTextWriter.WriteEndElement();
        }

        _xmlTextWriter.WriteEndElement();

        _xmlTextWriter.WriteStartElement(ResHeaderStr);
        {
            _xmlTextWriter.WriteAttributeString(NameStr, VersionStr);
            _xmlTextWriter.WriteStartElement(ValueStr);
            {
                _xmlTextWriter.WriteString(Version);
            }

            _xmlTextWriter.WriteEndElement();
        }

        _xmlTextWriter.WriteEndElement();

        _xmlTextWriter.WriteStartElement(ResHeaderStr);
        {
            _xmlTextWriter.WriteAttributeString(NameStr, ReaderStr);
            _xmlTextWriter.WriteStartElement(ValueStr);
            {
                _xmlTextWriter.WriteString(ParserHelpers.GetAssemblyQualifiedName(typeof(ResXResourceReader), _typeNameConverter));
            }

            _xmlTextWriter.WriteEndElement();
        }

        _xmlTextWriter.WriteEndElement();

        _xmlTextWriter.WriteStartElement(ResHeaderStr);
        {
            _xmlTextWriter.WriteAttributeString(NameStr, WriterStr);
            _xmlTextWriter.WriteStartElement(ValueStr);
            {
                _xmlTextWriter.WriteString(ParserHelpers.GetAssemblyQualifiedName(typeof(ResXResourceWriter), _typeNameConverter));
            }

            _xmlTextWriter.WriteEndElement();
        }

        _xmlTextWriter.WriteEndElement();
    }

    private XmlWriter Writer
    {
        get
        {
            InitializeWriter();
            return _xmlTextWriter;
        }
    }

    /// <summary>
    ///  Adds aliases to the resource file.
    /// </summary>
    public virtual void AddAlias(string? aliasName, AssemblyName assemblyName)
    {
        if (assemblyName == null) { throw new ArgumentNullException(nameof(assemblyName)); }
        _cachedAliases ??= new();
        _cachedAliases[assemblyName.FullName] = aliasName;
    }

    /// <summary>
    ///  Adds the given value to the collection of metadata.  These name/value pairs
    ///  will be emitted to the &lt;metadata&gt; elements in the .resx file.
    /// </summary>
    public void AddMetadata(string name, byte[] value) => AddDataRow(MetadataStr, name, value);

    /// <summary>
    ///  Adds the given value to the collection of metadata.  These name/value pairs
    ///  will be emitted to the &lt;metadata&gt; elements in the .resx file.
    /// </summary>
    public void AddMetadata(string name, string? value) => AddDataRow(MetadataStr, name, value);

    /// <summary>
    ///  Adds the given value to the collection of metadata.  These name/value pairs
    ///  will be emitted to the &lt;metadata&gt; elements in the .resx file.
    /// </summary>
    public void AddMetadata(string name, object? value) => AddDataRow(MetadataStr, name, value);

    /// <summary>
    ///  Adds a blob resource to the resources.
    /// </summary>
    public void AddResource(string name, byte[]? value) => AddDataRow(DataStr, name, value);

    /// <summary>
    ///  Adds a resource to the resources. If the resource is a string, it will be saved that way, otherwise it
    ///  will be serialized and stored with the rules of <see cref="Internal.CustomFormatter.ExtensibleFormatter"/> class.
    /// </summary>
    public void AddResource(string name, object? value)
    {
        if (value is ResXDataNode node)
        {
            AddResource(node);
        }
        else
        {
            AddDataRow(DataStr, name, value);
        }
    }

    /// <summary>
    ///  Adds a string resource to the resources.
    /// </summary>
    public void AddResource(string name, string? value) => AddDataRow(DataStr, name, value);

    /// <summary>
    ///  Adds a string resource to the resources.
    /// </summary>
    public void AddResource(ResXDataNode node)
    {
        // Clone the node to work on a copy.
        ResXDataNode nodeClone = node.DeepClone();
        ResXFileRef? fileRef = nodeClone.FileRef;

        if (fileRef is not null && !string.IsNullOrEmpty(BasePath))
        {
#if NET6_0_OR_GREATER
            string modifiedBasePath = Path.EndsInDirectorySeparator(BasePath)
                ? BasePath
                : $"{BasePath}{Path.DirectorySeparatorChar}";
#else
            string modifiedBasePath = InternalImports.EndsInDirectorySeparator(BasePath)
                ? BasePath
                : $"{BasePath}{Path.DirectorySeparatorChar}";
#endif
            fileRef.MakeFilePathRelative(modifiedBasePath);
        }

        DataNodeInfo info = nodeClone.GetDataNodeInfo();
        AddDataRow(DataStr, info.Name, info.ValueData, info.TypeName, info.MimeType, info.Comment);
    }

    /// <summary>
    ///  Adds a blob resource to the resources.
    /// </summary>
    private void AddDataRow(string elementName, string name, byte[]? value)
        => AddDataRow(
            elementName,
            name,
            value is null ? null : ToBase64WrappedString(value),
            ParserHelpers.GetAssemblyQualifiedName(typeof(byte[]), _typeNameConverter),
            mimeType: null,
            comment: null);

    /// <summary>
    ///  Adds a resource to the resources. If the resource is a string, it will be saved that way, otherwise it
    ///  will be serialized and stored as in binary.
    /// </summary>
    private void AddDataRow(string elementName, string name, object? value)
    {
        if (s_resValueProviderSwitch.TraceVerbose) { System.Diagnostics.Trace.WriteLine($"  resx: adding resource {name}"); }
        switch (value)
        {
            case string str:
                AddDataRow(elementName, name, str);
                break;
            case byte[] bytes:
                AddDataRow(elementName, name, bytes);
                break;
            case ResXFileRef fileRef:
                {
                    ResXDataNode node = new ResXDataNode(name, fileRef, _typeNameConverter);
                    DataNodeInfo info = node.GetDataNodeInfo();
                    AddDataRow(elementName, info.Name, info.ValueData, info.TypeName, info.MimeType, info.Comment);
                    break;
                }

            default:
                {
                    ResXDataNode node = new ResXDataNode(name, value, _typeNameConverter);
                    DataNodeInfo info = node.GetDataNodeInfo();
                    AddDataRow(elementName, info.Name, info.ValueData, info.TypeName, info.MimeType, info.Comment);
                    break;
                }
        }
    }

    /// <summary>
    ///  Adds a string resource to the resources.
    /// </summary>
    private void AddDataRow(string elementName, string name, string? value)
    {
        // if it's a null string, set it here as a resxnullref
        string? typeName =
            value is null
                ? ParserHelpers.GetAssemblyQualifiedName(typeof(ResXNullRef), _typeNameConverter)
                : null;
        AddDataRow(elementName, name, value, typeName, null, null);
    }

    /// <summary>
    ///  Adds a new row to the Resources table. This helper is used because
    ///  we want to always late bind to the columns for greater flexibility.
    /// </summary>
    private void AddDataRow(string elementName, string name, string? value, string? type, string? mimeType, string? comment)
    {
        if (_hasBeenSaved)
        {
            throw new InvalidOperationException(Resources.ResXResourceWriterSaved);
        }

        string? alias = null;
        if (!string.IsNullOrEmpty(type) && elementName == DataStr)
        {
            string? assemblyName = GetFullName(type);
            if (string.IsNullOrEmpty(assemblyName))
            {
                try
                {
                    Type? typeObject = Type.GetType(type);
                    if (typeObject == typeof(string))
                    {
                        type = null;
                    }
                    else if (typeObject is not null)
                    {
                        string? qualifiedName = ParserHelpers.GetAssemblyQualifiedName(typeObject, _typeNameConverter);
                        if (qualifiedName is not null)
                        {
                            // Let this throw argument null for bad type name (to match old behavior)
                            assemblyName = GetFullName(qualifiedName);
                            alias = GetAliasFromName(new AssemblyName(assemblyName!));
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
                // Let this throw argument null for bad type name (to match old behavior)
                alias = GetAliasFromName(new AssemblyName(GetFullName(type)!));
            }
        }

        Writer.WriteStartElement(elementName);
        {
            Writer.WriteAttributeString(NameStr, name);

            if (!string.IsNullOrEmpty(alias) && !string.IsNullOrEmpty(type) && elementName == DataStr)
            {
                Writer.WriteAttributeString(TypeStr, $"{GetTypeName(type)}, {alias}");
            }
            else
            {
                if (type is not null)
                {
                    Writer.WriteAttributeString(TypeStr, type);
                }
            }

            if (mimeType is not null)
            {
                Writer.WriteAttributeString(MimeTypeStr, mimeType);
            }

            if ((type is null && mimeType is null) || (type is not null && type.StartsWith("System.Char", StringComparison.Ordinal)))
            {
                Writer.WriteAttributeString("xml", "space", null, "preserve");
            }

            Writer.WriteStartElement(ValueStr);
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Writer.WriteString(value);
                }
            }

            Writer.WriteEndElement();

            if (!string.IsNullOrEmpty(comment))
            {
                Writer.WriteStartElement(CommentStr);
                {
                    Writer.WriteString(comment);
                }

                Writer.WriteEndElement();
            }
        }

        Writer.WriteEndElement();
    }

    private void AddAssemblyRow(string elementName, string? alias, string? name)
    {
        Writer.WriteStartElement(elementName);
        {
            if (!string.IsNullOrEmpty(alias))
            {
                Writer.WriteAttributeString(AliasStr, alias);
            }

            if (!string.IsNullOrEmpty(name))
            {
                Writer.WriteAttributeString(NameStr, name);
            }
        }

        Writer.WriteEndElement();
    }

    private string? GetAliasFromName(AssemblyName assemblyName)
    {
        _cachedAliases ??= new();
        if (!_cachedAliases.TryGetValue(assemblyName.FullName, out string? alias) || string.IsNullOrEmpty(alias))
        {
            alias = assemblyName.Name;
            AddAlias(alias, assemblyName);
            AddAssemblyRow(AssemblyStr, alias, assemblyName.FullName);
        }

        return alias;
    }

    /// <summary>
    ///  Closes any files or streams locked by the writer.
    /// </summary>
    public void Close() => Dispose();

    /// <summary>
    /// Releases the resources used by the <see cref="ResXResourceWriter"/> instance.
    /// </summary>
    /// <remarks>
    /// The <see cref="Dispose()"/> method calls the <see cref="Generate()"/> method, if necessary, 
    /// before releasing managed and unmanaged system resources. <br />
    /// Calling <see cref="Dispose()"/> allows the system resources used by the <see cref="ResXResourceWriter"/> to be reallocated for other purposes. 
    /// </remarks>
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="ResXResourceWriter"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    /// <remarks>
    /// The <see cref="Dispose()"/> method calls the <see cref="Generate()"/> method, if necessary, 
    /// before releasing managed and unmanaged system resources. <br /> <br />
    /// This method is called by the public <see cref="Dispose()"/> method and the <see cref="System.Object.Finalize()"/> method, if it has been overridden. 
    /// <see cref="IDisposable.Dispose()"/> invokes this method with the <paramref name="disposing"/> parameter set to <see langword="true"/>. 
    /// <see cref="System.Object.Finalize()"/> invokes this method with <paramref name="disposing"/> set to <see langword="false"/>. <br /> <br />
    /// When the <paramref name="disposing"/> parameter is <see langword="true"/>, this method releases all resources held by any managed 
    /// objects that this <see cref="ResXResourceWriter"/> references. This method invokes the <see cref="IDisposable.Dispose()"/> method of each referenced object.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_hasBeenSaved)
            {
                Generate();
            }

            _xmlTextWriter?.Close();
            _xmlTextWriter = null;

            _stream?.Close();
            _stream = null;

            _textWriter?.Close();
            _textWriter = null;
        }
    }

    private static string GetTypeName(string typeName)
    {
        int indexStart = typeName.IndexOf(',');
        return (indexStart == -1) ? typeName : typeName[..indexStart];
    }

    private static string? GetFullName(string typeName)
    {
        int indexStart = typeName.IndexOf(',');
        return indexStart == -1 ? null : typeName[(indexStart + 2)..];
    }

    private static string ToBase64WrappedString(byte[] data)
    {
        const int lineWrap = 80;
        const string prefix = "        ";
        string raw = Convert.ToBase64String(data);
        if (raw.Length > lineWrap)
        {
            // Word wrap on lineWrap chars, \r\n.
            StringBuilder output = new(raw.Length + (raw.Length / lineWrap) * 3);
            int current = 0;

            for (; current < raw.Length - lineWrap; current += lineWrap)
            {
                output.AppendLine();
                output.Append(prefix);
                output.Append(raw, current, lineWrap);
            }

            output.AppendLine();
            output.Append(prefix);
            output.Append(raw, current, raw.Length - current);
            output.AppendLine();
            return output.ToString();
        }

        return raw;
    }

    /// <summary>
    ///  Writes the resources out to the file or stream.
    /// </summary>
    public void Generate()
    {
        if (_hasBeenSaved)
        {
            throw new InvalidOperationException(Resources.ResXResourceWriterSaved);
        }

        _hasBeenSaved = true;
        if (s_resValueProviderSwitch.TraceVerbose)
        {
            System.Diagnostics.Trace.WriteLine("writing XML");
        }

        Writer.WriteEndElement();
        Writer.Flush();
        if (s_resValueProviderSwitch.TraceVerbose)
        {
            System.Diagnostics.Trace.WriteLine("done");
        }
    }
}
