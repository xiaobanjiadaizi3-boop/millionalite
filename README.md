# millionalite

「大富豪 × ローグライト」のカードゲーム。Unity 6（uGUI）で開発中。

- 仕様は [`SPEC.md`](SPEC.md)（ルールブックと Phase 0〜7 の開発計画）
- 現在の実装範囲: **Phase 1 — 素の大富豪1ラウンド**

## 開くまで

1. Unity Hub で **Unity 6 LTS（6000.0 系）** をインストール
2. Hub の「Add」からこのリポジトリのフォルダを追加して開く
3. `Assets/Scenes/Main.unity` を開いて Play

シーンには `Bootstrap` という GameObject が1つあるだけで、画面はすべて実行時にコードから組み立てます。
シーンが開けない場合は、空のシーンを作って空の GameObject に `GameBootstrap` を付けるだけで同じように動きます。
メニューの **millionalite → シーンを作り直す** でも再生成できます。

## ディレクトリ

```
Assets/
  Scripts/
    Core/        UnityEngine に依存しない対局ロジック（単体で検証できる）
      Card.cs            カード1枚。強さは 3..14=3..A / 15=2 / 16=JOKER
      DeckFactory.cs     山札の生成とシャッフル。山札は1人1デッキ
      PlayerState.cs     1人分の手札・山札・順位
      DaifugoRound.cs    1ラウンドの進行（出す／パス／場が流れる／上がり）
      SimpleCpu.cs       超簡易AI: 出せる中で最弱、出せなければパス
    Unity/       uGUI の画面。Core を呼ぶだけで、ルールは持たない
      GameBootstrap.cs   起点。画面の組み立てと手番の制御
      UiFactory.cs       uGUI をコードから作る小道具
      CardView.cs        カードの見た目
      Theme.cs           配色（背景 #0a0805 / 金 #c9a961 / 血赤 #8b2e2e）
      JpFont.cs          日本語フォントの用意
  Editor/        シーン再生成などのエディタメニュー
  Scenes/Main.unity
tools/
  CoreSim/       Unity なしで対局ロジックを大量に回す検証プログラム
  UnityStubs/    Unity を起動せずスクリプトを型チェックするためのスタブ
```

ルールを `Core/` に閉じ込めてあるので、Unity を起動しなくても検証できます。

## 検証

[.NET SDK 8](https://dotnet.microsoft.com/) があれば、Unity なしで次を実行できます。

```bash
# 対局ロジックを200ラウンド回して不変条件を確認する（違反があれば終了コード1）
dotnet run --project tools/CoreSim 200

# Unity スクリプトの型チェック（スタブに対するコンパイル）
dotnet build tools/UnityStubs
```

`CoreSim` が見るもの: 配札が各自9枚、デッキ総数（自分53 / CPU各52）、カードIDの一意性、
ジョーカーの所在、順位が1〜4位で重複しないこと、上がった人の手札が0枚であること、進行が止まらないこと。

> 型チェックは「自分のコードの整合性」を見るもので、Unity 実機での動作保証ではありません。

## 実装済み（Phase 1）

- 山札は**1人1デッキ**。プレイヤーは52枚 + ジョーカー1枚、CPUは各52枚
- ラウンド開始時に9枚配るのみ（ウェーブごとの補充はしない）
- 出し方はシングルのみ。場が空なら何でも、場に札があればより強い1枚だけ
- 出せない札は暗く表示して押せない
- 一度パスした人は場が流れるまで参加できない
- 全員パスで場が流れ、最後に出した人から再開
- 3人が上がったら残り1人が大貧民。順位を表示

## 未実装

Phase 2 以降（21ラウンド進行、コイン、ショップ、マークの素とパッシブ、ボス戦、
シール・消耗品・コインランドリー、デッキ選択・実績）。詳細は `SPEC.md`。

## WebGL / unityroom に出すとき

まだ試していません。出す前に必要になるのは次の3点です。

1. **日本語フォントの同梱** — WebGL にはOSフォントが無いため、いまのままでは日本語が表示されません。
   ライセンス上問題ない日本語フォント（Noto Sans JP など）を `Assets/Resources/Fonts/Main.ttf` に置くと
   `JpFont` が自動的に使います。エディタと PC ビルドでは OS のフォントを使うので、そのままでも読めます。
2. **ビルド対象シーンの登録** — メニューの **millionalite → このシーンをビルド対象に追加**、
   または Build Settings から `Assets/Scenes/Main.unity` を追加
3. **画面比率の決定** — いまは縦長（1080×1920 基準）で作っています。投稿先の画面サイズに合わせて
   `GameBootstrap.BuildUi` の `referenceResolution` を調整してください。

圧縮形式など投稿先ごとの指定は、各サイトの最新のマニュアルを確認してください。

## 以前の HTML 版について

Phase 0〜1 は単一 `index.html` のブラウザ版として作り、GitHub Pages で公開していました。
Unity 版への移行にあたって削除しています。履歴には残っているので、必要なら次で取り出せます。

```bash
git show 54b2d85:index.html > index.html
```
