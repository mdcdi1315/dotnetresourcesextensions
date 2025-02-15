using System;
using System.Runtime.CompilerServices;

// This file contains all miscellaneous types.

namespace DotNetResourcesExtensions.Internal
{
    /// <summary>
    /// Defines base properties for all resource parser exceptions.
    /// </summary>
    public interface IResourceParsersExceptionBase
    {
        /// <summary>
        /// Gets the parser error category of the implemented exception.
        /// </summary>
        public ParserErrorType ErrorCategory { get; }

        /// <summary>
        /// Gets the original parser error message. This might not be always available.
        /// </summary>
        public System.String ParseErrorMessage { get; set; }
    }

    /// <summary>
    /// Serves as the base implementation for resources-related exception types.
    /// </summary>
    public interface IResourceExceptionBase
    {
        /// <summary>
        /// Gets the resource name that caused the implemented exception.
        /// </summary>
        public System.String ResourceName { get; }
    }

    /// <summary>
    /// Defines special constants that categorize the parser errors.
    /// </summary>
    public enum ParserErrorType : System.Byte
    {
        /// <summary>Internal dummy field. Do not use.</summary>
        Default,
        /// <summary>The error refers to header conformance deserialization.</summary>
        Header,
        /// <summary>The error refers to versioning mistake.</summary>
        Versioning,
        /// <summary>The error refers to deserialization error.</summary>
        Deserialization,
        /// <summary>The error refers to serialization error.</summary>
        Serialization
    }

    /// <summary>
    /// For classes that use streams , this simple interface defines whether 
    /// the class has control of the lifetime of the supplied stream.
    /// </summary>
    public interface IStreamOwnerBase
    {
        /// <summary>
        /// Gets or sets a value whether the implementing class controls the lifetime of the underlying stream.
        /// </summary>
        public System.Boolean IsStreamOwner { get; set; }
    }

    /// <summary>
    /// Internal enumeration so as to cope with classes that either use or do not use streams.
    /// </summary>
    internal enum StreamMixedClassManagement : System.Byte { None, NotStream, InitialisedWithStream, FileUsed }

}
