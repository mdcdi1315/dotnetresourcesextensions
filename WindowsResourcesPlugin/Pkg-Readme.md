## The `NativeWindowsResources` package

This package is the artifact result of `NativeWindowsResources` project hosted
in [`DotNetResourcesExtensions`](https://github.com/mdcdi1315/dotnetresourcesextensions)
repository.

The purpose of the package is that it is fully optional; the `DotNetResourcesExtensions` package
can operate without this package.

The package includes resource readers and a basic infrastracture for reading Win32
resources out-of-the-box , for reading RC resources from PE files , or even reading and writing .RES files too!

Additionally , all the interface that does utilize does use common patterns of `DotNetResourcesExtensions` - 
so that intergration can be made easy!

For more information head to the repository website [here](https://github.com/mdcdi1315/dotnetresourcesextensions).

Usage Example Code (Compilable as a C# program):

~~~C# 
 using DotNetResourcesExtensions;

 class Program
 {
	public static void Main(System.String[] args)
	{
		NativeWindowsResourcesReader R = new("C:\\Windows\\System32\\User32.dll");
		NativeWindowsResourceEntry temp;
		var d = R.GetEnumerator();
		while (d.MoveNext())
		{
			temp = d.ResourceEntry;
			// Do something with this entry...
		}
		R.Dispose();
	}
 }
~~~