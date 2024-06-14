
namespace DotNetResourcesExtensions.Internal.ResX
{
    /// <summary>
    /// Emulates the Point structure for the frameworks that do not support the System.Drawing namespace.
    /// </summary>
    public struct Point
    {
        private System.Int32 x , y;

        /// <summary>
        /// Creates a new instance of the <see cref="Point"/> structure.
        /// </summary>
        public Point() { x = 0; y = 0; }

        /// <summary>
        /// Gets or sets the X-coordinate.
        /// </summary>
        public System.Int32 X { readonly get { return x; } set { x = value; } }

        /// <summary>
        /// Gets or sets the Y-coordinate.
        /// </summary>
        public System.Int32 Y { readonly get { return y; } set { y = value; } }
    }
}
