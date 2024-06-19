## The `IResourceLoader` interface. 

This is the main abstraction interface that loads arbitrary resources from any source.

I stress the "any source" because the interface cannot only take resources from a resource reader - 
any class implementation that fully implements this interface should work!

Inheritance diagram:

~~~mermaid
---
title: Inheritance Diagram for IResourceLoader interface
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

IResourceLoader <.. IDisposable : Implemented
IResourceLoader <.. IAsyncDisposable : Implemented

class IResourceEnumerable {
	<<Interface>>
	public IAdvancedResourceEnumerator GetAdvancedResourceEnumerator()
	public IFullResourceEnumerator GetEnumerator()
	public ISimpleResourceEnumerator GetSimpleResourceEnumerator()
}

IResourceLoader <.. IResourceEnumerable : Implemented

class IResourceLoader {
	<<Interface>>
	System.String GetStringResource(System.String Name)
	IEnumerable~System.String~ GetRelativeResources(System.String Name)
	IEnumerable~System.String~ GetResourceNamesStartingWith(System.String start)
	IEnumerable~System.String~ GetAllResourceNames()
	T GetResource~T~(System.String Name) where T : notnull
	new void Dispose()
	new System.Threading.Tasks.ValueTask DisposeAsync()
}

IResourceLoader <.. DefaultResourceLoader : Implements

class DefaultResourceLoader {
	<<Abstract>>
	protected System.Resources.IResourceReader read : Resource reader instance

	protected DefaultResourceLoader() Default constructor
	public virtual void Dispose() Overriden
	public virtual System.Threading.Tasks.ValueTask DisposeAsync() Overriden
	void IDisposable.Dispose() Explicit implementation for IDisposable interface
	System.Threading.Tasks.ValueTask IAsyncDisposable.DisposeAsync() Explicit implementation
	public IEnumerable~System.String~ GetAllResourceNames() Overriden
	public virtual System.String GetStringResource(System.String Name) Overriden , Overridable
	public IEnumerable~System.String~ GetResourceNamesStartingWith(System.String start) Overriden
	public virtual T GetResource~T~(System.String Name) where T : notnull : Overriden , Overridable
	public IEnumerable<System.String> GetRelativeResources(System.String Name) Overriden
	public IAdvancedResourceEnumerator GetAdvancedResourceEnumerator() Overriden
	public IFullResourceEnumerator GetEnumerator() Overriden , can work with foreach
	public ISimpleResourceEnumerator GetSimpleResourceEnumerator() Overriden
}

DefaultResourceLoader <|-- OptimizedResourceLoader : Inherits
IResourceLoader <.. OptimizedResourceLoader : Implements

class OptimizedResourceLoader {
	<<Abstract>>
	protected OptimizedResourceLoader() Default Constructor
	public override System.String GetStringResource(System.String Name) Overriden
	public override void Dispose() Overriden
	public override System.Threading.Tasks.ValueTask DisposeAsync() Overriden
}

~~~

And from the `OptimizedResourceLoader` most included loaders do inherit from it.

It is useful to stress here that IF you have a custom resource reader that implements
the `System.Resources.IResourceReader` interface and you want to use this interface ,
then inherit the `DefaultResourceLoader` or the `OptimizedResourceLoader` classes to
a new class and add proper constructor methods that will pass to their `read` field a 
new instance of your reader class.

It is not recommended to directly implement the `IResourceLoader` interface 
due to the fact that someone must implement a lot of stuff that someone might not even
have knowledge for-so leave everything to `DefaultResourceLoader` if possible!


