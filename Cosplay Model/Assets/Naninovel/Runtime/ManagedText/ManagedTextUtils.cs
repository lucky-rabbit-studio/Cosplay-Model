// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    public static class ManagedTextUtils
    {
        private const BindingFlags managedFieldBindings = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        /// <summary>
        /// Creates new <see cref="ManagedText"/> from provided <see cref="FieldInfo"/>.
        /// Field should be static string with <see cref="ManagedTextAttribute"/>.
        /// </summary>
        public static ManagedText CreateManagedTextFromFieldInfo (FieldInfo fieldInfo)
        {
            var attribute = fieldInfo.GetCustomAttribute<ManagedTextAttribute>();
            Debug.Assert(attribute != null && fieldInfo.IsStatic && fieldInfo.FieldType == typeof(string));

            var fieldId = $"{fieldInfo.ReflectedType}.{fieldInfo.Name}";
            var fieldValue = fieldInfo.GetValue(null) as string;
            var category = attribute.Category;
            var comment = attribute.Comment;
            return new ManagedText(fieldId, fieldValue, category, comment);
        }

        /// <summary>
        /// Scans all the static <see cref="string"/> fields marked with <see cref="ManagedTextAttribute"/> in the assembly,
        /// transforming them to a <see cref="HashSet{T}"/> of <see cref="ManagedText"/>.
        /// </summary>
        public static HashSet<ManagedText> GetManagedTextFromAssembly ()
        {
            var result = ReflectionUtils.ExportedDomainTypes
                .SelectMany(type => type.GetFields(managedFieldBindings))
                .Where(field => field.IsDefined(typeof(ManagedTextAttribute)))
                .Select(field => CreateManagedTextFromFieldInfo(field)).ToList();

            // Add display names for the existing character metadata.
            var charConfig = Configuration.LoadOrDefault<CharactersConfiguration>();
            foreach (var kv in charConfig.Metadata.ToDictionary())
                result.Add(new ManagedText(kv.Key, kv.Value.DisplayName, CharactersConfiguration.DisplayNamesCategory, null));

            return new HashSet<ManagedText>(result);
        }

        /// <summary>
        /// Scans all the static <see cref="string"/> fields marked with <see cref="ManagedTextAttribute"/> in the type,
        /// transforming them to a <see cref="HashSet{T}"/> of <see cref="ManagedText"/>.
        /// </summary>
        public static HashSet<ManagedText> GetManagedTextFromType (Type type)
        {
            var result = type.GetFields(managedFieldBindings)
                .Where(field => field.IsDefined(typeof(ManagedTextAttribute)))
                .Select(field => CreateManagedTextFromFieldInfo(field));
            return new HashSet<ManagedText>(result);
        }

        /// <summary>
        /// Parses provided managed text <see cref="NovelScript"/> resource.
        /// </summary>
        /// <remarks>
        /// Kinda hacky, but we treat managed text resources as novel scripts here for convenience.
        /// Managed field ID is assoicated with actor name and value is the text of the <see cref="PrintText"/>.
        /// </remarks>
        public static HashSet<ManagedText> GetManagedTextFromScript (NovelScript managedTextScript)
        {
            var managedTextSet = new HashSet<ManagedText>();
            var printActions = managedTextScript.CollectAllActionLines().Select(l => NovelAction.FromScriptLine(l)).OfType<PrintText>();

            foreach (var printTextAction in printActions)
            {
                var fieldId = printTextAction.ActorId;
                if (string.IsNullOrEmpty(fieldId)) continue;
                var fieldValue = printTextAction.Text;
                // When actual value is not set in the resource, set ID instead to make it clear which field is missing.
                if (string.IsNullOrEmpty(fieldValue)) fieldValue = fieldId;
                var category = managedTextScript.Name;
                var comment = managedTextScript.GetCommentForLine(printTextAction.LineIndex);
                var managedText = new ManagedText(fieldId, fieldValue, category, comment);
                managedTextSet.Add(managedText);
            }

            return managedTextSet;
        }

        /// <summary>
        /// Scans entire type system setting static <see cref="string"/> field values with
        /// <see cref="ManagedTextAttribute"/> to corresponding values from the provided set.
        /// </summary>
        public static void SetManagedTextValues (HashSet<ManagedText> managedTextSet)
        {
            foreach (var managedText in managedTextSet)
            {
                var fieldPath = managedText.FieldId;
                var fieldValue = managedText.FieldValue;
                var typeFullName = fieldPath.GetBeforeLast(".") ?? fieldPath;
                var fieldName = fieldPath.GetAfter(".") ?? fieldPath;

                var type = Type.GetType(typeFullName);
                if (type is null) continue;

                var fieldInfo = type.GetField(fieldName, managedFieldBindings);
                if (fieldInfo is null) { Debug.LogWarning($"TextManager: Field '{fieldName}' isn't found in '{typeFullName}'."); continue; }

                fieldInfo.SetValue(null, fieldValue);
            }
        }
    }
}
