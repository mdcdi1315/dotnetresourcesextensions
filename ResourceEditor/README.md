## `ResourceEditor`: A .NET Framework program to add , remove , or modify resource files.

The program , using as it's backend the `DotNetResourcesExtensions` project , reads and writes 
resources to files. 

You can also create new resource files and view them in a interactive mode.

NOTE: This tool is only supported for Windows due to WinForms dependency and due to the fact that it uses .NET Framework to run.

To build the tool , just `cd` to this directory , open the project file in Visual Studio and build it.

For manipulating resource files at build time , see the [`BuildTasks`](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/BuildTasks/README.md) project.