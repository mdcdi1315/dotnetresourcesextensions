﻿
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
        /// <param name="write">The writer to write resources to.</param>
        public AbstractResourceTransferer(System.Resources.IResourceReader read , System.Resources.IResourceWriter write) : base() 
        {
            reader = read; writer = write;
            PrepareInstance();
        }
    }

    /// <summary>
    /// Transfers custom ResX resources built with <see cref="Internal.ResX.ResXResourceWriter"/> and writes them to 
    /// the preserialized .NET <see cref="T:System.Resources.Extensions.PreserializedResourceWriter"/> format.
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
                  new Internal.DotNetResources.PreserializedResourceWriter(targetstream)) { }

        /// <summary>
        /// Create a new instance of <see cref="CustomResXToBinaryTransferer"/> from the specified 
        /// input and target file paths.
        /// </summary>
        /// <param name="inputpath">The file path to get the resources from.</param>
        /// <param name="targetpath">The file path to write the resources to.</param>
        public CustomResXToBinaryTransferer(System.String inputpath, System.String targetpath)
         : base(new Internal.ResX.ResXResourceReader(inputpath),
                  new Internal.DotNetResources.PreserializedResourceWriter(targetpath)) { }
    }

    /// <summary>
    /// Transfers custom JSON resources built with <see cref="JSONResourcesWriter"/> and writes them to 
    /// the preserialized .NET <see cref="T:System.Resources.Extensions.PreserializedResourceWriter"/> format.
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
            new Internal.DotNetResources.PreserializedResourceWriter(outpath)) { }

        /// <summary>
        /// Create a new instance of <see cref="CustomJSONToBinaryTransferer"/> from the specified 
        /// input and target streams.
        /// </summary>
        /// <param name="input">The input stream to get the resources from.</param>
        /// <param name="targetstream">The target stream to write the resources to.</param>
        public CustomJSONToBinaryTransferer(System.IO.Stream input , System.IO.Stream targetstream) : base(
            new JSONResourcesReader(input) ,
            new Internal.DotNetResources.PreserializedResourceWriter(targetstream)) { }
    }

    /// <summary>
    /// Transfers custom XML resources built with <see cref="XMLResourcesWriter"/> and writes them to 
    /// the preserialized .NET <see cref="T:System.Resources.Extensions.PreserializedResourceWriter"/> format.
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
            new Internal.DotNetResources.PreserializedResourceWriter(outpath))
        { }

        /// <summary>
        /// Create a new instance of <see cref="CustomXMLToBinaryTransferer"/> from the specified 
        /// input and target streams.
        /// </summary>
        /// <param name="input">The input stream to get the resources from.</param>
        /// <param name="targetstream">The target stream to write the resources to.</param>
        public CustomXMLToBinaryTransferer(System.IO.Stream input, System.IO.Stream targetstream) : base(
            new XMLResourcesReader(input),
            new Internal.DotNetResources.PreserializedResourceWriter(targetstream))
        { }
    }

    /// <summary>
    /// Transfers custom MS-INI resources built with <see cref="MsIniResourcesWriter"/> and writes them to 
    /// the preserialized .NET <see cref="T:System.Resources.Extensions.PreserializedResourceWriter"/> format.
    /// </summary>
    public sealed class CustomMsIniToBinaryTransferer : DefaultResourcesTransferer
    {
        /// <summary>
        /// Create a new instance of <see cref="CustomMsIniToBinaryTransferer"/> from the specified 
        /// input and target file paths.
        /// </summary>
        /// <param name="Input">The file path to get the resources from.</param>
        /// <param name="Output">The file path to write the resources to.</param>
        public CustomMsIniToBinaryTransferer(System.String Input , System.String Output) : base()
        {
            reader = new MsIniResourcesReader(Input);
            writer = new Internal.DotNetResources.PreserializedResourceWriter(Output);
            PrepareInstance();
        }

        /// <summary>
        /// Create a new instance of <see cref="CustomMsIniToBinaryTransferer"/> from the specified 
        /// input and target streams.
        /// </summary>
        /// <param name="input">The input stream to get the resources from.</param>
        /// <param name="targetstream">The target stream to write the resources to.</param>
        public CustomMsIniToBinaryTransferer(System.IO.Stream input , System.IO.Stream targetstream) : base()
        {
            reader = new MsIniResourcesReader(input);
            writer = new Internal.DotNetResources.PreserializedResourceWriter(targetstream);
            PrepareInstance();
        }
    }
}
