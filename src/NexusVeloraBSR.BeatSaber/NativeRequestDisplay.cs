using TMPro;
using UnityEngine;

namespace NexusVeloraBSR.BeatSaber
{
    internal sealed class NativeRequestDisplay
    {
        private GameObject? _root;
        private TextMeshProUGUI? _text;
        private string _message = "NEXUS VELORA BSR\nBridge: waiting...";

        public void EnsureCreated()
        {
            if (_root != null) return;

            var canvasObject = new GameObject("NEXUS Velora BSR Canvas");
            Object.DontDestroyOnLoad(canvasObject);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            var rect = canvasObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -70f);
            rect.sizeDelta = new Vector2(900f, 220f);
            rect.localScale = Vector3.one;

            var textObject = new GameObject("NEXUS Request Text");
            textObject.transform.SetParent(canvasObject.transform, false);
            _text = textObject.AddComponent<TextMeshProUGUI>();
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            _text.alignment = TextAlignmentOptions.Center;
            _text.fontSize = 34f;
            _text.color = Color.white;
            _text.textWrappingMode = TextWrappingModes.Normal;
            _text.text = _message;

            _root = canvasObject;
            Plugin.Log?.Info("NEXUS screen-space request display created.");
        }

        public void SetConnected(string song, string key, string requester, int count)
        {
            _message = count > 0
                ? $"NEXUS VELORA BSR\nNEXT: {song} [{key}]\nRequested by: {requester}\nQueue: {count}"
                : "NEXUS VELORA BSR\nBridge: CONNECTED\nQueue is empty.";
            Apply();
        }

        public void SetDisconnected()
        {
            _message = "NEXUS VELORA BSR\nBridge: OFFLINE";
            Apply();
        }

        private void Apply()
        {
            if (_text != null) _text.text = _message;
        }
    }
}
