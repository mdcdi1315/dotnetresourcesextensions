
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// The <see cref="IResourceTransferer"/> interface has the ability to transfer resources from one format to another. <br />
    /// You can also consider this interface as a 'Resource Copy' operation. <br />
    /// Additionally , you can add and more resources that the reader does not have , because this interface also implements the
    /// <see cref="System.Resources.IResourceWriter"/> interface.
    /// </summary>
    public interface IResourceTransferer : System.Resources.IResourceWriter , IAsyncDisposable
    {
        /// <summary>
        /// Gets all the resource names that are available from the reader.
        /// </summary>
        public IEnumerable<System.String> ReaderResourceNames { get; }

        /// <summary>
        /// Transfers sequentially all the current read resources to the target writer.
        /// </summary>
        public void TransferAll();

        /// <summary>
        /// This property gets the current resource reader that resources from it will be transferred to the <see cref="CurrentUsedWriter"/>.
        /// </summary>
        public System.Resources.IResourceReader CurrentUsedReader { get; }

        /// <summary>
        /// This property gets the current resource writer that all the resulting resources will be saved to it.
        /// </summary>
        public System.Resources.IResourceWriter CurrentUsedWriter { get; }

        /// <summary>
        /// This method transfers to the target writer only the selected resources with the resource names defined in the
        /// <paramref name="resnames"/> parameter..
        /// </summary>
        /// <param name="resnames">The resource names to transfer.</param>
        public void TransferSelection(IEnumerable<System.String> resnames);

        /// <summary>
        /// Generates asyncronously the resources to the target. <br />
        /// This was added because you might need to transfer a lot of resources!
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> that contains the progress of resource generation.</returns>
        public ValueTask GenerateAsync();
    }
}
