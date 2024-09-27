using System;
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Internal.ResX
{
    /// <summary>
    /// Defines a tree with the versions of ResX for compatibility. <br />
    /// From this tree then , the ResX reader automagically selects the required version to load.
    /// </summary>
    public static class ResXCompatibilityData
    {
        // Holds a list of compatibility versions that can be used.
        private static List<ResXCompatibilityVersion> vers;

        static ResXCompatibilityData() {
            vers = new() {
                new("2.0" , "text/microsoft-resx" ,
                "System.Resources.ResXResourceReader, System.Windows.Forms, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" ,
                ".NET 8 ResX version"),
                new("2.0" , "text/microsoft-resx" ,
                "System.Resources.ResXResourceReader, System.Windows.Forms, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" ,
                ".NET 7 ResX version"),
                new("2.0" , "text/microsoft-resx" ,
                "System.Resources.ResXResourceReader, System.Windows.Forms, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" ,
                ".NET 6 ResX version"),
                new("2.0" , "text/microsoft-resx" ,
                "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" ,
                ".NET Framework ResX version (incompatible in some cases)"),
                new("1.1" , "text/microsoft-resx" ,
                "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" ,
                ".NET Framework ResX version (incompatible in some cases) (old format)"),
                new("1.0" , "text/microsoft-resx" ,
                "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" ,
                ".NET Framework ResX version (incompatible in some cases) (old format)"),
                new("1.1-initial" , "text/mdcdi1315-resx" ,
                typeof(ResXResourceReader).AssemblyQualifiedName ,
                typeof(ResXResourceWriter).AssemblyQualifiedName , 
                "DotNetResourcesExtensions ResX version"),
            };
        }

        private static System.Boolean IsDigit(System.Char c) => (System.UInt32)(c - '0') <= ('9' - '0');

        private static System.Version GetVersionOrFail(System.String d)
        {
            System.Boolean invd = false;
            foreach (System.Char c in d) { 
                if (IsDigit(c) || c == '.') { continue; }
                invd = true;
                break;
            }
            if (invd) { return null; }
            return new(d);
        }

        /// <summary>
        /// Gets all the ResX versions that the <see cref="ResXResourceReader"/> can handle.
        /// </summary>
        public static IEnumerable<ResXCompatibilityVersion> Versions => vers;

        /// <summary>
        /// Finds the best suitable version to load for the given ResX data provided as a <see cref="ResXVersionData"/> class.
        /// </summary>
        /// <param name="read">The ResX data to find a version for.</param>
        /// <returns>The optimal ResX version that the reader should use.</returns>
        public static ResXCompatibilityVersion FindBestSuitableVersion(ResXVersionData read)
        {
            // First we have to match against the version string.
            System.Version v2 = GetVersionOrFail(read.Version);
            System.Boolean precond = v2 is null;
            foreach (ResXCompatibilityVersion version in vers)
            {
                System.Version v1 = GetVersionOrFail(version.Version);
                System.Boolean cond;
                if (precond || v1 is null) {
                    cond = version.Version.Equals(read.Version , StringComparison.OrdinalIgnoreCase);
                } else {
                    cond = v2 >= v1; 
                }
                if (cond)
                {
                    // We have a match against the version string.
                    // Find whether we satisfy the reader and the writer , and the mime type.
                    if (version.ReaderFullString.Equals(read.ReaderFullString) && 
                        version.WriterFullString.Equals(read.WriterFullString) && 
                        version.ResourceMimeType.Equals(read.ResourceMimeType))
                    {
                        // Return then the compat version found , so.
                        return version;
                    }
                }
            }
            throw new ArgumentException($"No compatible version was found for \'{read.ReaderFullString}\' .");
        }

    }

    /// <summary>
    /// Defines a ResX version that is compatible.
    /// </summary>
    public sealed class ResXCompatibilityVersion
    {
        private readonly System.String ver, wrfs, rdfs, resmimetype , id;

        internal ResXCompatibilityVersion(System.String version , System.String resmime , 
            System.String rdtype , System.String wrtype , System.String identifier)
        {
            ver = version;
            wrfs = wrtype;
            rdfs = rdtype;
            resmimetype = resmime;
            id = identifier;
        }

        /// <summary>
        /// Gets the defined <see cref="System.Type.AssemblyQualifiedName"/> for the ResX resource writer for this ResX version.
        /// </summary>
        public System.String ReaderFullString { get => rdfs; }

        /// <summary>
        /// Gets the defined <see cref="System.Type.AssemblyQualifiedName"/> for the ResX resource reader for this ResX version.
        /// </summary>
        public System.String WriterFullString { get => wrfs; }

        /// <summary>
        /// Gets the version string that is supported by the current version.
        /// </summary>
        public System.String Version { get => ver; }

        /// <summary>
        /// Gets the ResX mimetype for this reader/writer set.
        /// </summary>
        public System.String ResourceMimeType { get => resmimetype; }

        /// <summary>
        /// Gets the identifier of this ResX version.
        /// </summary>
        public System.String UniqueIdentifier { get => id; }
    }

    /// <summary>
    /// Defines the ResX version read by the reader.
    /// </summary>
    public sealed class ResXVersionData
    {
        internal ResXVersionData() { }

        /// <summary>
        /// Gets the defined <see cref="System.Type.AssemblyQualifiedName"/> for the ResX resource writer for this ResX version.
        /// </summary>
        public System.String ReaderFullString { get; internal set; }

        /// <summary>
        /// Gets the defined <see cref="System.Type.AssemblyQualifiedName"/> for the ResX resource reader for this ResX version.
        /// </summary>
        public System.String WriterFullString { get; internal set; }

        /// <summary>
        /// Gets the version string that is supported by the current version.
        /// </summary>
        public System.String Version { get; internal set; }

        /// <summary>
        /// Gets the ResX mimetype for this reader/writer set.
        /// </summary>
        public System.String ResourceMimeType { get; internal set; }
    }
}
