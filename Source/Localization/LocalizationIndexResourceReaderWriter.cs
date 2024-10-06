using System;
using System.Collections;
using System.Resources;
using System.Globalization;
using System.Collections.Generic;
using DotNetResourcesExtensions.Internal;
using DotNetResourcesExtensions.Collections;

namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// Gets a resource reader that contains all resource cultures for all resources and returns them in a resource entry. <br />
    /// The resource name is the actual resource name and the resource value contains a string which contains a colon-seperated list
    /// of all the supported cultures.
    /// </summary>
    public interface ILocalizationIndexResourceReader : IResourceReader
    {
        /// <summary>
        /// By default , all derived instances of <see cref="ILocalizationIndexResourceReader"/> must contain
        /// a resource 'INVARIANT' which does contain the invariant culture for the index reader. This field
        /// might be subsequently used as the invariant culture in the <see cref="LocalizableResourceLoader"/> class.
        /// </summary>
        /// <remarks>Note that this culture is not needed to be included in the list in each resource.</remarks>
        public CultureInfo SelectedInvariantCulture { get; }

        /// <summary>
        /// Gets a new index resource enumerator to enumerate all possible cultures for each defined index resource entry.
        /// </summary>
        /// <returns>A index resource enumerator.</returns>
        public new ILocalizationIndexResourceEnumerator GetEnumerator();
    }

    /// <summary>
    /// Provides an abstract resource reader for reading localized resource indexes in the specified reader. <br />
    /// Note that this class must be preffered instead of <see cref="ILocalizationIndexResourceReader"/> interface
    /// because it performs a nicer dev interface plus it validates a localization index layout.
    /// </summary>
    public abstract class LocalizationIndexResourceReader : ILocalizationIndexResourceReader
    {
        private CultureInfo invculture;
        private IResourceReader reader;

        /// <summary>
        /// Initialize the localization index reader from the specified resource reader.
        /// </summary>
        /// <param name="reader">The resource reader to use and will act as a localization index reader.</param>
        protected LocalizationIndexResourceReader(IResourceReader reader)
        {
            this.reader = reader;
            invculture = null;
        }

        private sealed class Enumerator : ILocalizationIndexResourceEnumerator
        {
            private CultureInfo invc;
            private IDictionaryEnumerator original;

            private sealed class LocalizationIndexEntry : ILocalizationIndexResourceEntry
            {
                private System.String name, value;

                public LocalizationIndexEntry(IResourceEntry ent)
                {
                    name = ent.Name;
                    value = ent.Value as System.String;
                }

                public LocalizationIndexEntry(System.String Name , System.Object value)
                {
                    name = Name;
                    this.value = value as System.String;
                }

                public string Name => name;

                public object Value => value;

                public Type TypeOfValue => typeof(System.String);
            }

            public Enumerator(CultureInfo invariant, IDictionaryEnumerator original)
            {
                this.invc = invariant;
                this.original = original;
            }

            public IEnumerable<CultureInfo> EntryCultures
            {
                get {
                    yield return invc;
                    foreach (CultureInfo item in ResourceEntry.GetCultures())
                    {
                        yield return item;
                    }
                }
            }

            public ILocalizationIndexResourceEntry ResourceEntry => new LocalizationIndexEntry(original.Key.ToString() , original.Value);

            public object Key => ResourceEntry.Name;

            public object Value => ResourceEntry.Value;

            public DictionaryEntry Entry => new(ResourceEntry.Name , EntryCultures);

            public object Current => ResourceEntry;

            IResourceEntry IResourceEntryEnumerator.ResourceEntry => ResourceEntry;

            public bool MoveNext() => original.MoveNext();

            public void Reset() => original.Reset();
        }

        /// <inheritdoc />
        public CultureInfo SelectedInvariantCulture => invculture;

        /// <inheritdoc />
        public void Close() => reader?.Close();

        /// <summary>
        /// Disposes this localization index reader.
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Disposes the unmanaged resources , optionally disposing and all managed resources.
        /// </summary>
        /// <param name="disposing">A value whether to dispose and all managed resources.</param>
        protected virtual void Dispose(System.Boolean disposing)
        {
            if (disposing)
            {
                reader?.Dispose();
                reader = null;
                invculture = null;
            }
        }

        /// <summary>
        /// Validates the layout of the selected localization index resource reader.
        /// This must be done in construction-time.
        /// </summary>
        protected void Validate()
        {
            IDictionaryEnumerator de = reader.GetEnumerator();
            while (de.MoveNext()) 
            {
                if (de.Entry.Key.ToString() == "INVARIANT") { 
                    try { invculture = new CultureInfo(de.Entry.ToString()); } catch { }
                } else if (de.Entry.Value is not System.String) { 
                    throw new InvalidLocalizationIndexReaderLayout(reader.GetType(), $"the resource {de.Key} is not a string resource that contains cultures"); 
                }
            }
            de = null;
        }

        /// <inheritdoc />
        public ILocalizationIndexResourceEnumerator GetEnumerator() => new Enumerator(invculture , reader.GetEnumerator());

        IDictionaryEnumerator IResourceReader.GetEnumerator() => reader.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => reader.GetEnumerator();

        /// <summary>
        /// Finalizer to ensure that the reader will be successfully disposed.
        /// </summary>
        ~LocalizationIndexResourceReader() => Dispose(true);
    }

    /// <summary>
    /// Provides an abstract resource writer for writing localized resource indexes in the specified writer.
    /// </summary>
    public abstract class LocalizationIndexResourceWriter : IResourceWriter
    {
        private CultureInfo invculture;
        private System.Boolean invculturemodifidable;
        private readonly IResourceWriter basewr;

        private LocalizationIndexResourceWriter()
        {
            invculturemodifidable = true;
            invculture = CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LocalizationIndexResourceWriter"/> class with the specified underlying writer to use.
        /// </summary>
        /// <param name="writer">The writer where all the localized resources will be written to.</param>
        protected LocalizationIndexResourceWriter(IResourceWriter writer) : this()
        {
            basewr = writer;
        }

        /// <summary>
        /// Gets or sets the invariant culture for this reader. This value is written and cannot be modified after 
        /// <see cref="Generate()"/> is called.
        /// </summary>
        /// <remarks>By default , this property gets the value of <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public CultureInfo SelectedInvariantCulture => invculture;

        /// <summary>
        /// Writes or flushes all the resources in the underlying resource writer.
        /// </summary>
        public void Generate()
        {
            if (invculturemodifidable)
            {
                basewr?.AddResource("INVARIANT", invculture.NativeName);
                invculturemodifidable = false;
            }
            basewr?.Generate();
        }

        /// <inheritdoc />
        public void Close() => basewr?.Close();

        /// <summary>
        /// Adds to the resource index the specified resource with the <paramref name="Name"/> and the localizable
        /// <paramref name="cultures"/> provided.
        /// </summary>
        /// <param name="Name">The resource name which the provided cultures will be referenced.</param>
        /// <param name="cultures"></param>
        /// <exception cref="System.ArgumentNullException">Either <paramref name="Name"/> or <paramref name="cultures"/> were null.</exception>
        /// <exception cref="System.ArgumentException">The <paramref name="cultures"/> array must contain at least one culture.</exception>
        public void AddResource(System.String Name , params CultureInfo[] cultures)
        {
            ParserHelpers.ValidateName(Name);
            if (cultures is null) { throw new System.ArgumentNullException(nameof(cultures)); }
            System.String result = System.String.Empty;
            if (invculturemodifidable) {
                foreach (CultureInfo culture in cultures)
                {
                    result += culture.Name + ";";
                }
            } else {
                foreach (CultureInfo culture in cultures) {
                    if (culture.DisplayName == invculture.DisplayName) { continue; }
                    result += culture.Name + ";";
                }
            }
            if (System.String.IsNullOrEmpty(result)) { throw new System.ArgumentException("At least one culture must be contained in cultures array." , nameof(cultures)); }
            basewr.AddResource(Name, result);
        }

        void IResourceWriter.AddResource(string name, byte[] value) => throw new System.NotSupportedException("This operation is not supported for localizable index writers.");

        void IResourceWriter.AddResource(string name, object value)
        {
            if (value is CultureInfo[] cultures) {
                AddResource(name, cultures);
            }
            throw new System.NotSupportedException("This operation is not supported for localizable index writers.");
        }

        void IResourceWriter.AddResource(string name, string value) => throw new System.NotSupportedException("This operation is not supported for localizable index writers.");

        /// <summary>
        /// Disposes the current resource writer and the underlying writer too (due to the fact that is a strong convention).
        /// </summary>
        public void Dispose() {
            Generate();
            Close();
            basewr?.Dispose();
            invculture = null;
            GC.SuppressFinalize(this);
        }
    }
}