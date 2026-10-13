using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
            _root = new GameObject("NEXUS Velora VR Chat");
            UnityEngine.Object.DontDestroyOnLoad(_root);
            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = _root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(560f, 420f);
            // Place panel to the player's left, in front of the menu environment.
            _root.transform.position = new Vector3(-1.25f, 1.65f, 2.2f);
            _root.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            _root.transform.localScale = Vector3.one * 0.002f;
            var background = _root.AddComponent<Image>();
            background.color = new Color(0.02f, 0.04f, 0.09f, 0.86f);
            var textObject = new GameObject("Chat Lines");
            textObject.transform.SetParent(_root.transform, false);
            _text = textObject.AddComponent<TextMeshProUGUI>();
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20f, 20f);
            textRect.offsetMax = new Vector2(-20f, -20f);
            _text.fontSize = 25f;
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
                _lines.Add(Escape(line));
                if (_lines.Count > 8) _lines.RemoveAt(0);
            }
            Apply();
        }

        private static string Escape(string value) => (value ?? string.Empty).Replace("<", "&lt;").Replace(">", "&gt;");
        private void Apply()
        {
            if (_text != null)
                _text.text = "<color=#42D8FF><b>NEXUS VELORA LIVE CHAT</b></color>\n\n" +
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
