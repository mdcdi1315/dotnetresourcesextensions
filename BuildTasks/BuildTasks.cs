
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
                this.LogExceptionClass(new UnexpectedErrorException(ex));
                return false;
            }
            transferer?.Dispose();
            target?.Dispose();
            for (System.Int32 I = 0; I < streams.Length; I++) { streams[I]?.Dispose(); }
            return val;
        }

        private static System.String GetMetadataNames(ICollection collection)
        {
            System.String result = System.String.Empty;
            foreach (System.Object val in collection) { result += val.ToString() + " "; }
            return result;
        }

        private bool UnsafeExecute()
        {
            if (OutputFilePath == null || OutputFilePath.ItemSpec == null) {
                this.ThrowMessage(1);
                // Someone might not have yet included build code so as to generate resources , so return true to continue the build normally.
                return true;
            }
            if (InputFiles == null) {
                this.ThrowMessage(2);
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
                this.ThrowMessage(3 , OutputFileType);
                return false;
            }
            if (target == null) { this.ThrowMessage(4); return false; }
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
                if (dd is null) {
                    if (isfirst) {
                        this.ThrowMessage(5);
                        return false;
                    } else {
                        Log.LogMessage(MessageImportance.Normal, "The file {0} was skipped due to an unexpected error. See the log messages before for more information.", file.ItemSpec);
                        continue;
                    }
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
                    this.ThrowMessage(6 , file.ItemSpec , e.GetType().Name , e);
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
                this.ThrowMessage(40 , expecttofind[3] , item.ItemSpec);
                return false;
            }
            if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[4]))) {
                this.ThrowMessage(41 , item.ItemSpec);
                return false;
            }
            if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[1]))) {
                this.ThrowMessage(42);
                item.SetMetadata(expecttofind[1], "CSharp");
            }
            if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[2]))) {
                this.ThrowMessage(43 , item.GetMetadata(expecttofind[3]));
                item.SetMetadata(expecttofind[2], item.GetMetadata(expecttofind[3]));
            }
            if (String.IsNullOrWhiteSpace(item.GetMetadata(expecttofind[5]))) {
                this.ThrowMessage(44);
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
                    case ".resh":
                        this.ThrowMessage(118);
                        rdr = new HumanReadableFormatReader(streams[strindex]);
                        break;
#if WF_AVAILABLE
                    case ".resx":
                        rdr = new System.Resources.ResXResourceReader(streams[strindex]);
                        break;
#endif
                    default:
                        streams[strindex]?.Dispose();
                        streams[strindex] = null;
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
                    this.ThrowMessage(7, value, e.GetType().Name, e);
                    this.ThrowMessage(8);
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
            try { return UnsafeExecute(); } catch (System.Exception e) { this.LogExceptionClass(e); return false; }
        }

        private System.Boolean UnsafeExecute()
        {
            if (System.String.IsNullOrWhiteSpace(PackageRoot)) {
                if (System.IO.Directory.Exists(DownloadDeps.GeneratePlatformAgnosticPath(DownloadDeps.BuildTasksPath.FullName, "temp"))) {
                    PackageRoot = DownloadDeps.GeneratePlatformAgnosticPath(DownloadDeps.BuildTasksPath.FullName, "temp");
                } else {
                    PackageRoot = DownloadDeps.BuildTasksPath.CreateSubdirectory("temp").FullName;
                }
            }
            Log.LogMessage(MessageImportance.Normal, "Determined package root is {0}.", PackageRoot);
            System.DateTime current = System.DateTime.Now;
            try {
                Log.LogMessage(MessageImportance.Normal, "Ensuring 9 packages.");
                LogPackageInstallStart(DownloadDeps.SystemValueTuple);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemValueTuple, PackageRoot, RunningFramework);
                LogPackageInstallEnd(DownloadDeps.SystemValueTuple);
                LogPackageInstallStart(DownloadDeps.SystemBuffers);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemBuffers, PackageRoot, RunningFramework);
                LogPackageInstallEnd(DownloadDeps.SystemBuffers);
                LogPackageInstallStart(DownloadDeps.MicrosoftBclAsyncInterfaces);
                DownloadDeps.EnsurePackage(DownloadDeps.MicrosoftBclAsyncInterfaces, PackageRoot, RunningFramework);
                LogPackageInstallEnd(DownloadDeps.MicrosoftBclAsyncInterfaces);
                LogPackageInstallStart(DownloadDeps.SystemTextJson);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemTextJson, PackageRoot, RunningFramework);
                LogPackageInstallEnd(DownloadDeps.SystemTextJson);
                LogPackageInstallStart(DownloadDeps.SystemTextEncodingsWeb);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemTextEncodingsWeb, PackageRoot, RunningFramework);
                LogPackageInstallEnd(DownloadDeps.SystemTextEncodingsWeb);
                DownloadDeps.EnsurePackage(DownloadDeps.SystemMemory, PackageRoot, RunningFramework);
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
                Log.LogMessage(MessageImportance.Low, "The operation took {0} seconds to complete.", System.DateTime.Now.Subtract(current).TotalSeconds);
                return true;
            } catch (ArgumentException e) {
                Log.LogMessage(MessageImportance.Low, "The operation took {0} seconds to fail.", System.DateTime.Now.Subtract(current).TotalSeconds);
                throw new InvalidInputFromUserException(e);
            } catch (System.Net.Http.HttpRequestException e) {
                Log.LogMessage(MessageImportance.Low, "The operation took {0} seconds to fail.", System.DateTime.Now.Subtract(current).TotalSeconds);
                throw new System.AggregateException("Failed to download one or more packages.", e);
            } catch (Exception e) {
                Log.LogMessage(MessageImportance.Low, "The operation took {0} seconds to fail.", System.DateTime.Now.Subtract(current).TotalSeconds);
                throw new UnexpectedErrorException(e);
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
        public sealed class Package
        {
            private readonly string ln, lnver;

            public Package(System.String libname , System.String libver)
            {
                ln = libname;
                lnver = libver;
            }

            public System.String LibraryName => ln;
            public System.String LibraryVersion => lnver;
        }

        public static readonly Package SystemTextJson = new("System.Text.Json", "8.0.4");
        public static readonly Package SystemNumericsVectors = new("System.Numerics.Vectors", "4.5.0");
        public static readonly Package SystemRuntimeCompilerServicesUnsafe = new("System.Runtime.CompilerServices.Unsafe", "6.0.0");
        public static readonly Package MicrosoftBclAsyncInterfaces = new("Microsoft.Bcl.AsyncInterfaces", "8.0.0");
        public static readonly Package SystemBuffers = new("System.Buffers", "4.5.1");
        public static readonly Package SystemMemory = new("System.Memory", "4.5.5");
        public static readonly Package SystemTextEncodingsWeb = new("System.Text.Encodings.Web", "8.0.0");
        public static readonly Package SystemThreadingTasksExtensions = new("System.Threading.Tasks.Extensions", "4.5.4");
        public static readonly Package SystemValueTuple = new("System.ValueTuple", "4.5.0");
        public static readonly Package SystemDrawingCommon = new("System.Drawing.Common", "8.0.7");
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

    internal static class ErrorHandler
    {
        public enum MessageType : System.Byte
        {
            Message,
            Warning,
            Error,
            Critical
        }

        /// <summary>
        /// A stored type that saves all required data to create and show a message in MSBuild. <br />
        /// Note: The messages stored here are format strings that are expanded at run-time. Be careful!.
        /// </summary>
        public sealed class MessagePiece
        {
            private readonly System.String message;
            private readonly System.UInt32 code;
            private readonly MessageType type;

            public MessagePiece(System.UInt32 code, System.String message, MessageType msgtype)
            {
                this.message = message;
                this.code = code;
                this.type = msgtype;
            }

            public System.String Code => $"{BuildTasksErrorCodePrefix}{code:d4}";

            public System.UInt32 NumericCode => code;

            public MessageType Type => type;

            public System.String Message => message;
        }

        private const System.Int32 SpecialErrorCode = 7777;
        private const System.String BuildTasksErrorCodePrefix = "DNTRESEXT";
        private static readonly System.String SpecialErrorCodeString;
        private static readonly MessagePiece[] messages;

        static ErrorHandler() {
            SpecialErrorCodeString = $"{BuildTasksErrorCodePrefix}{SpecialErrorCode}";
            // Here in this array all messages are temporarily saved.
            messages = new MessagePiece[] {
                new(0 , "An unexpected exception has occured during code execution: \n{0}" , MessageType.Critical),
                new(1 , "A target output file was not specified. Please specify a valid path , then retry. " , MessageType.Error),
                new(2, "No input files were supplied. Please check whether all files are supplied correctly." , MessageType.Error),
                new(3 , "Unknown output file format specified: {0}" , MessageType.Error),
                new(4 , "Target should NOT BE NULL AT THIS POINT FAILURE OCCURED" , MessageType.Critical),
                new(5 , "Primary file for reading must always be valid. Resource generation stopped." , MessageType.Error),
                new(6 , "The strongly-typed resource class generation for item {0} has failed due to an unhandled {1}.\nAs a result , the final compilation might fail if your code depends on this class generation. \n{2}" , MessageType.Warning),
                new(7 , "The value specified , {0} was not accepted because of an {1}: \n {2} ." , MessageType.Warning),
                new(8 , "Setting the OutputFileType back to Resources due to an error. See the previous message for more information." , MessageType.Warning),
                new(40 , "Cannot generate code because the {0} property in {1} was not specified." , MessageType.Error),
                new(41 , "Cannot generate code because a valid output path for {0} was not specified. Please specify one and retry." , MessageType.Error),
                new(42 , "The strongly typed-class language was not specified. Presuming that it is 'CSharp'." , MessageType.Warning),
                new(43 , "The strongly typed-class .NET name was not specified. Presuming that it is the manifest name: \"{0}\"" , MessageType.Warning),
                new(44 , "The resource class visibility was set to an invalid value. Presuming that it's value is 'Internal'." , MessageType.Warning),
                new(118 , "The specified reader is still on a test phase. Do not trust this reader for saving critical resources." , MessageType.Warning),
                new(213 , "Could not find a resource reader for the file {0}. Resource generation for this item will be skipped. This may cause compilation or run-time errors." , MessageType.Warning),
                new(263 , "An internal exception has been detected in one of the BuildTasks task code. See the next error for more information." , MessageType.Critical)
            };
        }

        /// <summary>
        /// Throws the specified internal error message back to MSBuild , based it's code and severity of issue.
        /// </summary>
        /// <param name="task">The task object from which this message will be thrown to.</param>
        /// <param name="code">The unique code of the message to throw.</param>
        /// <param name="objects">Any formatting arguments that must be passed before the message is thrown.</param>
        public static void ThrowMessage(this Microsoft.Build.Utilities.Task task , System.UInt32 code , params System.Object[] objects)
        {
            foreach (MessagePiece piece in messages) 
            {
                if (piece.NumericCode == code) {
                    switch (piece.Type) {
                        case MessageType.Message:
                            task.Log.LogMessage(MessageImportance.High, piece.Message , objects);
                            break;
                        case MessageType.Warning:
                            task.Log.LogWarning("", piece.Code, "", "", "<Non-Existent>", 0, 0, 0, 0, piece.Message , objects);
                            break;
                        case MessageType.Error:
                            task.Log.LogError("", piece.Code, "", "", "<Non-Existent>", 0, 0, 0, 0, piece.Message , objects);
                            break;
                        case MessageType.Critical:
                            task.Log.LogCriticalMessage("", piece.Code, "", "<Non-Existent>", 0, 0, 0, 0, piece.Message , objects);
                            break;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Throws the internal error 263 followed by the given exception that is a critical exception and signifies that the build must stop.
        /// </summary>
        /// <param name="task">The task object from which this message will be thrown to</param>
        /// <param name="exception">The critical exception to throw.</param>
        public static void LogExceptionClass(this Microsoft.Build.Utilities.Task task , System.Exception exception) 
        {
            ThrowMessage(task, 263);
            switch (exception) {
                case InvalidInputFromUserException:
                    task.Log.LogError("", SpecialErrorCodeString, "", "", "<Non-Existent>", 0, 0, 0, 0, "Invalid argument inside a task call made the task {0} to fail. \nException: {1}" , task.GetType().Name , exception);
                    break;
                case UnexpectedErrorException:
                    task.Log.LogCriticalMessage("", SpecialErrorCodeString, "", "<Non-Existent>", 0, 0, 0, 0, "An unexpected hard error made the task {0} to fail (and consequently fail the build engine). \nException: {1}", task.GetType().Name, exception);
                    break;
                case AggregateException:
                    task.Log.LogError("", SpecialErrorCodeString, "", "", "<Non-Existent>", 0, 0, 0, 0, "One or more hard exceptions made the task {0} to fail. \nException: {1}", task.GetType().Name, exception);
                    break;
                default:
                    task.Log.LogCriticalMessage("", SpecialErrorCodeString, "", "<Non-Existent>", 0, 0, 0, 0, "[INTERNAL ERROR]: Cannot recognize exception type {0} ." , exception.GetType().FullName);
                    break;
            }
        }

    }
}
