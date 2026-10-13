using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NexusVeloraBSR.BeatSaber
{
    // Inspired by BeatSaberPlus' floating-chat layout: independent world-space
    // message rows, bounded history, consistent fonts, and side-mounted placement.
    // Uses no Unity UI Canvas/Image or BeatSaberPlus runtime dependencies.
    internal sealed class NativeVeloraChatDisplay
    {
        private GameObject? _root;
        private readonly List<TextMeshPro> _rows = new List<TextMeshPro>();
        private bool _visible = true;
        private readonly List<string> _lines = new List<string>();
        private TMP_FontAsset? _font;
        private const int MaxLines = 6;

        public void EnsureCreated()
        {
            if (_root != null) return;
            var font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                if (fonts.Length > 0) font = fonts[0];
            }
            if (font == null)
            {
                Plugin.Log?.Warn("NEXUS Velora VR chat: no TMP font available; skipping panel.");
                return;
            }
            _font = font;
            _root = new GameObject("NEXUS Velora Chat - Floating");
            UnityEngine.Object.DontDestroyOnLoad(_root);
            PositionAtSide();

            for (int i = 0; i <= MaxLines; i++)
            {
                var go = new GameObject(i == 0 ? "Velora Header" : "Velora Message " + i);
                go.transform.SetParent(_root.transform, false);
                go.transform.localPosition = new Vector3(0f, -i * 0.115f, 0f);
                go.transform.localRotation = Quaternion.identity;
                var tmp = go.AddComponent<TextMeshPro>();
                tmp.font = _font;
                tmp.fontSize = i == 0 ? 2.4f : 2.1f;
                tmp.rectTransform.sizeDelta = new Vector2(17f, 1.05f);
                tmp.rectTransform.localScale = Vector3.one * 0.1f;
                tmp.alignment = TextAlignmentOptions.TopLeft;
                tmp.color = Color.white;
                tmp.richText = true;
#pragma warning disable CS0618
                tmp.enableWordWrapping = false;
#pragma warning restore CS0618
                _rows.Add(tmp);
            }
            Apply();
            _root.SetActive(_visible);
            Plugin.Log?.Info("NEXUS Velora chat floating text initialized without UI canvas.");
        }

        private void PositionAtSide()
        {
            if (_root == null) return;
            // Fixed in the VR world after placement, not attached to the headset.
            // Left of the initial viewpoint to keep the song menu unobstructed.
            var head = Camera.main;
            if (head != null)
            {
                _root.transform.position = head.transform.TransformPoint(new Vector3(-2.05f, 0.45f, 2.7f));
                _root.transform.rotation = Quaternion.Euler(0f, head.transform.eulerAngles.y, 0f);
            }
            else
            {
                _root.transform.position = new Vector3(-2.4f, 2.35f, 2.8f);
                _root.transform.rotation = Quaternion.identity;
            }
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
                    ? "<color=#B6A0FF><b>" + safe.Substring(0, separator) + "</b></color>: " + safe.Substring(separator + 2)
                    : safe);
                if (_lines.Count > MaxLines) _lines.RemoveAt(0);
            }
            Apply();
        }

        private static string Escape(string value) =>
            (value ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        private void Apply()
        {
            if (_rows.Count == 0) return;
            _rows[0].text = "<color=#42D8FF><b>VELORA CHAT</b></color>";
            for (int i = 1; i < _rows.Count; i++)
                _rows[i].text = i <= _lines.Count ? _lines[_lines.Count - i] : string.Empty;
        }

        public void Destroy()
        {
            if (_root != null) UnityEngine.Object.Destroy(_root);
            _root = null;
            _rows.Clear();
        }
    }
}
