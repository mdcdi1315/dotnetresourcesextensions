// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace DotNetResourcesExtensions.Internal.ResX;

/// <summary>
///  ResX Null Reference class.  This class allows ResX to store null values.
///  It is a placeholder that is written into the file.  On read, it is replaced
///  with null.
/// </summary>
internal sealed class ResXNullRef : IFileReference
{
    public string FileName => "";

    public Type SavingType => null;

    public FileReferenceEncoding Encoding => FileReferenceEncoding.Binary;
}
