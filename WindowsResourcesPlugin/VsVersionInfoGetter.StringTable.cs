using System.Collections.Generic;
using DotNetResourcesExtensions.Internal;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Represents the .NET structure of native StringTable information.
    /// </summary>
    public sealed class VsVersionInfoStringTable
    {
        private System.String native;
        private System.Int16 langid, codepage; 
        private IDictionary<System.String, System.String> props;

        /// <summary>
        /// Creates a default instance of <see cref="VsVersionInfoStringTable"/>.
        /// </summary>
        public VsVersionInfoStringTable()
        {
            langid = 0;
            codepage = 0;
            native = null;
            props = new Dictionary<System.String , System.String>();
        }

        internal VsVersionInfoStringTable(System.String lgid) : this()
        {
            native = lgid;
            System.Byte[] temp = MinimalHexDecoder.GetBytes(lgid, 0, 4);
            System.Array.Reverse(temp);
            langid = System.BitConverter.ToInt16(temp, 0);
            temp = MinimalHexDecoder.GetBytes(lgid, 4, 4);
            System.Array.Reverse(temp);
            codepage = System.BitConverter.ToInt16(temp, 0);
            temp = null;
        }

        /// <summary>
        /// Gets the full identification string (header) for this <see cref="VsVersionInfoStringTable"/>.
        /// </summary>
        public System.String NativeIdentifier => native;

        /// <summary>
        /// Gets the encoding defined for the resource values of this <see cref="VsVersionInfoStringTable"/> class.
        /// </summary>
        public System.Text.Encoding Encoding => System.Text.Encoding.GetEncoding(codepage);

        /// <summary>
        /// Gets the langauge that the resource values do represent.
        /// </summary>
        public System.Globalization.CultureInfo Culture => new(langid);

        /// <summary>
        /// Gets all the properties that comprise this string table.
        /// </summary>
        public IDictionary<System.String , System.String> Properties => props;
    }
}
