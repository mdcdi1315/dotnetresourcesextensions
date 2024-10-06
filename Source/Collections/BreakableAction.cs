
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
    /// Specifies the next action that must be performed when the <see cref="BreakableActionExtended{T}"/> delegate
    /// is used. Just like when using the foreach statement , you can use the values of this enumeration
    /// to control enumeration just like when using the <see langword="continue"/> and <see  langword="break"/> keywords.
    /// </summary>
    public enum EnumerationContinuationAction : System.Byte
    {
        /// <summary>
        /// No any various action should be performed and the enumeration should continue normally.
        /// </summary>
        Nothing,
        /// <summary>
        /// The loop must perform the <see langword="continue"/> keyword as soon as the method returns this.
        /// </summary>
        Continue,
        /// <summary>
        /// The loop must break as soon as the method returns this.
        /// </summary>
        Break
    }

    /// <summary>
    /// Provides an extended version of the <see cref="BreakableAction{T}"/> <see langword="delegate"/> 
    /// that gives you more control on how the enumeration will be performed.
    /// </summary>
    /// <typeparam name="T">The input element type.</typeparam>
    /// <param name="element">The element type</param>
    /// <returns>A control value on how the enumeration should be continued. See the comments on the constants of the <see cref="EnumerationContinuationAction"/> enumeration.</returns>
    public delegate EnumerationContinuationAction BreakableActionExtended<in T>(T element);

    /// <summary>
    /// Defines extension methods where the <see cref="BreakableAction{T}"/> and <see cref="BreakableActionExtended{T}"/> delegates
    /// can be used in enumerables.
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

        /// <summary>
        /// Executes for all elements in the specified enumerable the specified extended breakable action.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="enumerable">The enumerable to use.</param>
        /// <param name="actionex">The extended breakable action to invoke on every element.</param>
        public static void ForEach<T>(this IEnumerable<T> enumerable , BreakableActionExtended<T> actionex)
        {
            foreach (var item in enumerable)
            {
                switch (actionex(item)) 
                {
                    case EnumerationContinuationAction.Continue:
                        continue;
                    case EnumerationContinuationAction.Break:
                        return;
                }
            }
        }

    }

}
