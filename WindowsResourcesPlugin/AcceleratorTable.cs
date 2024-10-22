using System;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Reads an accelerator table that is saved on a native resource entry. <br />
    /// A single accelerator entry acts as a shortcut in application UI's. <br />
    /// An example of an accelerator is the copy file operation in Windows Explorer which is performed with Ctrl+C.
    /// </summary>
    public sealed class AcceleratorTable
    {
        private List<Entry> entries;
        private System.IntPtr handle;
        private System.Boolean iserr;

        /// <summary>
        /// Defines an entry in this accelerator table.
        /// </summary>
        public sealed class Entry
        {
            private readonly System.Int32 flags;
            private readonly System.Char key;
            private readonly System.UInt16 id;

            internal Entry(int flags, char key, ushort id)
            {
                this.flags = flags;
                this.key = key;
                this.id = id;
            }

            /// <summary>
            /// Gets the virtual key flags that must be also used in order this accelerator entry to be activated.
            /// </summary>
            public System.Int32 VirtualKeyFlags => flags;

            /// <summary>
            /// Gets the control key as a character.
            /// </summary>
            public System.Char ControlKey => key;

            /// <summary>
            /// Gets the acclerator entry ID that is known when a Windows message has recieved an accelerator assert.
            /// </summary>
            public System.UInt16 ModifierID => id;
        }

        private AcceleratorTable()
        {
            handle = IntPtr.Zero;
            iserr = false;
            entries = new(8);
        }

        /// <summary>
        /// Creates a new accelerator table instance from the specified native entry.
        /// </summary>
        /// <param name="entry">The resource entry to read the accelerator table from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> was null.</exception>
        /// <exception cref="ArgumentException">The <see cref="NativeWindowsResourceEntry.NativeType"/> 
        /// property of <paramref name="entry"/> was not <see cref="WindowsResourceEntryType.RT_ACCELERATOR"/>.</exception>
        public AcceleratorTable(NativeWindowsResourceEntry entry) : this()
        {
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            if (entry.NativeType != WindowsResourceEntryType.RT_ACCELERATOR) {
                throw new ArgumentException("The entry's native type property must be RT_ACCELERATOR.");
            }
            GetTable(entry.Value);
        }

        private void GetTable(System.Byte[] raw)
        {
            List<Interop.ACCELTABLEENTRY> ents = new(8);
            Interop.ACCELTABLEENTRY temp;
            System.Int32 idx = 0;
            while (idx < raw.Length)
            {
                ents.Add(temp = Interop.ACCELTABLEENTRY.ReadFromArray(raw, idx));
                entries.Add(new((System.Int32)temp.FVirtual , temp.KeyCode , temp.IdOrCommand));
                idx += temp.Padding + 8;
            }
            if (Interop.ApisSupported())
            {
                Interop.ACCEL[] accelerators = new Interop.ACCEL[ents.Count];
                for (System.Int32 I = 0; I < accelerators.Length; I++)
                {
                    accelerators[I] = ents[I].ToAccelerator();
                }
                if ((handle = Interop.User32.CreateAcceleratorTable(accelerators)) == IntPtr.Zero) 
                {
                    handle = new(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                    iserr = true;
                }
                accelerators = null;
            }
            ents.Clear();
            ents = null;
        }

        /// <summary>
        /// Gets an enumerable of entries that comprise this accelerator table.
        /// </summary>
        public IEnumerable<Entry> Entries => entries;

        /// <summary>
        /// Gets a native handle to this accelerator table. If this accelerator table falls out of scope , it is then disposed.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">This API was invoked in other platform than Windows.</exception>
        public System.IntPtr Handle
        {
            get {
                if (Interop.ApisSupported() == false)
                {
                    throw new PlatformNotSupportedException("The handle property can be only instantiated from Windows.");
                }
                if (iserr) {
                    // If the accelerator table creation failed , it is thrown here by using the handle.
                    throw new System.ComponentModel.Win32Exception(handle.ToInt32());
                }
                return handle;
            }
        }

        /// <summary>
        /// Gets the number of the accelerator entries contained in the <see cref="Entries"/> property.
        /// </summary>
        public System.Int32 Count => entries.Count;

        /// <summary>
        /// Destroys this <see cref="AcceleratorTable"/> object.
        /// </summary>
        ~AcceleratorTable() { 
            if (handle != System.IntPtr.Zero && Interop.ApisSupported() && iserr == false)
            {
                Interop.User32.DestroyAcceleratorTable(handle);
                handle = System.IntPtr.Zero;
            }
            entries?.Clear();
            entries = null;
        }
    }
}
