using System;
using UnityEngine;

namespace Millionalite.Unity
{
    /// <summary>
    /// 日本語が出るフォントを用意する。
    ///
    /// 1. Assets/Resources/Fonts/Main.ttf があればそれを使う（WebGL ビルドではこれが必要）
    /// 2. 無ければ OS の日本語フォントを動的に使う（エディタ / PC ビルド用）
    /// 3. それも無ければ Unity 同梱フォント（日本語は豆腐になる）
    ///
    /// WebGL にはOSフォントが無いため、unityroom に出す前に 1 を用意すること。
    /// </summary>
    public static class JpFont
    {
        private static Font _cached;

        private static readonly string[] Candidates =
        {
            "Noto Sans CJK JP", "Noto Sans JP", "Hiragino Sans", "Hiragino Kaku Gothic ProN",
            "Yu Gothic", "YuGothic", "Meiryo", "MS Gothic", "IPAGothic", "TakaoGothic"
        };

        public static Font Get()
        {
            if (_cached != null) return _cached;

            _cached = Resources.Load<Font>("Fonts/Main");
            if (_cached != null) return _cached;

            try
            {
                var installed = Font.GetOSInstalledFontNames();
                if (installed != null)
                {
                    foreach (var wanted in Candidates)
                    {
                        if (Array.IndexOf(installed, wanted) < 0) continue;
                        _cached = Font.CreateDynamicFontFromOSFont(wanted, 32);
                        if (_cached != null) return _cached;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("OSフォントを取得できませんでした: " + e.Message);
            }

            try
            {
                _cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch (Exception)
            {
                _cached = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            if (_cached == null)
            {
                Debug.LogWarning("日本語フォントが見つかりません。Assets/Resources/Fonts/Main.ttf を置いてください。");
            }

            return _cached;
        }
    }
}
