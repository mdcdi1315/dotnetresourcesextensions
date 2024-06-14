

namespace DotNetResourcesExtensions.Internal.CustomFormatter
{
    /// <summary>
    /// The <see cref="ICustomFormatter"/> interface defines a custom formatter that 
    /// can SAFELY serialize and deserialize objects. <br />
    /// This formatter: 
    /// <list type="bullet">
    ///     <item>Cannot work outside the process boundaries.</item>
    ///     <item>Cannot serialize the entire object but only it's public API.</item>
    ///     <item>Is designed with both a generic and abstract object approaches.</item>
    ///     <item>Gives the chance to the implementers to be extended.</item>
    ///     <item>Needs to be disposed so as to safely destroy any references to the formatter.</item>
    ///     <item>Is used by the Custom ResX Classes.</item>
    ///     <item>Is as simple as possible.</item>
    ///     <item>Does not have any knowledge of serializing objects. You must implement the serialization methods.</item>
    ///     <item>Can have many de/serializers , based on the type resolvers that it has available.</item>
    /// </list>
    /// </summary>
    public interface ICustomFormatter : System.IDisposable
    {
        /// <summary>
        /// Gets the serialized byte array for the specified object.
        /// </summary>
        /// <typeparam name="TS">The object type to serialize.</typeparam>
        /// <param name="obj">The object instance to serialize.</param>
        /// <returns>The serialized object in bytes.</returns>
        public System.Byte[] GetBytes<TS>(TS obj) where TS : notnull;

        /// <summary>
        /// Gets the specified object instance from the specified typed byte array.
        /// </summary>
        /// <typeparam name="TD">The final object type.</typeparam>
        /// <param name="typedarray">The byte array to get the serialized data from.</param>
        /// <returns>The deserialized object.</returns>
        public TD GetObject<TD>(System.Byte[] typedarray) where TD : notnull;

        /// <summary>
        /// Registers the specified type resolver to the formatter instance. <br />
        /// Formatter instances can have more than one type resolvers!
        /// </summary>
        /// <param name="resolver">The resolver instance to bind to the formatter.</param>
        /// <exception cref="Exceptions.ConfilctingConverterException">A converter was about to be registered 
        /// although that another registered resolver can convert the specified type.</exception>
        public void RegisterTypeResolver(ITypeResolver resolver);

        /// <summary>
        /// This event is raised when the registered type resolvers did not had any converters
        /// to deserialize/serialize for the given object. <br /> 
        /// You can through this event to provide a <see cref="System.Type"/> that it can serialize and deserialize the
        /// object for which the serialization/deserialization requested. <br />
        /// If you do not want to provide a type or you did not found one if you listen to this event , return <see langword="null"/>.
        /// </summary>
        event TypeNotFoundEventHandler OnTypeNotFound;
    }

    /// <summary>
    /// The <see cref="TypeNotFoundEventHandler"/> <see langword="delegate"/> 
    /// handles cases where the registered type resolvers in a <see cref="ICustomFormatter"/>-implementing 
    /// interface instance have failed for the given type to find a converter.
    /// </summary>
    /// <param name="RequestedType">The object type which it needs a valid converter for it.</param>
    /// <returns>A <see cref="System.Type"/> that must implement the <see cref="IArrayRepresentation{T}"/> interface , 
    /// and can serialize/deserialize the object.</returns>
    /// <remarks>When you want to specify that you did not found a type , return <see langword="null"/> instead of throwing any exceptions.<br />
    /// Throwing exceptions from this <see langword="delegate"/> should throw <see cref="System.AggregateException"/> back to the caller.</remarks>
    public delegate System.Type TypeNotFoundEventHandler(System.Type RequestedType);

    /// <summary>
    /// Provides extension methods for the <see cref="ICustomFormatter"/> interface.
    /// </summary>
    public static class ICustomFormatterExtensions
    {
        /// <summary>
        /// Gets the serialized byte array for the specified object.
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="obj">The object instance to serialize.</param>
        /// <returns>The serialized object in bytes.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="obj"/> was null.</exception>
        /// <exception cref="System.MemberAccessException">The formatter passed to the <paramref name="formatter"/> was not a valid instance of the ICustomFormatter.</exception>
        public static System.Byte[] GetBytesFromObject(this ICustomFormatter formatter, System.Object obj)
        {
            if (obj is null) { throw new System.ArgumentNullException(nameof(obj)); }
            System.Type fr = formatter.GetType();
            if (fr.Name == nameof(ICustomFormatter)) {
                throw new System.MemberAccessException("The instance of ICustomFormatter interface is required , not the interface itself.");
            }
            System.Reflection.MethodInfo mi = fr.GetMethod("GetBytes").MakeGenericMethod(obj.GetType());
            try
            {
                return (System.Byte[])mi.Invoke(formatter, new System.Object[] { obj });
            } catch (System.Reflection.TargetInvocationException ex)
            {
                throw ex.InnerException ?? new System.AggregateException("Unknown exception occured when the formatter was called.");
            }
        }

        /// <summary>
        /// Gets the specified object instance from the specified typed byte array. <br />
        /// Note: For this to work , you must supply the correct type at the <paramref name="runtimetype"/>
        /// parameter.
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="typedarray">The byte array to get the serialized data from.</param>
        /// <param name="runtimetype">The resulting object type that will be finally transformed.</param>
        /// <returns>The deserialized object.</returns>
        /// <exception cref="System.ArgumentNullException">The <paramref name="runtimetype"/> or the <paramref name="typedarray"/> arguments were null.</exception>
        /// <exception cref="System.MemberAccessException">The formatter passed to the <paramref name="formatter"/> was not a valid instance of the ICustomFormatter.</exception>
        public static System.Object GetObjectFromBytes(this ICustomFormatter formatter , 
            System.Byte[] typedarray , System.Type runtimetype)
        {
            if (runtimetype is null) { throw new System.ArgumentNullException(nameof(runtimetype)); }
            System.Type fr = formatter.GetType();
            if (fr.Name == nameof(ICustomFormatter)) {
                throw new System.MemberAccessException("The instance of ICustomFormatter interface is required , not the interface itself.");
            }
            System.Reflection.MethodInfo mi = fr.GetMethod("GetObject").MakeGenericMethod(runtimetype);
            try
            {
                return mi.Invoke(formatter, new System.Object[] { typedarray });
            } catch (System.Reflection.TargetInvocationException ex)
            {
                throw ex.InnerException ?? new System.AggregateException("Unknown exception occured when the formatter was called.");
            }
        }
    }
}