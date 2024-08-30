
namespace DotNetResourcesExtensions.Internal
{
    internal static class XMLResourcesConstants
    {
        public const System.UInt16 Version = 2;
        /// <summary>
        /// Note: Header Version for custom XML is deprecated.
        /// Just keep this constant for V1 compat
        /// </summary>
        public const System.UInt16 HeaderVersion = 1;
        public const System.String Magic = "mdcdi1315.XML.RESOURCE.NET";
        public const System.String XMLHeader = "MDCDI1315-XML-RESOURCE-TABLE";
        public const System.String DataObjectName = "ResData";
        public const System.String SingleResourceObjName = "Data";
        public const XMLRESResourceType CurrentMask = XMLRESResourceType.FileReference;
        public const System.String GlobalNameSpaceName = "MDCDI1315-RES";
        public const System.String DefaultExceptionMsg = "This is not the XML Resources conforming format. Reader stopped.";
        public const System.Int32 SingleBase64ChunkCharacterLength = 488;

        public static System.Xml.XmlWriterSettings GlobalWriterSettings;
        public static System.Xml.XmlReaderSettings GlobalReaderSettings;
    
        static XMLResourcesConstants()
        {
            GlobalReaderSettings = new()
            {
                CloseInput = false,
                IgnoreComments = true,
                ConformanceLevel = System.Xml.ConformanceLevel.Document
            };

            GlobalWriterSettings = new()
            {
                CloseOutput = false,
                Indent = true,
                Encoding = System.Text.Encoding.UTF8,
                NewLineChars = "\n",
                IndentChars = "\t",
                ConformanceLevel = System.Xml.ConformanceLevel.Document
            };
        }
    }

    internal enum XMLRESResourceType : System.Byte { String , ByteArray, Object , FileReference }
}
