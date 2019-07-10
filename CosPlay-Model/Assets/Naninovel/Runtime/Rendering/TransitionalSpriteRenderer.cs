// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer)), DisallowMultipleComponent]
    public class TransitionalSpriteRenderer : MonoBehaviour
    {
        public Texture MainTexture { get => material.mainTexture; set { material.mainTexture = value; RebuildMeshQuad(); } }
        public Texture TransitionTexture { get => material.GetTexture(transitionTexName); set { material.SetTexture(transitionTexName, value); RebuildMeshQuad(); } }
        public Texture CustomDissolveTexture { get => material.GetTexture(customDissolveTexName); set => material.SetTexture(customDissolveTexName, value); }
        public TransitionType TransitionType { get => TransitionUtils.GetEnabled(material); set => TransitionUtils.EnableKeyword(material, value); }
        public float TransitionProgress { get => material.GetFloat(transitionProgressName); set => material.SetFloat(transitionProgressName, value); }
        public Vector4 TransitionParams { get => material.GetVector(transitionParamsName); set => material.SetVector(transitionParamsName, value); }
        public Vector2 RandomSeed { get => material.GetVector(randomSeedName); set => material.SetVector(randomSeedName, value); }
        public Color TintColor { get => material.GetColor(tintColorName); set => material.SetColor(tintColorName, value); }
        public float Opacity { get => material.GetColor(tintColorName).a; set => SetOpacity(value); }
        public bool FlipX { get => material.GetVector(flipName).x == -1; set => SetFlipX(value); }
        public bool FlipY { get => material.GetVector(flipName).y == -1; set => SetFlipY(value); }
        public Vector2 Pivot { get => pivot; set { if (value != Pivot) { pivot = value; RebuildMeshQuad(); }; } }
        public int PixelsPerUnit { get => pixelsPerUnit; set { if (value != PixelsPerUnit) { pixelsPerUnit = value; RebuildMeshQuad(); }; } }

        private const string transitionTexName = "_TransitionTex";
        private const string customDissolveTexName = "_CustomDissolveTex";
        private const string transitionProgressName = "_TransitionProgress";
        private const string transitionParamsName = "_TransitionParams";
        private const string randomSeedName = "_RandomSeed";
        private const string tintColorName = "_TintColor";
        private const string flipName = "_Flip";
        private const string shaderName = "Naninovel/TransitionalSprite";

        private static Texture2D sharedCloudsTexture;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Material material;
        private Tweener<FloatTween> transitionTweener;
        private Tweener<ColorTween> colorTweener;
        private Tweener<FloatTween> fadeTweener;
        private List<Vector3> vertices = new Vector3[4].ToList();
        private List<Vector2> mainUVs = new Vector2[4].ToList();
        private List<Vector2> transitionUVs = new Vector2[4].ToList();
        private List<int> triangles = new List<int> { 0, 1, 2, 3, 2, 1 };
        private Vector2 pivot = Vector2.one * .5f;
        private int pixelsPerUnit = 100;

        private void Awake ()
        {
            InitializeSharedResources();
            InitializeTweeners();
            InitializeMeshFilter();
            InitializeMeshRenderer();
        }

        private void OnEnable ()
        {
            meshRenderer.enabled = true;
            UpdateRandomSeed();
        }

        private void OnDisable ()
        {
            meshRenderer.enabled = false;
        }

        public async Task TransitionToAsync (Texture texture, float duration, EasingType easingType = default, 
            TransitionType? transitionType = null, Vector4? transitionParams = null, Texture customDissolveTexture = null)
        {
            if (transitionType.HasValue)
            {
                TransitionType = transitionType.Value;
                TransitionParams = transitionParams ?? transitionType.Value.GetDefaultParams();

                if (ObjectUtils.IsValid(customDissolveTexture))
                    CustomDissolveTexture = customDissolveTexture;

                UpdateRandomSeed();
            }

            if (!MainTexture)
            {
                MainTexture = texture;
                return;
            }
            else
            {
                TransitionTexture = texture;
                var tween = new FloatTween(TransitionProgress, 1, duration, value => TransitionProgress = value, false, easingType);
                if (transitionTweener.IsRunning) transitionTweener.CompleteInstantly();
                await transitionTweener.RunAsync(tween);
                MainTexture = TransitionTexture;
                TransitionProgress = 0;
            }
        }

        public async Task TintToAsync (Color color, float duration, EasingType easingType = default)
        {
            if (colorTweener.IsRunning) colorTweener.CompleteInstantly();
            var tween = new ColorTween(TintColor, color, ColorTweenMode.All, duration, value => TintColor = value, false, easingType);
            await colorTweener.RunAsync(tween);
        }

        public async Task FadeToAsync (float opacity, float duration, EasingType easingType = default)
        {
            if (fadeTweener.IsRunning) fadeTweener.CompleteInstantly();
            var tween = new FloatTween(Opacity, opacity, duration, value => Opacity = value, false, easingType);
            await fadeTweener.RunAsync(tween);
        }

        public async Task FadeOutAsync (float duration) => await FadeToAsync(0, duration);

        public async Task FadeInAsync (float duration) => await FadeToAsync(1, duration);

        private static void InitializeSharedResources ()
        {
            if (!sharedCloudsTexture)
                sharedCloudsTexture = Resources.Load<Texture2D>("Naninovel/Textures/Clouds"); 
        }

        private void InitializeTweeners ()
        {
            transitionTweener = new Tweener<FloatTween>(this);
            colorTweener = new Tweener<ColorTween>(this);
            fadeTweener = new Tweener<FloatTween>(this);
        }

        private void InitializeMeshFilter ()
        {
            if (!meshFilter)
            {
                meshFilter = GetComponent<MeshFilter>();
                if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.hideFlags = HideFlags.HideInInspector;
            }

            meshFilter.mesh = new Mesh();
            meshFilter.mesh.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            meshFilter.mesh.name = "Generated Quad Mesh (Instance)";
        }

        private void InitializeMeshRenderer ()
        {
            if (!meshRenderer)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                if (!meshRenderer) meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.hideFlags = HideFlags.HideInInspector;
            }

            var shader = Shader.Find(shaderName);
            if (!shader) Debug.LogError($"'{shaderName}' shader not found. Make sure the shader is included to the build.");
            meshRenderer.material = new Material(shader);
            meshRenderer.material.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            meshRenderer.material.SetTexture("_CloudsTex", sharedCloudsTexture);
            material = meshRenderer.material;
        }

        private void SetOpacity (float value)
        {
            var color = TintColor;
            color.a = value;
            TintColor = color;
        }

        private void SetFlipX (bool value)
        {
            var flip = material.GetVector(flipName);
            flip.x = value ? -1 : 1;
            material.SetVector(flipName, flip);
        }

        private void SetFlipY (bool value)
        {
            var flip = material.GetVector(flipName);
            flip.y = value ? -1 : 1;
            material.SetVector(flipName, flip);
        }

        private void UpdateRandomSeed ()
        {
            var sinTime = Mathf.Sin(Time.time);
            var cosTime = Mathf.Cos(Time.time);
            RandomSeed = new Vector2(Mathf.Abs(sinTime), Mathf.Abs(cosTime));
        }

        private void RebuildMeshQuad ()
        {
            if (!meshFilter || !MainTexture) return;

            meshFilter.mesh.Clear();

            // Find required texture sizes.
            var textureWidth = TransitionTexture && TransitionTexture.width > MainTexture.width ? TransitionTexture.width : MainTexture.width;
            var textureHeight = TransitionTexture && TransitionTexture.height > MainTexture.height ? TransitionTexture.height : MainTexture.height;

            // Setup vertices.
            var quadHalfWidth = textureWidth * .5f / PixelsPerUnit;
            var quadHalfHeight = textureHeight * .5f / PixelsPerUnit;
            vertices[0] = new Vector3(-quadHalfWidth, -quadHalfHeight, 0);
            vertices[1] = new Vector3(-quadHalfWidth, quadHalfHeight, 0);
            vertices[2] = new Vector3(quadHalfWidth, -quadHalfHeight, 0);
            vertices[3] = new Vector3(quadHalfWidth, quadHalfHeight, 0);

            // Setup main texture UVs.
            var mainScaleRatioX = textureWidth / (float)MainTexture.width - 1;
            var mainScaleRatioY = textureHeight / (float)MainTexture.height - 1;
            var mainMaxX = 1 + mainScaleRatioX * (1 - Pivot.x);
            var mainMaxY = 1 + mainScaleRatioY * (1 - Pivot.y);
            var mainMinX = 0 - mainScaleRatioX * Pivot.x;
            var mainMinY = 0 - mainScaleRatioY * Pivot.y;
            mainUVs[0] = new Vector2(mainMinX, mainMinY);
            mainUVs[1] = new Vector2(mainMinX, mainMaxY);
            mainUVs[2] = new Vector2(mainMaxX, mainMinY);
            mainUVs[3] = new Vector2(mainMaxX, mainMaxY);

            if (TransitionTexture)
            {
                // Setup transition texture UVs.
                var transitionScaleRatioX = textureWidth / (float)TransitionTexture.width - 1;
                var transitionScaleRatioY = textureHeight / (float)TransitionTexture.height - 1;
                var transitionMaxX = 1 + transitionScaleRatioX * (1 - Pivot.x);
                var transitionMaxY = 1 + transitionScaleRatioY * (1 - Pivot.y);
                var transitionMinX = 0 - transitionScaleRatioX * Pivot.x;
                var transitionMinY = 0 - transitionScaleRatioY * Pivot.y;
                transitionUVs[0] = new Vector2(transitionMinX, transitionMinY);
                transitionUVs[1] = new Vector2(transitionMinX, transitionMaxY);
                transitionUVs[2] = new Vector2(transitionMaxX, transitionMinY);
                transitionUVs[3] = new Vector2(transitionMaxX, transitionMaxY);
            }

            // Apply pivot.
            UpdatePivot();

            // Create quad.
            meshFilter.mesh.SetVertices(vertices);
            meshFilter.mesh.SetUVs(0, mainUVs);
            meshFilter.mesh.SetUVs(1, transitionUVs);
            meshFilter.mesh.SetTriangles(triangles, 0);
        }

        /// <summary>
        /// Corrects geometry data to to match current pivot value.
        /// </summary>
        private void UpdatePivot ()
        {
            var spriteRect = EvaluateSpriteRect();

            var curPivot = new Vector2(-spriteRect.min.x / spriteRect.size.x, -spriteRect.min.y / spriteRect.size.y);
            if (curPivot == Pivot) return;

            var curDeltaX = spriteRect.size.x * curPivot.x;
            var curDeltaY = spriteRect.size.y * curPivot.y;
            var newDeltaX = spriteRect.size.x * Pivot.x;
            var newDeltaY = spriteRect.size.y * Pivot.y;

            var deltaPos = new Vector3(newDeltaX - curDeltaX, newDeltaY - curDeltaY);

            for (int i = 0; i < vertices.Count; i++)
                vertices[i] -= deltaPos;
        }

        /// <summary>
        /// Calculates sprite rectangle using vertex data.
        /// </summary>
        private Rect EvaluateSpriteRect ()
        {
            var minVertPos = new Vector2(vertices.Min(v => v.x), vertices.Min(v => v.y));
            var maxVertPos = new Vector2(vertices.Max(v => v.x), vertices.Max(v => v.y));
            var spriteSizeX = Mathf.Abs(maxVertPos.x - minVertPos.x);
            var spriteSizeY = Mathf.Abs(maxVertPos.y - minVertPos.y);
            var spriteSize = new Vector2(spriteSizeX, spriteSizeY);
            return new Rect(minVertPos, spriteSize);
        }
    } 
}
