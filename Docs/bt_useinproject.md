## Use the `BuildTasks` in your project

A note first: The `BuildTasks` project does always ship with the 
`DotNetResourcesExtensions` package. Upon referencing it , you have also this project available.

_Note_: Using the BuildTasks project requires that you use MSBuild as your project system , 
and this document requires from you to know a basic grasp of how MSBuild defines it's project files.

### `BuildTasks` basic item definitions.

The project defines a interface to the MSBuild in order to communicate and pass everything needed for the
resource generation.

This is primarily done through two item definitions - the 
`GeneratableResource` item which defines all the input files , and the
`OutputResourceFileDef` item which defines all the output files , respectively.

The item definitions will be described below.

### The `GeneratableResource` item.

The item is defined in MSBuild XML as follows:

~~~xml
<GeneratableResource>
	<GenerateStrClass>False</GenerateStrClass>
	<StrClassLanguage>C#</StrClassLanguage>
	<StrClassName></StrClassName>
	<StrClassManifestName></StrClassManifestName>
	<StrOutPath></StrOutPath>
	<StrClassVisibility>Internal</StrClassVisibility>
	<BaseLoadingDirectory></BaseLoadingDirectory>
</GeneratableResource>
~~~

This item element defines a new input file that can be fed to BuildTasks engine.

Apart the file path which is specified through the `Include` attribute , 
it defines and some secondary options too:

- The `GenerateStrClass` option generates a new strongly-typed resource class suitable enough 
to be used with the `DotNetResourcesExtensions` project only. Specifying 'True' to this option causes
the generator to be triggered during the resource build phase. However , for the generator to work , 
you must also specify some other options too , described below.

- The `StrClassLanguage` option specifies the programming language under which the class will be generated.
Two are the valid values currently , `c#` or `csharp` and `vb` or `visualbasic`. Usually you set this the same value
as your project's programming language.

- The `StrClassName` specifies the full class and namespace where the generated class will live in your project.
If for example you want a class name of 'Resources' living in a namespace 'Generated' , then you would specify the following: Generated.Resources.

- The `StrClassManifestName` specifies the manifest name of the final resource stream saved inside the assembly.
Note: This should be set to the same value that `EmbeddedResource`'s `LogicalName` of the output file has been set.

- The `StrOutPath` specifies the path and the file name under which the generated file will be saved to. 
This option must have a valid path set.

- The `StrClassVisibility` specifies the class visibility by other code elements and/or assemblies.
The default visibility specifies the `Internal` value , which means that the class will be visible only from code elements 
inside the assembly. The `Public` value , on the other hand , specifies that the class will be visible publicly , which 
means other assemblies can access the generated class.

- The `BaseLoadingDirectory` option specifies a base loading directory where all the 
file-reference type resources will be resolved against. Currently this is an unimplemented 
logic in V1 `DotNetResourcesExtensions` assembly and it will be effective in the upcoming 
V2 release.

### The `OutputResourceFileDef` item.

The item is defined in MSBuild XML as follows:

~~~xml
<OutputResourceFileDef>
	<OutputType>Resources</OutputType>
	<Inputs></Inputs>
</OutputResourceFileDef>
~~~

This item specifies all settings required to build the final output files.

Apart the output file path which is specified through the `Include` attribute , 
it defines and some secondary options too:

- The `OutputType` option specifies the format type which this file will be saved under.
It does only currently accept three values: 
    - `Resources` , which generates the classical .NET .resources format.
	- `CustomBinary` , which generates the Custom Binary format of `DotNetResourcesExtensions` project.
	- `JSON` , which generates the Custom JSON format of `DotNetResourcesExtensions` project.

- The `Inputs` option specifies all the input files which this file will be generated from.
  Note that this option must contain the file paths exactly specified as in the Include attributes of `GeneratableResource` item elements.
  For multiple input files , the paths are delimited with semi-colons (;).


### Usage example on a real project file

The following XML project file defines three resource files at the parent directory where the file is saved , 
two output files are generated on the project output , and these two generated files are embedded to the final assembly
by using the .NET SDK `EmbeddedResource` item.

The project also defines strongly-typed resource classes for the three inputs and expects to find a source
file named Class1.cs at the directory of the saved project file.

The project file is configured to build using the C# language.

~~~xml
<Project Sdk="Microsoft.NET.Sdk">
	
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
	<LangVersion>10</LangVersion>
  </PropertyGroup>
  
  <ItemGroup>
	  <GeneratableResource Include="..\.test.resj">
		  <GenerateStrClass>True</GenerateStrClass>
		  <StrClassName>Generated.Resources</StrClassName>
		  <StrClassManifestName>generate.resources</StrClassManifestName>
		  <StrOutPath>$(BaseIntermediateOutputPath)/.generated.cs</StrOutPath>
	  </GeneratableResource>
	  <GeneratableResource Include="..\.test3.resj">
		  <GenerateStrClass>True</GenerateStrClass>
		  <StrClassName>Generated.RED</StrClassName>
		  <StrClassManifestName>EDT.resources</StrClassManifestName>
		  <StrOutPath>$(BaseIntermediateOutputPath)/.generated.2.cs</StrOutPath>
	  </GeneratableResource>
	  <GeneratableResource Include="..\.test2.resh">
		  <GenerateStrClass>True</GenerateStrClass>
		  <StrClassName>Generated.Test2</StrClassName>
		  <StrClassManifestName>EDT.resources</StrClassManifestName>
		  <StrOutPath>$(BaseIntermediateOutputPath)/.generated.3.cs</StrOutPath>
	  </GeneratableResource>
	  <OutputResourceFileDef Include="..\GEN1.resources">
		   <Inputs>..\.test2.resh;..\.test3.resj</Inputs>
	  </OutputResourceFileDef>
	  <OutputResourceFileDef Include="..\GEN2.resources">
		   <Inputs>..\.test.resj</Inputs>
	  </OutputResourceFileDef>
	  <EmbeddedResource Include="..\GEN1.resources">
		   <LogicalName>EDT.resources</LogicalName>
	  </EmbeddedResource>
	  <EmbeddedResource Include="..\GEN2.resources">
		   <LogicalName>generate.resources</LogicalName>
	  </EmbeddedResource>
	  <Compile Include="$(BaseIntermediateOutputPath)/.generated.cs" />
	  <Compile Include="$(BaseIntermediateOutputPath)/.generated.2.cs" />
	  <Compile Include="$(BaseIntermediateOutputPath)/.generated.3.cs" />
  </ItemGroup>
  
  <ItemGroup>
	<PackageReference Include="DotNetResourcesExtensions" Version="1.0.9" />
  </ItemGroup>

	<Target Name="InvokeResGeneration" BeforeTargets="BeforeBuild">
		<CallTarget Targets="DotNetResourcesExtensions_GenerateResource" />
	</Target>

</Project>
~~~

Note that the BuildTasks project will NEVER attempt to instantiate upon referencing the package only;
this is done in order for you to customize when the resource generation target will be invoked.
In the above example , the `InvokeResGeneration` target is the responsible for calling the BuildTasks
resource generator before the actual build.


[Back To Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)



