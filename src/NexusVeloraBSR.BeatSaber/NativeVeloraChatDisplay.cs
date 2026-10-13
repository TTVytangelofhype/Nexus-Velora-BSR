using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NexusVeloraBSR.BeatSaber
{
    // World-space panel for VR; never uses a desktop screen-space overlay.
    internal sealed class NativeVeloraChatDisplay
    {
        private GameObject? _root;
        private TextMeshPro? _text;
        private bool _visible = true;
        private readonly List<string> _lines = new List<string>();

        public void EnsureCreated()
        {
            if (_root != null) return;
            // Mesh-based world-space TMP text avoids Unity UI Image/Canvas rendering.
            _root = new GameObject("NEXUS Velora VR Chat");
            UnityEngine.Object.DontDestroyOnLoad(_root);
            // Side-mounted near the upper-left menu, not headset-attached.
            _root.transform.position = new Vector3(-2.2f, 2.35f, 2.9f);
            _root.transform.rotation = Quaternion.identity;
            _root.transform.localScale = Vector3.one;

            var textObject = new GameObject("Velora Chat Text");
            textObject.transform.SetParent(_root.transform, false);
            textObject.transform.localPosition = Vector3.zero;
            textObject.transform.localRotation = Quaternion.identity;
            _text = textObject.AddComponent<TextMeshPro>();
            var font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                var availableFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                if (availableFonts.Length > 0) font = availableFonts[0];
            }
            if (font != null) _text.font = font;
            else Plugin.Log?.Warn("NEXUS VR chat: no TMP font asset available.");
            _text.fontSize = 2.3f;
            _text.rectTransform.localScale = Vector3.one * 0.1f;
            _text.rectTransform.sizeDelta = new Vector2(16f, 9f);
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
