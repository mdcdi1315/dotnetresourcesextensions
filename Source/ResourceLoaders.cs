/*
    Resource Loaders file.
    This file defines the resource loaders shipped with this library.
 */

using System;
using System.IO;

namespace DotNetResourcesExtensions
{

#if NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER
    /// <summary>
    /// This class loads and uses resources loaded from .NET <see cref="System.Resources.ResourceReader"/> class. <br />
    /// Supported only for .NET Framework and .NET Standard targets. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class DotNetOldResourceLoader : OptimizedResourceLoader
    {
        /// <summary>
        /// Creates a new instance of <see cref="DotNetOldResourceLoader"/> with the specified path to a .resources file.
        /// </summary>
        /// <param name="Path">The path to the .resources file.</param>
        public DotNetOldResourceLoader(System.String Path) : base()
        {
            read = new System.Resources.ResourceReader(Path);
        }

        /// <summary>
        /// Creates a new instance of <see cref="DotNetOldResourceLoader"/> from a stream that contains resource data.
        /// </summary>
        /// <param name="str">The stream that contains resource data.</param>
        public DotNetOldResourceLoader(System.IO.Stream str) : base()
        {
            read = new System.Resources.ResourceReader(str);
        }
    }
#endif

#if NET472_OR_GREATER || WINDOWS10_0_19041_0_OR_GREATER
    /// <summary>
    /// This class loads and uses resources loaded from .NET <see cref="System.Resources.ResXResourceReader"/> class. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class ResXResourceLoader : OptimizedResourceLoader
    {
        /// <summary>
        /// Creates a new instance of <see cref="ResXResourceLoader"/> with the specified path to a .resources file.
        /// </summary>
        /// <param name="Path">The path to the .resources file.</param>
        public ResXResourceLoader(System.String Path) : base()
        {
            read = new System.Resources.ResXResourceReader(Path);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ResXResourceLoader"/> from a stream that contains resource data.
        /// </summary>
        /// <param name="str">The stream that contains resource data.</param>
        public ResXResourceLoader(System.IO.Stream str) : base()
        {
            read = new System.Resources.ResXResourceReader(str);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ResXResourceLoader"/> from a stream that contains resource data.
        /// </summary>
        /// <param name="str">The stream that contains resource data.</param>
        /// <param name="typeresolver">The type resolver to use when loading resources.</param>
        public ResXResourceLoader(System.IO.Stream str, System.ComponentModel.Design.ITypeResolutionService typeresolver) : base()
        {
            read = new System.Resources.ResXResourceReader(str, typeresolver);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ResXResourceLoader"/> from a stream that contains resource data.
        /// </summary>
        /// <param name="str">The stream that contains resource data.</param>
        /// <param name="assemblyNames">A list of assembly names to consider when loading resources.</param>
        public ResXResourceLoader(System.IO.Stream str, System.Reflection.AssemblyName[] assemblyNames) : base()
        {
            read = new System.Resources.ResXResourceReader(str, assemblyNames);
        }
    }
#endif

    /// <summary>
    /// This class loads and uses resources loaded from .NET <see cref="System.Resources.Extensions.DeserializingResourceReader"/> class. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class DotNetResourceLoader : OptimizedResourceLoader
    {
        /// <summary>
        /// Creates a new instance of <see cref="DotNetResourceLoader"/> with the specified path to a .resources file.
        /// </summary>
        /// <param name="Path">The path to the .resources file.</param>
        public DotNetResourceLoader(System.String Path) : base()
        {
            read = new System.Resources.Extensions.DeserializingResourceReader(Path);
        }

        /// <summary>
        /// Creates a new instance of <see cref="DotNetResourceLoader"/> from a stream that contains resource data.
        /// </summary>
        /// <param name="str">The stream that contains resource data.</param>
        public DotNetResourceLoader(System.IO.Stream str) : base()
        {
            read = new System.Resources.Extensions.DeserializingResourceReader(str);
        }
    }

    /// <summary>
    /// The JSON Resource Loader is just another loader that loads the resources using the preserialised JSON <br />
    /// written from the <see cref="JSONResourcesWriter"/> class. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class JSONResourcesLoader : OptimizedResourceLoader
    {
        /// <summary>
        /// Creates a new instance of <see cref="JSONResourcesLoader"/> with the specified path to a .resources file.
        /// </summary>
        /// <param name="Path">The path to the .resources file.</param>
        public JSONResourcesLoader(System.String Path) : base()
        {
            read = new JSONResourcesReader(Path);
        }

        /// <summary>
        /// Creates a new instance of <see cref="JSONResourcesLoader"/> from a stream that contains resource data.
        /// </summary>
        /// <param name="str">The stream that contains resource data.</param>
        public JSONResourcesLoader(System.IO.Stream str) : base()
        {
            read = new JSONResourcesReader(str);
        }

    }

    /// <summary>
    /// The XML Resource Loader is just another loader that loads the resources using the preserialised XML <br />
    /// written from the <see cref="XMLResourcesWriter"/> class. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class XMLResourcesLoader : OptimizedResourceLoader
    {
        /// <summary>
        /// Creates a new instance of <see cref="XMLResourcesLoader"/> with the specified path to a .resources file.
        /// </summary>
        /// <param name="Path">The path to the .resources file.</param>
        public XMLResourcesLoader(System.String Path) : base()
        {
            read = new XMLResourcesReader(Path);
        }

        /// <summary>
        /// Creates a new instance of <see cref="XMLResourcesLoader"/> from a stream that contains resource data.
        /// </summary>
        /// <param name="stream">The stream that contains resource data.</param>
        public XMLResourcesLoader(System.IO.Stream stream) : base()
        {
            read = new XMLResourcesReader(stream);
        }
    }

    /// <summary>
    /// The <see cref="CustomDataResourcesLoader"/> class loads resources using the <see cref="CustomBinaryResourceReader"/> class.
    /// </summary>
    public sealed class CustomDataResourcesLoader : OptimizedResourceLoader
    {
        /// <summary>
        /// Creates a new instance of <see cref="CustomDataResourcesLoader"/> with the specified path to a .resources file.
        /// </summary>
        /// <param name="Path">The path to the .resources file.</param>
        public CustomDataResourcesLoader(System.String Path) : base()
        {
            read = new CustomBinaryResourceReader(Path);
        }

        /// <summary>
        /// Creates a new instance of <see cref="CustomDataResourcesLoader"/> from a stream that contains resource data.
        /// </summary>
        /// <param name="stream">The stream that contains resource data.</param>
        public CustomDataResourcesLoader(System.IO.Stream stream) : base()
        {
            read = new CustomBinaryResourceReader(stream);
        }
    }

    /// <summary>
    /// Loads and gets resources built from the library's ResX resource writer , the <see cref="Internal.ResX.ResXResourceWriter"/> class.
    /// </summary>
    public sealed class CustomResXResourcesLoader : OptimizedResourceLoader
    {
        /// <summary>
        /// Creates a new instance of <see cref="CustomDataResourcesLoader"/> with the specified path to a custom .ResX resources file.
        /// </summary>
        /// <param name="Path">The path to the custom ResX resources file.</param>
        public CustomResXResourcesLoader(System.String Path) : base()
        {
            read = new Internal.ResX.ResXResourceReader(Path);
        }

        /// <summary>
        /// Creates a new instance of <see cref="CustomDataResourcesLoader"/> 
        /// with the specified stream that has data of a custom .ResX resources format.
        /// </summary>
        /// <param name="stream">The stream that contains resource data.</param>
        public CustomResXResourcesLoader(System.IO.Stream stream) : base()
        {
            read = new Internal.ResX.ResXResourceReader(stream);
        }

        /// <summary>
        /// Creates a new instance of <see cref="CustomDataResourcesLoader"/> 
        /// with the specified reader that has data of a custom .ResX resources format.
        /// </summary>
        /// <param name="reader">The reader that contains resource data.</param>
        public CustomResXResourcesLoader(System.IO.TextReader reader) : base()
        {
            read = new Internal.ResX.ResXResourceReader(reader);
        }

        /// <summary>
        /// Creates a new instance of <see cref="CustomDataResourcesLoader"/> 
        /// with the specified stream that has data of a custom .ResX resources format , and
        /// the type resolution service to use during type lookup.
        /// </summary>
        /// <param name="stream">The stream that contains resource data.</param>
        /// <param name="ttrs">The type resolution service to use.</param>
        public CustomResXResourcesLoader(System.IO.Stream stream , 
            System.ComponentModel.Design.ITypeResolutionService ttrs) : base()
        {
            read = new Internal.ResX.ResXResourceReader(stream, ttrs);
        }

        /// <summary>
        /// Creates a new instance of <see cref="CustomDataResourcesLoader"/> 
        /// with the specified reader that has data of a custom .ResX resources format , and
        /// the type resolution service to use during type lookup. 
        /// </summary>
        /// <param name="reader">The reader that contains resource data.</param>
        /// <param name="ttrs">The type resolution service to use.</param>
        public CustomResXResourcesLoader(System.IO.TextReader reader ,
            System.ComponentModel.Design.ITypeResolutionService ttrs) : base()
        {
            read  = new Internal.ResX.ResXResourceReader(reader, ttrs);
        }

    }

    /// <summary>
    /// The <see cref="CustomMsIniResourcesLoader"/> loads resources written using the <see cref="MsIniResourcesWriter"/> class.
    /// </summary>
    public sealed class CustomMsIniResourcesLoader : OptimizedResourceLoader
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CustomMsIniResourcesLoader"/> with the specified path to a file that contains the resource data to read.
        /// </summary>
        /// <param name="Path">The file path that contains the resource data to read.</param>
        public CustomMsIniResourcesLoader(System.String Path) : base() 
        {
            read = new MsIniResourcesReader(Path);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CustomMsIniResourcesLoader"/> with the specified stream that contains the resource data to read.
        /// </summary>
        /// <param name="stream">The stream to read data from.</param>
        public CustomMsIniResourcesLoader(System.IO.Stream stream) : base()
        {
            read = new MsIniResourcesReader(stream);
        }

    }

}
