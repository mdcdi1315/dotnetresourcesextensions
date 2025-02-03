namespace DotNetResourcesExtensions.Collections
{
    /// <summary>
    /// Utilizes the current process environment variables and exposes them by using the <see cref="IFileReferenceVariablePropertyStore"/> interface.
    /// </summary>
    public sealed class EnvironmentVariablesPropertyStore : IFileReferenceVariablePropertyStore
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EnvironmentVariablesPropertyStore"/> class.
        /// </summary>
        public EnvironmentVariablesPropertyStore() {  }

        /// <inheritdoc />
        public string GetVariable(string name)
        {
            var ret = System.Environment.GetEnvironmentVariable(name);
            ret ??= System.String.Empty;
            return ret;
        }

        /// <inheritdoc />
        public void SetVariable(string name, string value) => System.Environment.SetEnvironmentVariable(name, value);
    }
}
