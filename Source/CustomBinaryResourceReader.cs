
using System;

namespace DotNetResourcesExtensions
{
    using Internal;
    using System.Collections;
    using Internal.CustomFormatter;

    /// <summary>
    /// This enumerator is used only with the <see cref="CustomBinaryResourceReader"/> class. <br />
    /// You cannot create an instance of this class; Instead , to get an instance you must use the 
    /// <see cref="CustomBinaryResourceReader.GetEnumerator()"/> method.
    /// </summary>
    public sealed class CustomBinaryResourceEnumerator : Collections.IResourceEntryEnumerator
    {
        private System.Int64 currentindex, heldposition;
        private CustomBinaryResourceReader reader;
        private System.Boolean gotres;
        private BinResourceBlob entry;
        private CustomBinaryResource cbres;

        internal CustomBinaryResourceEnumerator(CustomBinaryResourceReader reader)
        {
            this.reader = reader;
            currentindex = -1;
            cbres = CustomBinaryResource.ReadFromStream(reader.targetstream);
            gotres = false;
            heldposition = reader.originalstreampos + reader.headerblob.WrittenBytesCount;
        }

        private void ReadNext()
        {
            if (gotres) { return; }
            entry = cbres.ReadResource(heldposition , reader.exf);
            if (entry.BytesRead != reader.headerblob.DataPositions[currentindex]) {
                throw new CustomBinaryFormatException($"Read {entry.BytesRead} bytes while expecting to read {reader.headerblob.DataPositions[currentindex]} bytes." , ParserErrorType.Deserialization);
            }
            if (entry.BinResType > reader.headerblob.CurrentFormatsMask) {
                throw new CustomBinaryFormatException($"The format specifies that the resource types allowed are all up to {reader.headerblob.CurrentFormatsMask} but read a value of {entry.BinResType} that is higher than the mask." , ParserErrorType.Versioning);
            }
            heldposition += entry.BytesRead;
            gotres = true;
        }

        /// <inheritdoc />
        public System.Boolean MoveNext()
        {
            gotres = false;
            currentindex++;
            System.Boolean result = currentindex < reader.headerblob.DataPositions.LongLength;
            if (result) { ReadNext(); }
            return result;
        }

        /// <inheritdoc />
        public void Reset()
        {
            currentindex = -1;
            heldposition = reader.originalstreampos + reader.headerblob.WrittenBytesCount;
        }

        /// <inheritdoc />
        public DictionaryEntry Entry => new(entry.Name, entry.Value);

        /// <summary>
        /// Returns the current read resource as a new <see cref="IResourceEntry"/>.
        /// </summary>
        public IResourceEntry ResourceEntry => entry;

        /// <inheritdoc />
        public object Key => entry.Name;

        /// <inheritdoc />
        public object Value => entry.Value;

        /// <inheritdoc />
        public object Current => Entry;
    }

    /// <summary>
    /// The <see cref="CustomBinaryResourceReader"/> class reads resources created from 
    /// the <see cref="CustomBinaryResourceWriter"/> class. <br />
    /// This format is the first custom for this library. <br />
    /// Currently , it reads both V1 and V2 resources. <br />
    /// This class cannot be inherited. <br />
    /// </summary>
    public sealed class CustomBinaryResourceReader : IDotNetResourcesExtensionsReader
    {
        internal System.IO.Stream targetstream;
        private System.Boolean isstreamowner , verified;
        internal ExtensibleFormatter exf;
        internal CustomBinaryHeaderBlob headerblob;
        internal System.Int64 originalstreampos;

        /// <summary>
        /// Gets or sets a value whether the class controls the lifetime of the underlying stream.
        /// </summary>
        public bool IsStreamOwner { get => isstreamowner; set => isstreamowner = value; }

        private CustomBinaryResourceReader() 
        {
            exf = new();
            targetstream = null;
            isstreamowner = false;
            verified = false;
            headerblob = null;
            originalstreampos = 0;
        }

        /// <summary>
        /// Creates a new instance of <see cref="CustomBinaryResourceReader"/> from the specified data stream.
        /// </summary>
        /// <param name="stream">The data stream to read resources from.</param>
        public CustomBinaryResourceReader(System.IO.Stream stream) : this()
        {
            targetstream = stream;
            isstreamowner = false;
            originalstreampos = targetstream.Position;
        }

        /// <summary>
        /// Creates a new instance of <see cref="CustomBinaryResourceReader"/> from the specified file.
        /// </summary>
        /// <param name="path">The path of the file to read. The file must have been written using the <see cref="CustomBinaryResourceWriter"/> class.</param>
        public CustomBinaryResourceReader(System.String path) : this()
        {
            targetstream = new System.IO.FileStream(path , System.IO.FileMode.Open);
            originalstreampos = targetstream.Position;
            isstreamowner = true;
        }

        private void VerifyHeader()
        {
            // We do not anymore need FindHeaderSizeProgrammatically.
            // The new reader handles such cases.
            CustomBinaryHeader CBH = CustomBinaryHeader.ReadFromStream(targetstream);
            headerblob = CBH.ReadHeader(targetstream.Position);
            CBH = null;
            verified = true;
        }

        /// <inheritdoc />
        public void Close() { if (isstreamowner) { targetstream.Close(); } headerblob = null; }

        /// <inheritdoc />
        public IDictionaryEnumerator GetEnumerator()
        {
            if (verified == false) { VerifyHeader(); }
            return new CustomBinaryResourceEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Specifies the count of resources found in this stream. Returns -1 if the reader has not yet begun reading.
        /// </summary>
        public System.Int64 Count
        {
            get {
                if (verified == false) { return -1; }
                return headerblob.DataPositions.LongLength;
            }
        }

        /// <summary>
        /// Disposes the <see cref="CustomBinaryResourceReader"/> instance.
        /// </summary>
        public void Dispose()
        {
            if (isstreamowner) { targetstream.Dispose(); }
            headerblob = null;
            exf?.Dispose();
            exf = null;
        }

        /// <inheritdoc />
        public void RegisterTypeResolver(ITypeResolver resolver)
        {
            exf.RegisterTypeResolver(resolver);
        }
    }
}
