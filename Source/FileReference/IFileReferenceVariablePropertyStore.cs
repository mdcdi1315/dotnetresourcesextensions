using System;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Defines a property store which can be used to load file references with dynamic paths. <br />
    /// The implementer of a file reference must define then how to decode the variables from the path string.
    /// </summary>
    public interface IFileReferenceVariablePropertyStore
    {
        /// <summary>
        /// Gets the value of the given file reference variable.
        /// </summary>
        /// <param name="name">The variable name to whose value is to be retrieved.</param>
        /// <returns>The variable value , or the empty string ("") in case that the variable does not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> was null , or the empty string.</exception>
        public System.String GetVariable(System.String name);

        /// <summary>
        /// Sets the value of a new or existing file reference variable.
        /// </summary>
        /// <param name="name">The name of the variable to be declared or to be assigned a new value.</param>
        /// <param name="value">The value of the variable of name <paramref name="name"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> was null , or the empty string.</exception>
        public void SetVariable(System.String name, System.String value);
    }
}
