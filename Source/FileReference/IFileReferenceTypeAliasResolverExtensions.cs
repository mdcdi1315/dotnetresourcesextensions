
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Provides extensibility methods for objects that implement the <see cref="IFileReferenceTypeAliasResolver"/> interface.
    /// </summary>
    public static class IFileReferenceTypeAliasResolverExtensions
    {

        /// <summary>
        /// Resolves the given aliased type name , and returns it's corresponding type.
        /// </summary>
        /// <param name="alias">The alias type name to get it's corresponding type.</param>
        /// <param name="resolver">The alias resolver to use.</param>
        /// <returns>The resolved type name.</returns>
        /// <exception cref="System.ArgumentNullException">The <paramref name="alias"/> parameter was null , or empty.</exception>
        /// <exception cref="System.ArgumentException">The <paramref name="alias"/> provided was not registered.</exception>
        public static System.Type ResolveAlias(this IFileReferenceTypeAliasResolver resolver , System.String alias)
        {
            if (resolver is null) { throw new System.NullReferenceException("Attempted to dereference a null alias resolver."); }
            System.Type type;
            if (resolver.TryResolveAlias(alias, out type) == false) { throw new System.ArgumentException($"Cannot find the given alias with name {alias}."); }
            return type;
        }

        /// <summary>
        /// Copies all the aliases found in <paramref name="aliases"/> into the current resolver.
        /// </summary>
        /// <param name="resolver">The alias resolver to use.</param>
        /// <param name="aliases">The aliases to register to <paramref name="resolver"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="aliases"/> were null.</exception>
        public static void CopyAliases(this IFileReferenceTypeAliasResolver resolver , IEnumerable<KeyValuePair<System.String , System.Type>> aliases)
        {
            if (resolver is null) { throw new System.NullReferenceException("Attempted to dereference a null alias resolver."); }
            if (aliases is null) { throw new System.ArgumentNullException(nameof(aliases)); }
            foreach (var kvp in aliases) 
            {
                resolver.RegisterAlias(kvp.Key, kvp.Value);
            }
        }

    }
}