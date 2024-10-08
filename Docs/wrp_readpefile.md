## The Native Windows Resources Plugin

This plugin intends to read native RC resources embedded into PE files.

Be noted , most of it's parts work without the need of Windows platform , 
which does mean that it is portable.

The purpose of these documents is to understand how the plugin 'works'.

For a first start , reference the plugin either by using the [package](https://nuget.org/packages/DotNetResourcesExtensions.NativeWindowsResources) shipped
or by just referencing the project results directly to your target project.

Then the things are simple.

To access the native resources reader you will need to reference the
`DotNetResourcesExtensions` namespace in your source file.

Then , you can use the `NativeWindowsResourcesReader` class.
The class defines two major constructor overloads , one that
directly reads a PE file from a path string , or by accessing a stream:

~~~C#
using DotNetResourcesExtensions;

// somewhere in your code...
NativeWindowsResourcesReader reader = new("filepath");
// Or , alternatively...
System.IO.MemoryStream any; // This contains a PE file.
NativeWindowsResourcesReader reader = new(any);
~~~
And done!

You now have a resources reader to read the PE embedded resources.

*NOTE*: This reader , just like will all other readers in the main project , implements
the `IDotNetResourcesExtensionsReader` interface to be as close as possible to the
`DotNetResourcesExtensions` reader manipulation level , although that some common 
patterns do not apply. See the next doc page for more information.

[Back to Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)