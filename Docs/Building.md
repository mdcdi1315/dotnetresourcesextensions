## Building and Using the `DotNetResourcesExtensions` project

**NOTE**: Most of these alternatives require that you have cloned this repo somewhere.
To clone it you need [`git`](https://git-scm.com) installed and you must execute this command:
~~~cmd
git clone https://github.com/mdcdi1315/dotnetresourcesextensions [DIRECTORY]
~~~
<br />

**NOTE** If you decide to clone the repository , you will need ~250 MB of free disk for building and using the project.

There are four different alternatives to build (and/or use) the project.

Method 1: Perform direct build and directly reference from your project

To build the `DotNetResourcesExtensions` project from it's own only you 
go to the directory where you have placed the cloned repository.

Then you execute there the following dotnet command:
~~~cmd
dotnet build [-f <TARGET-FRAMEWORK>]
~~~

Where `<TARGET-FRAMEWORK>` any supported framework of `DotNetResourcesExtensions`.
If you omit the `-f` flag completely , it will build then for all supported frameworks.

Then you add a reference to the built `DotNetResourcesExtensions` file to your project...
~~~XML
	<ItemGroup>
		<Reference Include="Path/To/DotNetResourcesExtensions.dll" />
	</ItemGroup>
~~~

Method 2: Use the `DotNetResourcesExtensions` package

There is also a package that includes the project on-the-whole and without additional
work from you. You do not even need to clone this repo.

From your project , just add a package reference to the project:

~~~XML
	<ItemGroup>
		<PackageReference Include="DotNetResourcesExtensions" Version="LATEST-STABLE-VERSION" />
	</ItemGroup>
~~~

Where `LATEST-STABLE-VERSION` the latest stable version that is provided. If you do not know how to supply this field , 
then head to [this page](https://www.nuget.org/packages/DotNetResourcesExtensions#versions-body-tab) and see 
the latest stable version (which is any that does not contain -alpha and -beta suffixes).

Copy that version and include it there.

Method 3: Use a Static Layout from a repo build

If you like to access nightly features or fixes , then this is for you!

A Static Layout is a NuGet package layout that emulates the features that the NuGet package also offers , but omitting all NuGet stuff.

Any change before being propagated to the NuGet package is first tested using such layouts.

These layouts have as their sole purpose to test features or to access nightly features.

They have some more complexity from the usual NuGet package and requires some more steps in order to work with your project.

Be noted: Most of these layouts are unstable because they are created using nightly builds of `DotNetResourcesExtensions`.

Also some of the most stable layouts are also delivered in the [Releases Tab](https://github.com/mdcdi1315/dotnetresourcesextensions/releases).

Building and using such a layout:

First execute this command:
~~~cmd
dotnet msbuild -t:GenerateStaticConsumptionLayout
~~~

which will create a static layout from the just built `DotNetResourcesExtensions` library.

If you see in the end an output like `The requested static layout was generated at <PATH>` then the 
Static Layout resides in a folder that the message specifies.

Navigate to that path , and copy the folder somewhere else.

Then , you must import the entry point file to your MSBuild project.

An example of how to import is here:

~~~XML
	<Import Project="Path/To/Static/Layout/DotNetResourcesExtensions.StaticLayout.targets" />

</Project>
~~~

**NOTE** The import directive must be before the project's close tag and after all your project items , properties and targets.

Anything else are performed by the Static Layout. You do not need to take any further actions.
If you have accidentally added a reference to the NuGet package , the layout will override that , delete the package reference and
use it's own references.

If you decide instead to use Static Layouts provided in the repo's [Releases Tab](https://github.com/mdcdi1315/dotnetresourcesextensions/releases) , then you just take 
that release zipped file and do the same actions after the command issue.

Method 4: Use Visual Studio

**NOTE** For this to work , you need the 2022 version and the .NET workload must have been successfully installed.

**NOTE** The project requires the Windows Visual Studio flavor. Anything else might possibly not work. If you like to port the build code so as to work in other platforms 
too , please feel free to report an issue or pull request!

The project also ships with a Visual Studio solution. Through Visual Studio you can build the project or edit it's code as well.

Currently , Visual Studio *DOES NOT SUPPORT THE CREATION OF STATIC LAYOUTS*. And possibly neither do I will add proper code
to build within it.

[Back to Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)