
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DotNetResourcesExtensions.Internal.CustomFormatter
{

    /// <summary>
    /// Defines global formatter extensions for all the current interfaces under this namespace.
    /// </summary>
    public static class FormatterExtensions
    {

        /// <summary>
        /// Gets the required converter from the specified resolvers array.
        /// </summary>
        /// <typeparam name="T">The type to find it's corresponding converter.</typeparam>
        /// <typeparam name="T2">The available converter type.</typeparam>
        /// <param name="resolvers">The resolvers array to search the converter.</param>
        /// <returns>The converter that implements the <see cref="IArrayRepresentation{T}"/> interface.</returns>
        /// <exception cref="Exceptions.ConverterNotFoundException">The specified converter was not found.</exception>
        public static T2 GetConverterFromTypeResolvers<T , T2>(this IEnumerable<ITypeResolver> resolvers)
            where T2 : IArrayRepresentation<T>
        {
            T2 repr;
            System.Exception lastexception = null;
            foreach (ITypeResolver resolver in resolvers)
            {
                try
                {
                    repr = (T2)resolver.GetConverter<T>();
                    // BEWARE: we must handle the case that GetConverter is null.
                    // This happens mainly in GetDefaultArrayRepresentation , which is used by our basic type resolver , which apparently is
                    // returning null if it has not found the specified converter.
                    if (repr == null) { continue; }
                    return repr;
                } catch (Exceptions.ConverterNotFoundException e) { lastexception = e; }
            }
            if (lastexception != null) { throw lastexception; }
            else { throw new Exceptions.ConverterNotFoundException(typeof(T)); }
        }

        /// <summary>
        /// Gets the required converter from the specified resolvers array.
        /// </summary>
        /// <typeparam name="T">The type to find it's corresponding converter.</typeparam>
        /// <param name="resolvers">The resolvers array to search the converter.</param>
        /// <returns>The converter that implements the <see cref="IArrayRepresentation{T}"/> interface.</returns>
        /// <exception cref="Exceptions.ConverterNotFoundException">The specified converter was not found.</exception>
        public static IArrayRepresentation<T> GetConverterFromTypeResolvers<T>(this IEnumerable<ITypeResolver> resolvers)
            => GetConverterFromTypeResolvers<T, IArrayRepresentation<T>>(resolvers);

        /// <summary>
        /// Gets the default type resolver that is shipped with the DotNetResourcesExtensions library.
        /// </summary>
        /// <returns>The default type resolver.</returns>
        public static ITypeResolver GetDefaultTypeResolver() => new BasicFormatterTypeResolver();

        /// <summary>
        /// Constructs a new implementation of <see cref="ITypeResolver"/> interface by using the specified converter.
        /// </summary>
        /// <typeparam name="T">The type of the converter.</typeparam>
        /// <param name="converter">The converter instance.</param>
        /// <returns>A new <see cref="ITypeResolver"/> constructed from <paramref name="converter"/>.</returns>
        public static ITypeResolver ConstructTypeResolver<T>(this IArrayRepresentation<T> converter)
        {
            ConstructedTypeResolver ctr = new();
            ctr.Add(converter);
            return ctr;
        }

        /// <summary>
        /// Constructs a new implementation of <see cref="ITypeResolver"/> interface by using the specified converters. <br />
        /// The boxed objects must implement the <see cref="IArrayRepresentation{T}"/> interface!
        /// </summary>
        /// <param name="converters">The converter instances to use.</param>
        /// <returns>A new <see cref="ITypeResolver"/> constructed from <paramref name="converters"/>.</returns>
        public static ITypeResolver ConstructTypeResolver(params System.Object[] converters)
        {
            ConstructedTypeResolver ctr = new();
            foreach (System.Object obj in converters) { ctr.Add(obj); }
            return ctr;
        }

        /// <summary>
        /// Gets the specified <see cref="ITypeResolver"/> class as a type resolution service. <br />
        /// Note: The service returns the converter types that implement the <see cref="IArrayRepresentation{T}"/> interface.
        /// </summary>
        /// <param name="typeResolver">The <see cref="ITypeResolver"/> to convert.</param>
        /// <returns>A class that implements the <see cref="System.ComponentModel.Design.ITypeResolutionService"/> behavior.</returns>
        public static System.ComponentModel.Design.ITypeResolutionService AsTypeResolutionService(this ITypeResolver typeResolver)
            => new TypeResolverMarshaller(typeResolver);

        /// <summary>
        /// Gets an object from bytes but uses the <see cref=" System.ComponentModel.Design.ITypeResolutionService"/>
        /// to get the type from a plain <see cref="String"/>. <br />
        /// NOTE: See also the GetObjectFromBytes <see cref="GetObjectFromBytes(ICustomFormatter, byte[], System.ComponentModel.Design.ITypeResolutionService, string, bool, bool)">method</see>.
        /// </summary>
        /// <param name="formatter">The class that implements the <see cref="ICustomFormatter"/> interface.</param>
        /// <param name="bytes">The plain bytes to get the object from.</param>
        /// <param name="resolutionservice">The resolution service to use for type resolving.</param>
        /// <param name="typename">The type name as a <see cref="String"/> understanable from the <paramref name="resolutionservice"/>.</param>
        /// <returns>The deserialized object.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="formatter"/> , <paramref name="typename"/> , or <paramref name="bytes"/> were <see langword="null"/>.</exception>
        /// <exception cref="Exceptions.UnresolvedTypeFoundException">The type string given in <paramref name="typename"/> parameter was invalid.</exception>
        /// <exception cref="System.MemberAccessException">The formatter passed to the <paramref name="formatter"/> was not a valid instance of the <see cref="ICustomFormatter"/> interface.</exception>
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The type that is requested to be resolved might have not referenced by the executing assembly.")]
        public static System.Object GetObjectFromBytes(this ICustomFormatter formatter, System.Byte[] bytes,
            System.ComponentModel.Design.ITypeResolutionService resolutionservice, System.String typename)
            => GetObjectFromBytes(formatter, bytes, resolutionservice, typename, false , true);

        /// <summary>
        /// Gets an object from bytes but uses the <see cref="System.ComponentModel.Design.ITypeResolutionService"/>
        /// to get the type from a plain <see cref="String"/> with additional settings. <br />
        /// Additionally some more rules occur too: 
        /// <list type="number">
        ///     <item>Checks through the <paramref name="resolutionservice"/> for the specified type.</item>
        ///     <item>
        ///         If <paramref name="resolutionservice"/> is null or did not found the specified type (when <paramref name="throwonerror"/> is <see langword="false"/>) , 
        ///         then checks the type as a run-time type using the <see cref="System.Type.GetType(System.String,System.Boolean , System.Boolean)"/> method.
        ///     </item>
        ///     <item>If the above also failed , then it throws <see cref="Exceptions.UnresolvedTypeFoundException"/>.</item>
        /// </list>
        /// </summary>
        /// <param name="formatter">The class that implements the <see cref="ICustomFormatter"/> interface.</param>
        /// <param name="bytes">The plain bytes to get the object from.</param>
        /// <param name="resolutionservice">The resolution service to use for type resolving.</param>
        /// <param name="typename">The type name as a <see cref="String"/> understanable from the <paramref name="resolutionservice"/>.</param>
        /// <param name="ignorecase"><see langword="true"/> to ignore case when searching for types; otherwise, <see langword="false"/>.</param>
        /// <param name="throwonerror"><see langword="true"/> if this method should throw an exception if the assembly cannot be located; <br />
        /// otherwise, <see langword="false"/>, and this method returns null if the assembly cannot be located.</param>
        /// <returns>The deserialized object.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="formatter"/> , <paramref name="typename"/> , or <paramref name="bytes"/> were <see langword="null"/>.</exception>
        /// <exception cref="Exceptions.UnresolvedTypeFoundException">The type string given in <paramref name="typename"/> parameter was invalid.</exception>
        /// <exception cref="System.MemberAccessException">The formatter passed to the <paramref name="formatter"/> was not a valid instance of the <see cref="ICustomFormatter"/> interface.</exception>
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The type that is requested to be resolved might have not referenced by the executing assembly.")]
        public static System.Object GetObjectFromBytes(this ICustomFormatter formatter, System.Byte[] bytes,
            System.ComponentModel.Design.ITypeResolutionService resolutionservice, System.String typename ,
            System.Boolean throwonerror , System.Boolean ignorecase)
        {
            if (formatter is null) { throw new ArgumentNullException(nameof(formatter)); }
            System.Type ft = resolutionservice?.GetType(typename, throwonerror, ignorecase);
            ft ??= Type.GetType(typename, throwonerror, ignorecase);
            if (ft is null) { throw new Exceptions.UnresolvedTypeFoundException(typename); }
            return formatter.GetObjectFromBytes(bytes, ft);
        }
    }

    internal sealed class TypeResolverMarshaller : System.ComponentModel.Design.ITypeResolutionService
    {
        private ITypeResolver resolver;

        public TypeResolverMarshaller(ITypeResolver res) => resolver = res;

        public Assembly GetAssembly(AssemblyName name)
        {
            throw new NotSupportedException("Not required for ITypeResolver classes");
        }

        public Assembly GetAssembly(AssemblyName name, bool throwOnError)
        {
            throw new NotSupportedException("Not required for ITypeResolver classes");
        }

        public string GetPathOfAssembly(AssemblyName name)
        {
            throw new NotSupportedException("Not required for ITypeResolver classes");
        }

        public Type GetType(string name)
            => GetType(name, false);

        public Type GetType(string name, bool throwOnError)
            => GetType(name, throwOnError, false);

        public Type GetType(string name, bool throwOnError, bool ignoreCase)
        {
            System.Type fd = resolver.GetConverterType(System.Type.GetType(name , throwOnError , ignoreCase));
            return fd;
        }

        public void ReferenceAssembly(AssemblyName name)
        {
            throw new NotSupportedException("Not required for ITypeResolver classes");
        }
    }


}