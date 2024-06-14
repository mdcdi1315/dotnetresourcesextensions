using System.Runtime.InteropServices;

namespace System.IO
{

	internal static class BinaryReaderExtensions
	{
		public static int Read7BitEncodedInt(this BinaryReader reader)
		{
			int num = 0;
			int num2 = 0;
			byte b;
			do
			{
				if (num2 == 35)
				{
					throw new FormatException(
						DotNetResourcesExtensions.Properties.Resources.Format_Bad7BitInt32);
				}
				b = reader.ReadByte();
				num |= (b & 0x7F) << num2;
				num2 += 7;
			} while ((b & 0x80u) != 0);
			return num;
		}
	}

    internal static class BinaryWriterExtensions
    {
        public static void Write7BitEncodedInt(this BinaryWriter writer, int value)
        {
            uint num;
            for (num = (uint)value; num >= 128; num >>= 7)
            {
                writer.Write((byte)(num | 0x80u));
            }
            writer.Write((byte)num);
        }
    }

    internal sealed class PinnedBufferMemoryStream : UnmanagedMemoryStream
    {
        private readonly byte[] _array;

        private GCHandle _pinningHandle;

        internal unsafe PinnedBufferMemoryStream(byte[] array)
        {
            _array = array;
            _pinningHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            int num = array.Length;
            fixed (byte* pointer = &MemoryMarshal.GetReference<byte>(array))
            {
                Initialize(pointer, num, num, FileAccess.Read);
            }
        }

        ~PinnedBufferMemoryStream()
        {
            Dispose(disposing: false);
        }

        protected override void Dispose(bool disposing)
        {
            if (_pinningHandle.IsAllocated)
            {
                _pinningHandle.Free();
            }
            base.Dispose(disposing);
        }
    }

}
