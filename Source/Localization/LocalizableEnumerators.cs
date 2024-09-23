
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

}
