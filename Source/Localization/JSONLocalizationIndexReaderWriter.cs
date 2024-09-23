using System;
using System.Collections;
using System.Globalization;

namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// Represents a localization index reader implementation which gets it's functionality from <see cref="JSONResourcesReader"/> class.
    /// </summary>
    public sealed class JSONLocalizationIndexReader : ILocalizationIndexResourceReader
    {
        private CultureInfo culture;
        private JSONResourcesReader index;
        
        /// <summary>
        /// Creates a new <see cref="JSONLocalizationIndexReader"/> class from the specified file on disk.
        /// </summary>
        /// <param name="file">The file path to create the reader from.</param>
        public JSONLocalizationIndexReader(System.String file) { index = new JSONResourcesReader(file); Init(); }

        /// <summary>
        /// Creates a new <see cref="JSONLocalizationIndexReader"/> class from the specified data stream.
        /// </summary>
        /// <param name="stream">The stream to create the reader from.</param>
        public JSONLocalizationIndexReader(System.IO.Stream stream) { index = new JSONResourcesReader(stream); Init(); }

        /// <summary>
        /// The enumerator implementation for the <see cref="JSONLocalizationIndexReader"/>.
        /// </summary>
        public sealed class Enumerator : IDictionaryEnumerator
        {
            private JSONResourcesEnumerator enumerator;

            internal Enumerator(JSONResourcesEnumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            /// <inheritdoc />
            public DictionaryEntry Entry => enumerator.Entry;
            
            /// <inheritdoc />
            public object Key => enumerator.Key;

            /// <inheritdoc />
            public object? Value => enumerator.Value;

            /// <inheritdoc />
            public object Current => enumerator.Current;

            /// <inheritdoc />
            public bool MoveNext() => enumerator.MoveNext();

            /// <inheritdoc />
            public void Reset() => enumerator.Reset();
        }

        private void Init() {
            foreach (DictionaryEntry de in index)
            {
                if (de.Key.ToString() == "INVARIANT")
                {
                    culture = new(de.Value.ToString());
                }
            }
            if (culture is null) { throw new InvalidLocalizedReaderLayoutException(typeof(JSONLocalizationIndexReader)); }
        }

        /// <inheritdoc/>
        public CultureInfo SelectedInvariantCulture => culture;

        /// <inheritdoc />
        public void Close() => index?.Close();

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            index?.Dispose();
            index = null;
            culture = null;
        }

        /// <inheritdoc cref="System.Resources.IResourceReader.GetEnumerator"/>
        public Enumerator GetEnumerator() => new(index.GetEnumerator());

        IDictionaryEnumerator System.Resources.IResourceReader.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
