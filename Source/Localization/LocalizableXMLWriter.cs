using System.Globalization;
using DotNetResourcesExtensions.Internal;

namespace DotNetResourcesExtensions.Localization
{

    internal static class LocalizableXMLReaderWriterConstants
    {
        public const System.String RootElementName = "root" , 
            HeaderElementName = "resloc" , 
            HeaderVersionAttributeName = "version" , 
            DataNameAttributeName = "dataname" , 
            CultureAttributeName = "lang" , 
            DataNameElementName = "Data";
        public static System.Version Version = new("1.0");
    }

    /// <summary>
    /// Defines a simple XML writer for writing only localized strings.
    /// </summary>
    public sealed class LocalizableXMLWriter : LocalizedResourceWriter
    {
        private CultureInfo culture;
        private System.IO.Stream stream;
        private System.Xml.XmlWriter writer;
        private System.Boolean isstreamowner , _closed;
        private System.Xml.XmlWriterSettings settings;

        private LocalizableXMLWriter()
        {
            culture = CultureInfo.CurrentCulture;
            settings = new() { ConformanceLevel = System.Xml.ConformanceLevel.Document , Indent = true , CloseOutput = false };
            isstreamowner = false;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LocalizableXMLWriter"/> class with the specified underlying <paramref name="stream"/> 
        /// and the <paramref name="culture"/> which declares in which culture the resources are stored.
        /// </summary>
        /// <param name="stream">The underlying stream where all the resulting data will be saved.</param>
        /// <param name="culture">The culture in which all the resources written with this instance will be stored.</param>
        public LocalizableXMLWriter(System.IO.Stream stream , CultureInfo culture) : this()
        {
            this.stream = stream;
            writer = System.Xml.XmlWriter.Create(this.stream, settings);
            this.culture = culture;
            WriteHeader();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LocalizableXMLWriter"/> class with the specified path to a non-existent file
        /// and the <paramref name="culture"/> which declares in which culture the resources are stored.
        /// </summary>
        /// <param name="outfile">The file path where all the resulting data will be saved.</param>
        /// <param name="culture">The culture in which all the resources written with this instance will be stored.</param>
        public LocalizableXMLWriter(System.String outfile , CultureInfo culture) : this()
        {
            stream = new System.IO.FileStream(outfile, System.IO.FileMode.CreateNew);
            writer = System.Xml.XmlWriter.Create(stream, settings);
            isstreamowner = true;
            this.culture = culture;
            WriteHeader();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LocalizableXMLWriter"/> class with the specified underlying <paramref name="stream"/>. <br />
        /// The culture used for saving the localized data is the culture of the thread that will create this class , that is the <see cref="CultureInfo.CurrentCulture"/> property.
        /// </summary>
        /// <param name="stream">The underlying stream where all the resulting data will be saved.</param>
        public LocalizableXMLWriter(System.IO.Stream stream) : this()
        {
            this.stream = stream;
            writer = System.Xml.XmlWriter.Create(this.stream , settings);
            WriteHeader();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LocalizableXMLWriter"/> class with the specified path to a non-existent file. <br />
        /// The culture used for saving the localized data is the culture of the thread that will create this class , that is the <see cref="CultureInfo.CurrentCulture"/> property.
        /// </summary>
        /// <param name="outfile">The file path where all the resulting data will be saved.</param>
        public LocalizableXMLWriter(System.String outfile) : this() 
        {
            stream = new System.IO.FileStream(outfile, System.IO.FileMode.CreateNew);
            writer = System.Xml.XmlWriter.Create(stream, settings);
            isstreamowner = true;
            WriteHeader();
        }

        private void WriteHeader()
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(LocalizableXMLReaderWriterConstants.RootElementName);
            writer.WriteStartElement(LocalizableXMLReaderWriterConstants.HeaderElementName);
            writer.WriteAttributeString(LocalizableXMLReaderWriterConstants.HeaderVersionAttributeName,
                LocalizableXMLReaderWriterConstants.Version.ToString());
            writer.WriteAttributeString(LocalizableXMLReaderWriterConstants.DataNameAttributeName,
                LocalizableXMLReaderWriterConstants.DataNameElementName);
            writer.WriteAttributeString(LocalizableXMLReaderWriterConstants.CultureAttributeName, culture.Name);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Gets the currently selected culture for this instance , so all the resources will be written under this culture.
        /// </summary>
        public override CultureInfo SelectedCulture => culture;

        /// <summary>
        /// Gets or sets a value whether the <see cref="LocalizableXMLWriter"/> should also dispose the underlying stream before exiting.
        /// </summary>
        public override bool IsStreamOwner { get => isstreamowner; set => isstreamowner = value; }

        /// <summary>
        /// Adds a new localized resource to the list of the resources to be written.
        /// </summary>
        /// <param name="Name">The resource name to write.</param>
        /// <param name="Value">The resource value to write.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="Name"/> or <paramref name="Value"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="Name"/> has invalid naming characters.</exception>
        public override void AddResource(System.String Name , System.String Value)
        {
            ParserHelpers.ValidateName(Name);
            if (Value is null) { throw new System.ArgumentNullException(nameof(Value)); }
            if (writer is null) { return; }
            writer.WriteStartElement(LocalizableXMLReaderWriterConstants.DataNameElementName);
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", "string");
            {
                writer.WriteStartElement("string");
                writer.WriteValue(Value);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Adds a new localized resource to the list of the resources to be written by using a localized resource entry.
        /// </summary>
        /// <param name="entry">The resource entry to add.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="entry"/> parameter or it's name is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">The <paramref name="entry"/> parameter has a name that is invalid , or attempted to write a resource other than a string.</exception>
        /// <exception cref="System.InvalidOperationException">The culture that the <paramref name="entry"/> defines is different of what culture expects to contain only.</exception>
        public override void AddLocalizableEntry(ILocalizedResourceEntry entry)
        {
            if (entry is null) { throw new System.ArgumentNullException(nameof(entry)); }
            ParserHelpers.ValidateName(entry.Name);
            if (entry.TypeOfValue != typeof(System.String))
            {
                throw new System.ArgumentException("Currently only string resources are allowed to be written.");
            }
            if (entry.Culture.LCID != culture.LCID)
            {
                throw new System.InvalidOperationException($"Cannot add to the resource list the resource \'{entry.Name}\' because it has a culture \'{entry.Culture}\' that is different than the currently selected culture \'{culture}\'.");
            }
            if (writer is null) { return; }
            writer.WriteStartElement(LocalizableXMLReaderWriterConstants.DataNameElementName);
            writer.WriteAttributeString("name", entry.Name);
            writer.WriteAttributeString("type", "string");
            {
                writer.WriteStartElement("string");
                writer.WriteValue(entry.Value);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Generates the resulting data to the specified stream or file.
        /// </summary>
        public override void Generate()
        {
            writer?.Flush();
        }

        /// <inheritdoc />
        public override void Close()
        {
            if (writer is not null)
            {
                writer.WriteEndElement();
                writer.Close();
                try { stream.Flush(); } catch { }
                stream.Close();
            }
        }

        /// <summary>
        /// Disposes this <see cref="LocalizableXMLWriter"/> instance.
        /// </summary>
        public override void Dispose()
        {
            if (writer is not null)
            {
                writer.Dispose();
                writer = null;
            }
            if (stream is not null)
            {
                if (isstreamowner) { stream.Dispose(); }
                stream = null;
            }
        }

        /// <inheritdoc />
        public sealed override void AddResource(string name, byte[] value)
        {
            throw new System.NotSupportedException("This method is not supported for this type of writer.");
        }

        /// <inheritdoc />
        public sealed override void AddResource(string name, object value)
        {
            throw new System.NotSupportedException("This method is not supported for this type of writer.");
        }
    }
}
