
extern alias DRESEXT;

using System;
using System.Collections;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using DRESEXT::DotNetResourcesExtensions;

namespace DotNetResourcesExtensions.BuildTasks
{
    /// <summary>
    /// Generates resources but it uses as it's backend the <see cref="DotNetResourcesExtensions"/> library.
    /// </summary>
    [RunInMTA]
    public class DotNetResExtGenerator : Microsoft.Build.Utilities.Task
    {
        private const MessageImportance DefaultImportance = MessageImportance.Low;
        private IResourceTransferer transferer;
        private System.IO.FileStream[] streams;
        private System.Resources.IResourceWriter target;
        private System.Byte strindex;
        private OutputResourceType restype;

        public DotNetResExtGenerator() : base()
        {
            strindex = 0;
            target = null;
            streams = new System.IO.FileStream[50];
            transferer = null;
            HelpKeywordPrefix = null;
            restype = OutputResourceType.Resources;
        }

        public override bool Execute()
        {
            System.Boolean val = false;
            try {
                val = UnsafeExecute();
            } catch (Exception ex) {
                ProduceError("DNTRESEXT0000", $"Unknown error occured during execution: \n{ex}");
                return false;
            }
            transferer?.Dispose();
            target?.Dispose();
            for (System.Int32 I = 0; I < streams.Length; I++) { streams[I]?.Dispose(); }
            return val;
        }

        private void ProduceError(System.String code , System.String msg)
        {
            Log.LogError("", code, "", "", "<Non-Existent>", 0, 0, 0, 0, msg);
        }

        private void ProduceWarning(System.String code, System.String msg)
        {
            Log.LogWarning("", code, "", "", "<Non-Existent>", 0, 0, 0, 0, msg);
        }

        private bool UnsafeExecute()
        {
            System.Int32 avcores = BuildEngine9.RequestCores(2);
            if (avcores == 0) 
            {
                ProduceError("DNTRESEXT0002", "Cannot allocate at least one core for generating the resources.");
                return false;
            }
            if (OutputFilePath == null || OutputFilePath.ItemSpec == null) 
            {
                ProduceError("DNTRESEXT0001" , "A target output file was not specified. Please specify a valid path , then retry. ");
                return false;
            }
            if (restype == OutputResourceType.Resources)
            {
                target = new DRESEXT::DotNetResourcesExtensions.Internal.DotNetResources.PreserializedResourceWriter(OutputFilePath.ItemSpec);
            } else if (restype == OutputResourceType.CustomBinary)
            {
                target = new CustomBinaryResourceWriter(OutputFilePath.ItemSpec);
            } else if (restype == OutputResourceType.JSON)
            {
                target = new JSONResourcesWriter(OutputFilePath.ItemSpec);
            } else
            {
                ProduceError("DNTRESEXT0003", $"Unknown output file format specified: {OutputFileType}");
                return false;
            }
            System.Boolean isfirst = true;
            BuildEngine9.Yield();
            foreach (var file in InputFiles)
            {
                var dd = GetReaderFromPath(file.ItemSpec);
                if (isfirst)
                {
                    transferer = new AbstractResourceTransferer(dd , target);
                    transferer.TransferAll();
                } else {
                    DictionaryEntry d;
                    IDictionaryEnumerator de = dd.GetEnumerator();
                    while (de.MoveNext())
                    {
                        d = de.Entry;
                        // The casts might even be redundant , but we do not care since this is done at compile-time.
                        if (d.Value is String ab) {
                            transferer.AddResource((System.String)d.Key, ab);
                        } else if (d.Value is System.Byte[] bytep) {
                            transferer.AddResource((System.String)d.Key, bytep);
                        } else {
                            transferer.AddResource((System.String)d.Key, d.Value);
                        }
                    }
                    de.Reset();
                    de = null;
                }
            }
            BuildEngine9.Reacquire();
            if (GenerateStronglyTypedClass)
            {
                if (String.IsNullOrWhiteSpace(StronglyTypedClassManifestName))
                {
                    ProduceError("DNTRESEXT0004" , $"Cannot generate code because the {nameof(StronglyTypedClassManifestName)} was not specified.");
                    return false;
                }
                if (String.IsNullOrWhiteSpace(StronglyTypedClassOutPath))
                {
                    ProduceError("DNTRESEXT0005", "Cannot generate code because a valid output path was not specified. Please specify one and retry.");
                    return false;
                }
                if (String.IsNullOrWhiteSpace(StronglyTypedClassLanguage))
                {
                    ProduceWarning("DNTRESEXT0006", "The strongly typed-class language was not specified. Presuming that it is \'CSharp\'.");
                    StronglyTypedClassLanguage = "CSharp";
                }
                if (String.IsNullOrWhiteSpace(StronglyTypedClassName))
                {
                    ProduceWarning("DNTRESEXT0009", $"The strongly typed-class .NET name was not specified. Presuming that it is the manifest name: \"{StronglyTypedClassManifestName}\"");
                    StronglyTypedClassName = StronglyTypedClassManifestName;
                }
                GenerateStrTypedClass();
            }
            return true;
        }

        private void GenerateStrTypedClass()
        {
            target?.Close();
            target?.Dispose();
            IResourceLoader loader = null;
            try {
                if (restype == OutputResourceType.Resources) {
                    loader = new DotNetResourceLoader(OutputFilePath.ItemSpec);
                } else if (restype == OutputResourceType.CustomBinary) {
                    loader = new CustomDataResourcesLoader(OutputFilePath.ItemSpec);
                } else if (restype == OutputResourceType.JSON) {
                    loader = new JSONResourcesLoader(OutputFilePath.ItemSpec);
                }
                switch (StronglyTypedClassLanguage.ToLower()) {
                    case "csharp":
                    case "c#":
                        StronglyTypedCodeProviderBuilder.WithCSharp(loader , StronglyTypedClassManifestName , StronglyTypedClassName , StronglyTypedClassOutPath , ResourceClassVisibilty.Internal);
                        break;
                    case "visualbasic":
                    case "vb":
                        StronglyTypedCodeProviderBuilder.WithVisualBasic(loader, StronglyTypedClassManifestName, StronglyTypedClassName, StronglyTypedClassOutPath, ResourceClassVisibilty.Internal);
                        break;
                }
            } catch (System.Exception e) {
                Log.LogErrorFromException(e);
            } finally { 
                loader?.Dispose();
            }
        }

        private System.Resources.IResourceReader GetReaderFromPath(System.String path)
        {
            Log.LogMessage(DefaultImportance , "Getting resource reader for file {0} ..." , path);
            System.IO.FileInfo FI = null;
            System.Resources.IResourceReader rdr = null; 
            try {
                FI = new(path);
                streams[strindex] = FI.OpenRead();
                switch (FI.Extension)
                {
                    case ".rescx":
                        rdr = new DRESEXT::DotNetResourcesExtensions.Internal.ResX.ResXResourceReader(streams[strindex]);
                        break;
                    case ".resj":
                        rdr = new JSONResourcesReader(streams[strindex]);
                        break;
                    case ".resxx":
                        rdr = new XMLResourcesReader(streams[strindex]);
                        break;
                    case ".resi":
                        rdr = new MsIniResourcesReader(streams[strindex]);
                        break;
                    case ".txt":
                        rdr = new KVPResourcesReader(streams[strindex]);
                        break;
                    case ".resx":
                        rdr = new System.Resources.ResXResourceReader(streams[strindex]);
                        break;
                }
            } catch (System.Exception ex) {
                Log.LogWarning("Could not load file {0} ... See next warning for more information." , path);
                Log.LogWarningFromException(ex); 
                return null;
            }
            return rdr;
        }

        /// <summary>
        /// Gets the input resource files to add.
        /// </summary>
        public ITaskItem[] InputFiles { get; set; }

        /// <summary>
        /// Defines the save output path to the file.
        /// </summary>
        public ITaskItem OutputFilePath { get; set; }

        /// <summary>
        /// The output file type. Must be one of the constants defined in <see cref="OutputResourceType"/> enumeration.
        /// </summary>
        public System.String OutputFileType { 
            get => restype.ToString();
            set {
                try {
                    restype = (OutputResourceType)System.Enum.Parse(typeof(OutputResourceType), value);
                } catch (ArgumentException e) {
                    ProduceWarning("DNTRESEXT0007", $"The value specified , {value} was not accepted because of \n {e} .");
                    ProduceWarning("DNTRESEXT0008", "Setting the OutputFileType back to Resources due to an error. See above for more information.");
                    restype = OutputResourceType.Resources;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value whether to generate a strongly-typed class for the current resource.
        /// </summary>
        public System.Boolean GenerateStronglyTypedClass { get; set; }

        /// <summary>
        /// Gets or sets the underlying manifest name of the actual resource. Must be compilant with it.
        /// </summary>
        public System.String StronglyTypedClassManifestName { get; set; }

        /// <summary>
        /// The output path to save the generated strongly-typed class.
        /// </summary>
        public System.String StronglyTypedClassOutPath { get; set; }

        /// <summary>
        /// The progamming language to produce the strongly-typed class for. <br />
        /// Available values: csharp , visualbasic. This string is case-insensitive.
        /// </summary>
        public System.String StronglyTypedClassLanguage { get; set; }

        /// <summary>
        /// The strongly-typed full class and namespace name to generate. <br />
        /// The name after the last dot is the class name , while before it is (or are) the namespace name (or names).
        /// </summary>
        public System.String StronglyTypedClassName { get; set; }
    }

    public enum OutputResourceType : System.Byte
    {
        Resources ,
        CustomBinary,
        JSON
    }

    public sealed class OutputResourceFileItem : ITaskItem
    {
        private System.String path;
        private OutputResourceType type;
        private Dictionary<System.String, System.String> custommetadata , resmetadata;

        public OutputResourceFileItem() 
        {
            path = null;
            type = OutputResourceType.Resources;
            custommetadata = new();
            resmetadata = new();
        }

        public OutputResourceFileItem(System.String path) : this()
        {
            this.path = path;
        }

        public string ItemSpec 
        { 
            get => path; 
            set {
                path = value;
                SetReservedMetadata("FullPath", path);
            }
        }

        public OutputResourceType FileResultType { get => type; set => type = value; }

        public ICollection MetadataNames
        {
            get {
                List<System.String> all = new(custommetadata.Keys);
                all.AddRange(resmetadata.Keys);
                return all;
            }
        }

        public int MetadataCount => custommetadata.Count + resmetadata.Count;

        public IDictionary CloneCustomMetadata()
        {
            Dictionary<System.String, System.String> cloned = new(custommetadata);
            return cloned;
        }

        public void CopyMetadataTo(ITaskItem destinationItem)
        {
            foreach (var kvp in custommetadata)
            {
                System.String v = destinationItem.GetMetadata(kvp.Key);
                if (v == null || v == System.String.Empty) 
                {
                    destinationItem.SetMetadata(kvp.Key, kvp.Value);
                }
            }
        }

        public string GetMetadata(string metadataName)
        {
            custommetadata.TryGetValue(metadataName, out var result);
            return result;
        }

        public void RemoveMetadata(string metadataName)
        {
            custommetadata.Remove(metadataName);
        }

        public void SetMetadata(string metadataName, string metadataValue)
        {
            if (custommetadata.ContainsKey(metadataName))
            {
                custommetadata[metadataName] = metadataValue;
            } else
            {
                custommetadata.Add(metadataName, metadataValue);
            }
        }
    
        private void SetReservedMetadata(System.String metadataname , System.String metadatavalue)
        {
            if (resmetadata.ContainsKey(metadataname))
            {
                resmetadata[metadataname] = metadatavalue;
            } else {
                resmetadata.Add(metadataname , metadatavalue);
            }
        }

        ~OutputResourceFileItem() {
            custommetadata?.Clear();
            custommetadata = null;
            resmetadata?.Clear();
            resmetadata = null;
        }
    }
}
