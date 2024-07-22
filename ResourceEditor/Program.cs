using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResourceEditor
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ResourceEditor.Main());
        }
    }

    internal static class Helper
    {
        private const System.Int32 BUFSIZE = 4096;
        public const System.String InvalidNameCharacters = "!@#$%^&*()`~,./?\\|\"\';:{}[]+=©§¥¢ž®¯";
        public const System.String InvalidFirstCharacterNameCharacters = "1234567890!@#$%^&*()`~,./?\\|\"';:{}[]+=-_©§¥¢ž®¯";


        public static void ShowErrorMessage(System.String msg)
        {
            MessageBox.Show(msg , "Error" , MessageBoxButtons.OK , MessageBoxIcon.Stop);
        }

        public static System.Boolean ShowQuestionMessage(System.String msg) 
        {
            return MessageBox.Show(msg, "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>
        /// Tests if the resource name given is valid.
        /// </summary>
        /// <param name="Name">The resource name to test.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        [System.Diagnostics.DebuggerHidden]
        public static void ValidateName(System.String Name)
        {
            if (Name == null || System.String.IsNullOrEmpty(Name))
            {
                throw new ArgumentNullException("Name");
            }

            if (Name.Length > 530)
            {
                throw new ArgumentException("A resource name must not have more than 530 characters.");
            }

            for (System.Int32 J = 0; J < InvalidFirstCharacterNameCharacters.Length; J++)
            {
                if (Name[0] == InvalidFirstCharacterNameCharacters[J])
                {
                    throw new ArgumentException($"The first character of a resource name must not have all the following characters: {InvalidFirstCharacterNameCharacters}.", "Name");
                }
            }

            for (System.Int32 I = 1; I < Name.Length; I++)
            {
                for (System.Int32 J = 0; J < InvalidNameCharacters.Length; J++)
                {
                    if (Name[I] == InvalidNameCharacters[J])
                    {
                        throw new ArgumentException($"A resource name must not have all the following characters: {InvalidNameCharacters}.", "Name");
                    }
                }
            }
        }

        public static System.Object ParseAsNumericValue(System.String Value)
        {
            System.Object ret = null;
            try { ret = System.Byte.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.Int16.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.UInt16.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.Int32.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.UInt32.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.Int64.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.UInt64.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            try { ret = System.Single.Parse(Value, System.Globalization.NumberStyles.Integer); goto G_Ret; } catch { }
            // The last try statement does not need goto since it will unconditionally go there.
            try { ret = System.Double.Parse(Value, System.Globalization.NumberStyles.Integer); } catch { }
            G_Ret:
            if (ret == null) { throw new FormatException("Could not parse the number. Maybe the string input is not a number?"); }
            return ret;
        }

        public static System.Byte[] ReadBuffered(System.IO.Stream Stream, System.Int64 RequestedBytes)
        {
            // Check for null conditions or whether we can read from this stream
            if (Stream == null) { return null; }
            if (Stream.CanRead == false) { return null; }
            // Create a new byte array with the requested size.
            System.Byte[] Contents = new System.Byte[RequestedBytes];
            if (RequestedBytes <= BUFSIZE)
            {
                // Read all bytes directly , if the requested bytes are less than the buffer limit.
                // Otherwise we don't care here; we do not read thousands or millions of bytes.
                Stream.Read(Contents, 0, Contents.Length);
            }
            else
            {
                System.Int32 Count;
                System.Int32 Offset = 0;
                // Read all bytes with buffered mode.
                do
                {
                    Count = Stream.Read(Contents, Offset, BUFSIZE);
                    Offset += BUFSIZE;
                    // Condition specifies that the loop will continue to run when the read bytes are
                    // more or equal than the buffer limit , plus make sure that the next read will not
                    // surpass the bytes that the final array can hold.
                } while ((Count >= BUFSIZE) && (Offset + BUFSIZE <= Contents.Length));
                // In case that the bytes were surpassed in the above condition , pass all the rest bytes again normally.
                if (Contents.Length - Offset > 0) { Stream.Read(Contents, Offset, Contents.Length - Offset); }
            }
            return Contents;
        }
    }
}
