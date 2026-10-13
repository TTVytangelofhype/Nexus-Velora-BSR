using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexusVeloraBSR.BeatSaber
{
    // World-space panel for VR; never uses a desktop screen-space overlay.
    internal sealed class NativeVeloraChatDisplay
    {
        private GameObject? _root;
        private TextMeshProUGUI? _text;
        private bool _visible = true;
        private readonly List<string> _lines = new List<string>();

        public void EnsureCreated()
        {
            if (_root != null) return;
            _root = new GameObject("NEXUS Velora VR Chat", typeof(RectTransform));
            UnityEngine.Object.DontDestroyOnLoad(_root);
            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 500;
            var rect = _root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(700f, 270f);
            // Compact Twitch-style chat strip at the upper-left of the VR menu.
            var head = Camera.main;
            if (head != null)
            {
                _root.transform.position = head.transform.TransformPoint(new Vector3(-1.1f, 0.35f, 2.2f));
                _root.transform.rotation = head.transform.rotation;
            }
            else
            {
                _root.transform.position = new Vector3(-1.65f, 2.35f, 2.65f);
                _root.transform.rotation = Quaternion.identity;
            }
            _root.transform.localScale = Vector3.one * 0.002f;
            var backgroundObject = new GameObject("Dark Chat Background", typeof(RectTransform));
            backgroundObject.transform.SetParent(_root.transform, false);
            var backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            var background = backgroundObject.AddComponent<Image>();
            background.raycastTarget = false;
            background.color = new Color(0.015f, 0.018f, 0.028f, 0.72f);
            var textObject = new GameObject("Chat Lines", typeof(RectTransform));
            textObject.transform.SetParent(_root.transform, false);
            _text = textObject.AddComponent<TextMeshProUGUI>();
            _text.raycastTarget = false;
            var font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                var availableFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                if (availableFonts.Length > 0) font = availableFonts[0];
            }
            if (font != null) _text.font = font;
            else Plugin.Log?.Warn("NEXUS VR chat: no TMP font asset available.");
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18f, 12f);
            textRect.offsetMax = new Vector2(-18f, -12f);
            _text.fontSize = 23f;
            _text.alignment = TextAlignmentOptions.TopLeft;
            _text.color = Color.white;
#pragma warning disable CS0618
            _text.enableWordWrapping = true;
#pragma warning restore CS0618
            Apply();
            _root.SetActive(_visible);
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            if (_root != null) _root.SetActive(visible);
        }

        public void UpdateLines(IEnumerable<string> lines)
        {
            _lines.Clear();
            foreach (var line in lines)
            {
                var safe = Escape(line);
                var separator = safe.IndexOf(": ", StringComparison.Ordinal);
                _lines.Add(separator > 0
                    ? "<color=#A98BFF><b>" + safe.Substring(0, separator) + "</b></color>: " + safe.Substring(separator + 2)
                    : safe);
                if (_lines.Count > 6) _lines.RemoveAt(0);
            }
            Apply();
        }

        private static string Escape(string value) => (value ?? string.Empty).Replace("<", "&lt;").Replace(">", "&gt;");
        private void Apply()
        {
            if (_text != null)
                _text.text = "<color=#42D8FF><b>VELORA CHAT</b></color>\n" +
                    (_lines.Count == 0 ? "Waiting for new chat messages..." : string.Join("\n", _lines));
        }

        public void Destroy()
        {
            if (_root != null) UnityEngine.Object.Destroy(_root);
            _root = null;
            _text = null;
        }
    }
}
