using System.Globalization;

namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// Specifies a localized resource entry enumerator for localization cases.
    /// </summary>
    public interface ILocalizedResourceEntryEnumerator : Collections.ISpecificResourceEntryEnumerator<ILocalizableResourceEntry>
    {
        /// <summary>
        /// Gets the localized resource entry for the current position in the enumerator. <br />
        /// If the <paramref name="culture"/> specified is not contained in <see cref="ILocalizableResourceEntry.Cultures"/> , 
        /// then the <see cref="ILocalizableResourceEntry.InvariantCulture"/> is used instead.
        /// </summary>
        /// <param name="culture">The localized version of the current resource to retrieve.</param>
        /// <returns>The localized resource entry.</returns>
        /// <exception cref="ResourceNotFoundException">The resource does not even exist in <see cref="ILocalizableResourceEntry.InvariantCulture"/>.</exception>
        public ILocalizedResourceEntry GetLocalizedResourceEntry(CultureInfo culture);
    }
}
