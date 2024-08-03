## The `IResourceEntry` - How To Use


The `IResourceEntry` defines a single , reusuable and reconstructible resource entry.

Result of this interface is the `IResourceEnumerable` interface , which is currently implemented by
the `IResourceLoader` interface.

It's primary usage is to inspect the resource itself for more information and to bridge it with
other resource ways even outside the scope of the `DotNetResourcesExtensions` library.

To retrieve resource entries , you either use the `GetEnumerator()` method provided 
from the `IResourceEnumerable` interface , or the `IResourceLoader` 
that implements `IResourceEnumerable`.

After retrieveing an instance of the `IResourceEntry` , you can:

- Clone the entry
- Test if the entry originates from `IResourceLoader` or any other interface that implements the `IResourceEnumerable` interface.
- Learn the type information set to the `Value` property.
- Test if the entry was cloned
- Cast the entry to other types such as `KeyValuePair` structures.
- Create a resource loader from single entries.
- Compare to other resource entries using the `CompareTo` extension method

Just using the `IResourceEntry` is not enough. To access all the features , 
you must access the extension methods. To access those , you must include
a import directive to your source file to the `DotNetResourcesExtensions` namespace.

[Back to Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)