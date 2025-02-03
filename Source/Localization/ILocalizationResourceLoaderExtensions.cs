
using System.Globalization;

namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// Provides extension methods around the <see cref="ILocalizationResourceLoader"/> interface.
    /// </summary>
    public static class ILocalizationResourceLoaderExtensions
    {

        /// <summary>
        /// Gets a localized string resource from the specified resource loader , with the specified culture.
        /// </summary>
        /// <param name="loader">The resource loader to use.</param>
        /// <param name="name">The name of the resource to be found.</param>
        /// <param name="culture">The culture under this resource name defined by <paramref name="name"/> should be found.</param>
        /// <returns>The specified string resource defined by <paramref name="name"/> and <paramref name="culture"/>.</returns>
        /// <seealso cref="ILocalizationResourceLoader.GetLocalizedResource{T}(string, CultureInfo)"/>
        public static System.String GetLocalizedStringResource(this ILocalizationResourceLoader loader , System.String name , CultureInfo culture)
            => loader.GetLocalizedResource<System.String>(name, culture);

        /// <summary>
        /// Gets a localized string resource from the specified resource loader , with the specified culture. <br />
        /// The <paramref name="formatobjs"/> parameter specify the formats of this string resource to be replaced.
        /// </summary>
        /// <param name="loader">The resource loader to use.</param>
        /// <param name="name">The name of the resource to be found.</param>
        /// <param name="culture">The culture under this resource name defined by <paramref name="name"/> should be found.</param>
        /// <param name="formatobjs">The objects to be used for transforming the formatted string resource.</param>
        /// <returns>The specified string resource defined by <paramref name="name"/> and <paramref name="culture"/>.</returns>
        /// <seealso cref="ILocalizationResourceLoader.GetLocalizedResource{T}(string, CultureInfo)"/>
        public static System.String GetFormattedLocalizedStringResource(this ILocalizationResourceLoader loader , System.String name , CultureInfo culture , params System.Object[] formatobjs)
            => System.String.Format(GetLocalizedStringResource(loader , name , culture) , formatobjs);

        /// <summary>
        /// Gets a localized byte array resource. Throws <see cref="ResourceNotFoundException"/> if not found.
        /// </summary>
        /// <param name="Name">The resource name to look up.</param>
        /// <param name="ldr">The resource loader to get the resource from.</param>
        /// <param name="culture">The culture of the given resource.</param>
        /// <returns>The byte array resource defined by <paramref name="Name"/>.</returns>
        /// <exception cref="ResourceNotFoundException">The specified resource was not found.</exception>
        /// <seealso cref="ILocalizationResourceLoader.GetLocalizedResource{T}(string, CultureInfo)"/>
        public static System.Byte[] GetLocalizedByteArrayResource(this ILocalizationResourceLoader ldr, System.String Name , CultureInfo culture)
            => ldr.GetLocalizedResource<System.Byte[]>(Name, culture);

        /// <summary>
        /// Gets a localized resource boxed in a <see cref="System.Object"/> instance , from the specified localization loader.
        /// </summary>
        /// <param name="ldr">The resource loader instance.</param>
        /// <param name="Name">The resource name.</param>
        /// <param name="culture">The culture under which the resource is to be retrieved.</param>
        /// <returns>The boxed resource value.</returns>
        /// <exception cref="ResourceNotFoundException">The specified resource name was not found.</exception>
        public static System.Object GetLocalizedResource(this ILocalizationResourceLoader ldr, System.String Name , CultureInfo culture)
        {
            var en = ldr.GetLocalizedResourceEntryEnumerator(culture);
            try {
                while (en.MoveNext())
                {
                    if (en.ResourceEntry.Name == Name) { return en.ResourceEntry.Value; }
                }
            } finally { en = null; }
            throw new ResourceNotFoundException(Name);
        }

        /// <summary>
        /// Gets a resource boxed in a <see cref="System.Object"/> instance , from the specified localization loader.
        /// </summary>
        /// <param name="ldr">The resource loader instance.</param>
        /// <param name="Name">The resource name.</param>
        /// <param name="underlyingtype">The resource type for the resource to be retrieved.</param>
        /// <param name="culture">The culture under which the resource is to be retrieved.</param>
        /// <returns>The boxed resource value.</returns>
        /// <exception cref="ResourceNotFoundException">The specified resource name and type were not found.</exception>
        public static System.Object GetLocalizedResource(this ILocalizationResourceLoader ldr, System.String Name, System.Type underlyingtype , CultureInfo culture)
        {
            var en = ldr.GetEnumerator();
            try {
                while (en.MoveNext())
                {
                    if (en.ResourceEntry.Name == Name && underlyingtype == en.ResourceEntry.TypeOfValue) { return en.ResourceEntry.Value; }
                }
            } finally { en = null; }
            throw new ResourceNotFoundException(Name);
        }

    }
}