// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Represents a managed text document record.
    /// </summary>
    public readonly struct ManagedTextRecord : IEquatable<ManagedTextRecord>
    {
        /// <summary>
        /// Name of the category (managed text document name) to use when
        /// no category is specified in <see cref="ManagedTextAttribute"/>.
        /// </summary>
        public const string DefaultCategoryName = "Uncategorized";

        /// <summary>
        /// Category (managed text document name) for which the record belongs to.
        /// </summary>
        public string Category { get; }
        /// <summary>
        /// Unique (inside category) key of the record.
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Value of the record.
        /// </summary>
        public string Value { get; }

        public ManagedTextRecord (string key, string value, string category = DefaultCategoryName)
        {
            Category = category;
            Key = key;
            Value = value;
        }

        public override bool Equals (object obj)
        {
            return obj is ManagedTextRecord && Equals((ManagedTextRecord)obj);
        }

        public bool Equals (ManagedTextRecord other)
        {
            return Category == other.Category &&
                   Key == other.Key;
        }

        public override int GetHashCode ()
        {
            var hashCode = 1514713669;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Category);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Key);
            return hashCode;
        }

        public static bool operator == (ManagedTextRecord record1, ManagedTextRecord record2)
        {
            return record1.Equals(record2);
        }

        public static bool operator != (ManagedTextRecord record1, ManagedTextRecord record2)
        {
            return !(record1 == record2);
        }
    }
}
