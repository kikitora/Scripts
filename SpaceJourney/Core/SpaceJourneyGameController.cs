// このクラスで何をするか：
// SpaceJourneyモードのゲーム全体を管理する土台クラス。
// モード切り替え（探索/編成/戦闘など）とカメラ位置の制御のハブになる予定。
// まずはシーン起動時の初期モード設定とカメラ初期位置のセットだけ実装する。

using System.Collections;
using UnityEngine;
using SteraCube.SpaceJourney;

public class SpaceJourneyGameController : SceneSingleton<SpaceJourneyGameController>
{
    [SerializeField] private CameraController cameraController;

    private void Start()
    {
        // とりあえず Exploration モードで開始（仕様に合わせてここは変えていく）
        ModeRouter.Instance.SwitchMode(GameMode.Exploration, force: true);

        // カメラを初期位置に移動（フィールド全体を上から見る感じ）
        if (cameraController != null)
        {
            StartCoroutine(cameraController.ChangeModeToPlayerFollow());
        }

        // 今後：
        // - 初期キューブの生成＆配置
        // - CommentUI でオープニング説明
        // - プレイヤー入力の有効化タイミング制御
        // などをここに足していく
    }
}
