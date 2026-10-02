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
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 1000;

            var rect = canvasObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(90f, 35f);
            rect.localScale = Vector3.one * 0.01f;
            rect.position = new Vector3(-1.15f, 1.75f, 2.2f);
            rect.rotation = Quaternion.Euler(0f, 18f, 0f);

            var textObject = new GameObject("NEXUS Request Text");
            textObject.transform.SetParent(canvasObject.transform, false);
            _text = textObject.AddComponent<TextMeshProUGUI>();
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            _text.alignment = TextAlignmentOptions.Center;
            _text.fontSize = 8f;
            _text.textWrappingMode = TextWrappingModes.Normal;
            _text.text = _message;

            _root = canvasObject;
            Plugin.Log?.Info("NEXUS native request display created.");
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
