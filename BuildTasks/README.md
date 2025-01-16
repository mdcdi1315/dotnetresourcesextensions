## About the `BuildTasks` project

The project aims to bring a different resource generator inside your `MSBuild` projects instead of it's classic 
counterpart which is the `GenerateResource` task provided by the [`MSBuild Tasks`](https://www.nuget.org/packages/Microsoft.Build.Tasks.Core) assembly.

It's purpose is to eliminate external dependencies  , like the 
[`System.Resources.Extensions`](https://nuget.org/packages/System.Resources.Extensions) package , and bring all features that the `DotNetResourcesExtensions` project provides.

The only assembly that the resulting files will need is a reference to the 
[`DotNetResourcesExtensions`](https://nuget.org/packages/DotNetResourcesExtensions) package , from which this project is provided from.

**NOTE**: It is much better to use any package version instead of compiling the project and
use it because it requires many external dependencies you are not aware of. If you **REALLY** want 
instead to use directly your own build of this project , **consider** using a [*Static Layout*](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Building.md) instead.

The project provides: 
- A resource generator that can generate to a single file multiple resources.
- A strongly typed resource class that instead uses the `DotNetResourcesExtensions` API.
- Automated creation , output and usage of resource files without additional hassle
- The resulting resources may be embedded in an assembly and retrieved from there using a
strongly typed resource class.
- Provides the ability to read and write to all the diverse formats provided by the `DotNetResourcesExtensions` project

### Using the `BuildTasks` project

If you have successfully referenced the [`DotNetResourcesExtensions`](https://nuget.org/packages/DotNetResourcesExtensions) package , 
then you can just use the `DotNetResourcesExtensions_GenerateResource` target from your build code.

Be noted that the first build initialisation after referencing the package might be very slow because the `BuildTasks` will download all of it's external dependencies , which it might take some time.


[Back To Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)


