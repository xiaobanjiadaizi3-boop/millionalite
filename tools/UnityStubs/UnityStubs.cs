// Unity を起動せずに Assets/Scripts/Unity の型チェックを行うためのスタブ。
// Unity 本体のAPIを「私が使っている範囲だけ」最小限に再現したもので、
// Unity プロジェクトからは参照されない（Assets の外に置いてある）。
// これで通るのは「自分のコードの整合性」であって、Unity 実機の保証ではない。
#pragma warning disable CS0649, CS0067, IDE0060

using System;

namespace UnityEngine
{
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => new Vector2(0, 0);
        public static Vector2 one => new Vector2(1, 1);
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator *(Vector2 a, float b) => new Vector2(a.x * b, a.y * b);
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a = 1f) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white => new Color(1, 1, 1);
        public static Color black => new Color(0, 0, 0);
        public static Color Lerp(Color a, Color b, float t) => a;
    }

    public struct Color32
    {
        public byte r, g, b, a;
        public Color32(byte r, byte g, byte b, byte a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static implicit operator Color(Color32 c) => new Color(c.r / 255f, c.g / 255f, c.b / 255f, c.a / 255f);
    }

    public class Object
    {
        public string name;
        public static void Destroy(Object obj) { }
        public static T FindFirstObjectByType<T>() where T : Object => null;
        public static implicit operator bool(Object obj) => !ReferenceEquals(obj, null);
    }

    public class Component : Object
    {
        public Transform transform => null;
        public GameObject gameObject => null;
        public T GetComponent<T>() where T : Component => null;
    }

    public class Behaviour : Component { public bool enabled; }

    public class Transform : Component
    {
        public int childCount => 0;
        public Transform parent => null;
        public Transform GetChild(int index) => null;
        public void SetParent(Transform parent, bool worldPositionStays) { }
    }

    public class RectTransform : Transform
    {
        public Vector2 anchorMin, anchorMax, offsetMin, offsetMax, sizeDelta, anchoredPosition, pivot;
    }

    public class GameObject : Object
    {
        public string tag;
        public GameObject(string name, params Type[] components) { this.name = name; }
        public GameObject(string name) { this.name = name; }
        public Transform transform => null;
        public T AddComponent<T>() where T : Component => null;
        public T GetComponent<T>() where T : Component => null;
        public void SetActive(bool value) { }
    }

    public class Coroutine { }
    public class YieldInstruction { }
    public class WaitForSeconds : YieldInstruction { public WaitForSeconds(float seconds) { } }

    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(System.Collections.IEnumerator routine) => null;
        public void StopCoroutine(Coroutine routine) { }
    }

    public enum CameraClearFlags { Skybox, SolidColor, Depth, Nothing }

    public class Camera : Behaviour
    {
        public static Camera main => null;
        public CameraClearFlags clearFlags;
        public Color backgroundColor;
        public bool orthographic;
    }

    public class Font : Object
    {
        public static string[] GetOSInstalledFontNames() => new string[0];
        public static Font CreateDynamicFontFromOSFont(string fontname, int size) => null;
    }

    public static class Resources
    {
        public static T Load<T>(string path) where T : Object => null;
        public static T GetBuiltinResource<T>(string path) where T : Object => null;
    }

    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
        public static void LogError(object message) { }
    }

    public class RectOffset
    {
        public RectOffset() { }
        public RectOffset(int left, int right, int top, int bottom) { }
    }

    public enum TextAnchor
    {
        UpperLeft, UpperCenter, UpperRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        LowerLeft, LowerCenter, LowerRight
    }

    public enum FontStyle { Normal, Bold, Italic, BoldAndItalic }
    public enum HorizontalWrapMode { Wrap, Overflow }
    public enum VerticalWrapMode { Truncate, Overflow }
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }

    public class Canvas : Behaviour { public RenderMode renderMode; }

    namespace Events
    {
        public delegate void UnityAction();

        public class UnityEvent
        {
            public void AddListener(UnityAction call) { }
            public void RemoveAllListeners() { }
        }
    }
}

namespace UnityEngine.UI
{
    using UnityEngine;

    public class Graphic : Behaviour
    {
        public Color color;
        public bool raycastTarget;
    }

    public class Image : Graphic { }

    public class Text : Graphic
    {
        public Font font;
        public string text;
        public int fontSize;
        public TextAnchor alignment;
        public FontStyle fontStyle;
        public HorizontalWrapMode horizontalOverflow;
        public VerticalWrapMode verticalOverflow;
    }

    public struct ColorBlock
    {
        public Color normalColor, highlightedColor, pressedColor, selectedColor, disabledColor;
        public float colorMultiplier, fadeDuration;
    }

    public class Selectable : Behaviour
    {
        public bool interactable;
        public Graphic targetGraphic;
        public ColorBlock colors;
    }

    public class Button : Selectable
    {
        public UnityEngine.Events.UnityEvent onClick => null;
    }

    public class LayoutElement : Behaviour
    {
        public float minWidth, minHeight, preferredWidth, preferredHeight, flexibleWidth, flexibleHeight;
    }

    public class LayoutGroup : Behaviour
    {
        public RectOffset padding;
        public TextAnchor childAlignment;
    }

    public class HorizontalOrVerticalLayoutGroup : LayoutGroup
    {
        public float spacing;
        public bool childControlWidth, childControlHeight, childForceExpandWidth, childForceExpandHeight;
    }

    public class HorizontalLayoutGroup : HorizontalOrVerticalLayoutGroup { }
    public class VerticalLayoutGroup : HorizontalOrVerticalLayoutGroup { }

    public class GridLayoutGroup : LayoutGroup
    {
        public enum Constraint { Flexible, FixedColumnCount, FixedRowCount }
        public Vector2 cellSize, spacing;
        public Constraint constraint;
        public int constraintCount;
    }

    public class CanvasScaler : Behaviour
    {
        public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize }
        public enum ScreenMatchMode { MatchWidthOrHeight, Expand, Shrink }
        public ScaleMode uiScaleMode;
        public Vector2 referenceResolution;
        public ScreenMatchMode screenMatchMode;
        public float matchWidthOrHeight;
    }

    public class GraphicRaycaster : Behaviour { }

    public class Shadow : Behaviour
    {
        public Color effectColor;
        public Vector2 effectDistance;
    }

    public class Outline : Shadow { }
}

namespace UnityEngine.EventSystems
{
    using UnityEngine;

    public class EventSystem : Behaviour { }
    public class StandaloneInputModule : Behaviour { }
}
