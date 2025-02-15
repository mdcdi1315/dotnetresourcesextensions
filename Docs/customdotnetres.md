## The Custom `System.Resources.Extensions` Resource Classes

:warning: __NOTE__: The changes described herein are now anymore considered _breaking_ changes between the original 
implementation and the .NET 9+ implementation. Changes will be described below.

These classes are modified cases of the `System.Resources.Extensions` resource readers and writers.

Notes:
- These classes have fully removed the `BinaryFormatter` dependency. This means that you cannot 
load resources written with this dependency.
- Resource objects serialized through `TypeConverter` classes are still supported.
- Resource objects that are serialized through `ICustomFormatter` are also supported.
Additionally , these classes implement the `IUsingCustomFormatter` abstract interface so as to add more resource objects to be serialized.
- Resources written with `ICustomFormatter` cannot be read with the actual `System.Resources.Extensions` classes.
Only the provided reader from this project has the information to deserialize formatted objects through `ICustomFormatter`.
- You can still load older resources from .NET Framework. You can also load and .NET Core resources , 
it is just enough to satisfy all the conditions I mentioned above.

You can find and use these classes. 
They are defined inside the `DotNetResourcesExtensions.Internal.DotNetResources` namespace.

The API Usage for them is exactly the same as it used to be for `System.Resources.Extensions` classes.

Just the namespace name does change , in order to be built out for all supported flavors.

Notes for .NET 9+ and V2 version:

The classes were improved so that these can have better performance and finally be fully compatible
with the model of other readers and writers.

Note that the `IsStreamOwner` property is now fully supported so you can change the code 
to also support normally this property.

Note that `BinaryReader` and `BinaryWriter` classes that the classes used were removed , because all the
methods used to have are now covered by faster internal streaming extension methods.

Finally I would like to mention here that these classes will no longer recieve newer updates from the original
ones as the norm would be.

This is done mostly due to the [`System.Formats.Nrbf`](https://nuget.org/packages/System.Formats.Nrbf) dependency which in fact
it is the old and very-known `BinaryFormatter` class. 

So , I will keep the classic `ICustomFormatter` interface which is enough safe.

:warning: __NOTE__: Nothing is _safe_ when using serialization techniques just like I have defined the
custom formatter. You should _never_ consider as a safe operation the deserialization techniques used
in `ICustomFormatter` even that it performs runtime-existent type and version checking,
that everything is serialized based on their public API's , and that it has rudimentary memory allocation checking.
Even these safe checks can __NEVER__ assure that only _trusted_ data will be processed,
and this is the main reason that NRBF payloads will never be supported on these classes.

 
[Back to Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)