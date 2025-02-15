
using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using DotNetResourcesExtensions.Internal;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions
{

    /// <summary>
    /// Defines a MS-INI resource entry.
    /// </summary>
    [DebuggerDisplay($"{{{nameof(Display)}(),nq}}")]
    public struct MsIniEntry : IResourceEntry
    {
        /// <summary>
        /// The name of this resource.
        /// </summary>
        public System.String ResourceName;
        /// <summary>
        /// The length in bytes , when <see cref="Data"/> are converted to a parseable byte array.
        /// </summary>
        public System.Int64 Length;
        /// <summary>
        /// The resource type that <see cref="Data"/> define.
        /// </summary>
        public System.Type ResourceType;
        /// <summary>
        /// The resource value or data.
        /// </summary>
        public System.Object Data;

        /// <summary>
        /// Forwarded implementation for the needs of <see cref="IResourceEntry"/> interface.
        /// </summary>
        public readonly System.String Name => ResourceName;

        /// <summary>
        /// Forwarded implementation for the needs of <see cref="IResourceEntry"/> interface.
        /// </summary>
        public readonly System.Object Value => Data;

        /// <summary>
        /// Forwarded implementation for the needs of <see cref="IResourceEntry"/> interface.
        /// </summary>
        public readonly System.Type TypeOfValue => Data?.GetType();

        private readonly string Display() => ResourceName ?? "Unassigned MsIniEntry";

        /// <summary>
        /// Gets a string value that represents the current instance. <br />
        /// Specifically , it returns the assigned value of <see cref="ResourceName"/> field.
        /// </summary>
        public readonly override System.String ToString() => Display();
    }

    /// <summary>
    /// This is the enumerator implementation that the <see cref="MsIniResourcesReader"/> currently uses. <br />
    /// You cannot create an instance of this class; Instead , use the <see cref="MsIniResourcesReader.GetEnumerator"/> method.
    /// </summary>
    [System.Obsolete(Obsoletions.MsIniResourcesDeprecated, false)]
    public sealed class MsIniResourcesEnumerator : Collections.AbstractDualResourceEntryEnumerator
    {
        private Internal.CustomFormatter.ICustomFormatter fmt;
        private System.String[,] entdata;
        private System.Int32 idx, idx2;
        private System.Int32 count;

        internal MsIniResourcesEnumerator(System.String[,] entitydata , Internal.CustomFormatter.ICustomFormatter inst)
        {
            fmt = inst;
            idx = idx2 = -1;
            entdata = entitydata;
            count = entdata.GetLength(0) / 4;
        }

        /// <summary>
        /// Moves to the next resource found.
        /// </summary>
        /// <returns><see langword="true"/> if moving to the next resource succeeded; otherwise <see langword="false"/>.</returns>
        public override System.Boolean MoveNext()
        {
            if (idx2 == -1) { idx = 0; } else { idx += 4; }
            idx2++;
            return idx2 < count;
        }

        private MsIniEntry GetEntry()
        {
            MsIniEntry ent = new();
            System.String typestr = System.String.Empty;
            ent.Length = -3;
            ent.ResourceName = entdata[idx, 1];
            if (System.String.Equals(entdata[idx + 2, 0], $"{ent.ResourceName}.Type"))
            {
                typestr = MsIniStringsEncoder.Decode(entdata[idx + 2, 1]);
                if (typestr != MsIniConstants.SpecialIFileReferenceTypeStr) { ent.ResourceType = System.Type.GetType(typestr, true, false); }
            }
            if (System.String.Equals(entdata[idx + 1, 0], $"{ent.ResourceName}.Length"))
            {
                ent.Length = ParserHelpers.ToNumber(entdata[idx + 1, 1]);
            }
            if (entdata[idx + 3, 0] == MsIniConstants.ResourceValue)
            {
                // We have a problem here. We cannot know if the string contains \n or \r characters which break the implementation.
                // For that reason , we will create an encoder/decoder that will encode-decode the data strings properly.
                if (ent.ResourceType != null && ent.Length != -3)
                {
                    System.String dec = MsIniStringsEncoder.Decode(entdata[idx + 3, 1]);
                    if (ent.ResourceType.FullName == "System.String")
                    {
                        // It is an encoded string , return it plainly
                        ent.Data = dec;
                    }
                    else
                    {
                        // Directly decode base64 data , we will need them in both cases.
                        ent.Data = dec.FromBase64();
                        if (ent.ResourceType.FullName == "System.Byte[]")
                        {
                            // It is a byte array , return it decoded as base64
                            return ent;
                        }
                        // This is an object encoded using the ExtensibleFormatter.
                        ent.Data = fmt.GetObjectFromBytes((System.Byte[])ent.Data, ent.ResourceType);
                    }
                } else if (typestr == MsIniConstants.SpecialIFileReferenceTypeStr && ent.Length != -3) {
                    // Oh. we have bumped into an encoded IFileReference.
                    // Decode the reference , then.
                    var tfr = InternalFileReference.ParseFromSerializedString(MsIniStringsEncoder.Decode(entdata[idx + 3, 1]));
                    System.IO.FileStream FS = null;
                    try {
                        FS = tfr.OpenStreamToFile();
                        System.Byte[] data = FS.ReadBytes(FS.Length);
                        ent.Data = tfr.SavingType.FullName switch
                        {
                            "System.String" => tfr.AsEncoding().GetString(data),
                            "System.Byte[]" => data,
                            _ => fmt.GetObjectFromBytes(data, tfr.SavingType),
                        };
                        data = null;
                    } finally {
                        FS?.Dispose();
                        FS = null;
                    }
                    tfr = null;
                } else {
                    // Cannot do nothing here!
                    throw new MSINIFormatException(Properties.Resources.DNTRESEXT_MSINIFMT_CANNOT_PARSE, ParserErrorType.Deserialization);
                }
            }
            return ent;
        }

        /// <summary>
        /// Gets a <see cref="DictionaryEntry"/> for this resource entry.
        /// </summary>
        public override DictionaryEntry Entry => AsMsIniEntry.AsDictionaryEntry();

        /// <summary>
        /// Gets the current entry as a instance of the <see cref="MsIniEntry"/> structure.
        /// </summary>
        public MsIniEntry AsMsIniEntry => GetEntry();

        /// <summary>
        /// Gets the current entry as a instance of the <see cref="IResourceEntry"/> interface.
        /// </summary>
        public override IResourceEntry ResourceEntry => GetEntry();

        /// <summary>
        /// Gets the resource name of the current entry.
        /// </summary>
        // Avoid calling GetEntry which can have heavy workloads , find it as GetEntry would have done.
        public override System.Object Key => entdata[idx, 1];

        /// <summary>
        /// Gets the resource value of the current entry.
        /// </summary>
        // We need to call it now so as to get the data.
        public override System.Object Value => GetEntry().Data;

        /// <summary>
        /// Resets the resource enumerator back to it's original state , which is before the first resource entry.
        /// </summary>
        public override void Reset() => idx2 = -1;
    }

    /// <summary>
    /// The <see cref="MsIniResourcesReader"/> class reads resources written using the 
    /// <see cref="MsIniResourcesWriter"/> class. <br /> Note that the file format is specific for 
    /// the current class , so any second thoughts about not being the defined format will be immediately caught.
    /// </summary>
    [System.Obsolete(Obsoletions.MsIniResourcesDeprecated , false)]
    public sealed class MsIniResourcesReader : IDotNetResourcesExtensionsReader
    {
        private StringableStream stream;
        private System.Text.Encoding encoding;
        private System.Int32 ver, sppmask;
        private System.Boolean isfile;
        private ExtensibleFormatter formatter;

        private MsIniResourcesReader()
        {
            formatter = ExtensibleFormatter.Create();
            stream = null;
            encoding = System.Text.Encoding.UTF8;
            ver = 0;
            sppmask = 0;
            isfile = false;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MsIniResourcesReader"/> class using the specified stream that contains 
        /// the custom written MS-INI resource data.
        /// </summary>
        /// <param name="str">The stream to read from.</param>
        public MsIniResourcesReader(System.IO.Stream str) : this()
        {
            stream = new(str , encoding);
            GetHeader();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MsIniResourcesReader"/> class from an existing file path that contains 
        /// the custom written MS-INI resource data.
        /// </summary>
        /// <param name="savepath">The file to open to read the resources from.</param>
        public MsIniResourcesReader(System.String savepath) : this()
        {
            stream = new(new System.IO.FileStream(savepath, System.IO.FileMode.Open) , encoding);
            stream.IsStreamOwner = true;
            isfile = true;
            GetHeader();
        }

        /// <inheritdoc />
        public System.Boolean IsStreamOwner 
        { 
            get => stream?.IsStreamOwner ?? false; 
            set { 
                if (stream is null) { throw new ObjectDisposedException(nameof(MsIniResourcesReader)); }
                stream.IsStreamOwner = value; 
            }
        }

        private void DetermineEncoding()
        {
            const System.String EncConstant = "; ENCODING: ";
            System.String ln = stream.ReadLiteralLine();
            if (ln is not null && ln.StartsWith(EncConstant))
            {
                ln = ln.Substring(EncConstant.Length + 2);
                ln = ln.Remove(ln.IndexOf(MsIniConstants.Quote.ToChar()));
                try
                {
                    encoding = System.Text.Encoding.GetEncoding(ParserHelpers.ToNumber(ln).ToInt32());
                } catch (System.ArgumentOutOfRangeException) { }
                stream.Encoding = encoding;
            }
        }

        private void GetHeader()
        {
            DetermineEncoding();
            var hdr = FetchEntityWithName(MsIniConstants.HeaderString);
            ExtensibleFormatter EF = new();
            System.Byte[] data;
            for (System.Int32 I = 0; I < hdr.GetLength(0); I += 4)
            {
                if (hdr[I , 1] == "Version")
                {
                    data = MsIniStringsEncoder.Decode(hdr[I + 3, 1]).FromBase64();
                    ver = EF.GetObject<System.UInt16>(data);
                } else if (hdr[I , 1] == MsIniConstants.ResMask)
                {
                    data = MsIniStringsEncoder.Decode(hdr[I + 3, 1]).FromBase64();
                    sppmask = EF.GetObject<System.Int32>(data);
                }
            }
            EF?.Dispose();
            if (ver > MsIniConstants.Version)
            {
                throw new MSINIFormatException(System.String.Format(Properties.Resources.DNTRESEXT_MSINIFMT_VER_MISMATCH , ver) , ParserErrorType.Header);
            } else if (ver == 0 && sppmask == 0)
            {
                throw new MSINIFormatException(Properties.Resources.DNTRESEXT_MSINIFMT_INVALID_DATA, ParserErrorType.Header);
            }
        }

        private System.String GetNextLine()
        {
            System.String result = System.String.Empty;
            System.Byte[] data;
            System.Int32 br;
            do
            {
                System.Int64 posbefore = stream.Position, posafter = posbefore;
                System.Int32 val;
                while ((val = stream.ReadByte()) > -1)
                {
                    if (val == 10 || val == 13) { break; }
                    posafter++;
                }
                stream.Position = posbefore;
                data = new System.Byte[posafter - posbefore];
                br = stream.Read(data, 0, data.Length);
                stream.Position++;
                result = encoding.GetString(data, 0, br);
            } while (result.StartsWith(";") && stream.Position < stream.Length);
            return result;
        }

        /// <summary>
        /// The format version that this reader currently reads out.
        /// </summary>
        public System.Int32 Version => ver;

        /// <summary>
        /// Returns the supported resource formats mask defined in the resource file.
        /// </summary>
        public System.Int32 SupportedFormatsMask => sppmask;

        private System.String[,] FetchEntityWithName(System.String name)
        {
            System.Int64 posbef , entend , streamatbegginning = stream.Position;
            System.String di = null , tmp , td;
            stream.Position = 0;
            do
            {
                tmp = stream.ReadLine();
                if (tmp.Length > 0)
                {
                    if (tmp[0] == '[' && tmp[^1] == ']') // First and last index
                    {
                        // We found an entity. Test it...
                        tmp = tmp[1..];
                        di = tmp.Remove(tmp.Length - 1);
                    } else if (tmp.StartsWith(";")) { 
                        // Comment , skip the line.
                        di = null;
                    } else {
                        // skip line , get to next one.
                        di = null;
                    }
                }
            } while (di != name && stream.Position < stream.Length);
            if (di != name) { throw new ArgumentException($"Specified entity was not found: {name}"); }
            // If we retrieve now the stream position , we will have the position after the entity new line. Keep it for now.
            posbef = stream.Position;
            // To understand entity end we need to fetch three times consecutively the \n\n pattern , or the file was finished , whichever occurs first.
            System.Int32 rb = -3 , ct = 0;
            while (ct < 3 && (rb = stream.ReadByte()) > -1)
            {
                if (rb == 10) { ct++; } else { ct = 0; }
            }
            if (ct == 3 || rb == -1) // We found the required pattern , or end of stream occured.
            {
                entend = stream.Position;
                // Go back to the entity beginning.
                stream.Position = posbef;
                // Fetch all lines now.
                List<System.String> names = new() , values = new();
                while (stream.Position <= entend)
                {
                    td = GetNextLine();
                    rb = td.IndexOf(MsIniConstants.ValueDelimiter.ToChar());
                    if (rb == -1) { continue; }
                    names.Add(td.Remove(rb)); 
                    values.Add(td.Substring(rb + 1));
                    td = null;
                }
                System.String[,] strings = new System.String[values.Count , 2];
                for (ct = 0; ct < names.Count; ct++)
                {
                    strings[ct, 0] = names[ct];
                    strings[ct, 1] = values[ct];
                }
                names.Clear();
                names = null;
                values.Clear();
                values = null;
                // Get back to our original read position.
                stream.Position = streamatbegginning;
                return strings;
            } else {
                throw new MSINIFormatException(Properties.Resources.DNTRESEXT_MSINIFMT_ENT_INVALID, ParserErrorType.Deserialization);
            }
        }

        /// <inheritdoc cref="System.Resources.IResourceReader.GetEnumerator" />
        /// <exception cref="ObjectDisposedException">This reader has been effectively disposed and cannot access the original instance data.</exception>
        public MsIniResourcesEnumerator GetEnumerator()
        {
            if (stream is null) { throw new ObjectDisposedException(nameof(MsIniResourcesReader)); }
            return new(FetchEntityWithName("ResourceIndex"), formatter);
        }

        IDictionaryEnumerator System.Resources.IResourceReader.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Closes this reader.
        /// </summary>
        public void Close()
        {
            if (isfile && IsStreamOwner == false) { IsStreamOwner = true; }
            stream?.Close();
        }

        /// <summary>
        /// Disposes the reader and any data associated with it.
        /// </summary>
        public void Dispose()
        {
            if (IsStreamOwner) {
                stream?.Dispose();
            }
            stream = null;
            encoding = null;
            formatter?.Dispose();
            formatter = null;
        }

        /// <inheritdoc />
        public void RegisterTypeResolver(ITypeResolver resolver) => formatter.RegisterTypeResolver(resolver);
    }

}
