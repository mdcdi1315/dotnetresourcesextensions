
using System;

namespace DotNetResourcesExtensions
{
    using Internal;
    using Properties;

    namespace Internal
    {
        /// <summary>
        /// Provides the default implementing exception for all resource 
        /// exception types defined in the DotNetResourcesExtensions library.
        /// </summary>
        public abstract class DotNetResourcesException : Exception , IResourceExceptionBase
        {
            private System.String rname;

            /// <summary>
            /// Serves as the default constructor.
            /// </summary>
            public DotNetResourcesException() : base() { rname = null; }
            
            /// <summary>
            /// Creates a new instance of <see cref="DotNetResourcesException"/> with the specified resource name
            /// that caused this exception.
            /// </summary>
            /// <param name="resname">The resource name that caused this exception.</param>
            public DotNetResourcesException(System.String resname) : base() { rname = resname; }

            /// <summary>
            /// Creates a new instance of <see cref="DotNetResourcesException"/> with the specified resource name
            /// that caused this exception , and a custom message that describes the exception.
            /// </summary>
            /// <param name="resname">The resource name that caused this exception.</param>
            /// <param name="msg">A custom message that describes the exception.</param>
            public DotNetResourcesException(System.String resname , System.String msg) : base(msg) 
            {
                rname = resname;
            }

            /// <inheritdoc cref="System.Exception.Message" />
            public virtual new string Message => System.String.Format(Resources.DotNetResourcesException_Message , base.Message , rname);

            /// <summary>
            /// Gets the resource name that caused the exception.
            /// </summary>
            public virtual System.String ResourceName { get => rname; }
        }

        /// <summary>
        /// Provides the default implementing exception for all resource 
        /// parsers exception types 
        /// defined in the DotNetResourcesExtensions library.
        /// </summary>
        public abstract class DotNetResourceParsersException : Exception , IResourceParsersExceptionBase
        {
            private System.String origerror;
            private ParserErrorType errortype;

            /// <summary>
            /// Initialises a default instance of <see cref="DotNetResourceParsersException"/>.
            /// </summary>
            public DotNetResourceParsersException() { origerror = null; errortype = ParserErrorType.Default; }

            /// <summary>
            /// Create a new instance of <see cref="DotNetResourceParsersException"/>
            /// with the specified message and parser error category.
            /// </summary>
            /// <param name="msg">The message of this exception</param>
            /// <param name="errortype">The parser error category.</param>
            public DotNetResourceParsersException(System.String msg ,  ParserErrorType errortype) 
                : base(msg) { this.errortype = errortype; }

            /// <summary>
            /// Create a new instance of <see cref="DotNetResourceParsersException"/>
            /// with the specified message , parser error message and parser error category.
            /// </summary>
            /// <param name="message">The message of this exception</param>
            /// <param name="parsermessage">The parser error message of this exception.</param>
            /// <param name="errortype">The parser error category.</param>
            public DotNetResourceParsersException(System.String message , System.String parsermessage ,  ParserErrorType errortype) : base(message)
            {
                origerror = parsermessage;
                this.errortype = errortype;
            }

            /// <inheritdoc />
            public override string Message => System.String.Format(
                Resources.ResourceParsersException_Message ,
                base.Message, errortype , origerror);

            /// <summary>
            /// Gets the parser error category of this exception.
            /// </summary>
            public ParserErrorType ErrorCategory { get => errortype; }

            /// <summary>
            /// Gets the original parser error message. This might not be always available.
            /// </summary>
            public System.String ParseErrorMessage { get => origerror; set => origerror = value; }
        }
    }

    /// <summary>
    /// Defines the XML Reader/Writer exception type that is thrown by the XML resource classes.
    /// </summary>
    public class XMLFormatException : DotNetResourceParsersException
    {
        /// <summary>
        /// Creates a new instance of <see cref="XMLFormatException"/> with the specified message 
        /// and parser error category.
        /// </summary>
        /// <param name="msg">The message to show.</param>
        /// <param name="errortype">The parser error category.</param>
        public XMLFormatException(System.String msg, ParserErrorType errortype) : base(msg , errortype) { }

        /// <summary>
        /// Creates a new instance of <see cref="XMLFormatException"/> with the specified message 
        /// , parser specific error message and parser error category.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="errortype">The parser error category.</param>
        /// <param name="parsermessage">The parser error specific message.</param>
        public XMLFormatException(System.String message, System.String parsermessage, ParserErrorType errortype) 
            : base (message , parsermessage , errortype) { }

    }

    /// <summary>
    /// Defines the JSON Reader/Writer exception type that is thrown by the JSON resource classes.
    /// </summary>
    public class JSONFormatException : DotNetResourceParsersException 
    {
        /// <summary>
        /// Creates a new instance of <see cref="JSONFormatException"/> with the specified message 
        /// and parser error category.
        /// </summary>
        /// <param name="msg">The message to show.</param>
        /// <param name="errortype">The parser error category.</param>
        public JSONFormatException(System.String msg, ParserErrorType errortype) : base(msg, errortype) { }

        /// <summary>
        /// Creates a new instance of <see cref="JSONFormatException"/> with the specified message 
        /// , parser specific error message and parser error category.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="errortype">The parser error category.</param>
        /// <param name="parsermessage">The parser error specific message.</param>
        public JSONFormatException(System.String message, System.String parsermessage, ParserErrorType errortype)
            : base(message, parsermessage, errortype) { }
    }

    /// <summary>
    /// Defines the MS-INI Reader/Writer exception type that is thrown by the MS-INI resource classes.
    /// </summary>
    public class MSINIFormatException : DotNetResourceParsersException
    {
        /// <summary>
        /// Creates a new instance of <see cref="MSINIFormatException"/> with the specified message 
        /// and parser error category.
        /// </summary>
        /// <param name="msg">The message to show.</param>
        /// <param name="errortype">The parser error category.</param>
        public MSINIFormatException(System.String msg, ParserErrorType errortype) : base(msg, errortype) { }

        /// <summary>
        /// Creates a new instance of <see cref="MSINIFormatException"/> with the specified message 
        /// , parser specific error message and parser error category.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="errortype">The parser error category.</param>
        /// <param name="parsermessage">The parser error specific message.</param>
        public MSINIFormatException(System.String message, System.String parsermessage, ParserErrorType errortype)
            : base(message, parsermessage, errortype) { }
    }

    /// <summary>
    /// Exception class that it is thrown when a specific resource was not found.
    /// </summary>
    public class ResourceNotFoundException : Internal.DotNetResourcesException
    {
        /// <summary>
        /// Create a new instance of <see cref="ResourceNotFoundException"/> with the specified resource name which was not found.
        /// </summary>
        /// <param name="ResName">The resource name that it was not found.</param>
        public ResourceNotFoundException(System.String ResName) : base(ResName, 
            System.String.Format(Resources.ResourceNotFoundException_Ctor, ResName)) { }
    }

    /// <summary>
    /// Exception class that it is thrown when a resource was attempted to get with an incorrect type.
    /// </summary>
    public class ResourceTypeMismatchException : Internal.DotNetResourcesException
    {
        private System.Type found, expected;

        private ResourceTypeMismatchException() : base() { found = null; expected = null; }

        /// <summary>
        /// Creates a new instance of <see cref="ResourceTypeMismatchException"/> with the specified expected type , 
        /// the type attempted to get the resource and the resource name which caused this exception.
        /// </summary>
        /// <param name="found">The type in which the resource was attempted to got.</param>
        /// <param name="expected">The expected type.</param>
        /// <param name="resourcename">The resource name which caused this exception.</param>
        public ResourceTypeMismatchException(System.Type found, System.Type expected, System.String resourcename)
            : base(resourcename, 
         System.String.Format(Resources.ResourceTypeMismatchException_Ctor, resourcename, found.Name, expected.Name))
        {
            this.found = found;
            this.expected = expected;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ResourceTypeMismatchException"/> with the specified expected type , 
        /// the type attempted to get the resource , a message and the resource name which caused this exception.
        /// </summary>
        /// <param name="message">The specified error message.</param>
        /// <param name="found">The type in which the resource was attempted to got.</param>
        /// <param name="expected">The expected type.</param>
        /// <param name="resourcename">The resource name which caused this exception.</param>
        public ResourceTypeMismatchException(System.Type found, System.Type expected, System.String message, System.String resourcename) : base(resourcename,  message)
        {
            this.found = found;
            this.expected = expected;
        }

        /// <summary>
        /// The expected resource type.
        /// </summary>
        public System.Type ExpectedType => expected;

        /// <summary>
        /// The resource type in which the resource name defined in <see cref="Internal.DotNetResourcesException.ResourceName"/> was attempted to got.
        /// </summary>
        public System.Type FoundType => found;

        /// <inheritdoc />
        public override string Message => System.String.Format(
            Resources.ResourceTypeMismatchException_Message,
            base.Message , expected.FullName , found.FullName , ResourceName);
    }

}