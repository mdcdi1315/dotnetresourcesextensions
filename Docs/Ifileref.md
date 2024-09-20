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

Additionally , any readers and writers must conform to a set of specified conditions:

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

[Back to Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)