﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;

namespace DotNetResourcesExtensions.Internal.ResX;

/// <summary>
///  ResX resource set.
/// </summary>
public class ResXResourceSet : System.Resources.ResourceSet
{
    /// <summary>
    ///  Creates a resource set for the specified file.
    /// </summary>
    public ResXResourceSet(string fileName) : base(new ResXResourceReader(fileName))
    {
    }

    /// <summary>
    ///  Creates a resource set for the specified stream.
    /// </summary>
    public ResXResourceSet(Stream stream) : base(new ResXResourceReader(stream))
    {
    }

    /// <summary>
    ///  Gets the default reader type associated with this set.
    /// </summary>
    public override Type GetDefaultReader() => typeof(ResXResourceReader);

    /// <summary>
    ///  Gets the default writer type associated with this set.
    /// </summary>
    public override Type GetDefaultWriter() => typeof(ResXResourceWriter);
}
