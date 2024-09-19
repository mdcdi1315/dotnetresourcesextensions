
using System;

namespace DotNetResourcesExtensions.Internal.CustomFormatter
{

    /// <summary>
    /// The <see cref="ExtensibleFormatter"/> class is a custom , easy to use and flexible formatter 
    /// for your resource applications. <br />
    /// Although it only by default it provides converters for the well-known BCL types , you can
    /// add more types to be used as converters through the <see cref="BaseFormatter.RegisterTypeResolver(ITypeResolver)"/> method. <br />
    /// Additionally , you can listen to the <see cref="BaseFormatter.OnTypeNotFound"/> event and provide from there a converter if you want; <br />
    /// For more information about this event , see the <see cref="TypeNotFoundEventHandler"/> <see langword="delegate"/>.
    /// </summary>
    public sealed class ExtensibleFormatter : BaseFormatter
    {
        /// <summary>
        /// Creates a default instance of <see cref="ExtensibleFormatter"/>.
        /// </summary>
        public ExtensibleFormatter() : base()
        {
            Resolvers.Add(FormatterExtensions.GetDefaultTypeResolver());
        }

        /// <summary>
        /// Creates and returns a default instance of <see cref="ExtensibleFormatter"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="ExtensibleFormatter"/> class.</returns>
        public static ExtensibleFormatter Create() => new();

        /// <summary>
        /// Creates and returns a new instance of <see cref="ExtensibleFormatter"/>
        /// with the specified type resolvers. <br />
        /// If the <paramref name="resolvers"/> parameter did not had any resolvers (empty array) , it returns a 
        /// default <see cref="ExtensibleFormatter"/> , just like it was created from <see cref="Create"/>.
        /// </summary>
        /// <param name="resolvers">The resolvers to add during instance creation.</param>
        /// <returns>A new instance of <see cref="ExtensibleFormatter"/> class.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="resolvers"/> was either null or no resolvers provided (Empty array).</exception>
        /// <exception cref="Exceptions.ConfilctingConverterException">A converter was about to be registered 
        /// although that another registered resolver can convert the specified type.</exception>
        public static ExtensibleFormatter FromTypeResolvers(params ITypeResolver[] resolvers)
        {
            if (resolvers == null) { throw new ArgumentNullException(nameof(resolvers)); }
            ExtensibleFormatter result = Create();
            foreach (ITypeResolver resolver in resolvers) { result.RegisterTypeResolver(resolver); }
            return result;
        }

        /// <inheritdoc />
        [System.Diagnostics.DebuggerHidden] // Hidden so to be not visible in the exception stack trace.
        protected override IArrayRepresentation<T> Resolve<T>()
        {
            IArrayRepresentation<T> repr;
            // Attempt to get our type to be serialized/deserialized using the already defined
            // resolvers.
            foreach (ITypeResolver resolver in Resolvers)
            {
                repr = resolver.GetConverter<T>();
                // BEWARE: we must handle the case that GetConverter is null.
                // This happens mainly in GetDefaultArrayRepresentation , which is used by our basic type resolver , which apparently is
                // returning null if it has not found the specified converter.
                if (repr is null) { continue; }
                return repr;
            }
            return null;
        }

        /// <summary>
        /// Disposes the current <see cref="ExtensibleFormatter"/> instance.
        /// </summary>
        public override void Dispose() {
            GC.SuppressFinalize(this);
            base.Dispose();
        }

        /// <summary>
        /// Guard so as to ensure that all resources are immediately released.
        /// </summary>
        ~ExtensibleFormatter() { Dispose(); }
    }


}
