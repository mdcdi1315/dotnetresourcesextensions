
extern alias DRESEXT;

using System;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using DRESEXT::DotNetResourcesExtensions;
using System.IO.Compression;

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
        private static System.Boolean resolverconnected = false;
        private OutputResourceType restype;

        public DotNetResExtGenerator() : base()
        {
            static System.Reflection.Assembly Resolver(System.Object sender , System.ResolveEventArgs e)
            {
                System.IO.DirectoryInfo DI = new(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                foreach (var file in DI.GetFiles("*.dll"))
                {
                    if ($"{e.Name}.dll" == file.Name)
                    {
                        return System.Reflection.Assembly.LoadFrom(file.FullName);
                    }
                }
                return null;
            }
            if (resolverconnected == false)
            {
                System.AppDomain.CurrentDomain.AssemblyResolve += Resolver;
            }
            strindex = 0;
            target = null;
            streams = new System.IO.FileStream[50];
            transferer = null;
            HelpKeywordPrefix = null;
            restype = OutputResourceType.Resources;
        }

        public override bool Execute()
        {
            System.Boolean val;
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
            if (InputFiles == null)
            {
                ProduceError("DNTRESEXT0012", "No input files were supplied. Please check whether all files are supplied correctly.");
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
            } else {
                ProduceError("DNTRESEXT0003", $"Unknown output file format specified: {OutputFileType}");
                return false;
            }
            if (target == null) { ProduceError("DNTRESEXT0010" , "Target should NOT BE NULL AT THIS POINT FAILURE OCCURED"); return false; }
#if DEBUG
            Log.LogMessage(MessageImportance.High, "Succeeded acquiring the target.");
#endif
            System.Boolean isfirst = true;
#if DEBUG
            Log.LogMessage(MessageImportance.High , "Yielding engine...");
#endif
            BuildEngine9.Yield();
#if DEBUG
            Log.LogMessage(MessageImportance.High, "Reading files...");
#endif
            foreach (var file in InputFiles)
            {
                if (file is null) { continue; }
                var dd = GetReaderFromPath(file.ItemSpec);
                if (dd is null)
                {
                    ProduceError("DNTRESEXT0011" , "Temporary reader MUST NOT BE NULL AT THIS POINT FAILURE OCCURED");
                    return false;
                }
#if DEBUG
                Log.LogMessage(MessageImportance.High, "Succeeded acquiring the reader.");
#endif
                if (isfirst)
                {
                    transferer = new AbstractResourceTransferer(dd , target);
                    transferer.TransferAll();
                    isfirst = false;
                } else {
#if DEBUG
                    Log.LogMessage(MessageImportance.High , "Secondary reader is now processed.");
#endif
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
#if DEBUG
            Log.LogMessage(MessageImportance.High, "Done writing the resources.");
#endif
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
            target?.Close();
            target?.Dispose();
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

    [RunInMTA]
    public class DependenciesResolver : Microsoft.Build.Utilities.Task
    {
        public DependenciesResolver() { }

        public override bool Execute()
        {
            try {
                DownloadDeps.EnsurePackage(DownloadDeps.SystemValueTuple , PackageRoot , RunningFramework);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemBuffers , PackageRoot , RunningFramework);
                DownloadDeps.EnsurePackage(DownloadDeps.MicrosoftBclAsyncInterfaces , PackageRoot , RunningFramework);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemTextJson , PackageRoot , RunningFramework);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemTextEncodingsWeb , PackageRoot , RunningFramework);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemMemory , PackageRoot , RunningFramework);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemRuntimeCompilerServicesUnsafe, PackageRoot, RunningFramework);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemThreadingTasksExtensions, PackageRoot, RunningFramework);
                return true;
            } catch (Exception e)  {
                Log.LogCriticalMessage("", "DOTNETRESOURCESEXTENSIONSDEPERROR", "", "<Non-Existent>", 0 , 0 , 0 ,0 , "Could not resolve dependencies due to an unhandled exception:\n {0}" , e);
                return false;
            }
        }

        [Required]
        public System.String PackageRoot { get; set; }

        [Required]
        public System.String RunningFramework { get; set; }
    }

    // Internal class that downloads and installs dependencies required by BuildTasks.
    internal static class DownloadDeps
    {
        public record class Package
        {
            public System.String LibraryName;
            public System.String LibraryVersion;
        }

        public static readonly Package SystemTextJson = new() { LibraryName = "System.Text.Json" , LibraryVersion = "8.0.4" };
        public static readonly Package SystemRuntimeCompilerServicesUnsafe = new() { LibraryName = "System.Runtime.CompilerServices.Unsafe" , LibraryVersion = "6.0.0" };
        public static readonly Package MicrosoftBclAsyncInterfaces = new() { LibraryName = "Microsoft.Bcl.AsyncInterfaces" , LibraryVersion = "8.0.0" };
        public static readonly Package SystemBuffers = new() { LibraryName = "System.Buffers" , LibraryVersion = "4.5.1" };
        public static readonly Package SystemMemory = new() { LibraryName = "System.Memory", LibraryVersion = "4.5.5" };
        public static readonly Package SystemTextEncodingsWeb = new() { LibraryName = "System.Text.Encodings.Web", LibraryVersion = "8.0.0" };
        public static readonly Package SystemThreadingTasksExtensions = new() { LibraryName = "System.Threading.Tasks.Extensions", LibraryVersion = "4.5.4" };
        public static readonly Package SystemValueTuple = new() { LibraryName = "System.ValueTuple", LibraryVersion = "4.5.0" };

        public static System.Uri GeneratePackageUrl(Package pkg) => new($"https://nuget.org/api/v2/package/{pkg.LibraryName}/{pkg.LibraryVersion}");

        public static void DownloadFileAndPlaceToPackageRoot(Package pkg , System.String path)
        {
            System.Net.Http.HttpClient HC = new();
            Task<System.IO.Stream> wrap = null;
            System.IO.Compression.ZipArchive archive = null;
            try {
                wrap = HC.GetStreamAsync(GeneratePackageUrl(pkg));
                wrap.Wait();
                archive = new(wrap.Result);
                // Now we must create the subsequent folders and extract the file there.
                System.IO.DirectoryInfo DI = new(path) , DG;
                if (System.IO.Directory.Exists($"{DI.FullName}\\{pkg.LibraryName}") == false)
                {
                    DG = DI.CreateSubdirectory(pkg.LibraryName);
                } else { DG = new($"{DI.FullName}\\{pkg.LibraryName}"); }
                if (System.IO.Directory.Exists($"{DG.FullName}\\{pkg.LibraryVersion}") == false) {
                    DG.CreateSubdirectory(pkg.LibraryVersion);
                }
                // After all these we are ready to extract the file
                archive.ExtractToDirectory($"{DG.FullName}\\{pkg.LibraryVersion}");
                DI = null; DG = null;
            } catch (System.AggregateException ex) {
                throw ex.InnerException ?? ex;
            } finally {
                HC?.Dispose();
                archive?.Dispose();
                if (wrap != null && wrap.IsCompleted) {
                    wrap.Result.Dispose();
                    wrap.Dispose();
                }
            }
        }

        public static void EnsurePackage(Package package , System.String nugetpackageroot , System.String targetresolvedid)
        {
            System.IO.DirectoryInfo DI = new($"{nugetpackageroot}\\{package.LibraryName}\\{package.LibraryVersion}");
            if (DI.Exists == false) {
                DownloadFileAndPlaceToPackageRoot(package, nugetpackageroot);
            }
            // Next step is to find the DLL and copy it to the BuildTasks root.
            // Refresh the directory object state if the package was just downloaded and extracted
            DI.Refresh();
            // Ok. The DLL must now be retrieved and copied to the BuildTasks.
            // For first we retrieve the directory of the location of this assembly which is the BuildTasks itself.
            System.IO.DirectoryInfo DBuild = new System.IO.FileInfo(typeof(DownloadDeps).Assembly.Location).Directory;
            // Return if the file exists already
            if (System.IO.File.Exists($"{DBuild.FullName}\\{package.LibraryName}.dll")) { return; }
            // Next we must determine the running framework provided by targetresolvedid.
            switch (targetresolvedid)
            {
                case "netstandard2.0":
                    // For .NET standard 2.0 , most assemblies exist for this type of framework so directly just copying the file.
                    try {
                        System.IO.File.Copy($"{DI.FullName}\\lib\\netstandard2.0\\{package.LibraryName}.dll", $"{DBuild.FullName}\\{package.LibraryName}.dll", true);
                    } catch (System.IO.DirectoryNotFoundException) { goto case "net472"; } // Oh. we do not have netstandard2.0 so copy the .NET framework one if exists.
                    break;
                case "net472":
                    // The .NET framework assemblies might not have a net472 variant so all cases must be examined
                    foreach (System.String tfname in new System.String[] { "net461" , "net462" , "net47" , "net471" , "net472" }) {
                        if (System.IO.Directory.Exists($"{DI.FullName}\\lib\\{tfname}")) {
                            // The first hit will be copied.
                            System.IO.File.Copy($"{DI.FullName}\\lib\\{tfname}\\{package.LibraryName}.dll", $"{DBuild.FullName}\\{package.LibraryName}.dll", true);
                            break;
                        }
                    }
                    throw new System.InvalidOperationException("The packages downloaded must support .NET Framework and .NET Standard.");
            }
        }
    }
}
