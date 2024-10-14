
using System;
using System.Collections;
using System.Collections.Generic;
using DotNetResourcesExtensions.Collections;

#pragma warning disable CA1416

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Defines the enumerator instance class layout that is currently used by <see cref="NativeWindowsResourcesReader"/>.
    /// </summary>
    public sealed class NativeWindowsResourcesEnumerator : ISpecificResourceEntryEnumerator<NativeWindowsResourceEntry>
    {
        private System.Boolean enf;
        private IEnumerable<System.ValueTuple<object, object, ushort, byte[]>> enumerable;
        private IEnumerator<System.ValueTuple<object, object, ushort, byte[]>> enumerator;

        internal NativeWindowsResourcesEnumerator(IEnumerable<System.ValueTuple<object, object, ushort, byte[]>> en)
        {
            enf = false;
            enumerable = en;
            enumerator = enumerable.GetEnumerator();
        }

        [System.Diagnostics.DebuggerHidden]
        private static TResult ParseEnum<TResult>(System.String data)
            where TResult : notnull, System.Enum
        {
            System.Type type = typeof(TResult);
            System.String[] names = type.GetEnumNames();
            for (System.Int32 I = 0; I < names.Length; I++)
            {
                if (names[I].Equals(data))
                {
                    return (TResult)type.GetEnumValues().GetValue(I);
                }
            }
            throw new System.ArgumentException($"The name {data} does not belong to a named constant in {type.Name} enumeration.");
        }

        /// <summary>
        /// Gets the native Windows resource entry instance.
        /// </summary>
        public NativeWindowsResourceEntry ResourceEntry
        {
            get {
                if (enf == false) { throw new System.InvalidOperationException("The enumeration has either finished or has not started yet."); }
                System.ValueTuple<object, object, ushort, System.Byte[]> temp = enumerator.Current;
                NativeWindowsResourceEntry ret = new();
                ret.Name = temp.Item1;
                if (temp.Item2 is System.UInt16 type)
                {
                    ret.NativeType = (WindowsResourceEntryType)type;
                } else if (temp.Item2 is System.String strtype)
                {
                    try { ret.NativeType = ParseEnum<WindowsResourceEntryType>(strtype); } 
                    // If the above fails too , then just assign the given type string to the NativeTypeString property.
                    catch (System.ArgumentException) { ret.NativeTypeString = strtype; }
                }
                ret.Value = temp.Item4;
                if (temp.Item3 == 0) { // If the culture is returned as zero , then the resource is invariant.
                    ret.Culture = System.Globalization.CultureInfo.InvariantCulture;
                } else {
                    try {
                        ret.Culture = new(temp.Item3);
                    } catch (System.Globalization.CultureNotFoundException) {
                        ret.Culture = System.Globalization.CultureInfo.InvariantCulture;
                    } catch (System.ArgumentOutOfRangeException) {
                        ret.Culture = System.Globalization.CultureInfo.InvariantCulture;
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// Gets the resource name.
        /// </summary>
        public object Key => Entry.Key;

        /// <summary>
        /// Gets the resource value , which is always a byte array.
        /// </summary>
        public object Value => Entry.Value;

        /// <summary>
        /// Gets the contents of <see cref="ResourceEntry"/> instance as a dictionary entry.
        /// </summary>
        public DictionaryEntry Entry => ResourceEntry.AsDictionaryEntry();

        /// <inheritdoc />
        public object Current => Entry;

        /// <summary>
        /// Gets a deserialized resource entry that is more closer to the enumerator implementations
        /// of DotNetResourcesExtensions project.
        /// </summary>
        public DeserializingWindowsResourceEntry DeserializedEntry => new(ResourceEntry);

        IResourceEntry IResourceEntryEnumerator.ResourceEntry => DeserializedEntry;

        /// <inheritdoc />
        public bool MoveNext() => enf = enumerator.MoveNext();

        /// <inheritdoc />
        public void Reset() {
            enf = false;
            enumerator = null;
            enumerator = enumerable.GetEnumerator();
        }
    }
}
