## How the custom formatting works?

This is simple enough.

To do all the serialization required , two diverse interfaces are needed to pass information from the one to the another.

Specifically this happens when we have an object to serialize:

~~~mermaid
---
title: GetBytes flowchart sample
---
flowchart TD

OA[/Object to serialize/] --> AGH
OA --> OB
OA --> |Object instance| AE
OB[Get Object Type] --> |Object's System.Type instance|AF
OB --> AGH
AD[ICustomFormatter interface] --> AE
AE[GetBytes Method] --> AF
AF[Find Converter Through ITypeResolver] --> TR2 
TR1[ITypeResolver Interface] --> TR3
TR2{Converter exists for this type of object?} --> |Yes|TR1
TR3[Call GetConverter Method] --> AGT
TG1{Found any through registered TypeNotFoundDelegates?} --> |No|TG2
TG2[Throw exception]
TG3[Get converter instance through reflection] 
TG4[Throw AggregateException]
TG1 --> |Yes|TG3
TG1 --> |Exception occured in a delegate call|TG4
TG3 --> AGT
TR2 --> |No|TG1
AGT[Use GetTransformMethod]  --> AGH
AGH[Pass required data] --> AG
AG[Invoke the conversion method] --> AT
AT[/Return the serialized bytes/]
~~~

This is and the case of how the formatter actually works.

The same about happen when a byte array is converted back to an object.

The ICustomFormatter uses the ITypeResolver interface to retrieve
the converter instance.

Through the object's underlying System.Type , the type resolver finds if
the the type got exists in it's `RegisteredQualifiedTypeNames` property.

If yes, it retrieves the converter instance and performs a call to serialize/deserialize the object instance.

If not , and all registered type resolvers were searched , and all TypeNotFoundEventHandlers have not given a result , it throws an exception.

Generally the custom formatter works in a very try approach so as to give to the user the option to
have methods to provide (At least late-bound) a converter instance.

And be noted , the Custom Formatter only serializes the fields that it needs so as to barely represent a minimal representation of the object.

Unlike the `BinaryFormatter` , which it used to create an object graph of all the contained objects , this does not store the object exactly. 

So , the original object might be inequal with the converted one , so be careful and keep it in mind when performing equality checks with unserialized and serialized objects.

[Back to Index](https://github.com/mdcdi1315/dotnettesourcesextensions/blob/master/Docs/Main.md)