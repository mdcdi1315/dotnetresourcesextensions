## The Custom `System.Resources.Extensions` Resource Classes

These classes are modified cases of the `System.Resources.Extensions` resource readers and writers.

Notes:
- These classes have fully removed the `BinaryFormatter` dependency. This means that you cannot 
load resources written with this dependency.
- Resource objects serialized through `TypeConverter` classes are still supported.
- Resource objects that are serialized through `ICustomFormatter` are also supported.
Additionally , these implement the `IUsingCustomFormatter` abstract interface.
- Resources written with `ICustomFormatter` cannot be read with the actual `System.Resources.Extensions` classes.
Only the provided reader from this project has the information to deserialize formatted objects through `ICustomFormatter`.
- You can still load older resources from .NET Framework. You can also load and .NET Core resources , 
it is just enough to satisfy all the conditions I mentioned above.

You can find and use these classes. 
They are defined inside the `DotNetResourcesExtensions.Internal.DotNetResources` namespace.

The API Usage for them is exactly the same as it used to be for `System.Resources.Extensions` classes.

Just the namespace name does change , in order to be built out for all supported flavors.

[Back to Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)