using System.Collections.Generic;
using System.Numerics.Hashing;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Resources.Extensions;

internal sealed class TypeNameComparer : IEqualityComparer<string>
{
	private static readonly char[] s_whiteSpaceChars = new char[4] { ' ', '\n', '\r', '\t' };

	public static TypeNameComparer Instance { get; } = new TypeNameComparer();


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static System.ReadOnlySpan<char> ReadTypeName(System.ReadOnlySpan<char> assemblyQualifiedTypeName)
	{
		int num = MemoryExtensions.IndexOf<char>(assemblyQualifiedTypeName, ',');
		if (num != -1)
		{
			return assemblyQualifiedTypeName.Slice(0, num);
		}
		return assemblyQualifiedTypeName;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static System.ReadOnlySpan<char> ReadAssemblySimpleName(System.ReadOnlySpan<char> assemblyName)
	{
		int num = MemoryExtensions.IndexOf<char>(assemblyName, ',');
		if (num != -1)
		{
			return MemoryExtensions.TrimEnd(assemblyName.Slice(0, num), s_whiteSpaceChars);
		}
		return assemblyName;
	}

	private static bool IsMscorlib(System.ReadOnlySpan<char> assemblyName)
	{
		return MemoryExtensions.Equals(assemblyName, MemoryExtensions.AsSpan("mscorlib"), StringComparison.OrdinalIgnoreCase);
	}

	public bool Equals(string assemblyQualifiedTypeName1, string assemblyQualifiedTypeName2)
	{
		if (assemblyQualifiedTypeName1 == null)
		{
			throw new ArgumentNullException("assemblyQualifiedTypeName1");
		}
		if (assemblyQualifiedTypeName2 == null)
		{
			throw new ArgumentNullException("assemblyQualifiedTypeName2");
		}
		if ((object)assemblyQualifiedTypeName1 == assemblyQualifiedTypeName2)
		{
			return true;
		}
		System.ReadOnlySpan<char> assemblyQualifiedTypeName3 = MemoryExtensions.TrimStart(MemoryExtensions.AsSpan(assemblyQualifiedTypeName1), s_whiteSpaceChars);
		System.ReadOnlySpan<char> assemblyQualifiedTypeName4 = MemoryExtensions.TrimStart(MemoryExtensions.AsSpan(assemblyQualifiedTypeName2), s_whiteSpaceChars);
		System.ReadOnlySpan<char> readOnlySpan = ReadTypeName(assemblyQualifiedTypeName3);
		System.ReadOnlySpan<char> readOnlySpan2 = ReadTypeName(assemblyQualifiedTypeName4);
		if (!MemoryExtensions.Equals(readOnlySpan, readOnlySpan2, StringComparison.Ordinal))
		{
			return false;
		}
		assemblyQualifiedTypeName3 = ((assemblyQualifiedTypeName3.Length > readOnlySpan.Length) ? MemoryExtensions.TrimStart(assemblyQualifiedTypeName3.Slice(readOnlySpan.Length + 1), s_whiteSpaceChars) : System.ReadOnlySpan<char>.Empty);
		assemblyQualifiedTypeName4 = ((assemblyQualifiedTypeName4.Length > readOnlySpan2.Length) ? MemoryExtensions.TrimStart(assemblyQualifiedTypeName4.Slice(readOnlySpan2.Length + 1), s_whiteSpaceChars) : System.ReadOnlySpan<char>.Empty);
		System.ReadOnlySpan<char> readOnlySpan3 = ReadAssemblySimpleName(assemblyQualifiedTypeName3);
		System.ReadOnlySpan<char> readOnlySpan4 = ReadAssemblySimpleName(assemblyQualifiedTypeName4);
		if ((readOnlySpan3.IsEmpty && !assemblyQualifiedTypeName3.IsEmpty) || (readOnlySpan4.IsEmpty && !assemblyQualifiedTypeName4.IsEmpty))
		{
			return false;
		}
		if (readOnlySpan3.IsEmpty)
		{
			if (!readOnlySpan4.IsEmpty)
			{
				return IsMscorlib(readOnlySpan4);
			}
			return true;
		}
		if (readOnlySpan4.IsEmpty)
		{
			return IsMscorlib(readOnlySpan3);
		}
		if (!MemoryExtensions.Equals(readOnlySpan3, readOnlySpan4, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (IsMscorlib(readOnlySpan3))
		{
			return true;
		}
		AssemblyName assemblyName = new AssemblyName(assemblyQualifiedTypeName3.ToString());
		AssemblyName assemblyName2 = new AssemblyName(assemblyQualifiedTypeName4.ToString());
		if (assemblyName.CultureInfo?.LCID != assemblyName2.CultureInfo?.LCID)
		{
			return false;
		}
		byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
		byte[] publicKeyToken2 = assemblyName2.GetPublicKeyToken();
		return MemoryExtensions.SequenceEqual<byte>(MemoryExtensions.AsSpan<byte>(publicKeyToken), publicKeyToken2);
	}

	public unsafe int GetHashCode(string assemblyQualifiedTypeName)
	{
		System.ReadOnlySpan<char> assemblyQualifiedTypeName2 = MemoryExtensions.TrimStart(MemoryExtensions.AsSpan(assemblyQualifiedTypeName), s_whiteSpaceChars);
		System.ReadOnlySpan<char> readOnlySpan = ReadTypeName(assemblyQualifiedTypeName2);
		int num = 0;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			int h = num;
			char c = readOnlySpan[i];
			num = HashHelpers.Combine(h, c.GetHashCode());
		}
		return num;
	}
}
