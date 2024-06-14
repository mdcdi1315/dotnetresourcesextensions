// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// -> © MDCDI1315. Edited under open source licenses.

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using DotNetResourcesExtensions.Properties;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions.Internal.ResX;

/// <summary>
/// Represents an element in an XML resource (.resx) file.
/// </summary>
public sealed class ResXDataNode : ISerializable
{
    private static readonly char[] s_specialChars = new char[] { ' ', '\r', '\n' };

    private DataNodeInfo? _nodeInfo;

    private string? _name;
    private string? _comment;

    private string? _typeName; // is only used when we create a resxdatanode manually with an object and contains the FQN

    private string? _fileRefFullPath;
    private string? _fileRefType;
    private string? _fileRefTextEncoding;

    private object? _value;
    private ResXFileRef? _fileRef;

    private CustomFormatter.ICustomFormatter? _binaryFormatter;

    // This is going to be used to check if a ResXDataNode is of type ResXFileRef
    private static readonly ITypeResolutionService s_internalTypeResolver
        = new AssemblyNamesTypeResolutionService(new AssemblyName[] { new AssemblyName("System.Windows.Forms") });

    // Callback function to get type name for multitargeting.
    // No public property to force using constructors for the following reasons:
    // 1. one of the constructors needs this field (if used) to initialize the object, make it consistent with the other constructors to avoid errors.
    // 2. once the object is constructed the delegate should not be changed to avoid getting inconsistent results.
    private Func<Type?, string>? _typeNameConverter;

    private ResXDataNode()
    {
    }

    internal ResXDataNode DeepClone()
    {
        return new ResXDataNode()
        {
            // Nodeinfo is just made up of immutable objects, we don't need to clone it
            _nodeInfo = _nodeInfo?.Clone(),
            _name = _name,
            _comment = _comment,
            _typeName = _typeName,
            _fileRefFullPath = _fileRefFullPath,
            _fileRefType = _fileRefType,
            _fileRefTextEncoding = _fileRefTextEncoding,
            // We don't clone the value, because we don't know how
            _value = _value,
            _fileRef = _fileRef?.Clone(),
            _typeNameConverter = _typeNameConverter
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResXDataNode" /> class.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="value">The resource to store.</param>
    public ResXDataNode(string name, object? value) : this(name, value, typeNameConverter: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResXDataNode" /> class.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="value">The resource to store.</param>
    /// <param name="typeNameConverter">A reference to a method that takes a <see cref="Type"/> and returns a string containing the <see cref="Type"/> name.</param>
    public ResXDataNode(string name, object? value, Func<Type?, string>? typeNameConverter)
    {
        if (name == null) { throw new ArgumentNullException(nameof(name)); }
        if (name.Length == 0)
        {
            throw (new ArgumentException(nameof(name)));
        }

        _typeNameConverter = typeNameConverter;

        Type valueType = (value is null) ? typeof(object) : value.GetType();

#pragma warning disable SYSLIB0050 // Type or member is obsolete
        if (value is not null && !valueType.IsSerializable)
        {
            throw new InvalidOperationException(string.Format(Resources.NotSerializableType, name, valueType.FullName));
        }
#pragma warning restore SYSLIB0050

        if (value is not null)
        {
            _typeName = ParserHelpers.GetAssemblyQualifiedName(valueType, _typeNameConverter);
        }

        _name = name;
        _value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResXDataNode" /> class with a reference to a resource file.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="fileRef">The file reference to use as the resource.</param>
    public ResXDataNode(string name, ResXFileRef fileRef) : this(name, fileRef, typeNameConverter: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResXDataNode" /> class with a reference to a resource file.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="fileRef">The file reference to use as the resource.</param>
    /// <param name="typeNameConverter">A reference to a method that takes a <see cref="Type"/> and returns a string containing the <see cref="Type"/> name.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public ResXDataNode(string name, ResXFileRef fileRef, Func<Type?, string>? typeNameConverter)
    {
        if (System.String.IsNullOrEmpty(name)) { throw new ArgumentNullException(nameof(name)); }
        _name = name;
        if (fileRef == null) { throw new ArgumentNullException(nameof(fileRef)); }
        _fileRef = fileRef;
        _typeNameConverter = typeNameConverter;
    }

    internal ResXDataNode(DataNodeInfo nodeInfo, string? basePath)
    {
        _nodeInfo = nodeInfo;
        _name = nodeInfo.Name;

        // We can only use our internal type resolver here because we only want to check if this is a ResXFileRef
        // node and we can't be sure that we have a typeResolutionService that can recognize this.
        Type? nodeType = null;

        // Default for string is TypeName == null
        if (!string.IsNullOrEmpty(_nodeInfo.TypeName))
        {
            nodeType = s_internalTypeResolver.GetType(_nodeInfo.TypeName, throwOnError: false, ignoreCase: true);
        }

        if (nodeType is not null && nodeType.Equals(typeof(ResXFileRef)))
        {
            // We have a fileref, split the value data and populate the fields.
            string[] fileRefDetails = ResXFileRef.Converter.ParseResxFileRefString(_nodeInfo.ValueData);
            if (fileRefDetails is not null && fileRefDetails.Length > 1)
            {
                _fileRefFullPath = !Path.IsPathRooted(fileRefDetails[0]) && basePath is not null
                    ? Path.Combine(basePath, fileRefDetails[0])
                    : fileRefDetails[0];

                _fileRefType = fileRefDetails[1];
                if (fileRefDetails.Length > 2)
                {
                    _fileRefTextEncoding = fileRefDetails[2];
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets an arbitrary comment regarding this resource.
    /// </summary>
    [AllowNull]
    public string Comment
    {
        get => _comment ?? _nodeInfo?.Comment ?? string.Empty;
        set => _comment = value;
    }

    /// <summary>
    /// Gets or sets the name of this resource.
    /// </summary>
    public string Name
    {
        get
        {
            Debug.Assert(_name is not null || _nodeInfo?.Name is not null);
            return _name ?? _nodeInfo?.Name ?? string.Empty;
        }
        set
        {
            if (System.String.IsNullOrEmpty(value)) { throw new ArgumentException(value , nameof(Name)); }
            _name = value;
        }
    }

    /// <summary>
    /// Gets the file reference for this resource.
    /// </summary>
    public ResXFileRef? FileRef
    {
        get
        {
            if (FileRefFullPath is null)
            {
                return null;
            }

            Debug.Assert(FileRefType is not null);

            return _fileRef ??= string.IsNullOrEmpty(FileRefTextEncoding)
                ? new ResXFileRef(FileRefFullPath, FileRefType!)
                : new ResXFileRef(FileRefFullPath, FileRefType!, Encoding.GetEncoding(FileRefTextEncoding));
        }
    }

    private string? FileRefFullPath => _fileRef?.FileName ?? _fileRefFullPath;

    private string? FileRefType => _fileRef?.TypeName ?? _fileRefType;

    private string? FileRefTextEncoding => _fileRef?.TextFileEncoding?.BodyName ?? _fileRefTextEncoding;

    private static string ToBase64WrappedString(byte[] data)
    {
        const int lineWrap = 80;
        const string prefix = "        ";
        string raw = Convert.ToBase64String(data);
        if (raw.Length > lineWrap)
        {
            // Word wrap on lineWrap chars, \r\n
            StringBuilder output = new StringBuilder(raw.Length + (raw.Length / lineWrap) * 3);
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

    private void FillDataNodeInfoFromObject(DataNodeInfo nodeInfo, object? value)
    {
        if (value is CultureInfo cultureInfo)
        {
            // Special-case CultureInfo, cannot use CultureInfoConverter for serialization.
            nodeInfo.ValueData = cultureInfo.Name;
            nodeInfo.TypeName = ParserHelpers.GetAssemblyQualifiedName(typeof(CultureInfo), _typeNameConverter);
            return;
        }
        else if (value is string @string)
        {
            nodeInfo.ValueData = @string;
            return;
        }
        else if (value is byte[] bytes)
        {
            nodeInfo.ValueData = ToBase64WrappedString(bytes);
            nodeInfo.TypeName = ParserHelpers.GetAssemblyQualifiedName(typeof(byte[]), _typeNameConverter);
            return;
        }

        Type valueType = value?.GetType() ?? typeof(object);
#pragma warning disable SYSLIB0050 // Type or member is obsolete
        if (value is not null && !valueType.IsSerializable)
        {
            throw new InvalidOperationException(string.Format(Resources.NotSerializableType, _name, valueType.FullName));
        }
#pragma warning restore SYSLIB0050

        TypeConverter converter = TypeDescriptor.GetConverter(valueType);

        try
        {
            // Can round trip through string.
            if (converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)))
            {
                nodeInfo.ValueData = converter.ConvertToInvariantString(value) ?? string.Empty;
                nodeInfo.TypeName = ParserHelpers.GetAssemblyQualifiedName(valueType, _typeNameConverter);
                return;
            }
        }
        catch (Exception ex) when (!ParserHelpers.IsCriticalException(ex))
        {
            // Some custom type converters will throw in ConvertTo(string) to indicate that the object should
            // be serialized through ISerializable instead of as a string. This is semi-wrong, but something we
            // will have to live with to allow user created Cursors to be serializable.
        }

        if (converter.CanConvertTo(typeof(byte[])) && converter.CanConvertFrom(typeof(byte[])))
        {
            // Can round trip through byte[]
            byte[]? data = (byte[]?)converter.ConvertTo(value, typeof(byte[]));
            nodeInfo.ValueData = data is null ? string.Empty : ToBase64WrappedString(data);
            nodeInfo.MimeType = ResXResourceWriter.ByteArraySerializedObjectMimeType;
            nodeInfo.TypeName = ParserHelpers.GetAssemblyQualifiedName(valueType, _typeNameConverter);
            return;
        }

        if (value is null)
        {
            nodeInfo.ValueData = string.Empty;
            nodeInfo.TypeName = ParserHelpers.GetAssemblyQualifiedName(typeof(ResXNullRef), _typeNameConverter);
            return;
        }

        // Our custom formatter that fully replaces the BinaryFormatter.
        _binaryFormatter ??= ExtensibleFormatter.Create();
        System.Byte[] arr = _binaryFormatter.GetBytesFromObject(value);
        nodeInfo.TypeName = value.GetType().AssemblyQualifiedName;
        nodeInfo.ValueData = ToBase64WrappedString(arr);
        nodeInfo.MimeType = ResXResourceWriter.Mdcdi1315SerializedObjectMimeType;
    }

    private object? GenerateObjectFromDataNodeInfo(DataNodeInfo dataNodeInfo, ITypeResolutionService? typeResolver)
    {
        string? mimeTypeName = dataNodeInfo.MimeType;

        // Default behavior: if we don't have a type name, it's a string.
        string? typeName = string.IsNullOrEmpty(dataNodeInfo.TypeName)
            ? ParserHelpers.GetAssemblyQualifiedName(typeof(string), _typeNameConverter)
            : dataNodeInfo.TypeName;

        if (!string.IsNullOrEmpty(mimeTypeName))
        {
            // Handle application/x-microsoft.net.object.bytearray.base64.
            return ResolveMimeType(mimeTypeName);
        }

        if (string.IsNullOrEmpty(typeName))
        {
            // If mimeTypeName and typeName are not filled in, the value must be a string.
            Debug.Assert(_value is string, "Resource entries with no Type or MimeType must be encoded as strings");
            return null;
        }

        Type type = ResolveTypeName(typeName);

        if (type == typeof(ResXNullRef))
        {
            return null;
        }

        if (type == typeof(byte[])
            || (typeName.Contains("System.Byte[]") && (typeName.Contains("mscorlib") || typeName.Contains("System.Private.CoreLib"))))
        {
            // Handle byte[]'s, which are stored as base-64 encoded strings. We can't hard-code byte[] type
            // name due to version number updates & potential whitespace issues with ResX files.
            return FromBase64WrappedString(dataNodeInfo.ValueData);
        }

        TypeConverter converter = TypeDescriptor.GetConverter(type);
        if (!converter.CanConvertFrom(typeof(string)))
        {
            Debug.WriteLine($"Converter for {type.FullName} doesn't support string conversion");
            return null;
        }

        try
        {
            return converter.ConvertFromInvariantString(dataNodeInfo.ValueData);
        }
        catch (NotSupportedException nse)
        {
            string newMessage = string.Format(Resources.NotSupported, typeName, dataNodeInfo.ReaderPosition.Y, dataNodeInfo.ReaderPosition.X, nse.Message);
            XmlException xml = new XmlException(newMessage, nse, dataNodeInfo.ReaderPosition.Y, dataNodeInfo.ReaderPosition.X);
            throw new NotSupportedException(newMessage, xml);
        }

        Type ResolveTypeName(string typeName)
        {
            if (ResolveType(typeName, typeResolver) is not Type type)
            {
                string newMessage = string.Format(
                    Resources.TypeLoadException,
                    typeName,
                    dataNodeInfo.ReaderPosition.Y,
                    dataNodeInfo.ReaderPosition.X);

                throw new TypeLoadException(
                    newMessage,
                    new XmlException(newMessage, null, dataNodeInfo.ReaderPosition.Y, dataNodeInfo.ReaderPosition.X));
            }

            return type;
        }

        object? ResolveMimeType(string mimeTypeName)
        {
            if (string.Equals(mimeTypeName, ResXResourceWriter.ByteArraySerializedObjectMimeType)
                && !string.IsNullOrEmpty(typeName)
                && TypeDescriptor.GetConverter(ResolveTypeName(typeName)) is { } converter
                && converter.CanConvertFrom(typeof(byte[]))
                && FromBase64WrappedString(dataNodeInfo.ValueData) is { } serializedData)
            {
                return converter.ConvertFrom(serializedData);
            }

            return null;
        }
    }

    private object? GenerateObjectFromBinaryDataNodeInfo(DataNodeInfo dataNodeInfo , ITypeResolutionService? resservice)
    {
        if (string.Equals(dataNodeInfo.MimeType, ResXResourceWriter.BinSerializedObjectMimeType) == false &&
            string.Equals(dataNodeInfo.MimeType, ResXResourceWriter.Mdcdi1315SerializedObjectMimeType) == false)
        {
            return null;
        }

        byte[] serializedData = FromBase64WrappedString(dataNodeInfo.ValueData);

        if (serializedData.Length <= 0)
        {
            return null;
        }

        _binaryFormatter ??= ExtensibleFormatter.Create();

        object? result = _binaryFormatter.GetObjectFromBytes(serializedData, resservice , dataNodeInfo.TypeName);
        if (result is ResXNullRef)
        {
            result = null;
        }

        return result;
    }

    internal DataNodeInfo GetDataNodeInfo()
    {
        bool shouldSerialize = true;
        if (_nodeInfo is not null)
        {
            shouldSerialize = false;
            _nodeInfo.Name = Name;
        }
        else
        {
            _nodeInfo = new()
            {
                Name = Name
            };
        }

        _nodeInfo.Comment = Comment;

        // We always serialize if this node represents a FileRef. This is because FileRef is a public property,
        // so someone could have modified it.
        if (shouldSerialize || FileRefFullPath is not null)
        {
            // If we don't have a datanodeinfo it could be either a direct object OR a fileref.
            if (FileRefFullPath is not null)
            {
                Debug.Assert(FileRef is not null);
                _nodeInfo.ValueData = FileRef?.ToString() ?? string.Empty;
                _nodeInfo.MimeType = null;
                _nodeInfo.TypeName = ParserHelpers.GetAssemblyQualifiedName(typeof(ResXFileRef), _typeNameConverter);
            }
            else
            {
                // Serialize to string inside the _nodeInfo.
                FillDataNodeInfoFromObject(_nodeInfo, _value);
            }
        }

        return _nodeInfo;
    }

    /// <summary>
    ///  Retrieves the position of the resource in the resource file.
    /// </summary>
    /// <returns>
    ///  A structure that specifies the location of this resource in the resource file as a line position (X) and
    ///  a column position (Y). If this resource is not part of a resource file, this method returns a structure
    ///  that has an X of 0 and a Y of 0.
    /// </returns>
    public Point GetNodePosition() => _nodeInfo?.ReaderPosition ?? default;

    /// <summary>
    ///  Retrieves the type name for the value by using the specified type resolution service
    /// </summary>
    public string? GetValueTypeName(ITypeResolutionService? typeResolver)
    {
        // The type name here is always a fully qualified name.
        if (!string.IsNullOrEmpty(_typeName))
        {
            return _typeName == ParserHelpers.GetAssemblyQualifiedName(typeof(ResXNullRef), _typeNameConverter)
                ? ParserHelpers.GetAssemblyQualifiedName(typeof(object), _typeNameConverter)
                : _typeName;
        }

        string? typeName = FileRefType;
        Type? objectType = null;

        // Do we have a fileref?
        if (typeName is not null)
        {
            // Try to resolve this type.
            objectType = ResolveType(typeName, typeResolver);
        }
        else if (_nodeInfo is not null)
        {
            // We don't have a fileref, try to resolve the type of the datanode.
            typeName = _nodeInfo.TypeName;

            // If typename is null, the default is just a string.
            if (!string.IsNullOrEmpty(typeName))
            {
                objectType = ResolveType(typeName, typeResolver);
            }
            else
            {
                if (string.IsNullOrEmpty(_nodeInfo.MimeType))
                {
                    // No typename, no mimetype, we have a string.
                    typeName = ParserHelpers.GetAssemblyQualifiedName(typeof(string), _typeNameConverter);
                }
                else
                {
                    // Have a mimetype, our only option is to deserialize to know what we're dealing with.
                    try
                    {

                        Type? type = _nodeInfo.MimeType == ResXResourceWriter.Mdcdi1315SerializedObjectMimeType
                            ? GenerateObjectFromBinaryDataNodeInfo(_nodeInfo , typeResolver)?.GetType()
                            : GenerateObjectFromDataNodeInfo(_nodeInfo , typeResolver)?.GetType();

                        typeName = type is null ? null : ParserHelpers.GetAssemblyQualifiedName(type, _typeNameConverter);
                    }
                    catch (Exception ex)
                    {
                        // It would be better to catch SerializationException but the underlying type resolver
                        // can throw things like FileNotFoundException.
                        if (ParserHelpers.IsCriticalException(ex))
                        {
                            throw;
                        }

                        // Something went wrong, type is not specified at all or stream is corrupted return system.object.
                        typeName = ParserHelpers.GetAssemblyQualifiedName(typeof(object), _typeNameConverter);
                    }
                }
            }
        }

        if (objectType is not null)
        {
            typeName = objectType == typeof(ResXNullRef)
                ? ParserHelpers.GetAssemblyQualifiedName(typeof(object), _typeNameConverter)
                : ParserHelpers.GetAssemblyQualifiedName(objectType, _typeNameConverter);
        }

        return typeName;
    }

    /// <summary>
    ///  Retrieves the type name for the value by examining the specified assemblies.
    /// </summary>
    public string? GetValueTypeName(AssemblyName[]? names)
        => GetValueTypeName(new AssemblyNamesTypeResolutionService(names));

    /// <summary>
    ///  Retrieves the object that is stored by this node by using the specified type resolution service.
    /// </summary>
    public object? GetValue(ITypeResolutionService? typeResolver)
    {
        if (_value is not null)
        {
            return _value;
        }

        if (FileRefFullPath is not null)
        {
            if (FileRefType is not null && ResolveType(FileRefType, typeResolver) is not null)
            {
                // We have the fully qualified name for this type
                _fileRef = FileRefTextEncoding is not null
                    ? new ResXFileRef(FileRefFullPath, FileRefType, Encoding.GetEncoding(FileRefTextEncoding))
                    : new ResXFileRef(FileRefFullPath, FileRefType);
                return TypeDescriptor.GetConverter(typeof(ResXFileRef)).ConvertFrom(_fileRef.ToString());
            }

            throw new TypeLoadException(string.Format(Resources.TypeLoadExceptionShort, FileRefType));
        }
        else if (_nodeInfo?.ValueData is not null)
        {
            // We cannot currenly support the normal BinSerializedObjectMimeType , so it will be thrown an exception for this.
            if (_nodeInfo.MimeType == ResXResourceWriter.BinSerializedObjectMimeType)
            {
                throw new NotSupportedException("This ResX reader/writer does not support binary de/serialization. Instead , a custom binary formatter provided does that work.");
            }
            // It's embedded, deserialize it.
            return _nodeInfo.MimeType == ResXResourceWriter.Mdcdi1315SerializedObjectMimeType
                ? GenerateObjectFromBinaryDataNodeInfo(_nodeInfo, typeResolver)
                : GenerateObjectFromDataNodeInfo(_nodeInfo, typeResolver);
        }

        // Schema is wrong and says minOccur for Value is 0, but it's too late to change it.
        return null;
    }

    /// <summary>
    ///  Retrieves the object that is stored by this node by searching the specified assemblies.
    /// </summary>
    public object? GetValue(AssemblyName[]? names) => GetValue(new AssemblyNamesTypeResolutionService(names));

    private static byte[] FromBase64WrappedString(string text)
    {
        if (text.IndexOfAny(s_specialChars) != -1)
        {
            StringBuilder builder = new(text.Length);
            foreach (char c in text)
            {
                switch (c)
                {
                    case ' ':
                    case '\r':
                    case '\n':
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            return Convert.FromBase64String(builder.ToString());
        }

        return Convert.FromBase64String(text);
    }

    private static Type? ResolveType(string typeName, ITypeResolutionService? typeResolver)
    {
        Type? resolvedType = null;
        if (typeResolver is not null)
        {
            // If we cannot find the strong-named type, then try to see if the TypeResolver can bind to partial
            // names. For this, we will strip out the partial names and keep the rest of the strong-name
            // information to try again.

            resolvedType = typeResolver.GetType(typeName, false);
            if (resolvedType is null)
            {
                string[] typeParts = typeName.Split(',');

                // Break up the type name from the rest of the assembly strong name.
                if (typeParts is not null && typeParts.Length >= 2)
                {
                    resolvedType = typeResolver.GetType($"{typeParts[0].Trim()}, {typeParts[1].Trim()}", false);
                }
            }
        }

        return resolvedType ??= Type.GetType(typeName, throwOnError: false);
    }

    void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        => throw new PlatformNotSupportedException();
}
