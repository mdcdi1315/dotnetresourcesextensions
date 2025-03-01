// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace DotNetResourcesExtensions.Internal.AssemblyReader
{
    internal readonly struct DirectoryEntry
    {
        public readonly int RelativeVirtualAddress;
        public readonly int Size;

        public DirectoryEntry(int relativeVirtualAddress, int size)
        {
            RelativeVirtualAddress = relativeVirtualAddress;
            Size = size;
        }

        internal DirectoryEntry(ref PEBinaryReader reader)
        {
            RelativeVirtualAddress = reader.ReadInt32();
            Size = reader.ReadInt32();
        }
    }
}
