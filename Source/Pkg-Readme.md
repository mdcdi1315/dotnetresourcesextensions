﻿## The `DotNetResourcesExtensions` Package

This package adds new possiblities for handling .NET resources ,
for those who do not like to use the default provided alternatives.

This package is provided for four different .NET flavors to use.

Supported .NET flavors:
- .NET Framework 4.7.2 or above
- .NET 7 or above
- .NET Windows Desktop 7 or above
- .NET Standard 2.0 or above

What Possibilities this package adds?

It defines a new alternative to get and load resources.
Additionally , it defines new custom resource readers and writers to use.

The new alternative is the `IResourceLoader` and all it's inheriting classes 
that currently support it.

What this interface does is to define a simple alternative to load and get resources 
in your application.

Additionally it is shipped with some default implementations of this interface so as to 
use it to load with your own implemented `System.Resources.IResourceReader`!

Example:

Supposing that you already have created a custom `System.Resources.IResourceReader` class , 
and you want to connect it with the `IResourceLoader` interface.

Step 1: Create a new class that implements `OptimizedResourceLoader` , an abstract implementation of `IResourceLoader`:

~~~C#
	using DotNetResourcesExtensions;
	
	public class YourClassName : OptimizedResourceLoader
~~~

Where `YourClassName` the name of your class.

Step 2: Create constructors that use on your own `System.Resources.IResourceReader` instance , 
and feed your instance to `read` field.

~~~C#
	{
		public YourClassName(System.String FilePath)
		{
			read = new YourIResourceReaderClass(FilePath);
		}
	}
~~~

Where `YourIResourceReaderClass` the `System.Resources.IResourceReader` instance we mentioned above.

You are ready! 
You can use this simple example to get and read resources.

Except from defining the `IResourceLoader` interface itself , it defines new resource format alternatives.

Currently , Custom ResX , JSON , Custom XML and the Custom Binary Resource Format are some of the formats that this library currently holds.

All readers and writers of course are always implementing the 
`System.Resources.IResourceReader` and
`System.Resources.IResourceWriter` interfaces , respectively.

There are also shipped some default classes for reading resources 
that are implementing the `IResourceLoader` interface.

Such as:

-> `DotNetResourceLoader`  , which loads and gets resources using modified copies of `System.Resources.Extensions` classes.

-> `ResXResourceLoader` , which loads and gets resources from even an arbitrary .resx XML file.

-> `DotNetOldResourceLoader` , which loads and gets resources from the old .resources format.

-> `CustomDataResourcesLoader` , which loads and gets resources from the custom resource stream format.

Note that not all loaders are supported on all platforms and .NET flavors.