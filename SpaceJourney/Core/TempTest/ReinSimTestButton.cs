using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 転生シミュレーションのテスト用 MonoBehaviour。
    /// 適当なGameObjectに貼り、ボタンのOnClickに TestReincarnation() を登録する。
    ///
    /// 処理の流れ：
    ///   1. 仮ソウル4体生成（registerToWorld:false → WorldState未登録）
    ///   2. 転生シミュ実行 → 転生済み主魂が完成
    ///   3. 完成した主魂だけ EnsureInstanceId() → WorldState登録
    /// </summary>
    public class ReinSimTestButton : MonoBehaviour
    {
        [Header("テスト設定")]
        [SerializeField, Range(1, 10)] private int testRank = 3;
        [SerializeField] private SoulJobTendency testTendency = SoulJobTendency.Warrior;

        /// <summary>ボタンのOnClickに登録する。</summary>
        public void TestReincarnation()
        {
            var soul = CreateReincarnatedSoul(testRank, testTendency);
            if (soul != null)
                Debug.Log($"[ReinSimTest] 完了: {soul.SoulTendency} Rank={soul.Rank}");
        }

        /// <summary>
        /// 転生済みソウルを1体生成してWorldStateに登録して返す。
        /// 仮の4体（シミュ入力用）は登録しない。
        /// </summary>
        public static SoulInstance CreateReincarnatedSoul(int rank, SoulJobTendency tendency)
        {
            var db = MasterDatabase.Instance;
            if (db == null)
            {
                Debug.LogError("[ReinSimTest] MasterDatabase が見つかりません。");
                return null;
            }

            var allEvents = db.ReinLifeEvents;
            if (allEvents == null || allEvents.Length == 0)
            {
                Debug.LogError("[ReinSimTest] MasterDatabase.ReinLifeEvents が空です。SOを登録してください。");
                return null;
            }

            // ---- 仮ソウル4体生成（registerToWorld:false → WorldState未登録）----
            var mainSoul = SoulInstance.CreateRandomInitialSoul(rank, tendency, registerToWorld: false);

            var allTendencies = (SoulJobTendency[])System.Enum.GetValues(typeof(SoulJobTendency));
            var guardians = new SoulInstance[3];
            for (int i = 0; i < 3; i++)
            {
                var t = allTendencies[UnityEngine.Random.Range(0, allTendencies.Length)];
                guardians[i] = SoulInstance.CreateRandomInitialSoul(rank, t, registerToWorld: false);
            }

            // ---- 転生シミュ実行 ----
            var reinData = ReinSimRunner.Run(
                mainSoul: mainSoul,
                guardians: guardians,
                allEvents: allEvents,
                replaceIndex: -1
            );

            if (reinData == null)
            {
                Debug.LogError("[ReinSimTest] ReinSimRunner.Run が失敗しました。");
                return null;
            }

            // ---- 完成した主魂だけ WorldState に登録 ----
            mainSoul.EnsureInstanceId();

            return mainSoul;
        }
    }
}