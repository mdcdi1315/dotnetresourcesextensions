using System;
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Collections
{
    /// <summary>
    /// Defines a default property store which is mutable , and does not depend on anything reader-specific stuff.
    /// </summary>
    public sealed class MemoryBackingVariablePropertyStore : IFileReferenceVariablePropertyStore
    {
        private List<KeyValuePair<System.String, System.String>> memory;
        
        /// <summary>
        /// Creates a new instance of the <see cref="MemoryBackingVariablePropertyStore"/> class.
        /// </summary>
        public MemoryBackingVariablePropertyStore()
        {
            memory = new(10);
        }

        /// <inheritdoc />
        public string GetVariable(string name)
        {
            if (System.String.IsNullOrEmpty(name)) { throw new ArgumentNullException(nameof(name)); }
            foreach (var prop in memory) 
            {
                if (prop.Key.Equals(name))
                {
                    return prop.Value;
                }
            }
            return System.String.Empty;
        }

        /// <inheritdoc />
        public void SetVariable(string name, string value)
        {
            if (System.String.IsNullOrEmpty(name)) { throw new ArgumentNullException(nameof(name)); }
            System.Int32 fd = -1;
            for (System.Int32 I = 0; I < memory.Count && fd == -1; I++)
            {
                if (memory[I].Key.Equals(name)) { fd = I; }
            }
            if (fd < 0) {
                memory.Add(new(name, value));
            } else {
                memory[fd] = new(name , value);
            }
        }
    }
}
