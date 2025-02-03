
namespace DotNetResourcesExtensions.Collections
{
    /// <summary>
    /// Represents a <see cref="System.Collections.DictionaryEntry"/> that does have a comment.
    /// </summary>
    public struct DictionaryEntryWithComment
    {
        /// <summary><inheritdoc cref="System.Collections.DictionaryEntry.Key"/></summary>
        public System.Object Key { get; set; }

        /// <summary><inheritdoc cref="System.Collections.DictionaryEntry.Value"/></summary>
        public System.Object Value { get; set; }

        /// <summary>
        /// Gets or sets the comment in the key/value pair.
        /// </summary>
        public System.String Comment { get; set; }

        /// <inheritdoc cref="System.Collections.DictionaryEntry.DictionaryEntry(object, object)" />
        public DictionaryEntryWithComment(System.Object key, System.Object value)
        {
            if (key is null) { throw new System.ArgumentNullException(nameof(key)); }
            if (value is null) { throw new System.ArgumentNullException(nameof(value)); }
            Key = key; Value = value;
        }

        /// <summary>
        /// Creates a new instance of <see cref="DictionaryEntryWithComment"/> structure with the specified 
        /// key , value and comment.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="comment">The comment.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="key"/> or <paramref name="value"/> were null.</exception>
        public DictionaryEntryWithComment(System.Object key, System.Object value, System.String comment)
        {
            // comment can be null , so I do perform a check here.
            if (key is null) { throw new System.ArgumentNullException(nameof(key)); }
            if (value is null) { throw new System.ArgumentNullException(nameof(value)); }
            Key = key; Value = value; Comment = comment;
        }
    }

}