using System;
using System.Windows.Forms;

namespace ResourceEditor
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ResourceEditor.Main());
        }
    }

    internal static class Helper
    {
        private const System.Int32 BUFSIZE = 4096;
        public const System.String InvalidNameCharacters = "!@#$%^&*()`~,./?\\|\"\';:{}[]+=©§¥¢ž®¯";
        public const System.String InvalidFirstCharacterNameCharacters = "1234567890!@#$%^&*()`~,./?\\|\"';:{}[]+=-_©§¥¢ž®¯";


        public static void ShowErrorMessage(System.String msg)
        {
            MessageBox.Show(msg , "Error" , MessageBoxButtons.OK , MessageBoxIcon.Stop);
        }

        public static System.Boolean ShowQuestionMessage(System.String msg) 
        {
            return MessageBox.Show(msg, "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>
        /// Tests if the resource name given is valid.
        /// </summary>
        /// <param name="Name">The resource name to test.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        [System.Diagnostics.DebuggerHidden]
        public static void ValidateName(System.String Name)
        {
            if (Name == null || System.String.IsNullOrEmpty(Name))
            {
                throw new ArgumentNullException("Name");
            }

            if (Name.Length > 530)
            {
                throw new ArgumentException("A resource name must not have more than 530 characters.");
            }

            for (System.Int32 J = 0; J < InvalidFirstCharacterNameCharacters.Length; J++)
            {
                if (Name[0] == InvalidFirstCharacterNameCharacters[J])
                {
                    throw new ArgumentException($"The first character of a resource name must not have all the following characters: {InvalidFirstCharacterNameCharacters}.", "Name");
                }
            }

            for (System.Int32 I = 1; I < Name.Length; I++)
            {
                for (System.Int32 J = 0; J < InvalidNameCharacters.Length; J++)
                {
                    if (Name[I] == InvalidNameCharacters[J])
                    {
                        throw new ArgumentException($"A resource name must not have all the following characters: {InvalidNameCharacters}.", "Name");
                    }
                }
            }
        }

        public static System.Object ParseAsNumericValue(System.String Value)
        {
            // Parses any raw string input as a numeric value. An exception is thrown if Value is not a numeric value.
            System.Object ret = null;
            try { ret = System.Byte.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.Int16.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.UInt16.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.Int32.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.UInt32.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.Int64.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.UInt64.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.Single.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            // The last try statement does not need goto since it will unconditionally go there.
            try { ret = System.Double.Parse(Value, System.Globalization.NumberStyles.Integer); } catch { }
            G_Ret:
            if (ret == null) { throw new FormatException("Could not parse the number. Maybe the string input is not a number?"); }
            return ret;
        }

        public static System.Byte[] ReadBuffered(System.IO.Stream Stream, System.Int64 RequestedBytes)
        {
            // Check for null conditions or whether we can read from this stream
            if (Stream == null) { return null; }
            if (Stream.CanRead == false) { return null; }
            // Create a new byte array with the requested size.
            System.Byte[] Contents = new System.Byte[RequestedBytes];
            if (RequestedBytes <= BUFSIZE)
            {
                // Read all bytes directly , if the requested bytes are less than the buffer limit.
                // Otherwise we don't care here; we do not read thousands or millions of bytes.
                Stream.Read(Contents, 0, Contents.Length);
            }
            else
            {
                System.Int32 Count;
                System.Int32 Offset = 0;
                // Read all bytes with buffered mode.
                do
                {
                    Count = Stream.Read(Contents, Offset, BUFSIZE);
                    Offset += BUFSIZE;
                    // Condition specifies that the loop will continue to run when the read bytes are
                    // more or equal than the buffer limit , plus make sure that the next read will not
                    // surpass the bytes that the final array can hold.
                } while ((Count >= BUFSIZE) && (Offset + BUFSIZE <= Contents.Length));
                // In case that the bytes were surpassed in the above condition , pass all the rest bytes again normally.
                if (Contents.Length - Offset > 0) { Stream.Read(Contents, Offset, Contents.Length - Offset); }
            }
            return Contents;
        }

        public static ResourceClasses FromFileName(System.String FileName)
        {
            if (FileName is null) { throw new ArgumentNullException(nameof(FileName)); }
            if (FileName.Length == 0) { throw new ArgumentException("FileName must not be empty."); }
            System.Int32 idx = FileName.LastIndexOf('.');
            if (idx < 0) { throw new ArgumentException("FileName must contain at least one dot."); }
            return FileName.Substring(idx) switch {
                ".resj" => ResourceClasses.CustomJSON,
                ".resx" => ResourceClasses.ResX,
                ".resb" => ResourceClasses.CustomBinary,
                ".resi" => ResourceClasses.CustomMsIni,
                ".rescx" => ResourceClasses.CustomResX,
                ".resxx" => ResourceClasses.CustomXml,
                ".resources" => ResourceClasses.DotNetResources,
                _ => ResourceClasses.Unknown
            };
        }

        public static System.Resources.IResourceWriter CreateWriter(System.String FileName) => CreateWriter(FromFileName(FileName), FileName);

        public static System.Resources.IResourceWriter CreateWriter(ResourceClasses desired , System.String FileName)
        {
            switch (desired)
            {
                case ResourceClasses.DotNetResources:
                    return new DotNetResourcesExtensions.Internal.DotNetResources.PreserializedResourceWriter(FileName);
                case ResourceClasses.ResX:
                    return new System.Resources.ResXResourceWriter(FileName);
                case ResourceClasses.CustomXml:
                    return new DotNetResourcesExtensions.XMLResourcesWriter(FileName);
                case ResourceClasses.CustomMsIni:
                    return new DotNetResourcesExtensions.MsIniResourcesWriter(FileName);
                case ResourceClasses.CustomResX:
                    return new DotNetResourcesExtensions.Internal.ResX.ResXResourceWriter(FileName);
                case ResourceClasses.CustomBinary:
                    return new DotNetResourcesExtensions.CustomBinaryResourceWriter(FileName);
                case ResourceClasses.CustomJSON:
                    return new DotNetResourcesExtensions.JSONResourcesWriter(FileName);
                default:
                    throw new InvalidOperationException("Attempted to create an unknown writer. This is invalid.");
            }
        }

        public static System.Resources.IResourceReader CreateReader(System.String FileName) => CreateReader(FromFileName(FileName), FileName);

        public static System.Resources.IResourceReader CreateReader(ResourceClasses desired , System.String FileName)
        {
            switch (desired)
            {
                case ResourceClasses.DotNetResources:
                    return new DotNetResourcesExtensions.Internal.DotNetResources.DeserializingResourceReader(FileName);
                case ResourceClasses.ResX:
                    return new System.Resources.ResXResourceReader(FileName);
                case ResourceClasses.CustomXml:
                    return new DotNetResourcesExtensions.XMLResourcesReader(FileName);
                case ResourceClasses.CustomMsIni:
                    return new DotNetResourcesExtensions.MsIniResourcesReader(FileName);
                case ResourceClasses.CustomResX:
                    return new DotNetResourcesExtensions.Internal.ResX.ResXResourceReader(FileName);
                case ResourceClasses.CustomBinary:
                    return new DotNetResourcesExtensions.CustomBinaryResourceReader(FileName);
                case ResourceClasses.CustomJSON:
                    return new DotNetResourcesExtensions.JSONResourcesReader(FileName);
                default:
                    throw new InvalidOperationException("Attempted to create an unknown writer. This is invalid.");
            }
        }

        public static DotNetResourcesExtensions.BuildTasks.OutputResourceType ToResourceType(this ResourceClasses rclass) => rclass switch { 
            ResourceClasses.DotNetResources => DotNetResourcesExtensions.BuildTasks.OutputResourceType.Resources,
            ResourceClasses.CustomBinary => DotNetResourcesExtensions.BuildTasks.OutputResourceType.CustomBinary,
            ResourceClasses.CustomJSON => DotNetResourcesExtensions.BuildTasks.OutputResourceType.JSON,
            _ => throw new ArgumentException("Only DotNetResources , CustomBinary and CustomJSON are allowed as inputs.")
        };

        public static DotNetResourcesExtensions.BuildTasks.ResourceClassVisibilty ToVisibilityFromString(System.String visibility)
        => visibility.ToLowerInvariant() switch { 
            "public" => DotNetResourcesExtensions.BuildTasks.ResourceClassVisibilty.Public,
            "internal" => DotNetResourcesExtensions.BuildTasks.ResourceClassVisibilty.Internal,
            _ => throw new ArgumentException("This string cannot be parsed to the visibility enumeration.")
        };

        public static void ShowCriticalMessageAndExit_Dispose(System.Exception e , System.String type)
        {
            ShowErrorMessage($"CRITICAL: {type} was unexpectedly locked. Error is occuring from\n" +
                            $"a {e.GetType().FullName} . The Resource Editor will shut down as soon as you press \'OK\'");
            System.Environment.FailFast($"The DotNetResourcesExtensions Resource Editor application was shut down due to a locking exception while attempting to free resources from the resource {type.ToLower()}.", e);
        }

        public static void EnsureDisposeOrFail(System.IDisposable disposable , System.String type)
        {
            try { disposable?.Dispose(); } catch (System.Exception e) { ShowCriticalMessageAndExit_Dispose(e , type); }
        }
    }

    public enum ResourceClasses : System.Byte
    {
        Unknown,
        CustomJSON,
        CustomXml,
        CustomMsIni,
        CustomBinary,
        CustomResX,
        ResX,
        DotNetResources
    }

    // Copied from BuildTasks.cs
    /// <summary>
    /// Provides a very minimal resource loader. It directly wraps a IResourceReader instance without additional checks.
    /// </summary>
    internal sealed class MinimalResourceLoader : DotNetResourcesExtensions.OptimizedResourceLoader
    {
        public MinimalResourceLoader(System.Resources.IResourceReader rdr) : base() { read = rdr; }

        public override void Dispose()
        {
            read = null; // Directly set this so as to avoid of being disposed accidentally by the internal mechanisms.
            base.Dispose();
        }

        public override System.Threading.Tasks.ValueTask DisposeAsync() => new(System.Threading.Tasks.Task.Run(Dispose));
    }

}

namespace DotNetResourcesExtensions.BuildTasks
{
    public enum OutputResourceType : System.Byte
    {
        Resources,
        CustomBinary,
        JSON
    }
}