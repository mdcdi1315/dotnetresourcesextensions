
using System;

namespace DotNetResourcesExtensions.Internal
{
    internal static class MsIniConstants
    {
        public const System.UInt16 Version = 2;
        public const System.Byte EntityStart = (System.Byte)'[';
        public const System.Byte EntityEnd = (System.Byte)']';
        public const System.Byte ValueDelimiter = (System.Byte)'=';
        public const System.Byte Quote = (System.Byte)'\"';
        public const System.Byte Comment = (System.Byte)';';
        public const System.Byte Enter = (System.Byte)'\n';
        public const MS_INIResourceType Mask = MS_INIResourceType.FileReference;
        public const System.String HeaderString = "MDCDI1315-RES-HEADER";
        public const System.String ResMask = "RESOURCEMASK";
        public const System.String ResourceName = "Name";
        public const System.String ResourceValue = "Value";
        public const System.String SpecialIFileReferenceTypeStr = "DotNetResourcesExtensions.IFileReference , DotNetResourcesExtensions";
    }

    internal enum MS_INIResourceType : System.Byte { String, ByteArray, Object , FileReference }
}
