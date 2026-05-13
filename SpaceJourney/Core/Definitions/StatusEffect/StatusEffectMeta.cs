// StatusEffectMeta.cs
// 状態異常 (StatusEffectType) の slot / rank を引くための static ユーティリティ。
// 起動時に MasterDatabase 経由で StatusEffectMetaDatabase を Init し、
// ApplyStatusEffect 等から GetSlot/GetRank で参照する。

using UnityEngine;

namespace SteraCube.SpaceJourney
{
    public static class StatusEffectMeta
    {
        private static StatusEffectMetaDatabase _db;

        /// <summary>
        /// 起動時に MasterDatabase 経由で 1 度だけ呼ぶ。db が null でも例外は出さず、
        /// GetSlot/GetRank はデフォルト値を返す (運用上の安全装置)。
        /// </summary>
        public static void Init(StatusEffectMetaDatabase db)
        {
            _db = db;
            if (db != null) db.BuildCache();
        }

        /// <summary>
        /// 指定 enum の slot を返す。db 未初期化や entry 未登録の場合は None。
        /// </summary>
        public static StatusEffectSlot GetSlot(StatusEffectType type)
        {
            if (_db == null)
            {
                // 戦闘中に毎フレーム警告が出ると地獄なので、警告は init 失敗時に 1 度だけ
                // (ここでは沈黙)。 None を返す = 同 slot 比較で必ず 1 つの None 枠に集まる
                // = 全効果が排他になってしまう。
                // 開発初期は問題になりやすいので、まず Database を必ず登録する想定。
                return StatusEffectSlot.None;
            }
            var entry = _db.Get(type);
            return entry != null ? entry.slot : StatusEffectSlot.None;
        }

        /// <summary>
        /// 指定 enum のランクを返す。db 未初期化や entry 未登録の場合は 0。
        /// 高い方が同 slot 内の上書き勝負で勝つ。
        /// </summary>
        public static int GetRank(StatusEffectType type)
        {
            if (_db == null) return 0;
            var entry = _db.Get(type);
            return entry != null ? entry.rank : 0;
        }

        /// <summary>テスト/デバッグ用。db 直接差し替え。</summary>
        public static StatusEffectMetaDatabase CurrentDatabase => _db;

        /// <summary>
        /// 「行動妨害 (進行中スキルキャンセル + 硬直)」を引き起こす状態異常か?
        /// 付与時に SpaceJourneyUnit.OnDisruptApplied 経由で realtime 側 OnDisrupted が呼ばれる。
        /// memory [Disruption spec]: DealDamage前=CT払戻+硬直0、後=CT通常+残アニメ秒硬直
        ///
        /// 対象 (= ActionDisrupt 枠に入っているもの):
        ///   - Stun, Freeze: 完全に動きが止まる
        ///   - Charm: 敵味方ランダム攻撃 (旧 Confusion 挙動を統合) → 一旦キャンセルして再考
        ///   - Silence: スキル使用禁止 → 進行中スキルもキャンセル
        ///
        /// 対象外 (動けるので続行):
        ///   - Taunt: ターゲット指定のみ、進行中攻撃は完了させて次の行動から切替
        ///   - DoT/HoT/Stat バフ・デバフ: 行動には影響しない
        /// </summary>
        public static bool IsDisruptive(StatusEffectType type)
        {
            if (type == StatusEffectType.Taunt) return false;
            if (type == StatusEffectType.Immobilize) return false;

            // ActionDisrupt 枠に属していれば disrupt 対象
            return GetSlot(type) == StatusEffectSlot.ActionDisrupt;
        }
    }
}
