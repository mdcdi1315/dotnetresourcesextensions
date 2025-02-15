
using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
    using Internal;
    using Internal.CustomFormatter;

    /// <summary>
    /// The JSON Resources Enumerator is used only with the <see cref="JSONResourcesReader"/> class. <br />
    /// You cannot create an instance of this class. Instead , to get an instance you must use the 
    /// <see cref="JSONResourcesReader.GetEnumerator()"/> method , and cast to this class.
    /// </summary>
    public sealed class JSONResourcesEnumerator : Collections.AbstractDualResourceEntryEnumerator
    {
        private JSONResourcesReader reader;
        private System.Int32 ElementsCount;
        private System.Int32 CurrentIndex;
        private JSONResourceEntry cachedresource;
        private System.Boolean resourceread;

        private sealed class JSONResourceEntry : IResourceEntryWithComment
        {
            private System.String name , cmt;
            private System.Object value;

            public JSONResourceEntry(System.String Name , System.Object Value , System.String Comment) 
            {
                name = Name;
                value = Value;
                cmt = Comment;
            }

            public JSONResourceEntry() { }

            public System.String Name { get => name; set => name = value; }

            public System.Object Value { get => value; set => this.value = value; }

            public System.String Comment { get => cmt; set => cmt = value; }

            public System.Type TypeOfValue => value?.GetType();
        }

        private static JSONRESResourceType ParseType(System.String stringdat) 
        => stringdat.ToLowerInvariant() switch { 
            "string" => JSONRESResourceType.String,
            "bytearray" => JSONRESResourceType.ByteArray,
            "object" => JSONRESResourceType.Object,
            "filereference" => JSONRESResourceType.FileReference,
            "fileref" => JSONRESResourceType.FileReference,
            _ => ParserHelpers.ParseEnumerationConstant<JSONRESResourceType>(stringdat)
        };

        private JSONResourcesEnumerator()
        {
            cachedresource = default;
            resourceread = false;
            CurrentIndex = -1;
            ElementsCount = 0;
        }

        internal JSONResourcesEnumerator(JSONResourcesReader rd) : this()
        { reader = rd; ElementsCount = reader.JE.GetProperty("Data").GetArrayLength(); }

        private JSONResourceEntry GetResource()
        {
            var jdt = reader.JE.GetProperty("Data")[CurrentIndex];
            JSONResourceEntry result;
            // To determine what to decode , we must get first the header version.
            System.UInt16 ver = jdt.GetProperty("HeaderVersion").GetUInt16();
            // Throw exception if the read header has greater version than the allowed
            if (ver > reader.CurrentHeaderVersion)
            {
                throw new JSONFormatException(Properties.Resources.DNTRESEXT_JSONFMT_CANNOT_READ_HEADER, ParserErrorType.Deserialization);
            }
            DictionaryEntry tmp;
            // Now , switch through cases to find the appropriate decoder.
            switch (ver)
            {
                case 1:
                    tmp = GetV1Resource(jdt);
                    result = new(tmp.Key.ToString() , tmp.Value , System.String.Empty);
                    break;
                case 2:
                    tmp = GetV2Resource(jdt);
                    result = new(tmp.Key.ToString(), tmp.Value, System.String.Empty);
                    break;
                case 3:
                    result = GetV3Resource(jdt);
                    break;
                default:
                    throw new JSONFormatException(Properties.Resources.DNTRESEXT_JSONFMT_DECODE_VERSION_NOT_FOUND, $"Version string was {ver}." , ParserErrorType.Deserialization);
            }
            resourceread = true;
            return result;
        }

        // This method only reads V3 resources.
        private JSONResourceEntry GetV3Resource(System.Text.Json.JsonElement JSE)
        {
            [System.Diagnostics.DebuggerHidden]
            static System.Byte[] GetV3ByteArray(System.Text.Json.JsonElement JE)
            {
                System.Int64 flen = JE.GetProperty("TotalLength").GetInt64(),
                        chunks = JE.GetProperty("Chunks").GetInt64(),
                        alignment = JE.GetProperty("Base64Alignment").GetInt64(),
                        // Means Correct Chunk Count. 
                        // At least chunks-1 must be chunks with correct alignment.
                        // The last is excluded because it might not save all characters (due to divide operations).
                        ccc = 0;
                System.Text.Json.JsonElement array = JE.GetProperty("Value");
                System.Text.StringBuilder sb = new((chunks * alignment).ToInt32());
                System.String t2;
                for (System.Int32 I = 0; I < chunks; I++)
                {
                    // We had perf issues in V2 in byte arrays. The V3 method is much faster than the V2 one , although that V2 is also optimized
                    // but not still so fast as now V3 is.
                    t2 = array[I].GetString();
                    if (t2.Length == alignment) { ccc++; }
                    sb.Append(t2);
                }
                t2 = null;
                if (ccc < chunks - 1)
                {
                    throw new JSONFormatException(System.String.Format(Properties.Resources.DNTRESEXT_JSONFMT_INVALID_DATA_TOPIC_ALIGNMENT , alignment), ParserErrorType.Deserialization);
                }
                System.Byte[] bt = sb.ToString().FromBase64();
                sb = null;
                if (bt.LongLength != flen) { throw new JSONFormatException(System.String.Format(Properties.Resources.DNTRESEXT_JSONFMT_INVALID_DATA_LENMISMATCH , flen , bt.LongLength), ParserErrorType.Deserialization); }
                return bt;
            }
            static System.String GetFullPath(System.String fn)
            {
                System.String ret = System.String.Empty;
                if (System.String.IsNullOrEmpty(fn)) { return ret; }
                if (fn.Length == 0) { return ret; }
                ret = ParserHelpers.RemoveQuotes(fn);
                return System.IO.Path.GetFullPath(ret);
            }
            // For first stage , get the resource type to read.
            System.Text.Json.JsonElement restype = JSE.GetProperty("ResourceType");
            JSONRESResourceType type;
            if (restype.ValueKind == System.Text.Json.JsonValueKind.String) {
                type = ParseType(restype.GetString());
            } else if (restype.ValueKind == System.Text.Json.JsonValueKind.Number) {
                type = (JSONRESResourceType)restype.GetInt64();
            } else {
                throw new JSONFormatException(Properties.Resources.DNTRESEXT_JSONFMT_INVALID_RESTYPEDATATYPE, ParserErrorType.Deserialization);
            }
            if (type > reader.CurrentActiveMask) { goto g_640; }
            JSONResourceEntry result = new();
            result.Name = JSE.GetProperty("ResourceName").GetString();
            // The Comment property is an optional property , so always attempt to get it and do only assign if it was found.
            if (JSE.TryGetProperty("Comment", out var jdt)) { result.Comment = jdt.GetString(); }
            switch (type) {
                case JSONRESResourceType.String:
                    result.Value = JSE.GetProperty("Value").GetString();
                    break;
                case JSONRESResourceType.ByteArray:
                    result.Value = GetV3ByteArray(JSE);
                    break;
                case JSONRESResourceType.Object:
                    // We have a serialised object. Deserialise it using the CustomFormatter.
                    // Get the type for the object to return.
                    System.String ObjType = JSE.GetProperty("DotnetType").GetString();
                    // Get the object itself.
                    result.Value = reader.exf.GetObjectFromBytes(GetV3ByteArray(JSE), 
                        System.Type.GetType(ObjType, true, true));
                    break;
                case JSONRESResourceType.FileReference:
                    // We have a file reference. Read it , so.
                    System.Text.Json.JsonElement fileref = JSE.GetProperty("Value");
                    // OK. We have the file reference. Now , open the file and the encoding will determine whether we will read in binary style or will be returned as a string.
                    System.IO.FileStream FS = null;
                    try {
                        FS = new System.IO.FileStream(GetFullPath(fileref[0].GetString()) , System.IO.FileMode.Open);
                        System.Byte[] temp = FS.ReadBytes(FS.Length);
                        FS.Close();
                        System.Type underlying;
                        try {
                            underlying = System.Type.GetType(fileref[1].GetString(), true, false);
                        } catch (System.TypeLoadException tle) {
                            try {
                                underlying = reader.fraliasresolver.ResolveAlias(fileref[1].GetString());
                            } catch (System.ArgumentException) { throw tle; }
                        }
                        result.Value = underlying.FullName switch
                        {
                            // This applies for strings.
                            "System.String" => ParserHelpers.ParseEnumerationConstant<FileReferenceEncoding>(fileref[2].GetString()).AsEncoding().GetString(temp),
                            // Byte arrays.
                            "System.Byte[]" => temp,
                            // Serialized objects.
                            _ => reader.exf.GetObjectFromBytes(temp, underlying),
                        };
                    } finally {
                        FS?.Dispose();
                        FS = null;
                        fileref = default;
                    }
                    break;
                default:
                    goto g_640;        
            }
            return result;
        g_640:
            throw new JSONFormatException(Properties.Resources.DNTRESEXT_JSONFMT_RESTYPE_INVALID, ParserErrorType.Deserialization);
        }

        // This method only reads V2 resources.
        private DictionaryEntry GetV2Resource(System.Text.Json.JsonElement JE)
        {
            [System.Diagnostics.DebuggerHidden]
            static System.Byte[] GetV2ByteArray(System.Text.Json.JsonElement JE)
            {
                System.Int64 flen = JE.GetProperty("TotalLength").GetInt64(),
                        chunks = JE.GetProperty("Chunks").GetInt64(),
                        alignment = JE.GetProperty("Base64Alignment").GetInt64() , 
                        // Means Correct Chunk Count. 
                        // At least chunks-1 must be chunks with correct alignment.
                        // The last is excluded because it might not save all characters (due to divide operations).
                        ccc = 0;
                System.Text.StringBuilder sb = new((chunks * alignment).ToInt32());
                System.String t2;
                for (System.Int32 I = 1; I <= chunks; I++) {
                    // Because the compiler-generated code for this method boxes the chunk index , 
                    // we will construct the string for GetProperty ourselves.
                    t2 = JE.GetProperty(System.String.Concat("Value[" , I.ToString() , "]")).GetString();
                    if (t2.Length == alignment) { ccc++; }
                    sb.Append(t2);
                }
                t2 = null;
                if (ccc < chunks-1) {
                    throw new JSONFormatException(System.String.Format(Properties.Resources.DNTRESEXT_JSONFMT_INVALID_DATA_TOPIC_ALIGNMENT, alignment), ParserErrorType.Deserialization);
                }
                System.Byte[] bt = sb.ToString().FromBase64();
                sb.Clear();
                sb = null;
                if (bt.LongLength != flen) { throw new JSONFormatException(System.String.Format(Properties.Resources.DNTRESEXT_JSONFMT_INVALID_DATA_LENMISMATCH , flen , bt.LongLength), ParserErrorType.Deserialization); }
                return bt;
            }
            // For first stage , get the resource type to read.
            JSONRESResourceType type = (JSONRESResourceType)JE.GetProperty("ResourceType").GetUInt16();
            if (type > reader.CurrentActiveMask) { throw new JSONFormatException(Properties.Resources.DNTRESEXT_JSONFMT_RESTYPE_INVALID, ParserErrorType.Deserialization); }
            DictionaryEntry result = new();
            result.Key = JE.GetProperty("ResourceName").GetString();
            switch (type) {
                case JSONRESResourceType.ByteArray:
                    // Use GetV2ByteArray method and return the results as-it-is.
                    result.Value = GetV2ByteArray(JE);
                    break;
                case JSONRESResourceType.Object:
                    // We have a serialised object. Deserialise it using the CustomFormatter.
                    // Get the type for the object to return.
                    System.String ObjType = JE.GetProperty("DotnetType").GetString();
                    // Get the object itself.
                    result.Value = reader.exf.GetObjectFromBytes(
                        GetV2ByteArray(JE) , System.Type.GetType(ObjType, true, true));
                    break;
            }
            return result;
        }

        // This method only reads V1 resources.
        private DictionaryEntry GetV1Resource(System.Text.Json.JsonElement JE)
        {
            System.Byte[] GetByteArrayResource()
            {
                // The byte array is written into base64's.
                System.Int32 expl = JE.GetProperty("TotalLength").GetInt32();
                System.Byte[] data = JE.GetProperty("Value[0]").GetBytesFromBase64();
                if (data.LongLength != expl) { throw new JSONFormatException(System.String.Format(Properties.Resources.DNTRESEXT_JSONFMT_INVALID_DATA_LENMISMATCH ,expl , data.LongLength) , ParserErrorType.Deserialization); }
                return data;
            }
            // For first stage , get the resource type to read.
            JSONRESResourceType type = (JSONRESResourceType)JE.GetProperty("ResourceType").GetUInt16();
            if (type > reader.CurrentActiveMask) { throw new JSONFormatException(Properties.Resources.DNTRESEXT_JSONFMT_RESTYPE_INVALID , ParserErrorType.Deserialization); }
            DictionaryEntry result = new();
            result.Key = JE.GetProperty("ResourceName").GetString();
            switch (type)
            {
                case JSONRESResourceType.String:
                    result.Value = JE.GetProperty("Value[0]").GetString();
                    break;
                case JSONRESResourceType.ByteArray:
                    result.Value = GetByteArrayResource();
                    break;
                case JSONRESResourceType.Object:
                    // We have a serialised object. Deserialise it using the CustomFormatter.
                    // Get the type for the object to return.
                    System.String ObjType = JE.GetProperty("DotnetType").GetString();
                    // Get the object itself.
                    result.Value = reader.exf.GetObjectFromBytes(
                        JE.GetProperty("Value[0]").GetBytesFromBase64()
                        , System.Type.GetType(ObjType , true , true));
                    break;
            }
            return result;
        }

        /// <inheritdoc />
        public override void Reset() => CurrentIndex = -1;

        /// <inheritdoc />
        public override bool MoveNext()
        {
            resourceread = false;
            CurrentIndex++;
            return CurrentIndex < ElementsCount;
        }

        /// <inheritdoc />
        // For accessing directly the resource name if you do not want to read the resource.
        public override object Key {
            get {
                if (CurrentIndex == -1) { throw new InvalidOperationException("The enumerator is uninitialized."); }
                if (resourceread) { return cachedresource.Name; }
                return reader.JE.GetProperty("Data")[CurrentIndex].GetProperty("ResourceName").GetString();
            }
        }

        /// <inheritdoc />
        // Unfortunately , accessing the value requires the resource to be read.
        public override object Value => ResourceEntry.Value;

        /// <inheritdoc />
        public override DictionaryEntry Entry => ResourceEntry.AsDictionaryEntry();

        /// <inheritdoc />
        public override IResourceEntry ResourceEntry
        {
            get {
                if (CurrentIndex == -1) { throw new InvalidOperationException("The enumerator is uninitialized."); }
                if (resourceread == false) { cachedresource = GetResource(); }
                return cachedresource;
            }
        }

        /// <summary>
        /// Disposes the enumerator instance.
        /// </summary>
        public sealed override void Dispose()
        {
            reader = null;
            resourceread = false;
            CurrentIndex = 0;
            ElementsCount = 0;
            cachedresource = null;
            base.Dispose();
        }
    }

    /// <summary>
    /// The <see cref="JSONResourcesReader"/> reads the custom resources format of <see cref="JSONResourcesWriter"/>. <br />
    /// You can use it so as to read such data. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class JSONResourcesReader : IDotNetResourcesExtensionsReader
    {
        private System.Text.Json.JsonDocument JDT;
        internal ExtensibleFormatter exf;
        internal System.Text.Json.JsonElement JE;
        internal JSONRESResourceType CurrentActiveMask;
        internal IFileReferenceTypeAliasResolver fraliasresolver;
        internal System.UInt16 CurrentHeaderVersion;

        private JSONResourcesReader()
        {
            exf = new();
            CurrentActiveMask = 0;
            CurrentHeaderVersion = 0;
            fraliasresolver = new BasicFileReferenceTypeAliasResolver();
            JDT = null;
        }

        /// <summary>
        /// Creates a new instance of <see cref="JSONResourcesReader"/> from the specified data stream.
        /// </summary>
        /// <param name="data">The data stream to read resources from.</param>
        public JSONResourcesReader(System.IO.Stream data) : this()
        {
            JDT = System.Text.Json.JsonDocument.Parse(data, JSONRESOURCESCONSTANTS.JsonReaderOptions);
            JE = JDT.RootElement;
            ValidateAndParse();
        }

        /// <summary>
        /// Creates a new instance of <see cref="JSONResourcesReader"/> from the specified file.
        /// </summary>
        /// <param name="path">The path of the file to read. The file must have been written using the <see cref="JSONResourcesWriter"/> class.</param>
        public JSONResourcesReader(System.String path) : this()
        {
            System.IO.FileStream FS = new(path, System.IO.FileMode.Open);
            JDT = System.Text.Json.JsonDocument.Parse(FS, JSONRESOURCESCONSTANTS.JsonReaderOptions);
            JE = JDT.RootElement;
            FS.Dispose();
            ValidateAndParse();
        }

        private void ValidateAndParse()
        {
            System.Exception g;
            try
            {
                var header = JE.GetProperty(JSONRESOURCESCONSTANTS.JSONHeader);
                System.UInt16 version;
                if ((version = header.GetProperty("Version").GetUInt16()) < JSONRESOURCESCONSTANTS.Version)
                {
                    g = new JSONFormatException(System.String.Format(Properties.Resources.DNTRESEXT_JSONFMT_VERMISMATCH , version), ParserErrorType.Header);
                    goto g_639;
                }
                CurrentActiveMask = (JSONRESResourceType)header.GetProperty("SupportedFormatsMask").GetUInt16();
                if (header.GetProperty("Magic").GetString() != JSONRESOURCESCONSTANTS.Magic)
                {
                    g = new JSONFormatException(Properties.Resources.DNTRESEXT_JSONFMT_INVALIDMAGIC, ParserErrorType.Header);
                    goto g_639;
                }
                CurrentHeaderVersion = header.GetProperty("CurrentHeaderVersion").GetUInt16();
                if (version == 2 && header.TryGetProperty("FileReferenceTypeAliases" , out var aliases))
                {
                    foreach (var alias in aliases.EnumerateArray())
                    {
                        // The object array format is as in the following example:
                        // "FileReferenceTypeAliases": [
                        //      {
                        //          "alias": "SystemFunction",
                        //          "type": "System.Func`1, System.Private.CoreLib, Version=9.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"
                        //      }
                        //      // We may have more aliases here too..
                        // ]
                        fraliasresolver.RegisterAlias(alias.GetProperty("alias").GetString(),
                            System.Type.GetType(alias.GetProperty("type").GetString() , true , true));
                    }
                }
            } catch (KeyNotFoundException DD1) { g = DD1; goto g_639; }
            return;
        g_639:
            if (g is not null) { throw new JSONFormatException(Properties.Resources.DNTRESEXT_JSONFMT_MSG_GENERIC, g.Message , ParserErrorType.Header); }
        }

        /// <inheritdoc />
        /// <remarks>Note: For this class definition , this method effectively does nothing.</remarks>
        public void Close() { }

        /// <summary>
        /// Disposes the <see cref="JSONResourcesReader"/> class.
        /// </summary>
        public void Dispose() { JE = default; JDT?.Dispose(); }
        
        /// <inheritdoc cref="System.Resources.IResourceReader.GetEnumerator" />
        public JSONResourcesEnumerator GetEnumerator() => new(this);

        IDictionaryEnumerator System.Resources.IResourceReader.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public void RegisterTypeResolver(ITypeResolver resolver) => exf.RegisterTypeResolver(resolver);

        System.Boolean IStreamOwnerBase.IsStreamOwner { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    }

}
