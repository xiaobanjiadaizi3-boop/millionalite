using UnityEngine;
using UnityEngine.UI;

namespace Millionalite.Unity
{
    /// <summary>uGUI をコードから組み立てるための小道具。シーンはほぼ空のままで済む。</summary>
    public static class UiFactory
    {
        public static GameObject Create(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        public static RectTransform Rect(GameObject go) => (RectTransform)go.transform;

        /// <summary>親いっぱいに広げる。</summary>
        public static RectTransform Stretch(GameObject go)
        {
            var rt = Rect(go);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image Panel(GameObject go, Color color)
        {
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static GameObject Box(string name, Transform parent, Color color)
        {
            var go = Create(name, parent);
            Panel(go, color);
            return go;
        }

        public static Text Label(string name, Transform parent, string text, int size, Color color,
            TextAnchor anchor = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var go = Create(name, parent);
            var label = go.AddComponent<Text>();
            label.font = JpFont.Get();
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = anchor;
            label.fontStyle = style;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        /// <summary>酒場風の枠付きボタン。戻り値の Button に onClick を足して使う。</summary>
        public static Button TextButton(string name, Transform parent, string text, int size,
            float height, bool primary = false)
        {
            var go = Create(name, parent);

            var image = go.AddComponent<Image>();
            image.color = primary ? Theme.Hex(0x221A10) : Theme.BgPanel;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = primary ? Theme.GoldDim : Theme.Line;
            outline.effectDistance = new Vector2(2f, -2f);

            var button = go.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0.38f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.targetGraphic = image;

            var label = Label("Label", go.transform, text, size, primary ? Theme.Gold : Theme.Ink);
            Stretch(label.gameObject);

            var element = go.AddComponent<LayoutElement>();
            element.minHeight = height;
            element.preferredHeight = height;

            return button;
        }

        public static VerticalLayoutGroup VerticalLayout(GameObject go, int spacing, RectOffset padding,
            TextAnchor align = TextAnchor.UpperCenter)
        {
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childAlignment = align;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static HorizontalLayoutGroup HorizontalLayout(GameObject go, int spacing, RectOffset padding,
            TextAnchor align = TextAnchor.MiddleCenter)
        {
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childAlignment = align;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static LayoutElement Size(GameObject go, float minHeight = -1f, float flexibleHeight = -1f,
            float minWidth = -1f, float flexibleWidth = -1f)
        {
            var element = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            if (minHeight >= 0f) { element.minHeight = minHeight; element.preferredHeight = minHeight; }
            if (flexibleHeight >= 0f) element.flexibleHeight = flexibleHeight;
            if (minWidth >= 0f) { element.minWidth = minWidth; element.preferredWidth = minWidth; }
            if (flexibleWidth >= 0f) element.flexibleWidth = flexibleWidth;
            return element;
        }

        /// <summary>
        /// 子をすべて消す。Destroy はフレーム末まで実体が残りレイアウトが乱れるので、
        /// 先に親から切り離してから破棄する。
        /// </summary>
        public static void Clear(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                child.SetParent(null, false);
                Object.Destroy(child.gameObject);
            }
        }
    }
}
