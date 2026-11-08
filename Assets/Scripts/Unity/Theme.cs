using UnityEngine;

namespace Millionalite.Unity
{
    /// <summary>薄暗い酒場の色調。HTML版と同じ値を使う。</summary>
    public static class Theme
    {
        public static readonly Color Bg       = Hex(0x0A0805);  // 酒場の闇
        public static readonly Color BgRaise  = Hex(0x141009);  // 卓上
        public static readonly Color BgPanel  = Hex(0x1B1510);
        public static readonly Color Gold     = Hex(0xC9A961);  // 燭台の金
        public static readonly Color GoldDim  = Hex(0x8A7340);
        public static readonly Color GoldHi   = Hex(0xF0D89A);
        public static readonly Color Blood    = Hex(0x8B2E2E);  // 血赤
        public static readonly Color BloodHi  = Hex(0xC2494A);
        public static readonly Color Ink      = Hex(0xE8DCC4);  // 羊皮紙の文字
        public static readonly Color InkDim   = Hex(0x96876C);
        public static readonly Color Line     = Hex(0x3A2F22);

        public static readonly Color CardFace     = Hex(0xEDE2C8);
        public static readonly Color CardFaceGold = Hex(0xE2C87E);
        public static readonly Color CardInk      = Hex(0x1B1510);
        public static readonly Color CardInkRed   = Hex(0x9C2020);
        public static readonly Color CardDisabled = Hex(0x6B6253);

        public static Color Hex(int rgb)
        {
            return new Color32(
                (byte)((rgb >> 16) & 0xFF),
                (byte)((rgb >> 8) & 0xFF),
                (byte)(rgb & 0xFF),
                0xFF);
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
