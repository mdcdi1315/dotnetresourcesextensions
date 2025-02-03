
using System;
using System.Collections;
using DotNetResourcesExtensions.Localization;

namespace DotNetResourcesExtensions.Collections
{
    /// <summary>
    /// Defines a resource entry enumerator suitable for enumerating localized resources.
    /// </summary>
    public abstract class BaseLocalizedResourceEntryEnumerator : IDictionaryEnumerator, ISpecificResourceEntryEnumerator<ILocalizedResourceEntry>
    {
        /// <summary>
        /// Creates a new default derived instance of <see cref="BaseLocalizedResourceEntryEnumerator"/> class.
        /// </summary>
        protected BaseLocalizedResourceEntryEnumerator() { }

        /// <inheritdoc />
        public object Key => Entry.Key;

        /// <inheritdoc />
        public object Value => Entry.Value;

        /// <inheritdoc />
        public DictionaryEntry Entry => ResourceEntry.AsDictionaryEntry();

        /// <inheritdoc />
        public object Current => Entry;

        /// <summary>
        /// Returns the resource entry object that implements the <see cref="ILocalizedResourceEntry"/>
        /// interface at the current position of the enumerator.
        /// </summary>
        public abstract ILocalizedResourceEntry ResourceEntry { get; }

        IResourceEntry IResourceEntryEnumerator.ResourceEntry => ResourceEntry;

        /// <inheritdoc />
        public abstract bool MoveNext();

        /// <inheritdoc />
        public abstract void Reset();
    }
}
