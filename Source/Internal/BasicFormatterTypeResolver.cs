﻿
using DotNetResourcesExtensions.Internal.CustomFormatter.Converters;
using System;
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Internal.CustomFormatter
{
    /// <summary>
    /// A default Type resolver that provides converters for all the basic BCL classes.
    /// </summary>
    internal sealed class BasicFormatterTypeResolver : ITypeResolver
    {
        private System.String[] qualifiednames;

        public BasicFormatterTypeResolver()
        {
            // I prefer the qualified names to be resolved at run-time because 
            // each BCL defines these types with different qualified names.
            qualifiednames = new System.String[] {
                typeof(System.Double).AssemblyQualifiedName,
                typeof(System.Single).AssemblyQualifiedName,
                typeof(System.Int32).AssemblyQualifiedName,
                typeof(System.Int64).AssemblyQualifiedName,
                typeof(System.UInt32).AssemblyQualifiedName,
                typeof(System.UInt64).AssemblyQualifiedName,
                typeof(System.UInt16).AssemblyQualifiedName,
                typeof(System.Int16).AssemblyQualifiedName,
                typeof(System.TimeSpan).AssemblyQualifiedName,
                typeof(System.DateTime).AssemblyQualifiedName,
                typeof(System.Decimal).AssemblyQualifiedName,
                typeof(System.DateTimeOffset).AssemblyQualifiedName,
                typeof(System.Guid).AssemblyQualifiedName,
                typeof(System.IO.Stream).AssemblyQualifiedName,
                typeof(System.Type).AssemblyQualifiedName,
                typeof(System.Version).AssemblyQualifiedName,
                typeof(System.Boolean).AssemblyQualifiedName,
                typeof(System.Uri).AssemblyQualifiedName,
                typeof(System.ApplicationId).AssemblyQualifiedName,
#if WINDOWS10_0_17763_0_OR_GREATER || NET471_OR_GREATER // .NET Framework & .NET Windows Desktop representations
                typeof(System.Drawing.Color).AssemblyQualifiedName,
                typeof(System.Drawing.Point).AssemblyQualifiedName,
                typeof(System.Drawing.PointF).AssemblyQualifiedName,
                typeof(System.Drawing.Icon).AssemblyQualifiedName,
                typeof(System.Drawing.Bitmap).AssemblyQualifiedName,
                typeof(System.Drawing.Rectangle).AssemblyQualifiedName,
                typeof(System.Drawing.RectangleF).AssemblyQualifiedName,
                typeof(System.Drawing.Size).AssemblyQualifiedName,
                typeof(System.Drawing.SizeF).AssemblyQualifiedName,
                typeof(System.Drawing.StringFormat).AssemblyQualifiedName,
#endif
#if NET7_0_OR_GREATER // For all .NET Core flavors only
                typeof(System.DateOnly).AssemblyQualifiedName,
                typeof(System.Half).AssemblyQualifiedName,
                typeof(System.UInt128).AssemblyQualifiedName,
                typeof(System.Int128).AssemblyQualifiedName,
#endif
            };
        }

        public IEnumerable<System.String> RegisteredQualifiedTypeNames => qualifiednames;

        public IArrayRepresentation<T> GetConverter<T>()
        {
            System.Type type = typeof(T);
            foreach (System.String qname in qualifiednames)
            {
                if (type.AssemblyQualifiedName == qname)
                {
                    return IArrayRepresentationExtensions.GetArrayRepresentation<T>();
                }
            }
            throw new Exceptions.ConverterNotFoundException(typeof(T));
        }
    }

    internal sealed class ConstructedTypeResolver : ITypeResolver
    {
        private List<System.Object> currentconverters;
        private List<System.String> currentqualifiedtypenames;

        public ConstructedTypeResolver() 
        {
            currentconverters = new();
            currentqualifiedtypenames = new();
        }

        public IEnumerable<string> RegisteredQualifiedTypeNames => currentqualifiedtypenames;

        public void Add<T>(IArrayRepresentation<T> converter)
        {
            currentconverters.Add(converter);
            currentqualifiedtypenames.Add(converter.OriginalType.AssemblyQualifiedName);
        }

        public void Add(System.Object obj)
        {
            System.Type objtype = obj.GetType();
            if (objtype.GetInterface(IArrayRepresentationExtensions.ArrReprString) == null)
            {
                // Does not implement IArrayRepresentation , throw exception.
                throw new InvalidOperationException("Invalid attempt to add a converter that does not implement the IArrayRepresentation interface.");
            }
            currentconverters.Add(obj);
            // Call the OriginalType property through reflection.
            System.Type typed = (System.Type)objtype.GetProperty("OriginalType").GetValue(obj, null);
            if (typed == null) { throw new ArgumentException("The OriginalType property has not returned correct results."); }
            currentqualifiedtypenames.Add(typed.AssemblyQualifiedName);
        }

        public IArrayRepresentation<T> GetConverter<T>()
        {
            for (System.Int32 I = 0 ; I < currentconverters.Count; I++) 
            {
                if (typeof(T).AssemblyQualifiedName == currentqualifiedtypenames[I])
                {
                    return (IArrayRepresentation<T>)currentconverters[I];
                }
            }
            throw new Exceptions.ConverterNotFoundException(typeof(T));
        }
    }

}

