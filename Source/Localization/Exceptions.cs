using DotNetResourcesExtensions.Internal;
using System;
using System.Globalization;

// This file contains exceptions related to localization subpart.

namespace DotNetResourcesExtensions
{

    /// <summary>
    /// A more specific version of <see cref="ResourceNotFoundException"/> but is thrown when the
    /// specified resource name does exist but does not exist in the specified culture.
    /// </summary>
    public sealed class LocalizationNotFoundException : ResourceNotFoundException
    {
        private readonly string resname;
        private readonly CultureInfo info;
        
        /// <summary>
        /// Creates a new instance of the <see cref="LocalizationNotFoundException"/> class with the causing resource
        /// name and culture.
        /// </summary>
        /// <param name="ResName">The resource name that causes this exception.</param>
        /// <param name="info">The resource culture that causes this exception.</param>
        public LocalizationNotFoundException(System.String ResName , CultureInfo info) : base(ResName)
        {
            this.info = info;
            resname = ResName;
        }

        /// <inheritdoc />
        public override string Message => $"The resource \'{resname}\' was not found because the localized variant \'{info}\' of the resource does not exist.";
    }

    /// <summary>
    /// Specifies that a resource was not found because the requested culture for the resource
    /// is not available (reader not found error).
    /// </summary>
    public sealed class LocalizedReaderNotFoundException : ResourceNotFoundException
    {
        private readonly string resname;
        private readonly CultureInfo info;

        /// <summary>
        /// Creates a new instance of the <see cref="LocalizedReaderNotFoundException"/> class with the causing resource
        /// name and culture.
        /// </summary>
        /// <param name="ResName">The resource name that causes this exception.</param>
        /// <param name="info">The resource culture that causes this exception.</param>
        public LocalizedReaderNotFoundException(System.String ResName, CultureInfo info) : base(ResName)
        {
            this.info = info;
            resname = ResName;
        }

        /// <inheritdoc />
        public override string Message => $"The resource \'{resname}\' was not found because a localized reader with culture \'{info}\' was not found.";
    }

    /// <summary>
    /// Exception class that is thrown when a localized reader has an invalid layout , as
    /// such not having some expected resources , or the resources that contains are invalid.
    /// </summary>
    public sealed class InvalidLocalizedReaderLayoutException : DotNetResourcesException
    {
        private readonly string readername;

        /// <summary>
        /// Initializes a new instance of the class that had an invalid layout.
        /// </summary>
        /// <param name="readername">The reader type name that was invalid.</param>
        public InvalidLocalizedReaderLayoutException(string readername)
        {
            this.readername = readername;
        }

        /// <summary>
        /// Initializes a new instance of the class that had an invalid layout.
        /// </summary>
        /// <param name="readertype">The reader type that was invalid.</param>
        public InvalidLocalizedReaderLayoutException(System.Type readertype)
        {
            readername = readertype is null ? "<Error>" : readertype.FullName;
        }

        /// <summary>
        /// Gets the reader name that was invalid.
        /// </summary>
        public System.String Name => readername;

        /// <inheritdoc />
        public sealed override string Message => $"The reader \'{readername}\' had an invalid layout and cannot be used as any localized reader.";
    }

}
