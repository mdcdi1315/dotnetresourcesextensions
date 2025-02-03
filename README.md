# The <strong><code>DotNetResourcesExtensions</code></strong> Project <img src="./Global/ProjectImage.png" />

-> What is it?

The name `DotNetResourcesExtensions` refers to exactly what is meant to be:

`"Defines extensions for .NET for Resource API's"` .

That is it.

This project aims to find new alternatives and ways of reading , getting , writing and loading .NET 
resources on a managed application.

The result of this are many interface abstractions , others inspired by how .NET ResX classes create resources , 
and others having a higher level of manipulation , such as a resource loader.

But it does not stop here. It defines also diverse classes for reading and writing resources:

- Custom binary resources. It is the first custom resources binary format for this library.

- Custom JSON resources. Writes and reads out custom JSON resources using the `System.Text.Json` API's.

- Custom XML resources. Writes and reads out custom XML (**NOT** `Resx`) using the `System.Xml` namespace.

- Custom MS-INI resources. Although that sounds weird , those classes depend on a pseudo-syntax of the usual INI files to write and read resources.

- Custom ResX resources. These are modified copies of the ResX classes defined in the [Windows Forms Project](https://github.com/dotnet/winforms). 
The Windows Forms ResX reader cannot read the results produced from them , because they are modified to accomondate some changes in order to make them cross-compatible.
Additionally the Custom ResX reader can now read all ResX formats , old and new , effectively.

    - Due to the modifications happened , one of them was to remove the usage of BinaryFormatter , and create an
	alternative to it. The result was a custom formatter that is allowed to serialize only specific objects.
		- You can also use it and extend it too!

Also this project includes a build target and infrastracture to use for MSBuild projects for resource generation
through the `DotNetResourcesExtensions` project. See the [`BuildTasks`](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/BuildTasks) project for more info.

You may also find useful the new Windows Native Resources Reader plugin. You can see more information about it and how-to-use in the docs.

Currently , the implementation is fairly enough stable.
You can see the usage documentation [here](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md).

Please report any bugs you have found out during the project usage.

Most programs are imperfect , and even this one is no exception. 
I will be very glad to hear about any bugs you have found!

<p style="font-size:9.2509px">
	(This project is open-source and is licensed to you under the MIT License)
</p>