
using System;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Build.Framework;

namespace DotNetResourcesExtensions.BuildTasks
{
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
            if (System.String.IsNullOrWhiteSpace(PackageRoot))
            {
                if (System.IO.Directory.Exists(DownloadDeps.GeneratePlatformAgnosticPath(DownloadDeps.BuildTasksPath.FullName, "temp")))
                {
                    PackageRoot = DownloadDeps.GeneratePlatformAgnosticPath(DownloadDeps.BuildTasksPath.FullName, "temp");
                }
                else
                {
                    PackageRoot = DownloadDeps.BuildTasksPath.CreateSubdirectory("temp").FullName;
                }
            }
            Log.LogMessage(MessageImportance.Normal, "Determined package root is {0}.", PackageRoot);
            System.DateTime current = System.DateTime.Now;
            try
            {
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
                if (EngineRuntimeType.ToLower() == "core" && DownloadDeps.Is_Windows)
                {
                    Log.LogMessage(MessageImportance.Normal, "Installing 1 more package due to true conditions (MSBuildRuntimeType is core and we are on Windows).");
                    LogPackageInstallStart(DownloadDeps.SystemDrawingCommon);
                    DownloadDeps.EnsurePackage(DownloadDeps.SystemDrawingCommon, PackageRoot, RunningFramework);
                    LogPackageInstallEnd(DownloadDeps.SystemDrawingCommon);
                }
                Log.LogMessage(MessageImportance.Low, "The operation took {0} seconds to complete.", System.DateTime.Now.Subtract(current).TotalSeconds);
                return true;
            }
            catch (ArgumentException e)
            {
                Log.LogMessage(MessageImportance.Low, "The operation took {0} seconds to fail.", System.DateTime.Now.Subtract(current).TotalSeconds);
                throw new InvalidInputFromUserException(e);
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                Log.LogMessage(MessageImportance.Low, "The operation took {0} seconds to fail.", System.DateTime.Now.Subtract(current).TotalSeconds);
                throw new System.AggregateException("Failed to download one or more packages.", e);
            }
            catch (Exception e)
            {
                Log.LogMessage(MessageImportance.Low, "The operation took {0} seconds to fail.", System.DateTime.Now.Subtract(current).TotalSeconds);
                throw new UnexpectedErrorException(e);
            }
        }

        private void LogPackageInstallStart(DownloadDeps.Package pkg)
        {
            Log.LogMessage(MessageImportance.Normal, "Ensuring package {0} with preferred version {1}.", pkg.LibraryName, pkg.LibraryVersion);
        }

        private void LogPackageInstallEnd(DownloadDeps.Package pkg)
        {
            Log.LogMessage(MessageImportance.Normal, "Package {0} sucessfully installed.", pkg.LibraryName);
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

            public Package(System.String libname, System.String libver)
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
            System.String result, seperator, temp;
            // I currently know that Windows have the backslash as path seperator.
            // If more platforms have other path seperator or use backslash plz notify me.
            if (Is_Windows) { seperator = "\\"; } else { seperator = "/"; }
            result = System.String.Empty;
            if (paths.Length == 0) { return result; }
            for (System.Int32 I = 0; I < paths.Length - 1; I++)
            {
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
         => new(GeneratePlatformAgnosticPath(root, pkg.LibraryName.ToLowerInvariant(), pkg.LibraryVersion));

        public static System.String GeneratePackageLibraryPath(System.IO.DirectoryInfo basedir, Package pkg, System.String tfname)
            => GeneratePlatformAgnosticPath(basedir.FullName, "lib", tfname, $"{pkg.LibraryName}.dll");

        public static void DownloadFileAndPlaceToPackageRoot(Package pkg, System.String path)
        {
            System.Net.Http.HttpClient HC = new();
            Task<System.IO.Stream> wrap = null;
            System.IO.Compression.ZipArchive archive = null;
            try
            {
                wrap = HC.GetStreamAsync(GeneratePackageUrl(pkg));
                wrap.Wait();
                archive = new(wrap.Result);
                // Now we must create the subsequent folders and extract the file there.
                System.IO.DirectoryInfo DI = GeneratePackagePath(pkg, path);
                // If the subsequent folders do not exist , they will be created
                if (DI.Exists == false) { DI.Create(); }
                // After these we are ready to extract the file
                archive.ExtractToDirectory(DI.FullName);
            }
            catch (System.AggregateException ex)
            {
                throw ex.InnerException ?? ex;
            }
            finally
            {
                HC?.Dispose();
                archive?.Dispose();
                if (wrap != null && wrap.IsCompleted)
                {
                    wrap.Result.Dispose();
                    wrap.Dispose();
                }
            }
        }

        public static void EnsurePackage(Package package, System.String nugetpackageroot, System.String targetresolvedid)
        {
            System.IO.DirectoryInfo DI = GeneratePackagePath(package, nugetpackageroot);
            if (DI.Exists == false)
            {
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
                    try
                    {
                        System.IO.File.Copy(GeneratePackageLibraryPath(DI, package, "netstandard2.0"), ResultPath, true);
                    }
                    catch (System.IO.FileNotFoundException) { goto case "net472"; }
                    catch (System.IO.DirectoryNotFoundException) { goto case "net472"; } // Oh. we do not have netstandard2.0 so copy the .NET framework one if exists.
                    return;
                case "net472":
                    // The .NET framework assemblies might not have a net472 variant so all cases must be examined
                    foreach (System.String tfname in new System.String[] { "net45", "net451", "net452", "net46", "net461", "net462", "net463", "net47", "net471", "net472" })
                    {
                        if (System.IO.File.Exists(GeneratePackageLibraryPath(DI, package, tfname)))
                        {
                            // The first hit will be copied.
                            System.IO.File.Copy(GeneratePackageLibraryPath(DI, package, tfname), ResultPath, true);
                            return;
                        }
                    }
                    throw new System.InvalidOperationException("The packages downloaded must support .NET Framework and .NET Standard.");
                default:
                    throw new ArgumentException("The Target Resolver Framework must be either netstandard2.0 or net472.");
            }
        }
    }
}
