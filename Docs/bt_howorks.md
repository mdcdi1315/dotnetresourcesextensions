## The `BuildTasks` logic on processing resources.

Unlike the default .NET alternatives (which is in MSBuild the `GenerateResource` target) ,
the `BuildTasks` project aims to extend the possibilities on how resources are produced for the final targets.

The newly reworked engine of the project processes the resources in a different manner than usual resource tools.

### 'Process multiple files to multiple files'

This is the primary principle that BuildTasks uses so that it can generate resources.

You specify your desired resource files and these can be specified in a single target.

Multiple different source and target files may also be specified.

### Engine processing

The `BuildTasks` engine processes all the files as if we have to generate 'output files'.

For each output file , all the input files are retrieved , opened one-by-one and generated to the final output file.

Once all input files are processed for one output file , the file is closed , and the engine procceds 
with the creation of another output file , as the project has specified.

So , in the project you just simply declare which files act as input ones with settings that you have chosen ,
and the output ones declare which input files will use and where will be saved.

[Back To Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)



