## The `NativeWindowsResources` project

-> What it is about?

This project aims specifically in retrieveing resources from any PE files , 
whether they are written for .NET or not.

Additionally , it aims to intergrate with DotNetResourcesExtensions at a optimal
level and provide a stable and reliable interface for reading Win32 resources.

While the first thought it was to be intergrated in `DotNetResourcesExtensions` project , 
this was not done for a couple or reasons:

- The code requires hijacking the assembly metadata to get the native resources. 
To do that , it uses the [`System.Reflection.Metadata`](https://nuget.org/packages/system.reflection.metadata)
package , which in turn uses other two packages. Generally , it was not my intention to add more dependencies to
the main project.

- The project itself is not any useful than for those who want specifically to read Win32 resources , 
and reading Win32 resources is not in a every day life of a .NET developer.

- The project is only an optional experience that can be selectable.

- Give the ability to share this feature independently and to a different NuGet package than the 
main project.

Be noted that the updates for this project will be less than the main project because
it is optional (it is not another part of the main project) , as I stated above.

**Note**: This project also utilizes code from the `Microsoft.NET.HostModel` assembly , 
where this implementation roots from.

-> Using the project

Most common way would be to reference the distributed package in your project:
~~~XML
	<PackageReference Include="DotNetResourcesExtensions.NativeWindowsResources" Version="1.0.0" />
~~~

Be noted that the Version attribute value is not standard but always make sure that you use the latest stable version of this package.
