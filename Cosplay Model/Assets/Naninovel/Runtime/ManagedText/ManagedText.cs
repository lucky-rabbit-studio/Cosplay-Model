// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;

namespace Naninovel
{
    public readonly struct ManagedText : IEquatable<ManagedText>
    {
        public readonly string FieldId;
        public readonly string FieldValue;
        public readonly string Category;
        public readonly string Comment;

        public ManagedText (string fieldId, string fieldValue, string category, string comment)
        {
            FieldId = fieldId;
            FieldValue = fieldValue;
            Category = category;
            Comment = comment;
        }

        public override bool Equals (object obj)
        {
            return obj is ManagedText && Equals((ManagedText)obj);
        }

        public bool Equals (ManagedText other)
        {
            return FieldId == other.FieldId;
        }

        public override int GetHashCode ()
        {
            return 1711119226 + EqualityComparer<string>.Default.GetHashCode(FieldId);
        }

        public static bool operator == (ManagedText text1, ManagedText text2)
        {
            return text1.Equals(text2);
        }

        public static bool operator != (ManagedText text1, ManagedText text2)
        {
            return !(text1 == text2);
        }
    }
}
