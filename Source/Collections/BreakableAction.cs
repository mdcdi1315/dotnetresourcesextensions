
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Collections
{
    /// <summary>
    /// Specifies an action in a for-each method that is breakable if the return value of the action is <see langword="true"/>. <br />
    /// This delegate is the abstraction , and the input type can be different among any implementations.
    /// </summary>
    /// <typeparam name="T">The input element type.</typeparam>
    /// <param name="element">The element to get.</param>
    /// <returns>A value whether to continue or break the enumeration.</returns>
    public delegate System.Boolean BreakableAction<in T>(T element);


    /// <summary>
    /// Defines extension methods where the <see cref="BreakableAction{T}"/> delegate can be used in enumerables.
    /// </summary>
    public static class BreakableActionExtensions
    {
        /// <summary>
        /// Executes for all elements in the specified enumerable the specified breakable action.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="enumerable">The enumerable to use.</param>
        /// <param name="action">The breakable action to invoke on every element.</param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, BreakableAction<T> action)
        {
            foreach (var item in enumerable)  {
                if (action(item)) { break; }
            }
        }
    }

}
