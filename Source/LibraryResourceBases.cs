using System.Resources;
using DotNetResourcesExtensions.Internal;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions
{
   
    /// <summary>
    /// Dummy interface that all resource writers in the DotNetResourcesExtensions project are currently implementing. <br />
    /// It is not recommended for the end user to use this interface; instead , he could also use the <see cref="IResourceWriter"/> interface alone.
    /// </summary>
    public interface IDotNetResourcesExtensionsWriter : IResourceWriter , IStreamOwnerBase , IUsingCustomFormatter { }

    /// <summary>
    /// Dummy interface that all resource readers in the DotNetResourcesExtensions project are currently implementing. <br />
    /// It is not recommended for the end user to use this interface; besides, the <see cref="IResourceReader"/> interface 
    /// alone can be used in <see cref="DefaultResourceLoader"/> derived classes.
    /// </summary>
    public interface IDotNetResourcesExtensionsReader : IResourceReader , IStreamOwnerBase , IUsingCustomFormatter { }

}
