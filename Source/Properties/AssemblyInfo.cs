
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// Setting ComVisible to false makes the types in this assembly not visible to COM
// components.  If you need to access a type in this assembly from COM, set the ComVisible
// attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM.

[assembly: Guid("ebeab497-d80a-4a81-bb52-3a3633f0ec62")]
// The DLL search paths attribute is removed from here because that support is handled by NativeWindowsResources.

/// <summary>
/// Defines some obsolete API part descriptions.
/// </summary>
internal static class Obsoletions
{
    public const System.String EnumeratorsImplObsoletion = "The old enumerator implementations are now OBSOLETED. " +
        "Use instead the dual enumerator implementations that provide the maximal compatibility and flexibility to your client app.";

    public const System.String AdvancedEnumeratorsObsoleted = "Advanced techniques around the IResourceEnumerable interface have been " +
        "deprecated and will be removed in V3. You do not need to implement this method.";

    public const System.String ExtensionMethodDeprecated_AdvancedEnumerators = "This extension method meant to support the older alternatives " +
        "and will be removed in V3. You do not need to use this method and it is not recommended in new designs.";

    public const System.String EnumeratorNotSupported = "These enumerator implementations were meant to support the internal infrastracture " +
        "and are now not useful. Please move to the new and recommended design instead.";
}