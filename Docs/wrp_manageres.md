## Managing resources of a `NativeWindowsResourcesReader`

Although that most classes in the main project have all common conventions
for reading resources , this reader acts differently and returns different
stuff in contrary to the other readers.

After reading this document you will be able to manage native PE resources
by using the plugin resource entries implementations.

-> Reading resources

Let's create now a PE resource reader from the well-known DLL User32 , which
it does contains multiple resources.

~~~C#
using DotNetResourcesExtensions;

// somewhere in your code...
NativeWindowsResourcesReader reader = new("C:\\Windows\\System32\\User32.dll");
~~~

The creation will succeed and the reader instance is saved on the `reader` variable.

Now that we have the reader , the enumerator can be created:

~~~C#
var d = reader.GetEnumerator();
~~~

Important notice here: DO NOT USE THE FOREACH STATEMENT.

The `foreach` statement will only return you the resource name and value as an abstract array
and nothing else - see below a recommneded approach of how to manage resources:

~~~C#
NativeWindowsResourceEntry entry;
while (d.MoveNext())
{
	entry = d.ResourceEntry;
	// Do something interesting with this entry...
}
~~~

This apporach allows you to get the real resource entry - and all the data that carries with it.

-> Properties and positives on using `NativeWindowsResourceEntry`

This special-derived class from the base interface `IResourceEntry`
has it's purpose to hold all the data required by some shipped helpers
to read the actual values out of these resources - which all will be documented later.

A truncated class layout is:
~~~C#

namespace DotNetResourcesExtensions;

public sealed class NativeWindowsResourceEntry : IResourceEntry
{
	public System.UInt16 NumericName { get; }

	public System.Object Name { get; }

	public System.Byte[] Value { get; }

	public System.Globalization.CultureInfo Culture { get; }

	public WindowsResourceEntryType NativeType { get; }

	public System.String NativeTypeString { get; }

	public System.Type TypeOfValue => typeof(System.Byte[]);
}
// Implementations omitted for brevity.
~~~

So this is not a regular `IResourceEntry`-derived instance.

You can see that the name is a object , the resource value
is always a byte array (and so why the `TypeOfValue` property does always return the byte array type)
and it contains some more and unknown properties only specific for this reader:

-> The `NumericName` property defines the resource name as a numeric value - 
it is very common that the RC does write all the resource names with numeric values , 
except for user-defined types (will be documented later - be patient).

-> The `Culture` property gets the culture defined for the current resource.
This format allows each one resource to be saved under different culture directly
(for localization sake).

-> The `NativeType` property defines the 'actual' resource type that the `Value` property
does contain. The value of this resource depends on the resource itself , just like it happens
with the usual resource readers in the main project. This value is actually numeric with some 
common constants defined out - you can consult the `WindowsResourceEntryType` enumeration
to see what resource type of each of the constants do represent.

-> The `NativeTypeString` property acts like the `NativeType` property but returns it's result as a string.
Additionally , if the instance holds a user-defined type , then this property contains the user-defined type name of this resource.


As I described above, the PE resources work in a different manner than a usual resource entry.
And also these resources are always returned in terms of byte arrays , which does discourage you from using the reader -
but this is the step where this plugin helps out , by providing classes to read these 'raw' data , and make these more understandable.

[Back to Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)

