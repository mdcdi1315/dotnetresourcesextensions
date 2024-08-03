## The Custom Formatter of `DotNetResourcesExtensions` Project

What is it?

This custom formatter is used so as to define an alternative to the 
(deprecated now) very-known `System.Runtime.Serialization.Formatters.Binary.BinaryFormatter`.
Because the original ResX implementation which the Custom ResX has been created from had that dependency ,
it had to be removed and instead a new formatter take it's place.

The result of it is the `DotNetResourcesExtensions.Internal.CustomFormatter` namespace.

To ensure that any derivable formatters do not expose information to the web (and thus be vulnerable to hack attacks) it does:
- Not expose any formatted data outside the process (at least not explicitly)
- It is only meant to be used for resources classes
- It does only serialize the Public API of the class object meant to be serialized
- Does not get or has any knowledge of how serializing an object. You must implement a converter for that.


Apart from these , it provides a default , abstract formatter (`BaseFormatter`) and a ready-to-use
formatter (`ExtensibleFormatter`) which is currently used in some resource classes.

[Back to Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)

