
using System.Collections.Generic;
using System.Globalization;

namespace DotNetResourcesExtensions.Localization
{

    /// <summary>
    /// This extended version of the <see cref="IResourceLoader"/> interface is capable of loading multiple localized resources specified by many readers.
    /// </summary>
    public interface ILocalizationResourceLoader : IResourceLoader
    {
        /// <summary>
        /// Specifies the resource reader that provides the invariant culture resources. <br />
        /// This value may be changed at any time; it just specifies to the resource loader what it should load. <br />
        /// This value is used , for example , by the inherited <see cref="IResourceLoader.GetResource{T}(string)"/> method.
        /// </summary>
        public CultureInfo InvariantResourceLoading { get; set; }

        /// <summary>
        /// Gets a list of the current cultures that are supported to be loaded by this instance.
        /// </summary>
        public IEnumerable<CultureInfo> DefinedCultures { get; }

        /// <summary>
        /// Gets a localized resource of type <typeparamref name="T"/> and the culture defined in <paramref name="culture"/> parameter.
        /// </summary>
        /// <typeparam name="T">The type of the resource to be loaded.</typeparam>
        /// <param name="Name"></param>
        /// <param name="culture"></param>
        /// <returns>The specified resource defined by <paramref name="Name"/> parameter , of the specified <paramref name="culture"/>.</returns>
        /// <seealso cref="IResourceLoader.GetResource{T}(string)"/>
        /// <exception cref="ResourceNotFoundException">The specified resource was not found.</exception>
        /// <exception cref="ResourceTypeMismatchException">The specified resource does not have as a type the type passed in <typeparamref name="T"/>.</exception>
        /// <exception cref="LocalizationNotFoundException">The specified resource name does exist , but it is undefined in the current culture.</exception>
        public T GetLocalizedResource<T>(System.String Name, CultureInfo culture) where T : notnull;

        /// <summary>
        /// Gets a localized resource enumerator for the culture defined in <paramref name="culture"/> parameter. <br />
        /// If the culture specified by <paramref name="culture"/> parameter does not exist in the current instance , 
        /// the resource loader must return the currently defined invariant culture.
        /// </summary>
        /// <param name="culture">The culture to be searched.</param>
        /// <returns>The localized resource entry enumerator for culture <paramref name="culture"/>.</returns>
        public Collections.BaseLocalizedResourceEntryEnumerator GetLocalizedResourceEntryEnumerator(CultureInfo culture);
    }

}