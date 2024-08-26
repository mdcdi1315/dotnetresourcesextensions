using System;
using System.Xml.Linq;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// Reads the data written by the <see cref="LocalizableXMLWriter"/> class.
    /// </summary>
    public sealed class LocalizableXMLReader : LocalizedResourceReader
    {
        private System.String dataelementname;
        private CultureInfo culture;
        private XDocument xdt;

        private sealed class Enumerator : IDictionaryEnumerator
        {
            private LocalizableXMLReader reader;
            private IEnumerator<XElement> elements;

            public Enumerator(LocalizableXMLReader rdr)
            {
                reader = rdr;
                Reinitialize();
            }

            private void Reinitialize()
            {
                IEnumerable<XElement> el = reader.xdt.Root.Elements(XName.Get(reader.dataelementname))
                    ?? throw new ArgumentException($"Cannot find any descendant resources that use the name {reader.dataelementname}.");
                elements = el.GetEnumerator();
                el = null;
            }

            public DictionaryEntry Entry
            {
                get {
                    System.String name, type;
                    XElement element = elements.Current;
                    name = element.Attribute("name").Value;
                    type = element.Attribute("type").Value;
                    return new(name, element.Element(System.Xml.Linq.XName.Get(type)).Value);
                }
            }

            public object Key => elements.Current.Attribute("name").Value;

            public object? Value => Entry.Value;

            public object Current => Entry;

            public bool MoveNext() => elements.MoveNext();

            public void Reset() => Reinitialize();
        }

        private LocalizableXMLReader() : base()
        {
            xdt = null;
            dataelementname = null;
            culture = CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LocalizableXMLReader"/> class by reading the specified stream.
        /// </summary>
        /// <param name="str">The stream that contains data written using the <see cref="LocalizableXMLWriter"/> class.</param>
        public LocalizableXMLReader(System.IO.Stream str) : this() {
            xdt = XDocument.Load(str);
            ReadHeader();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LocalizableXMLReader"/> class by reading the file.
        /// </summary>
        /// <param name="path">The file path that contains data written using the <see cref="LocalizableXMLWriter"/> class.</param>
        public LocalizableXMLReader(System.String path) : this() {
            xdt = XDocument.Load(path);
            ReadHeader();
        }

        private void ReadHeader()
        {
            if (xdt.Root.Name != LocalizableXMLReaderWriterConstants.RootElementName)
            {
                throw new FormatException("This is not the localizable resource format.");
            }
            XElement ed = xdt.Root.Element(LocalizableXMLReaderWriterConstants.HeaderElementName);
            if (ed is null)
            {
                throw new FormatException("After the root element , the header element must exist.");
            }
            if (ed.HasAttributes == false)
            {
                throw new FormatException("The header element must contain the language , version and the data element name attributes.");
            }
            foreach (var attr in ed.Attributes())
            {
                switch (attr.Name.LocalName) {
                    case LocalizableXMLReaderWriterConstants.DataNameAttributeName:
                        dataelementname = attr.Value; break;
                    case LocalizableXMLReaderWriterConstants.HeaderVersionAttributeName:
                        if (LocalizableXMLReaderWriterConstants.Version > new System.Version(attr.Value))
                        {
                            throw new FormatException($"This reader cannot read this format version. Use instead a reader that supports the format version {attr.Value}.");
                        }
                        break;
                    case LocalizableXMLReaderWriterConstants.CultureAttributeName:
                        culture = new(attr.Value);
                        break;
                }
            }
            if (culture is null || dataelementname is null)
            {
                throw new FormatException("Could not read the required attributes for determining the reader instance. Creation Failed.");
            }
        }

        /// <inheritdoc />
        public override CultureInfo SelectedCulture => culture;

        /// <summary>
        /// Attempting to get this property always returns false , while attempting to set it always throws <see cref="NotSupportedException"/>.
        /// </summary>
        public override bool IsStreamOwner {
            get => false;
            set => throw new NotSupportedException("This operation is not supported.");
        }

        /// <summary>
        /// This method effectively does nothing.
        /// </summary>
        public override void Close() { }

        /// <summary>
        /// Disposes the <see cref="LocalizableXMLReader"/> class.
        /// </summary>
        public override void Dispose() {
            dataelementname = null;
            culture = null;
            xdt = null;
        }

        /// <inheritdoc />
        public override IDictionaryEnumerator GetEnumerator()
        {
            if (xdt is null) { throw new ObjectDisposedException(nameof(LocalizableXMLReader)); }
            return new Enumerator(this);
        }
    }
}
