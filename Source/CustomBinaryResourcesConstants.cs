
using System;
using System.Collections.Generic;
using DotNetResourcesExtensions.Internal.CustomFormatter;

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
        public const System.String DataPositionsCount = "DATAPOSITIONSCOUNT";
        public const System.String BaseDataPosition = "BASEDATAPOSITION";
        public const System.String EndHeaderOrValue = "\u0007\n";
        public const System.String ResourceHeader = "RESOURCE\u0004";
        public const System.String ResourceName = "RESOURCENAME";
        public const System.String HeaderVersion = "HEADERVERSION";
        public const System.String DotNetType = "DOTNETTYPE";
        public const System.String ResourceSize = "RESOURCESIZE";
        public const System.String ResourceType = "RESOURCETYPE";
        public const System.String ResourceValue = "VALUE";
        public const System.UInt16 CurrentHeaderVersion = 2;
        public const System.UInt16 CurrentFormatVersion = 2;
        public const System.Byte HeaderResourcesPartsSeperator = 0;
        public const System.Byte ValueStart = (System.Byte)ValueStartChar;
        public const System.Char ValueStartChar = '=';
        public const BinaryRESTypes CurrentSupportedFormats = BinaryRESTypes.Object;
    }

    // Re-implementation of BinaryResourceRepresentation as Version 2.
    // Can also read V1 resources.
    // It is more simple and code-performant and worked when tested at once (serialized and deserialized the resource successfully).
    internal sealed class CustomBinaryResource
    {
        private System.IO.Stream rwstream;
        private System.Text.Encoding encoding;

        private CustomBinaryResource() { encoding = System.Text.Encoding.UTF8; }

        public static CustomBinaryResource WriteToStream(System.IO.Stream str)
        {
            if (str.CanWrite == false) { throw new ArgumentException(Properties.Resources.DNTRESEXT_STRCannotBeUsed_Write); }
            CustomBinaryResource ret = new();
            ret.rwstream = str;
            return ret;
        }

        public static CustomBinaryResource ReadFromStream(System.IO.Stream str)
        {
            if (str.CanRead == false) { throw new ArgumentException(Properties.Resources.DNTRESEXT_STRCannotBeUsed_Read); }
            CustomBinaryResource ret = new();
            ret.rwstream = str;
            return ret;
        }

        private void WriteString(System.String str)
        {
            System.Byte[] dt = encoding.GetBytes(str);
            rwstream.Write(dt , 0 , dt.Length);
            dt = null;
        }

        private void WriteEndHeaderValue() => WriteString(CustomBinaryResourcesConstants.EndHeaderOrValue);

        private void WriteValueStart() => rwstream.WriteByte(CustomBinaryResourcesConstants.ValueStart);

        private void WriteNumericValue(System.Int64 val)
        {
            System.Byte[] f = val.GetBytes();
            rwstream.Write(f , 0 , f.Length);
            f = null;
        }

        public System.Int64 WriteResource(BinResourceBlob blob , CustomFormatter.ICustomFormatter fmt)
        {
            if (rwstream.CanWrite == false) { throw new ArgumentException(Properties.Resources.DNTRESEXT_StreamUnwriteable); }
            System.Int64 sposition = rwstream.Position;
            WriteString(CustomBinaryResourcesConstants.ResourceHeader);
            WriteString($"{CustomBinaryResourcesConstants.ResourceName}{CustomBinaryResourcesConstants.ValueStartChar}{blob.Name}");
            WriteEndHeaderValue();
            WriteString(CustomBinaryResourcesConstants.HeaderVersion);
            WriteValueStart();
            WriteNumericValue(CustomBinaryResourcesConstants.CurrentHeaderVersion);
            WriteEndHeaderValue();
            WriteString(CustomBinaryResourcesConstants.DotNetType);
            WriteValueStart();
            WriteString(blob.TypeOfValue.AssemblyQualifiedName);
            WriteEndHeaderValue();
            BinaryRESTypes bres = BinaryRESTypes.String;
            switch (blob.TypeOfValue.FullName)
            {
                case "System.String":
                    break;
                case "System.Byte[]":
                    bres = BinaryRESTypes.ByteArray;
                    break;
                default:
                    bres = BinaryRESTypes.Object;
                    break;
            }
            WriteString(CustomBinaryResourcesConstants.ResourceType);
            WriteValueStart();
            WriteNumericValue((System.Int64)bres);
            WriteEndHeaderValue();
            System.Byte[] data = null;
            switch (bres)
            {
                case BinaryRESTypes.String:
                    data = encoding.GetBytes(blob.Value.ToString());
                    break;
                case BinaryRESTypes.ByteArray:
                    data = (System.Byte[])blob.Value;
                    break;
                case BinaryRESTypes.Object:
                    data = fmt.GetBytesFromObject(blob.Value);
                    break;
            }
            WriteString(CustomBinaryResourcesConstants.ResourceSize);
            WriteValueStart();
            WriteNumericValue(data.LongLength);
            WriteEndHeaderValue();
            WriteString(CustomBinaryResourcesConstants.ResourceValue);
            WriteValueStart();
            ParserHelpers.WriteBuffered(rwstream, data);
            WriteEndHeaderValue();
            // Easy to compute here how many bytes were written.
            return rwstream.Position - sposition;
        }
    
        private System.Int64 ReadNextNumericValue()
        {
            System.Byte[] d = new System.Byte[8];
            rwstream.Read(d, 0, d.Length);
            if (EnsureHeaderEnd() == false) { throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_CORRUPTED, ParserErrorType.Deserialization); }
            return d.ToInt64(0);
        }

        private System.Boolean StringMatchesThis(System.String str)
        {
            System.Byte[] dt = encoding.GetBytes(str) ,dg = new System.Byte[dt.LongLength];
            rwstream.Read(dg, 0, dg.Length);
            System.Boolean val = true; // Presuming that the strings do originally match
            for (System.Int64 I = 0; I < dt.LongLength; I++)
            {
                if (dg[I] != dt[I]) { val = false; break; } // If not break the loop and return false.
            }
            dg = null; dt = null;
            return val;
        }

        private System.Byte[] ReadUntilHeaderEnd()
        {
            System.Int64 origin = rwstream.Position , final = -1;
            System.Byte[] Header = encoding.GetBytes(CustomBinaryResourcesConstants.EndHeaderOrValue);
            System.Int32 rb;
            while ((rb = rwstream.ReadByte()) > -1)
            {
                // I prefer here both sides of AND to be checked.
                // If I was using && it would needed more conditions to check.
                // Keeping it this way is very fast , simple & performant.
                if (rb == Header[0] & rwstream.ReadByte() == Header[1])
                {
                    final = rwstream.Position - 2;
                    break;
                }
                rwstream.Position--;
            }
            if (final == -1) { throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_HEADER_END, ParserErrorType.Deserialization); }
            System.Byte[] ret = new System.Byte[final - origin];
            rwstream.Position = origin;
            rwstream.Read(ret, 0, ret.Length);
            if (EnsureHeaderEnd() == false) { throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_CORRUPTED, ParserErrorType.Deserialization); }
            return ret;
        }

        private System.String ReadStringUntilHeaderEnd() => encoding.GetString(ReadUntilHeaderEnd());

        private System.Boolean EnsureHeaderEnd()
        {
            System.Byte[] hdr = encoding.GetBytes(CustomBinaryResourcesConstants.EndHeaderOrValue) , dr = new System.Byte[hdr.LongLength];
            rwstream.Read(dr , 0 , dr.Length);
            System.Boolean val = true;
            for (System.Int64 I = 0; I < dr.LongLength; I++)
            {
                if (hdr[I] != dr[I]) { val = false; break; }
            }
            hdr = null; dr = null;
            return val;
        }

        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The created data might be data coming outside from DotNetResourcesExtensions project and the project might have them unavailable while reading them.")]
        public BinResourceBlob ReadResource(System.Int64 position , CustomFormatter.ICustomFormatter fmt)
        {
            // Create the blob.
            BinResourceBlob BR = new();
            rwstream.Position = position;
            // BEGIN RESOURCE READ
            if (StringMatchesThis(CustomBinaryResourcesConstants.ResourceHeader) == false)
            {
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_RESOURCE_HEADER, ParserErrorType.Deserialization);
            }
            if (StringMatchesThis($"{CustomBinaryResourcesConstants.ResourceName}{CustomBinaryResourcesConstants.ValueStartChar}") == false)
            {
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_RESOURCENAME_HEADER, ParserErrorType.Deserialization);
            }
            // Read the resource name.
            BR.FillNameStringFromArray(ReadUntilHeaderEnd(), encoding);
            // Read the header version.
            if (StringMatchesThis($"{CustomBinaryResourcesConstants.HeaderVersion}{CustomBinaryResourcesConstants.ValueStartChar}") == false)
            {
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_HEADERVERSION_HEADER, ParserErrorType.Deserialization);
            }
            // Read the numeric version value and directly compare if it is correct.
            System.Int64 v;
            if ((v = ReadNextNumericValue()) > CustomBinaryResourcesConstants.CurrentHeaderVersion)
            {
                // Version is not 1 or 2 , fail.
                // Be noted , no factual changes performed in V2 , the version was just incremented for safety and 
                // due to the fact that this is a completely new reading method.
                // Note that the writer always writes the resource as V2 anymore.
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_VER_MISMATCH, ParserErrorType.Versioning);
            }
            // Read the .NET type of this resource. Will be needed if our resource is an object , which is required by our formatter...
            if (StringMatchesThis($"{CustomBinaryResourcesConstants.DotNetType}{CustomBinaryResourcesConstants.ValueStartChar}") == false)
            {
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_DOTNETTYPE_HEADER, ParserErrorType.Deserialization);
            }
            // Get the type directly. Will fail as needed.
            System.Type type = System.Type.GetType(ReadStringUntilHeaderEnd(), true, false);
            // Resource type.
            if (StringMatchesThis($"{CustomBinaryResourcesConstants.ResourceType}{CustomBinaryResourcesConstants.ValueStartChar}") == false)
            {
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_RESOURCETYPE_HEADER, ParserErrorType.Deserialization);
            }
            // Read as numeric value and save it.
            BR.BinResType = (BinaryRESTypes)ReadNextNumericValue();
            // Read the byte array size , read it then save it to the blob.
            if (StringMatchesThis($"{CustomBinaryResourcesConstants.ResourceSize}{CustomBinaryResourcesConstants.ValueStartChar}") == false)
            {
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_RESOURCESIZE_HEADER, ParserErrorType.Deserialization);
            }
            // Read array size , then read the whole byte array.
            v = ReadNextNumericValue(); 
            if (StringMatchesThis($"{CustomBinaryResourcesConstants.ResourceValue}{CustomBinaryResourcesConstants.ValueStartChar}") == false)
            {
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_VALUE_HEADER, ParserErrorType.Deserialization);
            }
            // Read it with classic buffered method.
            System.Byte[] arb = ParserHelpers.ReadBuffered(rwstream, v);
            // Ensure correct header value termination.
            if (EnsureHeaderEnd() == false) { throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_CORRUPTED, ParserErrorType.Deserialization); }
            // switch through binary resource types
            switch (BR.BinResType)
            {
                case BinaryRESTypes.String:
                    BR.Value = encoding.GetString(arb);
                    break;
                case BinaryRESTypes.ByteArray:
                    BR.Value = arb;
                    break;
                case BinaryRESTypes.Object:
                    BR.Value = fmt.GetObjectFromBytes(arb , type);
                    break;
            }
            // NOTE: I am only afraid of ByteArray case. If you have a null array , the issue is on the above switch statement.
            // To fix it just copy the byte array to a new one , or null it only at Object and String cases.
            arb = null; // Delete the array ref from here
            // Return the bytes read by this reader. Easy to compute at this point.
            BR.BytesRead = rwstream.Position - position;
            // Return the object. Read successfull.
            return BR;
        }
    }

    // Specifies a binary resource entry. I call it 'Resource Blob'.
    internal record class BinResourceBlob : IResourceEntry
    {
        private System.String name;
        private System.Object Data;
        // The binary resource type read. 
        // Will be used by the reader to determine whether the entry must be accepted or not.
        public BinaryRESTypes BinResType;
        // The total bytes read , including header and the resource itself.
        // Will be used by the new reader.
        public System.Int64 BytesRead;

        public System.String Name { get => name; set => name = value; }

        public System.Object Value { get => Data; set => Data = value; }

        public System.Type TypeOfValue => Data?.GetType();

        internal void FillNameStringFromArray(System.Byte[] dt , System.Text.Encoding e) => name = e.GetString(dt);
    }

    // Re-implementation of BinaryHeaderReaderWriter as Version 2.
    // Can also read the V1 header too.
    // It is more simple and code-performant and worked when tested at once (serialized and deserialized the header , even a V1 one deserialized successfully).
    internal sealed class CustomBinaryHeader
    {
        private System.IO.Stream rwstream;
        private System.Text.Encoding encoding;

        private CustomBinaryHeader() { encoding = System.Text.Encoding.UTF8; }

        public static CustomBinaryHeader ReadFromStream(System.IO.Stream str)
        {
            if (str.CanRead == false) { throw new ArgumentException(Properties.Resources.DNTRESEXT_STRCannotBeUsed_Read); }
            CustomBinaryHeader ret = new();
            ret.rwstream = str;
            return ret;
        }

        public static CustomBinaryHeader WriteToStream(System.IO.Stream str)
        {
            if (str.CanWrite == false) { throw new ArgumentException(Properties.Resources.DNTRESEXT_STRCannotBeUsed_Write); }
            CustomBinaryHeader ret = new();
            ret.rwstream = str;
            return ret;
        }

        private void WriteString(System.String str)
        {
            System.Byte[] dt = encoding.GetBytes(str);
            rwstream.Write(dt, 0, dt.Length);
            dt = null;
        }

        private void WriteEndHeaderValue() => WriteString(CustomBinaryResourcesConstants.EndHeaderOrValue);

        private void WriteValueStart() => rwstream.WriteByte(CustomBinaryResourcesConstants.ValueStart);

        private void WriteNumericValue(System.Int64 val)
        {
            System.Byte[] f = val.GetBytes();
            rwstream.Write(f, 0, f.Length);
            f = null;
        }

        private System.Int64 ReadNextNumericValue()
        {
            System.Byte[] d = new System.Byte[8];
            rwstream.Read(d, 0, d.Length);
            if (EnsureHeaderEnd() == false) { throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_RESHEADER_CORRUPTED, ParserErrorType.Deserialization); }
            return d.ToInt64(0);
        }

        private System.Boolean StringMatchesThis(System.String str)
        {
            System.Byte[] dt = encoding.GetBytes(str), dg = new System.Byte[dt.LongLength];
            rwstream.Read(dg, 0, dg.Length);
            System.Boolean val = true; // Presuming that the strings do originally match
            for (System.Int64 I = 0; I < dt.LongLength; I++)
            {
                if (dg[I] != dt[I]) { val = false; break; } // If not break the loop and return false.
            }
            dg = null; dt = null;
            return val;
        }

        private System.Boolean EnsureHeaderEnd()
        {
            System.Byte[] hdr = encoding.GetBytes(CustomBinaryResourcesConstants.EndHeaderOrValue), dr = new System.Byte[hdr.LongLength];
            rwstream.Read(dr, 0, dr.Length);
            System.Boolean val = true;
            for (System.Int64 I = 0; I < dr.LongLength; I++)
            {
                if (hdr[I] != dr[I]) { val = false; break; }
            }
            hdr = null; dr = null;
            return val;
        }

        public System.Int64 WriteHeader(System.Int64 position , CustomBinaryHeaderBlob blob)
        {
            if (rwstream.CanWrite == false) { throw new ArgumentException("Cannot use an unwriteable stream for writing data."); }
            rwstream.Position = position;
            // BEGIN HEADER WRITE
            WriteString(CustomBinaryResourcesConstants.Magic);
            WriteEndHeaderValue();
            WriteString(CustomBinaryResourcesConstants.Version);
            WriteValueStart();
            WriteNumericValue(blob.Version);
            WriteEndHeaderValue();
            WriteString(CustomBinaryResourcesConstants.SupportedFormatsMask);
            WriteValueStart();
            WriteNumericValue((System.Int64)blob.CurrentFormatsMask);
            WriteEndHeaderValue();
            WriteString(CustomBinaryResourcesConstants.SupportedHeaderVersion);
            WriteValueStart();
            WriteNumericValue(blob.SupportedHeaderVersion);
            WriteEndHeaderValue();
            WriteString(CustomBinaryResourcesConstants.DataAlignmentHeader);
            WriteValueStart();
            WriteNumericValue(blob.DataPositionsAlignment);
            WriteEndHeaderValue();
            WriteString(CustomBinaryResourcesConstants.DataPositionsCount);
            WriteValueStart();
            WriteNumericValue(blob.DataPositions.LongLength);
            WriteEndHeaderValue();
            WriteString(CustomBinaryResourcesConstants.NextDataPositions);
            WriteValueStart();
            foreach (System.Int64 value in blob.DataPositions) { WriteNumericValue(value); }
            WriteEndHeaderValue();
            // DONE WRITING HEADER BLOB
            return rwstream.Position - position;
        }

        public CustomBinaryHeaderBlob ReadHeader(System.Int64 position)
        {
            if (rwstream.CanRead == false) { throw new ArgumentException(Properties.Resources.DNTRESEXT_StreamUnreadable); }
            CustomBinaryHeaderBlob ret = new();
            rwstream.Position = position;
            // BEGIN HEADER READ
            // Start by reading the magic value and determine if is correct.
            if (StringMatchesThis(CustomBinaryResourcesConstants.Magic) == false)
            {
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_INVALID_MAGIC, ParserErrorType.Deserialization);
            }
            if (EnsureHeaderEnd() == false) { throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_RESHEADER_CORRUPTED, ParserErrorType.Deserialization); }
            if (StringMatchesThis($"{CustomBinaryResourcesConstants.Version}{CustomBinaryResourcesConstants.ValueStartChar}") == false)
            {
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_VERSION_HEADER, ParserErrorType.Deserialization);
            }
            // Next step is to read the resource header version.
            System.Int64 v;
            if ((v = ReadNextNumericValue()) > CustomBinaryResourcesConstants.CurrentHeaderVersion)
            {
                // Invalid version , fail immediately.
                throw new CustomBinaryFormatException(System.String.Format(Properties.Resources.DNTRESEXT_BINFMT_HDR_VER_INVALID , v , CustomBinaryResourcesConstants.CurrentHeaderVersion), ParserErrorType.Versioning);
            }
            // Set the version. No need here to verify header end , already done by ReadNextNumericValue.
            ret.Version = (System.UInt16)v;
            if (StringMatchesThis($"{CustomBinaryResourcesConstants.SupportedFormatsMask}{CustomBinaryResourcesConstants.ValueStartChar}") == false)
            {
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_CURRENTFORMATS_HEADER, ParserErrorType.Deserialization);
            }
            // Now we have to read the binary resource format mask. We cannot perform any check here
            // because it is a mask; it means that the reader will read those resources it can read.
            ret.CurrentFormatsMask = (BinaryRESTypes)ReadNextNumericValue();
            // Now , read the Supported Header Version.
            if (StringMatchesThis($"{CustomBinaryResourcesConstants.SupportedHeaderVersion}{CustomBinaryResourcesConstants.ValueStartChar}") == false)
            {
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_SUPPORTEDHEADERVERSION_HEADER, ParserErrorType.Deserialization);
            }
            if ((v = ReadNextNumericValue()) > CustomBinaryResourcesConstants.CurrentFormatVersion)
            {
                throw new CustomBinaryFormatException(System.String.Format(Properties.Resources.DNTRESEXT_BINFMT_FMT_VER_INVALID , v , CustomBinaryResourcesConstants.CurrentFormatVersion), ParserErrorType.Versioning);
            }
            ret.SupportedHeaderVersion = (System.UInt16)v;
            // Read the data positions alignment header.
            if (StringMatchesThis($"{CustomBinaryResourcesConstants.DataAlignmentHeader}{CustomBinaryResourcesConstants.ValueStartChar}") == false)
            {
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_DATAPOSALIGNMENT_HEADER, ParserErrorType.Deserialization);
            }
            if ((v = ReadNextNumericValue()) != CustomBinaryResourcesConstants.DataPositionsAlignment)
            {
                throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_INVALIDALIGNMENT, ParserErrorType.Deserialization);
            }
            ret.DataPositionsAlignment = (System.UInt16)v;
            // Now we have two cases: the first one which did not had support for DATAPOSITIONSCOUNT , version 1 and version 2 which has such support.
            if (ret.Version == 1) // Version 1
            {
                if (StringMatchesThis($"{CustomBinaryResourcesConstants.NextDataPositions}{CustomBinaryResourcesConstants.ValueStartChar}") == false)
                {
                    throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_DATAPOSITIONS_HEADER, ParserErrorType.Deserialization);
                }
                // We cannot know the exact header size , due to the fact that an exactly same stop sequence might exist and break the reader.
                // So , we will verify each time if a resource starts; if not , it will continue to read 8 bytes.
                System.Int64 lp = rwstream.Position;
                System.Boolean found = EnsureHeaderEnd() && StringMatchesThis(CustomBinaryResourcesConstants.ResourceHeader);
                System.Byte[] temp = new System.Byte[ret.DataPositionsAlignment];
                List<System.Int64> positions = new();
                while (found == false && rwstream.Position < rwstream.Length)
                {
                    rwstream.Position = lp;
                    // Read 8 bytes.
                    rwstream.Read(temp, 0, temp.Length);
                    // Pass to BitConverter and save the data position.
                    positions.Add(temp.ToInt64(0));
                    lp = rwstream.Position;
                    found = EnsureHeaderEnd() && StringMatchesThis(CustomBinaryResourcesConstants.ResourceHeader);
                }
                if (rwstream.Position >= rwstream.Length) // Stream corrupted
                {
                    throw new System.IO.EndOfStreamException("Invalid stream. This stream is corrupted. The resources defined herein are corrupted.");
                }
                rwstream.Position = lp + 2; // We have verified header end already , we are good here. 
                ret.DataPositions = positions.ToArray();
                positions.Clear();
                positions = null;
            } else if (ret.Version == 2) // Version 2
            {
                // Because we have added the field of how many resources are , we can easily find the exact header size.
                if (StringMatchesThis($"{CustomBinaryResourcesConstants.DataPositionsCount}{CustomBinaryResourcesConstants.ValueStartChar}") == false)
                {
                    throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_DATAPOSITIONSCOUNT_HEADER, ParserErrorType.Deserialization);
                }
                v = ReadNextNumericValue();
                // We have the resource count. Verify data positions header and read the byte array.
                if (StringMatchesThis($"{CustomBinaryResourcesConstants.NextDataPositions}{CustomBinaryResourcesConstants.ValueStartChar}") == false)
                {
                    throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_NO_DATAPOSITIONS_HEADER, ParserErrorType.Deserialization);
                }
                System.Byte[] temp = ParserHelpers.ReadBuffered(rwstream, ret.DataPositionsAlignment * v);
                System.Int64[] dps = new System.Int64[v]; // we know the exact resource count , just create the array directly.
                for (System.Int32 I = 0; I < dps.Length; I++) // with this algorithm , set all data positions.
                {
                    dps[I] = temp.ToInt64(I * ret.DataPositionsAlignment);
                }
                ret.DataPositions = dps;
                temp = null; // Destroy temp we do not need it anymore.
                dps = null; // Destroy dps too. We have passed it to the class.
                // Verify header end too.
                if (EnsureHeaderEnd() == false) { 
                    throw new CustomBinaryFormatException(Properties.Resources.DNTRESEXT_BINFMT_RESHEADER_CORRUPTED, ParserErrorType.Deserialization); 
                }
            }
            // Somewhere here we are done.
            ret.WrittenBytesCount = rwstream.Position - position;
            return ret;
        }
    }

    // Defines the V2 custom binary header blob.
    // Additionally the CustomBinaryHeader class can read old and 'destroyed' V1 blobs successfully.
    internal record class CustomBinaryHeaderBlob
    {
        public System.UInt16 Version;
        public BinaryRESTypes CurrentFormatsMask;
        public System.UInt16 SupportedHeaderVersion;
        public System.UInt16 DataPositionsAlignment;
        public System.Int64[] DataPositions;
        // Runtime field that will be used by the new reader to compute the actual data position offsets.
        public System.Int64 WrittenBytesCount;

        // Creates a default new header blob.
        public static CustomBinaryHeaderBlob Default => new() { 
            DataPositionsAlignment = CustomBinaryResourcesConstants.DataPositionsAlignment,
            CurrentFormatsMask = CustomBinaryResourcesConstants.CurrentSupportedFormats,
            SupportedHeaderVersion = CustomBinaryResourcesConstants.CurrentHeaderVersion,
            Version = CustomBinaryResourcesConstants.CurrentFormatVersion
        };
    }

    internal enum BinaryRESTypes : System.Byte { String , ByteArray , Object }
}
