## Using the `DotNetResourcesExtensions` Custom Formatter

The most recommended approaches are two:

-> Either you use the ready `ExtensibleFormatter` class.

-> Or you create a formatter that inherits the `BaseFormatter` class.

Note: The  `ExtensibleFormatter` can also have your own and custom registered converters.
It is enough to just provide your own `ITypeResolver` instance.

From the other side , if you use the `BaseFormatter` , you must declare how the desirable converter will be probed out each time , 
by implementing the `Resolve<T>()` method.

For `BaseFormatter` , the same stuff apply as of `ExtensibleFormatter` in everyting else , such as it can accept custom type resolvers.

Example 1: Using the `ExtensibleFormatter`:

You can easily include the `ExtensibleFormatter` in your code.
Be noted that by default it provides support for major system types , such as 
`System.Int32` , `System.Single` and `System.Double`. If you need to serialize types that the included type resolver does not include , 
then just create one type resolver of your own , implement your own converter(s) ,
and include your resolver through the `RegisterTypeResolver` method.

If you want to serialize some custom types only and you don't want to create a new type resolver class that derives
from `ITypeResolver` , you can create an 'implicit' type resolver through the `ConstructTypeResolver` extension methods.
The method belongs to the `IArrayRepresentation<T>` interface.

But for the most cases , even the default type resolver might be enough.

~~~C#
// At the top of your source file...
using DotNetResourcesExtensions.Internal.CustomFormatter;

// During coding...
System.Int32 objecttoserialize = 89754;
System.Byte[] data = cf.GetBytes(objecttoserialize);
// Do something with the serialized data...

// Possibly the custom formatter will have been declared as a local or a field and initialised before:
ExtensibleFormatter cf = new();
~~~

Example 2: Declare a class that extends the `BaseFormatter` and call it from your code:

Presuming that you need your own custom formatter class because you might want to probe the converter differently than the `ExtensibleFormatter` does probe.

~~~C#
// At the top of your source file...
using DotNetResourcesExtensions.Internal.CustomFormatter;

// Create the custom derived class...
class CustomFormatter : BaseFormatter
{
	
	protected override IArrayRepresentation<T> Resolve<T>()
	{
		// Define your own probing method here...
	}

}

// Use it in your code...
System.Int32 objecttoserialize = 89754;
System.Byte[] data = cf.GetBytes(objecttoserialize);
// Do something with the serialized data...

// And define before the 'cf' as:
CustomFormatter cf = new();
~~~

Example 3: Include a converter for a custom class that is NOT defined in the default type resolver.

You might need to serialize a class that is not defined in the default type resolver.

In such a case , you must define your own converter.

All Custom Formatter Converters must and do implement the `IArrayRepresentation<T>` interface , 
where `T` the subject object type that it can be serialized/deserialized. Note: Not all objects are possible to be serialized.

The `IArrayRepresentation<T>` is the base interface of declaring a plain converter.

The interface will be later explained in the 'Advanced Usage' of this Custom Formatter.

Presuming that you create a game and you want to serialize multple dog entities to save them in resources to use them later
(Or to be your base objects and change their behavior at run-time).

Let's presume this , imaginary , but , simple `Dog` class:

~~~C#
class Dog
{
	// Default behaviors
	public System.Boolean IsSitting , IsBarking , IsSleeping;

	// The color of it's fur (or hair).
	public System.Int32 FurColor;

	// More dog attributes?
	public System.Int32 Attributes;
}
~~~

Ok. Let's now define a converter class for it:
~~~C#
// At the top of your source file...
using DotNetResourcesExtensions.Internal.CustomFormatter;

class DogConverter : IArrayRepresentation<Dog>
{
	// Property to retrieve the correct runtime-type of Dog class. 
	public System.Type OriginalType => typeof(Dog);

	public System.Converter<Dog , System.Byte[]> GetTransformMethod()
	{
		System.Byte[] Method(Dog data) // This is the actual converter. It's handle is returned, and then is invoked from the custom formatter when required.
		{
			// We have an instance of the Dog class here.
			// This is a sample serializer. You may change the order
			// which the fields of the Dog class are serialized.
			// Just make sure to update the same order to the deserializer too.
			System.Byte[] result = new System.Byte[11]; // You must compute the exact size of the serialized fields that you will save.
			// For booleans , it is best to allocate one byte with 1-0 values to save them.
			// You can even not include all class fields!
			result[0] = data.IsSitting ? 1 : 0; // To indicate the boolean value.
			result[1] = data.IsBarking ? 1 : 0;
			result[2] = data.IsSleeping ? 1 : 0;
			System.Byte[] temp = System.BitConverter.GetBytes(data.FurColor); // To dec/encode numeric fields , use the BitConverter class.
			// Int32 is a 4 byte signed integer so we copy 4 bytes.
			System.Array.ConstrainedCopy(temp , 0 , result , 3 , 4);
			//    The encoded FurColor   --^ 
	//Index of the first element in the 'temp' --^
			//				Copy to the 'result' array --^
			//                Index to start copying into  --^
			//      Bytes to copy. Also known as the 'length'. --^
			temp = System.BitConverter.GetBytes(data.Attributes);
			System.Array.ConstrainedCopy(temp , 0 , result , 7 , 3); // Again the same , just properly updated to save the Attributes field.
			return result;
		}
		return Method; // The method handle is returned (Or the 'method token').
	}

	public System.Converter<System.Byte[], T> GetUntransformMethod()
	{
		Dog Method(System.Byte[] data) // This is the deserializer of the Dog class. It will enable us to retrieve the original object.
		{
			Dog ret = new(); // Initialise it with default values . Will be supplied though field assignments.
			ret.IsSitting = data[0] == 1; // When data[0] is 1 will give true , so it is serialized correctly.
			ret.IsBarking = data[1] == 1;
			ret.IsSleeping = data[2] == 1;
			// Decode the FurColor field
			ret.FurColor = System.BitConverter.ToInt32(data , 3);
			// Decode the Attributes field
			ret.Attributes = System.BitConverter.ToInt32(data , 7);
			return ret;
		}
		return Method;
	}
}
~~~

We have now the `Dog` object and it's equivalent(let's say) `DogConverter` converter class. 

Let's define the custom converter in the `ExtensibleFormatter` and serialize it:
~~~C#
// At the top of your source file...
using DotNetResourcesExtensions.Internal.CustomFormatter;

// Create the default ExtensibleFormatter:
ExtensibleFormatter EF = new();
// Register our custom converter through the ConstructTypeResolver method...
// Note: The ConstructTypeResolver() creates an implicit type resolver and creating many of them for each class can impact the search performance.
EF.RegisterTypeResolver(new DogConverter().ConstructTypeResolver());
// Serialize a default Dog object...
Dog anyobj = new();
System.Byte[] data = EF.GetBytes(anyobj);
// Do something with the serialized data...
~~~

So , the call method remains the same , but:

- Under normal conditions and if we had not included our custom converter , the class would have not been serialized because the converter for it would not exist.
- There are more complex situations which you may need more classes to serialize , so this simple example cannot cover such cases.
(Just adding too many implicit type resolvers can deteriorate performance dramatically).

Same stuff apply if you were to deserialize an object from bytes. 

For our last example the code would be:
~~~C#
System.Byte[] data ; // Contains the serialized data
Dog inst = EF.GetObject<Dog>(data);
// Do something with the deserialized instance...
~~~


[Back to Index](https://github.com/mdcdi1315/dotnettesourcesextensions/blob/master/Docs/Main.md)