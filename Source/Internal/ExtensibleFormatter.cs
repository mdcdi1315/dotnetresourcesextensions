
using System;
using System.Linq;
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Internal.CustomFormatter
{

    /// <summary>
    /// The <see cref="ExtensibleFormatter"/> class is a custom , easy to use and flexible formatter 
    /// for your resource applications. <br />
    /// Although it only by default it provides converters for the well-known BCL types , you can
    /// add more types to be used as converters through the <see cref="RegisterTypeResolver(ITypeResolver)"/> method. <br />
    /// Additionally , you can listen to the <see cref="OnTypeNotFound"/> event and provide from there a converter if you want; <br />
    /// For more information about this event , see the <see cref="TypeNotFoundEventHandler"/> <see langword="delegate"/>.
    /// </summary>
    public sealed class ExtensibleFormatter : ICustomFormatter
    {
        private List<ITypeResolver> resolvers;
        private List<TypeNotFoundEventHandler> typenotfoundeventhandlers;

        /// <summary>
        /// Creates a default instance of <see cref="ExtensibleFormatter"/>.
        /// </summary>
        public ExtensibleFormatter()
        {
            resolvers = new List<ITypeResolver>() { new BasicFormatterTypeResolver() };
            typenotfoundeventhandlers = new List<TypeNotFoundEventHandler>();
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

        [System.Diagnostics.DebuggerHidden] // Hidden so to be not visible in the exception stack trace.
        private IArrayRepresentation<T> ForEachTypeResolver<T>()
        {
            IArrayRepresentation<T> repr;
            // Attempt to get our type to be serialized/deserialized using the already defined
            // resolvers.
            foreach (ITypeResolver resolver in resolvers)
            {
                try
                {
                    repr = resolver.GetConverter<T>();
                    // BEWARE: we must handle the case that GetConverter is null.
                    // This happens mainly in GetDefaultArrayRepresentation , which is used by our basic type resolver , which apparently is
                    // returning null if it has not found the specified converter.
                    if (repr is null) { continue; }
                    return repr;
                } catch (Exceptions.ConverterNotFoundException) { }
            }
            // If our type was not found in the converter list , test if all event handlers in typenotfoundeventhandlers
            // have returned a converter for us.
            // This success variable successfully detects whether the InvalidCastException is coming from the method
            // or it comes from the cast done when the instance was created.
            // If it comes from the method , it will be normally routed to the AggregateException and fed there.
            System.Boolean succ = false;
            foreach (TypeNotFoundEventHandler handler in typenotfoundeventhandlers)
            {
                try
                {
                    succ = false;
                    System.Type decl = handler(typeof(T));
                    // Default behavior as defined in doc: If the Handler returns null , it means that it not had found a type.
                    if (decl == null) { continue; }
                    // Do only set it if it is to test the type instance.
                    succ = true;
                    repr = (IArrayRepresentation<T>)Activator.CreateInstance(decl , null);
                    return repr;
                } catch (InvalidCastException) when (succ)
                {
                    continue;
                } catch (Exception ex)
                {
                    throw new AggregateException(
                        "A Type Not Found Event Handler has thrown an exception. This is unexpected and should not occur at normal cases." , ex);
                }
            }
            throw new Exceptions.ConverterNotFoundException(typeof(T));
        }

        /// <inheritdoc />
        public byte[] GetBytes<T>(T obj) where T : notnull
        {
            IArrayRepresentation<T> ret = ForEachTypeResolver<T>();
            return ret.InvokeWriter(obj);
        }

        /// <inheritdoc />
        /// <remarks>Note: You must also supply the correct resulting object type for this to work!</remarks>
        public T GetObject<T>(byte[] typedarray) where T : notnull
        {
            IArrayRepresentation<T> ret = ForEachTypeResolver<T>();
            return ret.InvokeReader(typedarray);
        }

        /// <summary>
        /// Registers the specified type resolver to the formatter instance. <br />
        /// <see cref="ExtensibleFormatter"/> can have more than one type resolvers!
        /// </summary>
        /// <param name="resolver">The resolver instance to bind to the formatter.</param>
        /// <exception cref="Exceptions.InvalidResolverLayoutException">The specified resolver does not satisfy the minimum required criteria.</exception>
        /// <exception cref="Exceptions.ConfilctingConverterException">The resolver defines a converter that has already defined by another resolver.</exception>
        public void RegisterTypeResolver(ITypeResolver resolver)
        {
            List<System.String> strings = new();
            foreach (System.String s in resolver.RegisteredQualifiedTypeNames)
            {
                if (System.Type.GetType(s , false , false) == null)
                {
                    throw new Exceptions.InvalidResolverLayoutException("All types defined within the resolver must be existing static types.");
                }
                strings.Add(s);
            }
            if (strings.Count == 0) { throw new Exceptions.InvalidResolverLayoutException("The current resolver does not have registered any supported types."); }
            System.String[] qfnstrings;
            foreach (ITypeResolver resv in resolvers)
            {
                qfnstrings = resv.RegisteredQualifiedTypeNames.ToArray();
                for (System.Int32 I = 0; I < strings.Count && I < qfnstrings.Length; I++)
                {
                    if (qfnstrings[I] == strings[I])
                    {
                        System.Type rtype = System.Type.GetType(strings[I]);
                        throw new Exceptions.ConfilctingConverterException(rtype , 
                            resv.GetConverterType(rtype) , 
                            resolver.GetConverterType(rtype));
                    }
                }
            }
            strings.Clear();
            strings = null;
            resolvers.Add(resolver);
        }

        /// <inheritdoc />
        public event TypeNotFoundEventHandler OnTypeNotFound
        {
            add { typenotfoundeventhandlers.Add(value); }
            remove { typenotfoundeventhandlers.Remove(value); }
        }

        /// <summary>
        /// Invalidates and disposes the <see cref="ExtensibleFormatter"/> class instance.
        /// </summary>
        public void Dispose()
        {
            resolvers?.Clear();
            resolvers = null;
            typenotfoundeventhandlers?.Clear();
            typenotfoundeventhandlers = null;
        }
		
		/// <summary>
        /// Guard so as to ensure that all resources are immediately released.
        /// </summary>
		~ExtensibleFormatter() { Dispose(); }
    }


}
