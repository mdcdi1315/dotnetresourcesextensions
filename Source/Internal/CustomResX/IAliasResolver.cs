// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace DotNetResourcesExtensions.Internal.ResX;

internal interface IAliasResolver
{
    AssemblyName? ResolveAlias(string alias);
    void PushAlias(string? alias, AssemblyName name);
}
