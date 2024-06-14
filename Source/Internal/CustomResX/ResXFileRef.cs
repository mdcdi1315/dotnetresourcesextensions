// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace DotNetResourcesExtensions.Internal.ResX;

/// <summary>
///  ResX File Reference class. This allows the developer to represent
///  a link to an external resource. When the resource manager asks
///  for the value of the resource item, the external resource is loaded.
/// </summary>
[TypeConverter(typeof(Converter))]
public partial class ResXFileRef
{
    /// <summary>
    ///  Creates a new ResXFileRef that points to the specified file.
    ///  The type refered to by typeName must support a constructor
    ///  that accepts a System.IO.Stream as a parameter.
    /// </summary>
    public ResXFileRef(string fileName, string typeName)
    {
        if (fileName == null) { throw new ArgumentNullException(nameof(fileName)); }
        if (typeName == null) { throw new ArgumentNullException(nameof(typeName)); }
        FileName = fileName;
        TypeName = typeName;
    }

    /// <summary>
    ///  Creates a new ResXFileRef that points to the specified file.
    ///  The type refered to by typeName must support a constructor
    ///  that accepts a System.IO.Stream as a parameter.
    /// </summary>
    public ResXFileRef(string fileName, string typeName, Encoding textFileEncoding) : this(fileName, typeName)
    {
        TextFileEncoding = textFileEncoding;
    }

    internal ResXFileRef Clone()
        => TextFileEncoding is null ? new(FileName, TypeName) : new(FileName, TypeName, TextFileEncoding);

    /// <summary>
    /// The file name of this file reference.
    /// </summary>
    public string FileName { get; private set; }

    /// <summary>
    /// The type name that this reference is associated with.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Gets the encoding specified in the current <see cref="ResXFileRef"/> constructor.
    /// </summary>
    public Encoding? TextFileEncoding { get; }

    /// <summary>
    ///  path1+result = path2
    ///  A string which is the relative path difference between path1 and
    ///  path2 such that if path1 and the calculated difference are used
    ///  as arguments to Combine(), path2 is returned
    /// </summary>
    private static string PathDifference(string path1, string path2, bool compareCase)
    {
        int i;
        int si = -1;

        for (i = 0; (i < path1.Length) && (i < path2.Length); ++i)
        {
            if ((path1[i] != path2[i])
                && (compareCase || (char.ToLower(path1[i], CultureInfo.InvariantCulture) != char.ToLower(path2[i], CultureInfo.InvariantCulture))))
            {
                break;
            }

            if (path1[i] == Path.DirectorySeparatorChar)
            {
                si = i;
            }
        }

        if (i == 0)
        {
            return path2;
        }

        if ((i == path1.Length) && (i == path2.Length))
        {
            return string.Empty;
        }

        StringBuilder relPath = new StringBuilder();

        for (; i < path1.Length; ++i)
        {
            if (path1[i] == Path.DirectorySeparatorChar)
            {
                relPath.Append($"..{Path.DirectorySeparatorChar}");
            }
        }

        relPath.Append(path2.AsSpan(si + 1).ToString());

        return relPath.ToString();
    }

    internal void MakeFilePathRelative(string basePath)
    {
        if (string.IsNullOrEmpty(basePath))
        {
            return;
        }

        FileName = PathDifference(basePath, FileName, false);
    }

    /// <summary>
    /// Returns the file reference that is constructed.
    /// </summary>
    public override string ToString()
    {
        string result;

        if (FileName.Contains(";") || FileName.Contains("\""))
        {
            result = $"\"{FileName}\";{TypeName}";
        }
        else
        {
            result = $"{FileName};{TypeName}";
        }

        if (TextFileEncoding is not null)
        {
            result += $";{TextFileEncoding.WebName}";
        }

        return result;
    }
}
