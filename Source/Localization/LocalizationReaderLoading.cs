
using System;
using System.Globalization;

namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// Specifies flags for how a localization should be loaded into a localization resource loader.
    /// </summary>
    [Flags]
    public enum LocalizationSpecialTypeFlags : System.Byte
    {
        /// <summary>
        /// This culture does not have any special traits.
        /// </summary>
        None = 0,
        /// <summary>
        /// This culture should be set as the fallback culture. <br />
        /// This flag must be set on only one culture.
        /// </summary>
        IsFallbackCulture = 0x01,
        /// <summary>
        /// This culture is optional; which it means that it may not be included safely in the current localization list.
        /// </summary>
        IsOptionalCulture = 0x02,
        /// <summary>
        /// The translations defined for this culture are not final; which it does mean that the translations may be changed at any time.
        /// </summary>
        IsPreRelease = 0x04
    }

    /// <summary>
    /// Defines how a resource loader should load a localization reader , and which culture the reader does represent. <br />
    /// The localization readers must have full support for this class data , as it finally defines how the resource loaders should behave.
    /// </summary>
    public sealed record class LocalizationEntry
    {
        /// <summary>
        /// Specifies the special flags that must be applied to this localization when it is loaded by any reader.
        /// </summary>
        public LocalizationSpecialTypeFlags Flags;
        /// <summary>
        /// Specifies the culture information that will be bound to this entry.
        /// </summary>
        public CultureInfo Culture;

        /// <summary>
        /// Creates an empty instance of the <see cref="LocalizationEntry"/> class.
        /// </summary>
        public LocalizationEntry() { Culture = null; Flags = 0; }

        /// <summary>
        /// Defines the default equality scheme for <see cref="LocalizationEntry"/> types. <br />
        /// Two <see cref="LocalizationEntry"/> are considered equal if they do reference the same LCID.
        /// </summary>
        /// <param name="other">The other culture to test.</param>
        /// <returns>A value that represents their equality.</returns>
        public System.Boolean Equals(LocalizationEntry other) => other is not null && other.Culture is not null && Culture is not null && other.Culture.LCID == Culture.LCID;

        /// <summary>
        /// Serves as a hash function for the current <see cref="LocalizationEntry"/>, suitable
        /// for hashing algorithms and data structures, such as a hash table.
        /// </summary>
        /// <returns>A hash code for the current <see cref="LocalizationEntry"/>.</returns>
        public override System.Int32 GetHashCode() => Culture is null ? 0 : Culture.GetHashCode();

        internal LocalizationEntry(LOCALIZATIONENTRY ent)
        {
            Flags = ent.Flags;
            Culture = new(ent.LCID);
        }
    }
}