namespace DotNetResourcesExtensions.Localization
{
    /// <summary>
    /// Represents a localization index reader implementation which gets it's functionality from <see cref="JSONResourcesReader"/> class.
    /// </summary>
    public sealed class JSONLocalizationIndexReader : LocalizationIndexResourceReader
    {
        /// <summary>
        /// Creates a new <see cref="JSONLocalizationIndexReader"/> class from the specified file on disk.
        /// </summary>
        /// <param name="file">The file path to create the reader from.</param>
        public JSONLocalizationIndexReader(System.String file) : base(new JSONResourcesReader(file)) { Validate(); }

        /// <summary>
        /// Creates a new <see cref="JSONLocalizationIndexReader"/> class from the specified data stream.
        /// </summary>
        /// <param name="stream">The stream to create the reader from.</param>
        public JSONLocalizationIndexReader(System.IO.Stream stream) : base(new JSONResourcesReader(stream)) { Validate(); }
    }

    /// <summary>
    /// Represents a localization index writer implementation which gets it's functionality from <see cref="JSONResourcesWriter"/> class.
    /// </summary>
    public sealed class JSONLocalizationIndexWriter : LocalizationIndexResourceWriter
    {
        /// <summary>
        /// Creates a new <see cref="JSONLocalizationIndexWriter"/> class by creating a new file specified by <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file path to create the writer from.</param>
        public JSONLocalizationIndexWriter(System.String file) : base(new JSONResourcesWriter(file)) { }

        /// <summary>
        /// Creates a new <see cref="JSONLocalizationIndexWriter"/> class by saving all the data to the data <paramref name="stream"/> specified.
        /// </summary>
        /// <param name="stream">The stream to write the resulting data to.</param>
        public JSONLocalizationIndexWriter(System.IO.Stream stream) : base(new JSONResourcesWriter(stream)) { }
    }

    /// <summary>
    /// Represents a localization index reader implementation which gets it's functionality from <see cref="XMLResourcesReader"/> class.
    /// </summary>
    public sealed class CustomXMLLocalizationIndexReader : LocalizationIndexResourceReader
    {
        /// <summary>
        /// Creates a new <see cref="CustomXMLLocalizationIndexReader"/> class from the specified file on disk.
        /// </summary>
        /// <param name="file">The file path to create the reader from.</param>
        public CustomXMLLocalizationIndexReader(System.String file) : base(new XMLResourcesReader(file)) { Validate(); }
        
        /// <summary>
        /// Creates a new <see cref="CustomXMLLocalizationIndexReader"/> class from the specified data stream.
        /// </summary>
        /// <param name="stream">The stream to create the reader from.</param>
        public CustomXMLLocalizationIndexReader(System.IO.Stream stream) : base(new XMLResourcesReader(stream)) { Validate(); }
    }

    /// <summary>
    /// Represents a localization index writer implementation which gets it's functionality from <see cref="XMLResourcesWriter"/> class.
    /// </summary>
    public sealed class CustomXMLLocalizationIndexWriter : LocalizationIndexResourceWriter
    {
        /// <summary>
        /// Creates a new <see cref="CustomXMLLocalizationIndexWriter"/> class by creating a new file specified by <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file path to create the writer from.</param>
        public CustomXMLLocalizationIndexWriter(System.String file) : base(new XMLResourcesWriter(file)) { }

        /// <summary>
        /// Creates a new <see cref="CustomXMLLocalizationIndexWriter"/> class by saving all the data to the data <paramref name="stream"/> specified.
        /// </summary>
        /// <param name="stream">The stream to write the resulting data to.</param>
        public CustomXMLLocalizationIndexWriter(System.IO.Stream stream) : base(new XMLResourcesWriter(stream)) { }
    }

    /// <summary>
    /// Represents a localization index reader implementation which gets it's functionality from <see cref="MsIniResourcesReader"/> class.
    /// </summary>
    public sealed class CustomMsIniLocalizationIndexReader : LocalizationIndexResourceReader
    {
        /// <summary>
        /// Creates a new <see cref="CustomMsIniLocalizationIndexReader"/> class from the specified file on disk.
        /// </summary>
        /// <param name="file">The file path to create the reader from.</param>
        public CustomMsIniLocalizationIndexReader(System.String file) : base(new MsIniResourcesReader(file)) { Validate(); }

        /// <summary>
        /// Creates a new <see cref="CustomMsIniLocalizationIndexReader"/> class from the specified data stream.
        /// </summary>
        /// <param name="stream">The stream to create the reader from.</param>
        public CustomMsIniLocalizationIndexReader(System.IO.Stream stream) : base(new MsIniResourcesReader(stream)) { Validate(); }
    }

    /// <summary>
    /// Represents a localization index writer implementation which gets it's functionality from <see cref="MsIniResourcesWriter"/> class.
    /// </summary>
    public sealed class CustomMsIniLocalizationIndexWriter : LocalizationIndexResourceWriter
    {
        /// <summary>
        /// Creates a new <see cref="CustomMsIniLocalizationIndexWriter"/> class by creating a new file specified by <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file path to create the writer from.</param>
        public CustomMsIniLocalizationIndexWriter(System.String file) : base(new XMLResourcesWriter(file)) { }

        /// <summary>
        /// Creates a new <see cref="CustomMsIniLocalizationIndexWriter"/> class by saving all the data to the data <paramref name="stream"/> specified.
        /// </summary>
        /// <param name="stream">The stream to write the resulting data to.</param>
        public CustomMsIniLocalizationIndexWriter(System.IO.Stream stream) : base(new XMLResourcesWriter(stream)) { }
    }

    /// <summary>
    /// Represents a localization index reader implementation which gets it's functionality from <see cref="Internal.ResX.ResXResourceReader"/> class.
    /// </summary>
    public sealed class CustomResXLocalizationIndexReader : LocalizationIndexResourceReader
    {
        /// <summary>
        /// Creates a new <see cref="CustomResXLocalizationIndexReader"/> class from the specified file on disk.
        /// </summary>
        /// <param name="file">The file path to create the reader from.</param>
        public CustomResXLocalizationIndexReader(System.String file) : base(new Internal.ResX.ResXResourceReader(file)) { Validate(); }

        /// <summary>
        /// Creates a new <see cref="CustomXMLLocalizationIndexReader"/> class from the specified data stream.
        /// </summary>
        /// <param name="stream">The stream to create the reader from.</param>
        public CustomResXLocalizationIndexReader(System.IO.Stream stream) : base(new Internal.ResX.ResXResourceReader(stream)) { Validate(); }
    }

    /// <summary>
    /// Represents a localization index writer implementation which gets it's functionality from <see cref="Internal.ResX.ResXResourceWriter"/> class.
    /// </summary>
    public sealed class CustomResXLocalizationIndexWriter : LocalizationIndexResourceWriter
    {
        /// <summary>
        /// Creates a new <see cref="CustomResXLocalizationIndexWriter"/> class by creating a new file specified by <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file path to create the writer from.</param>
        public CustomResXLocalizationIndexWriter(System.String file) : base(new Internal.ResX.ResXResourceWriter(file)) { }

        /// <summary>
        /// Creates a new <see cref="CustomResXLocalizationIndexWriter"/> class by saving all the data to the data <paramref name="stream"/> specified.
        /// </summary>
        /// <param name="stream">The stream to write the resulting data to.</param>
        public CustomResXLocalizationIndexWriter(System.IO.Stream stream) : base(new Internal.ResX.ResXResourceWriter(stream)) { }
    }
}
