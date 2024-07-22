using System.Collections;
using System.Resources;
using DotNetResourcesExtensions.Internal;
using DotNetResourcesExtensions.Internal.CustomFormatter;

namespace DotNetResourcesExtensions
{
   
    /// <summary>
    /// Dummy interface that all resource writers in the DotNetResourcesExtensions project are currently implementing. <br />
    /// It is not recommended for the end user to use this interface; instead , he could also use the <see cref="IResourceWriter"/> interface alone.
    /// </summary>
    public interface IDotNetResourcesExtensionsWriter : IResourceWriter , IStreamOwnerBase , IUsingCustomFormatter { }

    /// <summary>
    /// Dummy interface that all resource readers in the DotNetResourcesExtensions project are currently implementing. <br />
    /// It is not recommended for the end user to use this interface; besides, the <see cref="IResourceReader"/> interface 
    /// alone can be used in <see cref="DefaultResourceLoader"/> derived classes.
    /// </summary>
    public interface IDotNetResourcesExtensionsReader : IResourceReader , IStreamOwnerBase , IUsingCustomFormatter { }

    /// <summary>
    /// Reader and writer extensions that specifically belong to the <see cref="IDotNetResourcesExtensionsReader"/> and
    /// <see cref="IDotNetResourcesExtensionsWriter"/> interfaces.
    /// </summary>
    public static class ReaderWriterExtensions
    {
        /// <summary>
        /// Adds the specified resource entry to the resources to be written by the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The writer to add the resource to.</param>
        /// <param name="entry">The resource entry to add to the <paramref name="writer"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="entry"/> or <paramref name="writer"/> were <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">The <see cref="IResourceEntry.Value"/> property of the entry was <see langword="null"/>.</exception>
        public static void AddResourceEntry(this IDotNetResourcesExtensionsWriter writer , IResourceEntry entry)
        {
            if (entry == null) { throw new System.ArgumentNullException(nameof(entry)); }
            if (writer == null) { throw new System.ArgumentNullException(nameof(writer)); }
            switch (entry.TypeOfValue?.FullName)
            {
                case "":
                case null:
                    throw new System.ArgumentException("The Value property of this entry is null.");
                case "System.String":
                    writer.AddResource(entry.Name , (System.String)entry.Value);
                    break;
                case "System.Byte[]":
                    writer.AddResource(entry.Name , (System.Byte[])entry.Value);
                    break;
                default:
                    writer.AddResource(entry.Name , entry.Value);
                    break;
            }
        }

        /// <summary>
        /// Adds the specified resource entries to be written by the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The writer to add the resources to.</param>
        /// <param name="enumerable">The resource entries to be added to the <paramref name="writer"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="enumerable"/> or <paramref name="writer"/> were <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">This exception is more generic and is thrown at many cases. See the inner exception that is returned along with this one for more information.</exception>
        public static void AddResourceEntries(this IDotNetResourcesExtensionsWriter writer , System.Collections.Generic.IEnumerable<IResourceEntry> enumerable)
        {
            if (enumerable == null) { throw new System.ArgumentNullException(nameof(enumerable)); }
            if (writer == null) { throw new System.ArgumentNullException(nameof(writer)); }
            try
            {
                foreach (var entry in enumerable) { AddResourceEntry(writer , entry); }
            } catch (System.ArgumentException e) when (e is System.ArgumentNullException)
            {
                throw new System.ArgumentException("A resource entry inside the entries was null." , e);
            } catch (System.ArgumentException e)
            {
                throw new System.ArgumentException("An entry inside the entries has as it's value null and cannot be added to the resource list.", e);
            }
        }

        /// <summary>
        /// Adds the specified resource entries to be written by the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The writer to add the resources to.</param>
        /// <param name="entries">The resource entries to be added to the <paramref name="writer"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="entries"/> or <paramref name="writer"/> were <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">This exception is more generic and is thrown at many cases. See the inner exception that is returned along with this one for more information.</exception>
        public static void AddResourceEntries(this IDotNetResourcesExtensionsWriter writer , Collections.IResourceEntryEnumerable entries)
        {
            if (entries == null) { throw new System.ArgumentNullException(nameof(entries)); }
            if (writer == null) { throw new System.ArgumentNullException(nameof(writer)); }
            AddResourceEntries(writer , enumerable: entries);
        }

        /// <summary>
        /// Gets the specified resource entry from the specified <paramref name="Name"/>.<br />
        /// If the resource entry was not found , it throws <see cref="ResourceNotFoundException"/>.
        /// </summary>
        /// <param name="reader">The reader to get the entry from.</param>
        /// <param name="Name">The resource name that you want to get it's resource value.</param>
        /// <returns>The resource entry with <paramref name="Name"/> , if the entry was found.</returns>
        /// <exception cref="ResourceNotFoundException">The resource with the name specified by the <paramref name="Name"/> parameter was not found.</exception>
        public static IResourceEntry GetResourceEntry(this IDotNetResourcesExtensionsReader reader , System.String Name)
        {
            foreach (DictionaryEntry enumerated in reader) {
                if (enumerated.Key.ToString() == Name) { return enumerated.AsResourceEntry(); }
            }
            throw new ResourceNotFoundException(Name);
        }
    }

}
