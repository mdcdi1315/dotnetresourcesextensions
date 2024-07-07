
using System;

namespace DotNetResourcesExtensions
{
    using Internal;
    using System.Collections;
    using Internal.CustomFormatter;

    /// <summary>
    /// This enumerator is used only with the <see cref="CustomBinaryResourceReader"/> class. <br />
    /// You cannot create an instance of this class. Instead , to get an instance you must use the 
    /// <see cref="CustomBinaryResourceReader.GetEnumerator()"/> method.
    /// </summary>
    public sealed class CustomBinaryResourceEnumerator : IDictionaryEnumerator
    {
        private CustomBinaryResourceReader reader;
        private System.Int64 elements, currentindex;
        private System.Int64 heldposition , heldpos;
        private System.Boolean gotres;
        private DictionaryEntry entry;
        private System.Byte[] temp;

        internal CustomBinaryResourceEnumerator(CustomBinaryResourceReader reader)
        {
            this.reader = reader;
            elements = this.reader.indices.LongLength;
            currentindex = -1;
            temp = null;
            gotres = false;
            heldpos = 0;
            heldposition = this.reader.resourcesposition;
        }

        private void ReadNext()
        {
            System.Int64 len;
            if (currentindex == 0) // First initialisation
            {
                heldpos = reader.indices[currentindex] + reader.resourcesposition + 1;
                reader.targetstream.Position = heldposition;
                len = heldpos - heldposition;
            } else if (currentindex + 1 == elements) // Last resource alignment
            {
                heldpos = reader.indices[currentindex] + heldposition - 1;
                reader.targetstream.Position = heldposition - 1;
                len = heldpos - heldposition;
            } else // All other resources
            {
                heldpos = reader.indices[currentindex] + heldposition;
                reader.targetstream.Position = heldposition - 1;
                len = heldpos - heldposition;
            }
            temp = ParserHelpers.ReadBuffered(reader.targetstream, len);
            heldposition = heldpos;
        }

        private DictionaryEntry GetResource()
        {
            if (currentindex >= elements) { throw new IndexOutOfRangeException("The enumerator has no more elements to iterate."); }
            BinaryResourceRepresentation brp = BinaryResourceRepresentation.GetFromBytes(temp , 0);
            temp = null;
            DictionaryEntry result = new();
            result.Key = brp.HeaderName;
            if (brp.BinaryResourceType == BinaryRESTypes.String)
            {
                result.Value = System.Text.Encoding.UTF8.GetString(brp.RawData);
            } else if (brp.BinaryResourceType == BinaryRESTypes.ByteArray)
            {
                result.Value = brp.RawData;
            } else if (brp.BinaryResourceType == BinaryRESTypes.Object)
            {
                result.Value = reader.exf.GetObjectFromBytes(brp.RawData , brp.ResourceType);
            }
            return result;
        }

        /// <inheritdoc />
        public System.Boolean MoveNext()
        {
            gotres = false;
            currentindex++;
            System.Boolean result = currentindex < elements;
            if (result) { ReadNext(); }
            return result;
        }

        /// <inheritdoc />
        public void Reset()
        {
            currentindex = -1;
            heldposition = reader.resourcesposition;
            heldpos = 0;
            reader.targetstream.Position = 0;
        }

        /// <inheritdoc />
        public DictionaryEntry Entry
        {
            get {
                if (gotres) { return entry; }
                gotres = true;
                entry = GetResource();
                return entry;
            }
        }

        /// <inheritdoc />
        public object Key => Entry.Key;

        /// <inheritdoc />
        public object Value => Entry.Value;

        /// <inheritdoc />
        public object Current => Entry;
    }

    /// <summary>
    /// The <see cref="CustomBinaryResourceReader"/> class reads resources created from 
    /// the <see cref="CustomBinaryResourceWriter"/> class. This format is the first custom for this library. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class CustomBinaryResourceReader : IDotNetResourcesExtensionsReader
    {
        internal System.IO.Stream targetstream;
        private System.Boolean isstreamowner , verified;
        internal ExtensibleFormatter exf;
        internal System.Int64 resourcesposition;
        internal BinaryRESTypes supportedresmask;
        internal System.UInt16 supportedheaderversion;
        internal System.UInt16 writeversion;
        internal System.Int64[] indices;

        /// <summary>
        /// Gets or sets a value whether the class controls the lifetime of the underlying stream.
        /// </summary>
        public bool IsStreamOwner { get => isstreamowner; set => isstreamowner = value; }

        private CustomBinaryResourceReader() 
        {
            exf = new();
            resourcesposition = 0;
            targetstream = null;
            isstreamowner = false;
            verified = false;
            writeversion = 0;
            indices = null;
            supportedheaderversion = 0;
            supportedresmask = BinaryRESTypes.String;
        }

        /// <summary>
        /// Creates a new instance of <see cref="CustomBinaryResourceReader"/> from the specified data stream.
        /// </summary>
        /// <param name="stream">The data stream to read resources from.</param>
        public CustomBinaryResourceReader(System.IO.Stream stream) : this()
        {
            targetstream = stream;
            targetstream.Position = 0;
            isstreamowner = false;
        }

        /// <summary>
        /// Creates a new instance of <see cref="CustomBinaryResourceReader"/> from the specified file.
        /// </summary>
        /// <param name="path">The path of the file to read. The file must have been written using the <see cref="CustomBinaryResourceWriter"/> class.</param>
        public CustomBinaryResourceReader(System.String path) : this()
        {
            targetstream = new System.IO.FileStream(path , System.IO.FileMode.Open);
            isstreamowner = true;
        }

        private void VerifyHeader()
        {
            System.Byte[] all;
            // The reader will use the FindHeaderSizeProgrammatically method which will efficiently find the array length and push it to the 'all' array.
            targetstream.Position = 0;
            System.Int64 count = FindHeaderSizeProgrammatically();
            if (count == -1) { throw new FormatException("Invalid header detected. This is not the custom resource format."); }
            all = new System.Byte[count];
            resourcesposition = count;
            System.Int32 reqcount = all.Length , fcount = 0;
            while (fcount != reqcount)
            {
                targetstream.Position = 0;
                fcount = targetstream.Read(all, 0, all.Length);
                if (fcount == 0) { break; }
                if (fcount < reqcount) { fcount = 0; break; }
            }
            if (fcount == 0) { throw new System.IO.EndOfStreamException("Unexpected end of stream detected"); }
            targetstream.Position = 0;
            BinaryHeaderReaderWriter bw = null;
            try
            {
                 bw = BinaryHeaderReaderWriter.GetFromArray(all);
            } catch (System.Exception e) { if (e.Message == "Invalid Header") { throw new FormatException("This is not the custom resources format."); } throw e; }
            all = null;
            indices = bw.NextDataPositions;
            supportedresmask = bw.HeaderFormatsMask;
            supportedheaderversion = bw.SupportedResourceHeaderVersion;
            writeversion = bw.HeaderVersion;
            verified = true;
        }

        // Finds the header size by searching 500 bytes each time.
        // The header size is the size before the first RESOURCE occurs.
        private System.Int64 FindHeaderSizeProgrammatically()
        {
            System.Byte[] constant = System.Text.Encoding.UTF8.GetBytes(
                CustomBinaryResourcesConstants.ResourceHeader);
            System.Byte[] temp = new System.Byte[500];
            System.Int64 pos = 0;
            System.Int32 readbytes;
            while (pos < targetstream.Length && (readbytes = targetstream.Read(temp , 0 , temp.Length)) != 0)
            {
                // Find \u0004 first.
                System.Int32 I = 0;
                System.Boolean stop;
                while ((stop = I < temp.Length) && (System.Char)temp[I] != '\u0004') { I++; }
                if (stop)
                {
                    // \u0004 was found at least once. Check if before this one the wanted string exists.
                    // Check first if so many bytes pointed by the constant can be allocated.
                    if (I - 1 >= CustomBinaryResourcesConstants.ResourceHeader.Length)
                    {
                        // Such a size can be allocated , continue.
                        System.Boolean match = true;
                        System.Int32 K = 0;
                        for (System.Int32 J = I - CustomBinaryResourcesConstants.ResourceHeader.Length + 1; 
                            J <= I && K < constant.Length;J++)
                        {
                            // If a value is different than the constant's then the original assumption is broken.
                            // The loop stops finding and the outer loop pefrorms all these operations again.
                            if (constant[K] != temp[J]) { match = false; break; }
                            K++;
                        }
                        if (match) { return I - CustomBinaryResourcesConstants.ResourceHeader.Length + 1; }
                    }
                }
                pos += readbytes;
            }
            return -1;
        }

        /// <inheritdoc />
        public void Close() { if (isstreamowner) { targetstream.Close(); } }

        /// <inheritdoc />
        public IDictionaryEnumerator GetEnumerator()
        {
            if (verified == false) { VerifyHeader(); }
            return new CustomBinaryResourceEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Disposes the <see cref="CustomBinaryResourceReader"/> instance.
        /// </summary>
        public void Dispose()
        {
            if (isstreamowner) { targetstream.Dispose(); }
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
