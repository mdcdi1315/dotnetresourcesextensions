
using System;
using System.Collections;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetResourcesExtensions.Collections;

namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// Specifies the default implementation class for connecting instances of <see cref="LocalizedResourceReader"/> classes and the
    /// <see cref="ILocalizationResourceLoader"/> interface.
    /// </summary>
    public abstract class DefaultLocalizationResourceLoader : ILocalizationResourceLoader
    {
        private sealed class LocalDualResourceEntryEnumerator : AbstractSimpleDualResourceEntryEnumerator
        {
            private BaseLocalizedResourceEntryEnumerator ent;

            public LocalDualResourceEntryEnumerator(BaseLocalizedResourceEntryEnumerator ent)
            {
                this.ent = ent;
            }

            public override IResourceEntry ResourceEntry => ent.ResourceEntry;

            public override bool MoveNext() => ent.MoveNext();

            public override void Reset() => ent.Reset();
        }

        private List<LocalizedResourceReader> readers;
        private CultureInfo ic;

        /// <summary>
        /// Creates a default instance of the <see cref="DefaultLocalizationResourceLoader"/> class from the specified localized readers.
        /// </summary>
        /// <param name="readers">The localized readers to initiate the loader from.</param>
        /// <exception cref="System.ArgumentException">No readers were registered.</exception>
        protected DefaultLocalizationResourceLoader(IEnumerable<LocalizedResourceReader> readers) : this()
        {
            Initialize(readers);
        }

        /// <summary>
        /// Creates a default instance of the <see cref="DefaultLocalizationResourceLoader"/> class.
        /// </summary>
        protected DefaultLocalizationResourceLoader() {
            readers = null;
            ic = null;
        }

        /// <summary>
        /// Initializes a default instance of the <see cref="DefaultLocalizationResourceLoader"/> class from the specified localized readers. <br />
        /// Should be used when the constructor alternative is undesired.
        /// </summary>
        /// <param name="readers">The localized readers to initiate the loader from.</param>
        /// <exception cref="System.ArgumentException">No readers were registered.</exception>
        [System.Diagnostics.DebuggerHidden]
        protected void Initialize(IEnumerable<LocalizedResourceReader> readers)
        {
            this.readers = new(readers);
            if (this.readers.Count == 0) { throw new System.ArgumentException("No localized readers were registered at all!"); }
            foreach (var reader in this.readers) 
            {
                if (reader.LocalizationEntryData.Flags.HasFlag(LocalizationSpecialTypeFlags.IsFallbackCulture))
                {
                    ic = reader.LocalizationEntryData.Culture;
                    break;
                }
            }
            ic ??= this.readers[0].LocalizationEntryData.Culture;
        }

        /// <inheritdoc />
        public CultureInfo InvariantResourceLoading 
        {
            get => ic; 
            set { 
                if (value is null) { return; }
                foreach (var culture in DefinedCultures)
                {
                    if (value.LCID == culture.LCID) { ic = value; break; }
                }
            } 
        }

        /// <inheritdoc />
        public IEnumerable<CultureInfo> DefinedCultures
        {
            get {
                foreach (var rdr in readers)
                {
                    yield return rdr.LocalizationEntryData.Culture;
                }
            }
        }

        /// <summary>
        /// Gets the currently defined culture-invariant reader.
        /// </summary>
        protected LocalizedResourceReader InvariantReader
        {
            get {
                foreach (var sp in readers)
                {
                    if (sp.LocalizationEntryData.Culture.LCID == ic.LCID)
                    {
                        return sp;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Disposes any data held and allocated by the <see cref="DefaultLocalizationResourceLoader"/> class.
        /// </summary>
        public void Dispose()
        {
            if (readers is not null)
            {
                foreach (var sp in readers)
                {
                    sp.Dispose();
                }
                readers.Clear();
                readers = null;
            }
            ic = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes any data held and allocated asyncronously by the <see cref="DefaultLocalizationResourceLoader"/> class.
        /// </summary>
        public ValueTask DisposeAsync() => new(Task.Run(Dispose));

        /// <inheritdoc />
        [Obsolete("This method is not any longer useful and will always throw an exception.", true)]
        public ISimpleResourceEnumerator GetSimpleResourceEnumerator() => throw new NotSupportedException("This method is deprecated and will be removed in V3.");

        /// <inheritdoc />
        [Obsolete("This method is not any longer useful and will always throw an exception.", true)]
        public IAdvancedResourceEnumerator GetAdvancedResourceEnumerator() => throw new NotSupportedException("This method is deprecated and will be removed in V3.");

        /// <inheritdoc />
        public IEnumerable<string> GetAllResourceNames()
        {
            foreach (DictionaryEntry de in InvariantReader)
            {
                yield return de.Key.ToString();
            }
        }

        /// <inheritdoc />
        public IDualResourceEntryEnumerator GetEnumerator() => new LocalDualResourceEntryEnumerator(InvariantReader.GetEnumerator());

        /// <inheritdoc />
        public T GetLocalizedResource<T>(string Name, CultureInfo culture) where T : notnull
        {
            if (System.String.IsNullOrEmpty(Name)) { throw new ArgumentNullException(nameof(Name)); }
            if (culture is null) { throw new ArgumentNullException(nameof(culture)); }
            foreach (var sp in readers)
            {
                if (sp.LocalizationEntryData.Culture.LCID == culture.LCID)
                {
                    // Culture was found , try to return the localized resource.
                    foreach (DictionaryEntry retent in sp)
                    {
                        if (retent.Key.ToString() == Name) 
                        { 
                            if (retent.Value.GetType() != typeof(T)) { throw new ResourceTypeMismatchException(typeof(T) , retent.Value.GetType() , Name); }
                            return (T)retent.Value; 
                        }
                    }
                    throw new LocalizationNotFoundException(Name, culture);                    
                }
            }
            throw new ResourceNotFoundException(Name);
        }

        /// <inheritdoc />
        public BaseLocalizedResourceEntryEnumerator GetLocalizedResourceEntryEnumerator(CultureInfo culture)
        {
            foreach (var sp in readers) 
            {
                if (sp.LocalizationEntryData.Culture.LCID == culture.LCID)
                {
                    return sp.GetEnumerator();
                }
            }
            return InvariantReader.GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerable<string> GetRelativeResources(string Name)
        {
            System.String inline;
            foreach (DictionaryEntry D in InvariantReader)
            {
                inline = D.Key.ToString();
                if (inline.StartsWith(Name)) { yield return inline; }
                if (inline == Name) { yield return inline; }
            }
        }

        /// <inheritdoc />
        public T GetResource<T>(string Name) where T : notnull => GetLocalizedResource<T>(Name, ic);

        /// <inheritdoc />
        public IEnumerable<string> GetResourceNamesStartingWith(string start)
        {
            foreach (DictionaryEntry dg in InvariantReader)
            {
                if (dg.Key.ToString().StartsWith(start)) { yield return dg.Key.ToString(); }
            }
        }

        /// <inheritdoc />
        public virtual string GetStringResource(string Name) => GetResource<System.String>(Name);

        /// <summary>
        /// Default implementation to call Dispose when the instance is casted to an IDisposable.
        /// </summary>
        void IDisposable.Dispose() => Dispose();

        /// <summary>
        /// Default implementation to call DisposeAsync when the instance is casted to an IAsyncDisposable.
        /// </summary>
        ValueTask IAsyncDisposable.DisposeAsync() => DisposeAsync();
    }
}