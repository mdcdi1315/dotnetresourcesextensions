
using System;
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Internal
{
    internal static class HumanReadableFormatConstants
    {
        public sealed record class Property
        {
            private readonly string name;
            private readonly object value;

            public Property(System.String name, object value)
            {
                this.name = name;
                this.value = value;
            }

            public static Property GetProperty(IDictionary<System.String, System.Object> data, System.String Name)
            {
                if (data is null) { throw new ArgumentNullException(nameof(data)); }
                if (Name is null) { throw new ArgumentNullException(nameof(Name)); }

                if (!data.TryGetValue(Name, out System.Object? value)) { throw new ArgumentException($"The {Name} property does not exist in the dictionary."); }

                return new Property(Name, value);
            }

            public System.String Name { get => name; }

            public System.String StringValue => value?.ToString();

            public System.Int64 Int64Value => ReadInt64OrFail(value);

            public System.Int32 Int32Value => ReadInt32OrFail(value);

            public void WriteToStreamAsTabbed(StringableStream stream, System.Byte tabs)
            {
                stream.WriteTabbedStringLine(tabs, $"{name} = {value}");
            }

            public override int GetHashCode() => base.GetHashCode();

            public System.Boolean Equals(Property other) => name == other.name;

            public System.Boolean ValueEquals(Property other) => value.Equals(other.value);

            public override string ToString() => $"Property: {{ Name: {name} Value: {value} }}";
        }

        static HumanReadableFormatConstants()
        {
            Version = new("version", (System.Int64)1);
            SchemaName = new("schema", "mdcdi1315.HRFMT");
            TypeIsString = new("type", "string");
            TypeIsByteArray = new("type", "bytearray");
            TypeIsFileRef = new("type", "filereference");
            TypeIsSerObj = new("type", "serobject");
        }

        public static readonly Property Version, SchemaName, TypeIsString, TypeIsSerObj, TypeIsByteArray, TypeIsFileRef;
        private const System.Int32 stringlinesize = 512;

        public static System.Int64 ReadInt64OrFail(System.Object data)
        {
            if (data is System.Int64 value) { return value; }
            throw new FormatException("The value is not a numeric value.");
        }

        public static System.Int32 ReadInt32OrFail(System.Object data) => ReadInt64OrFail(data).ToInt32();

        internal static System.Boolean StringsMatch(System.String one, System.String two) => one.Equals(two, StringComparison.InvariantCultureIgnoreCase);

        internal static void AddProperty(IDictionary<System.String, System.Object> properties, System.String value)
        {
            static System.Boolean GetNumber(System.String data, out System.Int64 num)
            {
                [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                static System.Boolean IsDigit(System.Char ch) => (System.UInt32)(ch - '0') <= ('9' - '0');
                if (data.Length > 0 && IsDigit(data[0]))
                {
                    num = ParserHelpers.ToNumber(data);
                    return true;
                }
                num = 0;
                return false;
            }
            System.Int32 eqindex = value.IndexOf('=');
            if (eqindex > -1)
            {
                System.String name = value.Remove(eqindex - 1);
                if (properties.ContainsKey(name)) { return; }
                System.Object propval = null;
                System.String data = value.Substring(eqindex + 2);
                if (GetNumber(data, out System.Int64 number)) { propval = number; } else { propval = data; }
                data = null;
                properties.Add(name, propval);
                propval = null;
                name = null;
            }
        }

        public static System.Byte[] ReadBase64ChunksValue(this StringableStream reader, System.Int32 expected, System.Int32 alignment, System.Int32 chunks)
        {
            System.String temp;
            System.Int32 chwithalignment = 0;
            System.Text.StringBuilder sb = new(chunks * alignment);
            while ((temp = reader.ReadLine()) is not null)
            {
                if (temp.StartsWith("chunk"))
                {
                    if (GetChunkIndex(temp) > chunks) { break; }
                    temp = reader.ReadLine();
                    sb.Append(temp);
                    if (temp.Length == alignment) { chwithalignment++; }
                    if (reader.ReadLine() != "end chunk") { throw new FormatException(DotNetResourcesExtensions.Properties.Resources.DNTRESEXT_HRFMT_CHUNK_INVALID); }
                }
                if (temp.Equals("end value")) { break; }
            }
            if (chwithalignment < (chunks - 1))
            {
                throw new FormatException(System.String.Format(DotNetResourcesExtensions.Properties.Resources.DNTRESEXT_HRFMT_DATA_CORRUPTED, chunks - 1, chwithalignment));
            }
            System.Byte[] decoded = sb.ToString().FromBase64();
            sb.Clear();
            sb = null;
            temp = null;
            if (decoded.Length != expected) { throw new FormatException(System.String.Format(DotNetResourcesExtensions.Properties.Resources.DNTRESEXT_HRFMT_INVALID_DATA_LEN, expected, decoded.Length)); }
            return decoded;
        }

        public static System.String ReadExactlyAndConvertToString(this StringableStream reader, System.Int32 rb)
        {
            System.Byte[] bytes = new System.Byte[rb];
            System.Int32 arb = reader.Read(bytes, 0, rb);
            return reader.Encoding.GetString(bytes, 0, arb);
        }

        private static System.Int32 GetChunkIndex(System.String data)
        {
            System.Int32 indxs = data.IndexOf('['), indxe = data.IndexOf(']');
            if (indxe == -1 || indxs == -1) { return -1; }
            System.Int32 ret = 0, prg = 1;
            for (System.Int32 I = indxe - 1; I > indxs; I--)
            {
                ret += (data[I].ToInt32() - 48) * prg;
                prg *= 10;
            }
            return ret;
        }

        public static void WriteBase64ChunksValue(this StringableStream writer, byte[] data)
        {
            System.String base64 = data.ToBase64();
            System.Int32 chunks = base64.Length / stringlinesize;
            System.Int32 lastrem = base64.Length % stringlinesize;
            writer.WriteTabbedStringLine(1, $"alignment = {stringlinesize}");
            writer.WriteTabbedStringLine(1, $"size = {data.LongLength}");
            writer.WriteTabbedStringLine(1, $"chunks = {(lastrem > 0 ? chunks + 1 : chunks)}");
            writer.WriteTabbedStringLine(1, "begin value");
            System.Int32 cgs = 0;
            System.String dt;
            for (System.Int32 I = 1; I <= chunks; I++)
            {
                if (cgs + stringlinesize > base64.Length)
                { dt = base64.Substring(cgs, base64.Length - stringlinesize); }
                else
                { dt = base64.Substring(cgs, stringlinesize); }
                writer.WriteTabbedStringLine(2, $"chunk[{I}]");
                writer.WriteTabbedStringLine(3, dt);
                writer.WriteTabbedStringLine(2, "end chunk");
                cgs += stringlinesize;
            }
            if (lastrem > 0)
            {
                dt = base64.Substring(cgs);
                writer.WriteTabbedStringLine(2, $"chunk[{chunks + 1}]");
                writer.WriteTabbedStringLine(3, dt);
                writer.WriteTabbedStringLine(2, "end chunk");
            }
            base64 = null;
            dt = null;
            writer.WriteTabbedStringLine(1, "end value");
        }
    }
}