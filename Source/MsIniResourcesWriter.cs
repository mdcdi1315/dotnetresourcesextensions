
using System;
using System.Collections.Generic;
using DotNetResourcesExtensions.Internal;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// The <see cref="MsIniResourcesWriter"/> is a new class that writes out resources 
    /// using a syntax similar to the MS-INI one. <br />
    /// It uses valid (As most as possible) MS-INI syntax semantics , and writes out all the required information for later retrieving the resources. <br />
    /// It works very well and fast with small input , such as string or small object resources.
    /// </summary>
    public sealed class MsIniResourcesWriter : IDotNetResourcesExtensionsWriter
    {
        private System.IO.Stream stream;
        private System.Boolean strmown , gen;
        private System.Text.Encoding encoding;
        private ExtensibleFormatter fmt;

        private MsIniResourcesWriter()
        {
            fmt = ExtensibleFormatter.Create();
            encoding = System.Text.Encoding.UTF8;
            stream = null;
            strmown = false;
            gen = false;
        }

        /// <summary>
        /// Constructs a new instance of <see cref="MsIniResourcesWriter"/> using the specified stream that the implementation will write data to.
        /// </summary>
        /// <param name="stream">The stream to write the resources to.</param>
        public MsIniResourcesWriter(System.IO.Stream stream) : this()
        {
            this.stream = stream;
            GenerateHeaderEntity();
        }

        /// <summary>
        /// Constructs a new instance of <see cref="MsIniResourcesWriter"/> using the specified stream that the implementation will write data to , 
        /// and the format encoding that this writer will write in.
        /// </summary>
        /// <param name="stream">The stream to write the resources to.</param>
        /// <param name="enc">The format encoding to use.</param>
        public MsIniResourcesWriter(System.IO.Stream stream , System.Text.Encoding enc)
        {
            this.stream = stream;
            encoding = enc;
            GenerateHeaderEntity();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MsIniResourcesWriter"/> class which writes to the specified file.
        /// </summary>
        /// <param name="savepath">The file to save the generated data to.</param>
        public MsIniResourcesWriter(System.String savepath) : this()
        {
            stream = new System.IO.FileStream(savepath , System.IO.FileMode.Create);
            strmown = true;
            GenerateHeaderEntity();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MsIniResourcesWriter"/> class which writes to the specified file , 
        /// and the format encoding that this writer will write in.
        /// </summary>
        /// <param name="savepath">The file to save the generated data to.</param>
        /// <param name="enc">The format encoding to use.</param>
        public MsIniResourcesWriter(System.String savepath , System.Text.Encoding enc) : this()
        {
            stream = new System.IO.FileStream(savepath, System.IO.FileMode.Create);
            strmown = true;
            encoding = enc;
            GenerateHeaderEntity();
        }

        /// <inheritdoc />
        public System.Boolean IsStreamOwner { get => strmown; set => strmown = value; }

        private (System.Byte[] , System.String) GetObjectAsBytes(System.Object obj)
        {
            if (obj is null) { throw new ArgumentNullException(nameof(obj)); }
            System.String dt;
            if (obj is System.Byte[] bt) {
                dt = MsIniStringsEncoder.Encode(bt.ToBase64());
                return (encoding.GetBytes(dt), obj.GetType().AssemblyQualifiedName);
            } else if (obj is System.String str) {
                dt = MsIniStringsEncoder.Encode(str);
                return (encoding.GetBytes(dt), obj.GetType().AssemblyQualifiedName);
            } else if (obj is IFileReference reference) {
                dt = MsIniStringsEncoder.Encode(reference.ToSerializedString());
                return (encoding.GetBytes(dt), MsIniConstants.SpecialIFileReferenceTypeStr);
            } else {
                System.Byte[] one = fmt.GetBytesFromObject(obj);
                dt = MsIniStringsEncoder.Encode(one.ToBase64());
                one = null;
                return (encoding.GetBytes(dt), obj.GetType().AssemblyQualifiedName);
            }
        }

        private void WriteString(System.String value)
        {
            if (stream is null || value is null) { return; }
            System.String ve = MsIniStringsEncoder.Encode(value);
            System.Byte[] dat = encoding.GetBytes(ve);
            ve = null; // Null it to avoid allocations
            stream.Write(dat, 0, dat.Length);
            dat = null;
        }

        private void WriteComment(System.String comment)
        {
            if (stream is null || comment is null) { return; }
            System.Byte[] dat = encoding.GetBytes($"; {comment}\n");
            stream.Write(dat, 0, dat.Length);
            dat = null;
        }

        private void WriteEntryAndValue(KeyValuePair<System.String, System.Object> pair)
        {
            ParserHelpers.ValidateName(pair.Key);
            var dt = GetObjectAsBytes(pair.Value);
            WriteString(MsIniConstants.ResourceName);
            stream.WriteByte(MsIniConstants.ValueDelimiter);
            WriteString(pair.Key);
            stream.WriteByte(MsIniConstants.Enter);
            WriteString($"{pair.Key}.Length");
            stream.WriteByte(MsIniConstants.ValueDelimiter);
            WriteString(ParserHelpers.NumberToString(dt.Item1.LongLength));
            stream.WriteByte(MsIniConstants.Enter);
            WriteString($"{pair.Key}.Type");
            stream.WriteByte(MsIniConstants.ValueDelimiter);
            WriteString(dt.Item2);
            stream.WriteByte(MsIniConstants.Enter);
            WriteString(MsIniConstants.ResourceValue);
            stream.WriteByte(MsIniConstants.ValueDelimiter);
            stream.Write(dt.Item1, 0, dt.Item1.Length);
            stream.WriteByte(MsIniConstants.Enter);
        }

        private void GenerateEntityWithValues(System.String name , IDictionary<System.String , System.Object> values)
        {
            stream.WriteByte(MsIniConstants.EntityStart);
            System.Byte[] dt = encoding.GetBytes(name);
            stream.Write(dt, 0, dt.Length);
            dt = null;
            stream.WriteByte(MsIniConstants.EntityEnd);
            stream.WriteByte(MsIniConstants.Enter);
            foreach (KeyValuePair<System.String, System.Object> pair in values) {
                WriteEntryAndValue(pair);
            }
        }

        private void GenerateHeaderEntity()
        {
            System.Byte[] data = System.Text.Encoding.ASCII.GetBytes($"; ENCODING: \"{ParserHelpers.NumberToString(encoding.CodePage)}\"\n");
            stream.Write(data, 0, data.Length);
            data = null;
            WriteComment("Auto-Generated Context From DotNetResourcesExtensions.MsIniResourcesWriter .NET class");
            WriteComment("=====================NOTE=======================");
            WriteComment("This is not \'syntactically\' valid MS-INI syntax. It just uses it so as to provide an encoding base on how to retrieve the resources defined in such files.");
            WriteComment("=====================NOTE=======================");
            Dictionary<System.String, System.Object> rd = new()
            {
                { "Version", MsIniConstants.Version },
                { MsIniConstants.ResMask, (System.Int32)MsIniConstants.Mask }
            };
            GenerateEntityWithValues(MsIniConstants.HeaderString, rd);
            stream.WriteByte(MsIniConstants.Enter);
            stream.WriteByte(MsIniConstants.Enter);
            stream.WriteByte(MsIniConstants.EntityStart);
            WriteString("ResourceIndex");
            stream.WriteByte(MsIniConstants.EntityEnd);
            stream.WriteByte(MsIniConstants.Enter);
        }

        /// <inheritdoc />
        public void AddResource(System.String name , System.Byte[] value)
        {
            if (value is null) { throw new ArgumentNullException(nameof(name)); }
            WriteEntryAndValue(new(name, value));
        }

        /// <inheritdoc />
        public void AddResource(System.String name , System.String value)
        {
            WriteEntryAndValue(new(name, value));
        }

        /// <inheritdoc />
        public void AddResource(System.String name , System.Object value)
        {
            if (value is null) { throw new ArgumentNullException(nameof(value)); }
            WriteEntryAndValue(new(name, value));
        }

        /// <summary>
        /// Writes a new file reference to the list of resources to be written.
        /// </summary>
        /// <param name="name">The resource name under which the reference will be retrieved.</param>
        /// <param name="reference">The reference to add.</param>
        public void AddFileReference(System.String name , IFileReference reference)
        {
            WriteEntryAndValue(new(name, reference));
        }

        /// <summary>
        /// Gets the encoding which the writer writes in.
        /// </summary>
        public System.Text.Encoding Encoding => encoding;

        /// <summary>
        /// Flushes the underlying stream where all resources are written to.
        /// </summary>
        public void Generate()
        {
            gen = true;
            stream?.Flush();
        }

        /// <summary>
        /// Does preparation tasks before this instance is finally disposed.
        /// </summary>
        public void Close()
        {
            if (gen == false) { Generate(); }
            if (strmown)
            {
                stream?.Close();
            }
        }

        /// <summary>
        /// Disposes the current instance.
        /// </summary>
        public void Dispose()
        {
            if (gen == false) { Generate(); }
            if (strmown)
            {
                stream?.Dispose();
                stream = null;
            }
            fmt?.Dispose();
            fmt = null;
            encoding = null;
        }

        /// <inheritdoc />
        public void RegisterTypeResolver(ITypeResolver resolver)
        {
            fmt.RegisterTypeResolver(resolver);
        }
    }
}
