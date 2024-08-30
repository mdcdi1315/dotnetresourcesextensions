## Changes performed in the `Custom XML` format.

The reason of this being documented is to define and show the
differences of V1 and V2 format versions , and to highlight the strong
aspects of the new format.

The V2 format was created for a multiple of reasons , some of them are:

- The format to be closer in syntactic form as the Microsoft ResX format.
- The format to look like just as the upcoming Localizable XML Format.
- Faster data write & access.
- Allow file references to be written and read. By the time of this being written , 
only the Human Readable Format supported such references.

Specifically , the V1 format was a bit more complex and required more steps in order
to be understood. 
An example V1 format is this:
~~~XML
﻿<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<MDCDI1315-RES>
	<MDCDI1315-XML-RESOURCE-TABLE>
		<Version>1</Version>
		<Magic>mdcdi1315.XML.RESOURCE.NET</Magic>
		<SupportedFormatsMask>2</SupportedFormatsMask>
		<CurrentHeaderVersion>1</CurrentHeaderVersion>
	</MDCDI1315-XML-RESOURCE-TABLE>
	<ResData>
		<EDTG>
			<HeaderVersion>1</HeaderVersion>
			<ResourceType>0</ResourceType>
			<Value_0>EDFGPG</Value_0>
		</EDTG>
		<Pack>
			<HeaderVersion>1</HeaderVersion>
			<ResourceType>0</ResourceType>
			<Value_0>34</Value_0>
		</Pack>
		<EPDL>
			<HeaderVersion>1</HeaderVersion>
			<ResourceType>0</ResourceType>
			<Value_0>EPD</Value_0>
		</EPDL>
	</ResData>
</MDCDI1315-RES>
~~~

This design had some flaws:
- The resource name was saved in the element name. This directly excludes some important allowed resource names.
- The header version was a useless field since no structural design changes could be performed.
- The resource type could be an attribute 
- The `Value_0` is a very typed name for the resource value.
- All the above add 30%+ size to the target.
- All base64 data were encoded in a single line (not viewable in the example , but was true) , and were not formatted with line breaks.

Instead , a better and much more simple design was created:
~~~XML
﻿<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<MDCDI1315-RES>
	<MDCDI1315-XML-RESOURCE-TABLE>
		<Version>2</Version>
		<Magic>mdcdi1315.XML.RESOURCE.NET</Magic>
		<SupportedFormatsMask>3</SupportedFormatsMask>
		<CurrentHeaderVersion>1</CurrentHeaderVersion>
	</MDCDI1315-XML-RESOURCE-TABLE>
	<ResData>
		<Data name="EDTG" type="0">
			<Value>EDFGPG</Value>
		</Data>
		<Data name="Pack" type="0">
			<Value>34</Value>
		</Data>
		<Data name="EPDL" type="0">
			<Value>EPD</Value>
		</Data>
	</ResData>
</MDCDI1315-RES>
~~~

As you can see , both designs have serialized exactly the same resources.

The new format defines the resource name and type as attributes , 
all resources have as a base element the same name (which is `Data`) ,
the size is much more smaller than the V1 one , and all resources have the
element `Value` which designates the resource value.

In this way , the format is done more human-friendly and people can understand 
and write in-hand more string resources.

Additionally , in the same way that strings are written , file references are also written because they are just serialized strings
returned from `IFileReferenceExtensions.ToSerializedString()` method.

So , even file references can be written in-hand too.

Additionally , all encoded byte-array data are written in base-64 chunks , just like it does already happen in
`JSON` and `Human Readable Format` readers and writers.

Note that the `CurrentHeaderVersion` element in header is deprecated but is retained in the header for V1 compatibility reasons.

Be noted that the `XMLResourcesReader` can still read the old V1 format so as to give a chance to migrate to the new V2 version of the format.

__Performance Notes__

I noticed out at some time that the `Custom JSON` reader cannot read so fast a large byte array resource due to the fact that 
I cannot write a single base64 chunk without writing a new string element.

However XML allows such case and this is what is implemented currently in
the reader. As it seems , the `Custom XML` has currently better performance than
`Custom JSON` in reading large byte arrays.

[Back to Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)