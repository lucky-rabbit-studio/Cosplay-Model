// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class RevealableUIText : Text, IRevealableText
    {
        private readonly struct CharInfo
        {
            public static readonly CharInfo Invalid = new CharInfo(-1, -1, new UICharInfo { charWidth = 0 }, default);

            public readonly int CharIndex;
            public readonly int LineIndex;
            public readonly UICharInfo Char;
            public readonly UILineInfo Line;

            public bool Visible => Char.charWidth > 0;
            public float Origin => Char.cursorPos.x;
            public float XAdvance => Char.cursorPos.x + Char.charWidth;
            public float Ascender => Line.topY;

            public CharInfo (int charIndex, int lineIndex, UICharInfo @char, UILineInfo line)
            {
                CharIndex = charIndex;
                LineIndex = lineIndex;
                Char = @char;
                Line = line;
            }
        }

        public string Text { get => text; set { text = value; rebuildPending = rebuildPending || rectTransform.hasChanged; } }
        public Color TextColor { get => color; set => color = value; }
        public GameObject GameObject => gameObject;
        public bool IsFullyRevealed => !rebuildPending && RevealProgress >= 1f;
        public float RevealProgress { get => GetRevealProgress(); set => SetRevealProgress(value); }

        protected int LastRevealedVisibleCharIndex { get; private set; }
        protected int LastVisibleCharIndex { get; private set; }
        protected Transform CanvasTransform => canvasTransformCache ? canvasTransformCache : (canvasTransformCache = canvas.GetComponent<Transform>());

        [Tooltip("Width (in pixels) of the gradient fade near the reveal border.")]
        [SerializeField] private float revealFadeWidth = 100f;
        [Tooltip("Whether to smoothly reveal the text. Disable for the `typewriter` effect.")]
        [SerializeField] private bool slideClipRect = true;

        private static readonly int lineClipRectPropertyId = Shader.PropertyToID("_LineClipRect");
        private static readonly int charClipRectPropertyId = Shader.PropertyToID("_CharClipRect");
        private static readonly int charFadeWidthPropertyId = Shader.PropertyToID("_CharFadeWidth");
        private static readonly int charSlantAnglePropertyId = Shader.PropertyToID("_CharSlantAngle");

        private Transform canvasTransformCache;
        private Vector3[] worldCorners = new Vector3[4];
        private Vector3[] canvasCorners = new Vector3[4];
        private Vector4 curLineClipRect, curCharClipRect;
        private float curCharFadeWidth, curCharSlantAngle;
        private CharInfo revealStartChar;
        private float lastRevealDelay, lastRevealTime, lastRevealClipX, lastRevealFadeWidth;
        private bool rebuildPending, revealAllAfterRebuild;

        public void RevealAll ()
        {
            if (rebuildPending) revealAllAfterRebuild = true;
            else SetLastRevealedVisibleCharIndex(LastVisibleCharIndex);
        }

        public void HideAll ()
        {
            SetLastRevealedVisibleCharIndex(-1);
        }

        public bool RevealNextChar (float revealDelay)
        {
            // While rebuild is pending, we can't rely on visible char indexes, so make the caller wait.
            if (rebuildPending) return true;

            lastRevealDelay = Mathf.Max(revealDelay, 0);
            
            if (LastRevealedVisibleCharIndex >= LastVisibleCharIndex)
                return false;

            // Skip invisible characters (eg, formating tags).
            var nextVisibleCharIndex = FindNextVisibleCharIndex(LastRevealedVisibleCharIndex);
            if (nextVisibleCharIndex == -1) // No visible characters left to reveal.
            {
                RevealAll();
                return false;
            }

            SetLastRevealedVisibleCharIndex(nextVisibleCharIndex);

            return true;
        }

        public Vector2 GetLastRevealedCharPosition ()
        {
            UpdateClipRects();
            var lastChar = GetVisibleCharAt(LastRevealedVisibleCharIndex);
            var localPos = new Vector2(curCharClipRect.x, curCharClipRect.w - lastChar.Line.height / pixelsPerUnit);
            return CanvasTransform.TransformPoint(localPos);
        }

        public char GetLastRevealedChar ()
        {
            var absIndex = VisibleToAbsoluteCharIndex(LastRevealedVisibleCharIndex);
            if (Text is null || absIndex < 0 || absIndex >= Text.Length)
                return default;
            return Text[absIndex];
        }

        public override void Rebuild (CanvasUpdate update)
        {
            base.Rebuild(update);

            // Visible char indexes could potentially change after the rebuild; recalculate them.
            LastVisibleCharIndex = FindLastVisibleCharIndex();
            // Set current last revealed char as the start position for the reveal effect to 
            // prevent it from affecting this char again when resuming the revealing without resetting the text.
            revealStartChar = GetVisibleCharAt(LastRevealedVisibleCharIndex);

            rebuildPending = false;

            if (revealAllAfterRebuild)
            {
                RevealAll();
                UpdateClipRects();
                revealAllAfterRebuild = false;
            }
        }

        protected override void Start ()
        {
            base.Start();
            if (!Application.isPlaying) return; // Text : ... : Graphic has [ExecuteInEditMode]

            material = Instantiate(material);
        }

        protected override void OnRectTransformDimensionsChange ()
        {
            base.OnRectTransformDimensionsChange();
            if (!Application.isPlaying) return; // Text : ... : Graphic has [ExecuteInEditMode]

            // When text layout changes (eg, content size fitter decides to increase height),
            // we need to force-update clip rect; otherwise it will be delayed by one frame
            // and user fill see incorrectly revealed text for a moment.
            UpdateClipRects();
            Update();
        }

        private void Update ()
        {
            if (!Application.isPlaying) return; // TextMeshProUGUI has [ExecuteInEditMode]
            
            if (slideClipRect)
            {
                var slideProgress = lastRevealDelay <= 0 ? 1f : (Time.time - lastRevealTime) / lastRevealDelay;
                var slidedCharClipRectX = Mathf.Lerp(lastRevealClipX, curCharClipRect.x, slideProgress);
                var slidedCharClipRect = new Vector4(slidedCharClipRectX, curCharClipRect.y, curCharClipRect.z, curCharClipRect.w);
                var slidedFadeWidth = Mathf.Lerp(lastRevealFadeWidth, curCharFadeWidth, slideProgress);
                SetMaterialProperties(curLineClipRect, slidedCharClipRect, slidedFadeWidth, curCharSlantAngle);
            }
            else SetMaterialProperties(curLineClipRect, curCharClipRect, curCharFadeWidth, curCharSlantAngle);
            
            //Debug.DrawLine(CanvasTransform.TransformPoint(new Vector3(curLineClipRect.x, curLineClipRect.y)), CanvasTransform.TransformPoint(new Vector3(curLineClipRect.z, curLineClipRect.w)), Color.blue);
            //Debug.DrawLine(CanvasTransform.TransformPoint(new Vector3(curCharClipRect.x, curCharClipRect.y)), CanvasTransform.TransformPoint(new Vector3(curCharClipRect.z, curCharClipRect.w)), Color.red);
        }

        private void SetMaterialProperties (Vector4 lineClipRect, Vector4 charClipRect, float charFadeWidth, float charSlantAngle)
        {
            material.SetVector(lineClipRectPropertyId, lineClipRect);
            material.SetVector(charClipRectPropertyId, charClipRect);
            material.SetFloat(charFadeWidthPropertyId, charFadeWidth);
            material.SetFloat(charSlantAnglePropertyId, charSlantAngle);
        }

        private void SetLastRevealedVisibleCharIndex (int visibleCharIndex)
        {
            if (LastRevealedVisibleCharIndex == visibleCharIndex) return;

            if (slideClipRect)
            {
                var curChar = GetVisibleCharAt(LastRevealedVisibleCharIndex);
                var nextChar = GetVisibleCharAt(visibleCharIndex);
                var resetSlide = visibleCharIndex == LastVisibleCharIndex || visibleCharIndex == -1 || curChar.LineIndex != nextChar.LineIndex;
                if (visibleCharIndex < 0) lastRevealClipX = curLineClipRect.x; // Clearing the content and next char is unavailable, start at the line clip rect.
                else if (resetSlide) // Use x position of the char clip rect that is going to be calculated for the next revealed char (at the UpdateClipRects).
                    lastRevealClipX = GetTextCornersInCanvasSpace().x + (nextChar.Origin + rectTransform.pivot.x * cachedTextGenerator.rectExtents.width) / pixelsPerUnit;
                else lastRevealClipX = curCharClipRect.x; // Otherwise, use char clip rect.
                lastRevealFadeWidth = resetSlide ? 0 : curCharFadeWidth;
                lastRevealTime = Time.time;
            }

            LastRevealedVisibleCharIndex = visibleCharIndex;
            UpdateClipRects();
        }

        private float GetRevealProgress ()
        {
            if (LastVisibleCharIndex <= 0) return LastRevealedVisibleCharIndex >= 0 ? 1f : 0f;
            return Mathf.Clamp01(LastRevealedVisibleCharIndex / (float)LastVisibleCharIndex);
        }

        private void SetRevealProgress (float revealProgress)
        {
            revealProgress = Mathf.Clamp01(revealProgress);
            var charIndex = Mathf.CeilToInt(LastVisibleCharIndex * revealProgress);
            SetLastRevealedVisibleCharIndex(charIndex);
        }

        private void UpdateClipRects ()
        {
            if (LastRevealedVisibleCharIndex > LastVisibleCharIndex) return;

            var fullClipRect = GetTextCornersInCanvasSpace();

            if (LastRevealedVisibleCharIndex < 0) // Hide all.
            {
                curLineClipRect = fullClipRect;
                curCharClipRect = fullClipRect;
                return;
            }

            var currentChar = GetVisibleCharAt(LastRevealedVisibleCharIndex);
            var lineTopY = currentChar.Ascender + (rectTransform.pivot.y - 1f) * cachedTextGenerator.rectExtents.height;
            var lineBottomY = lineTopY - currentChar.Line.height;
            var charRightX = currentChar.XAdvance + rectTransform.pivot.x * cachedTextGenerator.rectExtents.width;

            curLineClipRect = fullClipRect + new Vector4(0, 0, 0, lineBottomY / pixelsPerUnit);
            curCharClipRect = fullClipRect + new Vector4(charRightX / pixelsPerUnit, 0, 0, lineTopY / pixelsPerUnit);
            curCharClipRect.y = curLineClipRect.w;

            var lineFirstChar = GetVisibleCharAt(AbsoluteToVisibleCharIndex(currentChar.Line.startCharIdx));
            var lineLastChar = GetLastVisibleCharAtLine(lineFirstChar.CharIndex, currentChar.LineIndex);
            // When starting from a new line, add extra fade width to fade the first character.
            var startLimit = currentChar.LineIndex == revealStartChar.LineIndex ? currentChar.Origin - revealStartChar.Origin : currentChar.XAdvance - lineFirstChar.Origin;
            var finishLimit = lineLastChar.Origin - currentChar.Origin;
            var widthLimit = Mathf.Min(Mathf.Abs(startLimit), Mathf.Abs(finishLimit));
            curCharFadeWidth = Mathf.Clamp(revealFadeWidth, 0f, widthLimit);

            // TODO: Find a way to find the slant (italic) angle of the last revealed character.
            curCharSlantAngle = 10f;
        }

        private Vector4 GetTextCornersInCanvasSpace ()
        {
            rectTransform.GetWorldCorners(worldCorners);
            for (int i = 0; i < 4; i++)
                canvasCorners[i] = CanvasTransform.InverseTransformPoint(worldCorners[i]);

            // Positions of diagonal corners.
            return new Vector4(canvasCorners[0].x, canvasCorners[0].y, canvasCorners[2].x, canvasCorners[2].y);
        }

        private CharInfo GetVisibleCharAt (int requestedVisibleCharIndex)
        {
            var absoluteIndex = VisibleToAbsoluteCharIndex(requestedVisibleCharIndex);
            if (absoluteIndex < 0 || absoluteIndex >= cachedTextGenerator.characterCount)
                return CharInfo.Invalid;

            var lineInfo = FindLineContainingChar(absoluteIndex, out var lineIndex);
            var visibleCharInfo = cachedTextGenerator.characters[absoluteIndex];
            return new CharInfo(requestedVisibleCharIndex, lineIndex, visibleCharInfo, lineInfo);
        }

        private CharInfo GetLastVisibleCharAtLine (int firstVisibleCharInLineIndex, int lineIndex)
        {
            var curVisibleIndex = -1;
            for (int i = 0; i < cachedTextGenerator.characterCount; i++)
            {
                if (cachedTextGenerator.characters[i].charWidth == 0f) continue;
                curVisibleIndex++;
                if (curVisibleIndex < firstVisibleCharInLineIndex) continue;

                FindLineContainingChar(i, out var curLindeIndex);
                if (lineIndex != curLindeIndex) break;
            }
            return GetVisibleCharAt(curVisibleIndex);
        }

        private UILineInfo FindLineContainingChar (int absoluteCharIndex, out int lineIndex)
        {
            lineIndex = 0;
            for (int i = 0; i < cachedTextGenerator.lineCount; i++)
            {
                if (cachedTextGenerator.lines[i].startCharIdx > absoluteCharIndex)
                    break;
                lineIndex = i;
            }
            return cachedTextGenerator.lines[lineIndex];
        }

        private int FindNextVisibleCharIndex (int startVisibleCharIndex = 0)
        {
            var curVisibleIndex = -1;
            for (int i = 0; i < cachedTextGenerator.characterCount; i++)
            {
                if (cachedTextGenerator.characters[i].charWidth == 0f) continue;
                curVisibleIndex++;
                if (curVisibleIndex <= startVisibleCharIndex) continue;
                return curVisibleIndex;
            }
            return -1;
        }

        private int FindLastVisibleCharIndex ()
        {
            var curVisibleIndex = -1;
            for (int i = 0; i < cachedTextGenerator.characterCount; i++)
            {
                if (cachedTextGenerator.characters[i].charWidth == 0f) continue;
                curVisibleIndex++;
            }
            return curVisibleIndex;
        }

        private int AbsoluteToVisibleCharIndex (int absoluteCharIndex)
        {
            var curVisibleIndex = -1;
            for (int i = 0; i < cachedTextGenerator.characterCount; i++)
            {
                if (cachedTextGenerator.characters[i].charWidth == 0f) continue;
                curVisibleIndex++;
                if (i >= absoluteCharIndex) break;
            }
            return curVisibleIndex;
        }

        private int VisibleToAbsoluteCharIndex (int visibleCharIndex)
        {
            var curVisibleIndex = -1;
            for (int i = 0; i < cachedTextGenerator.characterCount; i++)
            {
                if (cachedTextGenerator.characters[i].charWidth == 0f) continue;
                curVisibleIndex++;
                if (curVisibleIndex >= visibleCharIndex) return i;
            }
            return -1;
        }
    }
}
