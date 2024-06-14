
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
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
    /// Defines an advanced resource enumerator for cases that you need to progammatically
    /// access resources in a <see cref="IResourceLoader"/> interface.
    /// </summary>
    public interface IAdvancedResourceEnumerator : IEnumerator<IResourceEntry>
    {
        /// <summary>
        /// Gets the underlying resource entry.
        /// </summary>
        public IResourceEntry Entry { get; }
    }

    /// <summary>
    /// Defines a simple resource enumerator.
    /// </summary>
    public interface ISimpleResourceEnumerator : IEnumerator<IResourceEntry>
    {
        /// <summary>
        /// Although that it should be named as the resource name, it is kept that way for compatibility 
        /// lines with the <see cref="System.Collections.IDictionaryEnumerator"/> interface.
        /// </summary>
        public System.String Key { get; }

        /// <summary>
        /// Gets the underlying resource value.
        /// </summary>
        public System.Object Value { get; }
    }

    /// <summary>
    /// It is a dummy interface that defines both <see cref="IAdvancedResourceEnumerator"/> and <see cref="ISimpleResourceEnumerator"/>
    /// for the <see cref="System.Collections.IEnumerable"/> interface needs.
    /// </summary>
    public interface IFullResourceEnumerator : IAdvancedResourceEnumerator , ISimpleResourceEnumerator { }

    /// <summary>
    /// Defines a way to enumerate resources with multiple ways. <br />
    /// </summary>
    public interface IResourceEnumerable
    {
        /// <summary>
        /// Gets a implementation of <see cref="IAdvancedResourceEnumerator"/> for enumerating the resources.
        /// </summary>
        /// <returns>An advanced resource enumerator.</returns>
        public IAdvancedResourceEnumerator GetAdvancedResourceEnumerator();

        /// <summary>
        /// The default enumerator. Also combines the nature of both <see cref="ISimpleResourceEnumerator"/> and
        /// <see cref="IFullResourceEnumerator"/> interfaces.
        /// </summary>
        /// <returns>An enumerator to iterate through all the resources that are defined.</returns>
        public IFullResourceEnumerator GetEnumerator();

        /// <summary>
        /// Gets a implementation of <see cref="ISimpleResourceEnumerator"/> for enumerating the resources.
        /// </summary>
        /// <returns>A simple resource enumerator.</returns>
        public ISimpleResourceEnumerator GetSimpleResourceEnumerator();
    }

    /// <summary>
    /// Defines common extension methods for the <see cref="IResourceEnumerable"/> interface.
    /// </summary>
    public static class IResourceEnumerableExtensions
    {
        private enum UsageManagement : System.Byte
        {
            IEnumerator,
            IDictionaryEnumerator
        }

        /// <summary>
        /// This is a default reference implementation class for the <see cref="IFullResourceEnumerator"/> interface. <br />
        /// Also used for the underlying enumerator implementations in <see cref="DefaultResourceLoader"/>
        /// and <see cref="OptimizedResourceLoader"/> classes..
        /// </summary>
        public sealed class DefaultFullResourceEnumerator : IFullResourceEnumerator
        {
            private IEnumerator<IResourceEntry> enumerator;
            private IDictionaryEnumerator enumerator1;
            private UsageManagement mgmt;

            /// <summary>
            /// Creates a new instance of <see cref="DefaultFullResourceEnumerator"/> class with the specified dictionary enumerator to use.
            /// </summary>
            /// <param name="enumerator">The enumerator to use.</param>
            public DefaultFullResourceEnumerator(IDictionaryEnumerator enumerator)
            {
                enumerator1 = enumerator;
                mgmt = UsageManagement.IDictionaryEnumerator;
            }

            /// <summary>
            /// Creates a new instance of <see cref="DefaultFullResourceEnumerator"/> class with the specified enumerator to use.
            /// </summary>
            /// <param name="enumerator">The enumerator to use.</param>
            public DefaultFullResourceEnumerator(IEnumerator<IResourceEntry> enumerator)
            {
                this.enumerator = enumerator;
                mgmt = UsageManagement.IEnumerator;
            }

            /// <summary>
            /// The resource name.
            /// </summary>
            public string Key
            {
                get
                {
                    if (mgmt == UsageManagement.IEnumerator)
                    {
                        return enumerator.Current.Name;
                    }
                    else
                    {
                        return (System.String)enumerator1.Key;
                    }
                }
            }

            /// <summary>
            /// The resource value.
            /// </summary>
            public object Value
            {
                get
                {
                    if (mgmt == UsageManagement.IEnumerator)
                    {
                        return enumerator.Current.Value;
                    }
                    else
                    {
                        return enumerator1.Value;
                    }
                }
            }

            /// <summary>
            /// The current resource entry.
            /// </summary>
            public IResourceEntry Current
            {
                get
                {
                    if (mgmt == UsageManagement.IEnumerator)
                    {
                        return enumerator.Current;
                    }
                    else
                    {
                        return enumerator1.Entry.AsResourceEntry();
                    }
                }
            }

            /// <summary>
            /// Gets the resource entry.
            /// </summary>
            public IResourceEntry Entry => Current;

            object IEnumerator.Current => Current;

            /// <summary>
            /// Dereferences the underlying enumerator.
            /// </summary>
            public void Dispose()
            {
                enumerator1 = null;
                enumerator = null;
            }

            /// <summary>
            /// Moves to a next resource defined in the underlying enumerator.
            /// </summary>
            /// <returns>A value whether more resources exist after the moved one.</returns>
            public bool MoveNext()
            {
                if (mgmt == UsageManagement.IEnumerator)
                {
                    return enumerator.MoveNext();
                } else {
                    return enumerator1.MoveNext();
                }
            }

            /// <summary>
            /// Resets the enumerator to it's initial position , which is before the first resource found.
            /// </summary>
            public void Reset() => enumerator.Reset();
        }

        /// <summary>
        /// This is a default reference implementation class for the <see cref="IAdvancedResourceEnumerator"/> interface. <br />
        /// Also used for the underlying enumerator implementations in <see cref="DefaultResourceLoader"/>
        /// and <see cref="OptimizedResourceLoader"/> classes..
        /// </summary>
        public sealed class DefaultAdvancedResourceEnumerator : IAdvancedResourceEnumerator
        {
            private IEnumerator<IResourceEntry> enumerator;
            private IDictionaryEnumerator enumerator1;
            private UsageManagement mgmt;

            /// <summary>
            /// Creates a new instance of <see cref="DefaultAdvancedResourceEnumerator"/> class with the specified dictionary enumerator to use.
            /// </summary>
            /// <param name="enumerator">The enumerator to use.</param>
            public DefaultAdvancedResourceEnumerator(IDictionaryEnumerator enumerator)
            {
                enumerator1 = enumerator;
                mgmt = UsageManagement.IDictionaryEnumerator;
            }

            /// <summary>
            /// Creates a new instance of <see cref="DefaultAdvancedResourceEnumerator"/> class with the specified enumerator to use.
            /// </summary>
            /// <param name="enumerator">The enumerator to use.</param>
            public DefaultAdvancedResourceEnumerator(IEnumerator<IResourceEntry> enumerator)
            {
                this.enumerator = enumerator;
                mgmt = UsageManagement.IEnumerator;
            }

            /// <summary>
            /// The current resource entry.
            /// </summary>
            public IResourceEntry Current
            {
                get
                {
                    if (mgmt == UsageManagement.IEnumerator)
                    {
                        return enumerator.Current;
                    }
                    else
                    {
                        return enumerator1.Entry.AsResourceEntry();
                    }
                }
            }

            /// <summary>
            /// Gets the resource entry.
            /// </summary>
            public IResourceEntry Entry => Current;

            object IEnumerator.Current => Current;

            /// <summary>
            /// Dereferences the underlying enumerator.
            /// </summary>
            public void Dispose()
            {
                enumerator1 = null;
                enumerator = null;
            }

            /// <summary>
            /// Moves to a next resource defined in the underlying enumerator.
            /// </summary>
            /// <returns>A value whether more resources exist after the moved one.</returns>
            public bool MoveNext()
            {
                if (mgmt == UsageManagement.IEnumerator)
                {
                    return enumerator.MoveNext();
                }
                else
                {
                    return enumerator1.MoveNext();
                }
            }

            /// <summary>
            /// Resets the enumerator to it's initial position , which is before the first resource found.
            /// </summary>
            public void Reset() => enumerator.Reset();
        }

        /// <summary>
        /// This is a default reference implementation class for the <see cref="ISimpleResourceEnumerator"/> interface. <br />
        /// Also used for the underlying enumerator implementations in <see cref="DefaultResourceLoader"/>
        /// and <see cref="OptimizedResourceLoader"/> classes..
        /// </summary>
        public sealed class DefaultSimpleResourceEnumerator : ISimpleResourceEnumerator
        {
            private IEnumerator<IResourceEntry> enumerator;
            private IDictionaryEnumerator enumerator1;
            private UsageManagement mgmt;

            /// <summary>
            /// Creates a new instance of <see cref="DefaultSimpleResourceEnumerator"/> class with the specified dictionary enumerator to use.
            /// </summary>
            /// <param name="enumerator">The enumerator to use.</param>
            public DefaultSimpleResourceEnumerator(IDictionaryEnumerator enumerator)
            {
                enumerator1 = enumerator;
                mgmt = UsageManagement.IDictionaryEnumerator;
            }

            /// <summary>
            /// Creates a new instance of <see cref="DefaultSimpleResourceEnumerator"/> class with the specified enumerator to use.
            /// </summary>
            /// <param name="enumerator">The enumerator to use.</param>
            public DefaultSimpleResourceEnumerator(IEnumerator<IResourceEntry> enumerator)
            {
                this.enumerator = enumerator;
                mgmt = UsageManagement.IEnumerator;
            }

            /// <summary>
            /// The resource name.
            /// </summary>
            public string Key
            {
                get {
                    if (mgmt == UsageManagement.IEnumerator)
                    {
                        return enumerator.Current.Name;
                    } else {
                        return (System.String)enumerator1.Key;
                    }
                }
            }

            /// <summary>
            /// The resource value.
            /// </summary>
            public object Value
            {
                get {
                    if (mgmt == UsageManagement.IEnumerator)
                    {
                        return enumerator.Current.Value;
                    } else {
                        return enumerator1.Value;
                    }
                }
            }

            /// <summary>
            /// The current resource entry.
            /// </summary>
            public IResourceEntry Current
            {
                get {
                    if (mgmt == UsageManagement.IEnumerator)
                    {
                        return enumerator.Current;
                    } else {
                        return enumerator1.Entry.AsResourceEntry();
                    }
                }
            }

            object IEnumerator.Current => Current;

            /// <summary>
            /// Dereferences the underlying enumerator.
            /// </summary>
            public void Dispose()
            {
                enumerator1 = null;
                enumerator = null;
            }

            /// <summary>
            /// Moves to a next resource defined in the underlying enumerator.
            /// </summary>
            /// <returns>A value whether more resources exist after the moved one.</returns>
            public bool MoveNext()
            {
                if (mgmt == UsageManagement.IEnumerator)
                {
                    return enumerator.MoveNext();
                }
                else
                {
                    return enumerator1.MoveNext();
                }
            }

            /// <summary>
            /// Resets the enumerator to it's initial position , which is before the first resource found.
            /// </summary>
            public void Reset() => enumerator.Reset();
        }
        
        /// <summary>
        /// Returns this enumerator but defined as a <see cref="DefaultSimpleResourceEnumerator"/> class.
        /// </summary>
        /// <param name="en">The original enumerator</param>
        /// <returns>The constructed default enumerator.</returns>
        public static DefaultSimpleResourceEnumerator AsDefaultEnumerator(this ISimpleResourceEnumerator en)
         => new DefaultSimpleResourceEnumerator(en);

        /// <summary>
        /// Returns this enumerator but defined as a <see cref="DefaultAdvancedResourceEnumerator"/> class.
        /// </summary>
        /// <param name="en">The original enumerator</param>
        /// <returns>The constructed default enumerator.</returns>
        public static DefaultAdvancedResourceEnumerator AsDefaultEnumerator(this IAdvancedResourceEnumerator en)
            => new DefaultAdvancedResourceEnumerator(en);

        /// <summary>
        /// Returns this enumerator but defined as a <see cref="DefaultFullResourceEnumerator"/> class.
        /// </summary>
        /// <param name="en">The original enumerator</param>
        /// <returns>The constructed default enumerator.</returns>
        public static DefaultFullResourceEnumerator AsDefaultFullEnumerator(this IFullResourceEnumerator en)
            => new DefaultFullResourceEnumerator(en);

        /// <summary>
        /// Gets <see cref="IResourceEnumerable.GetEnumerator"/> but defined as a <see cref="DefaultFullResourceEnumerator"/>..
        /// </summary>
        /// <param name="en">The object instance to take the <see cref="IResourceEnumerable.GetEnumerator"/> method.</param>
        /// <returns>A new <see cref="DefaultFullResourceEnumerator"/> that wraps the contents of <see cref="IResourceEnumerable.GetEnumerator"/> method.</returns>
        public static DefaultFullResourceEnumerator GetDefaultFullResourceEnumerator(this IResourceEnumerable en)
            => new DefaultFullResourceEnumerator(en.GetEnumerator());

        /// <summary>
        /// Gets <see cref="IResourceEnumerable.GetAdvancedResourceEnumerator"/> but defined as a <see cref="DefaultAdvancedResourceEnumerator"/>..
        /// </summary>
        /// <param name="en">The object instance to take the <see cref="IResourceEnumerable.GetAdvancedResourceEnumerator"/> method.</param>
        /// <returns>A new <see cref="DefaultAdvancedResourceEnumerator"/> that wraps the contents of <see cref="IResourceEnumerable.GetAdvancedResourceEnumerator"/> method.</returns>
        public static DefaultAdvancedResourceEnumerator GetDefaultAdvancedResourceEnumerator(this IAdvancedResourceEnumerator en)
            => new DefaultAdvancedResourceEnumerator(en);

        /// <summary>
        /// Gets <see cref="IResourceEnumerable.GetSimpleResourceEnumerator"/> but defined as a <see cref="DefaultSimpleResourceEnumerator"/>..
        /// </summary>
        /// <param name="en">The object instance to take the <see cref="IResourceEnumerable.GetSimpleResourceEnumerator"/> method.</param>
        /// <returns>A new <see cref="DefaultSimpleResourceEnumerator"/> that wraps the contents of <see cref="IResourceEnumerable.GetSimpleResourceEnumerator"/> method.</returns>
        public static DefaultSimpleResourceEnumerator GetDefaultSimpleResourceEnumerator(this ISimpleResourceEnumerator en)
            => new DefaultSimpleResourceEnumerator(en);
    }

    /// <summary>
    /// Defines common resource entry extensions.
    /// </summary>
    public static class IResourceEntryExtensions
    {
        private class DefaultResourceEntry : IResourceEntry
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
        }

        private sealed class TypedResourcesLoader : IResourceLoader
        {
            private List<IResourceEntry> resources;

            public TypedResourcesLoader() { resources = new(); }

            public TypedResourcesLoader(params IResourceEntry[] resources)
            {
                this.resources = new();
                foreach (var resource in resources)
                {
                    this.resources.Add(resource);
                }
            }

            public void Dispose() { resources?.Clear(); resources = null; }

            public void Add(IResourceEntry ent) => resources.Add(ent);

            public ValueTask DisposeAsync() => new ValueTask(Task.Run(Dispose));

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
        /// <returns></returns>
        public static System.Boolean IsRealResourceEntry(this IResourceEntry entry) => entry is not DefaultResourceEntry;
    
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
        
    }

}
