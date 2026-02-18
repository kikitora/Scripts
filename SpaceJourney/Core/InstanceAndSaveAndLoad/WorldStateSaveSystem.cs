// WorldStateSaveSystem.cs
// このクラスで何をするか：
// ・WorldState 全体を JSON ファイルとして保存／ロードするヘルパークラス。
// ・マップ上のキューブ配置 / ソウル / ボディ / フィールド状態など、
//   「ジャーニーモードの世界そのもの」を丸ごとセーブデータ化する。
// 他クラスから主に使うメソッド：
// - SaveWorld(WorldState world)              : 現在の WorldState をディスクに保存
// - bool TryLoadWorld(string profileId, out WorldState world)
//                                        : プロフィールIDから WorldState を復元
//
// ※注意点：
// - MasterDatabase.Instance が存在しているタイミングで Load を呼ぶと、
//   SoulInstance.OnAfterDeserialize() と PostLoadFixup により、
//   JobDefinition や IconSprite が安全に再解決される。

using System;
using System.IO;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    public static class WorldStateSaveSystem
    {
        /// <summary>
        /// セーブファイル名のフォーマット。
        /// profileId を埋め込んだ "world_xxx.json" という名前にする。
        /// </summary>
        private const string FileNameFormat = "world_{0}.json";

        //======================================================================
        // 公開API：セーブ
        //======================================================================

        /// <summary>
        /// 現在の WorldState を JSON として保存します。
        /// profileId が空なら "default" として保存されます。
        /// </summary>
        public static void SaveWorld(WorldState world)
        {
            if (world == null)
            {
                Debug.LogWarning("[WorldStateSaveSystem] SaveWorld: world が null です。");
                return;
            }

            // プロファイルIDが空なら強制的に "default"
            string profileId = string.IsNullOrEmpty(world.ProfileId)
                ? "default"
                : world.ProfileId;

            string path = GetSavePath(profileId);

            try
            {
                // WorldState → JSON 文字列
                string json = JsonUtility.ToJson(world, prettyPrint: true);

                // ディレクトリ作成
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // ファイル書き込み
                File.WriteAllText(path, json);

#if UNITY_EDITOR
                Debug.Log($"[WorldStateSaveSystem] Saved WorldState to: {path}\n{json}");
#else
                Debug.Log($"[WorldStateSaveSystem] Saved WorldState to: {path}");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WorldStateSaveSystem] SaveWorld 失敗: {ex}");
            }
        }

        //======================================================================
        // 公開API：ロード
        //======================================================================

        /// <summary>
        /// 指定した profileId 用の WorldState をロードします。
        /// セーブファイルが存在しない / 読み込み失敗時は false を返し、world は null になります。
        /// </summary>
        public static bool TryLoadWorld(string profileId, out WorldState world)
        {
            world = null;

            if (string.IsNullOrEmpty(profileId))
            {
                profileId = "default";
            }

            string path = GetSavePath(profileId);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[WorldStateSaveSystem] セーブファイルが存在しません: {path}");
                return false;
            }

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError($"[WorldStateSaveSystem] セーブファイルが空です: {path}");
                    return false;
                }

                // JSON → WorldState
                world = JsonUtility.FromJson<WorldState>(json);

                // SoulInstance.OnAfterDeserialize でも解決されているはずだが、
                // 念のため MasterDatabase 初期化後にもう一度再解決を掛けておく。
                PostLoadFixup(world);

#if UNITY_EDITOR
                Debug.Log($"[WorldStateSaveSystem] Loaded WorldState from: {path}\n{json}");
#else
                Debug.Log($"[WorldStateSaveSystem] Loaded WorldState from: {path}");
#endif

                return (world != null);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WorldStateSaveSystem] LoadWorld 失敗: {ex}");
                world = null;
                return false;
            }
        }

        //======================================================================
        // 内部ヘルパー
        //======================================================================

        /// <summary>
        /// 実際のセーブパス（Application.persistentDataPath/SaveData/world_xxx.json）を返します。
        /// </summary>
        private static string GetSavePath(string profileId)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                profileId = "default";
            }

            string fileName = string.Format(FileNameFormat, profileId);
            string dir = Path.Combine(Application.persistentDataPath, "SaveData");
            return Path.Combine(dir, fileName);
        }

        /// <summary>
        /// ロード直後の WorldState に対して、各種再解決や補完処理を行う。
        /// - MasterDatabase からの再解決（ソウルの JobDef / IconSprite）
        /// - BodyInstance の instanceId 補完（古いセーブデータ対策）
        /// </summary>
        private static void PostLoadFixup(WorldState world)
        {
            if (world == null) return;

            var db = MasterDatabase.Instance;
            if (db == null)
            {
                // ここに来る場合：
                // - MasterDatabase がまだ起動していない
                // → タイトルシーンなど、別の場所でロードするなら、
                //   MasterDatabase 初期化後に WorldState 側を再走査する処理を足してもOK。
                Debug.LogWarning("[WorldStateSaveSystem] PostLoadFixup: MasterDatabase.Instance が null です。");
                return;
            }

            // ─────────────────────────────────────
            // 1) ボディの instanceId を補完
            //    - 新しいセーブデータ: JSONにinstanceIdが含まれていればそのまま
            //    - 古いセーブデータ: instanceId が空のものに GUID を発行
            // ─────────────────────────────────────
            if (world.ExBodys != null)
            {
                foreach (var body in world.ExBodys)
                {
                    body?.EnsureInstanceId();
                }
            }

            // ─────────────────────────────────────
            // 2) ソウルの JobDef / IconSprite を再解決
            // ─────────────────────────────────────
            if (world.ExSouls != null)
            {
                foreach (var soul in world.ExSouls)
                {
                    soul?.ResolveJobsForReinSouls();
                    soul?.ResolveIconSpriteFromDatabase();
                }
            }
        }

    }
}
