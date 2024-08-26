
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
        public static System.Int64 ToNumber(System.String value)
        {
            System.Int64 result = 0, prg = 1;
            for (System.Int32 I = value.Length - 1; I > -1; I--)
            {
                result += (((System.Int64)value[I]) - 48) * prg;
                prg *= 10;
            }
            return result;
        }

        // Simple function to convert a simple number to a number string.
        // Only works currently for numbers that are positive.
        public static System.String NumberToString(System.Int64 value)
        {
            List<System.Char> chars = new();
            System.Int64 prg = 1;
            System.Int64 rt;
            System.Byte I = 19;
            do
            {
                I--;
                rt = (value / prg) % 10;
                chars.Add((System.Char)(rt + 48));
                prg *= 10;
            } while (I > 0);
            chars.Reverse();
            return new(chars.ToArray()); // Although it will return a value like of full zeroes (If it is zero) , we are not being bothered by that.
        }

        /// <summary>
        /// Tests if the resource name given is valid.
        /// </summary>
        /// <param name="Name">The resource name to test.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        [System.Diagnostics.DebuggerHidden]
        public static void ValidateName(System.String Name)
        {
            if (Name == null || System.String.IsNullOrEmpty(Name))
            {
                throw new ArgumentNullException("Name");
            }

            if (Name.Length > 530)
            {
                throw new ArgumentException("A resource name must not have more than 530 characters.");
            }

            for (System.Int32 J = 0; J < InvalidFirstCharacterNameCharacters.Length; J++)
            {
                if (Name[0] == InvalidFirstCharacterNameCharacters[J])
                {
                    throw new ArgumentException($"The first character of a resource name must not have all the following characters: {InvalidFirstCharacterNameCharacters}.", "Name");
                }
            }

            for (System.Int32 I = 1; I < Name.Length; I++)
            {
                for (System.Int32 J = 0; J < InvalidNameCharacters.Length; J++)
                {
                    if (Name[I] == InvalidNameCharacters[J])
                    {
                        throw new ArgumentException($"A resource name must not have all the following characters: {InvalidNameCharacters}.", "Name");
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

#pragma warning disable 0618
        public static bool IsSecurityOrCriticalException(Exception ex)
        {
            if (!(ex is NullReferenceException) && !(ex is StackOverflowException) && !(ex is OutOfMemoryException) && !(ex is ThreadAbortException) && !(ex is ExecutionEngineException) && !(ex is IndexOutOfRangeException) && !(ex is AccessViolationException))
            {
                return ex is SecurityException;
            }
            return true;
        }

        public static bool IsCriticalException(Exception ex)
        {
            if (!(ex is NullReferenceException) && !(ex is StackOverflowException) && !(ex is OutOfMemoryException) && !(ex is ThreadAbortException) && !(ex is ExecutionEngineException) && !(ex is IndexOutOfRangeException))
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
    }
}
