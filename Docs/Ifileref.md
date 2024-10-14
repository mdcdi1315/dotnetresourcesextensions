## The `IFileReference` interface - and it's usage.

The `IFileReference` interface was originally designed on top of the ResX
File Reference class , the [`System.Resources.ResXFileRef`](https://learn.microsoft.com/en-us/dotnet/api/system.resources.resxfileref) class.

But this interface intends to extend it's behavior and make it a generalized solution for saving file references to resource formats efficiently.

The interface layout is this:

~~~mermaid
---
title: IFileReference interface layout
---
classDiagram
class IFileReference {
	<<Interface>>
	public System.String FileName : Contains the file path of this reference.
	public System.Type SavingType : Specifies the type that the resource will be retrieved when any reader reads this reference.
	public FileReferenceEncoding Encoding : Specifies the file encoding that the file has used.
}
~~~

Although that this is the interface itself , a bunch of extension methods can be found in the `IFileReferenceExtensions` class.

Purpose of any object implementing this interface is that all the data required to write the reference are encoded in the resource target , 
but when the reference will be read out , the result data must be returned and not the reference data themselves.

Additionally , any readers and writers must conform to the below set of specified conditions:

1. Any writer must write the file reference efficiently and be aware of such references.
2. The reader must know that the file reference will _NEVER_ be outputted to the user at any circumstance.
3. The reader must follow the following rules when reading the data and ready to produce the result:
	- If the `SavingType` property defines the String type , it returns it's value based on the `Encoding` property , no matter how.
    	- Note that if the `Encoding` property has a value of `Undefined` , it must do the same actions as if `Binary` was defined.
	- If the `SavingType` property defines the bytearray type , it must return the data read by the file as-it-is.
	- If the `SavingType` property does not define none of the stated types , then the data are treated like it was 
	a byte array and passed to the reader's formatter to get the requested object.


At the V2 version of DotNetResourcesExtensions is expected an interface to be added that will add the support of using
file references inside any marked resource writer.

Currently , there are some resources readers and writers that can handle this interface.

The examples will present the usage of the `Custom JSON` format.

All of these examples will require the following code:
~~~C#
using DotNetResourcesExtensions;

public class DefaultFileReference : IFileReference
{
	public System.String FileName { get; set; }

	public System.Type SavingType { get; set; }

	public FileReferenceEncoding Encoding { get; set; }
}
~~~

Example 1: Use a text file that contains plain text and get it's data as a string.

Let's say that we have a file in the root of drive C named cats.txt , which contains cat names delimited with newlines.
You want this file to be saved as a string.

~~~C#
using DotNetResourcesExtensions;

public void Any(JSONResourcesWriter writer)
{
	DefaultFileReference reference = new() { FileName = "C:\\cats.txt" , SavingType = typeof(string) , Encoding = FileReferenceEncoding.UTF8 };
	writer.AddFileReference("name" , reference);
}
~~~

Note that the file may have any possible encoding; make sure that you provide the correct file encoding in Encoding property.

Example 2: Use any file to be included in the list of resources.

Supposing that in a build you want to embed an executable inside a resource file to be executed by the app that has the resources later.

You specify , so a file reference that it's `SavingType` gets a value of `typeof(byte[])`:
~~~C#
using DotNetResourcesExtensions;

public void Any(JSONResourcesWriter writer)
{
	DefaultFileReference reference = new() { FileName = "Path/To/File" , SavingType = typeof(byte[]) };
	writer.AddFileReference("name" , reference);
}
~~~

Be noted that setting the `Encoding` property is not useful here: 
You want binary data , not a string resource. 

Be noted that the `Encoding` property is only supported for those file references which their `SavingType` 
property is `System.String` and nothing else. By default , it will give a value to serialize it. This property is never reached for other types.

Example 3: Use an image file but get it as a `System.Drawing.Bitmap` object.

In WinForms is very much common to see that any image is always encapsulated in a `System.Drawing.Bitmap` object.

Supposing that we want to include an image file to be embedded in the list of resources.

Instead of defining the bytearray type , you define in it's place the `Bitmap` class:

~~~C#
using DotNetResourcesExtensions;

public void Any(JSONResourcesWriter writer)
{
	DefaultFileReference reference = new() { FileName = "Path/To/Image/File" , SavingType = typeof(System.Drawing.Bitmap) };
	writer.AddFileReference("name" , reference);
}
~~~

So , when you will retrieve the resource with name 'name' you will see that you have instead a `Bitmap` instead of arbitrary information.

[Back to Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)