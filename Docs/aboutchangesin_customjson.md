## Changes performed in Version 3 of the `Custom JSON` format.

This document describes the changes among all the resource versions of the Custom JSON format , 
and describes their strong and weak points. Additionally it mentions why using the V3 format is better than the old ones.

As always , the bundled JSON reader can read all 3 versions abruptly.

The V3 format was implemented because:

- V2 had issues regarding the read performance of byte arrays which it was very much slow.
Although that the 1.0.5 version partially fixed the issue , there was still room for improvement.
- The format was partially understandable , both V1 and V2 were not understanable on some points.
- It tackles the issues produced in V2 around the byte arrays read perf and V1 for it's bad syntactic of resource definitions.
- It implements and supports the incoming DotNetResourcesExtensions `IResourceEntryWithComment` interface.
- The support for file references was required.

A typical V2 JSON resource file would be expected as:
~~~JSON
{
  "MDCDI1315-JSON-RESOURCE-TABLE": {
    "Version": 1,
    "Magic": "mdcdi1315.JRT.RESOURCE.NET",
    "SupportedFormatsMask": 2,
    "CurrentHeaderVersion": 2
  },
  "Data": [
    {
      "HeaderVersion": 1,
      "ResourceName": "eDMFT",
      "ResourceType": 0,
      "Value[0]": "eDFR"
    },
    {
      "HeaderVersion": 2,
      "ResourceName": "edt",
      "ResourceType": 2,
      "TotalLength": 6874,
      "Base64Alignment": 322,
      "DotnetType": "System.Drawing.Bitmap, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
      "Chunks": 29,
      "Value[1]": "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAGnxJREFUeF6dWnd8VlW2zc\u002BGFGkJJYWWQArpoSUhQCChlxQMEHoRpIMKCCiIIDK2wRlGBRwFUUSa0psVREQQsKLYqKHZZpyZN2/elP3W2vee\u002B52vEHzvj/W79/ty7z1nrb32PvvcL2FVG9QXfzQIOLrnDYNRzYX/9w2lWmRliPQhqjJEKapH40hY5zwGISbagd/3vs/mPn2WAcaBAJi0DZLwPlvkzdGCEi",
      (more data here...)
      "Value[29]": "ILwfLl2CHqxrvHvNeC6MxY3NtcDbxB3S3n\u002B9gGdY9ahIUVAE1w2\u002BtOCFtgg\u002BIZzVw4VR2Q\u002BuQDqpSoCJO\u002BdGuABSJGp99l3vg3GpY21D1oH\u002BV4uStshrgIkY\u002BV/sYvXoNCklBQAAAABJRU5ErkJggg=="
    }
  ]
}
~~~

Which it does have the following flaws:

- All resource types were interpreted as numbers without understanding what do these mean
- Typical resource values were as 'Value[0]' - bad property name for resource value.
- All byte array chunks were written in seperate properties , slowing down the access to the byte array.
- Even worse in V1 , the entire byte array was written in a single property  , making slow all text readers meant to read the format data.

Now let's see the respective V3 format (Be noted that the exact same resources have been encoded as the above):
~~~JSON
{
  "MDCDI1315-JSON-RESOURCE-TABLE": {
    "Version": 1,
    "Magic": "mdcdi1315.JRT.RESOURCE.NET",
    "SupportedFormatsMask": 3,
    "CurrentHeaderVersion": 3
  },
  "Data": [
    {
      "HeaderVersion": 3,
      "ResourceName": "eDMFT",
      "ResourceType": "String",
      "Value": "eDFR"
    },
    {
      "HeaderVersion": 3,
      "ResourceName": "Default",
      "ResourceType": "Object",
      "TotalLength": 6874,
      "Base64Alignment": 355,
      "DotnetType": "System.Drawing.Bitmap, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
      "Chunks": 26,
      "Value": [
        "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAGnxJREFUeF6dWnd8VlW2zc\u002BGFGkJJYWWQArpoSUhQCChlxQMEHoRpIMKCCiIIDK2wRlGBRwFUUSa0psVREQQsKLYqKHZZpyZN2/elP3W2vee\u002B52vEHzvj/W79/ty7z1nrb32PvvcL2FVG9QXfzQIOLrnDYNRzYX/9w2lWmRliPQhqjJEKapH40hY5zwGISbagd/3vs/mPn2WAcaBAJi0DZLwPlvkzdGCEiJhhfs5CCBjjkEkHdgTDJyk\u002Bc6BS8gj6x4",
        (more data here...)
        "KoUfkju2MOEcQTZfmVJwFhkdXmHgI4wpkHMN64ojkD67JjotcNwEaVaaYHiEqrvHuIVmPMMe0SbvELasz2krcdTp/BGbAvX\u002BUVBFijBtcIQC/H04BFULhOsMVhOI46UI4whjoxAJgf\u002BeRILwfLl2CHqxrvHvNeC6MxY3NtcDbxB3S3n\u002B9gGdY9ahIUVAE1w2\u002BtOCFtgg\u002BIZzVw4VR2Q\u002BuQDqpSoCJO\u002BdGuABSJGp99l3vg3GpY21D1oH\u002BV4uStshrgIkY\u002BV/sYvXoNCklBQAAAABJRU5ErkJggg=="
      ]
    }
  ]
}
~~~
Some notable differences in V3 Format:

- All resource value properties are named as 'Value' , regardless of the resource type being read.
- All resources can have at any time comments , even if those were not written initially.
To define one write a property named 'Comment' and your comment string inside at any resource definition.
- The resource value property is adaptable - which means that for string resources it is a string property , while for bytearray-based resources it is a base-64 array.
- Using a base-64 character array than defining the chunks in seperate properties boosts performance by a lot.
- All resource types are written and read by default as a piece of string that defines the resource.

Analytically , these values do apply for the 'ResourceType' property:

`String` , for any resource that holds a string resource.

`ByteArray` , for any resource that holds a plain byte array.

`Object` , for any resource that holds any serialized object and then converted to a plain byte array.

`FileReference` , for any resource that holds a file reference. It is encoded as a string array with 3 elements.

You may also define the resource type as a numeric value , just like it happened in V2 and V1 formats. The `String` type can be also said that is the type 0 , type 1 means the `ByteArray` type , and so on.

__Performance Notes__

The performance in reading byte arrays using JSON and XML is almost the same anymore , 
although it can be stated that XML still wins because the entire chunks are written as one string with line breaks
and it is more code-performant , while accessing a JSON array might be significantly performance-costing.

__Other Notes__

Any users that still use the V2 format is recommended to move to the V3 format immediately due to the +500% better performance and understandability.

For those though who are still using the V2 the read performance has been improved by +300% but it is still adequately slow.
And again please migrate to V3 as soon as possible.


[Back to Index](https://github.com/mdcdi1315/dotnetresourcesextensions/blob/master/Docs/Main.md)
