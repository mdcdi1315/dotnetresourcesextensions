using System.Collections;
using System.Collections.Generic;

namespace System.Resources.Extensions;

internal sealed class RuntimeResourceSet : ResourceSet, IEnumerable
{
	private Dictionary<string, ResourceLocator> _resCache;

	private DeserializingResourceReader _defaultReader;

	private Dictionary<string, ResourceLocator> _caseInsensitiveTable;

	internal RuntimeResourceSet(IResourceReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		_defaultReader = (reader as DeserializingResourceReader) ?? throw new ArgumentException(String.Format(
			DotNetResourcesExtensions.Properties.Resources.NotSupported_WrongResourceReader_Type, reader.GetType()), "reader");
		_resCache = new Dictionary<string, ResourceLocator>(System.Resources.FastResourceComparer.Default);
		_defaultReader._resCache = _resCache;
	}

	protected override void Dispose(bool disposing)
	{
		if (_defaultReader != null)
		{
			if (disposing)
			{
				_defaultReader?.Close();
			}
			_defaultReader = null;
			_resCache = null;
			_caseInsensitiveTable = null;
			base.Dispose(disposing);
		}
	}

	public override IDictionaryEnumerator GetEnumerator()
	{
		return GetEnumeratorHelper();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumeratorHelper();
	}

	private IDictionaryEnumerator GetEnumeratorHelper()
	{
		DeserializingResourceReader defaultReader = _defaultReader;
		if (defaultReader == null)
		{
			throw new ObjectDisposedException(null, DotNetResourcesExtensions.Properties.Resources.ObjectDisposed_ResourceSet);
		}
		return defaultReader.GetEnumerator();
	}

	public override string GetString(string key)
	{
		object @object = GetObject(key, ignoreCase: false, isString: true);
		return (string)@object;
	}

	public override string GetString(string key, bool ignoreCase)
	{
		object @object = GetObject(key, ignoreCase, isString: true);
		return (string)@object;
	}

	public override object GetObject(string key)
	{
		return GetObject(key, ignoreCase: false, isString: false);
	}

	public override object GetObject(string key, bool ignoreCase)
	{
		return GetObject(key, ignoreCase, isString: false);
	}

	private object GetObject(string key, bool ignoreCase, bool isString)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		DeserializingResourceReader defaultReader = _defaultReader;
		Dictionary<string, ResourceLocator> resCache = _resCache;
		if (defaultReader == null || resCache == null)
		{
			throw new ObjectDisposedException(null, DotNetResourcesExtensions.Properties.Resources.ObjectDisposed_ResourceSet);
		}
		ResourceLocator value;
		object obj;
		lock (resCache)
		{
			int dataPosition;
			if (resCache.TryGetValue(key, out value))
			{
				obj = value.Value;
				if (obj != null)
				{
					return obj;
				}
				dataPosition = value.DataPosition;
				return isString ? defaultReader.LoadString(dataPosition) : defaultReader.LoadObject(dataPosition);
			}
			dataPosition = defaultReader.FindPosForResource(key);
			if (dataPosition >= 0)
			{
				obj = ReadValue(defaultReader, dataPosition, isString, out value);
				resCache[key] = value;
				return obj;
			}
		}
		if (!ignoreCase)
		{
			return null;
		}
		bool flag = false;
		Dictionary<string, ResourceLocator> dictionary = _caseInsensitiveTable;
		if (dictionary == null)
		{
			dictionary = new Dictionary<string, ResourceLocator>(StringComparer.OrdinalIgnoreCase);
			flag = true;
		}
		lock (dictionary)
		{
			if (flag)
			{
				DeserializingResourceReader.ResourceEnumerator enumeratorInternal = defaultReader.GetEnumeratorInternal();
				while (enumeratorInternal.MoveNext())
				{
					string key2 = (string)enumeratorInternal.Key;
					ResourceLocator value2 = new ResourceLocator(enumeratorInternal.DataPosition, null);
					dictionary.Add(key2, value2);
				}
				_caseInsensitiveTable = dictionary;
			}
			if (!dictionary.TryGetValue(key, out value))
			{
				return null;
			}
			if (value.Value != null)
			{
				return value.Value;
			}
			obj = ReadValue(defaultReader, value.DataPosition, isString, out value);
			if (value.Value != null)
			{
				dictionary[key] = value;
			}
		}
		return obj;
	}

	private static object ReadValue(DeserializingResourceReader reader, int dataPos, bool isString, out ResourceLocator locator)
	{
		object obj;
		System.Resources.ResourceTypeCode typeCode;
		if (isString)
		{
			obj = reader.LoadString(dataPos);
			typeCode = System.Resources.ResourceTypeCode.String;
		}
		else
		{
			obj = reader.LoadObject(dataPos, out typeCode);
		}
		locator = new ResourceLocator(dataPos, ResourceLocator.CanCache(typeCode) ? obj : null);
		return obj;
	}
}
