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
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;

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
            AttachToMainCamera();
            Plugin.Log?.Info("NEXUS native request display created.");
        }

        public void AttachToMainCamera()
        {
            if (_root == null) return;
            var camera = Camera.main;
            if (camera == null)
            {
                Plugin.Log?.Warn("NEXUS display is waiting for the Beat Saber main camera.");
                return;
            }

            var rect = _root.GetComponent<RectTransform>();
            rect.SetParent(camera.transform, false);
            rect.localPosition = new Vector3(0f, -0.22f, 2.0f);
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one * 0.01f;
            Plugin.Log?.Info("NEXUS display attached to the active Beat Saber camera.");
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
