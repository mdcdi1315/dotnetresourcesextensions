
using System.Globalization;
using DotNetResourcesExtensions.Localization;

namespace DotNetResourcesExtensions.Collections
{
    /// <summary>
    /// Represents a localized resource entry enumerator for enumerating the resources.
    /// </summary>
    public interface ILocalizedResourceEntryEnumerator : ISpecificResourceEntryEnumerator<ILocalizedResourceEntry> { }

    /// <summary>
    /// Represents a localized resource entry enumerator with comment for enumerating the resources.
    /// </summary>
    public interface ILocalizedResourceEntryWithCommentEnumerator : ISpecificResourceEntryEnumerator<ILocalizedResourceEntryWithComment> , ILocalizedResourceEntryEnumerator { }

    /// <summary>
    /// Represents an invariant resource entry enumerator for enumerating localization index entries.
    /// </summary>
    public interface ILocalizationIndexResourceEnumerator : ISpecificResourceEntryEnumerator<ILocalizationIndexResourceEntry> 
    {
        /// <summary>
        /// Gets an enumerable of all resource entry cultures defined in the current resource entry of this enumerator
        /// instance.
        /// </summary>
        public System.Collections.Generic.IEnumerable<CultureInfo> EntryCultures { get; }
    }
}
