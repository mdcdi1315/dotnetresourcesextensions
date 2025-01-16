
using System;

namespace DotNetResourcesExtensions.BuildTasks
{

    internal record class OutputItemData
    {
        public System.Boolean HasValidData;
        public System.String FilePath;
        public OutputResourceType OutType;
        public InputItemData[] Inputs;
    }

    internal record class InputItemData
    {
        public System.Boolean HasValidData;
        public System.String FilePath;
        public System.Boolean GenerateStrClass;
        public System.String OutputStrFilePath;
        public System.String ManifestResourceName;
        public System.String ClassName;
        public System.String ClassLang;
        public ResourceClassVisibilty ClsVisibility;
    }

}