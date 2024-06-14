
namespace DotNetResourcesExtensions.Internal
{
    internal static class XMLResourcesConstants
    {
        public const System.UInt16 Version = 1;
        public const System.UInt16 HeaderVersion = 1;
        public const System.String Magic = "mdcdi1315.XML.RESOURCE.NET";
        public const System.String XMLHeader = "MDCDI1315-XML-RESOURCE-TABLE";
        public const System.String DataObjectName = "ResData";
        public const XMLRESResourceType CurrentMask = XMLRESResourceType.Object;
        public const System.String GlobalNameSpaceName = "MDCDI1315-RES";
        public const System.String DefaultExceptionMsg = "This is not the XML Resources conforming format. Reader stopped.";

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

    internal enum XMLRESResourceType : System.Byte { String , ByteArray, Object }
}
