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
using System.Collections;
using System.Collections.Generic;
using SteraCube.SpaceJourney.Realtime;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    public class StatusEffectVfxBinder : MonoBehaviour
    {
        private SpaceJourneyUnit _unit;
        private Animator _animator;
        private StatusEffectMetaDatabase _db;
        private RealtimeBattleManager _manager;
        private DamagePopupSpawner _popupSpawner;
        private const int PermanentStatusThreshold = 9000;
        private const float StealthAlpha = 0.35f;

        // type → 装着中の aura GameObject
        private readonly Dictionary<StatusEffectType, GameObject> _aurasOn = new();
        private readonly HashSet<StatusEffectType> _activeEffectsOn = new();

        // 直前の Animator.speed (pause 解除時に戻す用)
        private float _normalAnimatorSpeed = 1f;
        private bool _isPausedByStatus = false;
        private Coroutine _stealthFadeCoroutine;
        private bool _stealthTransparent;
        private bool _stealthVisualActive;
        private readonly List<RendererMaterialState> _stealthRendererStates = new();

        private sealed class RendererMaterialState
        {
            public Renderer renderer;
            public Material[] originalSharedMaterials;
            public Material[] runtimeMaterials;
            public UnityEngine.Rendering.ShadowCastingMode originalShadowCastingMode;
            public bool originalReceiveShadows;
        }

        public void Bind(SpaceJourneyUnit unit)
        {
            _unit = unit;
            _animator = GetComponentInChildren<Animator>();
            _manager = UnityEngine.Object.FindFirstObjectByType<RealtimeBattleManager>();
            if (_animator != null) _normalAnimatorSpeed = _animator.speed;
        }

        private void LateUpdate()
        {
            if (_unit == null) return;

            // Stealth transparency only depends on SpaceJourneyUnit.activeEffects.
            // Keep it independent from StatusEffectMetaDatabase timing.
            if (_unit.IsDead)
            {
                ClearAllAuras();
                RestoreAnimatorSpeedIfPausedByStatus();
                return;
            }
            SyncStealthTransparency();

            // db の取得は遅延 (MasterDatabase が起動順で後の場合に備え)
            if (_db == null)
            {
                _db = MasterDatabase.Instance != null ? MasterDatabase.Instance.StatusEffectMetaDatabase : null;
                if (_db == null) return;
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
                bool wasActive = _activeEffectsOn.Contains(t);
                bool auraPresent = _aurasOn.TryGetValue(t, out var existingAura) && existingAura != null;
                bool isPermanent = has && IsPermanentStatusEffect(t);

                if (isPermanent)
                {
                    if (t == StatusEffectType.Stealth)
                    {
                        if (!wasActive)
                            _activeEffectsOn.Add(t);
                        continue;
                    }

                    if (wasActive)
                    {
                        _activeEffectsOn.Remove(t);
                        if (existingAura != null) Destroy(existingAura);
                        _aurasOn.Remove(t);
                    }
                    continue;
                }

                if (has && !wasActive)
                {
                    _activeEffectsOn.Add(t);

                    // 付与瞬間: aura 装着 + hit VFX ワンショット
                    if (t == StatusEffectType.Immobilize)
                    {
                        var aura = CreateImmobilizeGroundMark();
                        _aurasOn[t] = aura;
                        _manager?.RegisterRuntimeEffect(aura);
                    }
                    else if (entry.auraPrefab != null)
                    {
                        var aura = Instantiate(entry.auraPrefab, transform);
                        aura.transform.localPosition = Vector3.zero;
                        aura.transform.localRotation = Quaternion.identity;
                        _aurasOn[t] = aura;
                        _manager?.RegisterRuntimeEffect(aura);
                    }
                    if (entry.hitVfxPrefab != null)
                    {
                        var hit = Instantiate(entry.hitVfxPrefab, transform.position, Quaternion.identity);
                        _manager?.RegisterRuntimeEffect(hit);
                        // ワンショット: 5秒後に破棄 (Particle の自動 Destroy 想定でも保険)
                        Destroy(hit, 5f);
                    }
                    SpawnStatusText(entry, t);
                }
                else if (!has && wasActive)
                {
                    _activeEffectsOn.Remove(t);

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
            _activeEffectsOn.Clear();
            _stealthVisualActive = false;
            EndStealthVisual();
        }

        public void ClearBattleVisuals()
        {
            ClearAllAuras();
        }

        private void SpawnStatusText(StatusEffectMetaDatabase.Entry entry, StatusEffectType type)
        {
            if (IsPermanentStatusEffect(type)) return;
            if (entry == null) return;

            string label = entry.popupText;
            if (string.IsNullOrEmpty(label)) return;

            var sp = GetPopupSpawner();
            if (sp == null) return;

            var kind = IsDebuffPopup(type)
                ? DamagePopupSpawner.PopupKind.Debuff
                : DamagePopupSpawner.PopupKind.Buff;
            sp.SpawnText(transform.position, label, kind);
        }

        private bool IsPermanentStatusEffect(StatusEffectType type)
        {
            if (_unit == null) return false;
            if (!_unit.TryGetActiveEffectRemainingTime(type, out int remaining)) return false;
            return remaining >= PermanentStatusThreshold;
        }

        private DamagePopupSpawner GetPopupSpawner()
        {
            if (_popupSpawner != null) return _popupSpawner;

            var manager = UnityEngine.Object.FindFirstObjectByType<RealtimeBattleManager>();
            if (manager == null) return null;

            _popupSpawner = manager.damagePopup;
            if (_popupSpawner == null)
            {
                var go = new GameObject("DamagePopupSpawner(Auto)");
                go.transform.SetParent(manager.transform);
                _popupSpawner = go.AddComponent<DamagePopupSpawner>();
#if UNITY_EDITOR
                _popupSpawner.prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<DamagePopup>(
                    "Assets/0SteraCube/Prefabs/UI/DamagePopup.prefab");
#endif
                manager.damagePopup = _popupSpawner;
            }
            return _popupSpawner;
        }

        private static bool IsDebuffPopup(StatusEffectType type)
        {
            return type == StatusEffectType.DebuffAt
                || type == StatusEffectType.DebuffDf
                || type == StatusEffectType.DebuffAgi
                || type == StatusEffectType.DebuffMat
                || type == StatusEffectType.DebuffMdf
                || type == StatusEffectType.DamageTakenUp
                || type == StatusEffectType.DisableHealing
                || type == StatusEffectType.Immobilize;
        }

        private GameObject CreateImmobilizeGroundMark()
        {
            var root = new GameObject("ImmobilizeGroundMark");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            root.transform.localRotation = Quaternion.identity;

            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(1f, 0.04f, 0.02f, 0.9f);

            var ring = new GameObject("Ring");
            ring.transform.SetParent(root.transform, false);
            var ringLine = ring.AddComponent<LineRenderer>();
            ConfigureGroundLine(ringLine, mat, 0.055f);
            const int segments = 64;
            const float radius = 0.58f;
            ringLine.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                float a = (Mathf.PI * 2f) * i / segments;
                ringLine.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius));
            }

            var slash = new GameObject("Slash");
            slash.transform.SetParent(root.transform, false);
            var slashLine = slash.AddComponent<LineRenderer>();
            ConfigureGroundLine(slashLine, mat, 0.07f);
            slashLine.positionCount = 2;
            slashLine.SetPosition(0, new Vector3(-0.36f, 0.01f, -0.36f));
            slashLine.SetPosition(1, new Vector3(0.36f, 0.01f, 0.36f));

            return root;
        }

        private static void ConfigureGroundLine(LineRenderer line, Material mat, float width)
        {
            line.useWorldSpace = false;
            line.loop = false;
            line.widthMultiplier = width;
            line.numCapVertices = 8;
            line.numCornerVertices = 8;
            line.alignment = LineAlignment.TransformZ;
            line.textureMode = LineTextureMode.Stretch;
            line.material = mat;
            line.startColor = mat.color;
            line.endColor = mat.color;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
        }

        private void BeginStealthVisual()
        {
            if (_stealthFadeCoroutine != null)
            {
                StopCoroutine(_stealthFadeCoroutine);
                _stealthFadeCoroutine = null;
            }
            ApplyStealthTransparency();
        }

        private void SyncStealthTransparency()
        {
            bool hasStealth = _unit != null && _unit.HasActiveEffect(StatusEffectType.Stealth);
            if (hasStealth && !_stealthVisualActive)
            {
                _stealthVisualActive = true;
                BeginStealthVisual();
            }
            else if (!hasStealth && _stealthVisualActive)
            {
                _stealthVisualActive = false;
                EndStealthVisual();
            }
            else if (hasStealth)
            {
                ApplyStealthTransparency();
            }
        }

        private void EndStealthVisual()
        {
            if (_stealthFadeCoroutine != null)
            {
                StopCoroutine(_stealthFadeCoroutine);
                _stealthFadeCoroutine = null;
            }
            RestoreStealthTransparency();
        }

        private void ApplyStealthTransparency()
        {
            if (_unit == null || !_unit.HasActiveEffect(StatusEffectType.Stealth)) return;

            if (_stealthTransparent)
            {
                MaintainTrackedStealthRenderers();
                return;
            }

            var renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null || r is SpriteRenderer || r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer)
                    continue;
                if (IsStealthTransparencyExcluded(r)) continue;
                if (IsStealthRendererTracked(r)) continue;

                var original = r.sharedMaterials;
                if (original == null || original.Length == 0) continue;

                var runtime = new Material[original.Length];
                for (int m = 0; m < original.Length; m++)
                {
                    runtime[m] = CreateStealthRuntimeMaterial(original[m], StealthAlpha);
                }

                _stealthRendererStates.Add(new RendererMaterialState
                {
                    renderer = r,
                    originalSharedMaterials = original,
                    runtimeMaterials = runtime,
                    originalShadowCastingMode = r.shadowCastingMode,
                    originalReceiveShadows = r.receiveShadows,
                });
                r.materials = runtime;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
            }

            _stealthTransparent = _stealthRendererStates.Count > 0;
        }

        private void MaintainTrackedStealthRenderers()
        {
            for (int i = 0; i < _stealthRendererStates.Count; i++)
            {
                var st = _stealthRendererStates[i];
                if (st == null || st.renderer == null) continue;
                if (!RendererUsesRuntimeMaterials(st.renderer, st.runtimeMaterials))
                    st.renderer.materials = st.runtimeMaterials;
                st.renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                st.renderer.receiveShadows = false;
            }
        }

        private static bool IsStealthTransparencyExcluded(Renderer renderer)
        {
            if (renderer == null) return true;

            Transform t = renderer.transform;
            while (t != null)
            {
                if (t.GetComponent<Realtime.Projectile>() != null
                    || t.GetComponent<Realtime.AttachmentCurveDriver>() != null
                    || t.GetComponent<Realtime.AttachmentRotator>() != null)
                    return true;

                string n = t.name;
                if (!string.IsNullOrEmpty(n)
                    && (n.Contains("NockArrow")
                        || n.Contains("Arrow_Projectile")
                        || n.Contains("Bow_Arrow")))
                    return true;

                t = t.parent;
            }

            return false;
        }

        private bool IsStealthRendererTracked(Renderer renderer)
        {
            for (int i = 0; i < _stealthRendererStates.Count; i++)
            {
                var st = _stealthRendererStates[i];
                if (st != null && st.renderer == renderer)
                {
                    if (!RendererUsesRuntimeMaterials(renderer, st.runtimeMaterials))
                        renderer.materials = st.runtimeMaterials;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    return true;
                }
            }
            return false;
        }

        private static bool RendererUsesRuntimeMaterials(Renderer renderer, Material[] runtimeMaterials)
        {
            if (renderer == null || runtimeMaterials == null || runtimeMaterials.Length == 0) return false;

            var current = renderer.sharedMaterials;
            if (current == null || current.Length != runtimeMaterials.Length) return false;

            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] != runtimeMaterials[i])
                    return false;
            }
            return true;
        }

        private static void ConfigureTransparentMaterial(Material mat, float alpha)
        {
            if (mat == null) return;

            Color c = Color.white;
            if (mat.HasProperty("_BaseColor")) c = mat.GetColor("_BaseColor");
            else if (mat.HasProperty("_Color")) c = mat.GetColor("_Color");
            c.a = alpha;

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            if (mat.HasProperty("_Alpha")) mat.SetFloat("_Alpha", alpha);

            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_SurfaceType")) mat.SetFloat("_SurfaceType", 1f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
            if (mat.HasProperty("_BlendMode")) mat.SetFloat("_BlendMode", 0f);
            if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_AlphaSrcBlend")) mat.SetFloat("_AlphaSrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (mat.HasProperty("_AlphaDstBlend")) mat.SetFloat("_AlphaDstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
            if (mat.HasProperty("_AlphaCutoffEnable")) mat.SetFloat("_AlphaCutoffEnable", 0f);
            if (mat.HasProperty("_UseOutline")) mat.SetFloat("_UseOutline", 0f);

            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.EnableKeyword("_BLENDMODE_ALPHA");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("USE_OUTLINE");
            mat.renderQueue = 3000;
        }

        private static Material CreateStealthRuntimeMaterial(Material original, float alpha)
        {
            if (original == null) return null;
            var mat = new Material(original);
            ConfigureTransparentMaterial(mat, alpha);
            return mat;
        }

        private void RestoreStealthTransparency()
        {
            for (int i = 0; i < _stealthRendererStates.Count; i++)
            {
                var st = _stealthRendererStates[i];
                if (st == null) continue;
                if (st.renderer != null && st.originalSharedMaterials != null)
                {
                    st.renderer.sharedMaterials = st.originalSharedMaterials;
                    st.renderer.shadowCastingMode = st.originalShadowCastingMode;
                    st.renderer.receiveShadows = st.originalReceiveShadows;
                }

                if (st.runtimeMaterials == null) continue;
                for (int m = 0; m < st.runtimeMaterials.Length; m++)
                {
                    if (st.runtimeMaterials[m] != null)
                        Destroy(st.runtimeMaterials[m]);
                }
            }

            _stealthRendererStates.Clear();
            _stealthTransparent = false;
        }

        private void OnDestroy()
        {
            ClearAllAuras();
        }
    }
}
