

using System.Collections.Generic;

namespace DotNetResourcesExtensions.Internal.CustomFormatter
{
    
    /// <summary>
    /// The <see cref="ITypeResolver"/> interface resolves a formatted type 
    /// and gets it's equal converter.
    /// </summary>
    public interface ITypeResolver
    {
        /// <summary>
        /// Gets a converter that can serialize the given type.
        /// </summary>
        /// <typeparam name="T">The type to search from the types that the <see cref="ITypeResolver"/> supports.</typeparam>
        /// <returns>The converter instance that can serialize the given type.</returns>
        public IArrayRepresentation<T> GetConverter<T>();

        /// <summary>
        /// Gets all the <see cref="System.Type.AssemblyQualifiedName"/>s that are supported for this type resolver.
        /// </summary>
        public IEnumerable<System.String> RegisteredQualifiedTypeNames { get; }
    }

    /// <summary>
    /// Contains helper methods for the <see cref="ITypeResolver"/> interface.
    /// </summary>
    public static class ITypeResolverExtensions
    {
        /// <summary>
        /// Gets a converter that can serialize the given type.
        /// </summary>
        /// <param name="resolver">The resolver instance.</param>
        /// <param name="type">The type to search from the types that the <see cref="ITypeResolver"/> supports.</param>
        /// <returns>The converter instance that can serialize the given type.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="type"/> was <see langword="null"/>. </exception>
        /// <exception cref="System.MissingMethodException">The <paramref name="resolver"/> has declared the <see cref="ITypeResolver.GetConverter{T}"/> method as non-public. <br />
        /// This method must be always public for this to work.</exception>
        public static System.Object GetConverter(this ITypeResolver resolver , System.Type type)
        {
            if (type is null) { throw new System.ArgumentNullException(nameof(type)); }
            System.Type t = resolver.GetType();
            System.Reflection.MethodInfo mi = (t.GetMethod("GetConverter")?.MakeGenericMethod(type)) 
            ?? throw new System.MissingMethodException($"The GetConverter method is missing from the current resolver. \nFailing Type: {t.AssemblyQualifiedName}");
            return mi.Invoke(resolver, System.Array.Empty<System.Object>()); // A bit hard to fail here.
        }

        /// <summary>
        /// Gets the converter type that can serialize the given type.
        /// </summary>
        /// <param name="resolver">The resolver instance.</param>
        /// <param name="type">The type to search from the types that the <see cref="ITypeResolver"/> supports.</param>
        /// <returns>The converter type that can serialize the given type.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="type"/> was <see langword="null"/>. </exception>
        /// <exception cref="System.MissingMethodException">The <paramref name="resolver"/> has declared the <see cref="ITypeResolver.GetConverter{T}"/> method as non-public. <br />
        /// This method must be always public for this to work.</exception>
        public static System.Type GetConverterType(this ITypeResolver resolver , System.Type type)
            => GetConverter(resolver , type).GetType();


    }

}