using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace System.Resources.Extensions;

/// <summary>Provides APIs similar to <see cref="T:System.Resources.ResourceWriter" /> that can write pre-serialized resource data.</summary>
public sealed class PreserializedResourceWriter : IResourceWriter, IDisposable
{
	private sealed class PrecannedResource
	{
		internal readonly string TypeName;

		internal readonly object Data;

		internal PrecannedResource(string typeName, object data)
		{
			TypeName = typeName;
			Data = data;
		}
	}

	private sealed class StreamWrapper
	{
		internal readonly Stream Stream;

		internal readonly bool CloseAfterWrite;

		internal StreamWrapper(Stream s, bool closeAfterWrite)
		{
			Stream = s;
			CloseAfterWrite = closeAfterWrite;
		}
	}

	private sealed class ResourceDataRecord
	{
		internal readonly SerializationFormat Format;

		internal readonly object Data;

		internal readonly bool CloseAfterWrite;

		internal ResourceDataRecord(SerializationFormat format, object data, bool closeAfterWrite = false)
		{
			Format = format;
			Data = data;
			CloseAfterWrite = closeAfterWrite;
		}
	}

	private const int AverageNameSize = 40;

	internal const string ResourceReaderFullyQualifiedName = "System.Resources.ResourceReader, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	private const string ResSetTypeName = "System.Resources.RuntimeResourceSet";

	private const int ResSetVersion = 2;

	private SortedDictionary<string, object> _resourceList;

	private Stream _output;

	private Dictionary<string, object> _caseInsensitiveDups;

	private Dictionary<string, PrecannedResource> _preserializedData;

	private bool _requiresDeserializingResourceReader;

	internal const string DeserializingResourceReaderFullyQualifiedName = "System.Resources.Extensions.DeserializingResourceReader, System.Resources.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";

	internal const string RuntimeResourceSetFullyQualifiedName = "System.Resources.Extensions.RuntimeResourceSet, System.Resources.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";

	private static readonly string UnknownObjectTypeName = typeof(UnknownType).FullName;

	private static readonly Dictionary<string, Type> s_primitiveTypes = new Dictionary<string, Type>(16, TypeNameComparer.Instance)
	{
		{
			typeof(string).FullName,
			typeof(string)
		},
		{
			typeof(int).FullName,
			typeof(int)
		},
		{
			typeof(bool).FullName,
			typeof(bool)
		},
		{
			typeof(char).FullName,
			typeof(char)
		},
		{
			typeof(byte).FullName,
			typeof(byte)
		},
		{
			typeof(sbyte).FullName,
			typeof(sbyte)
		},
		{
			typeof(short).FullName,
			typeof(short)
		},
		{
			typeof(long).FullName,
			typeof(long)
		},
		{
			typeof(ushort).FullName,
			typeof(ushort)
		},
		{
			typeof(uint).FullName,
			typeof(uint)
		},
		{
			typeof(ulong).FullName,
			typeof(ulong)
		},
		{
			typeof(float).FullName,
			typeof(float)
		},
		{
			typeof(double).FullName,
			typeof(double)
		},
		{
			typeof(decimal).FullName,
			typeof(decimal)
		},
		{
			typeof(DateTime).FullName,
			typeof(DateTime)
		},
		{
			typeof(TimeSpan).FullName,
			typeof(TimeSpan)
		}
	};

	private string ResourceReaderTypeName
	{
		get
		{
			if (!_requiresDeserializingResourceReader)
			{
				return "System.Resources.ResourceReader, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
			}
			return "System.Resources.Extensions.DeserializingResourceReader, System.Resources.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";
		}
	}

	private string ResourceSetTypeName
	{
		get
		{
			if (!_requiresDeserializingResourceReader)
			{
				return "System.Resources.RuntimeResourceSet";
			}
			return "System.Resources.Extensions.RuntimeResourceSet, System.Resources.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.Extensions.PreserializedResourceWriter" /> class that writes the resources to the specified file.</summary>
	/// <param name="fileName">The output file name.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="fileName" /> is <see langword="null" />.</exception>
	public PreserializedResourceWriter(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		_output = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
		_resourceList = new SortedDictionary<string, object>(System.Resources.FastResourceComparer.Default);
		_caseInsensitiveDups = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.Extensions.PreserializedResourceWriter" /> class that writes the resources to the provided stream.</summary>
	/// <param name="stream">The output stream.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> is not writable.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is <see langword="null" />.</exception>
	public PreserializedResourceWriter(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanWrite)
		{
			throw new ArgumentException(DotNetResourcesExtensions.Properties.Resources.Argument_StreamNotWritable);
		}
		_output = stream;
		_resourceList = new SortedDictionary<string, object>(System.Resources.FastResourceComparer.Default);
		_caseInsensitiveDups = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
	}

	/// <summary>Adds a string as a named resource to the list of resources to be written to a file.</summary>
	/// <param name="name">The resource name.</param>
	/// <param name="value">The string to add as a resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The name is <see langword="null" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The resource list is <see langword="null" />.</exception>
	public void AddResource(string name, string? value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (_resourceList == null)
		{
			throw new InvalidOperationException(DotNetResourcesExtensions.Properties.Resources.InvalidOperation_ResourceWriterSaved);
		}
		_caseInsensitiveDups.Add(name, null);
		_resourceList.Add(name, value);
	}

	/// <summary>Adds an object as a named resource to the list of resources to be written to a file.</summary>
	/// <param name="name">The resource name.</param>
	/// <param name="value">The object to add as a resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The name is <see langword="null" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The resource list is <see langword="null" />.</exception>
	/// <exception cref="T:System.ArgumentException">The stream is unseekable.</exception>
	public void AddResource(string name, object? value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (_resourceList == null)
		{
			throw new InvalidOperationException(DotNetResourcesExtensions.Properties.Resources.InvalidOperation_ResourceWriterSaved);
		}
		if (value != null && value is Stream)
		{
			AddResourceInternal(name, (Stream)value, closeAfterWrite: false);
			return;
		}
		_caseInsensitiveDups.Add(name, null);
		_resourceList.Add(name, value);
	}

	/// <summary>Adds a <see cref="T:System.IO.Stream" /> as a named resource to the list of resources to be written to a file.</summary>
	/// <param name="name">The resource name.</param>
	/// <param name="value">The stream to add as a resource.</param>
	/// <param name="closeAfterWrite">An optional value that indicates whether, after resources have been written, the stream should be closed (<see langword="true" />) or left open (<see langword="false" />, the default value).</param>
	public void AddResource(string name, Stream? value, bool closeAfterWrite = false)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (_resourceList == null)
		{
			throw new InvalidOperationException(DotNetResourcesExtensions.Properties.Resources.InvalidOperation_ResourceWriterSaved);
		}
		AddResourceInternal(name, value, closeAfterWrite);
	}

	private void AddResourceInternal(string name, Stream value, bool closeAfterWrite)
	{
		if (value == null)
		{
			_caseInsensitiveDups.Add(name, null);
			_resourceList.Add(name, value);
			return;
		}
		if (!value.CanSeek)
		{
			throw new ArgumentException(DotNetResourcesExtensions.Properties.Resources.NotSupported_UnseekableStream);
		}
		_caseInsensitiveDups.Add(name, null);
		_resourceList.Add(name, new StreamWrapper(value, closeAfterWrite));
	}

	/// <summary>Adds a byte array as a named resource to the list of resources to be written to a file.</summary>
	/// <param name="name">The resource name.</param>
	/// <param name="value">The byte array to add as a resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The name is <see langword="null" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The resource list is <see langword="null" />.</exception>
	public void AddResource(string name, byte[]? value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (_resourceList == null)
		{
			throw new InvalidOperationException(DotNetResourcesExtensions.Properties.Resources.InvalidOperation_ResourceWriterSaved);
		}
		_caseInsensitiveDups.Add(name, null);
		_resourceList.Add(name, value);
	}

	private void AddResourceData(string name, string typeName, object data)
	{
		if (_resourceList == null)
		{
			throw new InvalidOperationException(DotNetResourcesExtensions.Properties.Resources.InvalidOperation_ResourceWriterSaved);
		}
		_caseInsensitiveDups.Add(name, null);
		if (_preserializedData == null)
		{
			_preserializedData = new Dictionary<string, PrecannedResource>(System.Resources.FastResourceComparer.Default);
		}
		_preserializedData.Add(name, new PrecannedResource(typeName, data));
	}

	/// <summary>Calls <see cref="Dispose()" /> to dispose the resource writer.</summary>
	/// <exception cref="System.InvalidOperationException">The resource list is <see langword="null" />.</exception>
	public void Close()
	{
		Dispose(disposing: true);
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_resourceList != null)
			{
				Generate();
			}
			_output?.Dispose();
		}
		_output = null;
		_caseInsensitiveDups = null;
	}

	/// <summary>Calls <see cref="M:System.Resources.Extensions.PreserializedResourceWriter.Generate" /> to write out all resources to the output stream in the system default format.</summary>
	/// <exception cref="T:System.InvalidOperationException">The resource list is <see langword="null" />.</exception>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Writes all resources to the output stream.</summary>
	/// <exception cref="T:System.InvalidOperationException">The resource list is <see langword="null" />.</exception>
	public void Generate()
	{
		if (_resourceList == null)
		{
			throw new InvalidOperationException(DotNetResourcesExtensions.Properties.Resources.InvalidOperation_ResourceWriterSaved);
		}
		BinaryWriter binaryWriter = new BinaryWriter(_output, Encoding.UTF8);
		List<string> list = new List<string>();
		binaryWriter.Write(ResourceManager.MagicNumber);
		binaryWriter.Write(ResourceManager.HeaderVersionNumber);
		MemoryStream memoryStream = new MemoryStream(240);
		BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream);
		binaryWriter2.Write(ResourceReaderTypeName);
		binaryWriter2.Write(ResourceSetTypeName);
		binaryWriter2.Flush();
		binaryWriter.Write((int)memoryStream.Length);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		memoryStream.CopyTo(binaryWriter.BaseStream, (int)memoryStream.Length);
		binaryWriter.Write(2);
		int num = _resourceList.Count;
		if (_preserializedData != null)
		{
			num += _preserializedData.Count;
		}
		binaryWriter.Write(num);
		int[] array = new int[num];
		int[] array2 = new int[num];
		int num2 = 0;
		MemoryStream memoryStream2 = new MemoryStream(num * 40);
		BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream2, Encoding.Unicode);
		Stream stream = new MemoryStream();
		using (stream)
		{
			BinaryWriter binaryWriter4 = new BinaryWriter(stream, Encoding.UTF8);
			if (_preserializedData != null)
			{
				foreach (KeyValuePair<string, PrecannedResource> preserializedDatum in _preserializedData)
				{
					_resourceList.Add(preserializedDatum.Key, preserializedDatum.Value);
				}
			}
			foreach (KeyValuePair<string, object> resource in _resourceList)
			{
				array[num2] = System.Resources.FastResourceComparer.HashFunction(resource.Key);
				array2[num2++] = (int)binaryWriter3.Seek(0, SeekOrigin.Current);
				binaryWriter3.Write(resource.Key);
				binaryWriter3.Write((int)binaryWriter4.Seek(0, SeekOrigin.Current));
				object value = resource.Value;
				System.Resources.ResourceTypeCode resourceTypeCode = FindTypeCode(value, list);
				binaryWriter4.Write7BitEncodedInt((int)resourceTypeCode);
				if (value is PrecannedResource precannedResource)
				{
					WriteData(binaryWriter4, precannedResource.Data);
				}
				else
				{
					WriteValue(resourceTypeCode, value, binaryWriter4);
				}
			}
			binaryWriter.Write(list.Count);
			foreach (string item in list)
			{
				binaryWriter.Write(item);
			}
			Array.Sort(array, array2);
			binaryWriter.Flush();
			int num3 = (int)binaryWriter.BaseStream.Position & 7;
			if (num3 > 0)
			{
				for (int i = 0; i < 8 - num3; i++)
				{
					binaryWriter.Write("PAD"[i % 3]);
				}
			}
			int[] array3 = array;
			foreach (int value2 in array3)
			{
				binaryWriter.Write(value2);
			}
			int[] array4 = array2;
			foreach (int value3 in array4)
			{
				binaryWriter.Write(value3);
			}
			binaryWriter.Flush();
			binaryWriter3.Flush();
			binaryWriter4.Flush();
			int num4 = (int)(binaryWriter.Seek(0, SeekOrigin.Current) + memoryStream2.Length);
			num4 += 4;
			binaryWriter.Write(num4);
			if (memoryStream2.Length > 0)
			{
				memoryStream2.Seek(0L, SeekOrigin.Begin);
				memoryStream2.CopyTo(binaryWriter.BaseStream, (int)memoryStream2.Length);
			}
			binaryWriter3.Dispose();
			stream.Position = 0L;
			stream.CopyTo(binaryWriter.BaseStream);
			binaryWriter4.Dispose();
		}
		binaryWriter.Flush();
		_resourceList = null;
	}

	private static System.Resources.ResourceTypeCode FindTypeCode(object value, List<string> types)
	{
		if (value == null)
		{
			return System.Resources.ResourceTypeCode.Null;
		}
		Type type = value.GetType();
		if (type == typeof(string))
		{
			return System.Resources.ResourceTypeCode.String;
		}
		if (type == typeof(int))
		{
			return System.Resources.ResourceTypeCode.Int32;
		}
		if (type == typeof(bool))
		{
			return System.Resources.ResourceTypeCode.Boolean;
		}
		if (type == typeof(char))
		{
			return System.Resources.ResourceTypeCode.Char;
		}
		if (type == typeof(byte))
		{
			return System.Resources.ResourceTypeCode.Byte;
		}
		if (type == typeof(sbyte))
		{
			return System.Resources.ResourceTypeCode.SByte;
		}
		if (type == typeof(short))
		{
			return System.Resources.ResourceTypeCode.Int16;
		}
		if (type == typeof(long))
		{
			return System.Resources.ResourceTypeCode.Int64;
		}
		if (type == typeof(ushort))
		{
			return System.Resources.ResourceTypeCode.UInt16;
		}
		if (type == typeof(uint))
		{
			return System.Resources.ResourceTypeCode.UInt32;
		}
		if (type == typeof(ulong))
		{
			return System.Resources.ResourceTypeCode.UInt64;
		}
		if (type == typeof(float))
		{
			return System.Resources.ResourceTypeCode.Single;
		}
		if (type == typeof(double))
		{
			return System.Resources.ResourceTypeCode.Double;
		}
		if (type == typeof(decimal))
		{
			return System.Resources.ResourceTypeCode.Decimal;
		}
		if (type == typeof(DateTime))
		{
			return System.Resources.ResourceTypeCode.DateTime;
		}
		if (type == typeof(TimeSpan))
		{
			return System.Resources.ResourceTypeCode.TimeSpan;
		}
		if (type == typeof(byte[]))
		{
			return System.Resources.ResourceTypeCode.ByteArray;
		}
		if (type == typeof(StreamWrapper))
		{
			return System.Resources.ResourceTypeCode.Stream;
		}
		if (type == typeof(PrecannedResource))
		{
			string typeName = ((PrecannedResource)value).TypeName;
			if (typeName.StartsWith("ResourceTypeCode.", StringComparison.Ordinal))
			{
				typeName = typeName.Substring(17);
				return (System.Resources.ResourceTypeCode)Enum.Parse(typeof(System.Resources.ResourceTypeCode), typeName);
			}
			int num = types.IndexOf(typeName);
			if (num == -1)
			{
				num = types.Count;
				types.Add(typeName);
			}
			return (System.Resources.ResourceTypeCode)(num + 64);
		}
		throw new PlatformNotSupportedException(DotNetResourcesExtensions.Properties.Resources.NotSupported_BinarySerializedResources);
	}

	private static void WriteValue(System.Resources.ResourceTypeCode typeCode, object value, BinaryWriter writer)
	{
		switch (typeCode)
		{
		case System.Resources.ResourceTypeCode.String:
			writer.Write((string)value);
			break;
		case System.Resources.ResourceTypeCode.Boolean:
			writer.Write((bool)value);
			break;
		case System.Resources.ResourceTypeCode.Char:
			writer.Write((ushort)(char)value);
			break;
		case System.Resources.ResourceTypeCode.Byte:
			writer.Write((byte)value);
			break;
		case System.Resources.ResourceTypeCode.SByte:
			writer.Write((sbyte)value);
			break;
		case System.Resources.ResourceTypeCode.Int16:
			writer.Write((short)value);
			break;
		case System.Resources.ResourceTypeCode.UInt16:
			writer.Write((ushort)value);
			break;
		case System.Resources.ResourceTypeCode.Int32:
			writer.Write((int)value);
			break;
		case System.Resources.ResourceTypeCode.UInt32:
			writer.Write((uint)value);
			break;
		case System.Resources.ResourceTypeCode.Int64:
			writer.Write((long)value);
			break;
		case System.Resources.ResourceTypeCode.UInt64:
			writer.Write((ulong)value);
			break;
		case System.Resources.ResourceTypeCode.Single:
			writer.Write((float)value);
			break;
		case System.Resources.ResourceTypeCode.Double:
			writer.Write((double)value);
			break;
		case System.Resources.ResourceTypeCode.Decimal:
			writer.Write((decimal)value);
			break;
		case System.Resources.ResourceTypeCode.DateTime:
		{
			long value2 = ((DateTime)value).ToBinary();
			writer.Write(value2);
			break;
		}
		case System.Resources.ResourceTypeCode.TimeSpan:
			writer.Write(((TimeSpan)value).Ticks);
			break;
		case System.Resources.ResourceTypeCode.ByteArray:
		{
			byte[] array3 = (byte[])value;
			writer.Write(array3.Length);
			writer.Write(array3, 0, array3.Length);
			break;
		}
		case System.Resources.ResourceTypeCode.Stream:
		{
			StreamWrapper streamWrapper = (StreamWrapper)value;
			if (streamWrapper.Stream.GetType() == typeof(MemoryStream))
			{
				MemoryStream memoryStream = (MemoryStream)streamWrapper.Stream;
				if (memoryStream.Length > int.MaxValue)
				{
					throw new ArgumentException(DotNetResourcesExtensions.Properties.Resources.ArgumentOutOfRange_StreamLength);
				}
				byte[] array = memoryStream.ToArray();
				writer.Write(array.Length);
				writer.Write(array, 0, array.Length);
				break;
			}
			Stream stream = streamWrapper.Stream;
			if (stream.Length > int.MaxValue)
			{
				throw new ArgumentException(DotNetResourcesExtensions.Properties.Resources.ArgumentOutOfRange_StreamLength);
			}
			stream.Position = 0L;
			writer.Write((int)stream.Length);
			byte[] array2 = new byte[4096];
			int count;
			while ((count = stream.Read(array2, 0, array2.Length)) != 0)
			{
				writer.Write(array2, 0, count);
			}
			if (streamWrapper.CloseAfterWrite)
			{
				stream.Close();
			}
			break;
		}
		default:
			throw new PlatformNotSupportedException(DotNetResourcesExtensions.Properties.Resources.NotSupported_BinarySerializedResources);
		case System.Resources.ResourceTypeCode.Null:
			break;
		}
	}

	/// <summary>Adds a resource of the specified type represented by a string value.</summary>
	/// <param name="name">The resource name.</param>
	/// <param name="value">The value of the resource in string form understood by the type's <see cref="T:System.ComponentModel.TypeConverter" />.</param>
	/// <param name="typeName">The assembly qualified type name of the resource.</param>
	public void AddResource(string name, string value, string typeName)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		if (s_primitiveTypes.TryGetValue(typeName, out var value2))
		{
			if (value2 == typeof(string))
			{
				AddResource(name, value);
				return;
			}
			TypeConverter converter = TypeDescriptor.GetConverter(value2);
			if (converter == null)
			{
				throw new TypeLoadException(String.Format(DotNetResourcesExtensions.Properties.Resources.TypeLoadException_CannotLoadConverter, value2));
			}
			object value3 = converter.ConvertFromInvariantString(value);
			AddResource(name, value3);
		}
		else
		{
			AddResourceData(name, typeName, new ResourceDataRecord(SerializationFormat.TypeConverterString, value));
			_requiresDeserializingResourceReader = true;
		}
	}

	/// <summary>Adds a resource of the specified type represented by a byte array that is passed to the type's <see cref="T:System.ComponentModel.TypeConverter" /> when reading the resource.</summary>
	/// <param name="name">The resource name.</param>
	/// <param name="value">A byte array containing the resource in a form understood by the type's <see cref="T:System.ComponentModel.TypeConverter" />.</param>
	/// <param name="typeName">The assembly qualified type name of the resource.</param>
	public void AddTypeConverterResource(string name, byte[] value, string typeName)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		AddResourceData(name, typeName, new ResourceDataRecord(SerializationFormat.TypeConverterByteArray, value));
		_requiresDeserializingResourceReader = true;
	}

	/// <summary>Adds a resource of the specified type, represented by a byte array, that will be passed to <see cref="T:System.Runtime.Serialization.Formatters.Binary.BinaryFormatter" /> when reading the resource.</summary>
	/// <param name="name">The resource name.</param>
	/// <param name="value">A byte array containing the value of the resource in <c>Byte[]</c> form understood by <see cref="T:System.Runtime.Serialization.Formatters.Binary.BinaryFormatter" />.</param>
	/// <param name="typeName">The optional assembly qualified type name of the resource. The default value is <see langword="null" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="value" /> is <see langword="null" />.</exception>
	[Obsolete("BinaryFormatter serialization is obsolete and should not be used. See https://aka.ms/binaryformatter for more information." , true)]
	public void AddBinaryFormattedResource(string name, byte[] value, string? typeName = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (typeName == null)
		{
			typeName = UnknownObjectTypeName;
		}
		AddResourceData(name, typeName, new ResourceDataRecord(SerializationFormat.BinaryFormatter, value));
		_requiresDeserializingResourceReader = true;
	}

	/// <summary>Adds a resource of the specified type represented by a <see cref="T:System.IO.Stream" /> value that is passed to the type's constructor when reading the resource.</summary>
	/// <param name="name">The resource name.</param>
	/// <param name="value">The value of the resource in <see cref="T:System.IO.Stream" /> form understood by the type's constructor.</param>
	/// <param name="typeName">The assembly qualified type name of the resource.</param>
	/// <param name="closeAfterWrite">An optional value that indicates whether, after resources have been written, the stream should be closed (<see langword="true" />) or left open (<see langword="false" />, the default value).</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" />, <paramref name="typeName" />, or <paramref name="value" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.ArgumentException">The object's type is <see cref="T:System.IO.Stream" />, but it is unseekable.</exception>
	public void AddActivatorResource(string name, Stream value, string typeName, bool closeAfterWrite = false)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		if (!value.CanSeek)
		{
			throw new ArgumentException(DotNetResourcesExtensions.Properties.Resources.NotSupported_UnseekableStream);
		}
		AddResourceData(name, typeName, new ResourceDataRecord(SerializationFormat.ActivatorStream, value, closeAfterWrite));
		_requiresDeserializingResourceReader = true;
	}

	private void WriteData(BinaryWriter writer, object dataContext)
	{
		ResourceDataRecord resourceDataRecord = dataContext as ResourceDataRecord;
		if (_requiresDeserializingResourceReader)
		{
			writer.Write7BitEncodedInt((int)resourceDataRecord.Format);
		}
		try
		{
			switch (resourceDataRecord.Format)
			{
			case SerializationFormat.BinaryFormatter:
			{
				byte[] array2 = (byte[])resourceDataRecord.Data;
				if (_requiresDeserializingResourceReader)
				{
					writer.Write7BitEncodedInt(array2.Length);
				}
				writer.Write(array2);
				break;
			}
			case SerializationFormat.ActivatorStream:
			{
				Stream stream = (Stream)resourceDataRecord.Data;
				if (stream.Length > int.MaxValue)
				{
					throw new ArgumentException(DotNetResourcesExtensions.Properties.Resources.ArgumentOutOfRange_StreamLength);
				}
				stream.Position = 0L;
				writer.Write7BitEncodedInt((int)stream.Length);
				stream.CopyTo(writer.BaseStream);
				break;
			}
			case SerializationFormat.TypeConverterByteArray:
			{
				byte[] array = (byte[])resourceDataRecord.Data;
				writer.Write7BitEncodedInt(array.Length);
				writer.Write(array);
				break;
			}
			case SerializationFormat.TypeConverterString:
			{
				string value = (string)resourceDataRecord.Data;
				writer.Write(value);
				break;
			}
			default:
				throw new ArgumentException("Format");
			}
		}
		finally
		{
			if (resourceDataRecord.Data is IDisposable disposable && resourceDataRecord.CloseAfterWrite)
			{
				disposable.Dispose();
			}
		}
	}
}
