

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
    /// <see cref="JSONResourcesReader.GetEnumerator()"/> method.
    /// </summary>
    public sealed class JSONResourcesEnumerator : IDictionaryEnumerator, IEnumerator
    {
        private JSONResourcesReader reader;
        private System.Int32 ElementsCount;
        private System.Int32 CurrentIndex;
        private DictionaryEntry cachedresource;
        private System.Boolean resourceread;

        private JSONResourcesEnumerator()
        {
            cachedresource = default;
            resourceread = false;
            CurrentIndex = -1;
            ElementsCount = 0;
        }

        internal JSONResourcesEnumerator(JSONResourcesReader rd) : this()
        { reader = rd; ElementsCount = reader.JE.GetProperty("Data").GetArrayLength(); }

        private DictionaryEntry GetResource()
        {
            var jdt = reader.JE.GetProperty("Data")[CurrentIndex];
            DictionaryEntry result;
            // To determine what to decode , we must get first the header version.
            System.UInt16 ver = jdt.GetProperty("HeaderVersion").GetUInt16();
            // Throw exception if the read header has greater version than the allowed
            if (ver > reader.CurrentHeaderVersion)
            {
                throw new JSONFormatException(
                $"This header cannot be read with this version of the class. Please use a reader that supports header version {ver} or higher." ,
                ParserErrorType.Deserialization);
            }
            // Now , switch through cases to find the appropriate decoder.
            switch (ver)
            {
                case 1:
                    result = GetV1Resource(jdt);
                    break;
                case 2:
                    result = GetV2Resource(jdt);
                    break;
                default:
                    throw new JSONFormatException("A decoder version was not found. Internal error detected." , $"Version string was {ver}." , ParserErrorType.Deserialization);
            }
            resourceread = true;
            return result;
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
                System.String temp = System.String.Empty, t2;
                for (System.Int32 I = 1; I <= chunks; I++) {
                    // Because the compiler-generated code for this method boxes the chunk index , 
                    // we will construct the string for GetProperty ourselves.
                    t2 = JE.GetProperty(System.String.Concat("Value[" , I.ToString() , "]")).GetString();
                    if (t2.Length == alignment) { ccc++; }
                    temp += t2;
                }
                t2 = null;
                if (ccc < chunks-1) {
                    throw new JSONFormatException($"Each bytearray chunk must be exactly {alignment} characters.", ParserErrorType.Deserialization);
                }
                System.Byte[] bt = temp.FromBase64();
                temp = null;
                if (bt.LongLength != flen) { throw new JSONFormatException($"The bytes read were not equal to the expected bytes (Expected {flen} while read {bt.LongLength} bytes).", ParserErrorType.Deserialization); }
                return bt;
            }
            // For first stage , get the resource type to read.
            JSONRESResourceType type = (JSONRESResourceType)JE.GetProperty("ResourceType").GetUInt16();
            if (type > reader.CurrentActiveMask) { throw new JSONFormatException($"The type {type} is not currently supported in the reader.", ParserErrorType.Deserialization); }
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
                if (data.LongLength != expl) { throw new JSONFormatException("Could not get correct array length on byte string data. The data might be corrupted." , ParserErrorType.Deserialization); }
                return data;
            }
            // For first stage , get the resource type to read.
            JSONRESResourceType type = (JSONRESResourceType)JE.GetProperty("ResourceType").GetUInt16();
            if (type > reader.CurrentActiveMask) { throw new JSONFormatException($"The type {type} is not currently supported in the reader." , ParserErrorType.Deserialization); }
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
        public void Reset() => CurrentIndex = -1;

        /// <inheritdoc />
        public bool MoveNext()
        {
            resourceread = false;
            CurrentIndex++;
            return CurrentIndex < ElementsCount;
        }

        /// <inheritdoc />
        // For accessing directly the resource name if you do not want to read the resource.
        public object Key {
            get {
                if (CurrentIndex == -1) { throw new InvalidOperationException("The enumerator is uninitialized."); }
                if (resourceread) { return cachedresource.Key; }
                return reader.JE.GetProperty("Data")[CurrentIndex].GetProperty("ResourceName").GetString();
            }
        }

        /// <inheritdoc />
        public object Value => Entry.Value;

        /// <inheritdoc />
        public DictionaryEntry Entry
        {
            get
            {
                if (CurrentIndex == -1) { throw new InvalidOperationException("The enumerator is uninitialized."); }
                if (resourceread) { return cachedresource; }
                else { return (cachedresource = GetResource()); }
            }
        }

        /// <inheritdoc />
        public object Current => Entry;
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
        internal System.UInt16 CurrentHeaderVersion;

        private JSONResourcesReader()
        {
            exf = new();
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
                System.UInt16 placeholder;
                if ((placeholder = header.GetProperty("Version").GetUInt16()) < JSONRESOURCESCONSTANTS.Version)
                {
                    g = new JSONFormatException($"Cannot read a newer version of the JSON resources file. Version found was {placeholder} ." , ParserErrorType.Header);
                    goto g_639;
                }
                CurrentActiveMask = (JSONRESResourceType)header.GetProperty("SupportedFormatsMask").GetUInt16();
                if (header.GetProperty("Magic").GetString() != JSONRESOURCESCONSTANTS.Magic)
                {
                    g = new JSONFormatException("Magic value was incorrect - this reader cannot read this resources format." , ParserErrorType.Header);
                    goto g_639;
                }
                CurrentHeaderVersion = header.GetProperty("CurrentHeaderVersion").GetUInt16();
            }
            catch (KeyNotFoundException DD1) { g = DD1; goto g_639; }
            return;
        g_639:
            if (g != null) { throw new JSONFormatException("This is not a mdcdi1315 JSON Resource Format stream.", g.Message , ParserErrorType.Header); }
        }

        /// <inheritdoc />
        /// <remarks>Note: For this class definition , this method effectively does nothing.</remarks>
        public void Close() { }

        /// <summary>
        /// Disposes the <see cref="JSONResourcesReader"/> class.
        /// </summary>
        public void Dispose() { JE = default; JDT?.Dispose(); }
        
        /// <inheritdoc />
        public IDictionaryEnumerator GetEnumerator() => new JSONResourcesEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public void RegisterTypeResolver(ITypeResolver resolver)
        {
            exf.RegisterTypeResolver(resolver);
        }

        System.Boolean IStreamOwnerBase.IsStreamOwner { get; set; }
    }

}
