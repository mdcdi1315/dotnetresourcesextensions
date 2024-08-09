
using System;
using System.Collections;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
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
        private IResourceTransferer transferer;
        private System.IO.FileStream[] streams;
        private System.Resources.IResourceWriter target;
        private System.Byte strindex;
        private static System.Boolean resolverconnected = false;
        private OutputResourceType restype;

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

        private static System.String GetMetadataNames(ICollection collection)
        {
            System.String result = System.String.Empty;
            foreach (System.Object val in collection) { result += val.ToString() + " "; }
            return result;
        }

        private void ProduceWarning(System.String code, System.String msg)
        {
            Log.LogWarning("", code, "", "", "<Non-Existent>", 0, 0, 0, 0, msg);
        }

        private bool UnsafeExecute()
        {
            if (OutputFilePath == null || OutputFilePath.ItemSpec == null) {
                ProduceError("DNTRESEXT0001" , "A target output file was not specified. Please specify a valid path , then retry. ");
                // Someone might not have yet included build code so as to generate resources , so return true to continue the build normally.
                return true;
            }
            if (InputFiles == null) {
                ProduceError("DNTRESEXT0012", "No input files were supplied. Please check whether all files are supplied correctly.");
                // Someone might not have yet included build code so as to generate resources , so return true to continue the build normally.
                return true;
            }
            if (restype == OutputResourceType.Resources)
            {
                target = new PreserializedResourceWriter(OutputFilePath.ItemSpec);
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
            Log.LogMessage(MessageImportance.High, "Reading files...");
#endif
            MinimalResourceLoader templdr = null;
            foreach (var file in InputFiles)
            {
                if (file is null) { continue; }
                System.Resources.IResourceReader dd = GetReaderFromPath(file.ItemSpec);
                if (dd is null && isfirst) {
                    ProduceError("DNTRESEXT0018" , "Primary file for reading must always be valid. Resource generation stopped.");
                    return false;
                } else if (isfirst == false) {
                    Log.LogMessage(MessageImportance.Normal, "The file {0} was skipped due to an unexpected error. See the log messages before for more information." , file.ItemSpec);
                    continue;
                }
                Log.LogMessage(MessageImportance.Normal , "Loaded file {0} into memory. Metadata Names: {1} Metadata Count: {2}" , file.ItemSpec , GetMetadataNames(file.MetadataNames) , file.MetadataCount);
                if (isfirst)
                {
                    Log.LogMessage(MessageImportance.Low, "As the file {0} is first in the list , the transferer instance will attach to this input resource file." , file.ItemSpec);
                    transferer = new AbstractResourceTransferer(dd , target);
                    transferer.TransferAll();
                    isfirst = false;
                } else {
                    Log.LogMessage(MessageImportance.Low , "The file {0} is a secondary file imported after the first file , and thus it's resources will be manually transferred." , file.ItemSpec);
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
                Log.LogMessage(MessageImportance.Low , "Done transferring resources from {0} ." , file.ItemSpec);
                // After transferring resources to it , it is easy to generate str's if the user demands it
                try {
                    templdr = new(dd);
                    GenerateStrTypedClassForItem(templdr, file);
                } catch (System.Exception e) {
                    ProduceWarning("DNTRESEXT0014" , $"The strongly-typed resource class generation for item {file.ItemSpec} has failed due to an unhandled {e.GetType().Name}.\n" +
                        $"As a result , the final compilation might fail if your code depends on this class generation. \n{e}");
                } finally { templdr?.Dispose();  }
            }
#if DEBUG
            Log.LogMessage(MessageImportance.High, "Done writing the resources.");
#endif
            target?.Close();
            target?.Dispose();
            return true;
        }

        // Generates a strongly-typed class from the specified task item.
        // The task item must conform to the format provided inside GeneratableResource.
        // If not , it throws a FormatException and explains which situation it found.
        private System.Boolean GenerateStrTypedClassForItem(IResourceLoader ldr , ITaskItem item)
        {
            static ResourceClassVisibilty ParseVisibility(System.String frommeta) => frommeta.ToLower() switch {
                "internal" => ResourceClassVisibilty.Internal,
                "public" => ResourceClassVisibilty.Public,
                _ => ResourceClassVisibilty.Internal,
            };
            System.String[] expecttofind = { "GenerateStrClass", "StrClassLanguage" , "StrClassName" , "StrClassManifestName" , "StrOutPath" , "StrClassVisibility" };
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
            // Determine whether the user wants to generate a str for this input. If not , exit.
            // Any string input that matches the letters false should prevent from creating a str.
            // On purpose we return here true since we have successfully executed anyway.
            if (item.GetMetadata(expecttofind[0]).ToLower() == "false") { return true; }
            // OK. Now we need to validate all input data.
            if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[3]))) {
                ProduceError("DNTRESEXT0004", $"Cannot generate code because the {expecttofind[3]} property in {item.ItemSpec} was not specified.");
                return false;
            }
            if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[4]))) {
                ProduceError("DNTRESEXT0005", $"Cannot generate code because a valid output path for {item.ItemSpec} was not specified. Please specify one and retry.");
                return false;
            }
            if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[1]))) {
                ProduceWarning("DNTRESEXT0006", "The strongly typed-class language was not specified. Presuming that it is \'CSharp\'.");
                item.SetMetadata(expecttofind[1], "CSharp");
            }
            if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[2]))) {
                ProduceWarning("DNTRESEXT0009", $"The strongly typed-class .NET name was not specified. Presuming that it is the manifest name: \"{item.GetMetadata(expecttofind[3])}\"");
                item.SetMetadata(expecttofind[2], item.GetMetadata(expecttofind[3]));
            }
            if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[5]))) {
                ProduceWarning("DNTRESEXT0011", "The resource class visibility was set to an invalid value. Presuming that it's value is \'Internal\'.");
                item.SetMetadata(expecttofind[5], "Internal");
            }
            // After verifying everything , next step is to generate our resource class...
            // Also ignore language letters cases.
            switch (item.GetMetadata(expecttofind[1]).ToLower())
            {
                case "csharp":
                case "c#":
                    StronglyTypedCodeProviderBuilder.WithCSharp(ldr ,
                        item.GetMetadata(expecttofind[3]) ,
                        item.GetMetadata(expecttofind[2]) ,
                        item.GetMetadata(expecttofind[4]) ,
                        ParseVisibility(item.GetMetadata(expecttofind[5])) ,
                        restype);
                    break;
                case "visualbasic":
                case "vb":
                    StronglyTypedCodeProviderBuilder.WithVisualBasic(ldr,
                        item.GetMetadata(expecttofind[3]),
                        item.GetMetadata(expecttofind[2]),
                        item.GetMetadata(expecttofind[4]),
                        ParseVisibility(item.GetMetadata(expecttofind[5])) , 
                        restype);
                    break;
            }
            return true;
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
                        rdr = new ResXResourceReader(streams[strindex]);
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
#if WF_AVAILABLE
                    case ".resx":
                        rdr = new System.Resources.ResXResourceReader(streams[strindex]);
                        break;
#endif
                    default:
                        streams[strindex]?.Dispose();
                        streams[strindex] = null;
                        ProduceWarning("DNTRESEXT0213" , $"Could not find a resource reader for the file {FI.FullName}. Resource generation for this item will be skipped. This can cause compilation or run-time errors.");
                        break;
                }
            } catch (System.Exception ex) {
                Log.LogWarning("Could not load file {0} ... See next warning for more information." , path);
                Log.LogWarningFromException(ex , true); 
                return null;
            }
            return rdr;
        }

        /// <summary>
        /// Gets the input resource files to add. <br />
        /// Additionally these task items are expected to have the same layout as exported from GeneratableResource.
        /// </summary>
        [Required]
        public ITaskItem[] InputFiles { get; set; }

        /// <summary>
        /// Defines the save output path to the file.
        /// </summary>
        [Required]
        public ITaskItem OutputFilePath { get; set; }

        /// <summary>
        /// The output file type. Must be one of the constants defined in <see cref="OutputResourceType"/> enumeration.
        /// </summary>
        [Required]
        public System.String OutputFileType { 
            get => restype.ToString();
            set {
                try {
                    restype = (OutputResourceType)System.Enum.Parse(typeof(OutputResourceType), value);
                } catch (ArgumentException e) {
                    ProduceWarning("DNTRESEXT0007", $"The value specified , {value} was not accepted because of an {e.GetType().Name}: \n {e} .");
                    ProduceWarning("DNTRESEXT0008", "Setting the OutputFileType back to Resources due to an error. See the previous message for more information.");
                    restype = OutputResourceType.Resources;
                }
            }
        }
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
            if (System.String.IsNullOrWhiteSpace(PackageRoot)) {
                if (System.IO.Directory.Exists(DownloadDeps.GeneratePlatformAgnosticPath(DownloadDeps.BuildTasksPath.FullName , "temp"))) {
                    PackageRoot = DownloadDeps.GeneratePlatformAgnosticPath(DownloadDeps.BuildTasksPath.FullName, "temp");
                } else {
                    PackageRoot = DownloadDeps.BuildTasksPath.CreateSubdirectory("temp").FullName; 
                }
            }
            Log.LogMessage(MessageImportance.Normal, "Determined package root is {0}." , PackageRoot);
            try {
                Log.LogMessage(MessageImportance.Normal, "Ensuring 9 packages.");
                LogPackageInstallStart(DownloadDeps.SystemValueTuple);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemValueTuple , PackageRoot , RunningFramework);
                LogPackageInstallEnd(DownloadDeps.SystemValueTuple);
                LogPackageInstallStart(DownloadDeps.SystemBuffers);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemBuffers , PackageRoot , RunningFramework);
                LogPackageInstallEnd(DownloadDeps.SystemBuffers);
                LogPackageInstallStart(DownloadDeps.MicrosoftBclAsyncInterfaces);
                DownloadDeps.EnsurePackage(DownloadDeps.MicrosoftBclAsyncInterfaces , PackageRoot , RunningFramework);
                LogPackageInstallEnd(DownloadDeps.MicrosoftBclAsyncInterfaces);
                LogPackageInstallStart(DownloadDeps.SystemTextJson);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemTextJson , PackageRoot , RunningFramework);
                LogPackageInstallEnd (DownloadDeps.SystemTextJson);
                LogPackageInstallStart(DownloadDeps.SystemTextEncodingsWeb);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemTextEncodingsWeb , PackageRoot , RunningFramework);
                LogPackageInstallEnd(DownloadDeps.SystemTextEncodingsWeb);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemMemory , PackageRoot , RunningFramework);
                LogPackageInstallStart(DownloadDeps.SystemRuntimeCompilerServicesUnsafe);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemRuntimeCompilerServicesUnsafe, PackageRoot, RunningFramework);
                LogPackageInstallEnd(DownloadDeps.SystemRuntimeCompilerServicesUnsafe);
                LogPackageInstallStart(DownloadDeps.SystemThreadingTasksExtensions);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemThreadingTasksExtensions, PackageRoot, RunningFramework);
                LogPackageInstallEnd(DownloadDeps.SystemThreadingTasksExtensions);
                LogPackageInstallStart(DownloadDeps.SystemNumericsVectors);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemNumericsVectors, PackageRoot, RunningFramework);
                LogPackageInstallEnd(DownloadDeps.SystemNumericsVectors);
                if (System.String.IsNullOrWhiteSpace(EngineRuntimeType)) { EngineRuntimeType = System.String.Empty; }
                if (EngineRuntimeType.ToLower() == "core" && DownloadDeps.Is_Windows) {
                    Log.LogMessage(MessageImportance.Normal, "Installing 1 more package due to true conditions (MSBuildRuntimeType is core and we are on Windows).");
                    LogPackageInstallStart(DownloadDeps.SystemDrawingCommon);
                    DownloadDeps.EnsurePackage(DownloadDeps.SystemDrawingCommon, PackageRoot, RunningFramework);
                    LogPackageInstallEnd(DownloadDeps.SystemDrawingCommon);
                }
                return true;
            } catch (Exception e)  {
                Log.LogErrorFromException(e , true , true , null);
                return false;
            }
        }

        private void LogPackageInstallStart(DownloadDeps.Package pkg)
        {
            Log.LogMessage(MessageImportance.Normal, "Ensuring package {0} with preferred version {1}." , pkg.LibraryName , pkg.LibraryVersion);
        }

        private void LogPackageInstallEnd(DownloadDeps.Package pkg)
        {
            Log.LogMessage(MessageImportance.Normal, "Package {0} sucessfully installed." , pkg.LibraryName);
        }

        public System.String PackageRoot { get; set; }

        [Required]
        public System.String RunningFramework { get; set; }

        public System.String EngineRuntimeType { get; set; }
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
        public static readonly Package SystemNumericsVectors = new() { LibraryName = "System.Numerics.Vectors", LibraryVersion = "4.5.0" };
        public static readonly Package SystemRuntimeCompilerServicesUnsafe = new() { LibraryName = "System.Runtime.CompilerServices.Unsafe" , LibraryVersion = "6.0.0" };
        public static readonly Package MicrosoftBclAsyncInterfaces = new() { LibraryName = "Microsoft.Bcl.AsyncInterfaces" , LibraryVersion = "8.0.0" };
        public static readonly Package SystemBuffers = new() { LibraryName = "System.Buffers" , LibraryVersion = "4.5.1" };
        public static readonly Package SystemMemory = new() { LibraryName = "System.Memory", LibraryVersion = "4.5.5" };
        public static readonly Package SystemTextEncodingsWeb = new() { LibraryName = "System.Text.Encodings.Web", LibraryVersion = "8.0.0" };
        public static readonly Package SystemThreadingTasksExtensions = new() { LibraryName = "System.Threading.Tasks.Extensions", LibraryVersion = "4.5.4" };
        public static readonly Package SystemValueTuple = new() { LibraryName = "System.ValueTuple", LibraryVersion = "4.5.0" };
        public static readonly Package SystemDrawingCommon = new() { LibraryName = "System.Drawing.Common", LibraryVersion = "8.0.7" };
        public static readonly System.IO.DirectoryInfo BuildTasksPath = new System.IO.FileInfo(typeof(DownloadDeps).Assembly.Location).Directory;
        public static readonly System.Boolean Is_Windows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

        public static System.Uri GeneratePackageUrl(Package pkg) => new($"https://nuget.org/api/v2/package/{pkg.LibraryName}/{pkg.LibraryVersion}");

        public static System.String GeneratePlatformAgnosticPath(params System.String[] paths)
        {
            System.String result, seperator , temp;
            // I currently know that Windows have the backslash as path seperator.
            // If more platforms have other path seperator or use backslash plz notify me.
            if (Is_Windows) { seperator = "\\"; } else { seperator = "/"; }
            result = System.String.Empty;
            if (paths.Length == 0) { return result; }
            for (System.Int32 I = 0; I < paths.Length - 1; I++) {
                temp = paths[I];
                if (temp.EndsWith("\\") || temp.EndsWith("/")) { temp = temp.Remove(temp.Length - 1); } // Strip last slash so no double slashes occur at the resulting string
                result += temp + seperator;
            }
            temp = paths[paths.Length - 1];
            if (temp.EndsWith("\\") || temp.EndsWith("/")) { temp = temp.Remove(temp.Length - 1); }
            result += temp;
            return result;
        }

        public static System.IO.DirectoryInfo GeneratePackagePath(Package pkg, System.String root)
         => new(GeneratePlatformAgnosticPath(root , pkg.LibraryName.ToLowerInvariant() , pkg.LibraryVersion));

        public static System.String GeneratePackageLibraryPath(System.IO.DirectoryInfo basedir, Package pkg, System.String tfname)
            => GeneratePlatformAgnosticPath(basedir.FullName , "lib" , tfname , $"{pkg.LibraryName}.dll");

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
                System.IO.DirectoryInfo DI = GeneratePackagePath(pkg, path);
                // If the subsequent folders do not exist , they will be created
                if (DI.Exists == false) { DI.Create(); }
                // After these we are ready to extract the file
                archive.ExtractToDirectory(DI.FullName);
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
            System.IO.DirectoryInfo DI = GeneratePackagePath(package, nugetpackageroot);
            if (DI.Exists == false) {
                DownloadFileAndPlaceToPackageRoot(package, nugetpackageroot);
            }
            // Next step is to find the DLL and copy it to the BuildTasks root.
            // Refresh the directory object state if the package was just downloaded and extracted
            DI.Refresh();
            // Ok. The DLL must now be retrieved and copied to the BuildTasks.
            // For first we retrieve the directory of the location of this assembly which is the BuildTasks itself.
            System.IO.DirectoryInfo DBuild = BuildTasksPath;
            // Return if the file exists already
            System.String ResultPath = GeneratePlatformAgnosticPath(DBuild.FullName, $"{package.LibraryName}.dll");
            if (System.IO.File.Exists(ResultPath)) { return; }
            // Next we must determine the running framework provided by targetresolvedid.
            switch (targetresolvedid)
            {
                case "netstandard2.0":
                    // For .NET standard 2.0 , most assemblies exist for this type of framework so directly just copying the file.
                    try {
                        System.IO.File.Copy(GeneratePackageLibraryPath(DI , package , "netstandard2.0"), ResultPath , true);
                    } catch (System.IO.FileNotFoundException) { goto case "net472"; }
                    catch (System.IO.DirectoryNotFoundException) { goto case "net472"; } // Oh. we do not have netstandard2.0 so copy the .NET framework one if exists.
                    return;
                case "net472":
                    // The .NET framework assemblies might not have a net472 variant so all cases must be examined
                    foreach (System.String tfname in new System.String[] { "net45" , "net451" , "net452" , "net46" , "net461" , "net462" , "net463" , "net47" , "net471" , "net472" }) {
                        if (System.IO.File.Exists(GeneratePackageLibraryPath(DI , package , tfname))) {
                            // The first hit will be copied.
                            System.IO.File.Copy(GeneratePackageLibraryPath(DI , package , tfname), ResultPath, true);
                            return;
                        }
                    }
                    throw new System.InvalidOperationException("The packages downloaded must support .NET Framework and .NET Standard.");
                default:
                    throw new ArgumentException("The Target Resolver Framework must be either netstandard2.0 or net472.");
            }
        }
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
