using System;
using System.Globalization;
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// Dummy derived interface from <see cref="IResourceEntry"/> class to define
    /// a localization resource entry that defines all the cultures (known as localization index). <br />
    /// To work with cultures and validate information , use the provided extension methods.
    /// </summary>
    public interface ILocalizationIndexResourceEntry : IResourceEntry { }

    /// <summary>
    /// Provides extensions for <see cref="ILocalizationIndexResourceEntry"/> interface.
    /// </summary>
    public static class ILocalizationIndexResourceEntryExtensions
    {
        /// <summary>
        /// Checks whether this localization index entry is valid. <br />
        /// If <paramref name="entry"/> is null , then this method returns false.
        /// </summary>
        /// <param name="entry">The localization index entry to test.</param>
        /// <returns>A value whether <paramref name="entry"/> is a valid localization index entry.</returns>
        public static System.Boolean IsValidLocalizationEntry(this ILocalizationIndexResourceEntry entry) => entry is not null && entry.Value is System.String;

        /// <summary>
        /// Gets all the available cultures that the given entry is currently defined into.
        /// </summary>
        /// <param name="entry">The entry to get the cultures from.</param>
        /// <returns>A culture list.</returns>
        /// <exception cref="ArgumentException"><paramref name="entry"/> was invalid or it contains invalid information.</exception>
        public static IEnumerable<CultureInfo> GetCultures(this ILocalizationIndexResourceEntry entry)
        {
            if (IsValidLocalizationEntry(entry) == false) { throw new ArgumentException("The entry given must be a valid localization index entry."); }
            CultureInfo temp = null;
            foreach (var cstring in entry.Value.ToString().Split(';')) 
            {
                try { temp = new CultureInfo(cstring); }
                catch (CultureNotFoundException) {
                    throw new ArgumentException($"The culture string {cstring} is not valid. Please check whether {cstring} is a valid culture.");
                }
                yield return temp;
            }
        }
    }
}
