using System;
using System.Collections;
using System.Resources;
using System.Globalization;

namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// Represents a localized resource reader. Use it together with the <see cref="LocalizedResourceWriter"/> class.
    /// </summary>
    public abstract class LocalizedResourceReader : IResourceReader , Internal.IStreamOwnerBase
    {
        /// <summary>
        /// Gets the culture that this reader will read. 
        /// </summary>
        public abstract CultureInfo SelectedCulture { get; }

        /// <inheritdoc />
        public abstract void Close();

        /// <inheritdoc />
        public abstract void Dispose();

        /// <inheritdoc />
        public abstract System.Boolean IsStreamOwner { get; set; }

        /// <inheritdoc />
        public abstract IDictionaryEnumerator GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Represents a localized resource writer.
    /// </summary>
    public abstract class LocalizedResourceWriter : IResourceWriter , Internal.IStreamOwnerBase
    {

        /// <summary>
        /// Default constructor. Must be always called from the inheriting class.
        /// </summary>
        protected LocalizedResourceWriter() { }

        /// <summary>
        /// Adds a new localized string resource to the list of the resources to be written.
        /// </summary>
        /// <param name="Name">The resource name to write.</param>
        /// <param name="Value">The resource value to write.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="Name"/> or <paramref name="Value"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="Name"/> has invalid naming characters.</exception>
        public abstract void AddResource(System.String Name, System.String Value);

        /// <summary>
        /// Adds a new localized resource to the list of the resources to be written by using a localized resource entry.
        /// </summary>
        /// <param name="entry">The resource entry to add.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="entry"/> parameter or it's name is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">The <paramref name="entry"/> parameter has a name that is invalid , or attempted to write a resource other than a string.</exception>
        /// <exception cref="System.InvalidOperationException">The culture that the <paramref name="entry"/> defines is different of what culture expects to contain only.</exception>
        public abstract void AddLocalizableEntry(ILocalizedResourceEntry entry);

        /// <inheritdoc />
        public abstract System.Boolean IsStreamOwner { get; set; }

        /// <summary>
        /// Gets the culture that this writer will write. 
        /// </summary>
        public abstract CultureInfo SelectedCulture { get; }

        /// <inheritdoc />
        public abstract void Generate();

        /// <inheritdoc />
        public abstract void Close();

        /// <inheritdoc />
        public abstract void Dispose();

        /// <inheritdoc />
        public virtual void AddResource(string name, System.Byte[] value)
        {
            throw new NotImplementedException("For the localized readers/writers , this method is optional. Override this method so as to provide support for these cases.");
        }

        /// <inheritdoc />
        public virtual void AddResource(string name, object value)
        {
            throw new NotImplementedException("For the localized readers/writers , this method is optional. Override this method so as to provide support for these cases.");
        }
    }

}
