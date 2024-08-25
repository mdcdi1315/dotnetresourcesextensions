
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{

    namespace Collections
    {
        /// <summary>
        /// Compares two resource entries by their name. <br />
        /// Usually the name is enough to infer that two resource entries are equal.
        /// </summary>
        public sealed class ResourceEntryComparer : Comparer<IResourceEntry>
        {
            /// <summary>
            /// Compares two <see cref="IResourceEntry"/> instances.
            /// </summary>
            /// <param name="x">The first entry to compare.</param>
            /// <param name="y">The second entry to compare.</param>
            /// <returns><inheritdoc cref="Comparer{T}.Compare(T, T)"/></returns>
            public override int Compare(IResourceEntry x, IResourceEntry y) => x.Name.CompareTo(y.Name);

            /// <summary>
            /// Creates a new instance of this resource entry comparer.
            /// </summary>
            public ResourceEntryComparer() : base() { }
        }
    }

    /// <summary>
    /// Defines a reusuable resource entry layout. <br />
    /// Primarily used in cases where resources must be programmatically retrieved.
    /// </summary>
    public interface IResourceEntry
    {
        /// <summary>
        /// Gets the name of the current resource.
        /// </summary>
        public System.String Name { get; }

        /// <summary>
        /// Gets the resource value.
        /// </summary>
        public System.Object Value { get; }

        /// <summary>
        /// Gets the type of the object contained in <see cref="Value"/>. <br />
        /// Returns <see langword="null"/> when the <see cref="Value"/> is <see langword="null"/>.
        /// </summary>
        public System.Type TypeOfValue { get; }
    }

    /// <summary>
    /// Defines common resource entry extensions.
    /// </summary>
    public static class IResourceEntryExtensions
    {
        // Default wrapped entry class implementation to use in the IResourceEntry extensions.
        [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
        private sealed class DefaultResourceEntry : IResourceEntry
        {
            private System.String name;
            private System.Object value;
            private System.Boolean iscloned , istyped;

            public DefaultResourceEntry(DictionaryEntry entry) 
            {
                name = (System.String)entry.Key;
                value = entry.Value;
                iscloned = false;
                istyped = false;
            }

            public DefaultResourceEntry(KeyValuePair<System.String, System.Object> entry)
            {
                name = entry.Key;
                value = entry.Value;
                iscloned = false;
                istyped = false;
            }

            public DefaultResourceEntry(string name, object value)
            {
                this.name = name;
                this.value = value;
                iscloned = false;
                istyped = true;
            }

            public DefaultResourceEntry(IResourceEntry entry)
            {
                name = entry.Name;
                value = entry.Value;
                iscloned = true;
                istyped = false;
            }

            public string Name => name;

            public object Value => value;

            public Type TypeOfValue => value?.GetType();

            public System.Boolean IsCloned => iscloned;

            public System.Boolean IsTyped => istyped;

            public void Deconstruct(out System.String Name , out System.Object Value)
            {
                Name = name;
                Value = value;
            }

            private string GetDebuggerDisplay() => ToString();

            public override string ToString() => $"{nameof(IResourceEntry)}: {{ Name = {name} , Value = {value} }}";
        }

        // A typed resource loader to get resources from IResourceLoader that 
        // these extension methods define.
        private sealed class TypedResourcesLoader : IResourceLoader , ICollection<IResourceEntry>
        {
            private List<IResourceEntry> resources;

            public TypedResourcesLoader() { resources = new(); }

            public TypedResourcesLoader(params IResourceEntry[] resources)
            {
                this.resources = new();
                this.resources.AddRange(resources);
            }

            public void Dispose() { resources?.Clear(); resources = null; }

            public void Add(IResourceEntry ent) => resources.Add(ent);

            public System.Boolean Remove(IResourceEntry ent) => resources.Remove(ent);

            public void Clear() => resources.Clear();

            public System.Boolean Contains(IResourceEntry entry) => resources.Contains(entry);

            public void CopyTo(IResourceEntry[] array, int arrayIndex) => resources.CopyTo(array, arrayIndex);

            public System.Int32 Count => resources.Count;

            public System.Boolean IsReadOnly => false;

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            IEnumerator<IResourceEntry> IEnumerable<IResourceEntry>.GetEnumerator() => GetEnumerator();

            public ValueTask DisposeAsync() => new(Task.Run(Dispose));

            public IAdvancedResourceEnumerator GetAdvancedResourceEnumerator()
                => new IResourceEnumerableExtensions.DefaultAdvancedResourceEnumerator(resources.GetEnumerator());

            public IEnumerable<string> GetAllResourceNames()
            {
                foreach (var resource in resources)
                {
                    yield return resource?.Name;
                }
            }

            public IFullResourceEnumerator GetEnumerator() 
                => new IResourceEnumerableExtensions.DefaultFullResourceEnumerator(resources.GetEnumerator());

            public IEnumerable<string> GetRelativeResources(string Name)
            {
                System.String inline;
                foreach (var D in resources)
                {
                    inline = D.Name;
                    if (inline.StartsWith(Name)) { yield return inline; }
                    if (inline == Name) { yield return inline; }
                }
            }

            public T GetResource<T>(string Name) where T : notnull
            {
                foreach (var res in resources)
                {
                    if (res.Name == Name)
                    {
                        if (res.TypeOfValue != typeof(T))
                        {
                            throw new ResourceTypeMismatchException(typeof(T), res.TypeOfValue, Name);
                        }
                        return (T)res.Value;
                    }
                }
                throw new ResourceNotFoundException(Name);
            }

            public IEnumerable<string> GetResourceNamesStartingWith(string start)
            {
                foreach (var dg in resources)
                {
                    if (dg.Name.StartsWith(start)) { yield return dg.Name; }
                }
            }

            public ISimpleResourceEnumerator GetSimpleResourceEnumerator()
                => new IResourceEnumerableExtensions.DefaultSimpleResourceEnumerator(resources.GetEnumerator());

            public string GetStringResource(string Name) => GetResource<System.String>(Name);
        }

        /// <summary>
        /// Reinterprets the specified resource entry as a <see cref="DictionaryEntry"/> structure.
        /// </summary>
        /// <param name="entry">The entry to reinterpret.</param>
        /// <returns>The created <see cref="DictionaryEntry"/> structure.</returns>
        public static DictionaryEntry AsDictionaryEntry(this IResourceEntry entry) => new(entry.Name , entry.Value);  

        /// <summary>
        /// Reinterprets the specified dictionary entry as a <see cref="IResourceEntry"/> implementation.
        /// </summary>
        /// <param name="entry">The entry to reinterpret.</param>
        /// <returns>A created object that implements the <see cref="IResourceEntry"/> interface.</returns>
        public static IResourceEntry AsResourceEntry(this DictionaryEntry entry)
            => new DefaultResourceEntry(entry);
#pragma warning disable 1735
        /// <summary>
        /// Reinterprets the specified resource entry as a new <see cref="KeyValuePair{TKey, TValue}"/> , where
        /// <typeparamref name="TKey"/> is <see cref="System.String"/> and <typeparamref name="TValue"/>
        /// is <see cref="Object"/>.
        /// </summary>
        /// <param name="entry">The resource entry to reinterpret.</param>
        /// <returns>The created <see cref="KeyValuePair{TKey, TValue}"/> structure.</returns>
#pragma warning restore 1735
        public static KeyValuePair<System.String , System.Object> AsKeyValuePair(this IResourceEntry entry) => new(entry.Name , entry.Value);

#pragma warning disable 1735
        /// <summary>
        /// Reinterprets the specified <see cref="KeyValuePair{TKey, TValue}"/> , where
        /// <typeparamref name="TKey"/> is <see cref="System.String"/> and <typeparamref name="TValue"/>
        /// is <see cref="Object"/> , as a new resource entry.
        /// </summary>
        /// <param name="entry">The key-value pair to reinterpret.</param>
        /// <returns>The resulting resource entry.</returns>
#pragma warning restore 1735
        public static IResourceEntry AsResourceEntry(this KeyValuePair<System.String, System.Object> entry)
           => new DefaultResourceEntry(entry);

        /// <summary>
        /// Reinterprets the specified resource entry as a tuple pair , which it's first item is the resource name and
        /// the second one contains the resource entry value.
        /// </summary>
        /// <param name="entry">The resource entry to reinterpret.</param>
        /// <returns>A <see cref="System.Tuple{T1, T2}"/> , or pair that is a reinterpreted version of the resource entry given.</returns>
        public static System.Tuple<System.String , System.Object> AsTuple(this IResourceEntry entry) => new(entry.Name, entry.Value);

        /// <summary>
        /// Reinterprets the specified resource entry as a tuple pair , which it's first item is the resource name and
        /// the second one contains the resource entry value.
        /// </summary>
        /// <param name="entry">The resource entry to reinterpret.</param>
        /// <returns>A <see cref="System.ValueTuple{T1, T2}"/> , or a pair that is a reinterpreted version of the resource entry given.</returns>
        public static System.ValueTuple<System.String, System.Object> AsValueTuple(this IResourceEntry entry) => new(entry.Name , entry.Value);

        /// <summary>
        /// Reinterprets the specified tuple pair as a new resource entry. The tuple's first item must be the string
        /// that is the resource name and an object that represents the resource value.
        /// </summary>
        /// <param name="tuple">The tuple to reinterpret as a resource entry.</param>
        /// <returns>An object that implements the <see cref="IResourceEntry"/> interface and has as it's values 
        /// the values took from the <paramref name="tuple"/>.</returns>
        public static IResourceEntry AsResourceEntry(this System.Tuple<System.String, System.Object> tuple)
        => new DefaultResourceEntry(tuple.Item1, tuple.Item2);

        /// <summary>
        /// Reinterprets the specified tuple pair as a new resource entry. The tuple's first item must be the string
        /// that is the resource name and an object that represents the resource value.
        /// </summary>
        /// <param name="tuple">The tuple to reinterpret as a resource entry.</param>
        /// <returns>An object that implements the <see cref="IResourceEntry"/> interface and has as it's values 
        /// the values took from the <paramref name="tuple"/>.</returns>
        public static IResourceEntry AsResourceEntry(this System.ValueTuple<System.String, System.Object> tuple)
        => new DefaultResourceEntry(tuple.Item1 , tuple.Item2);

        /// <summary>
        /// Clones the specified resource entry to a new resource entry. <br />
        /// Works for all <see cref="IResourceEntry"/> descendants.
        /// </summary>
        /// <param name="entry">The entry to clone.</param>
        /// <returns>The cloned entry.</returns>
        public static IResourceEntry Clone(this IResourceEntry entry) => new DefaultResourceEntry(entry);

        /// <summary>
        /// Determines whether this entry is a cloned entry (That is , the created object from <see cref="Clone(IResourceEntry)"/> method). 
        /// </summary>
        /// <param name="entry">The entry to test.</param>
        /// <returns>A value that determines if this entry is cloned.</returns>
        public static System.Boolean IsClonedEntry(this IResourceEntry entry)
        {
            if (entry is DefaultResourceEntry dr) { return dr.IsCloned; }
            return false;
        }

        /// <summary>
        /// Determines whether this entry is an entry created using the <see cref="Create(string, object)"/> method.
        /// </summary>
        /// <param name="entry">The resource entry to test.</param>
        /// <returns>A value that determines whether this entry was created using the <see cref="Create(string, object)"/> method.</returns>
        public static System.Boolean IsTypedEntry(this IResourceEntry entry)
        {
            if (entry is DefaultResourceEntry dr) { return dr.IsTyped; }
            return false;
        }

        /// <summary>
        /// Creates a new resource entry from just a string that is the resource name and
        /// an object that is the value of the resource.
        /// </summary>
        /// <param name="Name">The resource name.</param>
        /// <param name="Value">The resource value. Can be null.</param>
        /// <returns>A new <see cref="IResourceEntry"/> object.</returns>
        public static IResourceEntry Create(System.String Name , System.Object Value) => new DefaultResourceEntry(Name , Value);

        /// <summary>
        /// Determines whether this entry originates from a <see cref="IResourceLoader"/> instance.
        /// </summary>
        /// <param name="entry">The entry to determine whether it originates from the <see cref="IResourceLoader"/>.</param>
        /// <returns><see langword="true"/> if this entry originates from a <see cref="IResourceLoader"/> instance; otherwise , <see langword="false"/>.</returns>
        public static System.Boolean IsRealResourceEntry(this IResourceEntry entry) => entry is not DefaultResourceEntry;
    
        /// <summary>
        /// Determines whether this resource entry name is equal to another resource entry name.
        /// </summary>
        /// <param name="entry">The first entry to test.</param>
        /// <param name="other">The second entry to test.</param>
        /// <returns><see langword="true"/> if their names are equal; otherwise <see langword="false"/>.</returns>
        public static System.Boolean IsEqualTo(this IResourceEntry entry , IResourceEntry other) => CompareTo(entry, other) == 0;

        /// <summary>
        /// Determines whether the <see cref="IResourceEntry.Value"/> property is effectively <see langword="null"/>.
        /// </summary>
        /// <param name="entry">The resource entry to test.</param>
        /// <returns><see langword="true"/> if the <see cref="IResourceEntry.Value"/> property is <see langword="null"/>;
        /// otherwise , it returns <see langword="false"/>. <br />
        /// The method also returns <see langword="false"/> when <paramref name="entry"/> is <see langword="null"/>.</returns>
        public static System.Boolean ValueIsNull(this IResourceEntry entry) => entry is not null && entry.Value is null;

        /// <summary>
        /// Determines whether the <see cref="IResourceEntry.Value"/> property holds a primitive structure. <br />
        /// Primitive structures are all the .NET numeric types , including the <see cref="System.Boolean">Boolean</see> and the <see cref="System.Char">Character</see> structure.
        /// </summary>
        /// <param name="entry">The entry to test.</param>
        /// <returns><see langword="true"/> if the <see cref="IResourceEntry.Value"/> property holds a primitive structure; otherwise <see langword="false"/>. <br />
        /// The method also returns <see langword="false"/> when the property is <see langword="null"/>.</returns>
        public static System.Boolean ValueIsPrimitive(this IResourceEntry entry)
        {
            if (ValueIsNull(entry)) { return false; }
            switch (entry.TypeOfValue.FullName)
            {
                case "System.Boolean":
                case "System.Char":
                case "System.SByte":
                case "System.Byte":
                case "System.Int16":
                case "System.UInt16":
                case "System.Int32":
                case "System.UInt32":
                case "System.Int64":
                case "System.UInt64":
                case "System.Single":
                case "System.Double":
                // Although that the below 3 cases are never reached for .NET Framework (because these were not implemented back then) ,
                // the method will still work as expected.
                case "System.Half": 
                case "System.Int128":
                case "System.UInt128":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Defines a generalized method to deconstruct an <see cref="IResourceEntry"/>-derived class. <br />
        /// Together with the language internals , this method can be used to deconstruct such an instance immediately. <br />
        /// If your implementing class provides more information that you need also to be passed, you must create your own deconstruction method overload.
        /// </summary>
        /// <param name="entry">The resource entry to deconstruct</param>
        /// <param name="Name">The deconstructed entry name.</param>
        /// <param name="Value">The deconstructed entry value.</param>
        public static void Deconstruct(this IResourceEntry entry , out System.String Name , out System.Object Value)
        {
            if (entry is null) { Name = System.String.Empty; Value = null; return; }
            Name = entry.Name;
            Value = entry.Value;
        }

        /// <summary>
        /// Compares this resource entry name to another resource entry name.
        /// </summary>
        /// <param name="entry">The first entry to compare.</param>
        /// <param name="other">The second entry to compare.</param>
        /// <returns>A signed integer that indicates their compared relationship.</returns>
        public static System.Int32 CompareTo(this IResourceEntry entry, IResourceEntry other) => new Collections.ResourceEntryComparer().Compare(entry, other);

        /// <summary>
        /// Gets a new instance of the <see cref="IResourceLoader"/> interface from the specified entries.
        /// </summary>
        /// <param name="entries">The entries to create the loader instance from.</param>
        /// <returns>A new object that implements the <see cref="IResourceLoader"/> behavior.</returns>
        public static IResourceLoader GetLoaderFromEntries(this IEnumerable<IResourceEntry> entries)
        {
            TypedResourcesLoader loader = new();
            foreach (var entry in entries) { loader.Add(entry); }
            return loader;
        }

        /// <summary>
        /// Gets a new instance of the <see cref="IResourceLoader"/> interface from the specified entries.
        /// </summary>
        /// <param name="entries">The entries to create the loader instance from.</param>
        /// <returns>A new object that implements the <see cref="IResourceLoader"/> behavior.</returns>
        public static IResourceLoader GetLoaderFromEntries(this IResourceEntry[] entries) => new TypedResourcesLoader(entries);

        /// <summary>
        /// Returns the loader resource entries in a new , reususable collection.
        /// </summary>
        /// <param name="loader">The loader to get the entries from.</param>
        /// <returns>An object that implements the <see cref="ICollection{T}"/> interface.</returns>
        public static ICollection<IResourceEntry> AsCollection(this IResourceLoader loader)
        {
            if (loader is TypedResourcesLoader ld) { return ld; }
            List<IResourceEntry> entries = new();
            foreach (IResourceEntry ent in loader) { entries.Add(ent); }
            return entries;
        }
    }

}
