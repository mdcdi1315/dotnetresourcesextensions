using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Defines a collection of strings that have been retrieved from a native resource that has it's type set as <see cref="WindowsResourceEntryType.RT_STRING"/>.
    /// </summary>
    public sealed class NativeStringsCollection : IList<System.String>
    {
        private System.Text.Encoding encoding;
        private List<System.String> strings;

        private NativeStringsCollection() 
        { 
            strings = new();
            encoding = new UnicodeEncoding(System.BitConverter.IsLittleEndian == false, false);
        }

        /// <summary>
        /// Creates a new instance of <see cref="NativeStringsCollection"/> class from the specified entry that contains strings. <br />
        /// The <see cref="NativeWindowsResourceEntry.NativeType"/> property of <paramref name="entry"/> 
        /// must be <see cref="WindowsResourceEntryType.RT_STRING"/> , otherwise the construction will fail.
        /// </summary>
        /// <param name="entry">The entry to read the strings from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> was effectively null.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property in <paramref name="entry"/> is not RT_STRING.</exception>
        public NativeStringsCollection(NativeWindowsResourceEntry entry) : this()
        {
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            if (entry.NativeType != WindowsResourceEntryType.RT_STRING)
            {
                throw new ArgumentException("The NativeType of the resource entry must have been set to RT_STRING.");
            }
            GetStrings(entry.Value);
        }

        private void GetStrings(System.Byte[] bytes)
        {
            try
            {
                System.Int32 length = -1;
                for (System.Int32 I = 0; I < bytes.Length; I++)
                {
                    // if bytes[I] == 0 , it is pad and must be avoided.
                    if (bytes[I] == 0) { continue; }
                    // Read length if not read previously.
                    if (length < 0) {
                        length = System.BitConverter.ToInt16(bytes , I);
                        // Due to UTF-16 encoding , I must multiply by 2.
                        length *= 2;
                        // I++ here to avoid the second byte read.
                        I++;
                    } else { 
                        // Read the string.
                        strings.Add(encoding.GetString(bytes, I, length));
                        // Skip I to length-1 bytes
                        I += length - 1;
                        // length = -1 to read the next string.
                        length = -1;
                    }
                }
                // I do only care for fallback exceptions , anything parsed will be returned.
            } catch (System.ArgumentException) { }
        }

        /// <inheritdoc />
        public System.Int32 Count => strings.Count;

        /// <inheritdoc />
        public System.Boolean IsReadOnly => true;

        /// <inheritdoc />
        public System.String this[int index] { get => strings[index]; set => throw new NotSupportedException("This collection is read-only."); }

        /// <inheritdoc />
        public void Add(System.String item) => throw new NotSupportedException("This collection is read-only.");

        /// <inheritdoc />
        public void Clear() => throw new NotSupportedException("This collection is read-only.");

        /// <inheritdoc />
        public bool Contains(System.String item) => strings.Contains(item);

        /// <inheritdoc />
        public void CopyTo(System.String[] array, int arrayIndex) => strings.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(System.String item) => throw new NotSupportedException("This collection is read-only.");

        /// <inheritdoc />
        public int IndexOf(System.String item) => strings.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, System.String item) => throw new NotSupportedException("This collection is read-only.");

        /// <inheritdoc />
        public void RemoveAt(int index) => throw new NotSupportedException("This collection is read-only.");

        /// <inheritdoc />
        public IEnumerator<System.String> GetEnumerator() => strings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
