
using System;
using System.Linq;
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Internal.CustomFormatter
{

    /// <summary>
    /// Represents a base custom formatter to use. Inherit it in your class , specify your resolve method and use it.
    /// </summary>
    public abstract class BaseFormatter : ICustomFormatter
    {
        /// <summary>
        /// The current <see cref="ITypeResolver"/>s bound to the current instance. <br />
        /// Use this field in conjuction with the <see cref="Resolve{T}"/> method so as to provide type resolver services.
        /// </summary>
        protected List<ITypeResolver> Resolvers;
        private List<TypeNotFoundEventHandler> TypeNotFoundEventHandlers;

        /// <summary>
        /// Creates a default instance of <see cref="BaseFormatter"/> class. Must be called from your inheriting code.
        /// </summary>
        [System.Security.SecuritySafeCritical] // Might be dangerous in implicit instantiations , but for simple cases this is ok.
        protected BaseFormatter()
        {
            Resolvers = new();
            TypeNotFoundEventHandlers = new();
        }

        [System.Security.SecurityCritical] // It is possible this to be dangerous for the runtime itself.
        [System.Diagnostics.DebuggerHidden]
        [System.Diagnostics.DebuggerStepThrough]
        private IArrayRepresentation<T> ResolveFromTypeHandlers<T>()
        {
            IArrayRepresentation<T> repr;
            System.Boolean succ = false;
            foreach (TypeNotFoundEventHandler handler in TypeNotFoundEventHandlers)
            {
                try
                {
                    succ = false;
                    System.Type decl = handler(typeof(T));
                    // Default behavior as defined in doc: If the Handler returns null , it means that it not had found a type.
                    if (decl == null) { continue; }
                    // Do only set it if it is to test the type instance.
                    succ = true;
                    repr = (IArrayRepresentation<T>)Activator.CreateInstance(decl, null);
                    return repr;
                } catch (InvalidCastException) when (succ) {
                    continue;
                } catch (Exception ex) {
                    throw new AggregateException(
                        "A Type Not Found Event Handler has thrown an exception. This is unexpected and should not occur at normal cases.", ex);
                }
            }
            throw new Exceptions.ConverterNotFoundException(typeof(T));
        }

        [System.Security.SecurityCritical] // It is possible this to be dangerous for the runtime itself.
        [System.Diagnostics.DebuggerHidden]
        [System.Diagnostics.DebuggerStepThrough]
        private IArrayRepresentation<T> ResolveInternal<T>()
        {
            IArrayRepresentation<T> result = Resolve<T>();
            if (result == null) { result = ResolveFromTypeHandlers<T>(); }
            // Although difficult for this statement to stand true , we must be assured of the result that we get.
            if (result == null) { throw new Exceptions.ConverterNotFoundException(typeof(T)); }
            return result;
        }

        /// <inheritdoc />
        public event TypeNotFoundEventHandler OnTypeNotFound
        {
            add { TypeNotFoundEventHandlers.Add(value); }
            remove { TypeNotFoundEventHandlers.Remove(value); }
        }

        /// <inheritdoc />
        public virtual byte[] GetBytes<T>(T obj) where T : notnull
        {
            IArrayRepresentation<T> apr = ResolveInternal<T>();
            return apr.GetTransformMethod()(obj);
        }

        /// <inheritdoc />
        public virtual T GetObject<T>(byte[] typedarray) where T : notnull
        {
            IArrayRepresentation<T> apr = ResolveInternal<T>();
            return apr.GetUntransformMethod()(typedarray);
        }

        /// <summary>
        /// Registers the specified type resolver to the formatter instance. <br />
        /// <see cref="BaseFormatter"/> can have more than one type resolvers!
        /// </summary>
        /// <param name="resolver">The resolver instance to bind to the formatter.</param>
        /// <exception cref="Exceptions.InvalidResolverLayoutException">The specified resolver does not satisfy the minimum required criteria.</exception>
        /// <exception cref="Exceptions.ConfilctingConverterException">The resolver defines a converter that has already defined by another resolver.</exception>
        public void RegisterTypeResolver(ITypeResolver resolver)
        {
            List<System.String> strings = new();
            foreach (System.String s in resolver.RegisteredQualifiedTypeNames)
            {
                if (System.Type.GetType(s, false, false) == null)
                {
                    throw new Exceptions.InvalidResolverLayoutException("All types defined within the resolver must be existing statically-defined types.");
                }
                strings.Add(s);
            }
            if (strings.Count == 0) { throw new Exceptions.InvalidResolverLayoutException("The current resolver does not have registered any supported types."); }
            System.String[] qfnstrings;
            foreach (ITypeResolver resv in Resolvers)
            {
                qfnstrings = resv.RegisteredQualifiedTypeNames.ToArray();
                for (System.Int32 I = 0; I < strings.Count && I < qfnstrings.Length; I++)
                {
                    if (qfnstrings[I] == strings[I])
                    {
                        System.Type rtype = System.Type.GetType(strings[I]);
                        throw new Exceptions.ConfilctingConverterException(rtype,
                            resv.GetConverterType(rtype),
                            resolver.GetConverterType(rtype));
                    }
                }
            }
            strings.Clear();
            strings = null;
            Resolvers.Add(resolver);
        }

        /// <summary>
        /// Resolves the given type that your own custom formatter uses for serialization-deserialization. <br />
        /// Different custom formatters can have different behavior , based 
        /// on it's usage and developer desire.
        /// </summary>
        /// <remarks>
        /// If you want to specify that you have not found a type using your own algorithm , then return <see langword="null" />. <br />
        /// If that happens , then the mechanism will search for any in the <see cref="OnTypeNotFound"/> event. <br />
        /// This behavior cannot be overriden. <br />
        /// Also , you should use the <see cref="Resolvers"/> list so as to get a list of the current registered resolvers and use them so as 
        /// to get the required results.
        /// </remarks>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>The resolved <see cref="IArrayRepresentation{T}"/> instance that is able to serialize-deserialize the specified type.</returns>
        /// <exception cref="Exceptions.ConverterNotFoundException">Must be thrown in cases where the current method failed to produce results.</exception>
#if DEBUG == false
        [System.Diagnostics.DebuggerStepThrough] // Stepping through the code is required for Release builds so as to not generate long stack traces.
#endif
        protected abstract IArrayRepresentation<T> Resolve<T>();

        /// <summary>
        /// Disposes the <see cref="BaseFormatter"/> instance. NOTE: You must call this last if you override this method! <br />
        /// Failing to ensure that might lead you to early instance disposition.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", 
            "CA1816:Dispose methods should call SuppressFinalize", 
            Justification = "It is unsafe to leave it stale if the user does not know how and when to dispose this class.")]
        public virtual void Dispose()
        {
            Resolvers?.Clear();
            Resolvers = null;
            TypeNotFoundEventHandlers?.Clear();
            TypeNotFoundEventHandlers = null;
        }

        /// <summary>
        /// Guard so as to ensure that all resources are immediately released.
        /// </summary>
        ~BaseFormatter() { Dispose(); }
    }
}
