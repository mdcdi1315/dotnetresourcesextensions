## Building Your Own Formatter using the `BaseFormatter` class.

You will learn more into how building and using a custom derived class from the
`BaseFormatter` abstract class.

In order to reach here it means that the `ExtensibleFormatter` is not enough for 
you. And it is logical , because you might need to defer or change types during de/serialization.
This is specially true for resource formats where they need to store the data in another format than 
they are defined.

I will only mention that if you are going to implement and consume it in a resource class,
consider implementing the `IUsingCustomFormatter` interface in the resource class , 
which can give to the user the option to add custom types and serialize them using your resource class.

For first start add the classic import at the top of a new source file:

~~~C#
using DotNetResourcesExtensions.Internal.CustomFormatter;
~~~

Then , create a new class that derives from `BaseFormatter`:

~~~C#
public class NewFormatter : BaseFormatter
{
	
}
~~~

Create basic constructors. I strongly recommend adding constructor overloads that support
adding more custom type resolvers:
~~~C#
public NewFormatter(params ITypeResolver[] resolvers) 
{
	Resolvers.AddRange(resolvers);
	// More code here?
}
~~~
Note: If you want to also support basic BCL types shipped with the project , 
invoke the GetDefaultTypeResolver() method defined in `FormatterExtensions` class , and feed it's result
to the `Resolvers` field at your implemented constructor(s):
~~~C#
public NewFormatter()
{
	Resolvers.Add(FormatterExtensions.GetDefaultTypeResolver());
}
~~~

Note that the `Resolvers` field is a [`List<ITypeResolver>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=net8.0) class so
you can add more type resolvers , remove or even clear all the registered resolvers through it.

Next step is to override the abstract `Resolve` method.

Note that you can resolve converters in any way you wish; Just to be enough the object it 
returns to be an `IArrayRepresentation`-implementing class.

For the sake of this example , the resource type probed will be changed to a `ResourceStream` that it is supposed 
to be implemented when the type given is a `System.IO.FileStream`.

~~~C#
public override IArrayRepresentation<T> Resolve<T>()
{
	if (typeof(T) == typeof(System.IO.FileStream))
	{
		return Resolve<ResourceStream>();
	}
	// Other resolving code here...
}
~~~

Done! you have successfully created a usuable formatter class that extends the `BaseFormatter` class.

[Back To Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)
