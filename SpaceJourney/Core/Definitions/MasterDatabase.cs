using System;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// ゲーム全体で参照する「ID→定義/Prefab/アイコン」類をインスペクター上で登録し、
    /// 文字列IDから取得できるようにする。
    ///
    /// 追加：
    /// - ランダム顔アイコンを「男/女/不明」の3枠で管理（インスペクター）
    /// - ランダム名前はインスペクターに出さず、コード内の static テーブルで管理（直書き）
    /// </summary>
    public class MasterDatabase : SceneSingleton<MasterDatabase>
    {
        [Header("キューブグラウンド定義（ID + Prefab）")]
        [SerializeField] private PrefabEntry[] cubeGroundEntries;

        [Header("キューブ本体定義（ID + Prefab）")]
        [SerializeField] private PrefabEntry[] cubeEntries;

        [Header("ソウル定義リスト")]
        [SerializeField] private SoulJobDefinition[] soulJobDefinitions;

        [Header("種族定義リスト")]
        [SerializeField] private RaceDefinition[] raceDefinitions;

        [Header("ボディ職定義リスト（BodyJob）")]
        [SerializeField] private BodyJobDefinition[] bodyJobDefinitions;

        [Header("武器定義リスト")]
        [SerializeField] private WeaponDefinition[] weaponDefinitions;

        [Header("スキル定義リスト")]
        [SerializeField] private SkillDefinition[] skillDefinitions;
        // ------------------------------
        // ランダム顔アイコン（3分類）
        // ------------------------------
        [Header("ランダム顔アイコン：男性（インデックス:int で参照）")]
        [SerializeField] private Sprite[] randomFaceIconsMale;

        [Header("ランダム顔アイコン：女性（インデックス:int で参照）")]
        [SerializeField] private Sprite[] randomFaceIconsFemale;

        [Header("ランダム顔アイコン：不明/中性（インデックス:int で参照）")]
        [SerializeField] private Sprite[] randomFaceIconsUnknown;

        // ------------------------------
        // ランダム名前（直書き・インスペクター非表示）
        // ※「RandomNamesMale」という名前はプロパティと衝突するので使わない
        // ------------------------------
        private static readonly string[] s_randomNamesMale =
        {
            // ここに男名前（例：200個）を入れる
            "アーロン","アーサー","アーネスト","アイヴァン","アイザック","アイデン","アキレス","アシュラフ","アシム","アダム",
            "アディル","アドリアン","アナトリー","アニル","アブドゥル","アマドゥ","アミル","アヤン","アリ","アリフ",
            "アレクサンダー","アレクス","アレッシオ","アレン","アントン","アンドレ","アンドリュー","アントニオ","イーサン","イーライ",
            "イグナシオ","イサーク","イシュマエル","イブラヒム","イリヤ","イワン","ウィリアム","ウスマン","エイデン","エヴァン",
            "エミール","エミリオ","エリアス","エリオット","エリック","エンリケ","オーウェン","オスカー","オスマン","オリヴァー",
            "オリオン","オレグ","カイ","カイオ","カイル","カシム","カスパー","カディール","カビール","カミル",
            "カリム","カルロ","カルロス","カレブ","ガブリエル","ガレス","キアラン","キース","ギル","クイン",
            "クウェンティン","クライヴ","クレイグ","クレメント","クロード","グスタフ","グレゴリー","ケイン","ケヴィン","ケマル",
            "ケン","コーディ","コール","コナー","ゴンサロ","ゴードン","サーシャ","サイモン","サイラス","サウル",
            "サミール","サミュエル","サルヴァトーレ","サンティアゴ","ザイード","ザカリア","シェーン","シグルド","シド","シモン",
            "シャキール","シャルル","シュテファン","ショーン","シルヴァン","シルヴィオ","ジーク","ジャスパー","ジャック","ジャクソン",
            "ジャマル","ジャン","ジュード","ジュリアン","ジョージ","ジョナサン","ジョナス","ジョセフ","ジョシュ","スヴェン",
            "スコット","スティーブン","ステファノ","セドリック","セバスチャン","セルゲイ","セルヒオ","ソーレン","ターレク","タリク",
            "ダニエル","ダミアン","ダリオ","ティモ","テオ","テオドール","デイビッド","トーマス","トビアス","トリスタン",
            "ドミニク","ドミトリ","ナイル","ナサニエル","ナディル","ニコ","ニコラ","ニコラス","ニルス","ノア",
            "ハーリド","ハサン","ヒュー","ヒューゴ","ビクター","ビョルン","ファリド","フィリップ","フィン","フェリックス",
            "フェルナンド","フランコ","フランツ","フレデリク","ブルーノ","ベン","ベンジャミン","ヘクター","ヘンリー","ペドロ",
            "ホセ","ボリス","ポール","マーカス","マテオ","マティアス","マルコ","ミカエル","ミゲル","ミラン",
            "ムサ","ムスタファ","モハメド","ユーリ","ユセフ","ラース","ラウル","ラファエル","リアム","ルーカス",
            "ルイ","ルカ","レオ","レオン","レミ","ロナン","ロベルト","ロマン"
        };

        private static readonly string[] s_randomNamesFemale =
        {
            // ここに女名前（例：200個）を入れる
            "アイシャ","アイリーン","アイリス","アウローラ","アウレリア","アグネス","アデリーヌ","アデル","アドリアナ","アナ",
            "アナイス","アニカ","アニタ","アヌーシュカ","アミーラ","アマラ","アマンダ","アミナ","アリア","アリアナ",
            "アリエル","アリシア","アリナ","アルマ","アルテミス","アレッサ","アレッシア","アン","アンナ","アンヌ",
            "イーデン","イザベル","イリナ","ウルスラ","エヴァ","エヴリン","エステル","エミリア","エミリー","エリザ",
            "エリザベス","エレナ","エレノア","オクサーナ","オリヴィア","カタリナ","カミラ","カリーナ","クララ","グレース",
            "サーラ","サフィア","サブリナ","シャーロット","ジュリア","ジュリエット","シルヴィ","ソフィア","ソニア","タチアナ",
            "ダリア","チアラ","ディアナ","テレサ","デルフィーヌ","ドロテア","ナディア","ナタリア","ニーナ","ノエミ",
            "ハンナ","ビアンカ","ビビアン","ファティマ","フィオナ","フェリシア","フローラ","フランチェスカ","ブリジット","ベラ",
            "ヘレナ","ペネロペ","ポーラ","マーヤ","マリア","マリナ","ミーラ","ミカエラ","ミシェル","ミラ",
            "メイ","モニカ","ヤスミン","ユリア","ラウラ","ラナ","リア","リディア","リナ","ルチア",
            "ルナ","レイラ","レベッカ","ローザ","ロザリンド","アニータ","エレイン","カレン","キャロル","サマラ",
            "シェリー","ステラ","セリーヌ","タマラ","ノラ","ビルギット","フレイヤ","マドレーヌ","ミーナ","ヨハンナ",
            "アニエス","エルザ","オフェリア","カミーユ","クレア","グロリア","サロメ","シモーネ","ダフネ","テオドラ",
            "ナオミ","ニコール","バーバラ","パトリシア","ヒルダ","フィリッパ","フリーダ","ヘスター","マリオン","ミレーヌ",
            "ユーディット","ラファエラ","リサ","ルイーズ","ロレーナ","ヴァレリア","ヴィクトリア","アナスタシア","エスメ","カヤ"
        };

        private static readonly string[] s_randomNamesNeutral =
        {
            "アキ","アリ","アレックス","アン","イーリス","エデン","エル","オリオン","カイ","キラ",
    "クオン","クロ","ケイ","サシャ","シオン","ジュン","スカイ","ソラ","タオ","チヒロ",
    "ツバサ","テラ","ナオ","ニコ","ノア","ハル","ヒカリ","フウ","ミカ","ユイ",
    "ユウ","ヨリ","ラピス","リオ","リン","ルー","レイ","レン","ロウ","アオ",
    "アッシュ","イオ","ウィン","エコー","カナ","キョウ","クレア","コウ","サイ","シエル",
    "ジン","セイ","ゼン","ソウ","タク","ティル","トワ","ナギ","ネオ","ノヴァ",
    "ハク","ヒナ","フレイ","ホシ","ミドリ","ムーン","メル","ヤマト","ユズ","ヨナ",
    "ラル","リュウ","ルミ","レム","ロク","ワカ","アマネ","イズミ","カナタ","マコト"
        };

        [Header("ユニーク顔アイコン（ID:string で参照）")]
        [SerializeField] private UniqueFaceIconDefinition[] uniqueFaceIcons;

        // ---- 公開プロパティ（必要なものだけ）----
        public PrefabEntry[] CubeGroundEntries { get => cubeGroundEntries; set => cubeGroundEntries = value; }
        public PrefabEntry[] CubeEntries { get => cubeEntries; set => cubeEntries = value; }

        public SoulJobDefinition[] SoulJobDefinitions { get => soulJobDefinitions; set => soulJobDefinitions = value; }
        public BodyJobDefinition[] BodyDefinitions { get => bodyJobDefinitions; set => bodyJobDefinitions = value; }

        public Sprite[] RandomFaceIconsMale { get => randomFaceIconsMale; set => randomFaceIconsMale = value; }
        public Sprite[] RandomFaceIconsFemale { get => randomFaceIconsFemale; set => randomFaceIconsFemale = value; }
        public Sprite[] RandomFaceIconsUnknown { get => randomFaceIconsUnknown; set => randomFaceIconsUnknown = value; }

        public UniqueFaceIconDefinition[] UniqueFaceIcons { get => uniqueFaceIcons; set => uniqueFaceIcons = value; }

        public int RandomFaceIconMaleCount => randomFaceIconsMale != null ? randomFaceIconsMale.Length : 0;
        public int RandomFaceIconFemaleCount => randomFaceIconsFemale != null ? randomFaceIconsFemale.Length : 0;
        public int RandomFaceIconUnknownCount => randomFaceIconsUnknown != null ? randomFaceIconsUnknown.Length : 0;

        public int RandomNameMaleCount => s_randomNamesMale != null ? s_randomNamesMale.Length : 0;
        public int RandomNameFemaleCount => s_randomNamesFemale != null ? s_randomNamesFemale.Length : 0;
        public int RandomNameUnknownCount => s_randomNamesNeutral != null ? s_randomNamesNeutral.Length : 0;

        // ░░░░░░░░░░ 共通ヘルパ ░░░░░░░░░░

        private int FindIndexById<T>(T[] array, string id, Func<T, string> getId)
        {
            if (string.IsNullOrEmpty(id)) return -1;
            if (array == null || array.Length == 0) return -1;

            for (int i = 0; i < array.Length; i++)
            {
                var item = array[i];
                if (item == null) continue;
                if (getId(item) == id) return i;
            }
            return -1;
        }

        private bool IsAllDigits(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            for (int i = 0; i < s.Length; i++)
                if (!char.IsDigit(s[i])) return false;
            return true;
        }

        private Sprite GetSpriteByIndex(Sprite[] array, int index)
        {
            if (array == null || array.Length == 0) return null;
            if (index < 0 || index >= array.Length) return null;
            return array[index];
        }

        private string GetNameByIndex(string[] array, int index)
        {
            if (array == null || array.Length == 0) return null;
            if (index < 0 || index >= array.Length) return null;
            return array[index];
        }

        // ░░░░░░░░░░ 定義取得 ░░░░░░░░░░

        public SoulJobDefinition GetSoulJobById(string jobId)
        {
            int index = FindIndexById(soulJobDefinitions, jobId, def => def.JobId);
            return index >= 0 ? soulJobDefinitions[index] : null;
        }

        /// <summary>
        /// 指定された傾向に一致するソウルジョブ定義一覧を返します（nullは除外）。
        /// </summary>
        public SoulJobDefinition[] GetSoulJobsByTendency(SoulJobTendency tendency)
        {
            if (soulJobDefinitions == null || soulJobDefinitions.Length == 0)
                return Array.Empty<SoulJobDefinition>();

            int count = 0;
            foreach (var def in soulJobDefinitions)
            {
                if (def != null && def.Tendency == tendency)
                    count++;
            }
            if (count == 0) return Array.Empty<SoulJobDefinition>();

            var result = new SoulJobDefinition[count];
            int idx = 0;
            foreach (var def in soulJobDefinitions)
            {
                if (def != null && def.Tendency == tendency)
                    result[idx++] = def;
            }
            return result;
        }

        /// <summary>
        /// 指定された傾向の中から、jobEasePercent を重みとしてランダムに1件返します。
        /// </summary>
        public SoulJobDefinition GetRandomSoulJobByTendency(SoulJobTendency tendency)
        {
            var list = GetSoulJobsByTendency(tendency);
            if (list == null || list.Length == 0) return null;

            int total = 0;
            for (int i = 0; i < list.Length; i++)
            {
                var def = list[i];
                if (def == null) continue;
                int w = Mathf.Max(0, def.JobEasePercent);
                total += w;
            }

            if (total <= 0)
            {
                // 全ジョブの jobEasePercent が 0 以下だった場合は等確率ランダム
                return list[UnityEngine.Random.Range(0, list.Length)];
            }

            int r = UnityEngine.Random.Range(0, total);
            for (int i = 0; i < list.Length; i++)
            {
                var def = list[i];
                if (def == null) continue;
                int w = Mathf.Max(0, def.JobEasePercent);
                if (r < w) return def;
                r -= w;
            }

            // フォールバック（理論上ここには来ないはず）
            return list[UnityEngine.Random.Range(0, list.Length)];
        }


        public RaceDefinition GetRaceById(string id)
        {
            int index = FindIndexById(raceDefinitions, id, def => def.raceId);
            return index >= 0 ? raceDefinitions[index] : null;
        }

        public BodyJobDefinition GetBodyJobById(string id)
        {
            int index = FindIndexById(bodyJobDefinitions, id, def => def.bodyJobId);
            return index >= 0 ? bodyJobDefinitions[index] : null;
        }

        /// <summary>
        /// 互換：昔のコードが GetBodyById を呼んでいても壊れないように残す。
        /// 最終的には全呼び出しを GetBodyJobById に寄せて削除してOK。
        /// </summary>
        public BodyJobDefinition GetBodyById(string bodyId) => GetBodyJobById(bodyId);

        public WeaponDefinition GetWeaponById(string id)
        {
            int index = FindIndexById(weaponDefinitions, id, def => def.weaponId);
            return index >= 0 ? weaponDefinitions[index] : null;
        }

        public SkillDefinition GetSkillById(string id)
        {
            int index = FindIndexById(skillDefinitions, id, def => def.SkillId);
            return index >= 0 ? skillDefinitions[index] : null;
        }
        // ░░░░░░░░░░ Prefab取得 ░░░░░░░░░░

        public GameObject GetCubeGroundById(string groundId) => GetPrefabById(cubeGroundEntries, groundId);
        public GameObject GetCubeById(string cubeId) => GetPrefabById(cubeEntries, cubeId);

        private GameObject GetPrefabById(PrefabEntry[] entries, string id)
        {
            if (entries == null || entries.Length == 0) return null;

            int index = FindIndexById(entries, id, e => e.Id);
            if (index < 0) return null;

            var entry = entries[index];
            return entry != null ? entry.Prefab : null;
        }

        // ░░░░░░░░░░ 顔＆名前取得 ░░░░░░░░░░

        public string GetRandomFaceIconId(FaceSexCategory category)
        {
            // 指定カテゴリの枠で 0..count-1 のインデックスをランダム選択し、数字文字列として返す。
            if (TryPick(category, out string id)) return id;

            // フォールバック（安全）：Unknown → Male → Female
            if (TryPick(FaceSexCategory.Unknown, out id)) return id;
            if (TryPick(FaceSexCategory.Male, out id)) return id;
            if (TryPick(FaceSexCategory.Female, out id)) return id;

            // どれも未登録なら "0" を返す（描画側で null になる可能性はある）
            return "0";

            bool TryPick(FaceSexCategory cat, out string result)
            {
                int count = GetRandomFaceIconCount(cat);
                if (count <= 0)
                {
                    result = null;
                    return false;
                }

                int index = UnityEngine.Random.Range(0, count);
                result = index.ToString();
                return true;
            }
        }

        private int GetRandomFaceIconCount(FaceSexCategory sex)
        {
            switch (sex)
            {
                case FaceSexCategory.Male: return RandomFaceIconMaleCount;
                case FaceSexCategory.Female: return RandomFaceIconFemaleCount;
                default: return RandomFaceIconUnknownCount;
            }
        }


        public string GetRandomName(FaceSexCategory category, int index)
        {
            switch (category)
            {
                case FaceSexCategory.Male: return GetNameByIndex(s_randomNamesMale, index);
                case FaceSexCategory.Female: return GetNameByIndex(s_randomNamesFemale, index);
                default: return GetNameByIndex(s_randomNamesNeutral, index);
            }
        }

        public Sprite GetFaceIconById(string iconId)
        {
            // sex 不明の場合は Unknown 枠として扱う（従来互換）。
            return GetFaceIconById(iconId, FaceSexCategory.Unknown);
        }

        public Sprite GetFaceIconById(string iconId, FaceSexCategory sex)
        {
            if (string.IsNullOrEmpty(iconId)) return null;

            // 1) ノーマル顔：数字だけなら、sex 枠のランダム顔配列から index を解決
            if (IsAllDigits(iconId))
            {
                if (!int.TryParse(iconId, out int index)) return null;

                // sex 枠を優先し、足りない場合は Unknown → Male → Female の順でフォールバック
                Sprite sprite = GetRandomFaceSpriteBySexAndIndex(sex, index);
                if (sprite != null) return sprite;

                sprite = GetRandomFaceSpriteBySexAndIndex(FaceSexCategory.Unknown, index);
                if (sprite != null) return sprite;

                sprite = GetRandomFaceSpriteBySexAndIndex(FaceSexCategory.Male, index);
                if (sprite != null) return sprite;

                sprite = GetRandomFaceSpriteBySexAndIndex(FaceSexCategory.Female, index);
                return sprite;
            }

            // 2) ユニークID：従来どおり UniqueFaceIcons から検索
            if (uniqueFaceIcons == null || uniqueFaceIcons.Length == 0) return null;

            int uniqueIndex = FindIndexById(uniqueFaceIcons, iconId, def => def.UniqueIconId);
            if (uniqueIndex < 0) return null;

            var def2 = uniqueFaceIcons[uniqueIndex];
            return def2 != null ? def2.IconSprite : null;
        }

        private Sprite GetRandomFaceSpriteBySexAndIndex(FaceSexCategory sex, int index)
        {
            switch (sex)
            {
                case FaceSexCategory.Male:
                    return GetSpriteByIndex(randomFaceIconsMale, index);
                case FaceSexCategory.Female:
                    return GetSpriteByIndex(randomFaceIconsFemale, index);
                default:
                    return GetSpriteByIndex(randomFaceIconsUnknown, index);
            }
        }


        private bool TryParsePrefixedIndex(string iconId, out FaceSexCategory category, out int index)
        {
            category = FaceSexCategory.Unknown;
            index = -1;

            // 形式：X_123
            if (iconId.Length < 3) return false;
            if (iconId[1] != '_') return false;

            char p = iconId[0];
            string num = iconId.Substring(2);
            if (!int.TryParse(num, out index)) return false;

            switch (p)
            {
                case 'M': category = FaceSexCategory.Male; return true;
                case 'F': category = FaceSexCategory.Female; return true;
                case 'U': category = FaceSexCategory.Unknown; return true;
                default: return false;
            }
        }

    }
}
