## Format documentation for the custom binary resource format defined in `DotNetResourcesExtensions` project

__Abstract:__

This resource format intends to be simple , full-descriptive and intuitive for storing and reading a `.NET Resource`.

A `.NET Resource` is any object resource retrieved at run-time in order to be consumed by a `.NET application`.

A `.NET Application` is the agent that executes code and retrieves these `.NET Resources` , when it requests them.

The format has the ability to store , index and retrieve a multiple of `.NET Resources` , which together 
they form the `Custom Binary Resource Stream` , which it will be described later.

In order a `.NET Resource` to be retrieved as-it-is when the `.NET application` requests it , 
the format saves some additional data which finally , when the format is read , the object is 
retrieved as it was when the format was written to any `data target`.

A `data target` specify any valid data structure that is able to store data , including files in a filesystem.

In order to save multiple `.NET Resources` in a `data target` , the format also defines a header for specifying the resources to read.

__Overview of a `.NET Resource`:__

Typically , a `.NET Resource` has a unique name which identifies it inside the `data target` and
a value that is a very abstract object which contains the data to retrieve when the specified 
resource is requested;

As of `.NET` , the resource name is a data structure represented by the `System.String` type , 
which is saving alphanumeric characters; while the resource value is any object deriving from the
`System.Object` hierarchy class.

__Overview of the `Custom Binary Resource Stream`:__

The `Custom Binary Resource Stream` defines a convenient format for saving any number of `.NET Resources` required in
a `data target`; It was written by `mdcdi1315` in 2024 for a try to define new resource acquire methods;
You can contact to him by mailing to `mds13363la@gmail.com` for more information.

_NOTE:_ The format is written on top of `.NET Runtime` environment , and thus , some points of the
format require knowledge of it; For more information you can go to the `.NET Runtime Official Repository` located at [https://github.com/dotnet/runtime](https://github.com/dotnet/runtime).

_NOTE:(2)_ The following format is only meant to be used in a project and any `.NET Applications` that use it called [`DotNetResourcesExtensions`](https://github.com/mdcdi1315/dotnetresourcesextensions);
However , the project is open-source and can be copied under the terms of the [`MIT Liscense`](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/LICENSE.txt).

__Format Versioning__

 V1 -> Original Format Version.  Deprecated due to an format error aroused during read.

 V2 -> Current Format Version , stable (Current issue of this formal document).

__Document Changes__

12 / 7 / 2024 , 7:05 PM +03:00 Standard European East Time : Original first issue of this document. V2 format is described.


__Resource Format Description__

An overview of this resource format header would be:
~~~
Hex Binary Representation:
4D 44 43 44 49 31 33 31 35 5F 42 46 5F 31 07 0A 56 45 52 53 49 4F 4E 3D 00 00 00 00 00 00 00 00 07 0A 43 55 52 52 45 4E 54 46 4F 52 4D 41 54 53 3D 00 00 00 00 00 00 00 00 07 0A 53 55 50 50 4F 52 54 45 44 48 45 41 44 45 52 56 45 52 53 49 4F 4E 3D 00 00 00 00 00 00 00 00 07 0A 44 41 54 41 50 4F 53 41 4C 49 47 4E 4D 45 4E 54 3D 00 00 00 00 00 00 00 00 07 0A 44 41 54 41 50 4F 53 49 54 49 4F 4E 53 43 4F 55 4E 54 3D 00 00 00 00 00 00 00 00 07 0A 
 |-------------------------------------|   |--| |------------------| |  |---------------------|  |--| |--------------------------------------|  |  |---------------------| |--|  |---------------------------------------------------------------| |  |---------------------| |--|  |---------------------------------------------| |  |---------------------| |--|  |--------------------------------------------------|  |  |---------------------| |--|
		|                            |             |         |             |               |                       |                    |              |             |                                  |                                  |             |             |                         |                          |             |             |                   |                                      |             |              |
		1                            2             3         4             5               2                       6                    4              7             2                                  8                                  4             9             2                         10                         4             11            2                   12                                     4            13              2
~~~

`1`: Defines the stream's magic value. It's current value in UTF-8 would be as `MDCDI1315_BF_1`.

`2`: Specifies the header or any valued-pair header end.

`3`: Specifies the `VERSION` header.

`4`: Represents the equal sign (=) and after it is specified any header value.

`5`: The version field is a signed 64-bit integer converted to bytes. 
All numeric fields of this format are described with this way.

Note that inside the actual generated file the number is not zero; but the actual value supplied.

The numeric values are just zeros to indicate that there in can be any number that the implementation requires.

As of V2 this is defined as : `02 00 00 00 00 00 00 00`.

`6`: Specifies the `CURRENTFORMATS` header , which is the resource formats mask , so the resource formats that are supported
for this file. This allows any implementing reader to detect corruption issues and verify correct resource read.

`7`: The formats mask field is a signed 64-bit integer that represents the number correspondent to
the current resource types enumeration.

As of V2 this is defined as : `02 00 00 00 00 00 00 00`. 

`8`: The `SUPPORTEDHEADERVERSION` specifies the resource version that the reader must be able to read.

It specifies up to which resource header version any resource can be read. For example , you have a V2 header but you want the reader to
read only V1 resource formats.

Be noted that the resource store format and the resource stream header have different versioning; This happens
because the versions are only subject to be changed at when a breaking change must happen; and to not fully break 
previous version support this allows readers that can only read headers of older versions to read resources written with 
a newer version if such readers have the knowledge to do that of course.

`9`: The supported resource store format version is any numeric version that corresponds
to a existing resource store format version.

`10`: The `DATAPOSALIGNMENT` header specifies the alignment of data positions inside the `DATAPOSITIONS` header.
It is the size of saved integers inside that array; which means that i.e. for the 4-byte signed integer this header should hold the number 4.

`11`: The actual data position alignment that is the numeric structure size , as it was elaborated in comment `10`.

As of V2 this is defined as: `08 00 00 00 00 00 00 00`.

`12`: The `DATAPOSITIONSCOUNT` header specifies the count of current written resources inside this resource file.
It is also used by the readers together with `DATAPOSALIGNMENT` to determine the byte array length in `DATAPOSITIONS` header.

The exact mathematic formula of computing the byte array length is: `DATAPOSALIGNMENT * DATAPOSITIONSCOUNT` where the `DATAPOSALIGNMENT` must be a multiple of 2.

Note that the format currently cannot support numeric types under 2-byte numeric types due to the formula requirement.

`13`: Specifies the resource count that this resource stream holds.


