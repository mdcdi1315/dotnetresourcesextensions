
using System;

namespace DotNetResourcesExtensions.Internal
{

    /// <summary>
    /// Decodes and encodes strings for the MS-INI resource data classes. <br />
    /// This class is needed due to the fact that the strings that contain invalid characters must be quoted, <br />
    /// plus the CR , LF and TAB characters must be encoded so as to not break stuff in the reader. <br />
    /// To do that , it uses as a sequence escaper the hyphen , and after it the n , t , r or - as the escape characters.
    /// </summary>
    public static class MsIniStringsEncoder
    {
        private static System.Char[] InvalidChars = new System.Char[] { '\"' , ',' , '=' , '[' , ']' , '\'' , ';' };

        /// <summary>
        /// Encodes the specified string to a custom but valid MS-INI string. <br />
        /// Also quotes it if needed.
        /// </summary>
        /// <param name="enc">The string to encode.</param>
        /// <returns>The encoded string equal to <paramref name="enc"/> parameter.</returns>
        public static System.String Encode(System.String enc)
        {
            System.String result = System.String.Empty;
            System.Boolean quotes = false;
            for (System.Int32 I = 0; I < enc.Length; I++)
            {
                for (System.Int32 J = 0; J < InvalidChars.Length && quotes == false; J++) // Do not search again if we have found such invalid character.
                {
                    if (enc[I] == InvalidChars[J]) { quotes = true; }
                }
                if (enc[I] == '\n') {
                    result += "-n";
                } else if (enc[I] == '\r') {
                    result += "-r";
                } else if (enc[I] == '\t') {
                    result += "-t";
                } else if (enc[I] == '-') {
                    result += "--";
                } else { result += enc[I]; }
            }
            if (quotes) {
                return $"\"{result}\"";
            } else {
                return result;
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
            System.String decoded = System.String.Empty , dc;
            if (enc.Length > 0 && enc[0] == '\"' && enc[enc.Length - 1] == '\"')
            {
                dc = enc.Substring(1);
                dc = dc.Remove(dc.Length - 1);
            } else { dc = enc; }
            for (System.Int32 I = 0; I < dc.Length; I++)
            {
                if (dc[I] == '-' && (I + 1) < dc.Length) {
                    System.Char c = dc[I+1]; // Indexing done only once , faster than indexing multiple times.
                    I++; // We are very much sure about it. In worst case , it throws an exception and we are done.
                    if (c == 'n') {
                        decoded += "\n";
                    } else if (c == 'r') {
                        decoded += "\r";
                    } else if (c == 't') {
                        decoded += "\t";
                    } else if (c == '-') {
                        decoded += "-";
                    } else {
                        throw new MSINIFormatException($"Invalid or unknown sequence detected: \'-{c}\'" , ParserErrorType.Deserialization);
                    }
                } else { // Otherwise just make sure to copy it to the result
                    decoded += dc[I]; 
                }
            }
            return decoded;
        }
    }

}
