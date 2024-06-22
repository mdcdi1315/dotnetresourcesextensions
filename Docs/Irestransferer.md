## The `IResourceTransferer` interface.

This interface defines a "default" way to transfer resources from a resource reader to a resource writer.

Although by default it is used only with `System.Resources.IResourceReader` for resource readers ,
there can be also any other implementation (Just what happens with the `IResourceLoader` interface).

Inheritance diagram:
~~~mermaid
---
title: Inheritance Diagram for the IResourceTransferer interface
---
classDiagram

namespace System {

class IDisposable {
	<<Interface>>
	public void Dispose()
}

class IAsyncDisposable {
	<<Interface>>
	public System.Threading.Tasks.ValueTask DisposeAsync()
}}

namespace System_Resources {
	
	class IResourceWriter {
		<<Interface>>
		public void AddResource(System.String , System.String)
		public void AddResource(System.String , System.Object)
		public void AddResource(System.String , System.Byte[])
		public void Close()
		public void Generate()
	}
}

IResourceTransferer <.. IResourceWriter : Implemented
IResourceTransferer <.. IDisposable : Implemented
IResourceTransferer <.. IAsyncDisposable : Implemented

class IResourceTransferer {
	<<Interface>>
	public System.Resources.IResourceWriter CurrentUsedWriter
	public System.Resources.IResourceReader CurrentUsedReader
	public IEnumerable~System.String~ ReaderResourceNames
	public System.Threading.Tasks.ValueTask GenerateAsync()
	public void TransferAll()
	public void TransferSelection(IEnumerable~System.String~ names)
}

class DefaultResourcesTransferer {
	<<Abstract>>
	protected System.Resources.IResourceWriter writer : Writer instance
	protected System.Resources.IResourceReader reader : Reader instance
	public System.Resources.IResourceWriter CurrentUsedWriter : Overriden
	public System.Resources.IResourceReader CurrentUsedReader : Overriden
	public IEnumerable~System.String~ ReaderResourceNames : Overriden
	public void Close() Overriden
	public void Generate() Overriden
	public void AddResource(System.String , System.String) Overriden
	public void AddResource(System.String , System.Object) Overriden
	public void AddResource(System.String , System.Byte[]) Overriden
	public void Dispose() Overriden
	public System.Threading.Tasks.ValueTask DisposeAsync() Overriden
	public System.Threading.Tasks.ValueTask GenerateAsync() Overriden
	public void TransferAll() Overriden
	public void TransferSelection(IEnumerable~System.String~ names) Overriden
	protected void PrepareInstance() Must be called in your inheriting class
	protected DefaultResourcesTransferer() Default constructor
	protected DefaultResourcesTransferer(IResourceReader reader, IResourceWriter writer) : Constructor that directly accepts reader/writer instances
}

IResourceTransferer <.. DefaultResourcesTransferer : Implements

~~~

There is no any implemented example due to the fact that everyting is already covered by an existing 
derived class from `DefaultResourcesTransferer` , the `AbstractResourceTransferer` , 
which accepts any reader and writer to use. Plus , there are some default transferers to use for transfering
resources.

[Back to Index](https://github.com/mdcdi1315/dotnettesourcesextensions/blob/master/Docs/Main.md)