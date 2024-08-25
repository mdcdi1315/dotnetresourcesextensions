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
}
