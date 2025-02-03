
namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Resolves file references dynamically by the specified file variable property store and the specified type resolver for type aliases.
    /// </summary>
    public interface IDynamicFileReference : IFileReference
    {
        /// <summary>
        /// The property store to resolve file path variables.
        /// </summary>
        public IFileReferenceVariablePropertyStore PropertyStore { get; }

        /// <summary>
        /// The alias resolver to use for type names.
        /// </summary>
        public IFileReferenceTypeAliasResolver TypeAliasResolver { get; }
    }
}