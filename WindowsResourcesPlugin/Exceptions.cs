#pragma warning disable CA1416

namespace DotNetResourcesExtensions
{
    
    /// <summary>
    /// Exception class that is thrown when no native resources were found.
    /// </summary>
    public sealed class NoNativeResourcesFoundException : Internal.DotNetResourcesException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NoNativeResourcesFoundException"/> class.
        /// </summary>
        public NoNativeResourcesFoundException() : base() { }

        /// <inheritdoc />
        public override string Message => "The native resource reader did not found any resources inside the specified image.";
    }

}
