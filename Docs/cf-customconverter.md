## Create a custom converter for a class that the default type resolver does not contain

__BE CAREFUL__:
Creating and using a custom converter can lead to improper 
resource deserialization and memory leak. Use with caution and observe
your code paths before you test it in the real world. 
I make no guarantees for other custom types defined outside the `DotNetResourcesExtensions` project.

There are some cases where you need to serialize a class that is not included with
this project. For that reason , the CustomFormatter provides the `IArrayRepresentation`
interface , an abstract interface to create and use converters.
The interface layout is:
~~~C#
    public interface IArrayRepresentation<T> where T : notnull
    {
        public System.Converter<System.Byte[], T> GetUntransformMethod();

        public System.Converter<T, System.Byte[]> GetTransformMethod();

        public System.Type OriginalType { get; }
    }
~~~

- The generic type `T` is the subject type you want to de/serialize.
- `GetTransformMethod()` provides a `System.Converter` delegate for serializing the object.
When you are ready to serialize , you invoke the returned converter delegate.
- `GetUntransformMethod()` provides a `System.Converter` delegate for deserializing and getting the original object.
 When you are ready to deserialize , you invoke the returned converter delegate.
- The `OriginalType` property defines the actual serialized object type. It is common to define
it's get accessor as: `get => typeof(T);` , which will give you the subject `System.Type` instance.

It is useful to mention here that you are never going to use plainly an instance of
this interface; The CustomFormatter part defines a large infrastracture mechanism in order
to use this interface and call de/serialization methods.

__NOTE__: Observe the constraint on `T` type parameter. You cannot use this type to represent objects such as
`int?` , `bool?` , or any type that uses the `System.Nullable<T>` class.

### Creating a class from the `IArrayRepresentation` interface:

Create a C# source file with these contents:

~~~C#
using DotNetResourcesExtensions.Internal.CustomFormatter;

public struct Data
{
    public System.Int32 Position;
    public System.Int32 Length;
}

public sealed class DataRepresentation : IArrayRepresentation<Data>
{
    public System.Converter<System.Byte[], Data> GetUntransformMethod();

    public System.Converter<Data, System.Byte[]> GetTransformMethod();

    public System.Type OriginalType { get => typeof(Data); }
}
~~~

Until so far you have created the class that we will work on , but there are still unimplemented the transform methods.

The `Data` structure that this source file gives is just an example; You can of course serialize any type , 
if it is possible to be serialized.

Let's first create the serialization method.
To create it , we will use the BitConverter class that gives us access to the bytes of
primitive types , such as the `System.Int32` , which the `Data` structure uses.
Then , we will copy the two numbers into one array , and which it will be returned back.
~~~C#
public System.Converter<Data, System.Byte[]> GetTransformMethod();
{
    System.Byte[] Method(Data dt)
    {
        // We have 2 int's to encode , each one has a size of 4 bytes , so 8 bytes.
        System.Byte[] final = new System.Byte[8];
        System.Byte[] temp = System.BitConverter.GetBytes(dt.Position);
        // copy the bytes to the final array
        System.Array.ConstrainedCopy(temp , 0 , final , 0 , 4);
        // Compute Length value to bytes.
        temp = System.BitConverter.GetBytes(dt.Length);
        // copy the bytes to the final array
        System.Array.ConstrainedCopy(temp , 0 , final , 4 , 4);
        // Return the resulting array
        return final;
    }
    return Method; // This will return the above method delegate.
}
~~~
For deserialization you could use this sample:
~~~C#
public System.Converter<System.Byte[], Data> GetUntransformMethod()
{
    Data Method(System.Byte[] bytes)
    {
        Data original = new();
        original.Position = System.BitConverter.ToInt32(bytes , 0);
        original.Length = System.BitConverter.ToInt32(bytes , 4);
        return original;
    }
    return Method;
}
~~~

Now you have a new class that can de/serialize the Data structure.

The trick here is to remember that you are flexible on how the data will be serialized;
you can do whatever you want. Additionally , remember that you return a method and not a
serialization result.

[Back To Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)