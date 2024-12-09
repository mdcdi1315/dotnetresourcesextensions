## Reading resource values from various resource types.

The types provided by the reader cannot be directly used for production - 
which it means that there are not a lot of stuff that can be done through a 
literal byte array.

So the project also includes readers for these special values and you can 
use them too.

- The Version Resource reader

The `VsVersionInfoGetter` class takes the burden to read versioning information
from the given resource.

Provides information about the PE version , PE name, and much more.

You can create an instance of this class when the native type of the entry is `RT_VERSION`.

Example:
~~~C#
using DotNetResourcesExtensions;

VsVersionInfoGetter gt = new(/* Your resource entry here */);

// Prints all the properties in all the string tables that the versioning
// information contains.
foreach (var table in gt.Tables)
{
	foreach (var prop in table.Properties)
	{
		System.Console.WriteLine($"{prop.Key}={prop.Value}");
	}
}

// Prints the assembly version contained in the fixed version structure.
System.Console.WriteLine(gt.Information.Version);
~~~

- The String Table block reader

It is known that there are embedded resource strings inside resource files
to use them at run-time. 
These resources are included in the String Table , which centrally collects
all the strings in a RC implemented reader or writer respectively.

When these resources are then converted to binary format ,
RC creates sections for them. A section can contain to a maximum of 16 strings.

When you read resources of type RT_STRING , this is a single string table section.
You can read these sections individually by feeding such a section through the
`NativeStringsCollection` class and learn the strings it contains.

Example:
~~~C#
using DotNetResourcesExtensions;

NativeStringsCollection strings = new(/* Your resource entry here */);

// Play around with the strings data such as printing the 0-th element if exist:
System.Console.WriteLine(strings[0]);
~~~

- The Accelerator Table reader

Some PE files contain in their resource data accelerator tables that are used in
UI's to define temporary fast keyboard shortcuts. 
When the custom sequence is detected , the OS sends back to the UI a unique code that 
means that the accelerator was used , and then the UI must handle that properly.

This unique code is also the table id for the RC format.

The `AcceleratorTable` class reads an accelerator table from such a resource entry.

Example:
~~~C#
using DotNetResourcesExtensions;

AcceleratorTable table = new(/* Your resource entry here */);

foreach (var entry in table.Entries)
{
	System.Console.WriteLine($"{entry.ModifierID}: {entry.VirtualKeyFlags} {entry.ControlKey}");
}
~~~

:warning: The below stated property can be used only from Windows since it calls native API's.

On the `AcceleratorTable` class there is also a `Handle` property which it does return a
`HACCEL` handle to the current acclerator table if it is called. 

This handle is created only once and when the class falls out of scope is automatically
disposed.

You may use this handle in native Windows operations such as using the accelerator table in action.

- The icon or cursor resource group reader

Inside the resource data there are also icon or cursor resource groups that define a subset of the 
contained icons or cursors of a PE file.

This is mostly used for internal RC guidance when locating resources but you can also use it to find
specific resources faster.

Note that this entry might not exist in all PE files that contain icon or cursor resources; 
sometimes it is generated , other times is not , so do not depend on it to load resources in a 'massive' form.

The `ResourceGroupInformation` class can read such entries which their resource type
is either `RT_GROUP_ICON` or `RT_GROUP_CURSOR`.

The entries then contain some critical information about the icon and it's resource id so that it can be located.

Example:
~~~C#
using DotNetResourcesExtensions;

ResourceGroupInformation info = new(/* Your resource entry here */);

// Prints all the resource entries ids that are contained in the current icon or cursor resource group.
foreach (var entry in info.Entries)
{
	System.Console.WriteLine(entry.ResourceId);
}
~~~

- Bitmap file readers

During time to time there were presented different classes that can read bitmap resources
from a plain byte array.

The most lightweight and important abstraction was at first the `BitmapReader` class.
This class takes the raw and unprocessed bitmap data and returns bytes that can be saved subsequently to
a file or a stream. It can work on any platform and does not have any other known platform
restrictions.

:warning: The next alternatives presented can only work in Windows only.
Failling to call them from Windows will result in a
`System.PlatformNotSupportedException` back to the caller.

The safe bitmap handle classes

There are two classes that can load bitmaps and get a Windows GDI Bitmap handle.
The handles produced by the classes can be then used in various native calls or 
even to feed them to the System.Drawing.Bitmap class.

However the one of the two of these handle classes is now deprecated (`SafeDeviceDependentBitmapHandle`) because I 
have not found any usage for it plus it does not work properly in many cases.

Be noted though that the `SafeDeviceIndependentBitmapHandle` class still fails on some bitmaps , there is an effort 
to find a solution soon.

The `GdiPlusBitmap` class

This class added recently due to the fact that it allows to instantiate a bitmap from
multiple sources , including a data stream , a `BitmapReader` class instance,
and from a plain resource entry. You may even instantiate this class from a safe icon handle!

This class corresponds as of .NET the `System.Drawing.Bitmap` class , and as of terms of C/C++ programming
it provides a subset of API's provided in GDI+ Bitmap class.

The main reason of adding this class is the fact that there are cases that you are from Windows , 
but do not have access to the Windows Desktop subsystem - and this hardens up the things when it comes that 
the `System.Drawing` namespace is now only accessible from Windows.

Note that there are still some cases that the `GdiPlusBitmap` fails to create some bitmaps - this is rooting 
from the same cause that bothers the `SafeDeviceIndependentBitmapHandle` class. 
As I stated above I am still looking of how to fix that.

- Icon/Cursor file reader

:notebook: *NOTE*: Anything that applies to icons applies in a similar manner to cursors too.
The equivalent class for cursor handles is called `SafeCursorHandle`.

The icons can be obtained as a Windows icon handles at run-time.
You may also directly save the icon resource entry bytes to a file , 
by using the `IconCursorPackageReader` class.

For Windows only , you can obtain a Windows icon handle to the icon resource entry
by using the `SafeIconHandle` class. This can be then used for a lot of things. 
Among them, you may also use it to instantiate a `System.Drawing.Icon` class!

- The `IconCursorPackageReader` class

The class is a platform-independent method to write valid icons and cursors
that have the .ico and .cur extensions , respectively.

The created data can be then saved to a file or a stream of your choice.

For icons only , you can also create randomly-typed icon packages
from a handful of resource entries that you have selected , thus not
requiring the reader object to obtain any icons.




