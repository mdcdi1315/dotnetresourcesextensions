﻿using System;
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

}