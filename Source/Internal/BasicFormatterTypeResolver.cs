
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
                typeof(System.Drawing.Point).AssemblyQualifiedName,
                typeof(System.Drawing.PointF).AssemblyQualifiedName,
                typeof(System.Drawing.Rectangle).AssemblyQualifiedName,
                typeof(System.Drawing.RectangleF).AssemblyQualifiedName,
                typeof(System.Drawing.Size).AssemblyQualifiedName,
                typeof(System.Drawing.SizeF).AssemblyQualifiedName,
#if WINDOWS10_0_17763_0_OR_GREATER || NET471_OR_GREATER // .NET Framework & .NET Windows Desktop representations
                typeof(System.Drawing.Color).AssemblyQualifiedName,
                typeof(System.Drawing.Icon).AssemblyQualifiedName,
                typeof(System.Drawing.Bitmap).AssemblyQualifiedName,
                typeof(System.Drawing.StringFormat).AssemblyQualifiedName,
#endif
#if NET7_0_OR_GREATER // For all .NET Core flavors only
                typeof(System.DateOnly).AssemblyQualifiedName,
                typeof(System.TimeOnly).AssemblyQualifiedName,
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
            AssemblyQualifiedNamesBreaker br = new(type.AssemblyQualifiedName), brt;
            foreach (System.String qname in qualifiednames)
            {
                brt = new(qname);
                if (brt.Satisfies(br)) {
                    return IArrayRepresentationExtensions.GetArrayRepresentation<T>();
                }
            }
            brt = null; br = null;
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

    internal sealed class AssemblyQualifiedNamesBreaker
    {
        private System.String fullname;
        private System.Reflection.AssemblyName name;

        public AssemblyQualifiedNamesBreaker(System.String full) 
        { 
            fullname = full;
            name = new(fullname.Substring(fullname.IndexOf(',') + 1));
        }

        public Version MinimumRequiredVersion => name.Version;

        public System.String FullTypeName => fullname.Remove(fullname.IndexOf(','));

        public System.Boolean Satisfies(AssemblyQualifiedNamesBreaker other)
        {
            if (other is null) { throw new ArgumentNullException(nameof(other)); }
            System.Boolean result = other.FullTypeName == FullTypeName && other.name.Version >= MinimumRequiredVersion;
            System.Diagnostics.Debug.WriteLine($"SATISFIES: ({FullTypeName} and {other.FullTypeName}): {result}");
            return result;
        }

        public System.Boolean Equals(AssemblyQualifiedNamesBreaker other)
        {
            if (other is null) { return false; }
            return Satisfies(other);
        }

        public static System.Boolean operator ==(AssemblyQualifiedNamesBreaker a, AssemblyQualifiedNamesBreaker b) => a.Equals(b);

        public static System.Boolean operator !=(AssemblyQualifiedNamesBreaker a , AssemblyQualifiedNamesBreaker b) => a.Equals(b) == false;
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            return Equals(obj as AssemblyQualifiedNamesBreaker);
        }

        public override int GetHashCode() { return base.GetHashCode(); }
    }

}

