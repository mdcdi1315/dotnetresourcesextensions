using System;
using System.Resources;
using System.Globalization;
using DotNetResourcesExtensions.Internal;

namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// Gets a resource reader that contains all resource cultures for all resources and returns them in a resource entry. <br />
    /// The resource name is the actual resource name and the resource value contains a string which contains a colon-seperated list
    /// of all the supported cultures.
    /// </summary>
    public interface ILocalizationIndexResourceReader : IResourceReader
    {
        /// <summary>
        /// By default , all derived instances of <see cref="ILocalizationIndexResourceReader"/> must contain
        /// a resource 'INVARIANT' which does contain the invariant culture for the index reader. This field
        /// might be subsequently used as the invariant culture in the <see cref="LocalizableResourceLoader"/> class.
        /// </summary>
        /// <remarks>Note that this culture is not needed to be included in the list in each resource.</remarks>
        public CultureInfo SelectedInvariantCulture { get; }
    }

    /// <summary>
    /// Provides an abstract resource writer for writing localized resource indexes in the specified writer.
    /// </summary>
    public abstract class LocalizationIndexResourceWriter : IResourceWriter
    {
        private CultureInfo invculture;
        private System.Boolean invculturemodifidable;
        private readonly IResourceWriter basewr;

        private LocalizationIndexResourceWriter()
        {
            invculturemodifidable = true;
            invculture = CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LocalizationIndexResourceWriter"/> class with the specified underlying writer to use.
        /// </summary>
        /// <param name="writer">The writer where all the localized resources will be written to.</param>
        protected LocalizationIndexResourceWriter(IResourceWriter writer) : this()
        {
            basewr = writer;
        }

        /// <summary>
        /// Gets or sets the invariant culture for this reader. This value is written and cannot be modified after 
        /// <see cref="Generate()"/> is called.
        /// </summary>
        /// <remarks>By default , this property gets the value of <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public CultureInfo SelectedInvariantCulture { get => invculture; }

        /// <summary>
        /// Writes or flushes all the resources in the underlying resource writer.
        /// </summary>
        public void Generate()
        {
            if (invculturemodifidable)
            {
                basewr?.AddResource("INVARIANT", invculture.NativeName);
            }
            invculturemodifidable = false;
            basewr?.Generate();
        }

        /// <inheritdoc />
        public void Close() => basewr?.Close();

        /// <summary>
        /// Adds to the resource index the specified resource with the <paramref name="Name"/> and the localizable
        /// <paramref name="cultures"/> provided.
        /// </summary>
        /// <param name="Name">The resource name which the provided cultures will be referenced.</param>
        /// <param name="cultures"></param>
        /// <exception cref="System.ArgumentNullException">Either <paramref name="Name"/> or <paramref name="cultures"/> were null.</exception>
        /// <exception cref="System.ArgumentException">The <paramref name="cultures"/> array must contain at least one culture.</exception>
        public void AddResource(System.String Name , params CultureInfo[] cultures)
        {
            ParserHelpers.ValidateName(Name);
            if (cultures is null) { throw new System.ArgumentNullException(nameof(cultures)); }
            System.String result = System.String.Empty;
            if (invculturemodifidable) {
                foreach (CultureInfo culture in cultures)
                {
                    result += culture.Name + ";";
                }
            } else {
                foreach (CultureInfo culture in cultures) {
                    if (culture.DisplayName == invculture.DisplayName) { continue; }
                    result += culture.Name + ";";
                }
            }
            if (System.String.IsNullOrEmpty(result)) { throw new System.ArgumentException("At least one culture must be contained in cultures array." , nameof(cultures)); }
            basewr.AddResource(Name, result);
        }

        void IResourceWriter.AddResource(string name, byte[] value) => throw new System.NotSupportedException("This operation is not supported for localizable index writers.");

        void IResourceWriter.AddResource(string name, object value) => throw new System.NotSupportedException("This operation is not supported for localizable index writers.");

        void IResourceWriter.AddResource(string name, string value) => throw new System.NotSupportedException("This operation is not supported for localizable index writers.");

        /// <summary>
        /// Disposes the current resource writer and the underlying writer too (due to the fact that is a strong convention).
        /// </summary>
        public void Dispose() {
            Generate();
            Close();
            basewr?.Dispose();
            invculture = null;
            GC.SuppressFinalize(this);
        }
    }
}
