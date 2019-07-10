// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Linq;
using UnityCommon;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    [CustomEditor(typeof(ScriptAsset))]
    public class ScriptAssetEditor : Editor
    {
        private const int previewLengthLimit = 5000;

        private string previewContent;
        private float labelsHeight, gotosHeight;
        private GUIContent[] labelTags;
        private GUIContent[] gotoTags;

        public override void OnInspectorGUI ()
        {
            var editorWasEnabled = GUI.enabled;
            GUI.enabled = true;

            if (labelTags != null && labelTags.Length > 0)
                DrawTags(labelTags, GUIStyles.ScriptLabelTag, ref labelsHeight);
            if (gotoTags != null && gotoTags.Length > 0)
                DrawTags(gotoTags, GUIStyles.ScriptGotoTag, ref gotosHeight);

            GUI.enabled = false;
            EditorGUILayout.LabelField(previewContent, EditorStyles.wordWrappedMiniLabel);

            GUI.enabled = editorWasEnabled;
        }

        private void OnEnable ()
        {
            var scriptAsset = (ScriptAsset)target;

            previewContent = scriptAsset.ScriptText;
            if (previewContent.Length > previewLengthLimit)
            {
                previewContent = previewContent.Substring(0, previewLengthLimit);
                previewContent += $"{System.Environment.NewLine}<...>";
            }

            var script = new Script(scriptAsset.name, scriptAsset.ScriptText, ignoreErrors: true);
            labelTags = script.LabelLines.Select(l => new GUIContent($"# {l.LabelText}")).ToArray();
            gotoTags = script.CommandLines
                .Where(c => c.CommandName.EqualsFastIgnoreCase("goto") && c.CommandParameters.TryGetValue(string.Empty, out var path) && !path.StartsWithFast("."))
                .Select(c => new GUIContent($"@goto {c.CommandParameters[string.Empty]}")).ToArray();
        }

        private void DrawTags (GUIContent[] tags, GUIStyle style, ref float heightBuffer)
        {
            const float horPadding = 5f;
            const float verPadding = 5f;

            if (Event.current.type == EventType.Repaint)
                heightBuffer = 0;

            GUILayout.Space(verPadding);

            // Create a rect to test how wide the label list can be.
            var widthProbeRect = GUILayoutUtility.GetRect(0, 10240, 0, 0);
            var rect = EditorGUILayout.GetControlRect();
            var controlHorPadding = rect.x;
            for (int i = 0; i < tags.Length; i++)
            {
                var content = tags[i];
                var labelSize = style.CalcSize(content);

                if (Event.current.type == EventType.Repaint && (rect.x + labelSize.x) >= widthProbeRect.xMax)
                {
                    var addHeight = GUIStyles.TagIcon.fixedHeight + verPadding;
                    rect.y += GUIStyles.TagIcon.fixedHeight + verPadding;
                    rect.x = controlHorPadding;
                    heightBuffer += addHeight;
                }

                var labelRect = new Rect(rect.x, rect.y, labelSize.x, labelSize.y);
                GUI.Label(labelRect, content, style);

                rect.x += labelSize.x + horPadding;
            }

            GUILayoutUtility.GetRect(0, heightBuffer);
        }
    }
}
