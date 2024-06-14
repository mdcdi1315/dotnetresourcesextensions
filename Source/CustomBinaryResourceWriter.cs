using System;
using System.Text;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
    using Internal;
    using Internal.CustomFormatter;

    /// <summary>
    /// The <see cref="CustomBinaryResourceWriter"/> class defines a new resource format that depends solely on binary data ,  <br />
    /// just like the <see cref="System.Resources.Extensions.PreserializedResourceWriter"/> does. <br />
    /// This is it's writer counterpart. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class CustomBinaryResourceWriter : System.Resources.IResourceWriter,IStreamOwnerBase
    {
        private System.IO.FileStream temporary;
        private System.IO.Stream targetstream;
        private System.Boolean isstreamowner , generated;
        private List<System.Int64> representations;

        private CustomBinaryResourceWriter() 
        {
            representations = new();
            generated = false;
            isstreamowner = false;
            temporary = new System.IO.FileStream(System.IO.Path.GetTempFileName(), System.IO.FileMode.Open);
        }

        /// <summary>
        /// Create a new instance of <see cref="CustomBinaryResourceWriter"/> with the specified stream as the data output.
        /// </summary>
        /// <param name="stream">The stream in which the result data will be written to.</param>
        public CustomBinaryResourceWriter(System.IO.Stream stream) : this()
        {
            targetstream = stream;
            targetstream.Position = 0;
        }

        /// <summary>
        /// Create a new instance of <see cref="CustomBinaryResourceWriter"/> that writes to the specified file path.
        /// </summary>
        /// <param name="path">The file path where the data will be saved to.</param>
        public CustomBinaryResourceWriter(System.String path) : this()
        {
            targetstream = new System.IO.FileStream(path, System.IO.FileMode.Create);
            targetstream.Position = 0;
            isstreamowner = true;
        }

        private void GenerateHeader()
        {
            BinaryHeaderReaderWriter br = new();
            br.CreateNew(representations.ToArray());
            targetstream.Position = 0;
            targetstream.SetLength(0);
            targetstream.Write(br.Result.ToArray(), 0, (System.Int32)br.HeaderLength);
            br = null;
            temporary.Position = 0;
            ParserHelpers.BlockCopy(temporary, targetstream);
            generated = true;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException">The specified resource name was invalid.</exception>
        /// <exception cref="InvalidOperationException">The method was called after Generate().</exception>
        public void AddResource(string name, string value)
        {
            ParserHelpers.ValidateName(name);
            BinaryResourceRepresentation brp = new(name , Encoding.UTF8.GetBytes(value) , typeof(System.String) ,  BinaryRESTypes.String);
            temporary.Write(brp.FinalBytes.ToArray() , 0 , brp.FinalBytes.Count);
            representations.Add(brp.FinalBytes.Count);
            brp = null;
            generated = false; // We must re-false the generated so that the generator must regenerate the result.
        }

        /// <inheritdoc />
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ArgumentException">The specified resource name was invalid.</exception>
        /// <exception cref="InvalidOperationException">The method was called after Generate().</exception>
        public void AddResource(System.String name , System.Object value) 
        {
            ParserHelpers.ValidateName(name);
            ExtensibleFormatter EF = new();
            BinaryResourceRepresentation brp = new(name, 
                EF.GetBytesFromObject(value), value.GetType(), 
                BinaryRESTypes.Object);
            EF = null;
            temporary.Write(brp.FinalBytes.ToArray(), 0, brp.FinalBytes.Count);
            representations.Add(brp.FinalBytes.Count);
            brp = null;
            generated = false; // We must re-false the generated so that the generator must regenerate the result.
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException">The specified resource name was invalid.</exception>
        /// <exception cref="OverflowException"></exception>
        /// <exception cref="InvalidOperationException">The method was called after Generate().</exception>
        public void AddResource(string name, byte[] value)
        {
            if (value.Length > System.Int32.MaxValue - 1400) { throw new OverflowException("The byte array length is extravagantly large to fit into the resource."); }
            ParserHelpers.ValidateName(name);
            BinaryResourceRepresentation brp = new(name, value, typeof(System.Byte[]), BinaryRESTypes.ByteArray);
            temporary.Write(brp.FinalBytes.ToArray(), 0, brp.FinalBytes.Count);
            representations.Add(brp.FinalBytes.Count);
            brp = null;
            generated = false; // We must re-false the generated so that the generator must regenerate the result.
        }

        /// <inheritdoc />
        /// <remarks>After this method is called , no more modifications can be performed.</remarks>
        public void Generate() { targetstream?.Flush(); GenerateHeader(); }

        /// <inheritdoc />
        public void Close() 
        { 
            if (generated == false) { GenerateHeader(); }
            try { targetstream?.Flush(); } catch (ObjectDisposedException) { }
            if (isstreamowner) { targetstream?.Close(); }
            temporary?.Close();
        }

        /// <summary>
        /// Gets or sets a value whether this class controls the lifetime of the underlying stream.
        /// </summary>
        public bool IsStreamOwner { get => isstreamowner; set => isstreamowner = value; }

        /// <summary>
        /// Disposes the <see cref="CustomBinaryResourceWriter"/> class.
        /// </summary>
        public void Dispose()
        {
            Close();
            if (isstreamowner) { targetstream?.Dispose(); }
            representations?.Clear();
            if (temporary != null) 
            {
                System.String fp = temporary.Name;
                temporary.Dispose(); 
                System.IO.File.Delete(fp);
                temporary = null;
            }
            representations = null;
            targetstream = null;
        }
    }
}
