# ShuttleGun

シャトルランをモチーフにした3DTPSゲーム


- **開発期間** : 1ヶ月
- **制作人数** : 1人
- **リリース先** : GitHub Releases

### 動作環境

- **OS** : Windows 11

### 参考書籍

- 『UniRx/UniTask 完全理解』 打田恭平

### ライセンス

本プロジェクトは以下のオープンソースライブラリを使用しています。

- **UniTask** - Copyright (c) 2019 Yoshifumi Kawai / Cysharp, Inc. - [MIT License](https://github.com/Cysharp/UniTask/blob/master/LICENSE)
- **UniRx** - Copyright (c) 2018 Yoshifumi Kawai - [MIT License](https://github.com/neuecc/UniRx/blob/master/LICENSE)

<br>
<br>

## 背景

好きな3Dアクションゲームに登場するスライディングや壁走り・よじ登りといったアクションを、自分ならどう実装するかという好奇心がきっかけとなった。

3Dにおけるベクトル演算とアニメーション制御を中心に据え、動きの気持ちよさにこだわって制作した。

また、単に動くものを作るだけでなく、拡張しやすく不具合を見つけやすい設計を意識することも目標とした。具体的には、UniRxを用いたMVPパターンによるUIの分離、ジェネリックFSMによる敵AIの状態管理、抽象クラスを活用した銃のポリモーフィズムといった設計を取り入れた。

<br>
<br>

## 使用技術

- **言語** : C#
- **IDE** : Visual Studio Code
- **ゲームエンジン** : Unity
- **ライブラリ** : UniRx, UniTask, DOTween

<br>

### 選定理由

#### UniRx

| ライブラリ | 特徴 |
|------------|------|
| **UniRx** | Unity向けに最適化されたReactive Extensions |
| C# 標準イベント | シンプルだが、複数イベントの合成や非同期との連携が難しい |
| UniTask のみ | 非同期処理は得意だが、値の変化の監視には不向き |

UIのデータバインディングにMVPパターンを採用するにあたり、モデルの値変化をViewへ伝える手段が必要だった。
UniRxの `ReactiveProperty` を使うことで、値の変化を購読するだけでUIが自動更新される仕組みを簡潔に実装できるため選定した。

---

#### UniTask

| ライブラリ | 特徴 |
|------------|------|
| **UniTask** | Unity向けに最適化された非同期ライブラリ。ゼロアロケーション |
| coroutine | Unityの標準機能だが、戻り値を扱えずコードが分散しやすい |
| C# async/await | 標準の Task はスレッドベースでUnityのメインスレッドと相性が悪い |

コルーチンでは戻り値の受け渡しや複数の非同期処理の合成が難しく、コードが煩雑になりやすい。
UniTaskはUnityのライフサイクルに統合された async/await を提供し、パフォーマンスへの影響も小さいため選定した。

---

#### DOTween

| ライブラリ | 特徴 |
|------------|------|
| **DOTween** | 軽量で高速。UniTaskとの統合が容易 |
| UnityのAnimation | シンプルだが、スクリプトから動的に制御しにくい |
| iTween | 機能は多いがDOTweenより低速でAPIが冗長 |

よじ登りでは目標座標への移動を2段階に分けて順番に実行する必要があり、アニメーションの完了を待って次の処理に進む制御が必要だった。
DOTweenは `AsyncWaitForCompletion()` でUniTaskの async/await と組み合わせられるため、複数のトゥイーンを直列に記述でき、選定した。

<br>
<br>

## 技術的な工夫

### ベクトル演算による移動制御

斜面上での移動では `Vector3.ProjectOnPlane` で移動方向を法線に対して投影し、斜面に沿った方向を求めている。
https://github.com/Matsumoto0628/PortfolioShuttleGun/blob/927e58f6b3cdc3f577ca0b67337e974b3308d0c2/Scripts/Player/PlayerMovement.cs#L697-L700

壁走りでは `Vector3.Cross` で壁の法線と上方向の外積を取り、壁に沿った前進方向を算出している。
https://github.com/Matsumoto0628/PortfolioShuttleGun/blob/b0a860d302424ac6f357e19573a5251364ee806e/Scripts/Player/PlayerMovement.cs#L645-L649

### アニメーション制御

Animatorのレイヤーを3層（下半身・上半身・腕）に分け、状態に応じてレイヤーウェイトをスクリプトから制御することで、限られたモーション素材から壁走り・よじ登り・スライディング中に銃を構えるアニメーションを実現している。
https://github.com/Matsumoto0628/PortfolioShuttleGun/blob/e5ba6107af808403d510257db1d451eb65fc434c/Scripts/Player/PlayerMovement.cs#L326-L337

RigBuilderパッケージのIKで手を銃のグリップに追従させ、コンストレイントで頭の向きをエイム方向に合わせている。
<p>
<img width="483" height="514" alt="スクリーンショット 2026-03-11 1 06 11" src="https://github.com/user-attachments/assets/30b737ee-9ac4-4110-a3df-05103b6a0d83" />
</p>


### MVPパターンとReactiveProperty

モデル（Player）の体力・シールド値を `ReactiveProperty` で保持し、Presenterが購読することでUIへの反映をViewから切り離している。
https://github.com/Matsumoto0628/PortfolioShuttleGun/blob/c353d69ce4d4ec002eb37c9a396c40b6cc839d6d/Scripts/Player/Player.cs#L10-L11
https://github.com/Matsumoto0628/PortfolioShuttleGun/blob/956894e84958263627fd12a482150f25a40ff4dd/Scripts/Player/PlayerPresenter.cs#L14-L19

### ジェネリックFSMによるAI制御

`IState<EState, T>` インターフェースと `FSM<EState, T>` クラスをジェネリックで実装し、敵AIの状態管理を行っている。状態ごとに `Enter` / `Update` / `Exit` を持つことで、遷移時の処理が明確に分離されている。
https://github.com/Matsumoto0628/PortfolioShuttleGun/blob/5f59cdf184085ebb474583a4c456923ed7473828/Scripts/Enemy/FSM.cs#L4-L10

### 抽象クラスによる銃の拡張

`Gun` 抽象クラスが `Fire()` の共通処理（Raycast・ソート）を担い、`Impact` / `Apply` / `CanFire` などを抽象メソッドとして派生クラスに委譲している。新しい銃を追加する際は抽象メソッドを実装するだけでよく、呼び出し側のコードを変更する必要がない。
https://github.com/Matsumoto0628/PortfolioShuttleGun/blob/a01ab758ba44d2603f287ab04567805c665839f5/Scripts/Gun/Gun.cs#L15-L27

### UniTaskによる非同期制御

よじ登りではDOTweenのアニメーションを `async/await` で順番に待機することで、コルーチンでは難しかった複数アニメーションの直列実行を簡潔に記述している。
https://github.com/Matsumoto0628/PortfolioShuttleGun/blob/d5b8371df8c36f0f072bc6472156e6f79b39883b/Scripts/Player/PlayerMovement.cs#L770-L779

<br>
<br>

## インストール方法
1. GitHubのReleasesページから `ShuttleGun.zip` をダウンロード
2. ZIPを展開する
3. `ShuttleGun.exe` を起動する

<p>
<img width="1345" height="752" alt="スクリーンショット 2026-03-11 011655" src="https://github.com/user-attachments/assets/8eb7491c-bf5f-41ea-9c22-5656ab4205b3" />
</p>

<br>
<br>

## 操作方法

- **移動**: WASDキー(左スティック)
- **視点移動**: マウス(右スティック)
- **ジャンプ**: SPACEキー(L2)
- **しゃがみ**: SHIFTキー(R2)
- **射撃**: 左クリック(R1)
- **狙う**: 右クリック(L1)
- **リロード**: Rキー(X)
