// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Manages cameras and other systems required for scene rendering.
    /// </summary>
    [InitializeAtRuntime(-1)] // Add camera at the start, so the user could see something while waiting for the engine init.
    public class CameraManager : IStatefulService<GameStateMap>
    {
        [Serializable]
        private class GameState
        {
            [Serializable]
            public struct CameraComponent
            {
                public string TypeName;
                public bool Enabled;

                public CameraComponent (MonoBehaviour comp)
                {
                    TypeName = comp.GetType().Name;
                    Enabled = comp.enabled;
                }
            }

            public float OrthoSize = -1f;
            public Vector2 Offset = Vector2.zero;
            public float Rotation = 0f;
            public float Zoom = 0f;
            public CameraComponent[] CameraComponents;
        }

        public event Action<float> OnAspectChanged;

        public Camera Camera { get; private set; }
        public Camera UICamera { get; private set; }
        public float ScreenAspect => (float)Screen.width / Screen.height;
        /// <summary>
        /// Whether the UI is being rendered by <see cref="UICamera"/>.
        /// </summary>
        public bool UsingUICamera => config.UseUICamera;
        public bool RenderUI
        {
            get => UsingUICamera ? UICamera.enabled : MaskUtils.GetLayer(Camera.cullingMask, uiLayer);
            set { if (UsingUICamera) UICamera.enabled = value; else Camera.cullingMask = MaskUtils.SetLayer(Camera.cullingMask, uiLayer, value); }
        }
        public FullScreenMode ScreenMode => Screen.fullScreenMode;
        public Vector2Int Resolution => new Vector2Int(Screen.width, Screen.height);
        public int RefreshRate => Screen.currentResolution.refreshRate;
        public int ResolutionIndex => GetCurrentResolutionIndex();
        public Vector2 ReferenceResolution => config.ReferenceResolution;
        public float ReferenceAspect => ReferenceResolution.x / ReferenceResolution.y;
        public float MaxOrthoSize => ReferenceResolution.x / ReferenceAspect / 200f;
        public float PixelsPerUnit => ReferenceResolution.y / (MaxOrthoSize * 2f);
        public Vector2 ReferenceSize => ReferenceResolution / PixelsPerUnit;
        public EasingType DefaultEasintType => config.DefaultEasing;
        /// <summary>
        /// Local camera position offset in units by X and Y axis.
        /// </summary>
        public Vector2 Offset
        {
            get => offset;
            set { CompleteOffsetTween(); offset = value; ApplyOffset(value); }
        }
        /// <summary>
        /// Local camera rotation by Z-axis in angle degrees.
        /// </summary>
        public float Rotation
        {
            get => rotation;
            set { CompleteRotationTween(); rotation = value; ApplyRotation(value); }
        }
        /// <summary>
        /// Relatize camera zoom (orthographic size scale), in 0.0 to 1.0 range.
        /// </summary>
        public float Zoom
        {
            get => zoom;
            set { CompleteZoomTween(); zoom = value; ApplyZoom(value); }
        }

        /// <summary>
        /// Current camera orthographic size. Use this property to accommodate <see cref="Zoom"/> when setting and filter it-out when getting the value.
        /// </summary>
        protected float OrthoSize { get => orthoSize; set { ApplyOrthoSizeZoomAware(value, Zoom); orthoSize = value; } }

        private readonly CameraConfiguration config;
        private ProxyBehaviour proxyBehaviour;
        private IEngineBehaviour engineBehaviour;
        private RenderTexture thumbnailRenderTexture;
        private float lastAspect;
        private float orthoSize;
        private Vector2 offset = Vector2.zero;
        private float rotation = 0f, zoom = 0f;
        private Tweener<VectorTween> offsetTweener;
        private Tweener<FloatTween> rotationTweener, zoomTweener;
        private int uiLayer;

        public CameraManager (CameraConfiguration config, IEngineBehaviour engineBehaviour)
        {
            this.config = config;
            this.engineBehaviour = engineBehaviour;

            thumbnailRenderTexture = new RenderTexture(config.ThumbnailResolution.x, config.ThumbnailResolution.y, 24);
        }

        public Task InitializeServiceAsync ()
        {
            // Do it here and not in ctor to allow camera initialize first.
            // Otherwise, when starting the game, for a moment, no cameras will be available for render.
            uiLayer = Engine.GetService<UIManager>().ObjectLayer;

            if (ObjectUtils.IsValid(config.CustomCameraPrefab))
                Camera = Engine.Instantiate(config.CustomCameraPrefab);
            else
            {
                Camera = Engine.CreateObject<Camera>(nameof(CameraManager));
                Camera.depth = 0;
                Camera.orthographic = true;
                Camera.backgroundColor = new Color32(35, 31, 32, 255);
                if (!UsingUICamera)
                    Camera.allowHDR = false; // Otherwise text artifacts appear when printing.
                if (Engine.OverrideObjectsLayer) // When culling is enabled, render only the engine object and UI (when not using UI camera) layers.
                    Camera.cullingMask = UsingUICamera ? (1 << Engine.ObjectsLayer) : ((1 << Engine.ObjectsLayer) | (1 << uiLayer));
                else if (UsingUICamera) Camera.cullingMask = ~(1 << uiLayer);
            }
            Camera.transform.position = config.InitialPosition;

            if (UsingUICamera)
            {
                if (ObjectUtils.IsValid(config.CustomUICameraPrefab))
                    UICamera = Engine.Instantiate(config.CustomUICameraPrefab);
                else
                {
                    UICamera = Engine.CreateObject<Camera>("UICamera");
                    UICamera.depth = 1;
                    UICamera.orthographic = true;
                    UICamera.allowHDR = false; // Otherwise text artifacts appear when printing.
                    UICamera.cullingMask = 1 << uiLayer;
                    UICamera.clearFlags = CameraClearFlags.Depth;
                }
                UICamera.transform.position = config.InitialPosition;
            }

            proxyBehaviour = Camera.gameObject.AddComponent<ProxyBehaviour>();
            offsetTweener = new Tweener<VectorTween>(proxyBehaviour);
            rotationTweener = new Tweener<FloatTween>(proxyBehaviour);
            zoomTweener = new Tweener<FloatTween>(proxyBehaviour);

            lastAspect = ScreenAspect;
            CorrectOrthoSize(lastAspect);

            engineBehaviour.OnBehaviourLateUpdate += MonitorAspect;
            return Task.CompletedTask;
        }

        public void ResetService () { }

        public void DestroyService ()
        {
            engineBehaviour.OnBehaviourLateUpdate -= MonitorAspect;

            if (thumbnailRenderTexture)
                UnityEngine.Object.Destroy(thumbnailRenderTexture);
            if (Camera && Camera.gameObject)
                UnityEngine.Object.Destroy(Camera.gameObject);
            if (UICamera && UICamera.gameObject)
                UnityEngine.Object.Destroy(UICamera.gameObject);
        }

        public Task SaveServiceStateAsync (GameStateMap stateMap)
        {
            var gameState = new GameState() {
                OrthoSize = OrthoSize,
                Offset = Offset,
                Rotation = Rotation,
                Zoom = Zoom,
                CameraComponents = Camera.gameObject.GetComponents<MonoBehaviour>().Select(c => new GameState.CameraComponent(c)).ToArray()
            };
            stateMap.SerializeObject(gameState);
            return Task.CompletedTask;
        }

        public Task LoadServiceStateAsync (GameStateMap stateMap)
        {
            var state = stateMap.DeserializeObject<GameState>() ?? new GameState();
            if (state.OrthoSize > 0) OrthoSize = state.OrthoSize;
            Offset = state.Offset;
            Rotation = state.Rotation;
            Zoom = state.Zoom;

            if (state.CameraComponents != null)
                foreach (var compState in state.CameraComponents)
                {
                    var comp = Camera.gameObject.GetComponent(compState.TypeName) as MonoBehaviour;
                    if (!comp) continue;
                    comp.enabled = compState.Enabled;
                }

            return Task.CompletedTask;
        }

        public void SetResolution (Vector2Int resolution, FullScreenMode screenMode, int refreshRate)
        {
            Screen.SetResolution(resolution.x, resolution.y, screenMode, refreshRate);
        }

        public Texture2D CaptureThumbnail ()
        {
            if (config.HideUIInThumbnails)
                RenderUI = false;

            // Hide the save-load menu in case it's visible.
            var saveLoadUI = Engine.GetService<UIManager>()?.GetUI<UI.ISaveLoadUI>();
            var saveLoadUIWasVisible = saveLoadUI?.IsVisible;
            if (saveLoadUIWasVisible.HasValue && saveLoadUIWasVisible.Value)
                saveLoadUI.IsVisible = false;

            var initialRenderTexture = Camera.targetTexture;
            Camera.targetTexture = thumbnailRenderTexture;
            Camera.Render();
            Camera.targetTexture = initialRenderTexture;

            if (RenderUI && UsingUICamera)
            {
                initialRenderTexture = UICamera.targetTexture;
                UICamera.targetTexture = thumbnailRenderTexture;
                UICamera.Render();
                UICamera.targetTexture = initialRenderTexture;
            }

            var thumbnail = thumbnailRenderTexture.ToTexture2D();

            // Restore the save-load menu in case we hid it.
            if (saveLoadUIWasVisible.HasValue && saveLoadUIWasVisible.Value)
                saveLoadUI.IsVisible = true;

            if (config.HideUIInThumbnails)
                RenderUI = true;

            return thumbnail;
        }

        public async Task ChangeOffsetAsync (Vector2 offset, float duration, EasingType easingType = default)
        {
            CompleteOffsetTween();
            var currentOffset = this.offset;
            this.offset = offset;

            var tween = new VectorTween(currentOffset, offset, duration, ApplyOffset, false, easingType);
            await offsetTweener.RunAsync(tween);
        }

        public async Task ChangeRotationAsync (float rotation, float duration, EasingType easingType = default)
        {
            CompleteRotationTween();
            var currentRotation = this.rotation;
            this.rotation = rotation;

            var tween = new FloatTween(currentRotation, rotation, duration, ApplyRotation, false, easingType);
            await rotationTweener.RunAsync(tween);
        }

        public async Task ChangeZoomAsync (float zoom, float duration, EasingType easingType = default)
        {
            CompleteZoomTween();
            var currentZoom = this.zoom;
            this.zoom = zoom;

            var tween = new FloatTween(currentZoom, zoom, duration, ApplyZoom, false, easingType);
            await zoomTweener.RunAsync(tween);
        }

        private void MonitorAspect ()
        {
            if (lastAspect != ScreenAspect)
            {
                OnAspectChanged?.Invoke(ScreenAspect);
                lastAspect = ScreenAspect;
                CorrectOrthoSize(lastAspect);
            }
        }

        /// <summary>
        /// Changes current <see cref="OrthoSize"/> to accommodate the provided aspect ratio. 
        /// </summary>
        private void CorrectOrthoSize (float aspect)
        {
            OrthoSize = Mathf.Clamp(ReferenceResolution.x / aspect / 200f, 0f, MaxOrthoSize);
        }

        /// <summary>
        /// Sets the provided ortho size to the camera respecting the provided zoom level.
        /// </summary>
        private void ApplyOrthoSizeZoomAware (float size, float zoom)
        {
            Camera.orthographicSize = size * (1f - Mathf.Clamp(zoom, 0, .99f));
        }

        /// <summary>
        /// Finds index of the closest to the real (current) available (native to display) resolution.
        /// </summary>
        private int GetCurrentResolutionIndex ()
        {
            var currentResolution = new Resolution() { width = Resolution.x, height = Resolution.y, refreshRate = RefreshRate };
            var closestResolution = Screen.resolutions.Aggregate((x, y) => ResolutionDiff(x, currentResolution) < ResolutionDiff(y, currentResolution) ? x : y);
            return Array.IndexOf(Screen.resolutions, closestResolution);
        }

        private int ResolutionDiff (Resolution a, Resolution b)
        {
            return Mathf.Abs(a.width - b.width) + Mathf.Abs(a.height - b.height) + Mathf.Abs(a.refreshRate - b.refreshRate);
        }

        private void ApplyOffset (Vector3 offset) => ApplyOffset((Vector2)offset);

        private void ApplyOffset (Vector2 offset)
        {
            Camera.transform.position = config.InitialPosition + (Vector3)offset;
        }

        private void ApplyRotation (float rotation)
        {
            Camera.transform.eulerAngles = new Vector3(0, 0, rotation);
        }

        private void ApplyZoom (float zoom)
        {
            ApplyOrthoSizeZoomAware(OrthoSize, zoom);
        }

        private void CompleteOffsetTween ()
        {
            if (offsetTweener.IsRunning)
                offsetTweener.CompleteInstantly();
        }

        private void CompleteRotationTween ()
        {
            if (rotationTweener.IsRunning)
                rotationTweener.CompleteInstantly();
        }

        private void CompleteZoomTween ()
        {
            if (zoomTweener.IsRunning)
                zoomTweener.CompleteInstantly();
        }
    } 
}
