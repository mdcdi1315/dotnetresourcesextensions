using System;
using System.Resources;
using System.Collections;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// Represents the base class for all localized resource readers.
    /// </summary>
    public abstract class LocalizedResourceReader : IDotNetResourcesExtensionsReader
    {
        /// <summary>
        /// Declares whether the current class instance has control over the input resource.
        /// </summary>
        protected System.Boolean strmown;

        /// <summary>
        /// Creates a default instance of the <see cref="LocalizedResourceReader"/> class.
        /// </summary>
        protected LocalizedResourceReader() { strmown = false; }

        /// <summary>
        /// Defines the loading behavior of this reader , when a
        /// resource loader loads this instance into the localization pool.
        /// </summary>
        public abstract LocalizationEntry LocalizationEntryData { get; }
        
        /// <inheritdoc />
        public bool IsStreamOwner { get => strmown; set => strmown = value; }

        /// <inheritdoc />
        public abstract void Close();

        /// <inheritdoc />
        public abstract void Dispose();

        /// <summary>
        /// Returns a special enumerator that is able to process multiple localized resources efficiently.
        /// </summary>
        /// <returns>A new derived class instance of <see cref="Collections.BaseLocalizedResourceEntryEnumerator"/> class.</returns>
        public abstract Collections.BaseLocalizedResourceEntryEnumerator GetEnumerator();

        /// <inheritdoc />
        /// <remarks>Note that it is not required to implement this method in your code; That's why it is marked as virtual.</remarks>
        public virtual void RegisterTypeResolver(ITypeResolver resolver) { }

        IDictionaryEnumerator IResourceReader.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Represents the base class for all localized resource writers.
    /// </summary>
    public abstract class LocalizedResourceWriter : IDotNetResourcesExtensionsWriter
    {
        /// <summary>
        /// Declares whether the current class instance has control over the input resource.
        /// </summary>
        protected System.Boolean strmown;

        /// <summary>
        /// Creates a default instance of the <see cref="LocalizedResourceReader"/> class.
        /// </summary>
        protected LocalizedResourceWriter() { strmown = false; }

        /// <summary>
        /// Defines the loading behavior of this writer. This is used when a
        /// resource loader loads the reader into the localization pool.
        /// </summary>
        public abstract LocalizationEntry LocalizationEntryData { get; set; }

        /// <inheritdoc />
        public bool IsStreamOwner { get => strmown; set => strmown = value; }

        /// <inheritdoc />
        public abstract void AddResource(string name, string value);

        /// <inheritdoc />
        public abstract void AddResource(string name, object value);

        /// <inheritdoc />
        public abstract void AddResource(string name, byte[] value);

        /// <inheritdoc />
        public abstract void Close();

        /// <inheritdoc />
        /// <exception cref="LocalizationUndefinedException">A valid localization target was not defined.</exception>
        public abstract void Generate();

        /// <inheritdoc />
        public virtual void RegisterTypeResolver(ITypeResolver resolver) { }

        /// <summary>
        /// Override this method in your class code so as to provide disposal routines.
        /// </summary>
        /// <param name="disposing"></param>
        protected abstract void Dispose(System.Boolean disposing);

        /// <summary>
        /// Disposes all the used resources utilized by the <see cref="LocalizedResourceWriter"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
