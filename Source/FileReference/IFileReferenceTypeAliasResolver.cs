namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Maps file reference types with type names. <br />
    /// For <see cref="IFileReference.SavingType"/> property.
    /// </summary>
    public interface IFileReferenceTypeAliasResolver
    {
        /// <summary>
        /// Resolves the given aliased type name. If the alias is registered , it returns <see langword="true"/> , and
        /// the resolved type is returned into the <paramref name="type"/> parameter.
        /// </summary>
        /// <param name="alias">The alias type name to get it's corresponding type.</param>
        /// <param name="type">The resolved type name.</param>
        /// <returns>A value whether the given alias was registered or not.</returns>
        /// <exception cref="System.ArgumentNullException">The <paramref name="alias"/> parameter was null , or empty.</exception>
        public System.Boolean TryResolveAlias(System.String alias , out System.Type type);

        /// <summary>
        /// Registers a new alias with the current alias resolver.
        /// </summary>
        /// <param name="alias">The alias type name that <paramref name="reference"/> will be aliased as.</param>
        /// <param name="reference">The type instance to be bound with <paramref name="alias"/>.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="alias"/> and/or <paramref name="reference"/> parameters were null.</exception>
        /// <exception cref="System.ArgumentException">The <paramref name="alias"/> provided was already registered.</exception>
        public void RegisterAlias(System.String alias , System.Type reference);

        /// <summary>
        /// Removes the given alias. <br />
        /// If the given alias does not exist , then this method does nothing.
        /// </summary>
        /// <param name="alias">The alias to be removed.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="alias"/> parameter was null , or empty.</exception>
        public void UnRegisterAlias(System.String alias);

        /// <summary>
        /// Clears all the registered aliases.
        /// </summary>
        public void Clear();
    }
}
