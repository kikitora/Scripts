// StatusEffectVfxBinder.cs
// SpaceJourneyUnit に紐づいた 3D モデル GameObject にアタッチして、
// status effect の付与/解除に応じて aura prefab を装着・破棄し、Animator.speed を制御する。
//
// 設計: ポーリング型 (毎フレーム HasActiveEffect 走査)。
//   - 「has=true & aura 未装着」を検出 → aura 装着 + hit VFX をワンショット再生
//   - 「has=false & aura 装着済」を検出 → aura 破棄
//   - Hardcc 系の pauseMode に応じて animator.speed を調整
//
// 装着箇所: RealtimeBattleStarter / BattleUnitSpawner で 3D モデル生成直後に
//           AddComponent<StatusEffectVfxBinder>().Bind(unit) を呼ぶ。

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    public class StatusEffectVfxBinder : MonoBehaviour
    {
        private SpaceJourneyUnit _unit;
        private Animator _animator;
        private StatusEffectMetaDatabase _db;

        // type → 装着中の aura GameObject
        private readonly Dictionary<StatusEffectType, GameObject> _aurasOn = new();

        // 直前の Animator.speed (pause 解除時に戻す用)
        private float _normalAnimatorSpeed = 1f;
        private bool _isPausedByStatus = false;

        public void Bind(SpaceJourneyUnit unit)
        {
            _unit = unit;
            _animator = GetComponentInChildren<Animator>();
            if (_animator != null) _normalAnimatorSpeed = _animator.speed;
        }

        private void LateUpdate()
        {
            if (_unit == null) return;

            // db の取得は遅延 (MasterDatabase が起動順で後の場合に備え)
            if (_db == null)
            {
                _db = MasterDatabase.Instance != null ? MasterDatabase.Instance.StatusEffectMetaDatabase : null;
                if (_db == null) return;
            }

            // 死亡時は aura を全部消す (3D モデルが消滅するまでに時間がある可能性)
            if (_unit.IsDead)
            {
                ClearAllAuras();
                RestoreAnimatorSpeedIfPausedByStatus();
                return;
            }

            bool anyPause = false;
            AnimatorPauseMode strongestPause = AnimatorPauseMode.None;

            // 全 enum を走査して状態を反映
            // (39 enum × ~10 unit = ~400 回/フレーム。問題なし)
            foreach (StatusEffectType t in Enum.GetValues(typeof(StatusEffectType)))
            {
                if (t == StatusEffectType.None) continue;
                var entry = _db.Get(t);
                if (entry == null) continue;

                bool has = _unit.HasActiveEffect(t);
                bool auraPresent = _aurasOn.TryGetValue(t, out var existingAura) && existingAura != null;

                if (has && !auraPresent)
                {
                    // 付与瞬間: aura 装着 + hit VFX ワンショット
                    if (entry.auraPrefab != null)
                    {
                        var aura = Instantiate(entry.auraPrefab, transform);
                        aura.transform.localPosition = Vector3.zero;
                        aura.transform.localRotation = Quaternion.identity;
                        _aurasOn[t] = aura;
                    }
                    if (entry.hitVfxPrefab != null)
                    {
                        var hit = Instantiate(entry.hitVfxPrefab, transform.position, Quaternion.identity);
                        // ワンショット: 5秒後に破棄 (Particle の自動 Destroy 想定でも保険)
                        Destroy(hit, 5f);
                    }
                }
                else if (!has && auraPresent)
                {
                    // 解除瞬間: aura 破棄
                    if (existingAura != null) Destroy(existingAura);
                    _aurasOn.Remove(t);
                }

                // Animator pause 集約
                if (has && entry.pauseMode != AnimatorPauseMode.None)
                {
                    anyPause = true;
                    if ((int)entry.pauseMode > (int)strongestPause)
                        strongestPause = entry.pauseMode;
                }
            }

            // Animator.speed 制御
            if (_animator != null)
            {
                if (anyPause)
                {
                    if (!_isPausedByStatus)
                    {
                        _normalAnimatorSpeed = _animator.speed;  // 元 speed を覚える
                        _isPausedByStatus = true;
                    }
                    float targetSpeed = strongestPause == AnimatorPauseMode.FullStop ? 0f : 0.3f;
                    if (!Mathf.Approximately(_animator.speed, targetSpeed))
                        _animator.speed = targetSpeed;
                }
                else if (_isPausedByStatus)
                {
                    _animator.speed = _normalAnimatorSpeed;
                    _isPausedByStatus = false;
                }
            }
        }

        private void RestoreAnimatorSpeedIfPausedByStatus()
        {
            if (_animator != null && _isPausedByStatus)
            {
                _animator.speed = _normalAnimatorSpeed;
                _isPausedByStatus = false;
            }
        }

        private void ClearAllAuras()
        {
            foreach (var kv in _aurasOn)
            {
                if (kv.Value != null) Destroy(kv.Value);
            }
            _aurasOn.Clear();
        }

        private void OnDestroy()
        {
            ClearAllAuras();
        }
    }
}
