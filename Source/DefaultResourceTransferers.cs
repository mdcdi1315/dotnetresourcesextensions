
namespace DotNetResourcesExtensions
{
    
    /// <summary>
    /// The <see cref="AbstractResourceTransferer"/> class can transfer resources from any kind of resource readers
    /// and writers. Just pass valid resource readers/writers to the constructor and you are ready.
    /// </summary>
    public sealed class AbstractResourceTransferer : DefaultResourcesTransferer
    {

        /// <summary>
        /// Creates a new instance of <see cref="AbstractResourceTransferer"/> class with the specified reader and writer.
        /// </summary>
        /// <param name="read">The reader to read resources from.</param>
        /// <param name="write">The writer to write resources.</param>
        public AbstractResourceTransferer(System.Resources.IResourceReader read , System.Resources.IResourceWriter write) : base() 
        {
            reader = read; writer = write;
            PrepareInstance();
        }
    }

    /// <summary>
    /// Transfers custom ResX resources built with <see cref="Internal.ResX.ResXResourceWriter"/> and writes them to 
    /// the preserialized .NET <see cref="System.Resources.Extensions.PreserializedResourceWriter"/> format.
    /// </summary>
    public sealed class CustomResXToBinaryTransferer : DefaultResourcesTransferer
    {
        /// <summary>
        /// Create a new instance of <see cref="CustomResXToBinaryTransferer"/> from the specified 
        /// input and target streams.
        /// </summary>
        /// <param name="inputstream">The input stream to get the resources from.</param>
        /// <param name="targetstream">The target stream to write the resources to.</param>
        public CustomResXToBinaryTransferer(System.IO.Stream inputstream , System.IO.Stream targetstream) 
            : base(new Internal.ResX.ResXResourceReader(inputstream) , 
                  new System.Resources.Extensions.PreserializedResourceWriter(targetstream)) { }

        /// <summary>
        /// Create a new instance of <see cref="CustomResXToBinaryTransferer"/> from the specified 
        /// input and target file paths.
        /// </summary>
        /// <param name="inputpath">The file path to get the resources from.</param>
        /// <param name="targetpath">The file path to write the resources to.</param>
        public CustomResXToBinaryTransferer(System.String inputpath, System.String targetpath)
         : base(new Internal.ResX.ResXResourceReader(inputpath),
                  new System.Resources.Extensions.PreserializedResourceWriter(targetpath)) { }
    }

    /// <summary>
    /// Transfers custom JSON resources built with <see cref="JSONResourcesWriter"/> and writes them to 
    /// the preserialized .NET <see cref="System.Resources.Extensions.PreserializedResourceWriter"/> format.
    /// </summary>
    public sealed class CustomJSONToBinaryTransferer : DefaultResourcesTransferer
    {
        /// <summary>
        /// Create a new instance of <see cref="CustomJSONToBinaryTransferer"/> from the specified 
        /// input and target file paths.
        /// </summary>
        /// <param name="inputpath">The file path to get the resources from.</param>
        /// <param name="outpath">The file path to write the resources to.</param>
        public CustomJSONToBinaryTransferer(System.String inputpath , System.String outpath) : base(
            new JSONResourcesReader(inputpath) , 
            new System.Resources.Extensions.PreserializedResourceWriter(outpath)) { }

        /// <summary>
        /// Create a new instance of <see cref="CustomJSONToBinaryTransferer"/> from the specified 
        /// input and target streams.
        /// </summary>
        /// <param name="input">The input stream to get the resources from.</param>
        /// <param name="targetstream">The target stream to write the resources to.</param>
        public CustomJSONToBinaryTransferer(System.IO.Stream input , System.IO.Stream targetstream) : base(
            new JSONResourcesReader(input) ,
            new System.Resources.Extensions.PreserializedResourceWriter(targetstream)) { }
    }

    /// <summary>
    /// Transfers custom XML resources built with <see cref="XMLResourcesWriter"/> and writes them to 
    /// the preserialized .NET <see cref="System.Resources.Extensions.PreserializedResourceWriter"/> format.
    /// </summary>
    public sealed class CustomXMLToBinaryTransferer : DefaultResourcesTransferer
    {
        /// <summary>
        /// Create a new instance of <see cref="CustomXMLToBinaryTransferer"/> from the specified 
        /// input and target file paths.
        /// </summary>
        /// <param name="inputpath">The file path to get the resources from.</param>
        /// <param name="outpath">The file path to write the resources to.</param>
        public CustomXMLToBinaryTransferer(System.String inputpath, System.String outpath) : base(
            new XMLResourcesReader(inputpath),
            new System.Resources.Extensions.PreserializedResourceWriter(outpath))
        { }

        /// <summary>
        /// Create a new instance of <see cref="CustomXMLToBinaryTransferer"/> from the specified 
        /// input and target streams.
        /// </summary>
        /// <param name="input">The input stream to get the resources from.</param>
        /// <param name="targetstream">The target stream to write the resources to.</param>
        public CustomXMLToBinaryTransferer(System.IO.Stream input, System.IO.Stream targetstream) : base(
            new XMLResourcesReader(input),
            new System.Resources.Extensions.PreserializedResourceWriter(targetstream))
        { }
    }
}
