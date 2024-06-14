
using System;
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Internal
{
    internal static class CustomBinaryResourcesConstants
    {
        public const System.String Magic = "MDCDI1315_BF_1";
        public const System.String HeaderSize = "HEADERSIZE";
        public const System.String Version = "VERSION";
        public const System.String SupportedFormatsMask = "CURRENTFORMATS";
        public const System.String SupportedHeaderVersion = "SUPPORTEDHEADERVERSION";
        public const System.String DataAlignmentHeader = "DATAPOSALIGNMENT";
        public const System.UInt16 DataPositionsAlignment = 8;
        public const System.String NextDataPositions = "DATAPOSITIONS";
        public const System.String EndHeaderOrValue = "\u0007\n";
        public const System.String ResourceHeader = "RESOURCE\u0004";
        public const System.String ResourceName = "RESOURCENAME";
        public const System.String HeaderVersion = "HEADERVERSION";
        public const System.String DotNetType = "DOTNETTYPE";
        public const System.String ResourceSize = "RESOURCESIZE";
        public const System.String ResourceType = "RESOURCETYPE";
        public const System.String ResourceValue = "VALUE";
        public const System.UInt16 CurrentHeaderVersion = 1;
        public const System.UInt16 CurrentFormatVersion = 1;
        public const System.Byte HeaderResourcesPartsSeperator = 0;
        public const System.Byte ValueStart = (System.Byte)ValueStartChar;
        public const System.Char ValueStartChar = '=';
        public const BinaryRESTypes CurrentSupportedFormats = BinaryRESTypes.Object;
    }

    internal sealed class BinaryHeaderReaderWriter
    {
        public List<System.Byte> Result;
        private System.Text.Encoding encoder;
        private System.Int64 headerlength;
        private System.UInt16 headerversion;
        private System.UInt16 headerformatsmask;
        private System.UInt16 supportedheaderversion;
        private System.Byte datalignment;
        private System.Int64[] datpositions;

        public BinaryHeaderReaderWriter()
        {
            encoder = System.Text.Encoding.UTF8;
        }
        
        /// <summary>
        /// Creates a new <see cref="BinaryHeaderReaderWriter"/> class from a typed byte array. <br />
        /// Apparently this is unsafe and quite dangerous.
        /// </summary>
        /// <param name="data">The byte array to get data from.</param>
        /// <returns>A new <see cref="BinaryHeaderReaderWriter"/> class.</returns>
        /// <exception cref="Exception"></exception>
        public static BinaryHeaderReaderWriter GetFromArray(System.Byte[] data)
        {
            BinaryHeaderReaderWriter result = new();
            System.Int64 pos = 0;
            System.String mgc = System.String.Empty;
            for (System.Int64 I = pos; I < CustomBinaryResourcesConstants.Magic.Length;I++) { mgc += (System.Char)data[I]; }
            if (mgc != CustomBinaryResourcesConstants.Magic) { throw new Exception("Invalid Header"); }
            // Update pos
            pos += mgc.Length + 2;
            (System.String, System.Byte[] , System.Int64) values = (System.String.Empty , null , 0);
            while (pos < data.LongLength)
            {
                values = result.GetNextValue(data , pos);
                pos += values.Item3;
                if (values.Item1 == CustomBinaryResourcesConstants.Version)
                {
                    result.headerversion = (System.UInt16)System.BitConverter.ToInt64(values.Item2, 0);
                } else if (values.Item1 == CustomBinaryResourcesConstants.SupportedFormatsMask)
                {
                    result.headerformatsmask = (System.UInt16)System.BitConverter.ToInt64(values.Item2, 0);
                } else if (values.Item1 == CustomBinaryResourcesConstants.SupportedHeaderVersion)
                {
                    result.supportedheaderversion = (System.UInt16)System.BitConverter.ToInt64(values.Item2, 0);
                } else if (values.Item1 == CustomBinaryResourcesConstants.DataAlignmentHeader)
                {
                    result.datalignment = (System.Byte)System.BitConverter.ToInt64(values.Item2, 0);
                } else if (values.Item1 == CustomBinaryResourcesConstants.NextDataPositions)
                {
                    System.Int64[] ret = new System.Int64[(values.Item2.LongLength / result.datalignment)];
                    for (System.Int32 I = 0 , K = 0; I < ret.Length; I++)
                    { 
                        ret[I] = System.BitConverter.ToInt64(values.Item2, K);
                        K += 8;
                    }
                    result.datpositions = ret;
                    pos -= values.Item2.LongLength - 8;
                    break;
                } else { break; }
            }
            result.headerlength = pos;
            return result;
        }

        private (System.String , System.Byte[] , System.Int64) GetNextValue(System.Byte[] dat , System.Int64 offset)
        {
            System.String r1 = System.String.Empty;
            System.Byte[] r2;
            System.Int64 bc;
            System.Int64 I = offset , count = 0;
            while ((I + 1) < dat.LongLength)
            {
                if (dat[I] == 7 && dat[I + 1] == 10) { break; }
                count++; I++; 
            }
            bc = count + 2;
            System.Int64 poseqsign = offset;
            while (dat[poseqsign] != CustomBinaryResourcesConstants.ValueStart) { poseqsign++; }
            for (System.Int64 J = offset; J < poseqsign;J++) { r1 += (System.Char)dat[J]; }
            bc -= r1.Length + 3;
            r2 = new System.Byte[bc];
            // Was: poseqsign + 1 + bc.
            for (System.Int64 J = poseqsign + 1, K = 0; J < poseqsign + bc + 1;J++) { r2[K] = dat[J];  K++; }
            System.Diagnostics.Debug.WriteLine($"[BINARYHEADERREADER]: Got Header Value ({r2.LongLength} bytes)");
            bc += r2.LongLength + r1.Length - 5;
            return (r1, r2 , bc);
        }

        private void EncodeStringAndAdd(System.String txt) => Result.AddRange(encoder.GetBytes(txt));

        private void WriteStringValue(System.String headername, System.String val)
        {
            EncodeStringAndAdd(headername);
            Result.Add(CustomBinaryResourcesConstants.ValueStart);
            EncodeStringAndAdd(val);
            EncodeStringAndAdd(CustomBinaryResourcesConstants.EndHeaderOrValue);
        }

        private void WriteNumericValue(System.String headername, System.Int64 value)
        {
            EncodeStringAndAdd(headername);
            Result.Add(CustomBinaryResourcesConstants.ValueStart);
            Result.AddRange(System.BitConverter.GetBytes(value));
            EncodeStringAndAdd(CustomBinaryResourcesConstants.EndHeaderOrValue);
        }

        public void CreateNew(params System.Int64[] datapositions)
        {
            Result = new();
            EncodeStringAndAdd(CustomBinaryResourcesConstants.Magic);
            EncodeStringAndAdd(CustomBinaryResourcesConstants.EndHeaderOrValue);
            WriteNumericValue(CustomBinaryResourcesConstants.Version, CustomBinaryResourcesConstants.CurrentFormatVersion);
            WriteNumericValue(CustomBinaryResourcesConstants.SupportedFormatsMask, 
                (System.Int64)CustomBinaryResourcesConstants.CurrentSupportedFormats);
            WriteNumericValue(CustomBinaryResourcesConstants.SupportedHeaderVersion, 
                CustomBinaryResourcesConstants.CurrentHeaderVersion);
            WriteNumericValue(CustomBinaryResourcesConstants.DataAlignmentHeader , CustomBinaryResourcesConstants.DataPositionsAlignment);
            EncodeStringAndAdd(CustomBinaryResourcesConstants.NextDataPositions);
            Result.Add(CustomBinaryResourcesConstants.ValueStart);
            for (System.Int64 I = 0; I < datapositions.LongLength;I++) { Result.AddRange(System.BitConverter.GetBytes(datapositions[I])); }
            EncodeStringAndAdd(CustomBinaryResourcesConstants.EndHeaderOrValue);
            headerlength = Result.Count;
        }

        public System.Int64 HeaderLength { get => headerlength; }

        public System.UInt16 HeaderVersion { get => headerversion; }

        public BinaryRESTypes HeaderFormatsMask { get => (BinaryRESTypes)headerformatsmask; }

        public System.UInt16 SupportedResourceHeaderVersion { get => supportedheaderversion; }

        public System.Int64[] NextDataPositions { get => datpositions; }

    }

    internal enum BinaryRESTypes : System.Byte { String , ByteArray , Object }

    // Internal utility classes. Do NOT USE.
    internal record class RawEncodedData
    {
        public System.String HeaderName;
        public System.String HeaderValueAsString;
        public System.Byte[] DataAsBytes;
        public System.Int64 UpdatedPosition;

        public System.Int64 GetNumericValueIfPossible()
         => System.BitConverter.ToInt64(DataAsBytes, 0);
    }

    internal record class ExpectsValueBlock
    {
        public System.Int64 computedlength;
        public BinaryRESTypes typetoread;
        public System.Boolean expects;
    }

    /// <summary>
    /// The <see cref="BinaryResourceRepresentation"/> class represents a binary resource in this custom format.
    /// </summary>
    internal sealed class BinaryResourceRepresentation
    {
        /// <summary>
        /// This field is used only when generating resources.
        /// </summary>
        public List<System.Byte> FinalBytes;
        private System.Text.Encoding encoder;
        private System.UInt16 headerversion;
        private System.Int64 nextdatapos;
        private System.String name;
        private System.Byte[] data;
        private System.Type restype;
        private BinaryRESTypes binrestype;
        private System.Boolean generated;

        public BinaryResourceRepresentation() 
        { 
            FinalBytes = new(); encoder = System.Text.Encoding.UTF8; 
            headerversion = 0;
            name = null;
            data = null;
            restype = null;
            binrestype = BinaryRESTypes.String;
            nextdatapos = 0;
            generated = false;
        }

        public BinaryResourceRepresentation(System.String Name , System.Byte[] data, Type restype , BinaryRESTypes bin) : this()
        {
            name = Name;
            this.data = data;
            this.restype = restype;
            binrestype = bin;
            generated = false;
            Generate();
        }

        /// <summary>
        /// Gets a <see cref="BinaryResourceRepresentation"/> from plain bytes. This is unsafe and should be done with 
        /// respect to the array bounds.
        /// </summary>
        /// <param name="data">The data to read.</param>
        /// <param name="offset">The data offset to start reading from.</param>
        /// <returns>A new <see cref="BinaryResourceRepresentation"/> class.</returns>
        /// <exception cref="Exception"></exception>
        public static BinaryResourceRepresentation GetFromBytes(System.Byte[] data , System.Int64 offset)
        {
            BinaryResourceRepresentation brp = new();

            System.Int64 pos = offset;

            // First  , to ensure valid header, we must read the ResourceHeader exactly.
            System.Byte[] temp = new System.Byte[CustomBinaryResourcesConstants.ResourceHeader.Length];
            for (System.Int64 I = pos , K = 0; I < pos + temp.Length; I++) { temp[K] = data[I]; K++; }

            System.String tmp2 = brp.encoder.GetString(temp);

            if (tmp2 != CustomBinaryResourcesConstants.ResourceHeader) { throw new FormatException($"The Resource string was incorrectly got. Data: \'{tmp2}\'"); }
            brp.name = tmp2;

            // Update pos appropriately
            pos += tmp2.Length;

            // Next is to read all the resource values.
            List<RawEncodedData> red = new();
            ExpectsValueBlock evb = new() { computedlength = 0 , expects = false };
            while (pos < data.LongLength)
            {
                RawEncodedData di = brp.GetRawSequence(data, pos , evb);
                red.Add(di);
                pos = di.UpdatedPosition;
                if (di.HeaderName == CustomBinaryResourcesConstants.ResourceSize) 
                { 
                    evb.expects = true;
                    evb.typetoread = (BinaryRESTypes)red.Find((RawEncodedData e) => { return e.HeaderName == CustomBinaryResourcesConstants.ResourceType; }).GetNumericValueIfPossible();
                    // Get computed length from bitconverter.
                    evb.computedlength = di.GetNumericValueIfPossible();
                }
                if (di.HeaderName == CustomBinaryResourcesConstants.ResourceValue) { break; }
            }
            // From our raw encoded data , bring them all back to the BinaryRepresentation class.
            foreach (RawEncodedData e in red)
            {
                if (e.HeaderName == CustomBinaryResourcesConstants.ResourceName)
                {
                    brp.name = e.HeaderValueAsString;
                }
                if (e.HeaderName == CustomBinaryResourcesConstants.DotNetType)
                {
                    brp.restype = System.Type.GetType(e.HeaderValueAsString);
                }
                if (e.HeaderName == CustomBinaryResourcesConstants.ResourceType)
                {
                    brp.binrestype = (BinaryRESTypes)e.GetNumericValueIfPossible();
                }
                if (e.HeaderName == CustomBinaryResourcesConstants.HeaderVersion)
                {
                    brp.headerversion = (System.UInt16)e.GetNumericValueIfPossible();
                }
                if (e.HeaderName == CustomBinaryResourcesConstants.ResourceValue)
                {
                    brp.data = e.DataAsBytes;
                }
            }
            // Clear our data , we will not need them anymore
            red.Clear();
            red = null;
            brp.nextdatapos = pos;
            brp.generated = true;
            return brp;
        }

        private RawEncodedData GetRawValueSequence(System.Byte[] dat , in System.Int64 pos1 , ExpectsValueBlock evb)
        {
            System.Int64 pos = pos1;
            RawEncodedData raw = new();
            // Find now from the bytearray the equal sign and use it so as to break our data.
            System.Int64 poseqsign = pos + 1;
            while (dat[poseqsign] != CustomBinaryResourcesConstants.ValueStart) { poseqsign++; }
            // We found the equal sign.
            // Read until there the VALUE header and save it.
            raw.HeaderName = System.String.Empty;
            for (System.Int64 I = pos; I < poseqsign; I++) { raw.HeaderName += (System.Char)dat[I]; }
            // Read our data. Our hard work has been done.
            System.Byte[] temp = new System.Byte[evb.computedlength];
            System.Int64 K = 0 , endpoint = poseqsign + 1 + temp.LongLength;
            for (System.Int64 I = poseqsign + 1; I < endpoint; I++) { temp[K] = dat[I]; K++; }
            if (evb.typetoread == BinaryRESTypes.String)
            {
                raw.HeaderValueAsString = encoder.GetString(temp);
            }
            raw.DataAsBytes = temp;
            raw.UpdatedPosition = poseqsign + temp.LongLength + 4;
            return raw;
        }

        private RawEncodedData GetRawSequence(System.Byte[] data , in System.Int64 pos1 , ExpectsValueBlock evb)
        {
            if (evb.expects) { return GetRawValueSequence(data , pos1 , evb); }
            System.Int64 pos = pos1;
            RawEncodedData raw = new();
            List<System.Byte> rawbytes = new();
            System.Int64 I = pos;
            while ((I + 1) < data.LongLength) 
            { 
                if (data[I] == 7 && data[I + 1] == 10) { break; }
                rawbytes.Add(data[I]); I++; 
            }
            // Update pos by the bytes found , plus two. 
            pos += rawbytes.Count + 2;
            // Get the encoded string.
            System.String rawstring = encoder.GetString(rawbytes.ToArray());
            raw.HeaderName = rawstring.Remove(rawstring.IndexOf(CustomBinaryResourcesConstants.ValueStartChar));
            raw.HeaderValueAsString = rawstring.Substring(rawstring.IndexOf(CustomBinaryResourcesConstants.ValueStartChar) + 1);
            // Find now from the bytearray the equal sign and use it so as to break our data.
            System.Int32 poseqsign = 0;
            while (rawbytes[poseqsign] != CustomBinaryResourcesConstants.ValueStart) { poseqsign++; }
            // poseqsign will return the actual position in the byte array , pad one so as to point to our array.
            poseqsign++;
            // Calculate array length
            System.Byte[] target = new System.Byte[rawbytes.Count - poseqsign];
            System.Int32 K = 0;
            for (System.Int32 J = poseqsign; J < rawbytes.Count;J++) { target[K] = rawbytes[J]; K++; }
            // Add the array to our result
            raw.DataAsBytes = target;
            // Add our new position
            raw.UpdatedPosition = pos;
            rawbytes.Clear();
            return raw;
        }

        private void EncodeStringAndAdd(System.String txt) =>
            FinalBytes.AddRange(encoder.GetBytes(txt));

        private void WriteStringValue(System.String headername, System.String val)
        {
            EncodeStringAndAdd(headername);
            FinalBytes.Add(CustomBinaryResourcesConstants.ValueStart);
            EncodeStringAndAdd(val);
            EncodeStringAndAdd(CustomBinaryResourcesConstants.EndHeaderOrValue);
        }

        private void WriteNumericValue(System.String headername, System.Int64 value)
        {
            EncodeStringAndAdd(headername);
            FinalBytes.Add(CustomBinaryResourcesConstants.ValueStart);
            FinalBytes.AddRange(System.BitConverter.GetBytes(value));
            EncodeStringAndAdd(CustomBinaryResourcesConstants.EndHeaderOrValue);
        }

        private void WriteRawData()
        {
            EncodeStringAndAdd(CustomBinaryResourcesConstants.ResourceValue);
            FinalBytes.Add(CustomBinaryResourcesConstants.ValueStart);
            FinalBytes.AddRange(data);
            EncodeStringAndAdd(CustomBinaryResourcesConstants.EndHeaderOrValue);
        }

        public System.String HeaderName { get => name; set => name = value; }

        public System.Byte[] RawData { get => data; set => data = value; }

        public System.UInt16 HeaderVersion { get => headerversion; set => headerversion = value; }

        public System.Int64 NextDataPosition { get => nextdatapos; }

        public System.Type ResourceType { get => restype; set => restype = value; }

        public BinaryRESTypes BinaryResourceType { get => binrestype; set => binrestype = value; }

        public void Generate()
        {
            if (generated) { return; }
            EncodeStringAndAdd(CustomBinaryResourcesConstants.ResourceHeader);
            WriteStringValue(CustomBinaryResourcesConstants.ResourceName, name);
            WriteNumericValue(CustomBinaryResourcesConstants.HeaderVersion, 
                CustomBinaryResourcesConstants.CurrentHeaderVersion);
            WriteStringValue(CustomBinaryResourcesConstants.DotNetType, restype.AssemblyQualifiedName);
            WriteNumericValue(CustomBinaryResourcesConstants.ResourceType, (System.Int64)binrestype);
            WriteNumericValue(CustomBinaryResourcesConstants.ResourceSize, data.LongLength);
            WriteRawData();
            generated = true;
        }

    }
}
