
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
    /// You cannot create an instance of this class; Instead , use the <see cref="MsIniResourcesReader.GetEnumerator"/> method , then cast to this type.
    /// </summary>
    public sealed class MsIniResourcesEnumerator : Collections.IResourceEntryEnumerator
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
            count = entdata.Length / 4;
        }

        /// <summary>
        /// Moves to the next resource found.
        /// </summary>
        /// <returns><see langword="true"/> if the moving to the next resource succeeded; otherwise <see langword="false"/>.</returns>
        public System.Boolean MoveNext()
        {
            if (idx2 == -1) { idx = 0; } else { idx += 4; }
            idx2++;
            return idx2 < count;
        }

        private MsIniEntry GetEntry()
        {
            MsIniEntry ent = new();
            ent.Length = -3;
            ent.ResourceName = entdata[idx, 1];
            if (System.String.Equals(entdata[idx + 2, 0], $"{ent.ResourceName}.Type"))
            {
                System.String tp = entdata[idx + 2, 1];
                tp = tp[1..];
                tp.Remove(tp.Length - 1);
                ent.ResourceType = System.Type.GetType(tp, true, true);
            }
            if (System.String.Equals(entdata[idx + 1, 0], $"{ent.ResourceName}.Length"))
            {
                ent.Length = ParserHelpers.ToNumber(entdata[idx + 1, 1]);
            }
            if (entdata[idx + 3, 0] == MsIniConstants.ResourceValue)
            {
                // We have a problen here. We cannot know if the string contains \n or \r characters which break the implementation.
                // For that reason , we will create an encoder/decoder that will encode-decode the data strings properly.
                if (ent.ResourceType != null && ent.Length != -3)
                {
                    System.String dec = Internal.MsIniStringsEncoder.Decode(entdata[idx + 3, 1]);
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
                }
                else
                {
                    // Cannot do nothing here!
                    throw new MSINIFormatException("Cannot parse the resource because impartial data were retrieved.", ParserErrorType.Deserialization);
                }
            }
            return ent;
        }

        /// <summary>
        /// Gets a <see cref="DictionaryEntry"/> for this resource entry.
        /// </summary>
        public DictionaryEntry Entry
        {
            get {
                MsIniEntry ent = AsMsIniEntry;
                return new(ent.ResourceName, ent.Data);
            }
        }

        /// <summary>
        /// Gets the current entry as a instance of the <see cref="MsIniEntry"/> structure.
        /// </summary>
        public MsIniEntry AsMsIniEntry => GetEntry();

        /// <summary>
        /// Gets the current entry as a instance of the <see cref="IResourceEntry"/> interface.
        /// </summary>
        public IResourceEntry ResourceEntry => GetEntry();

        /// <summary>
        /// Gets the resource name of the current entry.
        /// </summary>
        // Avoid calling GetEntry which can have heavy workloads , find it as GetEntry would have done.
        public System.Object Key => entdata[idx, 1];

        /// <summary>
        /// Gets the resource value of the current entry.
        /// </summary>
        // We need to call it now so as to get the data.
        public System.Object Value => GetEntry().Data;

        /// <summary>
        /// Returns a casted <see cref="DictionaryEntry"/> object. Equal to calling the <see cref="Entry"/> property.
        /// </summary>
        public System.Object Current => Entry;

        /// <summary>
        /// Resets the resource enumerator back to it's original state , which is before the first resource entry.
        /// </summary>
        public void Reset() => idx2 = -1;
    }

    /// <summary>
    /// The <see cref="MsIniResourcesReader"/> class reads resources written using the 
    /// <see cref="MsIniResourcesWriter"/> class. <br /> Note that the file format is specific for 
    /// the current class , so any second thoughts about not being the defined format will be immediately caught.
    /// </summary>
    public sealed class MsIniResourcesReader : IDotNetResourcesExtensionsReader
    {
        private System.IO.Stream stream;
        private System.Text.Encoding encoding;
        private System.Boolean strmown;
        private System.Int32 ver, sppmask;
        private ExtensibleFormatter formatter;

        private MsIniResourcesReader()
        {
            formatter = ExtensibleFormatter.Create();
            stream = null;
            encoding = System.Text.Encoding.UTF8;
            strmown = false;
            ver = 0;
            sppmask = 0;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MsIniResourcesReader"/> class using the specified stream that contains 
        /// the custom written MS-INI resource data.
        /// </summary>
        /// <param name="str">The stream to read from.</param>
        public MsIniResourcesReader(System.IO.Stream str) : this()
        {
            stream = str;
            GetHeader();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MsIniResourcesReader"/> class from an existing file path that contains 
        /// the custom written MS-INI resource data.
        /// </summary>
        /// <param name="savepath">The file to open to read the resources from.</param>
        public MsIniResourcesReader(System.String savepath) : this()
        {
            stream = new System.IO.FileStream(savepath, System.IO.FileMode.Open);
            strmown = true;
            GetHeader();
        }

        /// <inheritdoc />
        public System.Boolean IsStreamOwner { get => strmown; set => strmown = value; }

        private void DetermineEncoding()
        {
            const System.String EncConstant = "; ENCODING: ";
            System.String ln = GetNextLine();
            if (ln is not null && ln.StartsWith(EncConstant))
            {
                ln = ln.Substring(EncConstant.Length + 2);
                ln = ln.Remove(ln.IndexOf((System.Char)MsIniConstants.Quote));
                try
                {
                    encoding = System.Text.Encoding.GetEncoding((System.Int32)ParserHelpers.ToNumber(ln));
                } catch (System.ArgumentOutOfRangeException) { }
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
                    data = System.Convert.FromBase64String(MsIniStringsEncoder.Decode(hdr[I + 3, 1]));
                    ver = EF.GetObject<System.UInt16>(data);
                } else if (hdr[I , 1] == MsIniConstants.ResMask)
                {
                    data = System.Convert.FromBase64String(MsIniStringsEncoder.Decode(hdr[I + 3, 1]));
                    sppmask = EF.GetObject<System.Int32>(data);
                }
            }
            EF?.Dispose();
            if (ver > MsIniConstants.Version)
            {
                throw new MSINIFormatException($"This reader cannot read this format version ({ver})." , ParserErrorType.Header);
            } else if (ver == 0 && sppmask == 0)
            {
                throw new MSINIFormatException("Could not read header data. This might not be the MS-INI DotNetResourcesExtensions format." , ParserErrorType.Header);
            }
        }
        
        /// <summary>
        /// The format version that this reader currently reads out.
        /// </summary>
        public System.Int32 Version => ver;

        /// <summary>
        /// Returns the supported resource formats mask defined in the resource file.
        /// </summary>
        public System.Int32 SupportedFormatsMask => sppmask;

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

        private System.String[,] FetchEntityWithName(System.String name)
        {
            System.Int64 posbef , entend , streamatbegginning = stream.Position;
            System.String di , tmp;
            stream.Position = 0;
            do
            {
                tmp = GetNextLine();
                if (tmp.Length > 0 && tmp[0] == '[' && tmp[^1] == ']') // First and last index
                {
                    // We found an entity. Test it...
                    tmp = tmp[1..];
                    di = tmp.Remove(tmp.Length - 1);
                } else {
                    // skip line , get to next one.
                    di = null; 
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
                    System.String td = GetNextLine();
                    rb = td.IndexOf((System.Char)MsIniConstants.ValueDelimiter);
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
                throw new FormatException("Could not find entity end. The entity might have been misconstructed.");
            }
        }

        /// <inheritdoc />
        public IDictionaryEnumerator GetEnumerator() => new MsIniResourcesEnumerator(FetchEntityWithName("ResourceIndex") , formatter);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Closes this reader.
        /// </summary>
        public void Close()
        {
            if (strmown)
            {
                stream?.Close();
            }
        }

        /// <summary>
        /// Disposes the reader and any data associated with it.
        /// </summary>
        public void Dispose()
        {
            if (strmown)
            {
                stream?.Dispose();
                stream = null;
            }
            encoding = null;
        }

        /// <inheritdoc />
        public void RegisterTypeResolver(ITypeResolver resolver)
        {
            formatter.RegisterTypeResolver(resolver);
        }
    }

}
