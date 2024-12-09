using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using DotNetResourcesExtensions.Internal;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// A message table is a table that contains formatted resource strings for using them in Windows message boxes.
    /// </summary>
    public sealed class MessageTable
    {
        private List<KeyValuePair<System.UInt32, System.String>> strings;

        [StructLayout(LayoutKind.Explicit , Size = 12)]
        private unsafe struct MessageTableBlock
        {
            public static MessageTableBlock Read(System.Byte[] data , System.Int32 startindex)
            {
                System.Int32 size = sizeof(MessageTableBlock);
                if (size > data.Length - startindex)
                {
                    throw new ArgumentException("There are not enough bytes in order to initialize the Message Table Block.");
                }
                MessageTableBlock block = new();
                fixed (System.Byte* src = &data[startindex]) 
                {
                    fixed (System.Byte* dst = &Unsafe.AsRef(block.pin))
                    {
                        Unsafe.CopyBlockUnaligned(dst, src, size.ToUInt32());
                    }
                }
                return block;
            }

            [FieldOffset(0)]
            private System.Byte pin;

            // The first message ID defined in this block.
            [FieldOffset(0)]
            public System.UInt32 LowID;

            // The last message ID defined in this block.
            [FieldOffset(4)]
            public System.UInt32 HighID;

            // The offset required in order to reach the message entries.
            [FieldOffset(8)]
            public System.UInt32 OffsetToEntries;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MessageTable"/> class from the specified native entry.
        /// </summary>
        /// <param name="entry">The entry to construct the message table from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> was <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> property of <paramref name="entry"/> was not <see cref="WindowsResourceEntryType.RT_MESSAGETABLE"/>.</exception>
        public MessageTable(NativeWindowsResourceEntry entry)
        {
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            if (entry.NativeType != WindowsResourceEntryType.RT_MESSAGETABLE) {
                throw new ArgumentException("The native type of the resource entry must be RT_MESSAGETABLE.");
            }
            strings = new();
            Read(entry.Value);
        }

        private void Read(System.Byte[] data)
        {
            System.Int32 idx = 0;
            // Read the block count.
            System.UInt32 blkcount = data.ToUInt32(idx);
            idx += 4;
            // Create a new array of all the message blocks.
            MessageTableBlock[] blocks = new MessageTableBlock[blkcount];
            // Copy the found message blocks to the array.
            for (System.Int32 I = 0; I < blkcount; I++)
            {
                blocks[I] = MessageTableBlock.Read(data, idx);
                idx += 12;
            }
            System.UInt32 id, nentries;
            System.Int32 coffset;
            // For each message block , read it's message entries.
            for (System.Int32 I = 0; I < blkcount; I++)
            {
                id = blocks[I].LowID;
                // Get the number of entries , and increase array capacity
                strings.Capacity += (nentries = (blocks[I].HighID + 1) - id).ToInt32();
                // Get the starting offset.
                coffset = blocks[I].OffsetToEntries.ToInt32();
                for (System.UInt32 J = 0; J < nentries; J++)
                {
                    // Read the entries. The length here is the length of the whole message , 
                    // including the length and type variables.
                    System.UInt16 length = data.ToUInt16(coffset);
                    coffset += 2;
                    System.UInt16 type = data.ToUInt16(coffset);
                    coffset += 2;
                    // Read the string message appropriately.
                    System.UInt16 arraylen = (length - 4).ToUInt16();
                    if (type == 1) // Unicode string
                    {
                        strings.Add(new(id+J , System.Text.Encoding.Unicode.GetString(data , coffset , arraylen)));
                    } else // ANSI string
                    {
                        strings.Add(new(id+J, System.Text.Encoding.ASCII.GetString(data, coffset, arraylen)));
                    }
                    coffset += arraylen;
                }
            }
            blocks = null;
        }

        /// <summary>
        /// Gets an enumerable that iterates all the message table entries that exist in the current resource entry. <br />
        /// The key defines the resource ID and the value is the formatted message string.
        /// </summary>
        public IEnumerable<KeyValuePair<System.UInt32, System.String>> Entries => strings;

        /// <summary>
        /// Gets the number of messages that are contained in the current message table.
        /// </summary>
        public System.Int32 Count => strings.Count;
    }
}
