-----------------------------------------------------
	Very Animation
	Copyright (c) 2017 AloneSoft
	http://alonesoft.sakura.ne.jp/
	mail: support@alonesoft.sakura.ne.jp
	twitter: @AlSoSupport
-----------------------------------------------------

"Very Animation"をご購入いただきありがとうございます。


【更新方法】
・Assets/VeryAnimationを削除
・再度VeryAnimationをインポート


【ドキュメント】
Assets/VeryAnimation/Documents


【デモ】
Assets/VeryAnimation/Demo


【サポート】
mail: support@alonesoft.sakura.ne.jp
twitter: @AlSoSupport


【更新履歴】
Version 1.1.16
- ADD : Editor Window : Selectionの項目をScene Window上に表示する機能追加
- ADD : Timeline : DummyObject表示モードを追加しデフォルト設定を変更
- FIX : Hierarchy : HumanoidのHipsは編集対象がないためデフォルトで表示フラグ無効に変更
- FIX : Hierarchy : Humanoidのまとめて操作ボタン'Head'を'Face'に変更
- FIX : Unity2019.3 : 表示の問題を修正

Version 1.1.15
- ADD : Tools/Root Motion : Motionカーブを編集する機能を追加
- ADD : Animator IK : Scaleが変更されていることによりIKが正常に動作しない場合の警告を追加
- FIX : Humanoid : Avatarで設定されたboneが削除されていた場合のエラー表示対応
- FIX : Tools/Keyframe Reduction : Parameter RelatedカーブやGenericのRootカーブが削減されない問題の修正
- FIX : Gizmo : フォーカス状態のWindowがない場合にハンドル表示などが消える問題の修正
- FIX : Hierarchy : Projectでオブジェクトを選択する場合にコンソールにエラーが表示されることがある問題の修正

Version 1.1.14
- ADD : Unity2019.2対応
- ADD : Animation Window : Propertyの選択状態によるソート、フィルター対応
- FIX : Root Correction : 既存のモーション編集などでRootQが反転する問題修正
- FIX : IK : Parent空間動作の修正
- FIX : Settings : 項目の整理
- FIX : Animator IK : Head : Advanced設定でWeightが合計1になるように修正
- FIX : Hierarchy : 選択したオブジェクトがフレーム内に表示されるように修正
- FIX : Timeline : Animator Controllerがセットされていない場合のFoot IK更新がおかしくなっていた問題の修正
- FIX : Unity2019.3で発生するエラーの修正

Version 1.1.13p1
- FIX : Preview : Root Motion関係の修正、情報の追加
- FIX : Hand toolが動作するように修正
- FIX : 何も選択していない場合のEdit/Frame Selectedの動作を修正
- FIX : 編集中にHierarchyに変更があった場合に編集を継続するよう修正
- FIX : AnimationWindowとHierarchyWindowが同じDockAreaに属している場合、編集開始後すぐに終了してしまう問題の対処
- FIX : Legacy Animation : ClipがLoopする設定だと最終フレームで0フレームに戻る問題の修正

Version 1.1.13
- ADD : Options : Collision機能追加
- ADD : IK : Syncボタン追加
- FIX : Preview : SceneでのTransform情報をPreviewにそのまま適用するように修正

Version 1.1.12
- ADD : Tools : Create New Keyframe
- FIX : Keyframe Reduction : GameObjectの名前に'.'が含まれる場合に削減失敗する不具合修正
- FIX : Shortcuts : Unity2019.1.4f1からHキーがGlobalでなくなったので表示切り替えのキーをHキーに再変更
- FIX : Tools : Keyframe Reduction : Error値の指定が反映されなくなっていた不具合修正
- FIX : VA Tools : 全体処理でアクティブではない対象が処理されていない不具合修正
- FIX : エラーで起動しない問題の修正

Version 1.1.11p1
- FIX : 編集中に他のスクリプトなどが誤動作しないよう修正
- FIX : DaeExporter : Importerにできるだけ設定をそのまま引き継ぐように変更
- FIX : Keyframe Reduction : 回転が正常に削減されない不具合修正

Version 1.1.11
- ADD : Tools : Generic Root Motion
- FIX : 前のバージョンまでの手順ではGeneric Root Motionが正常に動作しないデータが存在するため、手順を変更
- FIX : DaeExporter : BindposeとPrefabポーズが異なるモデルでの出力不具合修正
- FIX : Keyframe Reduction : エラーによりRotationカーブが削減できない場合がある問題の修正
- FIX : Create New Clip : Mirror : Humanoid TDOFのミラー不具合修正

Version 1.1.10p1
- FIX : SaveSettings : PrefabにSaveSettingsがある場合にOverrides情報が異常になることがある問題の修正
- FIX : Assembly Definition File
- FIX : Skeletonのライン描画高速化

Version 1.1.10
- ADD : Extra functions/Trail of root position 追加
- FIX : Extra functions : 表示をFoldoutに対応
- FIX : BlendShape : 同じ名前が複数が存在する場合にエラーになる問題の修正
- FIX : 編集中のAnimationClipの変更が反映されないことがあったバグの修正
- FIX : 実行中の編集 : Speedが1ではない場合にTimeがずれる問題の修正

Version 1.1.9p2
- FIX : Particle Systemなどのアイコン取得エラーがログに表示される問題の修正
- FIX : SaveData : ロード処理のエラーチェック強化

Version 1.1.9p1
- FIX : EditorWindow : Blend Pose, Muscle Group, Blend Shape, SelectionのUI修正

Version 1.1.9
- ADD : Assembly Definition File : support
- ADD : Unity2019.1 : Timelineのアンインストールに対応
- ADD : Onion Skinning
- FIX : Timeline : IK : Space Parent 

Version 1.1.8
- ADD : Unity2019.1対応
- ADD : Shortcut Manager : 対応
- ADD : Tools : Combine追加
- FIX : Shortcut : 一部ショートカットの変更 (Unity2019.1以降のショートカットと競合するため)

Version 1.1.7
- ADD : Editor Window : ドッキングまたはフローティングのWindowスタイルを設定で選択できるように対応
- ADD : Generic Mirror : Scaleのミラー機能追加
- FIX : Create New Clip : Animator Controllerへの更新が保存されない場合がある問題の修正
- FIX : AnimatorIK : IKの精度修正
- FIX : IK : Animation Windowでの変更でIKが動作するよう対応

Version 1.1.6p1
- FIX : Keyframe Reduction : ModelImporter以外で作成されたHumanoidのAvatarで正常に動作しない場合がある問題の修正
- FIX : Keyframe Reduction : FootIKのために一部のカーブの削減を無効化する"Root and IK Goal Curves"フラグを追加
- FIX : Export : 出力ポーズが編集開始時のポーズになっていた問題を修正

Version 1.1.6
- ADD : BlendShape : すべての値を名前をつけて保存して再利用できる機能の追加
- FIX : 編集中にダブルクリックで編集対象を変更する機能でBlendShapeの情報などが変更されていなかった問題の修正

Version 1.1.5p1
- FIX : Timeline : 編集終了時に編集前のポーズに戻らない問題の修正
- FIX : Timeline : AnimatorIKとOriginalIKの主にLocal空間とParent空間の動作修正

Version 1.1.5
- ADD : Synchronize Animation : 複数対象をプレビューしながら編集するための機能
- ADD : 編集対象のAnimationClipを選択するUIを編集開始前の画面に追加
- ADD : 最後に選択されていたAnimationClipの保存と復元追加
- ADD : 編集中に他のGameObjectをダブルクリックすることで編集対象を変更する機能追加
- ADD : Settings : Tooltipの日本語化
- FIX : 一時停止での編集終了時に位置がずれる問題の修正
- FIX : 内部計算によってAnimator.Updateでイベントが呼び出されていた問題の修正
- FIX : 高速化

Version 1.1.4
- ADD : Unity2018.3対応
- ADD : 編集終了系動作にエラーログ表示追加
- ADD : DummyObjectカラー変更有効フラグ追加
- FIX : 一時停止での編集で位置がずれる問題の修正
- FIX : RootMotion関係の警告を追加
- FIX : MirrorBone保存関係修正
- FIX : Unity2019.1エラー

Version 1.1.3p2
- FIX  : ControlWindow/Selection : Move select : Active Game Objectの修正
- FIX : Pivot Center : 動作修正
- FIX : OriginalIK : Basic : Weightの初期値を1.0に修正
- FIX : OriginalIK : Resetの修正

Version 1.1.3p1
- ADD : サポート外のバージョンのUnityでのメッセージ表示を追加
- ADD : ControlWindow/Selection : Move select追加
- FIX : ControlWindow/Humanoid : Windowスクロール時の範囲選択バグ修正
- FIX : Generic Mirror : 動作修正
- FIX : Pivot Center : 動作修正
- FIX : 高速化

Version 1.1.3
- ADD : Mirror : Mirror対象の手動設定対応
- ADD : EditorWindow : HumanoidのボーンにRotation表示を追加
- ADD : Tools/Create new clip : ミラー追加
- ADD : Tools/Keyframe Reduction : 追加
- ADD : ControlWindow : Hierarchy : Mirror対象ボタンの追加
- ADD : EditorWindow : MuscleGroup, BlendShape : Mirror対象ボタンの追加
- ADD : Tools/Export : Active Only追加
- ADD : ToolWindow/Reset Pose, Template Pose : BlendShapeの対応追加
- FIX : EditorWindow : HumanoidのボーンのTDOF表示をPositionに変更
- FIX : Tools/Copy : コピー処理修正
- FIX : Tools/Cleanup : BlendShape処理修正
- FIX : Pose/Mirror : 処理修正
- FIX : Generic Mirror : ミラーボーン検索修正
- FIX : BlendShape Mirror : Timelineで動作しない問題の修正
- FIX : Humanoid TDOF : ハンドルのローカル軸修正
- FIX : BlendShape : リセット値の修正
- FIX : Mirror : 処理修正

Version 1.1.2p3
- FIX : Options/FootIK : カーブ生成部分の修正
- FIX : Tools/Create new clip : ファイルを上書きすると編集モードが終了する問題の修正

Version 1.1.2p2
- FIX : モデルに追加のボーンなどが作成されているとエラーで起動しない問題の修正

Version 1.1.2p1
- FIX : Resetなどでの新規アニメーションカーブ作成判定修正
- FIX : Mirror : GenericのMirror逆回転修正
- FIX : Tools/Rotation Curve Interpolation : Euler Angles (Quaternion) -> Quaternion変換修正

Version 1.1.2
- ADD : Tools/Range IK
- ADD : Animator IK, Original IK : ターゲット情報のコピー＆ペースト対応
- ADD : Animator IK : Footに踵の操作ハンドル追加
- FIX : Mirror : GenericのMirror動作修正
- FIX : Mac : Macエディタでのショートカットキーの不具合を修正
- FIX : Humanoid : ボーンのスケールが1ではない場合にBindとPrefabボタンの動作とAnimator IKの動作に異常があった問題の修正
- FIX : Timeline : Animator IK : Parent空間での動作をダミーオブジェクト空間動作からオブジェクト空間動作に変更
- FIX : Root Correction : Copy & PasteでRoot補正が動作していなかった問題の修正 
- FIX : Animator IK : 手足が真っ直ぐに伸びている場合のSwivel値取得修正
- FIX : Animator IK, Original IK : Fixed機能をオミット
- FIX : Settings : ダミーオブジェクト表示設定のデフォルトを変更
- FIX : Unity2018.3 : エラー修正
- FIX : Obsolete API
- FIX : 高速化

Version 1.1.1p4
- FIX : BlendShapeに同じ名前が複数存在する場合にエラーで動作しないことがある問題の修正

Version 1.1.1p3
- FIX : 同じ階層に同じ名前のGameObjectが複数存在する場合にエラーで動作しない問題の修正

Version 1.1.1p2
- FIX : BindPose取得処理のエラーで動作しない問題の対応
- FIX : IK : IK無効状態でリスト選択するとBoneが選択されるよう対応
- FIX : アニメーションカーブ更新処理の修正
- FIX : アニメーションカーブ更新でAnimationWindowのキーフレーム選択解除されないよう変更

Version 1.1.1p1
- ADD : Tools/Clearnup : BlendShapeを追加
- FIX : Tools/Clearnup : EyeとJawとToeを個別に変更
- FIX : Mirror : AnimationWindowでの操作で動作しないことがある問題を修正
- FIX : MuscleGroup/Part : HeadからEyeとJawをFaceとして独立
- FIX : Unity2018.2 : "Open Animation Window"ボタンが動作しない問題の修正

Version 1.1.1
- ADD : Unity2018.2対応
- ADD : Muscle Group : Foldoutを対象のNodeのBone選択ボタンに変更
- ADD : Blend Shape : Foldoutを対象のNodeのBone選択ボタンに変更
- ADD : 複数選択 : ハンドル操作に最大親子関係階層数を参照した補正を追加
- ADD : Animation Window : Animation Window側で操作された場合にClampとMirrorとRoot Correction動作の対応
- ADD : Animation Window : AnimationCurve選択がシンクロする場合に現在時間にあるKeyframeを選択する動作の対応
- FIX : Blend Shape : コピーペースト不具合
- FIX : Animator IK : 計算の修正
- FIX : Original IK : 計算の修正
- FIX : Root Correction : 最終フレーム以降への補間修正
- FIX : 高速化

Version 1.1.0p1
- FIX : MenuItem : priority指定を削除
- FIX : UnityEditor言語変更による不具合修正
- FIX : RootCorrection : 更新方式修正
- FIX : FootIK : Timelineリンク状態では強制的に有効になるよう変更
- FIX : FootIK : 更新方式修正
- FIX : Profiler関係が残っていたので削除
- FIX : Unity2018.2 : Timeline動作

Version 1.1.0
- ADD : Unity2018.1対応
- ADD : 言語選択 : 日本語対応
- ADD : Humanoid Root補正 : LockボタンからDisable,Single,Fullのボタンへ変更、Full機能追加
- FIX : AnimationWindowで編集された場合の動作修正
- FIX : OriginalIK : Basic : 先端の初期Weightを0.5に変更
- FIX : Selection Set : デフォルト名をアクティブオブジェクトから設定
- FIX : Hierarchy : Icon取得の変更
- FIX : 逆回転補正処理の修正
- FIX : FootIK : 更新処理変更
- FIX : BlendShape : ミラー対応が不足していた部分の対応、リセットで0でなく編集開始時点の値になるよう変更
- FIX : Unity2018.2で発生するエラー修正

Version 1.0.9
- ADD : PivotMode.Centerでの複数選択動作
- ADD : 'Based Upon'設定がOriginalではない場合の警告追加
- FIX : 複数選択動作の改善
- FIX : AnimationWindowで編集された場合の動作修正
- FIX : 高速化

Version 1.0.8
- ADD : Blend Pose
- FIX : 実行時の編集 : ショートカット起動で正しいアニメーションと時間を取得していなかったバグ修正
- FIX : 実行時の編集 : 編集後に元の位置に戻らない場合があるバグ修正
- FIX : Exporter : Texture2DではないTextureもテクスチャ出力対応、エラーチェック追加
- FIX : AnimatorIK : Head : 意図しない初期化が起こっていたバグ修正
- FIX : Select Bone : 最前面ポリゴンが選択されないことがあるバグ修正
- FIX : 高速化

Version 1.0.7p1
- FIX : 編集時のAnimationWindow自動ロック : Timelineでの無効化
- FIX : 選択セット : Nullエラー

Version 1.0.7
- ADD : 選択セット
- ADD : 編集時のAnimationWindow自動ロック
- ADD : 編集モード強制終了のエラー表示追加
- FIX : Tools : Clearnup : BlendShape情報の対応
- FIX : ControlWindow : Humanoid選択処理修正

Version 1.0.6
- ADD : BlendShape編集
- FIX : AnimatorIK : HeadのSwivel対応
- FIX : FreeRotateHandleが動作していなかったバグ修正
- FIX : EditorWindow : ToolBarを追加、以前の要素はOptionsに移動
- FIX : MuscleGroup : Resetでカーブの追加が必要でない場合でもカーブを作成していたバグ修正
- FIX : IKTarget : 空間がGlobal以外の場合の範囲選択不具合修正
- FIX : Humanoid : Neckが存在しない場合にHeadのGlobal回転がおかしくなるバグ修正
- FIX : その他バグ修正、高速化

Version 1.0.5
- ADD : Tools : Create New Clip
- ADD : 起動ショートカットキー対応
- FIX : AnimatorがAvatar作成時と違う階層へ移動されている場合の動作修正
- FIX : Glocal回転の操作が回転ぶん回らないような挙動修正
- FIX : IKTargetのMirror反映でお互いの空間が違う場合の不具合修正
- FIX : DaeExporter : '_Color'プロパティがないマテリアルでのエラー修正

Version 1.0.4
- ADD : IK : Global,Local,Parent空間切り替え対応
- ADD : IK : Rotationの自動反映切り替え
- FIX : Mirror側のカーブの変更がAnimationWindowへ反映されていないバグ修正
- FIX : その他バグ修正、高速化
- FIX : ドキュメント : 説明を追加
- FIX : Unity 2018.1 : エラーを修正

Version 1.0.3
- ADD : Original IK : Limb IK
- FIX : Original IK : GUI
- FIX : ショートカットキーの処理をEditorWindowがフォーカス状態にも有効に変更
- FIX : 一部のショートカットキー変更

Version 1.0.2p2
- FIX : Timeline : Dummy Objectが表示されなくなる問題の修正
- FIX : Timeline : Active変更への修正

Version 1.0.2p1
- ADD : Timeline : Dummy Timeline Position Type
- FIX : Timeline : Root : Reset All

Version 1.0.2
- ADD : Original IK
- ADD : Toolbar有効状態の保存
- ADD : IKの範囲選択対応
- ADD : Hierarchy : Settingsに選択オブジェクト自動展開設定を追加
- FIX : ショートカットキーの処理をSceneViewがフォーカス状態のみ有効に変更
- FIX : Animator IK
- FIX : Settings : IK Default
- FIX : 逆回転修正処理

Version 1.0.1
- ADD : Legacy(Animation Component)のサポート
- FIX : VA Tools : Remove Save Settings と Replace Reference

Version 1.0.0p1
- ADD : Generic Mirror条件設定、無視設定
- FIX : Save Settings

Version 1.0.0
- ファーストリリース
