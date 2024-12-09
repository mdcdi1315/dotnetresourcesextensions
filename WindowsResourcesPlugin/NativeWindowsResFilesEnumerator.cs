using System;
using System.Collections;
using DotNetResourcesExtensions.Internal;
using DotNetResourcesExtensions.Collections;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Provides the enumerator implementation that <see cref="NativeWindowsResFilesReader"/> class uses. <br />
    /// It is not possible to create an instance of this class; instead , use the <see cref="NativeWindowsResFilesReader.GetEnumerator"/> method.
    /// </summary>
    public sealed class NativeWindowsResFilesEnumerator : ISpecificResourceEntryEnumerator<NativeWindowsResourceEntry>
    {
        private RESFILEReader reader;
        private NativeWindowsResourceEntry entry;

        [System.Diagnostics.DebuggerHidden]
        internal NativeWindowsResFilesEnumerator(System.IO.Stream stream , System.Int64 start)
        {
            if (stream is null || stream.CanRead == false) { throw new ObjectDisposedException(nameof(NativeWindowsResFilesReader)); }
            stream.Position = start;
            reader = new(stream , stream.Length);
            entry = null;
        }

        /// <summary>
        /// Returns the native resource entry that was read by the reader.
        /// </summary>
        public NativeWindowsResourceEntry ResourceEntry => entry;

        /// <inheritdoc />
        public DictionaryEntry Entry => new(entry.Name , entry.Value);

        /// <inheritdoc />
        public object Key => entry.Name;

        /// <inheritdoc />
        public object Value => entry.Value;

        /// <inheritdoc />
        public object Current => Entry;

        IResourceEntry IResourceEntryEnumerator.ResourceEntry => entry;

        /// <summary>
        /// Advances the enumerator to the next resource entry.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator was successfully advanced to the next resource entry; 
        /// <see langword="false"/> if the enumerator has read all the resource entries.</returns>
        public bool MoveNext()
        {
            entry = reader.ReadNext();
            return entry is not null;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first resource entry in the stream.
        /// </summary>
        public void Reset() => reader.Reset();
    }
}
