// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// When applied to a static string field, the field's value will be ovewritten
    /// by the <see cref="TextManager"/> service (during initialization) based on the managed text resource.
    /// The field will also be included to the generated managed text resources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class ManagedTextAttribute : Attribute
    {
        /// <summary>
        /// Category of the generated text resource.
        /// </summary>
        public string Category { get; }
        /// <summary>
        /// Commentary to put before the field record in the generated managed text resource.
        /// </summary>
        public string Comment { get; }

        /// <param name="category">Category of the generated text resource.</param>
        /// <param name="comment">Commentary to put before the field record in the generated managed text resource.</param>
        public ManagedTextAttribute (string category = ManagedTextRecord.DefaultCategoryName, string comment = null)
        {
            Category = category;
            Comment = comment;
        }
    }
}
