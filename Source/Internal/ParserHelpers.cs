
using System;
using System.Security;
using System.Threading;
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Internal
{
    // Parser helpers.
    internal static class ParserHelpers
    {
        private const System.Int32 BUFSIZE = 4096;
        public const System.String InvalidNameCharacters = "!@#$%^&*()`~,./?\\|\"\';:{}[]+=©§¥¢ž®¯";
        public const System.String InvalidFirstCharacterNameCharacters = "1234567890!@#$%^&*()`~,./?\\|\"';:{}[]+=-_©§¥¢ž®¯";

        // Simple function to convert a simple number string to a numeric value. 
        // Can also parse negative values too!
        public static System.Int64 ToNumber(System.String value)
        {
            System.Int32 start = 0;
            System.Boolean negative = value[0] == '-';
            if (negative) { start = 1; }
            System.Int64 result = 0, prg = 1;
            for (System.Int32 I = value.Length - 1; I >= start; I--)
            {
                result += (value[I].ToInt64() - 48) * prg;
                prg *= 10;
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
            System.Int64 prg = 1 , rt;
            System.Byte I = 19 , end = (value < 0 ? 1 : 0).ToByte();
            do {
                I--;
                rt = (copy / prg) % 10;
                chars.Add((rt + 48).ToChar());
                prg *= 10;
            } while (I > end);
            // So as to perform negation
            if (value < 0) { chars.Add('-'); }
            chars.Reverse();
            return new(chars.ToArray()); // Although it will return a value like of full zeroes (If it is zero) , we are not being bothered by that.
        }

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

        public static System.Byte[] ReadBuffered(System.IO.Stream Stream, System.Int64 RequestedBytes)
        {
            // Check for null conditions or whether we can read from this stream
            if (Stream == null) { return null; }
            if (Stream.CanRead == false) { return null; }
            // Create a new byte array with the requested size.
            System.Byte[] Contents = new System.Byte[RequestedBytes];
            if (RequestedBytes <= BUFSIZE)
            {
                // Read all bytes directly , if the requested bytes are less than the buffer limit.
                // Otherwise we don't care here; we do not read thousands or millions of bytes.
                Stream.Read(Contents, 0, Contents.Length);
            }
            else
            {
                System.Int32 Count;
                System.Int32 Offset = 0;
                // Read all bytes with buffered mode.
                do
                {
                    Count = Stream.Read(Contents, Offset, BUFSIZE);
                    Offset += BUFSIZE;
                    // Condition specifies that the loop will continue to run when the read bytes are
                    // more or equal than the buffer limit , plus make sure that the next read will not
                    // surpass the bytes that the final array can hold.
                } while ((Count >= BUFSIZE) && (Offset + BUFSIZE <= Contents.Length));
                // In case that the bytes were surpassed in the above condition , pass all the rest bytes again normally.
                if (Contents.Length - Offset > 0) { Stream.Read(Contents, Offset, Contents.Length - Offset); }
            }
            return Contents;
        }

        public static void WriteBuffered(System.IO.Stream stream, System.Byte[] data)
        {
            // Abstract: Writes bytes to a stream with a 'buffered' method.
            // Calculate the blocks that will be raw-copied. 
            // Also , calculate the remaining data that will be plainly passed.
            System.Int64 blocks = data.LongLength / BUFSIZE,
                c = data.LongLength % BUFSIZE;
            System.Int32 pos = 0;
            // Copy all data to the stream
            while (blocks > 0)
            {
                stream.Write(data, pos, BUFSIZE);
                pos += BUFSIZE;
                blocks--;
            }
            // If the input array size is not exactly a multiple of BUFSIZE , the rest data will be copied as-it-is.
            // This even works for cases that data.LongLength < BUFSIZE because the while loop
            // will never be entered.
            if (c > 0) { stream.Write(data, pos, (System.Int32)c); }
        }

        public static void BlockCopy(System.IO.Stream input, System.IO.Stream output)
        {
            System.Byte[] buffer = new System.Byte[BUFSIZE];
            System.Int32 readin = 0;
            while ((readin = input.Read(buffer, 0, BUFSIZE)) > 0)
            {
                output.Write(buffer, 0, readin);
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
            if ((ex is not NullReferenceException) && (ex is not StackOverflowException) && (ex is not OutOfMemoryException) && (ex is not ThreadAbortException) && (ex is not ExecutionEngineException) && (ex is not IndexOutOfRangeException) && (ex is not AccessViolationException))
            {
                return ex is SecurityException;
            }
            return true;
        }

        public static bool IsCriticalException(Exception ex)
        {
            if ((ex is not NullReferenceException) && (ex is not StackOverflowException) && (ex is not OutOfMemoryException) && (ex is not ThreadAbortException) && (ex is not ExecutionEngineException) && (ex is not IndexOutOfRangeException))
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

        private static System.Boolean AreQuotesAtStartEnd(System.String chk) => ((chk[0] == '\"' || chk[0] == '\'') && (chk[^1] == '\"' || chk[^1] == '\''));

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
        // -> Each new iteration will be evaluated back to the foreach loop.
        // -> It's results can be saved to a System.Text.StringBuilder , and since the
        // base64 length is known appending is done adequately fast.
        public static IEnumerable<System.String> GetStringSplittedData(this System.String str , System.Char ch) 
        {
            System.String temp = System.String.Empty;
            foreach (System.Char c in str)
            {
                if (c == ch) {
                    yield return temp;
                    temp = System.String.Empty;
                } else {
                    temp += c;
                }
            }
            // Return any data that are positioned at the end of the string.
            // If no data exist or are just whitespaces , do not return them.
            if (System.String.IsNullOrWhiteSpace(temp) == false) { yield return temp; }
            temp = null;
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
