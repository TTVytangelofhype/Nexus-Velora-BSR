using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace NexusVeloraBSR.BeatSaber
{
    internal sealed class NativeRequestDisplay
    {
        private GameObject? _root;
        private TextMeshProUGUI? _text;
        private bool _visible = true;
        private string _message = "NEXUS VELORA BSR\nBridge: waiting...";

        public void SetVisible(bool visible)
        {
            _visible = visible;
            if (_root != null) _root.SetActive(visible);
        }

        public void EnsureCreated()
        {
            if (_root != null) return;

            var canvasObject = new GameObject("NEXUS Velora BSR Canvas");
            Object.DontDestroyOnLoad(canvasObject);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            var rect = canvasObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-35f, -130f);
            rect.sizeDelta = new Vector2(440f, 165f);
            rect.localScale = Vector3.one;

            var background = canvasObject.AddComponent<Image>();
            background.color = new Color(0.025f, 0.04f, 0.085f, 0.88f);

            var textObject = new GameObject("NEXUS Request Text");
            textObject.transform.SetParent(canvasObject.transform, false);
            _text = textObject.AddComponent<TextMeshProUGUI>();
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18f, 12f);
            textRect.offsetMax = new Vector2(-18f, -12f);
            _text.alignment = TextAlignmentOptions.TopLeft;
            _text.fontSize = 22f;
            _text.color = Color.white;
            // Legacy TextMeshPro API shared by Beat Saber 1.40.8 and 1.44.1.
#pragma warning disable CS0618
            _text.enableWordWrapping = true;
#pragma warning restore CS0618
            _text.text = _message;

            _root = canvasObject;
            _root.SetActive(_visible);
            Plugin.Log?.Info("NEXUS screen-space request display created.");
        }

        public void SetConnected(string song, string key, string requester, int count)
        {
            _message = count > 0
                ? $"<color=#3FD9FF><b>NEXUS VELORA BSR</b></color>\nNEXT: {song} [{key}]\nBy: {requester}  |  Queue: {count}"
                : "<color=#3FD9FF><b>NEXUS VELORA BSR</b></color>\nBridge: CONNECTED\nQueue is empty.";
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
