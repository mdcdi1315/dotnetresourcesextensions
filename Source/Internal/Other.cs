
using System;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions.Internal
{
    /// <summary>
    /// Defines a format to save serialized objects using the <see cref="ICustomFormatter"/> interface
    /// for resource classes which did not had support or was not considered such support. <br />
    /// Additionally it provides two extension methods that extend the <see cref="ICustomFormatter"/>
    /// interface for supporting using this format directly from the formatter instance.
    /// </summary>
    public static class ResourceInterchargeFormat
    {
        private const System.Int32 HDRLEN = 3;
        private const System.Int32 SIZEIDLEN = 4;
        /// <summary>
        /// Defines the header as bytes. Use it to recognize the format in the resource classes.
        /// </summary>
        public static System.Byte[] RifHeaderAsBytes = new System.Byte[3] { 82, 73, 70 };
        // This format is defined as follows: 
        // RIF(Header) -> 0000(Size of .NET type string) -> .NET type string -> 0000(Size of data to save) -> Data
        // For LE cases , the arrays will be reversed.

        /// <summary>
        /// Get a new Intercharge format from the specified formatter and the object to serialize.
        /// </summary>
        /// <param name="formatter">The formatter which will serialize the specified object.</param>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A new intercharge format array.</returns>
        public static System.Byte[] GetFromObject(CustomFormatter.ICustomFormatter formatter, System.Object obj)
        {
            System.Byte[] formatteddata = formatter.GetBytesFromObject(obj);
            System.Byte[] nameasbytes = System.Text.Encoding.UTF8.GetBytes(obj.GetType().AssemblyQualifiedName);
            System.Byte[] Final = new System.Byte[HDRLEN + SIZEIDLEN + nameasbytes.Length + SIZEIDLEN + formatteddata.Length];
            System.Int32 idx = 0;
            System.Array.ConstrainedCopy(RifHeaderAsBytes , 0 , Final , idx , HDRLEN);
            idx += HDRLEN;
            System.Byte[] temp = nameasbytes.Length.GetBytes();
            if (System.BitConverter.IsLittleEndian) { temp.Reverse(); }
            Array.ConstrainedCopy(temp, 0, Final, idx, SIZEIDLEN);
            idx += SIZEIDLEN;
            Array.ConstrainedCopy(nameasbytes, 0, Final, idx, nameasbytes.Length);
            idx += nameasbytes.Length;
            nameasbytes = null;
            temp = formatteddata.Length.GetBytes();
            if (System.BitConverter.IsLittleEndian) { temp.Reverse(); }
            Array.ConstrainedCopy(temp, 0, Final, idx, SIZEIDLEN);
            idx += SIZEIDLEN;
            Array.ConstrainedCopy(formatteddata , 0 , Final , idx , formatteddata.Length);
            return Final;
        }

        /// <summary>
        /// Get the original object from the produced array from <see cref="GetFromObject(CustomFormatter.ICustomFormatter, object)"/> 
        /// method and the formatter to deserialize data.
        /// </summary>
        /// <param name="formatter">The formatter which will deserialize the object back to it's original state.</param>
        /// <param name="bytes">The byte array acquired from <see cref="GetFromObject(CustomFormatter.ICustomFormatter, object)"/> method.</param>
        /// <returns>The original object.</returns>
        /// <exception cref="FormatException">The array given was not the array returned from <see cref="GetFromObject(CustomFormatter.ICustomFormatter, object)"/>.</exception>
        public static System.Object GetFromBytes(CustomFormatter.ICustomFormatter formatter , byte[] bytes)
        {
            System.Int32 I = 0;
            for (; I < HDRLEN;I++)
            {
                if (bytes[I] != RifHeaderAsBytes[I]) { throw new FormatException("This is not the intercharge format."); }
            }
            // Is at 3.
            // Read next 4 bytes , reverse them and call BitConverter.
            System.Byte[] temp = new System.Byte[SIZEIDLEN];
            Array.ConstrainedCopy(bytes , I , temp , 0 , SIZEIDLEN);
            if (System.BitConverter.IsLittleEndian) { temp.Reverse(); }
            System.Int32 len = temp.ToInt32(0);
            I += SIZEIDLEN; // Skip the 4 bytes read previously.
            // Read the string.
            System.String typestring = System.Text.Encoding.UTF8.GetString(ParserHelpers.GetBytes(bytes: bytes, I, len));
            // We will feed it to the type in a bit , keep it for now.
            // Skip the read bytes.
            I += len;
            // Read next 4 bytes.
            temp = ParserHelpers.GetBytes(bytes: bytes, I, SIZEIDLEN);
            if (System.BitConverter.IsLittleEndian) { temp.Reverse(); }
            len = temp.ToInt32(0);
            // Skip the 4 read bytes read previously.
            I += SIZEIDLEN;
            // Return the object.
            return formatter.GetObjectFromBytes(ParserHelpers.GetBytes(bytes: bytes , I , len) , System.Type.GetType(typestring , true , true));
        }

        /// <summary>
        /// Returns the serialized bytes for <paramref name="obj"/> but also directly saves it under the Resource Intercharge Format for directly saving it.
        /// </summary>
        /// <param name="formatter">The formatter which will serialize the specified object.</param>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A new intercharge format array containing <paramref name="obj"/>.</returns>
        public static System.Byte[] GetObjectAsResourceInterchargeFormatBytes(this CustomFormatter.ICustomFormatter formatter , System.Object obj) => GetFromObject(formatter , obj);

        /// <summary>
        /// Gets the original object defined in the Resource Intercharge Format from the specified <paramref name="bytes"/>.
        /// </summary>
        /// <param name="formatter">The formatter which will deserialize the object back to it's original state.</param>
        /// <param name="bytes">The byte array acquired from <see cref="GetFromObject(CustomFormatter.ICustomFormatter, object)"/> or <see cref="GetObjectAsResourceInterchargeFormatBytes(CustomFormatter.ICustomFormatter, object)"/> methods.</param>
        /// <returns>The original object.</returns>
        /// <exception cref="FormatException">The array given was not the array returned from <see cref="GetFromObject(CustomFormatter.ICustomFormatter, object)"/> or <see cref="GetObjectAsResourceInterchargeFormatBytes(CustomFormatter.ICustomFormatter, object)"/>.</exception>
        public static System.Object GetFromResourceInterchargeFormatBytes(this CustomFormatter.ICustomFormatter formatter, System.Byte[] bytes) => GetFromBytes(formatter, bytes);
    }

    /// <summary>
    /// Decodes and encodes strings for the MS-INI resource data classes. <br />
    /// This class is needed due to the fact that the strings that contain invalid characters must be quoted, <br />
    /// plus the CR , LF and TAB characters must be encoded so as to not break stuff in the reader. <br />
    /// To do that , it uses as a sequence escaper the hyphen , and after it the n , t , r or - as the escape characters.
    /// </summary>
    public static class MsIniStringsEncoder
    {
        private static System.Char[] InvalidChars = new System.Char[] { '\"', ',', '=', '[', ']', '\'', ';' };

        /// <summary>
        /// Encodes the specified string to a custom but valid MS-INI string. <br />
        /// Also quotes it if needed.
        /// </summary>
        /// <param name="enc">The string to encode.</param>
        /// <returns>The encoded string equal to <paramref name="enc"/> parameter.</returns>
        public static System.String Encode(System.String enc)
        {
            System.Text.StringBuilder result = new(enc.Length);
            System.Boolean quotes = false;
            for (System.Int32 I = 0; I < enc.Length; I++)
            {
                for (System.Int32 J = 0; J < InvalidChars.Length && quotes == false; J++) // Do not search again if we have found such invalid character.
                {
                    if (enc[I] == InvalidChars[J]) { quotes = true; }
                }
                result.Append(enc[I] switch
                {
                    '\n' => "-n",
                    '\r' => "-r",
                    '\t' => "-t",
                    '-' => "--",
                    _ => enc[I].ToString()
                });
            }
            if (quotes) {
                result.Insert(0, '\"');
                result.Append('\"');
                return result.ToString();
            } else {
                return result.ToString();
            }
        }

        /// <summary>
        /// Decodes the specified string produced from the <see cref="Encode(string)"/> method.
        /// </summary>
        /// <param name="enc">The encoded string.</param>
        /// <returns>The original string.</returns>
        /// <exception cref="MSINIFormatException">An invalid escaped sequence was found.</exception>
        public static System.String Decode(System.String enc)
        {
            System.String dc;
            System.Text.StringBuilder dec = new(enc.Length);
            if (enc.Length > 0 && ParserHelpers.AreQuotesAtStartEnd(enc))
            {
                dc = enc.Substring(1);
                dc = dc.Remove(dc.Length - 1);
            } else { dc = enc; }
            for (System.Int32 I = 0; I < dc.Length; I++)
            {
                if (dc[I] == '-' && (I + 1) < dc.Length)
                {
                    // switch through cases
                    dec.Append(dc[I + 1] switch { 
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        '-' => '-',
                        _ => throw new MSINIFormatException(System.String.Format(Properties.Resources.DNTRESEXT_MSINIFMT_INVALID_ENCODE_SEQUENCE , dc[I+1]), ParserErrorType.Deserialization)
                    });
                    I++; // We are very much sure about it. In worst case , it throws an exception and we are done.
                } else { 
                    // Otherwise just make sure to copy it to the result
                    dec.Append(dc[I]);
                }
            }
            return dec.ToString();
        }
    }
}