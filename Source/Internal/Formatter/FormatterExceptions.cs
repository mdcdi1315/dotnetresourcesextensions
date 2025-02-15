
namespace DotNetResourcesExtensions.Internal.CustomFormatter.Exceptions
{
    /// <summary>
    /// Defines the base exception for all types defined in the CustomFormatter namespace.
    /// </summary>
    public class BaseException : System.Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public BaseException() : base() { }

        /// <summary>
        /// Creates a new instance of the <see cref="BaseException"/> class with the specified message
        /// as the exception message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        public BaseException(System.String message) : base(message) { }

        /// <summary>
        /// Creates a new instance of the <see cref="BaseException"/> class with the specified message
        /// as the exception message , and the exception that caused this exception.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="innerexception">The inner exception that is the root cause of this exception.</param>
        public BaseException(System.String message , System.Exception? innerexception) : base(message, innerexception) { }
    }

    /// <summary>
    /// Exception class that is thrown when a specified converter for the given object was not found.
    /// </summary>
    public sealed class ConverterNotFoundException : BaseException
    {
        private System.Type neededtype;

        /// <summary>
        /// Creates a new instance of <see cref="ConverterNotFoundException"/> with the type which
        /// the corresponding converter was not found.
        /// </summary>
        /// <param name="neededtype">The type that it's corresponding converter does not exist.</param>
        public ConverterNotFoundException(System.Type neededtype) : base() { this.neededtype = neededtype; }

        /// <inheritdoc />
        public override string Message => 
            $"The type \'{neededtype.AssemblyQualifiedName}\' does not have a type converter.\n" +
            "Check if you have supplied the required converter so that is able to serialize/deserialise this type.";

        /// <summary>
        /// Gets the type of the object that caused this exception.
        /// </summary>
        public System.Type CausedType => neededtype;
    }

    /// <summary>
    /// Exception class that is thrown when an attempt to register a converter that another converter can serialize 
    /// too was added.
    /// </summary>
    public sealed class ConfilctingConverterException : BaseException
    {
        private System.Type subjecttype;
        private System.Type convertertype;
        private System.Type existingconvertertype;

        /// <summary>
        /// Creates a new instance of <see cref="ConfilctingConverterException"/> class with the specified
        /// subject type , the current registered converter and the converter that caused this exception.
        /// </summary>
        /// <param name="SubjectType">The subject type that both converters can convert.</param>
        /// <param name="CurrentConverterType">The currently applied converter type.</param>
        /// <param name="GivenConverterType">The conflicting converter type.</param>
        public ConfilctingConverterException(System.Type SubjectType ,
            System.Type CurrentConverterType , System.Type GivenConverterType) : base()
        {
            subjecttype = SubjectType;
            existingconvertertype = CurrentConverterType;
            convertertype = GivenConverterType;
        }

        /// <summary>
        /// Returns the subject type that both converters can convert.
        /// </summary>
        public System.Type SubjectType => subjecttype;

        /// <summary>
        /// Returns the currently applied converter type.
        /// </summary>
        public System.Type ConverterType => existingconvertertype;

        /// <summary>
        /// Returns the conflicting converter type.
        /// </summary>
        public System.Type ConfilictingConverterType => convertertype;

        /// <inheritdoc />
        public override string Message => $"The type \'{subjecttype.AssemblyQualifiedName}\' " +
            $"does already have an equal converter type \'{existingconvertertype.AssemblyQualifiedName}\' ," +
            $"but another converter type with info \'{convertertype.AssemblyQualifiedName}\' can convert the \'{subjecttype.FullName}\'.\n" +
            "Such type conflicts are not allowed and might cause confusion on the formatter internals.";
    }

    /// <summary>
    /// This class instance is thrown when a type was not found given it's search string.
    /// </summary>
    public sealed class UnresolvedTypeFoundException : BaseException
    {
        private System.String srcstring;

        /// <summary>
        /// Creates a new instance of the <see cref="UnresolvedTypeFoundException"/> class with the specified search string that caused this exception.
        /// </summary>
        /// <param name="srcstring">The search string that is the reason that this exception is created.</param>
        public UnresolvedTypeFoundException(string srcstring)
        {
            this.srcstring = srcstring;
        }

        /// <summary>
        /// The search string that caused this exception.
        /// </summary>
        public System.String TypeString => srcstring;

        /// <inheritdoc />
        public override string Message => 
            "Could not find the corresponding type from the search string. The search string might be invalid.\n" +
            $"Search String: {srcstring}";
    }

    /// <summary>
    /// Thrown when attempted to register a resolver that does not meet some specified criteria. <br />
    /// This specific issue can be learned from the exception message.
    /// </summary>
    public sealed class InvalidResolverLayoutException : BaseException 
    {
        /// <summary>
        /// Creates a new and default instance of the <see cref="InvalidResolverLayoutException"/> class.
        /// </summary>
        public InvalidResolverLayoutException() : base("The specified type resolver does not have a requirement and was rejected.") { }

        /// <summary>
        /// Creates a new instance of the <see cref="InvalidResolverLayoutException"/> class with the specified message to show to the user.
        /// </summary>
        /// <param name="msg">The exception-specific message.</param>
        public InvalidResolverLayoutException(System.String msg) : base(msg) { }

        /// <summary>
        /// Creates a new instance of the <see cref="InvalidResolverLayoutException"/> class with the specified message to show to the user , and
        /// the exception that is the root cause of this exception.
        /// </summary>
        /// <param name="msg">The exception-specific message.</param>
        /// <param name="inner">The exception that caused this exception to occur.</param>
        public InvalidResolverLayoutException(System.String msg , System.Exception inner) : base(msg, inner) { }
    }

}