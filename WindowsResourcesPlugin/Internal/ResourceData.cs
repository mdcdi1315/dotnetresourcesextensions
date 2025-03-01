// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using DotNetResourcesExtensions.Internal.AssemblyReader;

namespace DotNetResourcesExtensions.Win32Resources
{
    /// <summary>
    /// Resource abstraction to allow examination
    /// of a PE file that contains resources.
    /// </summary>
    internal unsafe partial class ResourceData
    {
        /// <summary>
        /// Initialize a ResourceData instance from a PE file
        /// </summary>
        /// <param name="peFile"></param>
        public ResourceData(PEReader peFile)
        {
            DirectoryEntry resourceDirectory = peFile.PEHeaders.PEHeader!.ResourceTableDirectory;
            if (resourceDirectory.Size != 0)
            {
                BlobReader resourceDataBlob = peFile.GetSectionData(resourceDirectory.RelativeVirtualAddress).GetReader(0, resourceDirectory.Size);
                ReadResourceData(resourceDataBlob, peFile, null);
            }
        }

        /// <summary>
        /// Find a resource in the resource data
        /// </summary>
        public byte[] FindResource(string name, string type, ushort language)
        {
            return FindResourceInternal(name, type, language);
        }

        /// <summary>
        /// Find a resource in the resource data
        /// </summary>
        public byte[] FindResource(ushort name, string type, ushort language)
        {
            return FindResourceInternal(name, type, language);
        }

        /// <summary>
        /// Find a resource in the resource data
        /// </summary>
        public byte[] FindResource(string name, ushort type, ushort language)
        {
            return FindResourceInternal(name, type, language);
        }

        /// <summary>
        /// Find a resource in the resource data
        /// </summary>
        public byte[] FindResource(ushort name, ushort type, ushort language)
        {
            return FindResourceInternal(name, type, language);
        }

        /// <summary>
        /// Add or update resource
        /// </summary>
        public void AddResource(string name, string type, ushort language, byte[] data) => AddResourceInternal(name, type, language, data);

        /// <summary>
        /// Add or update resource
        /// </summary>
        public void AddResource(string name, ushort type, ushort language, byte[] data) => AddResourceInternal(name, type, language, data);

        /// <summary>
        /// Add or update resource
        /// </summary>
        public void AddResource(ushort name, string type, ushort language, byte[] data) => AddResourceInternal(name, type, language, data);

        /// <summary>
        /// Add or update resource
        /// </summary>
        public void AddResource(ushort name, ushort type, ushort language, byte[] data) => AddResourceInternal(name, type, language, data);

        public IEnumerable<(object name, object type, ushort language, byte[] data)> GetAllResources()
        {
            return _resTypeHeadID.SelectMany(typeIdPair => SelectResType(typeIdPair.Key, typeIdPair.Value))
                .Concat(_resTypeHeadName.SelectMany(typeNamePair => SelectResType(typeNamePair.Key, typeNamePair.Value)));

            IEnumerable<(object name, object type, ushort language, byte[] data)> SelectResType(object type, ResType resType)
            {
                return resType.NameHeadID.SelectMany(nameIdPair => SelectResName(type, nameIdPair.Key, nameIdPair.Value))
                    .Concat(resType.NameHeadName.SelectMany(nameNamePair =>
                        SelectResName(type, nameNamePair.Key, nameNamePair.Value)));
            }

            IEnumerable<(object name, object type, ushort language, byte[] data)> SelectResName(object type, object name, ResName resType)
            {
                return resType.Languages.Select((lang) => (name, type, lang.Key, lang.Value.DataEntry));
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (_resTypeHeadID.Count > 0)
                    return false;

                if (_resTypeHeadName.Count > 0)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Add all resources in the specified ResourceData struct.
        /// </summary>
        public void CopyResourcesFrom(ResourceData moduleResources)
        {
            foreach ((object name, object type, ushort language, byte[] data) in moduleResources.GetAllResources())
                AddResourceInternal(name, type, language, data);
        }

        public void WriteResources(int sectionBase, ref ObjectDataBuilder dataBuilder)
        {
            WriteResources(sectionBase, ref dataBuilder, ref dataBuilder);
        }

        public void WriteResources(int sectionBase, ref ObjectDataBuilder dataBuilder, ref ObjectDataBuilder contentBuilder)
        {
            Debug.Assert(dataBuilder.CountBytes == 0);

            SortedDictionary<string, List<ObjectDataBuilder.Reservation>> nameTable = new SortedDictionary<string, List<ObjectDataBuilder.Reservation>>();
            Dictionary<ResLanguage, int> dataEntryTable = new Dictionary<ResLanguage, int>();
            List<Tuple<ResType, ObjectDataBuilder.Reservation>> resTypes = new List<Tuple<ResType, ObjectDataBuilder.Reservation>>();
            List<Tuple<ResName, ObjectDataBuilder.Reservation>> resNames = new List<Tuple<ResName, ObjectDataBuilder.Reservation>>();
            List<Tuple<ResLanguage, ObjectDataBuilder.Reservation>> resLanguages = new List<Tuple<ResLanguage, ObjectDataBuilder.Reservation>>();

            IMAGE_RESOURCE_DIRECTORY.Write(ref dataBuilder, checked((ushort)_resTypeHeadName.Count), checked((ushort)_resTypeHeadID.Count));
            foreach (KeyValuePair<string, ResType> res in _resTypeHeadName)
            {
                resTypes.Add(new Tuple<ResType, ObjectDataBuilder.Reservation>(res.Value, IMAGE_RESOURCE_DIRECTORY_ENTRY.Write(ref dataBuilder, res.Key, nameTable)));
            }
            foreach (KeyValuePair<ushort, ResType> res in _resTypeHeadID)
            {
                resTypes.Add(new Tuple<ResType, ObjectDataBuilder.Reservation>(res.Value, IMAGE_RESOURCE_DIRECTORY_ENTRY.Write(ref dataBuilder, res.Key)));
            }

            foreach (Tuple<ResType, ObjectDataBuilder.Reservation> type in resTypes)
            {
                dataBuilder.EmitUInt(type.Item2, (uint)dataBuilder.CountBytes | 0x80000000);
                IMAGE_RESOURCE_DIRECTORY.Write(ref dataBuilder, checked((ushort)type.Item1.NameHeadName.Count), checked((ushort)type.Item1.NameHeadID.Count));

                foreach (KeyValuePair<string, ResName> res in type.Item1.NameHeadName)
                {
                    resNames.Add(new Tuple<ResName, ObjectDataBuilder.Reservation>(res.Value, IMAGE_RESOURCE_DIRECTORY_ENTRY.Write(ref dataBuilder, res.Key, nameTable)));
                }
                foreach (KeyValuePair<ushort, ResName> res in type.Item1.NameHeadID)
                {
                    resNames.Add(new Tuple<ResName, ObjectDataBuilder.Reservation>(res.Value, IMAGE_RESOURCE_DIRECTORY_ENTRY.Write(ref dataBuilder, res.Key)));
                }
            }

            foreach (Tuple<ResName, ObjectDataBuilder.Reservation> type in resNames)
            {
                dataBuilder.EmitUInt(type.Item2, (uint)dataBuilder.CountBytes | 0x80000000);
                IMAGE_RESOURCE_DIRECTORY.Write(ref dataBuilder, 0, checked((ushort)type.Item1.Languages.Count));
                foreach (KeyValuePair<ushort, ResLanguage> res in type.Item1.Languages)
                {
                    resLanguages.Add(new Tuple<ResLanguage, ObjectDataBuilder.Reservation>(res.Value, IMAGE_RESOURCE_DIRECTORY_ENTRY.Write(ref dataBuilder, res.Key)));
                }
            }

            // Emit name table
            dataBuilder.PadAlignment(2); // name table is 2 byte aligned
            foreach (KeyValuePair<string, List<ObjectDataBuilder.Reservation>> name in nameTable)
            {
                foreach (ObjectDataBuilder.Reservation reservation in name.Value)
                {
                    dataBuilder.EmitUInt(reservation, (uint)dataBuilder.CountBytes | 0x80000000);
                }

                dataBuilder.EmitUShort(checked((ushort)name.Key.Length));
                foreach (char c in name.Key)
                {
                    dataBuilder.EmitUShort((ushort)c);
                }
            }

            // Emit byte arrays of resource data, capture the offsets
            foreach (Tuple<ResLanguage, ObjectDataBuilder.Reservation> language in resLanguages)
            {
                contentBuilder.PadAlignment(4); // Data in resource files is 4 byte aligned
                dataEntryTable.Add(language.Item1, contentBuilder.CountBytes);
                contentBuilder.EmitBytes(language.Item1.DataEntry);
            }

            dataBuilder.PadAlignment(4); // resource data entries are 4 byte aligned
            foreach (Tuple<ResLanguage, ObjectDataBuilder.Reservation> language in resLanguages)
            {
                dataBuilder.EmitInt(language.Item2, dataBuilder.CountBytes);
                IMAGE_RESOURCE_DATA_ENTRY.Write(ref dataBuilder, sectionBase, dataEntryTable[language.Item1], language.Item1.DataEntry.Length);
            }
            dataBuilder.PadAlignment(4); // resource data entries are 4 byte aligned
        }
    }
}
