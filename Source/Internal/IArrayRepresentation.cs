
using System;

namespace DotNetResourcesExtensions.Internal.CustomFormatter
{
    /// <summary>
    /// <see cref="IArrayRepresentation{T}"/> represents an object that can be formatted by using this interface. <br />
    /// Classes that implement this are exposing two callable methods that do the transforming to byte arrays and the 
    /// reverse.
    /// </summary>
    /// <typeparam name="T">The type that can be convertible to an abstract byte array.</typeparam>
    public interface IArrayRepresentation<T> where T : notnull
    {
        /// <summary>
        /// <see cref="GetUntransformMethod"/> returns a method that can be used to transform a byte array into  <br />
        /// the original object of type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>A method handle.</returns>
        public System.Converter<System.Byte[], T> GetUntransformMethod();

        /// <summary>
        /// <see cref="GetTransformMethod"/> returns a method that can be used to transform an object of type <typeparamref name="T"/> into bytes.
        /// </summary>
        /// <returns>A method handle.</returns>
        public System.Converter<T, System.Byte[]> GetTransformMethod();

        /// <summary>
        /// The type of the object that the implementing class is able to convert.
        /// </summary>
        public System.Type OriginalType { get; }
    }

    internal abstract class DefaultArrayRepresentation<T> : IArrayRepresentation<T>
    {
        public Type OriginalType => typeof(T);

        public abstract Converter<T, byte[]> GetTransformMethod();

        public abstract Converter<byte[], T> GetUntransformMethod();
    }

    /// <summary>
    /// Provides extension methods for the <see cref="IArrayRepresentation{T}"/> interface.
    /// </summary>
    public static class IArrayRepresentationExtensions
    {
        internal const System.String DefaultNamespace = "DotNetResourcesExtensions.Internal.CustomFormatter";
        internal const System.String ArrReprString = "IArrayRepresentation`1";

        /// <summary>
        /// Gets an instance of the available converter for the specified type 
        /// that is available in the DotNetResourcesExtensions library.
        /// </summary>
        /// <typeparam name="T">The type of the object that is needed to be converted.</typeparam>
        /// <returns>An <see cref="IArrayRepresentation{T}"/> instance that has the specified converter.</returns>
        public static IArrayRepresentation<T> GetArrayRepresentation<T>() => GetArrayRepresentation<T>(typeof(T));

        /// <summary>
        /// Gets an instance of the available converter for the specified type 
        /// that is available in the DotNetResourcesExtensions library.
        /// </summary>
        /// <typeparam name="T">The type of the object that is needed to be converted.</typeparam>
        /// <param name="type">The type to perform the search for.</param>
        /// <returns>An <see cref="IArrayRepresentation{T}"/> instance that has the specified converter.</returns>
        public static IArrayRepresentation<T> GetArrayRepresentation<T>(this System.Type type) 
        {
            System.Type srchtype = type;
            IArrayRepresentation <T> inst;
            foreach (System.Type t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.FullName.StartsWith(DefaultNamespace))
                {
                    if (t.GetInterface(ArrReprString) == null) { continue; }
                    try
                    {
                        System.Reflection.ConstructorInfo ctorinf = t.GetConstructor(System.Type.EmptyTypes);
                        if (ctorinf == null) { continue; }
                        inst = (IArrayRepresentation<T>)ctorinf.Invoke(System.Array.Empty<System.Object>());
                    } catch (InvalidCastException) { continue; }
                    if (inst.OriginalType == srchtype) { return inst; } else { inst = null; continue; }
                }
            }
            return null;
        }

        /// <summary>
        /// Invokes the converter writer for the specified object. <br />
        /// Be noted that you need an instance of the <see cref="IArrayRepresentation{T}"/>
        /// for this to work.
        /// </summary>
        /// <typeparam name="T">The type of the object to be converted to a byte array.</typeparam>
        /// <param name="cnv">The instance of the <see cref="IArrayRepresentation{T}"/> interface.</param>
        /// <param name="cnvobject">The object instance to convert.</param>
        /// <returns>The converted object to bytes.</returns>
        public static System.Byte[] InvokeWriter<T>(this IArrayRepresentation<T> cnv, T cnvobject)
            => cnv.GetTransformMethod()(cnvobject);

        /// <summary>
        /// Invokes the converter reader for the specified byte array which contains the
        /// object information. <br /> Be noted that you need an instance of the 
        /// <see cref="IArrayRepresentation{T}"/> for this to work.
        /// </summary>
        /// <typeparam name="T">The type of the object to be converted back.</typeparam>
        /// <param name="cnv">The converter instance to use for reading the object.</param>
        /// <param name="bytes">The converted bytes.</param>
        /// <returns>The original or a pseudo-instance that is almost equal to the original object.</returns>
        public static T InvokeReader<T>(this IArrayRepresentation<T> cnv, System.Byte[] bytes)
            => cnv.GetUntransformMethod()(bytes);
    }

}
