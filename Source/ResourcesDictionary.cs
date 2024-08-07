using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Collections
{

    /// <summary>
    /// Resource collection class. <br />
    /// It is a collection class that is based on <see cref="Memory{T}"/> API's for better resource pooling 
    /// and defines methods for writing the resources to resource writers.
    /// </summary>
    public sealed class ResourceCollection : ICollection<IResourceEntry> , IResourceEntryEnumerable
    {
        private sealed class DefaultResEntry : IResourceEntry
        {
            private readonly System.String _name;
            private readonly System.Object _value;

            public DefaultResEntry(System.String name, System.Object value)
            {
                _name = name;
                _value = value;
            }

            public System.String Name => _name;

            public System.Object Value => _value;

            public System.Type TypeOfValue => _value?.GetType();
        }


        private System.Int32 _index;
        private IComparer<IResourceEntry> _comparer;
        private System.Memory<IResourceEntry> entries;

        /// <summary>
        /// Creates a new instance of the <see cref="ResourceCollection"/> class.
        /// </summary>
        public ResourceCollection()
        {
            _index = 0;
            _comparer = new ResourceEntryComparer();
            entries = System.Memory<IResourceEntry>.Empty;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ResourceCollection"/> class from the specified entries.
        /// </summary>
        /// <param name="ents">The entries to include into the newly created instance.</param>
        public ResourceCollection(IEnumerable<IResourceEntry> ents) : this()
        {
            IResourceEntry[] t = ents.ToArray();
            entries = new(t , 0 , t.Length);
            t = null;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ResourceCollection"/> class using the specified
        /// <see cref="IComparer{T}"/> instance for comparing the entries.
        /// </summary>
        /// <param name="comparer">The entries comparer to use.</param>
        public ResourceCollection(IComparer<IResourceEntry> comparer) : this()
        {
            _comparer = comparer;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ResourceCollection"/> class from the specified entries , 
        /// and using the specified <see cref="IComparer{T}"/> instance for comparing the entries.
        /// </summary>
        /// <param name="ents">The entries to include into the newly created instance.</param>
        /// <param name="cmp">The entries comparer to use.</param>
        public ResourceCollection(IEnumerable<IResourceEntry> ents , IComparer<IResourceEntry> cmp) : this()
        {
            IResourceEntry[] t = ents.ToArray();
            entries = new(t, 0, t.Length);
            t = null;
            _comparer = cmp;
        }

        /// <summary>
        /// Enumerator implementation for the <see cref="ResourceCollection"/> class.
        /// </summary>
        public struct Enumerator : IEnumerator<IResourceEntry>
        {
            private System.Int32 idx;
            private ResourceCollection ents;

            /// <summary>
            /// Creates a default instance of the <see cref="Enumerator"/> structure.
            /// </summary>
            public Enumerator() { idx = -1; ents = null; }

            internal Enumerator(ResourceCollection ents) : this() => this.ents = ents;

            /// <inheritdoc />
            public readonly IResourceEntry Current => ents[idx];

            readonly object IEnumerator.Current => Current;

            /// <inheritdoc />
            public void Dispose() { Reset(); }

            /// <inheritdoc />
            public bool MoveNext()
            {
                idx++;
                return idx < ents.Count;
            }

            /// <inheritdoc />
            public void Reset() { idx = -1; ents = null; }
        }

        /// <inheritdoc />
        public int Count => _index == 0 ? 0 : entries.Length;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        private void EnsureCapacity(System.Int32 cap)
        {
            if (entries.Length < cap)
            {
                IResourceEntry[] tmp = new IResourceEntry[cap];
                System.Array.ConstrainedCopy(entries.ToArray(), 0, tmp, 0, entries.Length);
                entries = new(tmp , 0 , tmp.Length);
                tmp = null;
            }
        }

        private System.Int32 Find(IResourceEntry ent)
        {
            System.Int32 I = 0;
            IResourceEntry tent;
            while (I < entries.Length)
            {
                tent = entries.Span[I];
                if (_comparer.Compare(tent, ent) == 0) { return I; }
                I++;
            }
            return -1;
        }

        private void Recreate()
        {
            IResourceEntry temp;
            IResourceEntry[] tents = new IResourceEntry[entries.Length];
            System.Int32 I = 0, J = 0;
            for (; I < tents.Length; I++)
            {
                temp = entries.Span[I];
                if (temp != null)
                {
                    tents[J] = temp;
                    J++;
                }
            }
            entries = new(tents, 0, J);
            tents = null;
        }

        /// <inheritdoc />
        public void Add(IResourceEntry item)
        {
            EnsureCapacity(Count + 1);
            entries.Span[_index] = item;
            _index++;
        }

        /// <summary>
        /// Puts the specified resource name and resource value to an entry and then is added to the collection.
        /// </summary>
        /// <param name="Name">The resource name.</param>
        /// <param name="Value">The resource value.</param>
        public void Add(System.String Name , System.Object Value) => Add(new DefaultResEntry(Name , Value));

        /// <summary>
        /// Clears the contents of this <see cref="ResourceCollection"/>. <br />
        /// Note that this method does not completely invalidate the internal array; it just
        /// clears the saved data within. <br /> To ensure that the internal array
        /// is completely invalidated , use the <see cref="FullClear"/> method.
        /// </summary>
        public void Clear()
        {
            entries.Span.Clear();
            _index = 0;
        }

        /// <summary>
        /// Clears the contents of this <see cref="ResourceCollection"/> , but also
        /// invalidates the internal array for less memory impact as possible.
        /// </summary>
        public void FullClear()
        {
            Clear();
            entries = System.Memory<IResourceEntry>.Empty;
        }

        /// <inheritdoc />
        public bool Contains(IResourceEntry item) => Find(item) != -1;

        /// <inheritdoc />
        public void CopyTo(IResourceEntry[] array, int arrayIndex)
        {
            if (array == null) { throw new ArgumentNullException(nameof(array)); }
            if (arrayIndex < 0) { throw new ArgumentOutOfRangeException(nameof(arrayIndex) , "The array index must be greater or equal to zero."); }
            if (array.Length - (arrayIndex + 1) < Count) { throw new ArgumentException("Given the current array situation and the given array index, the source array is not large enough to fit all the elements contained in this ResourceCollection." , nameof(array)); }
            for (System.Int32 I = 0, J = arrayIndex; I < entries.Length; I++ , J++)
            {
                array[J] = entries.Span[I];
            }
        }

        /// <summary>
        /// Copies all the current resources of this <see cref="ResourceCollection"/> to the resource 
        /// writer specified in <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The writer to copy the resources to.</param>
        public void CopyTo(System.Resources.IResourceWriter writer)
        {
            System.Int32 I = 0;
            IResourceEntry ent;
            while (I < entries.Length)
            {
                ent = entries.Span[I];
                switch (ent.Value)
                {
                    case System.String di:
                        writer.AddResource(ent.Name, di);
                        break;
                    case System.Byte[] dg:
                        writer.AddResource(ent.Name, dg);
                        break;
                    default:
                        writer.AddResource(ent.Name, ent.Value);
                        break;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerator<IResourceEntry> GetEnumerator() => new Enumerator(this);

        /// <inheritdoc />
        public bool Remove(IResourceEntry item)
        {
            System.Int32 pos = Find(item);
            entries.Span[pos] = null;
            Recreate();
            return pos != -1;
        }

        /// <summary>
        /// Removes the first occurence of the specified resource name and value from the <see cref="ICollection{T}"/>. <br />
        /// The resource name and value given must exist inside the collection as an <see cref="IResourceEntry"/>; otherwise , 
        /// this method will fail.
        /// </summary>
        /// <param name="Name">The resource name which is to be removed.</param>
        /// <param name="obj">The resource value which is to be removed.</param>
        /// <returns><see langword="true"/> if the specified resource name and value were successfully removed from the <see cref="ResourceCollection"/>;
        /// otherwise, <see langword="false"/>. This method also returns <see langword="false"/> if the specified resource name and value is not found in the
        /// original <see cref="ResourceCollection"/>.</returns>
        public System.Boolean Remove(System.String Name, System.Object obj)
            => Remove(new DefaultResEntry(Name , obj));

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IResourceEntryEnumerator GetResourceEntryEnumerator() => new ResourceEntryEnumerator(GetEnumerator());

        /// <summary>
        /// Gets or sets the resource entry at the specified index pointed by the <paramref name="index"/> parameter.
        /// </summary>
        /// <param name="index">The index of the resource collection to retrieve.</param>
        /// <returns>The resource entry at <paramref name="index"/>.</returns>
        public IResourceEntry this[System.Int32 index] { get => entries.Span[index]; set => entries.Span[index] = value; }
    }

}