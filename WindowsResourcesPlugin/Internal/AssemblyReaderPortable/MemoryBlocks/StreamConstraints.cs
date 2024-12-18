// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace DotNetResourcesExtensions.Internal.AssemblyReader.MemoryBlocks
{
    internal readonly struct StreamConstraints
    {
        public readonly object? GuardOpt;
        public readonly long ImageStart;
        public readonly int ImageSize;

        public StreamConstraints(object? guardOpt, long startPosition, int imageSize)
        {
            GuardOpt = guardOpt;
            ImageStart = startPosition;
            ImageSize = imageSize;
        }
    }
}
