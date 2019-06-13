// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class LoadLoggerScrollRect : ScriptableUIComponent<ScrollRect>
    {
        [SerializeField] private Text loggerText = null;
        [SerializeField] private Text memoryUsageText = null;

        private ResourceProviderManager providersManager;

        protected override void Awake ()
        {
            base.Awake();

            this.AssertRequiredObjects(loggerText);

            providersManager = Engine.GetService<ResourceProviderManager>();
            loggerText.text = string.Empty;
        }

        protected override void Start ()
        {
            base.Start();

            if (memoryUsageText)
                memoryUsageText.gameObject.SetActive(providersManager.LogResourceLoading);
            UpdateMemoryUsage();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            providersManager.OnProviderMessage += LogResourceProviderMessage;
            Application.logMessageReceived += LogDebug;
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy();

            providersManager.OnProviderMessage -= LogResourceProviderMessage;
            Application.logMessageReceived -= LogDebug;
        }

        public void Log (string message)
        {
            if (!providersManager.LogResourceLoading) return;

            loggerText.text += message;
            loggerText.text += Environment.NewLine;
            UIComponent.verticalNormalizedPosition = 0;

            if (loggerText.text.Length > 10000) // UI.Text has char limit (depends on vertex count per char, 65k verts is the limit).
                loggerText.text = loggerText.text.GetAfterFirst(Environment.NewLine);
        }

        private void LogResourceProviderMessage (string message)
        {
            Log($"<color=lightblue>{message}</color>");
            UpdateMemoryUsage();
        }

        private void LogDebug (string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Assert || type == LogType.Error || type == LogType.Exception)
                Log($"<color=red>[System] {condition}</color>");
            else if (type == LogType.Warning)
                Log($"<color=yellow>[System] {condition}</color>");
            else Log($"[System] {condition}");
        }

        private void UpdateMemoryUsage ()
        {
            if (!memoryUsageText || !providersManager.LogResourceLoading) return;
            memoryUsageText.text = string.Concat("<b>Total memory used: ", Mathf.CeilToInt(GC.GetTotalMemory(false) * .000001f), "Mb</b>");
        }
    }
}
