

namespace DotNetResourcesExtensions.Internal
{
    internal enum JSONRESResourceType : System.Byte { String, ByteArray, Object }

    internal static class JSONRESOURCESCONSTANTS
    {
        public const System.UInt16 Version = 1;
        public const System.UInt16 HeaderVersion = 1;
        public const System.String Magic = "mdcdi1315.JRT.RESOURCE.NET";
        public const System.String JSONHeader = "MDCDI1315-JSON-RESOURCE-TABLE";
        public const System.String DataObjectName = "Data";
        public const JSONRESResourceType CurrentMask = JSONRESResourceType.Object;
        public const System.Int32 BASE64_SingleLineLength = 2048;
        public static System.Text.Json.JsonSerializerOptions ObjectSerializerOptions;
        public static System.Text.Json.JsonWriterOptions JsonWriterOptions;
        public static System.Text.Json.JsonDocumentOptions JsonReaderOptions;

        static JSONRESOURCESCONSTANTS()
        {
            ObjectSerializerOptions = new() { UnknownTypeHandling = System.Text.Json.Serialization.JsonUnknownTypeHandling.JsonNode };
            JsonWriterOptions = new() { Indented = true, MaxDepth = 7 };
            JsonReaderOptions = new() { CommentHandling = System.Text.Json.JsonCommentHandling.Skip, MaxDepth = 7 };
        }
    }
}
