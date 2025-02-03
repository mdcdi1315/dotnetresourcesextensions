using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetResourcesExtensions
{
    internal sealed class BasicFileReferenceTypeAliasResolver : IFileReferenceTypeAliasResolver
    {
        private List<KeyValuePair<System.String, System.Type>> aliases;

        public BasicFileReferenceTypeAliasResolver() 
        {
            aliases = new(10) { 
                new("StringType" , typeof(System.String)),
                new("ByteArrayType" , typeof(System.Byte[])),
                new("RawFile" , typeof(System.Byte[]))
            };
        }

        public void Clear() => aliases.Clear();

        public void RegisterAlias(string alias, Type reference)
        {
            if (alias is null) { throw new ArgumentNullException(nameof(alias)); }
            if (reference is null) { throw new ArgumentNullException(nameof(reference)); }
            foreach (var al in aliases) { if (al.Key.Equals(alias)) { throw new ArgumentException($"The alias with name {alias} does already exist."); } }
            aliases.Add(new(alias, reference));
        }

        public Type ResolveAlias(string alias)
        {
            if (TryResolveAlias(alias, out var type)) { return type; }
            throw new ArgumentException($"The alias with name {alias} cannot be found." , nameof(alias));
        }

        public bool TryResolveAlias(string alias, out Type type)
        {
            if (System.String.IsNullOrEmpty(alias)) { throw new ArgumentNullException(nameof(alias)); }
            type = null;
            foreach (var kvp in aliases)
            {
                if (kvp.Key.Equals(alias)) { type = kvp.Value; return true; }
            }
            return false;
        }

        public void UnRegisterAlias(string alias)
        {
            if (System.String.IsNullOrEmpty(alias)) { throw new ArgumentNullException(nameof(alias)); }
            for (System.Int32 I = 0; I < aliases.Count; I++) 
            { 
                if (aliases[I].Key.Equals(alias)) { aliases.RemoveAt(I); break; }
            }
        }
    }
}
