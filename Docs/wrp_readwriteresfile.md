## Reading and writing .RES compiled resource files.

In a normal Win32 project in Visual Studio , during compilation
phase the RC tool produces the central .RES file which contains all
the resources that the .RC file of the project has. This is subsequently
fed to the linker to produce the final executable , and is embedded to it.

The plugin now provides support for both reading and writing such files.

The format that .RES provides is documented in https://learn.microsoft.com/en-us/windows/win32/menurc/resourceheader 
but be noted that it is unclear for some cubersome pad alignments.

The best location to learn about the format is the source files that implement this
logic , specifically in RESFILEReader.cs and RESFILEWriter.cs respectively.

Reading from an exisiting .RES file:

Let's say that you have the RC's generated .RES file and it is named "resource.res".

You can open and read the file in the following fashion:

~~~C#

using DotNetResourcesExtensions;

....
NativeWindowsResFilesReader rdr = new("resource.res");
NativeWindowsResourceEntry entry;
var en = rdr.GetEnumerator();

while (en.MoveNext())
{
	entry = en.ResourceEntry;
	// Do something with this entry.
}
rdr.Dispose();
,,,,

~~~

As you can notice , the process of reading such a file is exactly the same as of a
Win32 assembly , just instead a .RES file is read.

The restriction on the `foreach` statement applies here too.

See the Win32 assembly resource reading document for more information.

*NOTE*(1): The .RES files reader **must** always return the first resource entry as all zeroes - this is an identifier that the file is valid.

Apart from the fact that you can read .RES files , it is also possible to write an in-hand .RES file
by using the plugin!

This can allow you to write or copy , combine and select resources from other files (either these are .RES files or Win32 assemblies)
and emit them in a single .RES file , which in turn the produced file can be fed to a linker that supports reading from .RES files!

Example:

~~~C#

using DotNetResourcesExtensions;

....
NativeWindowsResourceEntry[] entries; // An array that contains resource entries to write here
NativeWindowsResFilesWriter wr = new(stream); // any data stream or file

for (int I = 0; I < entries.Length; I++)
{
	wr.AddResource(entries[I]); // This kind of writer can only accept native Windows resource entries!
}
wr.Generate();
wr.Dispose();
....

~~~
