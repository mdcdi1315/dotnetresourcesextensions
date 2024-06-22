## About the `DotNetResourcesExtensions` project


The project was created for a need to provide alternatives on resource handling , initially.

While I continued to develop it , I added it more features and improved the code.

So the result was to provide a lot of abstractions in once , plus stuff I had never imagined to add
here.

Now , the project will be maintained and updated with new features for the indefinite time.

When I originally developed the project the intention was this simple diagram:

~~~mermaid
flowchart TD

A[DotNetResourcesExtensions] <--> B
B[.NET Runtime]
A <--> C[Resource File] <--> B
B <--> D[.NET App] <--> A
~~~

Which means that the resource handling leaves off the app's code and is partially or fully handled by the
`DotNetResourcesExtensions` project.

Additionally , the .NET Runtime provides the code required to access the resource file through the streams and
`DotNetResourcesExtensions` handles the resource read.

Finally the application by this way does not have to manipulate directly the resource file , because all control
is leveraged by the project. With this way you just request resources and are given to you!

[Back to Index](https://github.com/mdcdi1315/dotnettesourcesextensions/blob/master/Docs/Main.md)