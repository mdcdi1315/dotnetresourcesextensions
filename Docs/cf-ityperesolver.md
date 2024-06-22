## The `ITypeResolver` interface - How To Use

The `ITypeResolver` defines multiple `IArrayRepresentation<T>`-derived classes to
host so as all to be fed in a `ICustomFormatter`-implementing class , and retrieved if needed.

This allows to:
- Group many converter classes together without losing performance
- Group converter classes that serialize , for example , derived objects from the same interface.
- Get a converter instance directly to be ready to be used by the Custom Formatter.
- Use it to convert the same object but defined in different assembly versions.


Specifically the `ITypeResolver` has the `GetConverter<T>()` method , which returns 
a converter for the type `T` , and the `RegisteredQualifiedTypeNames` property informs
you for the converters that the current type resolver provide.

Example 1:

Let's say that we have the classes `Day` , `Month` , `Week` and `Year` and we have already defined converters 
for them , named as `DayConverter` , `MonthConverter` ,`WeekConverter` and `YearConverter` and a type resolver is
needed for them.

Because these classes are all referring to dates , we can safely name the resolver as `DateResolver`.

Let's define our type resolver:
~~~C#
// At the top of your source file...
using DotNetResourcesExtensions.Internal.CustomFormatter;

class DateResolver : ITypeResolver
{
	private System.Type[] types;
	// We cannot store IArrayRepresentation<T> instances to an array
	// because we have generics issues. Instead , define them as types and will
	// create them through reflection whenever the converters are requested.
	private System.Type[] convertertypes;

	public DateResolver()
	{
		types = new System.Type[] {
			typeof(Day),
			typeof(Month),
			typeof(Week),
			typeof(Year)
			// Add more types if you need...
		};
		convertertypes = new System.Type[] {
			typeof(DayConverter),
			typeof(MonthConverter),
			typeof(WeekConverter),
			typeof(YearConverter),
		};
	}

	public IArrayRepresentation<T> GetConverter<T>()
	{
		for (System.Int32 I = 0; I < types.Length; I++)
		{
			if (types[I] == typeof(T))
			{
				// Create a converter instance through reflection and return it casted to IArrayRepresentation<T>
				return (IArrayRepresentation<T>)convertertypes[I].GetConstructor(System.Type.EmptyTypes).Invoke(new System.Object[0]);
			}
		}
		// Throw this exception with the type provided so as most formatters understand that this resolver does not have a converter for this type of object.
		throw new Exceptions.ConverterNotFoundException(typeof(T));
	}

	public IEnumerable<System.String> RegisteredQualifiedTypeNames {
		get {
			foreach (System.Type tp in types)
			{
				yield return tp.AssemblyQualifiedName;
			}
		}
	}
}
~~~

So , registering this type resolver through a custom formatter would be able to serialize and deserialize
these 4 classes.

<p style="color: aqua">NOTE: <div>Failure to improperly 
register all types in <code>RegisteredQualifiedTypeNames</code> 
property will be considered by the formatters as a invalid instance and 
will be disposed. Be careful when registering type resolvers!</div></p>

[Back to Index](https://github.com/mdcdi1315/dotnettesourcesextensions/blob/master/Docs/Main.md)
