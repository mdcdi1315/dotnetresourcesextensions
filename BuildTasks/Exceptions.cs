using System;

namespace DotNetResourcesExtensions.BuildTasks
{
    /// <summary>
    /// Represents the BuildTasks base exception class.
    /// </summary>
    public abstract class BuildTasksBaseException : System.ApplicationException
    {
        /// <summary>
        /// Constructs a default instance of the <see cref="BuildTasksBaseException"/> class.
        /// </summary>
        protected BuildTasksBaseException() { }

        /// <summary>
        /// Constructs a new instance of <see cref="BuildTasksBaseException"/> with the specified message thst is used as the exception message.
        /// </summary>
        /// <param name="message">The message to be shown when any derived class is created.</param>
        public BuildTasksBaseException(string message) : base(message) { }

        /// <summary>
        /// Constructs a new instance of <see cref="BuildTasksBaseException"/> with the specified message thst is used as the exception message , 
        /// and the specified inner exception that is the root cause of this exception.
        /// </summary>
        /// <param name="message">The message to be shown when any derived class is created.</param>
        /// <param name="innerException">The inner exception to provide that is the root cause of this exception.</param>
        public BuildTasksBaseException(System.String message , Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Represents the cases where an exception in internal calls was found and was unexpected. 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Exception->UnexpectedErrorException")]
    public sealed class UnexpectedErrorException : BuildTasksBaseException
    {
        public UnexpectedErrorException(System.Exception ex) : base("" , ex) {}

        public override string Message => $"Exception of type {InnerException.GetType().FullName} was thrown. This exception is unexpected.";

        public override System.String ToString() => $"UnexpectedErrorException: {Message} --> \n{InnerException}";
    }

    /// <summary>
    /// Represents the exception that is thrown when the user has incorrectly used an API in a BuildTasks class.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Exception->InvalidInputFromUserException")]
    public sealed class InvalidInputFromUserException : BuildTasksBaseException
    {
        public InvalidInputFromUserException(System.Exception ex) : base("", ex) {}

        public InvalidInputFromUserException(System.ArgumentException ex) : base("" , ex) {}

        public override string Message => $"Invalid raw input was given to the code requirements.";
    }
}
