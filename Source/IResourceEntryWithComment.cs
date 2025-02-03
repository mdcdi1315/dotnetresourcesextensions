﻿using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// A simple descendant of <see cref="IResourceEntry"/> that also embeds a comment string to write in the list of resources.
    /// </summary>
    public interface IResourceEntryWithComment : IResourceEntry
    {
        /// <summary>
        /// Gets or sets the comment for this resource entry. <br />
        /// Comments may be modified at any time!
        /// </summary>
        public System.String Comment { get; set; }
    }

    /// <summary>
    /// Defines common extensions for the <see cref="IResourceEntryWithComment"/> interface.
    /// </summary>
    public static class IResourceEntryWithCommentExtensions
    {
        [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
        private sealed class DefaultResourceEntryWithComment : IResourceEntryWithComment
        {
            private System.String name, cmt;
            private System.Object value;
            private System.Boolean iscloned;

            private DefaultResourceEntryWithComment() {
                cmt = System.String.Empty;
                iscloned = false;
            }

            public DefaultResourceEntryWithComment(IResourceEntry ent) : this()
            {
                name = ent.Name;
                value = ent.Value;
            }

            public DefaultResourceEntryWithComment(IResourceEntryWithComment entry) : this()
            {
                name = entry.Name;
                value = entry.Value;
                cmt = entry.Comment;
            }

            public DefaultResourceEntryWithComment(System.String Name , System.Object Value , System.String Comment) : this()
            {
                name = Name;
                value = Value;
                cmt = Comment;
            }

            public static DefaultResourceEntryWithComment Clone(IResourceEntryWithComment ent) => new(ent) { iscloned = true };

            public string Comment { get => cmt; set => cmt = value; }

            public string Name => name;

            public object Value => value;

            public Type TypeOfValue => value?.GetType();

            public System.Boolean IsCloned => iscloned;

            public override string ToString() => $"{nameof(IResourceEntryWithComment)}: {{ Name = {name} , Value = {value} , Comment = {cmt} }}";

            private string GetDebuggerDisplay() => ToString();
        }

        /// <summary>
        /// Reinterprets the current instance as a instance of <see cref="Collections.DictionaryEntryWithComment"/> structure.
        /// </summary>
        /// <param name="entry">The entry to reinterpret.</param>
        /// <returns>The reintepreted version of <paramref name="entry"/>.</returns>
        public static Collections.DictionaryEntryWithComment AsDictionaryEntryWithComment(this IResourceEntryWithComment entry)
            => new(entry.Name, entry.Value, entry.Comment);

        /// <summary>
        /// Reinterprets the specified resource entry as a tuple triple , which it's first item is the resource name ,
        /// the second one contains the resource entry value , and the third one the entry comment.
        /// </summary>
        /// <param name="entry">The resource entry to reinterpret.</param>
        /// <returns>A <see cref="System.Tuple{T1, T2 , T3}"/> , or triple that is a reinterpreted version of the resource entry given.</returns>
        public static System.Tuple<System.String , System.Object , System.String> AsTuple(this IResourceEntryWithComment entry)
            => new(entry.Name , entry.Value, entry.Comment);

        /// <summary>
        /// Reinterprets the specified resource entry as a tuple triple , which it's first item is the resource name ,
        /// the second one contains the resource entry value , and the third one the entry comment.
        /// </summary>
        /// <param name="entry">The resource entry to reinterpret.</param>
        /// <returns>A <see cref="System.ValueTuple{T1, T2 , T3}"/> , or triple that is a reinterpreted version of the resource entry given.</returns>
        public static System.ValueTuple<System.String, System.Object, System.String> AsValueTuple(this IResourceEntryWithComment entry)
            => new(entry.Name, entry.Value, entry.Comment);

        /// <summary>
        /// Reinterprets the specified tuple pair as a new resource entry. The tuple's first item must be the string
        /// that is the resource name , the second one an object that represents the resource value , and the third one the entry comment.
        /// </summary>
        /// <param name="tuple">The tuple to reinterpret as a resource entry.</param>
        /// <returns>An object that implements the <see cref="IResourceEntryWithComment"/> interface and has as it's values 
        /// the values took from the <paramref name="tuple"/>.</returns>
        public static IResourceEntryWithComment AsResourceEntryWithComment(this System.Tuple<System.String , System.Object , System.String> tuple)
            => new DefaultResourceEntryWithComment(tuple.Item1 , tuple.Item2 , tuple.Item3);

        /// <summary>
        /// Reinterprets the specified tuple pair as a new resource entry. The tuple's first item must be the string
        /// that is the resource name , the second one an object that represents the resource value , and the third one the entry comment.
        /// </summary>
        /// <param name="tuple">The tuple to reinterpret as a resource entry.</param>
        /// <returns>An object that implements the <see cref="IResourceEntryWithComment"/> interface and has as it's values 
        /// the values took from the <paramref name="tuple"/>.</returns>
        public static IResourceEntryWithComment AsResourceEntryWithComment(this System.ValueTuple<System.String , System.Object , System.String> tuple)
            => new DefaultResourceEntryWithComment(tuple.Item1, tuple.Item2, tuple.Item3);

        /// <summary>
        /// Creates a new resource entry and returns a new entry that has a comment. 
        /// The comment for the newly created entry is provided from the <paramref name="comment"/> parameter.
        /// </summary>
        /// <param name="entry">The original resource entry to create an equivalent <see cref="IResourceEntryWithComment"/>.</param>
        /// <param name="comment">The comment for the newly created entry.</param>
        /// <returns>A new object that implements the <see cref="IResourceEntryWithComment"/> interface but with the values took from <paramref name="entry"/> and <paramref name="comment"/>.</returns>
        public static IResourceEntryWithComment CreateEntryWithComment(this IResourceEntry entry , System.String comment)
            => new DefaultResourceEntryWithComment(entry.Name, entry.Value, comment);

        /// <summary>
        /// Clones the specified resource entry to a new resource entry. <br />
        /// Works for all <see cref="IResourceEntryWithComment"/> descendants.
        /// </summary>
        /// <param name="ent">The entry to clone.</param>
        /// <returns>The cloned entry.</returns>
        public static IResourceEntryWithComment Clone(this IResourceEntryWithComment ent) => DefaultResourceEntryWithComment.Clone(ent);

        /// <summary>
        /// Determines whether this entry is a cloned entry (That is , the created object from <see cref="Clone(IResourceEntryWithComment)"/> method). 
        /// </summary>
        /// <param name="entry">The entry to test.</param>
        /// <returns>A value that determines if this entry is cloned.</returns>
        public static System.Boolean IsClonedEntry(this IResourceEntryWithComment entry) => entry is DefaultResourceEntryWithComment d && d.IsCloned;

        /// <summary>
        /// Defines a generalized method to deconstruct an <see cref="IResourceEntryWithComment"/>-derived class. <br />
        /// Together with the language internals , this method can be used to deconstruct such an instance immediately. <br />
        /// If your implementing class provides more information that you need also to be passed, you must create your own deconstruction method overload.
        /// </summary>
        /// <param name="entry">The resource entry to deconstruct</param>
        /// <param name="Name">The deconstructed entry name.</param>
        /// <param name="Value">The deconstructed entry value.</param>
        /// <param name="Comment">The deconstructed entry comment.</param>
        public static void Deconstruct(this IResourceEntryWithComment entry , out System.String Name , out System.Object Value , out System.String Comment)
        {
            Name = entry.Name;
            Value = entry.Value;
            Comment = entry.Comment;
        }

        /// <summary>
        /// Determines whether the given resource entry is a object that implements the <see cref="IResourceEntryWithComment"/> interface.
        /// </summary>
        /// <param name="entry">The entry to test.</param>
        /// <returns>A value whether the given object is a object that implements the <see cref="IResourceEntryWithComment"/> interface.</returns>
        public static System.Boolean IsResourceEntryWithComment(this IResourceEntry entry) => entry is IResourceEntryWithComment;
    
        /// <summary>
        /// Attempts to retrieve this resource entry as a instance of <see cref="IResourceEntryWithComment"/>.
        /// </summary>
        /// <param name="entry">The entry to test and retrieve for.</param>
        /// <param name="entrywithcomment">The resulting instance , if <paramref name="entry"/> can be an <see cref="IResourceEntryWithComment"/> descendant.</param>
        /// <returns><see langword="true"/> when the instance was successfully got as a <see cref="IResourceEntryWithComment"/> instance and <paramref name="entrywithcomment"/> contains the result; otherwise , <see langword="false"/>.</returns>
        public static System.Boolean TryGetResourceEntryWithComment(this IResourceEntry entry , out IResourceEntryWithComment entrywithcomment)
        {
            System.Boolean ret;
            if (entry is IResourceEntryWithComment cmt) {
                ret = true;
                entrywithcomment = cmt;
            } else {
                ret = false;
                entrywithcomment = null;
            }
            return ret;
        }
    }
}
