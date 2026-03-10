# EffectTool

パーティクルのエフェクトを作成し、スプライトシートで出力するツール

https://github.com/user-attachments/assets/64d4329e-9876-4dd1-8f61-f1ccf2dfcb33

- **開発期間** : 1ヶ月
- **制作人数** : 1人
- **リリース先** : GitHub Releases

### 動作環境

- **OS** : Windows 11
- **GPU** : DirectX 11 対応

### 参考書籍

- 『独習C++ 新版』 高橋 航平
- 『Effective C++』 Scott Meyers

<br>
<br>

## 背景

チーム制作をしていると、デザイナーやプランナーはUnityに不慣れなことが多い。

実際にUnityでエフェクトを作るには、以下の手順が必要がある。

1. リポジトリをCloneする
2. プロジェクトを開く
3. ParticleSystemで制作してPrefabにする

加えてUnityのような汎用エンジンはあらゆる用途に対応するため機能が多く、  
使い慣れるまでに時間がかかる。

そこで、**シンプルな操作でエフェクトを作成**し、 **スプライトシート形式で出力**することで  
幅広い環境で活用できるツールを作成した。  

<br>
<br>

## 使用技術

- **言語** : C++
- **IDE** : Visual Studio
- **グラフィックスAPI** : DirectX
- **UI** : ImGui
- **データ** : nlohmann/json

<br>

### 選定理由

#### ImGui

| ライブラリ | 特徴 |
|------------|------|
| **ImGui** | DirectXとの統合が容易で軽量 |
| Qt | 機能豊富だが大規模。ライセンス制約あり |
| wxWidgets | クロスプラットフォーム向けで本ツールには過剰 |

ゲーム・ツール開発向けに特化しており、DirectXへの組み込みが数行で完了する。
QtやwxWidgetsはGUIアプリ向けには強力だが、本ツールのような軽量なデバッグ・編集UIには過剰であるため、ImGuiを選定した。

---

#### nlohmann/json

| ライブラリ | 特徴 |
|------------|------|
| **nlohmann/json** | テキスト形式。人が読め、Gitでの差分管理が容易 |
| MessagePack / BSON | 高速・軽量だが人が読めず、差分が確認しにくい |
| RapidJSON | 高速だが、APIが冗長でコードが読みにくい |

保存データはバイナリ形式と迷ったが、テキスト形式であるJSONはGitで差分が見やすくバージョン管理がしやすい点を重視して選定した。
nlohmann/jsonはヘッダーだけで導入が簡単なうえ、直感的なAPIでコードの可読性も高い。

<br>
<br>

## 技術的な工夫

### オフスクリーンレンダリングによるポストエフェクトとスプライトシート出力

オフスクリーンにレンダリングし、ブラーのシェーダーを適用することでBloomを実装している。

https://github.com/Matsumoto0628/BasicTool/blob/c1f91ebc48b1369e9637a90cabb5038f12fd2d51/basic_tool/scripts/application/render_context.cpp#L187-L190

1フレームずつオフスクリーンにレンダリングし、全フレームを1枚のPNGとして並べて出力する。

https://github.com/Matsumoto0628/BasicTool/blob/8ab9197dc6c06e20f99159328d0ff0701fbc7823/basic_tool/scripts/application/render_exporter.cpp#L103-L112

---

### コンポーネント指向 : テンプレートによるジェネリック実装

`AddComponent` にテンプレートを用いることで、型ごとにコードを書かずにあらゆるコンポーネントを追加・取得できる。

https://github.com/Matsumoto0628/BasicTool/blob/599d4ab5456ad18a16c00b71adc7803f27e8de68/basic_tool/scripts/application/game_object.h#L31-L43

---

### JSON によるセーブ & ロード

各コンポーネントが `Serialize` / `Deserialize` を実装することで、`Scene` 側は型を意識せず一括で保存・復元できる。

https://github.com/Matsumoto0628/BasicTool/blob/1a94aa5e8957e64962500e3f4573cb6dbb0afc6d/basic_tool/scripts/component/transform.cpp#L131-L153

https://github.com/Matsumoto0628/BasicTool/blob/01cca8b93f857d2883123619873389c2b4a2d6e5/basic_tool/scripts/application/scene.cpp#L66-L72

<br>
<br>

## インストール方法
1. GitHubのReleasesページから `EffectTool.zip` をダウンロード
2. ZIPを展開する
3. `EffectTool.exe` を起動する
> **Note:** 起動時にWindowsのセキュリティ警告が表示される場合があります。「詳細情報」>「実行」をクリックして続行してください。
>

<p>
<img width="635" height="351" alt="スクリーンショット 2026-03-10 170632" src="https://github.com/user-attachments/assets/aeeba298-0234-4fc8-9850-3e16273bd5e7" />
</p>

<br>
<br>

## 操作方法

### エフェクトの編集

1. 左側の **Hierarchy** パネルで `ParticleController` をクリック

<p>
<img width="597" height="353" alt="スクリーンショット 2026-03-10 174733" src="https://github.com/user-attachments/assets/c72c01fe-4c84-4d78-9dfc-ace483322748" />
</p>

2. 右側の **Inspector** パネルに `ParticleController` コンポーネントのプロパティが表示される

<p>
<img width="597" height="354" alt="スクリーンショット 2026-03-10 174744" src="https://github.com/user-attachments/assets/78808e83-6bdf-42e6-ba2c-0599dd364b5e" />
</p>

3. `Gravity` や `Color` などのパラメータを変更するとパーティクルがリアルタイムで変化する

<p>
<img width="597" height="354" alt="スクリーンショット 2026-03-10 174826" src="https://github.com/user-attachments/assets/1d31a757-f0da-4394-91f1-b5f3734cd952" />
</p>

<br>

### 画像書き出し

- **書き出し** : File > Export (出力先は、exeと同じ階層の `export/` フォルダ (固定))

<p>
<img width="210" height="185" alt="スクリーンショット 2026-03-10 173431" src="https://github.com/user-attachments/assets/98defda4-87f8-4ce5-8d08-952d3b7cf1ef" />
<img width="444" height="167" alt="スクリーンショット 2026-03-10 173547" src="https://github.com/user-attachments/assets/3023beb6-85a7-4f29-8874-742580251f0d" />
</p>

<br>

### 保存 & 読み込み

- **保存** : File > Save
<p>
<img width="203" height="185" alt="スクリーンショット 2026-03-10 173424" src="https://github.com/user-attachments/assets/24c538d7-bf73-49c5-9678-ce09b069f4b9" />
<img width="344" height="116" alt="スクリーンショット 2026-03-10 173517" src="https://github.com/user-attachments/assets/48d03fdd-1656-47c6-bf3b-4972bb5e62d8" />
</p>

<br>

- **読み込み** : File > Open
<p>
<img width="176" height="148" alt="スクリーンショット 2026-03-10 173416" src="https://github.com/user-attachments/assets/5e52d3d9-ac1b-4f54-a9ff-56556c99b61d" />
<img width="364" height="161" alt="スクリーンショット 2026-03-10 173501" src="https://github.com/user-attachments/assets/ed2ba087-5aa9-43dd-a8da-817dabb25450" />
</p>
