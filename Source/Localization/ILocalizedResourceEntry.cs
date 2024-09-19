using System.Globalization;

namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// This is a resource entry implementation that has been already localized.
    /// </summary>
    public interface ILocalizedResourceEntry : IResourceEntry
    {
        /// <summary>
        /// Gets the culture of the resource that this <see cref="ILocalizedResourceEntry"/> contains.
        /// </summary>
        public CultureInfo Culture { get; }
    }

    /// <summary>
    /// This is a resource entry implementation with a comment that has been already localized. <br />
    /// Note that the comment is culture-invariant; which it means that it does not been affected by the localization API's.
    /// </summary>
    public interface ILocalizedResourceEntryWithComment : ILocalizedResourceEntry , IResourceEntryWithComment { }
}
