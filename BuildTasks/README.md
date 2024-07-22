## About the `BuildTasks` project

The project aims to bring a different resource generator inside your `MSBuild` projects instead of it's classic 
counterpart which is the `GenerateResource` provided by the `MSBuild Tasks` assembly.

It's unique purpose is to eliminate external dependencies  , like the 
[`System.Resources.Extensions`](https://nuget.org/packages/System.Resources.Extensions) package.

The only assembly that the resulting files will need is a reference to the 
`DotNetResourcesExtensions` package , from which this project is provided from.

NOTE: It is much better to use any package version instead of compiling the project and
use it because it requires many external dependencies you are not aware of.

The project provides: 
- A resource generator that can generate to a single file multiple resources.
- A strongly typed resource class that instead uses the `DotNetResourcesExtensions` API.
- Automated creation , output and usage of the resource file without additional hassle
- The resulting resources may be embedded in an assembly and retrieved from there using the 
strongly typed resource class.
- Provides the ability to read and write to all the diverse formats provided by the `DotNetResourcesExtensions` project

### Using the `BuildTasks` project

If you have successfully referenced the `DotNetResourcesExtensions` package , 
then you can just use the `DotNetResExtGenerator` task from your build code.

Be noted that the first initialisation will be very slow because the `BuildTasks` will download all of it's external dependencies , which it might take some time.


[Back To Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)


