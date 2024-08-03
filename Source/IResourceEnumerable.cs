

using System.Collections.Generic;
using System.Collections;
using System;

namespace DotNetResourcesExtensions
{
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
    /// for the <see cref="IResourceEnumerable"/> interface needs.
    /// </summary>
    public interface IFullResourceEnumerator : IAdvancedResourceEnumerator, ISimpleResourceEnumerator { }

    /// <summary>
    /// Defines a way to enumerate resources with multiple ways. <br />
    /// Note: This interface is not meant to be confused with <see cref="Collections.IResourceEntryEnumerable"/> 
    /// because this one is for <see cref="IResourceLoader"/> , while the other is for using it inside collections.
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
                    if (mgmt == UsageManagement.IEnumerator) {
                        return enumerator.Current;
                    } else {
                        return new HiddenEnumeratorResourceEntry(enumerator1.Entry);
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
                    if (mgmt == UsageManagement.IEnumerator) {
                        return enumerator.Current;
                    } else {
                        return new HiddenEnumeratorResourceEntry(enumerator1.Entry);
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
                    if (mgmt == UsageManagement.IEnumerator) {
                        return enumerator.Current;
                    } else {
                        return new HiddenEnumeratorResourceEntry(enumerator1.Entry);
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
                if (mgmt == UsageManagement.IEnumerator) {
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

        private sealed class HiddenEnumeratorResourceEntry : IResourceEntry
        {
            private readonly System.String name;
            private readonly System.Object value;

            public HiddenEnumeratorResourceEntry(string name, object value)
            {
                this.name = name;
                this.value = value;
            }

            public HiddenEnumeratorResourceEntry(DictionaryEntry de) : this(de.Key.ToString() , de.Value) { }

            public System.String Name => name;

            public System.Object Value => value;

            public System.Type TypeOfValue => value?.GetType();
        }

        /// <summary>
        /// Returns this enumerator but defined as a <see cref="DefaultSimpleResourceEnumerator"/> class.
        /// </summary>
        /// <param name="en">The original enumerator</param>
        /// <returns>The constructed default enumerator.</returns>
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public static DefaultSimpleResourceEnumerator AsDefaultEnumerator(this ISimpleResourceEnumerator en)
         => new DefaultSimpleResourceEnumerator(en);

        /// <summary>
        /// Returns this enumerator but defined as a <see cref="DefaultAdvancedResourceEnumerator"/> class.
        /// </summary>
        /// <param name="en">The original enumerator</param>
        /// <returns>The constructed default enumerator.</returns>
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public static DefaultAdvancedResourceEnumerator AsDefaultEnumerator(this IAdvancedResourceEnumerator en)
            => new DefaultAdvancedResourceEnumerator(en);

        /// <summary>
        /// Returns this enumerator but defined as a <see cref="DefaultFullResourceEnumerator"/> class.
        /// </summary>
        /// <param name="en">The original enumerator</param>
        /// <returns>The constructed default enumerator.</returns>
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public static DefaultFullResourceEnumerator AsDefaultEnumerator(this IFullResourceEnumerator en)
            => new DefaultFullResourceEnumerator(en);

        /// <summary>
        /// Gets <see cref="IResourceEnumerable.GetEnumerator"/> but defined as a <see cref="DefaultFullResourceEnumerator"/>..
        /// </summary>
        /// <param name="en">The object instance to take the <see cref="IResourceEnumerable.GetEnumerator"/> method.</param>
        /// <returns>A new <see cref="DefaultFullResourceEnumerator"/> that wraps the contents of <see cref="IResourceEnumerable.GetEnumerator"/> method.</returns>
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public static DefaultFullResourceEnumerator GetDefaultFullResourceEnumerator(this IResourceEnumerable en)
            => new DefaultFullResourceEnumerator(en.GetEnumerator());

        /// <summary>
        /// Gets <see cref="IResourceEnumerable.GetAdvancedResourceEnumerator"/> but defined as a <see cref="DefaultAdvancedResourceEnumerator"/>..
        /// </summary>
        /// <param name="en">The object instance to take the <see cref="IResourceEnumerable.GetAdvancedResourceEnumerator"/> method.</param>
        /// <returns>A new <see cref="DefaultAdvancedResourceEnumerator"/> that wraps the contents of <see cref="IResourceEnumerable.GetAdvancedResourceEnumerator"/> method.</returns>
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public static DefaultAdvancedResourceEnumerator GetDefaultAdvancedResourceEnumerator(this IAdvancedResourceEnumerator en)
            => new DefaultAdvancedResourceEnumerator(en);

        /// <summary>
        /// Gets <see cref="IResourceEnumerable.GetSimpleResourceEnumerator"/> but defined as a <see cref="DefaultSimpleResourceEnumerator"/>..
        /// </summary>
        /// <param name="en">The object instance to take the <see cref="IResourceEnumerable.GetSimpleResourceEnumerator"/> method.</param>
        /// <returns>A new <see cref="DefaultSimpleResourceEnumerator"/> that wraps the contents of <see cref="IResourceEnumerable.GetSimpleResourceEnumerator"/> method.</returns>
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public static DefaultSimpleResourceEnumerator GetDefaultSimpleResourceEnumerator(this ISimpleResourceEnumerator en)
            => new DefaultSimpleResourceEnumerator(en);

        /// <summary>
        /// Gets the first resource entry of the current resource enumerable.
        /// </summary>
        /// <param name="re">The resource enumerable to search.</param>
        /// <returns>The first resource , if at least one resource exist; otherwise , <see langword="null"/>.</returns>
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public static IResourceEntry First(this IResourceEnumerable re)
        {
            var inst = re.GetAdvancedResourceEnumerator();
            try
            {
                if (inst?.MoveNext() == true) {
                    return inst.Current;
                } else {
                    return null;
                }
            } finally { inst?.Dispose(); }
        }

        /// <summary>
        /// Gets the last resource entry of the current resource enumerable.
        /// </summary>
        /// <param name="re">The resource enumerable to search.</param>
        /// <returns>The last resource , if at least one resource exist; otherwise , <see langword="null"/>.</returns>
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public static IResourceEntry Last(this IResourceEnumerable re)
        {
            var inst = re.GetAdvancedResourceEnumerator();
            IResourceEntry entry = null;
            try
            {
                while (inst?.MoveNext() == true) { entry = inst.Current; }
                return entry;
            } finally { inst?.Dispose(); }
        }

        /// <summary>
        /// Returns the only one resource entry found in the resource enumerable. <br />
        /// If more than one or no elements were found , it throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="re">The resource enumerable to check.</param>
        /// <returns>The single element in this resource enumerable.</returns>
        /// <exception cref="InvalidOperationException">See the summary of how this exception occurs.</exception>
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public static IResourceEntry Single(this IResourceEnumerable re)
        {
            var inst = re.GetAdvancedResourceEnumerator();
            if (inst?.MoveNext() == true)
            {
                if (inst?.MoveNext() == true) { goto g_rf; }
                return inst.Current;
            }
        g_rf:
            inst?.Dispose();
            throw new InvalidOperationException("This method presumes that only ONE element is found in the collection.");
        }

        /// <summary>
        /// Returns an array that represents the found resources of this <see cref="IResourceEnumerable"/>.
        /// </summary>
        /// <param name="re">The resource enumerable to get the array from.</param>
        /// <returns>An array of resource entries.</returns>
        public static IResourceEntry[] ToArray(this IResourceEnumerable re) => ToList(re).ToArray();

        /// <summary>
        /// Returns a list that represents the found resources of this <see cref="IResourceEnumerable"/>.
        /// </summary>
        /// <param name="re">The resource enumerable to get the array from.</param>
        /// <returns>A list of resource entries.</returns>
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public static List<IResourceEntry> ToList(this IResourceEnumerable re)
        {
            var inst = re.GetAdvancedResourceEnumerator();
            List<IResourceEntry> entries = new();
            if (inst is null) { goto g_end; }
            while (inst.MoveNext()) { entries.Add(inst.Current); }
        g_end:
            return entries;
        }

        /// <summary>
        /// Gets the number of resources that are contained into the current resource enumerable.
        /// </summary>
        /// <param name="re">The resource enumerable to use.</param>
        /// <returns>The number of resources contained into this resource enumerable.</returns>
        public static System.Int64 LongCount(this IResourceEnumerable re)
        {
            var inst = re.GetAdvancedResourceEnumerator();
            System.Int64 count = 0;
            if (inst is null) { goto g_end; }
            while (inst?.MoveNext() == true && count < System.Int64.MaxValue) { count++; }
        g_end:
            return count;
        }

        /// <summary>
        /// Gets the number of resources that are contained into the current resource enumerable.
        /// </summary>
        /// <param name="re">The resource enumerable to use.</param>
        /// <returns>The number of resources contained into this resource enumerable.</returns>
        public static System.Int32 Count(this IResourceEnumerable re)
        {
            var inst = re.GetAdvancedResourceEnumerator();
            System.Int32 count = 0;
            if (inst is null) { goto g_end; }
            while (inst.MoveNext() && count < System.Int32.MaxValue) { count++; }
        g_end:
            return count;
        }

        /// <summary>
        /// Returns a collection that represents the found resources of this <see cref="IResourceEnumerable"/>.
        /// </summary>
        /// <param name="re">The resource enumerable to get the array from.</param>
        /// <returns>A collection of resource entries.</returns>
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public static Collections.ResourceCollection ToResourceCollection(this IResourceEnumerable re)
        {
            Collections.ResourceCollection Rc = new(new Collections.ResourceEntryComparer());
            IFullResourceEnumerator enumerator = re.GetEnumerator();
            if (enumerator is null) { goto g_end; }
            while (enumerator.MoveNext()) { Rc.Add(enumerator.Entry); }
        g_end:
            return Rc;
        }
    }

}