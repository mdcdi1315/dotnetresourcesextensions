using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Threading.Tasks;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Represents a default implementation of <see cref="IResourceTransferer"/> interface. <br />
    /// This class is <see langword="abstract"/> and it must be inherited.
    /// </summary>
    public abstract class DefaultResourcesTransferer : IResourceTransferer
    {
        /// <summary>
        /// Set to this field the reader to read resources from. <br />
        /// Do not perform to this field any disposal processing - all are handled by this class!
        /// </summary>
        protected System.Resources.IResourceReader reader;
        /// <summary>
        /// Set to this field the writer to copy the resources to. <br />
        /// Do not perform to this field any disposal processing - all are handled by this class!
        /// </summary>
        protected System.Resources.IResourceWriter writer;

        private IEnumerable<string> resourcenames;
        private System.Boolean prepared;

        /// <summary>
        /// Default constructor that must be called by your inheriting class using the 
        /// <c>: base()</c> convention. <br />
        /// Do not forget to call <see cref="PrepareInstance"/> after setting valid values in the writer/reader sets!
        /// </summary>
        protected DefaultResourcesTransferer()
        {
            prepared = false;
        }

        /// <summary>
        /// Prepares the current instance. You must call this after you have set a valid set of reader and writer instances!
        /// </summary>
        protected void PrepareInstance()
        {
            if (prepared) { return; }
            resourcenames = ResNames(reader);
            prepared = true;
        }

        /// <summary>
        /// Another direct constuctor that auto-fills the required reader and writer. <br />
        /// Note: Here is no need to call <see cref="PrepareInstance"/> because you have already given valid values for writer/reader instances!
        /// </summary>
        /// <param name="reader">The reader to supply.</param>
        /// <param name="writer">The writer to supply.</param>
        protected DefaultResourcesTransferer(IResourceReader reader, IResourceWriter writer) : this()
        {
            this.reader = reader;
            this.writer = writer;
            PrepareInstance();
        }

        private IEnumerable<System.String> ResNames(System.Resources.IResourceReader enumerator)
        {
            foreach (System.Collections.DictionaryEntry de in enumerator)
            {
                yield return (System.String)de.Key;
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> ReaderResourceNames => resourcenames;

        /// <inheritdoc />
        public IResourceReader CurrentUsedReader => reader;

        /// <inheritdoc />
        public IResourceWriter CurrentUsedWriter => writer;

        /// <inheritdoc />
        public void AddResource(string name, byte[]? value)
        => writer.AddResource(name, value);

        /// <inheritdoc />
        public void AddResource(string name, object? value)
        => writer.AddResource(name, value);

        /// <inheritdoc />
        public void AddResource(string name, string? value)
        => writer.AddResource(name, value);

        /// <inheritdoc />
        public void Close()
        {
            reader.Close();
            writer.Close();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            reader?.Dispose();
            writer?.Dispose();
            writer = null;
            reader = null;
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
         => new(Task.Run(Dispose));

        /// <inheritdoc />
        public void Generate() { writer.Generate(); }

        /// <inheritdoc />
        public ValueTask GenerateAsync()
            => new(Task.Run(Generate));

        /// <inheritdoc />
        public void TransferAll()
        {
            foreach (DictionaryEntry de in reader)
            {
                System.String key = de.Key.ToString();
                if (de.Value is System.Byte[] one) { writer.AddResource(key , one); }
                else if (de.Value is System.String two) { writer.AddResource(key, two); }
                else { writer.AddResource(key, de.Value); }
            }
        }

        /// <inheritdoc />
        public void TransferSelection(IEnumerable<string> resnames)
        {
            foreach (DictionaryEntry de in reader)
            {
                System.String key = de.Key.ToString();
                if (resnames.Contains(key))
                {
                    if (de.Value is System.Byte[] one) { writer.AddResource(key, one); }
                    else if (de.Value is System.String two) { writer.AddResource(key, two); }
                    else { writer.AddResource(key, de.Value); }
                }
            }
        }
    }
}
