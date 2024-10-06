using System;
using System.Collections;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetResourcesExtensions.Collections;

namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// Provides an abstract derived instance of <see cref="IResourceLoader"/> for managing , using and retrieveing localizable resources. <br />
    /// </summary>
    public abstract class LocalizableResourceLoader : IResourceLoader
    {
        private sealed class PrivateResourceEntry : IResourceEntry 
        {
            private readonly System.String _name;
            private readonly System.Object _value;

            public PrivateResourceEntry(System.String name, System.Object value)
            {
                _name = name;
                _value = value;
            }

            public PrivateResourceEntry(DictionaryEntry de) : this(de.Key.ToString() , de.Value) { }

            public System.String Name => _name;

            public System.Object Value => _value;

            public System.Type TypeOfValue => _value?.GetType();
        }

        private sealed class LocalizableResourceEnumerator : IDualResourceEntryEnumerator
        {
            private LocalizedResourceReader _inst;
            private IDictionaryEnumerator resources;

            public LocalizableResourceEnumerator(LocalizedResourceReader inst, IDictionaryEnumerator indexenumerator)
            {
                _inst = inst;
                resources = indexenumerator;
            }

            public DictionaryEntry Entry => Current.AsDictionaryEntry();

            DictionaryEntry IEnumerator<DictionaryEntry>.Current => Current.AsDictionaryEntry();

            KeyValuePair<string, object> IEnumerator<KeyValuePair<string, object>>.Current => Current.AsKeyValuePair();

            public string Key => Current.Name;

            System.Object IDictionaryEnumerator.Key => Current.Name;

            public object Value => Current.Value;

            public IResourceEntry ResourceEntry => Current;

            public IResourceEntry Current
            {
                get {
                    System.String fd = resources.Key.ToString();
                    foreach (DictionaryEntry res in _inst)
                    {
                        if (res.Key.ToString() == fd) {
                            return new PrivateResourceEntry(res);
                        }
                    }
                    throw new LocalizationNotFoundException(fd, _inst.SelectedCulture);
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose() {
                _inst = null;
                resources = null;
            }

            public bool MoveNext() => resources.MoveNext();

            public void Reset() => resources.Reset();
        }

        private readonly CultureInfo invariantculture;

        /// <summary>
        /// Before the constructor exits , this resource reader must contain an index of all cultures that each resource supports. <br />
        /// Then , the readers provided in <see cref="LocalizedReaders"/> field will return the matching resource.
        /// </summary>
        protected ILocalizationIndexResourceReader BaseResourceReader;

        /// <summary>
        /// Contains a list of localized resource readers to retrieve the localized resource. <br />
        /// You can from here freely remove or add localized resource readers.
        /// </summary>
        protected IList<LocalizedResourceReader> LocalizedReaders;

        /// <summary>
        /// Constructs a new instance of <see cref="LocalizableResourceLoader"/> class with the specified culture that will be used
        /// as a fallback culture.
        /// </summary>
        /// <remarks>Note that the fallback culture can be any culture; that is any language even other than the English culture.</remarks>
        /// <param name="invariantculture">The invariant culture to be used as a failback culture.</param>
        protected LocalizableResourceLoader(CultureInfo invariantculture)
        {
            this.invariantculture = invariantculture;
        }

        /// <summary>
        /// Returns the currently selected invariant culture. <br />
        /// In case that the requested resource with the specified culture is not found , then this culture is selected before returning the resource back to user. <br />
        /// This is also used for the explicit implementation of <see cref="IResourceLoader"/> interface.
        /// </summary>
        public CultureInfo InvariantCulture => invariantculture;

        /// <summary>
        /// Disposes the <see cref="LocalizableResourceLoader"/> class.
        /// </summary>
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            BaseResourceReader?.Dispose();
            BaseResourceReader = null;
            if (LocalizedReaders is not null)
            {
                foreach (var rdr in LocalizedReaders) { rdr.Dispose(); }
                LocalizedReaders.Clear();
            }
            LocalizedReaders = null;
        }

        /// <summary>
        /// Disposes asyncronously the <see cref="LocalizableResourceLoader"/> class.
        /// </summary>
        public virtual ValueTask DisposeAsync() => new(Task.Run(Dispose));

        /// <summary>
        /// Gets a localized string resource with it's name specified in <paramref name="Name"/> parameter.
        /// </summary>
        /// <param name="Name">The resource to retrieve.</param>
        /// <param name="culture">The culture variant of the resource to be retrieved.</param>
        /// <returns>The localized resource.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="Name"/> and/or <paramref name="culture"/> were <see langword="null"/>.</exception>
        /// <exception cref="LocalizationNotFoundException">The specified resource does exist , but the culture specific version of the resource does not exist.</exception>
        /// <exception cref="LocalizedReaderNotFoundException">The requested culture for the resource was not found , because the reader does not exist.</exception>
        /// <exception cref="ResourceNotFoundException">The resource provided was not found in the registered list of resources.</exception>
        /// <exception cref="ResourceTypeMismatchException">The type of the retrieved resource was not a <see cref="System.String"/>.</exception>
        public System.String GetStringResource(System.String Name, CultureInfo culture) => GetResource<System.String>(Name, culture);

        /// <summary>
        /// Gets a localized resource of type <typeparamref name="T"/> and returns it.
        /// </summary>
        /// <typeparam name="T">The resource type to return.</typeparam>
        /// <param name="Name">The resoruce name to return.</param>
        /// <param name="culture">The resource culture which the <paramref name="Name"/> will be returned.</param>
        /// <returns>The localized resource of type <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="Name"/> and/or <paramref name="culture"/> were <see langword="null"/>.</exception>
        /// <exception cref="LocalizationNotFoundException">The specified resource does exist , but the culture specific version of the resource does not exist.</exception>
        /// <exception cref="LocalizedReaderNotFoundException">The requested culture for the resource was not found , because the reader does not exist.</exception>
        /// <exception cref="ResourceNotFoundException">The resource provided was not found in the registered list of resources.</exception>
        /// <exception cref="ResourceTypeMismatchException">The <typeparamref name="T"/> type was different than the type the resource was defined in.</exception>
        public T GetResource<T>(System.String Name, CultureInfo culture) where T : notnull
        {
            if (System.String.IsNullOrEmpty(Name)) { throw new ArgumentNullException(nameof(Name)); }
            if (culture is null) { throw new ArgumentNullException(nameof(culture)); }
            foreach (DictionaryEntry de in BaseResourceReader)
            {
                if (de.Key.Equals(Name))
                {
                    // OK. The resource does exist in the index only.
                    // Now the resource must exist in the culture list provided.
                    // If not , you get an early-bound exception for misdefinition.
                    if (CultureDoesExist(de , culture) == false) {
                        // LNFE here because of misdefinition...
                        throw new LocalizationNotFoundException(Name, culture);
                    }
                    // Now use the LoadLocalizedEntry method to get our resource...
                    DictionaryEntry lde = LoadLocalizedEntry(Name, culture);
                    if (typeof(T) != lde.Value.GetType()) {
                        // Throw RTME classicly at such case...
                        throw new ResourceTypeMismatchException(typeof(T) , lde.Value.GetType() , Name);
                    }
                    return (T)lde.Value;
                }
            }
            // If the resource was not even found , throw an original ResourceNotFoundException.
            throw new ResourceNotFoundException(Name);
        }

        private static IEnumerable<CultureInfo> GetCultures(DictionaryEntry de)
        {
            if (de.Value is System.String vd)
            {
                foreach (var c in vd.Split(new System.Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    yield return new CultureInfo(c);
                }
            }
        }

        private static System.Boolean CultureDoesExist(DictionaryEntry de , CultureInfo culture)
        {
            System.Boolean result = false;
            foreach (var ci in GetCultures(de))
            {
                if (ci.LCID == culture.LCID) { result = true; break; }
            }
            return result;
        }

        [System.Diagnostics.DebuggerHidden]
        private DictionaryEntry LoadLocalizedEntry(System.String Name , CultureInfo culture)
        {
            LocalizedResourceReader reader = null;
            foreach (var rdr in LocalizedReaders)
            {
                // The below line will be useful if we must do fallback.
                if (rdr.SelectedCulture.LCID == invariantculture.LCID) { reader = rdr; }
                if (rdr.SelectedCulture.LCID == culture.LCID)
                {
                    foreach (DictionaryEntry res in rdr)
                    {
                        if (res.Key.Equals(Name)) { return res; }
                    }
                    // If the reader has reached it's end and the resource does not exist , throw this exception.
                    throw new LocalizationNotFoundException(Name, culture);
                }
            }
            // There is still the possibility this to be null. Avoid that case.
            if (reader is null) { throw new LocalizedReaderNotFoundException(Name, invariantculture); }
            // The resource was not found , attempt to retrieve it using the invariant culture.
            foreach (DictionaryEntry res in reader)
            {
                if (res.Key.Equals(Name)) { return res; }
            }
            // If even this does not work , then we must throw the exception.
            throw new LocalizedReaderNotFoundException(Name, culture);
        }

        /// <summary>
        /// Gets an enumerator to enumerate all the resources for the specified <paramref name="culture"/>.
        /// </summary>
        /// <param name="culture">The culture to retrieve the localized resources.</param>
        /// <returns>A culture-specific enumerator.</returns>
        /// <exception cref="LocalizedReaderNotFoundException">The specified localized reader is not available.</exception>
        public Collections.IDualResourceEntryEnumerator GetEnumeratorForCulture(CultureInfo culture)
        {
            foreach (var ldr in LocalizedReaders)
            {
                if (ldr.SelectedCulture.LCID == culture.LCID) {
                    return new LocalizableResourceEnumerator(ldr , BaseResourceReader.GetEnumerator());
                }
            }
            // If the requested culture was not found , fail with LocalizedReaderNotFoundException.
            throw new LocalizedReaderNotFoundException("" , culture);
        }

        /// <inheritdoc />
        [Obsolete("This method is not any longer useful and will always throw an exception.", true)]
        public ISimpleResourceEnumerator GetSimpleResourceEnumerator() => throw new NotSupportedException("This method is deprecated and will be removed in V3.");

        /// <inheritdoc />
        [Obsolete("This method is not any longer useful and will always throw an exception.", true)]
        public IAdvancedResourceEnumerator GetAdvancedResourceEnumerator() => throw new NotSupportedException("This method is deprecated and will be removed in V3.");

        /// <inheritdoc />
        public IEnumerable<string> GetAllResourceNames()
        {
            System.String temp;
            foreach (DictionaryEntry res in BaseResourceReader)
            {
                temp = res.Key.ToString();
                if (temp != "INVARIANT") { yield return temp; }
            }
            temp = null;
        }

        /// <summary>
        /// Gets an enumerator that does only return culture-invariant resources.
        /// </summary>
        public IDualResourceEntryEnumerator GetEnumerator() => GetEnumeratorForCulture(invariantculture);

        /// <inheritdoc />
        public IEnumerable<string> GetRelativeResources(string Name)
        {
            System.String inline;
            foreach (DictionaryEntry D in BaseResourceReader)
            {
                inline = D.Key.ToString();
                if (inline == "INVARIANT") { continue; }
                if (inline.StartsWith(Name)) { yield return inline; }
                if (inline == Name) { yield return inline; }
            }
            inline = null;
        }

        T IResourceLoader.GetResource<T>(string Name) => GetResource<T>(Name, invariantculture);

        /// <inheritdoc />
        public IEnumerable<string> GetResourceNamesStartingWith(string start)
        {
            System.String temp;
            foreach (DictionaryEntry res in BaseResourceReader)
            {
                temp = res.Key.ToString();
                if (temp != "INVARIANT" && temp.StartsWith(start)) { yield return temp; }
            }
            temp = null;
        }

        System.String IResourceLoader.GetStringResource(string Name) => GetStringResource(Name, invariantculture);

        /// <summary>
        /// Disposes a localized reader that has the culture <paramref name="culture"/>. <br />
        /// This allows better memory management when a particular localized reader is not required for the lifetime of this instance.
        /// </summary>
        /// <param name="culture">The reader to dispose that manages the provided culture.</param>
        /// <returns><see langword="true"/> if disposal succeeded; otherwise , <see langword="false"/>.</returns>
        public System.Boolean DisposeLocalizedReader(CultureInfo culture) 
        {
            // Do not delete the invariant reader at any way!
            // Additionally , do not throw any exception for bad usage. Return false instead.
            if (culture.LCID == invariantculture.LCID) { return false; }
            try {
                for (System.Int32 I = 0; I < LocalizedReaders.Count; I++) 
                {
                    if (LocalizedReaders[I].SelectedCulture.LCID == culture.LCID)
                    {
                        LocalizedReaders[I].Close();
                        LocalizedReaders[I].Dispose();
                        LocalizedReaders.RemoveAt(I);
                        return true;
                    }
                }
                return false;
            } catch { 
                return false;
            }
        }
    }
}
