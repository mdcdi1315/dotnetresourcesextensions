// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Resources;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using DotNetResourcesExtensions.Properties;

namespace DotNetResourcesExtensions.Internal.ResX;

/// <summary>
///  ResX resource reader.
/// </summary>
public partial class ResXResourceReader : IResourceReader
{
    private readonly string? _fileName;
    private TextReader? _reader;
    private Stream? _stream;
    private string? _fileContents;
    private readonly AssemblyName[]? _assemblyNames;
    private string? _basePath;
    private bool _isReaderDirty;
    private readonly ITypeResolutionService? _typeResolver;
    private readonly IAliasResolver _aliasResolver;

    private Dictionary<string, object>? _resData;
    private Dictionary<string, object>? _resMetadata;
    private ResXVersionData verdat;
    private ResXCompatibilityVersion viu;
    private bool _useResXDataNodes;

    private ResXResourceReader(ITypeResolutionService? typeResolver)
    {
        _typeResolver = typeResolver;
        _aliasResolver = new ReaderAliasResolver();
    }

    private ResXResourceReader(AssemblyName[] assemblyNames)
    {
        _assemblyNames = assemblyNames;
        _aliasResolver = new ReaderAliasResolver();
    }

    /// <summary>
    /// Initializes a new instance <see cref="ResXResourceReader"/> , using the specified file name as the base to read 
    /// the resources. <br /> 
    /// The resources file supplied must be a product of the <see cref="ResXResourceWriter"/> class.
    /// </summary>
    /// <param name="fileName">The file name to get resources from.</param>
    public ResXResourceReader(string fileName) : this(fileName, typeResolver: null, aliasResolver: null)
    {
    }

    /// <summary>
    /// Initializes a new instance <see cref="ResXResourceReader"/> , using the specified file name as the base to read 
    /// the resources , and the type resolver to use.  <br />
    /// The resources file supplied must be a product of the <see cref="ResXResourceWriter"/> class.
    /// </summary>
    /// <param name="fileName">The file name to get resources from.</param>
    /// <param name="typeResolver">The type resolver to use for this instance.</param>
    public ResXResourceReader(string fileName, ITypeResolutionService? typeResolver)
        : this(fileName, typeResolver, aliasResolver: null)
    {
    }

    internal ResXResourceReader(string fileName, ITypeResolutionService? typeResolver, IAliasResolver? aliasResolver)
    {
        _fileName = fileName;
        _typeResolver = typeResolver;
        _aliasResolver = aliasResolver ?? new ReaderAliasResolver();
    }

    /// <summary>
    /// Initializes a new instance <see cref="ResXResourceReader"/> , using the specified <see cref="TextReader"/> to get 
    /// the resources from. 
    /// </summary>
    /// <param name="reader">The reader instance to use.</param>
    public ResXResourceReader(TextReader reader) : this(reader, typeResolver: null, aliasResolver: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResXResourceReader"/> class using a text stream reader and a type resolution service.
    /// </summary>
    /// <param name="reader">A text stream reader that contains resources.</param>
    /// <param name="typeResolver">An object that resolves type names specified in a resource.</param>
    public ResXResourceReader(TextReader reader, ITypeResolutionService typeResolver)
        : this(reader, typeResolver, aliasResolver: null)
    {
    }

    internal ResXResourceReader(TextReader reader, ITypeResolutionService? typeResolver, IAliasResolver? aliasResolver)
    {
        _reader = reader;
        _typeResolver = typeResolver;
        _aliasResolver = aliasResolver ?? new ReaderAliasResolver();
    }

    /// <summary>
    /// Initializes a new instance <see cref="ResXResourceReader"/> , using the specified <see cref="Stream"/> to get 
    /// the resources from. 
    /// </summary>
    /// <param name="stream">The stream to read data from.</param>
    public ResXResourceReader(Stream stream) : this(stream, typeResolver: null, aliasResolver: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResXResourceReader"/> class using an input stream and a type resolution service.
    /// </summary>
    /// <param name="stream">An input stream that contains resources.</param>
    /// <param name="typeResolver">An object that resolves type names specified in a resource.</param>
    public ResXResourceReader(Stream stream, ITypeResolutionService typeResolver)
        : this(stream, typeResolver, aliasResolver: null)
    {
    }

    internal ResXResourceReader(Stream stream, ITypeResolutionService? typeResolver, IAliasResolver? aliasResolver)
    {
        _stream = stream;
        _typeResolver = typeResolver;
        _aliasResolver = aliasResolver ?? new ReaderAliasResolver();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResXResourceReader"/> class using a stream and an array of assembly names.
    /// </summary>
    /// <param name="stream">An input stream that contains resources.</param>
    /// <param name="assemblyNames">An array of <see cref="AssemblyName"/> objects that specifies one or more assemblies. The assemblies are used to resolve a type name in the resource to an actual type.</param>
    public ResXResourceReader(Stream stream, AssemblyName[] assemblyNames)
        : this(stream, assemblyNames, aliasResolver: null)
    {
    }

    internal ResXResourceReader(Stream stream, AssemblyName[] assemblyNames, IAliasResolver? aliasResolver)
    {
        _stream = stream;
        _assemblyNames = assemblyNames;
        _aliasResolver = aliasResolver ?? new ReaderAliasResolver();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResXResourceReader"/> class using a <see cref="TextReader"/> object and an array of assembly names.
    /// </summary>
    /// <param name="reader">An object used to read resources from a stream of text.</param>
    /// <param name="assemblyNames">An array of <see cref="AssemblyName"/> objects that specifies one or more assemblies. The assemblies are used to resolve a type name in the resource to an actual type.</param>
    public ResXResourceReader(TextReader reader, AssemblyName[] assemblyNames) : this(reader, assemblyNames, null)
    {
    }

    internal ResXResourceReader(TextReader reader, AssemblyName[] assemblyNames, IAliasResolver? aliasResolver)
    {
        _reader = reader;
        _assemblyNames = assemblyNames;
        _aliasResolver = aliasResolver ?? new ReaderAliasResolver();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResXResourceReader"/> class using an XML resource file name and an array of assembly names.
    /// </summary>
    /// <param name="fileName">The name of an XML resource file that contains resources.</param>
    /// <param name="assemblyNames">An array of <see cref="AssemblyName"/> objects that specifies one or more assemblies. The assemblies are used to resolve a type name in the resource to an actual type.</param>
    public ResXResourceReader(string fileName, AssemblyName[] assemblyNames)
        : this(fileName, assemblyNames, aliasResolver: null)
    {
    }

    internal ResXResourceReader(string fileName, AssemblyName[] assemblyNames, IAliasResolver? aliasResolver)
    {
        _fileName = fileName;
        _assemblyNames = assemblyNames;
        _aliasResolver = aliasResolver ?? new ReaderAliasResolver();
    }

    /// <summary>
    /// Default finalizer.
    /// </summary>
    ~ResXResourceReader() { Dispose(false); }

    /// <summary>
    ///  BasePath for relatives filepaths with ResXFileRefs.
    /// </summary>
    public string? BasePath
    {
        get => _basePath;
        set
        {
            if (_isReaderDirty)
            {
                throw new InvalidOperationException(Resources.InvalidResXBasePathOperation);
            }

            _basePath = value;
        }
    }

    /// <summary>
    ///  ResXFileRef's TypeConverter automatically unwraps it, creates the referenced
    ///  object and returns it. This property gives the user control over whether this unwrapping should
    ///  happen, or a ResXFileRef object should be returned. Default is true for backwards compatibility
    ///  and common case scenarios.
    /// </summary>
    public bool UseResXDataNodes
    {
        get => _useResXDataNodes;
        set
        {
            if (_isReaderDirty)
            {
                throw new InvalidOperationException(Resources.InvalidResXBasePathOperation);
            }

            _useResXDataNodes = value;
        }
    }

    /// <summary>
    /// Specifies the ResX version that the reader currently reads out.
    /// </summary>
    public ResXCompatibilityVersion CurrentRunningVersion => viu;

    /// <summary>
    ///  Closes any files or streams being used by the reader.
    /// </summary>
    public void Close() { ((IDisposable)this).Dispose(); }

    void IDisposable.Dispose() { GC.SuppressFinalize(this); Dispose(true); }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="ResXResourceReader"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_fileName is not null && _stream is not null)
            {
                _stream.Close();
                _stream = null;
            }

            if (_reader is not null)
            {
                _reader.Close();
                _reader = null;
            }
        }
        viu = null;
        verdat = null;
    }

    private static void SetupNameTable(XmlReader reader)
    {
        reader.NameTable.Add(ResXResourceWriter.TypeStr);
        reader.NameTable.Add(ResXResourceWriter.NameStr);
        reader.NameTable.Add(ResXResourceWriter.DataStr);
        reader.NameTable.Add(ResXResourceWriter.MetadataStr);
        reader.NameTable.Add(ResXResourceWriter.MimeTypeStr);
        reader.NameTable.Add(ResXResourceWriter.ValueStr);
        reader.NameTable.Add(ResXResourceWriter.ResHeaderStr);
        reader.NameTable.Add(ResXResourceWriter.VersionStr);
        reader.NameTable.Add(ResXResourceWriter.ResMimeTypeStr);
        reader.NameTable.Add(ResXResourceWriter.ReaderStr);
        reader.NameTable.Add(ResXResourceWriter.WriterStr);
        reader.NameTable.Add(ResXResourceWriter.Mdcdi1315SerializedObjectMimeType);
        reader.NameTable.Add(ResXResourceWriter.BinSerializedObjectMimeType);
        reader.NameTable.Add(ResXResourceWriter.SoapSerializedObjectMimeType);
        reader.NameTable.Add(ResXResourceWriter.AssemblyStr);
        reader.NameTable.Add(ResXResourceWriter.AliasStr);
    }

    /// <summary>
    ///  Demand loads the resource data.
    /// </summary>
    [MemberNotNull(nameof(_resData))]
    [MemberNotNull(nameof(_resMetadata))]
    private void EnsureResData()
    {
        if (_resData is not null && _resMetadata is not null)
        {
            return;
        }

        Debug.Assert(_resData is null && _resMetadata is null);

        _resData = new();
        _resMetadata = new();

        XmlTextReader? contentReader = null;

        try
        {
            // Create the reader and parse the XML
            if (_fileContents is not null)
            {
                contentReader = new XmlTextReader(new StringReader(_fileContents));
            }
            else if (_reader is not null)
            {
                contentReader = new XmlTextReader(_reader);
            }
            else if (_fileName is not null || _stream is not null)
            {
                _stream ??= new FileStream(_fileName!, FileMode.Open, FileAccess.Read, FileShare.Read);
                contentReader = new XmlTextReader(_stream);
            }

            Debug.Assert(contentReader is not null);

            if (contentReader is not null)
            {
                SetupNameTable(contentReader);
                contentReader.WhitespaceHandling = WhitespaceHandling.None;
                ParseXml(contentReader);
            }
        }
        finally
        {
            if (_fileName is not null && _stream is not null)
            {
                _stream.Close();
                _stream = null;
            }
        }
    }

    /// <summary>
    ///  Creates a reader with the specified file contents.
    /// </summary>
    public static ResXResourceReader FromFileContents(string fileContents)
        => FromFileContents(fileContents, (ITypeResolutionService?)null);

    /// <summary>
    ///  Creates a reader with the specified file contents.
    /// </summary>
    public static ResXResourceReader FromFileContents(string fileContents, ITypeResolutionService? typeResolver)
        => new(typeResolver) { _fileContents = fileContents };

    /// <summary>
    ///  Creates a reader with the specified file contents.
    /// </summary>
    public static ResXResourceReader FromFileContents(string fileContents, AssemblyName[] assemblyNames)
        => new(assemblyNames) { _fileContents = fileContents };

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Returns an enumerator for the current <see cref="ResXResourceReader"/> object.
    /// </summary>
    /// <returns>An enumerator for the current <see cref="System.Resources.ResourceReader"/> object.</returns>
    public IDictionaryEnumerator GetEnumerator()
    {
        _isReaderDirty = true;
        EnsureResData();
        return ((IDictionary)_resData).GetEnumerator();
    }

    /// <summary>
    ///  Returns a dictionary enumerator that can be used to enumerate the &lt;metadata&gt; elements in the .resx file.
    /// </summary>
    public IDictionaryEnumerator GetMetadataEnumerator()
    {
        EnsureResData();
        return ((IDictionary)_resMetadata).GetEnumerator();
    }

    /// <summary>
    ///  Attempts to return the line and column (Y, X) of the XML reader.
    /// </summary>
    private static Point GetPosition(XmlReader reader)
    {
        Point pt = default;

        if (reader is IXmlLineInfo lineInfo)
        {
            pt.Y = lineInfo.LineNumber;
            pt.X = lineInfo.LinePosition;
        }

        return pt;
    }

    private void ParseXml(XmlTextReader reader)
    {
        Debug.Assert(_resData is not null);
        Debug.Assert(_resMetadata is not null);

        bool success = false;
        try
        {
            try
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        string s = reader.LocalName;

                        if (reader.LocalName.Equals(ResXResourceWriter.AssemblyStr))
                        {
                            ParseAssemblyNode(reader);
                        }
                        else if (reader.LocalName.Equals(ResXResourceWriter.DataStr))
                        {
                            ParseDataNode(reader, isMetaData: false);
                        }
                        else if (reader.LocalName.Equals(ResXResourceWriter.ResHeaderStr))
                        {
                            ParseResHeaderNode(reader);
                        }
                        else if (reader.LocalName.Equals(ResXResourceWriter.MetadataStr))
                        {
                            ParseDataNode(reader, isMetaData: true);
                        }
                    }
                }

                success = true;
            }
            catch (SerializationException se)
            {
                Point pt = GetPosition(reader);
                string newMessage = string.Format(Resources.SerializationException, reader[ResXResourceWriter.TypeStr], pt.Y, pt.X, se.Message);
                XmlException xml = new XmlException(newMessage, se, pt.Y, pt.X);
                throw new SerializationException(newMessage, xml);
            }
            catch (TargetInvocationException tie)
            {
                Point pt = GetPosition(reader);
                string newMessage = string.Format(Resources.InvocationException, reader[ResXResourceWriter.TypeStr], pt.Y, pt.X, tie.InnerException?.Message);
                XmlException xml = new XmlException(newMessage, tie.InnerException, pt.Y, pt.X);
                throw new TargetInvocationException(newMessage, xml);
            }
            catch (XmlException e)
            {
                throw new ArgumentException(string.Format(Resources.InvalidResXFile, e.Message), e);
            }
            catch (Exception e)
            {
                if (ParserHelpers.IsCriticalException(e))
                {
                    throw;
                }
                else
                {
                    Point pt = GetPosition(reader);
                    XmlException xmlEx = new XmlException(e.Message, e, pt.Y, pt.X);
                    throw new ArgumentException(string.Format(Resources.InvalidResXFile, xmlEx.Message), xmlEx);
                }
            }
        }
        finally
        {
            if (!success)
            {
                _resData = null;
                _resMetadata = null;
            }
        }

        success = false;

        try {
            viu = ResXCompatibilityData.FindBestSuitableVersion(verdat);
            success = true;
        } catch (ArgumentException) { }

        if (!success)
        {
            _resData = null;
            _resMetadata = null;
            viu = null;
            throw new ArgumentException(Resources.InvalidResXFileReaderWriterTypes);
        }
    }

    private void ParseResHeaderNode(XmlReader reader)
    {
        string? name = reader[ResXResourceWriter.NameStr];
        if (name is null)
        {
            return;
        }

        verdat ??= new();

        reader.ReadStartElement();

        // The "1.1" schema requires the correct casing of the strings in the resheader, however the "1.0" schema
        // had a different casing. By checking the Equals first, we should see significant performance improvements.

        if (name == ResXResourceWriter.VersionStr)
        {
            verdat.Version = reader.NodeType == XmlNodeType.Element ? reader.ReadElementString() : reader.Value.Trim();
        } else if (name == ResXResourceWriter.ResMimeTypeStr)
        {
            verdat.ResourceMimeType = reader.NodeType == XmlNodeType.Element ? reader.ReadElementString() : reader.Value.Trim();
        } else if (name == ResXResourceWriter.ReaderStr)
        {
            verdat.ReaderFullString = reader.NodeType == XmlNodeType.Element ? reader.ReadElementString() : reader.Value.Trim();
        } else if (name == ResXResourceWriter.WriterStr)
        {
            verdat.WriterFullString = reader.NodeType == XmlNodeType.Element ? reader.ReadElementString() : reader.Value.Trim();
        } else
        {
            switch (name.ToLower(CultureInfo.InvariantCulture))
            {
                case ResXResourceWriter.VersionStr:
                    verdat.Version = reader.NodeType == XmlNodeType.Element ? reader.ReadElementString() : reader.Value.Trim();
                    break;
                case ResXResourceWriter.ResMimeTypeStr:
                    verdat.ResourceMimeType = reader.NodeType == XmlNodeType.Element ? reader.ReadElementString() : reader.Value.Trim();
                    break;
                case ResXResourceWriter.ReaderStr:
                    verdat.ReaderFullString = reader.NodeType == XmlNodeType.Element ? reader.ReadElementString() : reader.Value.Trim();
                    break;
                case ResXResourceWriter.WriterStr:
                    verdat.WriterFullString = reader.NodeType == XmlNodeType.Element ? reader.ReadElementString() : reader.Value.Trim();
                    break;
            }
        }
    }

    private void ParseAssemblyNode(XmlReader reader)
    {
        string? alias = reader[ResXResourceWriter.AliasStr];
        string? typeName = reader[ResXResourceWriter.NameStr];

        // Let this throw out the way it historically did (argument null)
        AssemblyName assemblyName = new AssemblyName(typeName!);

        if (string.IsNullOrEmpty(alias))
        {
            alias = assemblyName.Name;
        }

        _aliasResolver.PushAlias(alias, assemblyName);
    }

    private void ParseDataNode(XmlTextReader reader, bool isMetaData)
    {
        Debug.Assert(_resData is not null);
        Debug.Assert(_resMetadata is not null);

        DataNodeInfo nodeInfo = new DataNodeInfo
        {
            Name = reader[ResXResourceWriter.NameStr] ?? string.Empty
        };

        string? typeName = reader[ResXResourceWriter.TypeStr];
        string? alias = null;
        AssemblyName? assemblyName = null;

        if (!string.IsNullOrEmpty(typeName))
        {
            alias = GetAliasFromTypeName(typeName);
        }

        if (!string.IsNullOrEmpty(alias))
        {
            assemblyName = _aliasResolver.ResolveAlias(alias);
        }

        nodeInfo.TypeName = assemblyName is not null && typeName is not null
            ? $"{GetTypeFromTypeName(typeName)}, {assemblyName.FullName}"
            : reader[ResXResourceWriter.TypeStr];

        nodeInfo.MimeType = reader[ResXResourceWriter.MimeTypeStr];

        nodeInfo.ReaderPosition = GetPosition(reader);
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.EndElement
                && (reader.LocalName.Equals(ResXResourceWriter.DataStr) || reader.LocalName.Equals(ResXResourceWriter.MetadataStr)))
            {
                // We just found </data>, quit or </metadata>
                break;
            }

            // Could be a <value> or a <comment>
            if (reader.NodeType != XmlNodeType.Element)
            {
                // No <xxxx> tag, just the inside of <data> as text.
                nodeInfo.ValueData = reader.Value.Trim();
                continue;
            }

            if (reader.Name.Equals(ResXResourceWriter.CommentStr))
            {
                nodeInfo.Comment = reader.ReadString();
                continue;
            }

            if (!reader.Name.Equals(ResXResourceWriter.ValueStr))
            {
                continue;
            }

            WhitespaceHandling oldValue = reader.WhitespaceHandling;
            try
            {
                // Based on the documentation at https://learn.microsoft.com/dotnet/api/system.xml.xmltextreader.whitespacehandling
                // this is ok because:
                //
                //  "Because the XmlTextReader does not have DTD information available to it,
                //   SignificantWhitespace nodes are only returned within the an xml:space='preserve' scope."
                //
                // The xml:space would not be present for anything else than string and char (see ResXResourceWriter)
                // so this would not cause any breaking change while reading data from Everett (we never output
                // xml:space then) or from whidbey that is not specifically either a string or a char.
                // However please note that manually editing a resx file in Everett and in Whidbey because of the addition
                // of xml:space=preserve might have different consequences.
                reader.WhitespaceHandling = WhitespaceHandling.Significant;
                nodeInfo.ValueData = reader.ReadString();
            }
            finally
            {
                reader.WhitespaceHandling = oldValue;
            }
        }

        if (nodeInfo.Name is null)
        {
            throw new ArgumentException(string.Format(Resources.InvalidResXResourceNoName, nodeInfo.ValueData));
        }

        ResXDataNode dataNode = new(nodeInfo, _basePath);

        if (UseResXDataNodes)
        {
            _resData[nodeInfo.Name] = dataNode;
        }
        else
        {
            IDictionary data = isMetaData ? _resMetadata : _resData;
            data[nodeInfo.Name] = _assemblyNames is null ? dataNode.GetValue(_typeResolver) : dataNode.GetValue(_assemblyNames);
        }
    }

    private static string GetAliasFromTypeName(string typeName)
    {
        int indexStart = typeName.IndexOf(',');
        return typeName[(indexStart + 2)..];
    }

    private static string GetTypeFromTypeName(string typeName)
    {
        int indexStart = typeName.IndexOf(',');
        return typeName[..indexStart];
    }
}
