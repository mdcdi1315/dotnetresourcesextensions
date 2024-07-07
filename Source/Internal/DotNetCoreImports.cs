
#if NET471_OR_GREATER || NETSTANDARD2_0_OR_GREATER

namespace System.Diagnostics.CodeAnalysis
{

    /// <summary>Specifies that null is allowed as an input even if the corresponding type disallows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    internal sealed class AllowNullAttribute : Attribute { }

    /// <summary>Specifies that null is disallowed as an input even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    internal sealed class DisallowNullAttribute : Attribute { }

    /// <summary>Specifies that an output may be null even if the corresponding type disallows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    internal sealed class MaybeNullAttribute : Attribute { }

    /// <summary>Specifies that an output will not be null even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    internal sealed class NotNullAttribute : Attribute { }

    /// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter may be null even if the corresponding type disallows it.</summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class MaybeNullWhenAttribute : Attribute
    {
        /// <summary>Initializes the attribute with the specified return value condition.</summary>
        /// <param name="returnValue">
        /// The return value condition. If the method returns this value, the associated parameter may be null.
        /// </param>
        public MaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

        /// <summary>Gets the return value condition.</summary>
        public bool ReturnValue { get; }
    }

    /// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter will not be null even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        /// <summary>Initializes the attribute with the specified return value condition.</summary>
        /// <param name="returnValue">
        /// The return value condition. If the method returns this value, the associated parameter will not be null.
        /// </param>
        public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

        /// <summary>Gets the return value condition.</summary>
        public bool ReturnValue { get; }
    }

    /// <summary>Specifies that the output will be non-null if the named parameter is non-null.</summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
    internal sealed class NotNullIfNotNullAttribute : Attribute
    {
        /// <summary>Initializes the attribute with the associated parameter name.</summary>
        /// <param name="parameterName">
        /// The associated parameter name.  The output will be non-null if the argument to the parameter specified is non-null.
        /// </param>
        public NotNullIfNotNullAttribute(string parameterName) => ParameterName = parameterName;

        /// <summary>Gets the associated parameter name.</summary>
        public string ParameterName { get; }
    }

    /// <summary>Applied to a method that will never return under any circumstance.</summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class DoesNotReturnAttribute : Attribute { }

    /// <summary>Specifies that the method will not return if the associated Boolean parameter is passed the specified value.</summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class DoesNotReturnIfAttribute : Attribute
    {
        /// <summary>Initializes the attribute with the specified parameter value.</summary>
        /// <param name="parameterValue">
        /// The condition parameter value. Code after the method will be considered unreachable by diagnostics if the argument to
        /// the associated parameter matches this value.
        /// </param>
        public DoesNotReturnIfAttribute(bool parameterValue) => ParameterValue = parameterValue;

        /// <summary>Gets the condition parameter value.</summary>
        public bool ParameterValue { get; }
    }


#nullable enable
    /// <summary>Specifies that the method or property will ensure that the listed field and property members have not-null values.</summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    internal sealed class MemberNotNullAttribute : Attribute
    {
        /// <summary>Initializes the attribute with a field or property member.</summary>
        /// <param name="member">
        /// The field or property member that is promised to be not-null.
        /// </param>
        public MemberNotNullAttribute(string member) => Members = new[] { member };

        /// <summary>Initializes the attribute with the list of field and property members.</summary>
        /// <param name="members">
        /// The list of field and property members that are promised to be not-null.
        /// </param>
        public MemberNotNullAttribute(params string[] members) => Members = members;

        /// <summary>Gets field or property member names.</summary>
        public string[] Members { get; }
    }

    /// <summary>
    /// Suppresses reporting of a specific rule violation, allowing multiple suppressions on a
    /// single code artifact.
    /// </summary>
    /// <remarks>
    /// <see cref="UnconditionalSuppressMessageAttribute"/> is different than
    /// <see cref="SuppressMessageAttribute"/> in that it doesn't have a
    /// <see cref="ConditionalAttribute"/>. So it is always preserved in the compiled assembly.
    /// </remarks>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    internal sealed class UnconditionalSuppressMessageAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnconditionalSuppressMessageAttribute"/>
        /// class, specifying the category of the tool and the identifier for an analysis rule.
        /// </summary>
        /// <param name="category">The category for the attribute.</param>
        /// <param name="checkId">The identifier of the analysis rule the attribute applies to.</param>
        public UnconditionalSuppressMessageAttribute(string category, string checkId)
        {
            Category = category;
            CheckId = checkId;
        }

        /// <summary>
        /// Gets the category identifying the classification of the attribute.
        /// </summary>
        /// <remarks>
        /// The <see cref="Category"/> property describes the tool or tool analysis category
        /// for which a message suppression attribute applies.
        /// </remarks>
        public string Category { get; }

        /// <summary>
        /// Gets the identifier of the analysis tool rule to be suppressed.
        /// </summary>
        /// <remarks>
        /// Concatenated together, the <see cref="Category"/> and <see cref="CheckId"/>
        /// properties form a unique check identifier.
        /// </remarks>
        public string CheckId { get; }

        /// <summary>
        /// Gets or sets the scope of the code that is relevant for the attribute.
        /// </summary>
        /// <remarks>
        /// The Scope property is an optional argument that specifies the metadata scope for which
        /// the attribute is relevant.
        /// </remarks>
        public string? Scope { get; set; }

        /// <summary>
        /// Gets or sets a fully qualified path that represents the target of the attribute.
        /// </summary>
        /// <remarks>
        /// The <see cref="Target"/> property is an optional argument identifying the analysis target
        /// of the attribute. An example value is "System.IO.Stream.ctor():System.Void".
        /// Because it is fully qualified, it can be long, particularly for targets such as parameters.
        /// The analysis tool user interface should be capable of automatically formatting the parameter.
        /// </remarks>
        public string? Target { get; set; }

        /// <summary>
        /// Gets or sets an optional argument expanding on exclusion criteria.
        /// </summary>
        /// <remarks>
        /// The <see cref="MessageId "/> property is an optional argument that specifies additional
        /// exclusion where the literal metadata target is not sufficiently precise. For example,
        /// the <see cref="UnconditionalSuppressMessageAttribute"/> cannot be applied within a method,
        /// and it may be desirable to suppress a violation against a statement in the method that will
        /// give a rule violation, but not against all statements in the method.
        /// </remarks>
        public string? MessageId { get; set; }

        /// <summary>
        /// Gets or sets the justification for suppressing the code analysis message.
        /// </summary>
        public string? Justification { get; set; }
    }

    /// <summary>
    /// Indicates that the specified method requires dynamic access to code that is not referenced
    /// statically, for example through <see cref="System.Reflection"/>.
    /// </summary>
    /// <remarks>
    /// This allows tools to understand which methods are unsafe to call when removing unreferenced
    /// code from an application.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
    internal sealed class RequiresUnreferencedCodeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresUnreferencedCodeAttribute"/> class
        /// with the specified message.
        /// </summary>
        /// <param name="message">
        /// A message that contains information about the usage of unreferenced code.
        /// </param>
        public RequiresUnreferencedCodeAttribute(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets a message that contains information about the usage of unreferenced code.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets or sets an optional URL that contains more information about the method,
        /// why it requires unreferenced code, and what options a consumer has to deal with it.
        /// </summary>
        public string? Url { get; set; }
    }

#nullable disable

    /// <summary>
    /// Indicates that certain members on a specified <see cref="T:System.Type" /> are accessed dynamically,
    /// for example through <see cref="N:System.Reflection" />.
    /// </summary>
    /// <remarks>
    /// This allows tools to understand which members are being accessed during the execution
    /// of a program.
    ///
    /// This attribute is valid on members whose type is <see cref="T:System.Type" /> or <see cref="T:System.String" />.
    ///
    /// When this attribute is applied to a location of type <see cref="T:System.String" />, the assumption is
    /// that the string represents a fully qualified type name.
    ///
    /// When this attribute is applied to a class, interface, or struct, the members specified
    /// can be accessed dynamically on <see cref="T:System.Type" /> instances returned from calling
    /// <see cref="M:System.Object.GetType" /> on instances of that class, interface, or struct.
    ///
    /// If the attribute is applied to a method it's treated as a special case and it implies
    /// the attribute should be applied to the "this" parameter of the method. As such the attribute
    /// should only be used on instance methods of types assignable to System.Type (or string, but no methods
    /// will use it there).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, Inherited = false)]
    internal sealed class DynamicallyAccessedMembersAttribute : Attribute
    {
        /// <summary>
        /// Gets the <see cref="T:System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes" /> which specifies the type
        /// of members dynamically accessed.
        /// </summary>
        public DynamicallyAccessedMemberTypes MemberTypes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute" /> class
        /// with the specified member types.
        /// </summary>
        /// <param name="memberTypes">The types of members dynamically accessed.</param>
        public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes)
        {
            MemberTypes = memberTypes;
        }
    }

    /// <summary>
    /// Specifies the types of members that are dynamically accessed.
    /// <br />
    /// This enumeration has a <see cref="T:System.FlagsAttribute" /> attribute that allows a
    /// bitwise combination of its member values.
    /// </summary>
    [Flags]
    internal enum DynamicallyAccessedMemberTypes
    {
        /// <summary>
        /// Specifies no members.
        /// </summary>
        None = 0,
        /// <summary>
        /// Specifies the default, parameterless public constructor.
        /// </summary>
        PublicParameterlessConstructor = 1,
        /// <summary>
        /// Specifies all public constructors.
        /// </summary>
        PublicConstructors = 3,
        /// <summary>
        /// Specifies all non-public constructors.
        /// </summary>
        NonPublicConstructors = 4,
        /// <summary>
        /// Specifies all public methods.
        /// </summary>
        PublicMethods = 8,
        /// <summary>
        /// Specifies all non-public methods.
        /// </summary>
        NonPublicMethods = 0x10,
        /// <summary>
        /// Specifies all public fields.
        /// </summary>
        PublicFields = 0x20,
        /// <summary>
        /// Specifies all non-public fields.
        /// </summary>
        NonPublicFields = 0x40,
        /// <summary>
        /// Specifies all public nested types.
        /// </summary>
        PublicNestedTypes = 0x80,
        /// <summary>
        /// Specifies all non-public nested types.
        /// </summary>
        NonPublicNestedTypes = 0x100,
        /// <summary>
        /// Specifies all public properties.
        /// </summary>
        PublicProperties = 0x200,
        /// <summary>
        /// Specifies all non-public properties.
        /// </summary>
        NonPublicProperties = 0x400,
        /// <summary>
        /// Specifies all public events.
        /// </summary>
        PublicEvents = 0x800,
        /// <summary>
        /// Specifies all non-public events.
        /// </summary>
        NonPublicEvents = 0x1000,
        /// <summary>
        /// Specifies all interfaces implemented by the type.
        /// </summary>
        Interfaces = 0x2000,
        /// <summary>
        /// Specifies all members.
        /// </summary>
        All = -1
    }

    /// <summary>
    /// States a dependency that one member has on another.
    /// </summary>
    /// <remarks>
    /// This can be used to inform tooling of a dependency that is otherwise not evident purely from
    /// metadata and IL, for example a member relied on via reflection.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    internal sealed class DynamicDependencyAttribute : Attribute
    {
        /// <summary>
        /// Gets the signature of the member depended on.
        /// </summary>
        /// <remarks>
        /// Either <see cref="MemberSignature" /> must be a valid string or <see cref="MemberTypes" />
        /// must not equal <see cref="DynamicallyAccessedMemberTypes.None" />, but not both.
        /// </remarks>
        public string MemberSignature { get; }

        /// <summary>
        /// Gets the <see cref="DynamicallyAccessedMemberTypes" /> which specifies the type
        /// of members depended on.
        /// </summary>
        /// <remarks>
        /// Either <see cref="MemberSignature" /> must be a valid string or <see cref="MemberTypes" />
        /// must not equal <see cref="DynamicallyAccessedMemberTypes.None" />, but not both.
        /// </remarks>
        public DynamicallyAccessedMemberTypes MemberTypes { get; }

        /// <summary>
        /// Gets the <see cref="System.Type" /> containing the specified member.
        /// </summary>
        /// <remarks>
        /// If neither <see cref="Type" /> nor <see cref="TypeName" /> are specified,
        /// the type of the consumer is assumed.
        /// </remarks>
        public Type Type { get; }

        /// <summary>
        /// Gets the full name of the type containing the specified member.
        /// </summary>
        /// <remarks>
        /// If neither <see cref="Type" /> nor <see cref="TypeName" /> are specified,
        /// the type of the consumer is assumed.
        /// </remarks>
        public string TypeName { get; }

        /// <summary>
        /// Gets the assembly name of the specified type.
        /// </summary>
        /// <remarks>
        /// <see cref="AssemblyName" /> is only valid when <see cref="TypeName" /> is specified.
        /// </remarks>
        public string AssemblyName { get; }

        /// <summary>
        /// Gets or sets the condition in which the dependency is applicable, e.g. "DEBUG".
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDependencyAttribute" /> class
        /// with the specified signature of a member on the same type as the consumer.
        /// </summary>
        /// <param name="memberSignature">The signature of the member depended on.</param>
        public DynamicDependencyAttribute(string memberSignature)
        {
            MemberSignature = memberSignature;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDependencyAttribute" /> class
        /// with the specified signature of a member on a <see cref="System.Type" />.
        /// </summary>
        /// <param name="memberSignature">The signature of the member depended on.</param>
        /// <param name="type">The <see cref="System.Type" /> containing <paramref name="memberSignature" />.</param>
        public DynamicDependencyAttribute(string memberSignature, Type type)
        {
            MemberSignature = memberSignature;
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDependencyAttribute" /> class
        /// with the specified signature of a member on a type in an assembly.
        /// </summary>
        /// <param name="memberSignature">The signature of the member depended on.</param>
        /// <param name="typeName">The full name of the type containing the specified member.</param>
        /// <param name="assemblyName">The assembly name of the type containing the specified member.</param>
        public DynamicDependencyAttribute(string memberSignature, string typeName, string assemblyName)
        {
            MemberSignature = memberSignature;
            TypeName = typeName;
            AssemblyName = assemblyName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDependencyAttribute" /> class
        /// with the specified types of members on a <see cref="T:System.Type" />.
        /// </summary>
        /// <param name="memberTypes">The types of members depended on.</param>
        /// <param name="type">The <see cref="T:System.Type" /> containing the specified members.</param>
        public DynamicDependencyAttribute(DynamicallyAccessedMemberTypes memberTypes, Type type)
        {
            MemberTypes = memberTypes;
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDependencyAttribute" /> class
        /// with the specified types of members on a type in an assembly.
        /// </summary>
        /// <param name="memberTypes">The types of members depended on.</param>
        /// <param name="typeName">The full name of the type containing the specified members.</param>
        /// <param name="assemblyName">The assembly name of the type containing the specified members.</param>
        public DynamicDependencyAttribute(DynamicallyAccessedMemberTypes memberTypes, string typeName, string assemblyName)
        {
            MemberTypes = memberTypes;
            TypeName = typeName;
            AssemblyName = assemblyName;
        }
    }

    /// <summary>
    /// Indicates that the specified method requires the ability to generate new code at runtime,
    /// for example through <see cref="Reflection" />.
    /// </summary>
    /// <remarks>
    /// This allows tools to understand which methods are unsafe to call when compiling ahead of time.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
    internal sealed class RequiresDynamicCodeAttribute : Attribute
    {
        /// <summary>
        /// Gets a message that contains information about the usage of dynamic code.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets or sets an optional URL that contains more information about the method,
        /// why it requires dynamic code, and what options a consumer has to deal with it.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresDynamicCodeAttribute" /> class
        /// with the specified message.
        /// </summary>
        /// <param name="message">
        /// A message that contains information about the usage of dynamic code.
        /// </param>
        public RequiresDynamicCodeAttribute(string message)
        {
            Message = message;
        }
    }

    /// <summary>Specifies the syntax used in a string.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class StringSyntaxAttribute : Attribute
    {
        /// <summary>The syntax identifier for strings containing composite formats for string formatting.</summary>
        public const string CompositeFormat = "CompositeFormat";

        /// <summary>The syntax identifier for strings containing date format specifiers.</summary>
        public const string DateOnlyFormat = "DateOnlyFormat";

        /// <summary>The syntax identifier for strings containing date and time format specifiers.</summary>
        public const string DateTimeFormat = "DateTimeFormat";

        /// <summary>The syntax identifier for strings containing <see cref="T:System.Enum" /> format specifiers.</summary>
        public const string EnumFormat = "EnumFormat";

        /// <summary>The syntax identifier for strings containing <see cref="T:System.Guid" /> format specifiers.</summary>
        public const string GuidFormat = "GuidFormat";

        /// <summary>The syntax identifier for strings containing JavaScript Object Notation (JSON).</summary>
        public const string Json = "Json";

        /// <summary>The syntax identifier for strings containing numeric format specifiers.</summary>
        public const string NumericFormat = "NumericFormat";

        /// <summary>The syntax identifier for strings containing regular expressions.</summary>
        public const string Regex = "Regex";

        /// <summary>The syntax identifier for strings containing time format specifiers.</summary>
        public const string TimeOnlyFormat = "TimeOnlyFormat";

        /// <summary>The syntax identifier for strings containing <see cref="T:System.TimeSpan" /> format specifiers.</summary>
        public const string TimeSpanFormat = "TimeSpanFormat";

        /// <summary>The syntax identifier for strings containing URIs.</summary>
        public const string Uri = "Uri";

        /// <summary>The syntax identifier for strings containing XML.</summary>
        public const string Xml = "Xml";

        /// <summary>Gets the identifier of the syntax used.</summary>
        public string Syntax { get; }

        /// <summary>Optional arguments associated with the specific syntax employed.</summary>
        public object[] Arguments { get; }

        /// <summary>Initializes the <see cref="StringSyntaxAttribute" /> with the identifier of the syntax used.</summary>
        /// <param name="syntax">The syntax identifier.</param>
        public StringSyntaxAttribute(string syntax)
        {
            Syntax = syntax;
            Arguments = Array.Empty<object>();
        }

        /// <summary>Initializes the <see cref="StringSyntaxAttribute" /> with the identifier of the syntax used.</summary>
        /// <param name="syntax">The syntax identifier.</param>
        /// <param name="arguments">Optional arguments associated with the specific syntax employed.</param>
        public StringSyntaxAttribute(string syntax, params object[] arguments)
        {
            Syntax = syntax;
            Arguments = arguments;
        }
    }

}


namespace System
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Diagnostics.CodeAnalysis;

    internal static class InternalImports
    {
        public static System.Boolean Contains(this System.String value , System.Char character)
        {
            foreach (System.Char c in value) { if (character == c) return true; }
            return false;
        }

        public static System.String OrThrowIfNull(this System.String test)
        {
            if (test == null) { throw new ArgumentNullException(nameof(test)); }
            return test;
        }

        public static System.Boolean EndsInDirectorySeparator(System.String path)
        {
            System.String[] strings = new System.String[] { "\\" , "/" , System.IO.Path.DirectorySeparatorChar.ToString() };
            foreach (System.String s in strings) { if (path.EndsWith(s)) { return true; } }
            return false;
        }

        public static System.Object GetUninitializedObject(System.Type type)
        {
            if (type == null) { throw new System.ArgumentNullException(nameof(type)); }
            // When someone calls GetUninitializedObject , he will probably get the 'default' value from the structure.
            // This is not so simple to do. To do it , I have declared another method that is generic and gets it's default value. 
            System.Type cltype = typeof(InternalImports);
            return cltype.GetMethod("GetUntObject").MakeGenericMethod(type).Invoke(null, new System.Object[0]);
        }

        public static System.Object GetUntObject<T>() => default(T);
    }

#nullable enable
    /// <summary>Represent a range has start and end indexes.</summary>
    /// <remarks>
    /// Range is used by the C# compiler to support the range syntax.
    /// <code>
    /// int[] someArray = new int[5] { 1, 2, 3, 4, 5 };
    /// int[] subArray1 = someArray[0..2]; // { 1, 2 }
    /// int[] subArray2 = someArray[1..^0]; // { 2, 3, 4, 5 }
    /// </code>
    /// </remarks>
    internal readonly struct Range : IEquatable<Range>
    {
        /// <summary>Represent the inclusive start index of the Range.</summary>
        public Index Start { get; }

        /// <summary>Represent the exclusive end index of the Range.</summary>
        public Index End { get; }

        /// <summary>Construct a Range object using the start and end indexes.</summary>
        /// <param name="start">Represent the inclusive start index of the range.</param>
        /// <param name="end">Represent the exclusive end index of the range.</param>
        public Range(Index start, Index end)
        {
            Start = start;
            End = end;
        }

        /// <summary>Indicates whether the current Range object is equal to another object of the same type.</summary>
        /// <param name="value">An object to compare with this object</param>
        public override bool Equals([NotNullWhen(true)] object? value) =>
            value is Range r &&
            r.Start.Equals(Start) &&
            r.End.Equals(End);

        /// <summary>Indicates whether the current Range object is equal to another Range object.</summary>
        /// <param name="other">An object to compare with this object</param>
        public bool Equals(Range other) => other.Start.Equals(Start) && other.End.Equals(End);

        /// <summary>Returns the hash code for this instance.</summary>
        public override int GetHashCode()
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int Combine(int h1, int h2)
            {
                // RyuJIT optimizes this to use the ROL instruction
                // Related GitHub pull request: dotnet/coreclr#1830
                uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
                return ((int)rol5 + h1) ^ h2;
            }

            return Combine(Start.GetHashCode(), End.GetHashCode());
        }

        /// <summary>Converts the value of the current Range object to its equivalent string representation.</summary>
        public override string ToString()
        {
#if (!NETSTANDARD2_0 && !NETFRAMEWORK)
            Span<char> span = stackalloc char[2 + (2 * 11)]; // 2 for "..", then for each index 1 for '^' and 10 for longest possible uint
            int pos = 0;

            if (Start.IsFromEnd)
            {
                span[0] = '^';
                pos = 1;
            }
            bool formatted = ((uint)Start.Value).TryFormat(span.Slice(pos), out int charsWritten);
            Debug.Assert(formatted);
            pos += charsWritten;

            span[pos++] = '.';
            span[pos++] = '.';

            if (End.IsFromEnd)
            {
                span[pos++] = '^';
            }
            formatted = ((uint)End.Value).TryFormat(span.Slice(pos), out charsWritten);
            Debug.Assert(formatted);
            pos += charsWritten;

            return new string(span.Slice(0, pos));
#else
            return Start.ToString() + ".." + End.ToString();
#endif
        }

        /// <summary>Create a Range object starting from start index to the end of the collection.</summary>
        public static Range StartAt(Index start) => new Range(start, Index.End);

        /// <summary>Create a Range object starting from first element in the collection to the end Index.</summary>
        public static Range EndAt(Index end) => new Range(Index.Start, end);

        /// <summary>Create a Range object starting from first element to the end.</summary>
        public static Range All => new Range(Index.Start, Index.End);

        /// <summary>Calculate the start offset and length of range object using a collection length.</summary>
        /// <param name="length">The length of the collection that the range will be used with. length has to be a positive value.</param>
        /// <remarks>
        /// For performance reason, we don't validate the input length parameter against negative values.
        /// It is expected Range will be used with collections which always have non negative length/count.
        /// We validate the range is inside the length scope though.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (int Offset, int Length) GetOffsetAndLength(int length)
        {
            int start = Start.GetOffset(length);
            int end = End.GetOffset(length);

            if ((uint)end > (uint)length || (uint)start > (uint)end)
            {
                ThrowArgumentOutOfRangeException();
            }

            return (start, end - start);
        }

        private static void ThrowArgumentOutOfRangeException()
        {
            throw new ArgumentOutOfRangeException("length");
        }
    }

    /// <remarks>
    /// Index is used by the C# compiler to support the new index syntax
    /// <code>
    /// int[] someArray = new int[5] { 1, 2, 3, 4, 5 } ;
    /// int lastElement = someArray[^1]; // lastElement = 5
    /// </code>
    /// </remarks>
    internal readonly struct Index : IEquatable<Index>
    {
        private readonly int _value;

        /// <summary>Construct an Index using a value and indicating if the index is from the start or from the end.</summary>
        /// <param name="value">The index value. it has to be zero or positive number.</param>
        /// <param name="fromEnd">Indicating if the index is from the start or from the end.</param>
        /// <remarks>
        /// If the Index constructed from the end, index value 1 means pointing at the last element and index value 0 means pointing at beyond last element.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Index(int value, bool fromEnd = false)
        {
            if (value < 0)
            {
                ThrowValueArgumentOutOfRange_NeedNonNegNumException();
            }

            if (fromEnd)
                _value = ~value;
            else
                _value = value;
        }

        // The following private constructors mainly created for perf reason to avoid the checks
        private Index(int value)
        {
            _value = value;
        }

        /// <summary>Create an Index pointing at first element.</summary>
        public static Index Start => new Index(0);

        /// <summary>Create an Index pointing at beyond last element.</summary>
        public static Index End => new Index(~0);

        /// <summary>Create an Index from the start at the position indicated by the value.</summary>
        /// <param name="value">The index value from the start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Index FromStart(int value)
        {
            if (value < 0)
            {
                ThrowValueArgumentOutOfRange_NeedNonNegNumException();
            }

            return new Index(value);
        }

        /// <summary>Create an Index from the end at the position indicated by the value.</summary>
        /// <param name="value">The index value from the end.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Index FromEnd(int value)
        {
            if (value < 0)
            {
                ThrowValueArgumentOutOfRange_NeedNonNegNumException();
            }

            return new Index(~value);
        }

        /// <summary>Returns the index value.</summary>
        public int Value
        {
            get
            {
                if (_value < 0)
                    return ~_value;
                else
                    return _value;
            }
        }

        /// <summary>Indicates whether the index is from the start or the end.</summary>
        public bool IsFromEnd => _value < 0;

        /// <summary>Calculate the offset from the start using the giving collection length.</summary>
        /// <param name="length">The length of the collection that the Index will be used with. length has to be a positive value</param>
        /// <remarks>
        /// For performance reason, we don't validate the input length parameter and the returned offset value against negative values.
        /// we don't validate either the returned offset is greater than the input length.
        /// It is expected Index will be used with collections which always have non negative length/count. If the returned offset is negative and
        /// then used to index a collection will get out of range exception which will be same affect as the validation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffset(int length)
        {
            int offset = _value;
            if (IsFromEnd)
            {
                // offset = length - (~value)
                // offset = length + (~(~value) + 1)
                // offset = length + value + 1

                offset += length + 1;
            }
            return offset;
        }

        /// <summary>Indicates whether the current Index object is equal to another object of the same type.</summary>
        /// <param name="value">An object to compare with this object</param>
        public override bool Equals([NotNullWhen(true)] object? value) => value is Index && _value == ((Index)value)._value;

        /// <summary>Indicates whether the current Index object is equal to another Index object.</summary>
        /// <param name="other">An object to compare with this object</param>
        public bool Equals(Index other) => _value == other._value;

        /// <summary>Returns the hash code for this instance.</summary>
        public override int GetHashCode() => _value;

        /// <summary>Converts integer number to an Index.</summary>
        public static implicit operator Index(int value) => FromStart(value);

        /// <summary>Converts the value of the current Index object to its equivalent string representation.</summary>
        public override string ToString()
        {
            if (IsFromEnd)
                return ToStringFromEnd();

            return ((uint)Value).ToString();
        }

        private static void ThrowValueArgumentOutOfRange_NeedNonNegNumException()
        {
#if SYSTEM_PRIVATE_CORELIB
            throw new ArgumentOutOfRangeException("value", SR.ArgumentOutOfRange_NeedNonNegNum);
#else
            throw new ArgumentOutOfRangeException("value", "value must be non-negative");
#endif
        }

        private string ToStringFromEnd()
        {
#if (!NETSTANDARD2_0 && !NETFRAMEWORK)
            Span<char> span = stackalloc char[11]; // 1 for ^ and 10 for longest possible uint value
            bool formatted = ((uint)Value).TryFormat(span.Slice(1), out int charsWritten);
            Debug.Assert(formatted);
            span[0] = '^';
            return new string(span.Slice(0, charsWritten + 1));
#else
            return '^' + Value.ToString();
#endif
        }
    }
#nullable disable
}

#endif