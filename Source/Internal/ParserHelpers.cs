
using System;
using System.Security;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotNetResourcesExtensions.Internal
{
    // Parser helpers.
    internal static class ParserHelpers
    {
        public const System.String InvalidNameCharacters = "!@#$%^&*()`~,./?\\|\"\';:{}[]+=©§¥¢ž®¯";
        public const System.String InvalidFirstCharacterNameCharacters = "1234567890!@#$%^&*()`~,./?\\|\"';:{}[]+=-_©§¥¢ž®¯";

        // Simple function to convert a simple number string to a numeric value. 
        // Can also parse negative values too!
        public static System.Int64 ToNumber(System.String value)
        {
            System.Int32 start = 0;
            System.Boolean negative = value[0] == '-';
            if (negative) { start = 1; }
            System.Int64 result = 0, shift = 1;
            for (System.Int32 I = value.Length - 1; I >= start; I--)
            {
                result += (value[I].ToInt64() - 48) * shift;
                shift *= 10;
            }
            if (negative) { result = -result; }
            return result;
        }

        // Simple function to convert a simple number to a number string.
        // Can also work for negative numbers!
        public static System.String NumberToString(System.Int64 value)
        {
            System.Int64 copy = value;
            if (value < 0) { copy = -copy; }
            List<System.Char> chars = new();
            System.Int64 shift = 1 , rt;
            System.Byte I = 19 , end = (value < 0 ? 1 : 0).ToByte();
            do {
                I--;
                rt = (copy / shift) % 10;
                chars.Add((rt + 48).ToChar());
                shift *= 10;
            } while (I > end);
            // So as to perform negation
            if (value < 0) { chars.Add('-'); }
            chars.Reverse();
            return new(chars.ToArray()); // Although it will return a value like of full zeroes (If it is zero) , we are not being bothered by that.
        }

        // Obtained from JsonHelpers from System.Text.Json package.
        public static System.Boolean IsDigit(System.Char c) => (System.UInt32)(c - '0') <= ('9' - '0');

        /// <summary>
        /// Tests if the resource name given is valid.
        /// </summary>
        /// <param name="Name">The resource name to test.</param>
        /// <exception cref="ArgumentException"><paramref name="Name"/> was longer than 530 characters -or- contains invalid name characters.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="Name"/> was <see langword="null"/>.</exception>
        [System.Diagnostics.DebuggerHidden]
        public static void ValidateName(System.String Name)
        {
            if (System.String.IsNullOrEmpty(Name)) {
                throw new ArgumentNullException(nameof(Name));
            }

            if (Name.Length > 530)
            {
                throw new ArgumentException("A resource name must not have more than 530 characters.");
            }

            for (System.Int32 J = 0; J < InvalidFirstCharacterNameCharacters.Length; J++)
            {
                if (Name[0] == InvalidFirstCharacterNameCharacters[J])
                {
                    throw new ArgumentException($"The first character of a resource name must not have all the following characters: {InvalidFirstCharacterNameCharacters}.", nameof(Name));
                }
            }

            for (System.Int32 I = 1; I < Name.Length; I++)
            {
                for (System.Int32 J = 0; J < InvalidNameCharacters.Length; J++)
                {
                    if (Name[I] == InvalidNameCharacters[J])
                    {
                        throw new ArgumentException($"A resource name must not have all the following characters: {InvalidNameCharacters}.", nameof(Name));
                    }
                }
            }
        }

        public static string GetAssemblyQualifiedName(Type type, Func<Type, string> typeNameConverter)
        {
            string text = null;
            if (type != null)
            {
                if (typeNameConverter != null)
                {
                    try
                    {
                        text = typeNameConverter(type);
                    }
                    catch (Exception ex)
                    {
                        if (IsSecurityOrCriticalException(ex))
                        {
                            throw;
                        }
                    }
                }
                if (string.IsNullOrEmpty(text))
                {
                    text = type.AssemblyQualifiedName;
                }
            }
            return text;
        }

#pragma warning disable 0618 // ExecutionEngineException is obsolete
        public static bool IsSecurityOrCriticalException(Exception ex)
        {
            if ((ex is not NullReferenceException) && 
                (ex is not StackOverflowException) && 
                (ex is not OutOfMemoryException) && 
                (ex is not ThreadAbortException) && 
                (ex is not ExecutionEngineException) && 
                (ex is not IndexOutOfRangeException) && 
                (ex is not AccessViolationException))
            {
                return ex is SecurityException;
            }
            return true;
        }

        public static bool IsCriticalException(Exception ex)
        {
            if ((ex is not NullReferenceException) && 
            (ex is not StackOverflowException) && 
            (ex is not OutOfMemoryException) && 
            (ex is not ThreadAbortException) && 
            (ex is not ExecutionEngineException) && 
            (ex is not IndexOutOfRangeException))
            {
                return ex is AccessViolationException;
            }
            return true;
        }
#pragma warning restore 0618

        public static System.Byte[] GetBytes(System.Byte[] bytes, System.Int64 idx, System.Int64 count)
        {
            System.Int64 len = count <= bytes.LongLength ? count : (count - bytes.LongLength);
            System.Byte[] res = new System.Byte[len];
            System.Int64 I = idx, J = 0;
            for (; I < idx + count && I < bytes.LongLength; I++)
            {
                res[J] = bytes[I];
                J++;
            }
            // Recursive call when the bytes read were less than the expected.
            // In that case , the array length will be fixed to the number of elements read.
            if (J < len) { return GetBytes(res, 0, J); }
            return res;
        }

        public static System.Byte[] GetBytes(System.Span<System.Byte> span, System.Int32 idx, System.Int32 count)
        {
            System.Int32 len = count <= span.Length ? count : (count - span.Length);
            System.Byte[] res = new System.Byte[len];
            System.Int32 I = idx, J = 0;
            for (; I < idx + count && I < span.Length; I++)
            {
                res[J] = span[I];
                J++;
            }
            if (J < len) { return GetBytes(bytes: res, 0, J); }
            return res;
        }

        public static System.Byte[] GetBytes(System.Byte[] bytes, System.Int64 idx) => GetBytes(bytes, idx, bytes.LongLength - idx);

        /// <summary>
        /// Detects whether are single or double quotes at the start and the end of the string.
        /// </summary>
        /// <param name="chk">The string to check.</param>
        /// <returns>The <see cref="System.Boolean"/> indicating whether the string given has single or double quotes as desribed.</returns>
        public static System.Boolean AreQuotesAtStartEnd(System.String chk) => ((chk[0] == '\"' || chk[0] == '\'') && (chk[^1] == '\"' || chk[^1] == '\''));

        /// <summary>
        /// Removes single or double quotes , if these are found in the start and in the end of <paramref name="str"/> , 
        /// and returns the resulting string as a new string instance. <br />
        /// If the quotes are not balanced , the string is returned as-it-is.
        /// </summary>
        /// <param name="str">The string to remove the quotes from.</param>
        /// <returns>The result string after quote removal.</returns>
        public static System.String RemoveQuotes(System.String str)
        {
            System.String ret = System.String.Empty;
            if (AreQuotesAtStartEnd(str))
            {
                if (str[0] == '\"' || str[0] == '\'')
                {
                    ret = str.Substring(1);
                }
                if (ret[^1] == '\"' || ret[^1] == '\'')
                {
                    ret = ret.Remove(ret.Length - 1);
                }
            }
            else { ret = str; }
            return ret;
        }

        public static System.Int32 FindCharOccurence(this System.String str, System.Char ch)
        {
            System.Int32 result = 0;
            foreach (System.Char c in str) { if (ch == c) { result++; } }
            return result;
        }

        public static System.Boolean EqualsWithoutLeadingWhiteSpace(this System.String str, System.String other)
        {
            System.String copy = System.String.Empty;
            foreach (System.Char c in str)
            {
                switch (c)
                {
                    case ' ':
                    case '\t':
                        if (System.String.IsNullOrEmpty(copy)) { break; }
                        copy += c;
                        break;
                    default:
                        copy += c;
                        break;
                }
            }
            return copy.Equals(other);
        }

        // Optimized method for reading fast byte arrays encoded as base64's. 
        // Here I use yield return and IEnumerable because: 
        // -> I avoid calling FindCharOccurence and creating a new string array
        // -> I use a default buffer with StringBuilder to fast-up the process.
        // -> Each new iteration will be evaluated back to the foreach loop.
        // -> Uses System.Text.StringBuilder internally for better performance.
        // The buffer capacity is configurable , maximizing the performance as most as possible.
        // -> It's results can be saved to a System.Text.StringBuilder
        // base64 length is known appending is done adequately fast.
        public static IEnumerable<System.String> GetStringSplittedData(this System.String str , System.Char ch , System.Int32 recsize = 5192) 
        {
            System.Text.StringBuilder sbtemp = new(recsize);
            foreach (System.Char c in str)
            {
                if (c == ch) {
                    yield return sbtemp.ToString();
                    sbtemp.Clear();
                } else {
                    sbtemp.Append(c);
                }
            }
            // Return any data that are positioned at the end of the string.
            // If no data exist or are just whitespaces , do not return them.
            System.String tmp = sbtemp.ToString();
            if (System.String.IsNullOrWhiteSpace(tmp) == false) { yield return tmp; }
            tmp = null;
            sbtemp.Clear();
            sbtemp = null;
        }

        [System.Diagnostics.DebuggerHidden]
        private static TResult Enum_Parse<TResult>(System.String data)
            where TResult : notnull, System.Enum
        {
            System.Type type = typeof(TResult);
            System.String[] names = type.GetEnumNames();
            for (System.Int32 I = 0; I < names.Length; I++)
            {
                if (names[I].Equals(data)) {
                    return (TResult)type.GetEnumValues().GetValue(I);
                }
            }
            throw new ArgumentException($"The name {data} does not belong to a named constant in {type.Name} enumeration.");
        }

        /// <summary>
        /// Parses the given string and returns the equivalent constant of <typeparamref name="TEnum"/> enumeration.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enumeration to return the constant as.</typeparam>
        /// <param name="data">The string that is a name of a constant of <typeparamref name="TEnum"/> enumeration.</param>
        /// <returns>The equivalent parsed constant of <typeparamref name="TEnum"/> enumeration.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> was <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="data"/> does not exist as a enumerated constant of <typeparamref name="TEnum"/>.</exception>
        public static TEnum ParseEnumerationConstant<TEnum>(System.String data)
            where TEnum : notnull , System.Enum
        {
            if (System.String.IsNullOrEmpty(data)) { throw new ArgumentNullException(nameof(data)); }
            return Enum_Parse<TEnum>(data);
        }
    }
}
