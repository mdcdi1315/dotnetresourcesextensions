using System;
using System.Collections;
using System.Collections.Generic;
using DotNetResourcesExtensions.Internal;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions.Collections
{
    /// <summary>
    /// Defines a temporary backing store where resources can be temporarily saved and retrieved for the application lifetime. <br />
    /// It is expected that after application shutdown , this cache must have been deleted.
    /// </summary>
    public interface IResourceCache : IResourceEntryEnumerable , IDisposable
    {
        /// <summary>
        /// Adds a new resource entry to be cached.
        /// </summary>
        /// <param name="entry">The resource entry to be cached.</param>
        public void Add(IResourceEntry entry);

        /// <summary>
        /// Adds a new resource to be cached with the specified <paramref name="Name"/> and <paramref name="Value"/>.
        /// </summary>
        /// <param name="Name">The resource name to add.</param>
        /// <param name="Value">The resource value to add.</param>
        public void Add(System.String Name , System.Object Value);

        /// <summary>
        /// Adds a new resource that has the specified name and value from the <typeparamref name="TA"/> type.
        /// </summary>
        /// <typeparam name="TA">The resource value type that the resource must be saved in.</typeparam>
        /// <param name="Name">The resource name to add.</param>
        /// <param name="Value">The resource value to add.</param>
        public void Add<TA>(System.String Name , TA Value) where TA : notnull;

        /// <summary>
        /// Removes the first occurence of the specified resource entry defined in <paramref name="entry"/>.
        /// </summary>
        /// <param name="entry">The resource entry to remove.</param>
        /// <returns><see langword="true"/> if at least one occurence of the specified resource entry was found and deleted; otherwise , <see langword="false"/>.</returns>
        public System.Boolean Remove(IResourceEntry entry);

        /// <summary>
        /// Removes the first occurence of the specified resource which has the specified name and value.
        /// </summary>
        /// <param name="Name">The resource name to search so as to be deleted.</param>
        /// <param name="Value">The resource value to search so as to be deleted.</param>
        /// <returns><see langword="true"/> if at least one occurence of the specified resource was found and deleted; otherwise , <see langword="false"/>.</returns>
        public System.Boolean Remove(System.String Name , System.Object Value);

        /// <summary>
        /// Removes the first occurence of the specified resource which has the specified name and value. <br />
        /// The implementers must also implement type equality checks on <paramref name="Value"/> parameter using the <typeparamref name="TR"/> type to check the resource value.
        /// </summary>
        /// <typeparam name="TR">The type of the resource value to additionally check for type equality.</typeparam>
        /// <param name="Name">The resource name to search so as to be deleted.</param>
        /// <param name="Value">The resource value to search so as to be deleted.</param>
        /// <returns><see langword="true"/> if at least one occurence of the specified resource was found and deleted; otherwise , <see langword="false"/>.</returns>
        public System.Boolean Remove<TR>(System.String Name , TR Value) where TR : notnull;

        /// <summary>
        /// Defines the number of cached resources that this <see cref="IResourceCache"/> holds.
        /// </summary>
        public System.UInt32 Count { get; }

        /// <summary>
        /// Updates all the cached items inside the cache , if possible. <br />
        /// NOTE: This is an optional method , which it means that you can safely define an empty method for this.
        /// </summary>
        public void Update();

        /// <summary>
        /// Optimizes all the cached items inside the cache , if possible. <br />
        /// Resource cache optimization is a process which the resource items that have not been used for a while 
        /// are deleted. <br />
        /// NOTE: This is an optional method , which it means that you can safely define an empty method for this.
        /// </summary>
        public void Optimize();

        /// <summary>
        /// Clears all the resource entries from the cache , and optionally define whether to update the cache for looking up any leftovers.
        /// </summary>
        /// <param name="update">When this parameter is set to <see langword="true"/> , then a more intuitive search is undergone to delete more caches. This is actually true for resource-intensive applications which need as more resources as possible.</param>
        public void Clear(System.Boolean update = false);

        /// <summary>
        /// Gets the resource entry which has as a name the name defined by the <paramref name="name"/> parameter.
        /// </summary>
        /// <param name="name">The resource name to find the resource for.</param>
        /// <returns>The resource entry defined by <paramref name="name"/> if found; otherwise it throws <see cref="ResourceNotFoundException"/>.</returns>
        /// <exception cref="ResourceNotFoundException">The resource name defined in <paramref name="name"/> was not found.</exception>
        public IResourceEntry this[System.String name] { get; }

        /// <summary>
        /// Gets the resource entry inside the internal pooled array in the specified index. If the index does not exist , it throws <see cref="IndexOutOfRangeException"/>.
        /// </summary>
        /// <param name="index">The index to retrieve the resource entry at the specified position.</param>
        /// <returns>The resource entry at <paramref name="index"/> if found; otherwise it throws <see cref="IndexOutOfRangeException"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">The <paramref name="index"/> parameter defined an invalid index or was outside the pooled array bounds.</exception>
        public IResourceEntry this[System.Int32 index] { get; }
    }

    /// <summary>
    /// Represents a resource cache that is using a temporary filesystem to work.
    /// Note: There is still work on this class and it might not work as expected.
    /// </summary>
    public sealed class FileBasedResourceCache : IResourceCache, IUsingCustomFormatter
    {
        private sealed class FileResCacheResEntry : IResourceEntry
        {
            private readonly System.String name;
            private readonly System.Object value;

            public FileResCacheResEntry(System.String name , System.Object value) { this.name = name; this.value = value; }

            public System.String Name => name;

            public Object Value => value;

            public System.Type TypeOfValue => value?.GetType();

            public DictionaryEntry AsEntry => new(name, value);
        }

        private sealed class ResourceOptimizedEntryEnumerator : IResourceEntryEnumerator , IEnumerator<IResourceEntry>
        {
            private FileBasedResourceCache rc;
            private FileResCacheResEntry ent;
            private System.String resdat;

            public ResourceOptimizedEntryEnumerator(FileBasedResourceCache rc) 
            { 
                this.rc = rc; 
                this.rc.indexreadpos = 0;
                this.rc.srreadpos = 0;
            }

            public IResourceEntry ResourceEntry => ent;

            public object Key => ent.Name;

            public object Value => ent.Value;

            public DictionaryEntry Entry => ent.AsEntry;

            public object Current => ent.AsEntry;

            public bool MoveNext() {
                resdat = rc.ReadIndexLine();
                if (resdat is not null) { ent = rc.ReadResource(resdat); }
                return resdat is not null;
            }

            public void Reset() { rc.indexreadpos = 0; rc.srreadpos = 0; }

            IResourceEntry IEnumerator<IResourceEntry>.Current => ent;

            public void Dispose() { ent = null; resdat = null; }
        }

        private System.IO.DirectoryInfo basedirectoryinfo , filesdir;
        private ExtensibleFormatter formatter;
        private System.IO.FileStream strings , index;
        private System.Text.Encoding encoding;
        private System.Int64 indexreadpos , srreadpos;
        private System.UInt32 count;

        /// <summary>
        /// Creates a new instance of <see cref="FileBasedResourceCache"/> class by using the specified base directory for saving caches. <br />
        /// NOTE: The class will use the directory if the directory given is existing. If any other data exist in it , they will be deleted after disposing the class.
        /// </summary>
        /// <param name="basedirectory">The base directory to save the temporary caches.</param>
        /// <exception cref="ArgumentNullException"><paramref name="basedirectory"/> was <see langword="null"/>.</exception>
        public FileBasedResourceCache(System.IO.DirectoryInfo basedirectory)
        {
            formatter = ExtensibleFormatter.Create();
            basedirectoryinfo = basedirectory;
            if (basedirectoryinfo is null) { throw new ArgumentNullException(nameof(basedirectory)); }
            if (basedirectoryinfo.Exists == false) { basedirectoryinfo.Create(); }
            strings = new(System.IO.Path.Combine(basedirectoryinfo.FullName , "strings.txt") , System.IO.FileMode.Create , System.IO.FileAccess.ReadWrite);
            index = new(System.IO.Path.Combine(basedirectoryinfo.FullName, "head"), System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite);
            filesdir = basedirectoryinfo.CreateSubdirectory("files");
            encoding = System.Text.Encoding.UTF8;
            count = 0; indexreadpos = srreadpos = 0;
        }

        /// <summary>
        /// Creates a new instance of <see cref="FileBasedResourceCache"/> class by using the specified path that is a base directory for saving the temporary caches.
        /// </summary>
        /// <param name="path">The path to the base directory to be used.</param>
        public FileBasedResourceCache(System.String path) : this(new System.IO.DirectoryInfo(path)) { }

        /// <inheritdoc />
        /// <exception cref="System.ArgumentNullException"><paramref name="name"/> was <see langword="null"/> or empty.</exception>
        public IResourceEntry this[string name]
        {
            get {
                if (System.String.IsNullOrEmpty(name)) { throw new System.ArgumentNullException(nameof(name)); }
                System.String resdat = GetRawResource(name);
                return resdat is null ? throw new ResourceNotFoundException(name) : ReadResource(resdat);
            }
        }

        /// <summary>
        /// Runs <paramref name="index"/> times to locate and return a resource at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the resource inside the cache to return.</param>
        /// <returns>The resource saved to the cache that is located at <paramref name="index"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">Index was less than zero or was outside the resource cache data bounds.</exception>
        public IResourceEntry this[int index]
        {
            get {
                // Perform 'index' iterations to find the resource required
                System.String data = null;
                // If the index is less than zero , fail
                if (index < 0) { throw new IndexOutOfRangeException("index must be more than or equal to zero"); }
                indexreadpos = 0; // For searching the resource cache index
                for (System.Int32 I = 0; I <= index; I++) { data = ReadIndexLine(); }
                // If no data were found on such an index , fail
                if (data is null) { throw new IndexOutOfRangeException("index must be a zero-indexed number inside the registered resources bounds."); }
                // Read the resource and return it
                return ReadResource(data);
            }
        }

        /// <summary>
        /// Gets the number of resources that the <see cref="FileBasedResourceCache"/> holds.
        /// </summary>
        public uint Count => count;

        /// <summary>
        /// Adds a new derived instance of <see cref="IResourceEntry"/> to be cached.
        /// </summary>
        /// <param name="entry">The resource entry to be cached.</param>
        public void Add(IResourceEntry entry) => Add(entry.Name, entry.Value);

        /// <summary>
        /// Adds a new resource name and value to be cached.
        /// </summary>
        /// <param name="Name">The resource name.</param>
        /// <param name="Value">The resource value.</param>
        public void Add(string Name, object Value) => AddResource(Name, Value);

        private void AddResource<T>(System.String Name , T Value)
        {
            ParserHelpers.ValidateName(Name);
            count++;
            System.String fp ,ftyp;
            System.Byte[] temp , temp2;
            System.IO.FileStream FD = null;
            if (Value is System.String d) {
                temp = encoding.GetBytes($"{Name}=strings.txt");
                temp2 = encoding.GetBytes($"{Name}={MsIniStringsEncoder.Encode(d)}\n");
                strings.Write(temp2, 0, temp2.Length);
                strings.Flush();
            } else if (Value is System.Byte[] dt) {
                fp = System.IO.Path.Combine(filesdir.FullName, $"RESOURCE_{count}.ba");
                temp = encoding.GetBytes($"{Name}={fp}%{System.DateTime.Now.ToBinary()}");
                try {
                    FD = new(fp, System.IO.FileMode.Create);
                    FD.Write(dt, 0, dt.Length);
                } finally { FD?.Dispose(); }
            } else {
                fp = System.IO.Path.Combine(filesdir.FullName, $"RESOURCE_{count}.fe");
                ftyp = typeof(T).AssemblyQualifiedName;
                if (ftyp.StartsWith("System.Object")) { ftyp = Value.GetType().AssemblyQualifiedName; }
                temp = encoding.GetBytes($"{Name}={fp}%{ftyp}%{System.DateTime.Now.ToBinary()}");
                ftyp = null;
                temp2 = formatter.GetBytesFromObject(Value);
                try {
                    FD = new(fp, System.IO.FileMode.Create);
                    FD.Write(temp2, 0, temp2.Length);
                } finally { FD?.Dispose(); }
            }
            temp2 = null;
            index.Write(temp, 0, temp.Length);
            index.WriteByte(10);
            index.FlushAsync();
            temp = null;
        }

        private System.String GetRawResource(System.String Name)
        {
            System.String raw;
            System.String[] disassembled;
            indexreadpos = 0; // For searching the resource cache index
            while ((raw = ReadIndexLine()) is not null) {
                disassembled = raw.Split('=' , '%');
                if (disassembled.Length > 1 && disassembled[0] == Name) {
                    return raw;
                }
            }
            return null;
        }

        private FileResCacheResEntry ReadResource(System.String Data)
        {
            System.String temp;
            System.IO.FileStream tmpfs = null;
            List<System.String> disassembled = new(Data.Split(new System.Char[] { '=' } , 2));
            foreach (System.String adding in disassembled[1].Split('%')) { disassembled.Add(adding); }
            disassembled.RemoveAt(1);
            System.String[] temp2;
            switch (disassembled.Count) 
            {
                case 2:
                    if (disassembled[1] == "strings.txt") { // It is a string resource
                        srreadpos = 0;
                        while ((temp = ReadStringLine()) is not null) {
                            temp2 = temp.Split(new System.Char[] { '=' } , 2 , StringSplitOptions.RemoveEmptyEntries);
                            if (temp2[0] == disassembled[0]) {
                                return new FileResCacheResEntry(temp2[0], MsIniStringsEncoder.Decode(temp2[1]));
                            }
                        }
                    }
                    return null;
                case 3: // It is a byte array resource , which it means that the file must be read to end.
                    try {
                        tmpfs = new(disassembled[1], System.IO.FileMode.Open);
                        System.Byte[] dt = new System.Byte[tmpfs.Length];
                        tmpfs.Read(dt, 0, dt.Length);
                        return new FileResCacheResEntry(disassembled[0], dt);
                    } finally { tmpfs?.Dispose(); }
                case 4: // It is a serialized object saved to a file
                    try {
                        tmpfs = new(disassembled[1], System.IO.FileMode.Open);
                        System.Byte[] dt = new System.Byte[tmpfs.Length];
                        tmpfs.Read(dt, 0, dt.Length);
                        return new FileResCacheResEntry(disassembled[0], 
                            formatter.GetObjectFromBytes(dt, 
                            System.Type.GetType(disassembled[2], true, true)));
                    } finally { tmpfs?.Dispose(); }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Adds a new resource name and value with type <typeparamref name="TA"/>.
        /// </summary>
        /// <typeparam name="TA">The type of the resource value to add. Must be a non-nullable object.</typeparam>
        /// <param name="Name">The resource name to add.</param>
        /// <param name="Value">The resource value with type <typeparamref name="TA"/> to add.</param>
        public void Add<TA>(string Name, TA Value) where TA : notnull => AddResource(Name, Value);

        /// <summary>
        /// Clears all the resource entries from the cache , and optionally define whether to update the cache so as to delete any leftover temporary files.
        /// </summary>
        public void Clear(bool update = false)
        {
            strings.SetLength(0);
            index.SetLength(0);
            count = 0;
            if (update) {
                try { foreach (System.IO.FileInfo path in filesdir.GetFiles()) { path.Delete(); } } catch { }
            }
        }

        /// <summary>
        /// Deletes the directories and any data allocated for the <see cref="FileBasedResourceCache"/> class.
        /// </summary>
        public void Dispose() {
            formatter?.Dispose();
            formatter = null;
            index?.Dispose();
            index = null;
            strings?.Dispose();
            strings = null;
            filesdir?.Delete(true);
            filesdir = null;
            basedirectoryinfo?.Delete(true);
            basedirectoryinfo = null;
            encoding = null;
            count = 0;
            GC.SuppressFinalize(this);
        }

        private System.String ReadIndexLine()
        {
            if (indexreadpos >= index.Length) { return null; }
            index.Position = indexreadpos;
            System.Boolean cond = true , eos = false;
            while (cond) {
                switch (index.ReadByte()) {
                    case -1:
                        eos = true;
                        goto case 10;
                    case 10:
                    case 13:
                        cond = false;
                        break;
                }
            }
            System.Int32 count = (System.Int32)(eos ? index.Position - indexreadpos : index.Position - indexreadpos - 1);
            System.Byte[] temp = new System.Byte[count];
            index.Position = indexreadpos;
            index.Read(temp, 0, count);
            indexreadpos = index.Position + 1;
            return encoding.GetString(temp);
        }

        private System.String ReadStringLine()
        {
            if (srreadpos >= strings.Length) { return null; }
            strings.Position = srreadpos;
            System.Boolean cond = true, eos = false;
            while (cond)
            {
                switch (strings.ReadByte())
                {
                    case -1:
                        eos = true;
                        goto case 10;
                    case 10:
                    case 13:
                        cond = false;
                        break;
                }
            }
            System.Int32 count = (System.Int32)(eos ? strings.Position - srreadpos : strings.Position - srreadpos - 1);
            System.Byte[] temp = new System.Byte[count];
            strings.Position = srreadpos;
            strings.Read(temp, 0, count);
            srreadpos = strings.Position + 1;
            return encoding.GetString(temp);
        }

        /// <summary>
        /// Optimizes the resource cache by deleting any stale large data (such as byte arrays , etc) when those have been written for more than 10 minutes before.
        /// </summary>
        public void Optimize() {
            static System.Boolean CanBeDeletedSafely(System.String dtdata) {
                System.DateTime writetime = System.DateTime.FromBinary(System.Int64.Parse(dtdata));
                return System.DateTime.Now > writetime.AddMinutes(10);
            }
            System.String entry;
            System.String[] entrydat;
            while ((entry = ReadIndexLine()) is not null) {
                entrydat = entry.Split('%');
                if (entrydat.Length == 2) {
                    if (CanBeDeletedSafely(entrydat[1])) { System.IO.File.Delete(entrydat[0]); }
                } else if (entrydat.Length == 3) {
                    if (CanBeDeletedSafely(entrydat[2])) { System.IO.File.Delete(entrydat[0]); }
                }
            }
            indexreadpos = 0;
        }

        /// <inheritdoc />
        public void RegisterTypeResolver(ITypeResolver resolver) => formatter.RegisterTypeResolver(resolver);

        /// <summary>
        /// Removes the first occurence of a derived instance of <see cref="IResourceEntry"/> saved into the cache. <br />
        /// If the specified entry was not found , this method returns <see langword="false"/>.
        /// </summary>
        /// <param name="entry">The resource entry to delete.</param>
        /// <returns><see langword="true"/> on successfull deletion; <see langword="false"/> otherwise.</returns>
        public bool Remove(IResourceEntry entry) => Remove(entry.Name , entry.Value);

        /// <summary>
        /// Removes the first occurence of the specified resource name and value. <br />
        /// The type of the resource value must agree with the resource type saved in the cache! <br />
        /// If the specified resource name was not found , this method returns <see langword="false"/>.
        /// </summary>
        /// <param name="Name">The resource name which is to be deleted.</param>
        /// <param name="Value">The resource value which is to be deleted.</param>
        /// <returns><see langword="true"/> on successfull deletion; <see langword="false"/> otherwise.</returns>
        public bool Remove(string Name, object Value) => Remove<System.Object>(Name, Value);

        /// <summary>
        /// Removes the first occurence of the specified resource name and value. <br />
        /// The type of the resource value must agree with the resource type saved in the cache! <br />
        /// If the specified resource name was not found , this method returns <see langword="false"/>.
        /// </summary>
        /// <typeparam name="TR">The resource type of <paramref name="Value"/> to test when the resource will be deleted.</typeparam>
        /// <param name="Name">The resource name which is to be deleted.</param>
        /// <param name="Value">The resource value which is to be deleted.</param>
        /// <returns><see langword="true"/> on successfull deletion; <see langword="false"/> otherwise.</returns>
        /// <exception cref="ResourceTypeMismatchException">The type defined in <typeparamref name="TR"/> does not match the type defined in the cache.</exception>
        public bool Remove<TR>(string Name, TR Value) where TR : notnull
        {
            List<System.String> resources = new();
            try {
                System.String temp;
                indexreadpos = 0;
                while ((temp = ReadIndexLine()) is not null) { resources.Add(temp); }
                System.Boolean deleted = false;
                for (System.Int32 I = 0; I < resources.Count; I++) 
                {
                    if (deleted) { break; }
                    System.String[] dt = resources[I].Split('%', '=');
                    if (dt[0] != Name) { continue; }
                    deleted = true;
                    // If the second is strings.txt , we have a string resource to remove
                    if (dt.Length == 2 && dt[1] == "strings.txt" && Value is System.String) { 
                        resources.RemoveAt(I);
                    // This is a byte array , but remove it without checks
                    } else if (dt.Length == 3) {
                        resources.RemoveAt(I);
                    // This is a serialized object. Check TR first , but if TR is Object , then check Value for type equality.
                    } else if (dt.Length == 4) {
                        System.Boolean cond = false;
                        if (typeof(TR).FullName == "System.Object") {
                            cond = Value.GetType().AssemblyQualifiedName != dt[2];
                        } else { cond = typeof(TR).AssemblyQualifiedName != dt[2]; }
                        if (cond) {
                            throw new ResourceTypeMismatchException(Value.GetType(), System.Type.GetType(dt[2], true, true), "The resource type given in Value must match the one which is registered with this name.", Name);
                        }
                        // And , finally remove the resource if the information provided are correct.
                        resources.RemoveAt(I);
                    }
                }
                // If no changes were performed in the resource index , return false.
                if (deleted == false) { return false; }
                // Recreate the index appropriately
                count = resources.Count.ToUInt32();
                index.SetLength(0);
                indexreadpos = 0;
                foreach (var resource in resources) {
                    System.Byte[] bytes = encoding.GetBytes($"{resource}\n");
                    index.Write(bytes, 0, bytes.Length);
                }
                index.Flush();
                return true;
            } finally { resources.Clear(); resources = null; }
        }

        /// <summary>
        /// Flushes all string and resource search index data to the disk.
        /// </summary>
        public void Update() { index.Flush(); strings.Flush(); }

        /// <summary>
        /// Gets an enumerator which does directly provide resource entries to it's users. Suitable for simple enumeration cases.
        /// </summary>
        public IEnumerator<IResourceEntry> GetEnumerator() => new ResourceOptimizedEntryEnumerator(this);

        /// <inheritdoc />
        public IResourceEntryEnumerator GetResourceEntryEnumerator() => new ResourceOptimizedEntryEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Guard so as to ensure that all resources will be immediately released , if any.
        /// </summary>
        ~FileBasedResourceCache() => Dispose();
    }

}
