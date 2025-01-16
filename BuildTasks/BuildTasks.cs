
using System;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using DotNetResourcesExtensions.Internal.ResX;
using DotNetResourcesExtensions.Internal.DotNetResources;

namespace DotNetResourcesExtensions.BuildTasks
{
    /// <summary>
    /// Generates resources but it uses as it's backend the <see cref="DotNetResourcesExtensions"/> library.
    /// </summary>
    [RunInMTA]
    public class DotNetResExtGenerator : Microsoft.Build.Utilities.Task
    {
        private const MessageImportance DefaultImportance = MessageImportance.Low;
        private List<InputItemData> validatedinputs;
        private IResourceTransferer transferer;
        private static System.Boolean resolverconnected = false;

        private System.Reflection.Assembly Resolver(System.Object sender, System.ResolveEventArgs e)
        {
            System.IO.DirectoryInfo DI = DownloadDeps.BuildTasksPath;
            System.Reflection.AssemblyName AN = new(e.Name);
            Log.LogMessage(MessageImportance.Normal, "Path assembly prober of BuildTasks searches for a file called \"{0}\"." , AN.FullName);
            foreach (var file in DI.GetFiles("*.dll"))
            {
                if ($"{AN.Name}.dll" == file.Name)
                {
                    Log.LogMessage(MessageImportance.Normal, "Path assembly prober found a matching file called \"{0}\" with fully resolved path to be \"{1}\"." , file.Name , file.FullName);
                    Log.LogMessage(MessageImportance.Normal, "Loaded assembly details: Version: {0}" , AN.Version);
                    return System.Reflection.Assembly.LoadFrom(file.FullName);
                }
            }
            Log.LogMessage(MessageImportance.Normal, "Path assembly prober could not find a matching file for name \"{0}\".", e.Name);
            return null;
        }

        ~DotNetResExtGenerator() 
        {
            if (resolverconnected) { System.AppDomain.CurrentDomain.AssemblyResolve -= Resolver; resolverconnected = false; }
        }

        public DotNetResExtGenerator() : base()
        {  
            if (resolverconnected == false)
            {
                System.AppDomain.CurrentDomain.AssemblyResolve += Resolver;
            }
            transferer = null;
            validatedinputs = new(10);
            HelpKeywordPrefix = null;
        }

        public override bool Execute()
        {
            System.Boolean val;
            try {
                val = UnsafeExecute();
            } catch (Exception ex) {
                this.LogExceptionClass(new UnexpectedErrorException(ex));
                return false;
            }
            transferer?.Dispose();
            return val;
        }

        private static System.String GetMetadataNames(ICollection collection)
        {
            System.String result = System.String.Empty;
            foreach (System.Object val in collection) { result += val.ToString() + " "; }
            return result;
        }

        private System.Boolean UnsafeExecute()
        {
            if (InputFiles is null || OutputFiles is null) {
                return false;
            }
            System.Boolean dtb = IsDTBBuild is not null && IsDTBBuild.Equals("true" , StringComparison.OrdinalIgnoreCase);
            foreach (var fe in InputFiles) { ValidateInputFileItemFormat(fe); }
            if (validatedinputs.Count == 0) { return false; }
            List<OutputItemData> outitems = new(10);
            foreach (var fe in OutputFiles)
            {
                var item = ValidateOutputFileItemFormat(fe);
                if (item.HasValidData == false) { return false; }
                outitems.Add(item);
            }
            if (dtb) {
                return ExecuteInDTB(outitems);
            } else {
                return ExecuteInNonDTB(outitems);
            }
        }

        private System.Boolean ExecuteInNonDTB(IEnumerable<OutputItemData> outfiles)
        {
            foreach (var valid in outfiles)
            {
                System.IO.FileStream fso = null;
                try 
                {
                    Log.LogMessage("Starting Building file {0}..." , valid.FilePath);
                    fso = new(valid.FilePath, System.IO.FileMode.Create);
                    IDotNetResourcesExtensionsWriter writer = null;
                    switch (valid.OutType)
                    {
                        case OutputResourceType.Resources:
                            writer = new PreserializedResourceWriter(fso);
                            break;
                        case OutputResourceType.CustomBinary:
                            writer = new CustomBinaryResourceWriter(fso);
                            break;
                        case OutputResourceType.JSON:
                            writer = new JSONResourcesWriter(fso);
                            break;
                    }
                    System.IO.FileStream srcstreamshared = null;
                    Log.LogMessage("Reading file {0}..." , valid.Inputs[0].FilePath);
                    var rdr = GetReaderFromPath(valid.Inputs[0].FilePath, ref srcstreamshared);
                    try {
                        if (rdr is null) { this.ThrowMessage(4); return false; }
                        GenerateStrTypedClassForItem(rdr, valid.Inputs[0], valid);
                        Log.LogMessage("Creating resource transferer object.");
                        transferer = new AbstractResourceTransferer(rdr, writer);
                        Log.LogMessage("Transferring resources...");
                        transferer.TransferAll();
                        for (System.Int32 I = 1; I < valid.Inputs.Length; I++)
                        {
                            Log.LogMessage("Reading file {0}...", valid.Inputs[I].FilePath);
                            var rd2 = GetReaderFromPath(valid.Inputs[I].FilePath, ref srcstreamshared);
                            if (rd2 is null) { continue; }
                            try
                            {
                                GenerateStrTypedClassForItem(rd2, valid.Inputs[I], valid);
                                Log.LogMessage("Transferring resources...");
                                foreach (DictionaryEntry res in rd2)
                                {
                                    System.String name = res.Key as System.String;
                                    if (res.Value is System.String str) {
                                        transferer.AddResource(name, str);
                                    } else if (res.Value is System.Byte[] array) {
                                        transferer.AddResource(name, array);
                                    } else {
                                        transferer.AddResource(name, res.Value);
                                    }
                                }
                            } finally {
                                rd2.Dispose();
                            }
                        }
                    } finally {
                        rdr?.Dispose();
                        rdr = null;
                        srcstreamshared?.Dispose();
                        srcstreamshared = null;
                        transferer?.Dispose();
                        writer?.Dispose();
                        writer = null;
                    }
                } finally {
                    fso?.Dispose();
                    fso = null;
                }
                Log.LogMessage("Done! File was written successfully.");
            }
            return true;
        }

        private System.Boolean ExecuteInDTB(IEnumerable<OutputItemData> outfiles)
        {
            System.IO.FileStream fsm = null;
            foreach (var valid in outfiles)
            {
                foreach (var inputf in valid.Inputs)
                {
                    var reader = GetReaderFromPath(inputf.FilePath , ref fsm);
                    if (reader is null) { return false; }
                    try {
                        GenerateStrTypedClassForItem(reader, inputf, valid);
                    } finally {
                        reader.Dispose();
                        reader = null;
                    }
                }
            }
            return true;
        }

        // Generates a strongly-typed class from the specified task item.
        private void GenerateStrTypedClassForItem(System.Resources.IResourceReader rdr , InputItemData inputfile , OutputItemData forfile)
        {
            if (inputfile.GenerateStrClass == false) { return; }
            MinimalResourceLoader mrl = new(rdr);
            Log.LogMessage("Creating Strongly-Typed resource class for item {0}..." , inputfile.FilePath);
            switch (inputfile.ClassLang.ToLower())
            {
                case "csharp":
                case "c#":
                    StronglyTypedCodeProviderBuilder.WithCSharp(mrl, 
                        inputfile.ManifestResourceName,
                        inputfile.ClassName, 
                        inputfile.OutputStrFilePath,
                        inputfile.ClsVisibility, forfile.OutType);
                    break;
                case "visualbasic":
                case "vb":
                    StronglyTypedCodeProviderBuilder.WithVisualBasic(mrl,
                        inputfile.ManifestResourceName,
                        inputfile.ClassName,
                        inputfile.OutputStrFilePath,
                        inputfile.ClsVisibility, forfile.OutType);
                    break;
            }
            Log.LogMessage("The Strongly-Typed resource class for item {0} created successfully.", inputfile.FilePath);
            mrl.Dispose();
            mrl = null;
        }

        private System.Resources.IResourceReader GetReaderFromPath(System.String path , ref System.IO.FileStream streamshared)
        {
            Log.LogMessage(DefaultImportance , "Getting resource reader for file {0} ..." , path);
            System.IO.FileInfo FI = null;
            System.Resources.IResourceReader rdr = null; 
            try {
                FI = new(path);
                streamshared = FI.OpenRead();
                switch (FI.Extension)
                {
                    case ".rescx":
                        rdr = new ResXResourceReader(streamshared);
                        break;
                    case ".resj":
                        rdr = new JSONResourcesReader(streamshared);
                        break;
                    case ".resxx":
                        rdr = new XMLResourcesReader(streamshared);
                        break;
                    case ".resi":
                        rdr = new MsIniResourcesReader(streamshared);
                        break;
                    case ".txt":
                        rdr = new KVPResourcesReader(streamshared);
                        break;
                    case ".resh":
                        this.ThrowMessage(118);
                        rdr = new HumanReadableFormatReader(streamshared);
                        break;
#if WF_AVAILABLE
                    case ".resx":
                        rdr = new System.Resources.ResXResourceReader(streamshared);
                        break;
#endif
                    default:
                        streamshared?.Dispose();
                        streamshared = null;
                        this.ThrowMessage(213, FI.FullName);
                        break;
                }
            } catch (System.Exception ex) {
                Log.LogWarning("Could not load file {0} ... See next warning for more information." , path);
                Log.LogWarningFromException(ex , true); 
                return null;
            }
            return rdr;
        }

        // Validates an output file based on the given task item.
        // The returned object then specifies the correct data for the output file.
        // If the layout was invalid , or one of the input files were not found , it returns an object
        // with it's HasValidData field to be false.
        private OutputItemData ValidateOutputFileItemFormat(ITaskItem item)
        {
            OutputItemData dt = new();
            dt.HasValidData = false;
            if (item is null) { return dt; }
            if (item.MetadataCount < 2) { throw new FormatException("The OutputResourceFileDef item format requires the OutputType and the Inputs properties to exist."); }
            System.String tempstr = item.GetMetadata("OutputType");
            if (System.String.IsNullOrWhiteSpace(tempstr))
            {
                this.ThrowMessage(9);
                return dt;
            }
            dt.OutType = GetResTypeFromString(tempstr);
            dt.FilePath = item.ItemSpec;
            tempstr = item.GetMetadata("Inputs");
            if (System.String.IsNullOrWhiteSpace(tempstr))
            {
                this.ThrowMessage(10);
                return dt;
            }
            System.String[] paths = tempstr.Split(new char[] { ';' } , StringSplitOptions.RemoveEmptyEntries);
            System.Int32 fd = 0;
            List<InputItemData> inputs = new(10);
            foreach (var file in paths)
            {
                if (System.IO.File.Exists(file) == false) { this.ThrowMessage(11, file); return dt; }
                foreach (var vi in validatedinputs)
                {
                    if (vi.FilePath == file) { fd++; inputs.Add(vi); break; }
                }
            }
            if (fd != paths.Length) {
                this.ThrowMessage(12 , fd , paths.Length);
                return dt;
            }
            dt.Inputs = inputs.ToArray();
            inputs.Clear();
            inputs = null;
            dt.HasValidData = true;
            return dt;
        }

        // Validates an input file based on the given task item.
        // It adds it to the list of valid inputs if the given item is valid.
        // The given task item must conform to the format provided inside GeneratableResource.
        // If not , it throws a FormatException and explains which situation it found.
        private void ValidateInputFileItemFormat(ITaskItem item)
        {
            InputItemData idt = new();
            idt.HasValidData = false;
            static ResourceClassVisibilty ParseVisibility(System.String frommeta) => frommeta.ToLower() switch
            {
                "internal" => ResourceClassVisibilty.Internal,
                "public" => ResourceClassVisibilty.Public,
                _ => ResourceClassVisibilty.Internal,
            };
            System.String[] expecttofind = { "GenerateStrClass", "StrClassLanguage", "StrClassName", "StrClassManifestName", "StrOutPath", "StrClassVisibility" };
            System.Byte found = 0;
            foreach (System.Object obj in item.MetadataNames)
            {
                foreach (System.String str in expecttofind)
                {
                    if (str == obj.ToString()) { found++; break; }
                }
            }
            if (found < expecttofind.Length) {
                throw new FormatException($"INTERNAL ERROR: One or more metadata fields for this item are missing or have incorrect names. Expected {expecttofind.Length} but retrieved {found}.");
            }
            // Determine whether the user wants to generate a str for this input. If not , just set false to the field.
            // Any string input that matches the letters false should prevent from creating a str.
            idt.GenerateStrClass = item.GetMetadata(expecttofind[0]).ToLower() != "false";
            if (idt.GenerateStrClass) // Only validate when a new resource class is requested!
            {
                // OK. Now we need to validate all input data for the strongly-typed resource class.
                if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[3])))
                {
                    this.ThrowMessage(40, expecttofind[3], item.ItemSpec);
                    return;
                }
                if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[4])))
                {
                    this.ThrowMessage(41, item.ItemSpec);
                    return;
                }
                if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[1])))
                {
                    this.ThrowMessage(42);
                    item.SetMetadata(expecttofind[1], "CSharp");
                }
                if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[2])))
                {
                    this.ThrowMessage(43, item.GetMetadata(expecttofind[3]));
                    item.SetMetadata(expecttofind[2], item.GetMetadata(expecttofind[3]));
                }
                if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[5])))
                {
                    this.ThrowMessage(44);
                    item.SetMetadata(expecttofind[5], "Internal");
                }
                idt.FilePath = item.ItemSpec;
                if (System.IO.File.Exists(idt.FilePath) == false)
                {
                    this.ThrowMessage(11, idt.FilePath);
                    return;
                }
                idt.ClassLang = item.GetMetadata(expecttofind[1]);
                idt.ManifestResourceName = item.GetMetadata(expecttofind[3]);
                idt.ClassName = item.GetMetadata(expecttofind[2]);
                idt.OutputStrFilePath = item.GetMetadata(expecttofind[4]);
                idt.ClsVisibility = ParseVisibility(item.GetMetadata(expecttofind[5]));
            }
            idt.HasValidData = true;
            validatedinputs.Add(idt);
        }

        private static OutputResourceType GetResTypeFromString(System.String data)
            => data.ToLower() switch { 
                "resources" => OutputResourceType.Resources,
                "custombinary" => OutputResourceType.CustomBinary,
                "json" => OutputResourceType.JSON,
                _ => throw new FormatException("The Resource File Type must be a valid output resource type.")
            };

        /// <summary>
        /// Gets the input resource files to add. <br />
        /// Additionally these task items are expected to have the same layout as exported from GeneratableResource.
        /// </summary>
        [Required]
        public ITaskItem[] InputFiles { get; set; }
        
        /// <summary>
        /// Gets the output resource files that will contain the resources defined in <see cref="InputFiles"/> property.
        /// </summary>
        [Required]
        public ITaskItem[] OutputFiles { get; set; }

        /// <summary>
        /// Gets a value whether the current build that the resource generator runs into is a design-time build.
        /// </summary>
        public System.String IsDTBBuild { get; set; }
    }

    public enum OutputResourceType : System.Byte
    {
        Resources ,
        CustomBinary,
        JSON
    }

    /// <summary>
    /// Provides a very minimal resource loader. It directly wraps a IResourceReader instance without additional checks.
    /// </summary>
    internal sealed class MinimalResourceLoader : OptimizedResourceLoader
    {
        public MinimalResourceLoader(System.Resources.IResourceReader rdr) : base() { read = rdr; }

        public override void Dispose() {
            read = null; // Directly set this so as to avoid of being disposed accidentally by the internal mechanisms.
            base.Dispose();
        }

        public override ValueTask DisposeAsync() => new(Task.Run(Dispose));
    }
}
