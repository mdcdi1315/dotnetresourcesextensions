namespace DotNetResourcesExtensions.Internal
{
    internal unsafe partial class UnsafeMethods
    {
        /// <summary>
        /// Gets an instance of the <see cref="DotNetResourcesExtensions"/> project internal base-64 encoding implementation.
        /// </summary>
        public static System.Text.Encoding Base64Internal => Base64Encoding.Singleton;

        /// <summary>
        /// Converts the given byte array to a base64 sequence.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>The base64 data that are equivalent to <paramref name="bytes"/>.</returns>
        // Note that the base64 code implementation uses unsafe code , so it is okay to include it here.
        public static System.String ToBase64(this System.Byte[] bytes) => Base64Encoding.Singleton.GetString(bytes);

        /// <summary>
        /// Converts the given byte array to a base64 sequence.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>The base64 data that are equivalent to <paramref name="bytes"/>.</returns>
        /// <param name="startindex">The index to start computing the base64 information.</param>
        /// <param name="count">The elements to produce the base64 equivalent data.</param>
        public static System.String ToBase64Selected(this System.Byte[] bytes, System.Int32 startindex, System.Int32 count) => Base64Encoding.Singleton.GetString(bytes, startindex, count);

        /// <summary>
        /// Converts the given base64 string to the equivalent byte array representation.
        /// </summary>
        /// <param name="base64">The base64 string to convert.</param>
        /// <returns>The decoded byte information.</returns>
        public static System.Byte[] FromBase64(this System.String base64) => Base64Encoding.Singleton.GetBytes(base64, 0, base64.Length);

        /// <summary>
        /// Converts the given base64 string to the equivalent byte array representation.
        /// </summary>
        /// <param name="base64">The base64 string to convert.</param>
        /// <param name="index">The character index inside <paramref name="base64"/> to start decoding from.</param>
        /// <param name="count">The number of characters to decode.</param>
        /// <returns>The decoded byte information.</returns>
        public static System.Byte[] FromBase64Selected(this System.String base64, System.Int32 index, System.Int32 count)
        => Base64Encoding.Singleton.GetBytes(base64, index, count);
    }
}
