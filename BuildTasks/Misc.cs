
using System;
using System.Threading.Tasks;

namespace DotNetResourcesExtensions.BuildTasks
{

    internal record class OutputItemData
    {
        public System.Boolean HasValidData;
        public System.String FilePath;
        public OutputResourceType OutType;
        public InputItemData[] Inputs;
    }

    internal record class InputItemData
    {
        public System.Boolean HasValidData;
        public System.String FilePath;
        public System.Boolean GenerateStrClass;
        public System.String OutputStrFilePath;
        public System.String ManifestResourceName;
        public System.String ClassName;
        public System.String ClassLang;
        public ResourceClassVisibilty ClsVisibility;
    }

    public enum OutputResourceType : System.Byte
    {
        Resources,
        CustomBinary,
        JSON
    }

    /// <summary>
    /// Provides a very minimal resource loader. It directly wraps a IResourceReader instance without additional checks.
    /// </summary>
    internal sealed class MinimalResourceLoader : OptimizedResourceLoader
    {
        public MinimalResourceLoader(System.Resources.IResourceReader rdr) : base() { read = rdr; }

        public override void Dispose()
        {
            read = null; // Directly set this so as to avoid of being disposed accidentally by the internal mechanisms.
            base.Dispose();
        }

        public override ValueTask DisposeAsync() => new(Task.Run(Dispose));
    }


}