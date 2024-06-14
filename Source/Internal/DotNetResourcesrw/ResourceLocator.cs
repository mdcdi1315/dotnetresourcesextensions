namespace System.Resources.Extensions;

internal readonly struct ResourceLocator
{
	internal int DataPosition { get; }

	internal object Value { get; }

	internal ResourceLocator(int dataPos, object value)
	{
		DataPosition = dataPos;
		Value = value;
	}

	internal static bool CanCache(System.Resources.ResourceTypeCode value)
	{
		return value <= System.Resources.ResourceTypeCode.TimeSpan;
	}
}
