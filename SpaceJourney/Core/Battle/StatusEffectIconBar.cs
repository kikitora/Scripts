// StatusEffectIconBar.cs
// Shows active status effect icons above a realtime battle unit.

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    public class StatusEffectIconBar : MonoBehaviour
    {
        [SerializeField] private float iconWorldSize = 0.28f;
        [SerializeField] private float iconSpacing = 0.32f;
        [SerializeField] private float rowSpacing = 0.34f;
        [SerializeField] private float verticalPadding = 0.24f;
        [SerializeField] private float refreshInterval = 0.12f;
        [SerializeField] private int sortingOrder = 200;

        private readonly List<StatusEffectType> _active = new();
        private readonly List<StatusEffectType> _buffs = new();
        private readonly List<StatusEffectType> _bad = new();
        private readonly List<SpriteRenderer> _renderers = new();

        private SpaceJourneyUnit _unit;
        private StatusEffectMetaDatabase _db;
        private Transform _iconRoot;
        private Camera _cam;
        private float _nextRefreshTime;
        private string _lastSignature;
        private Vector3 _cachedLocalAnchor = new Vector3(0f, 1.8f, 0f);

        public void Bind(SpaceJourneyUnit unit)
        {
            _unit = unit;
            _db = MasterDatabase.Instance != null ? MasterDatabase.Instance.StatusEffectMetaDatabase : null;
            _cam = Camera.main;
            EnsureRoot();
            CacheModelAnchor();
            ForceRefresh();
        }

        public void Configure(float worldSize, float spacing, float rowGap, float padding)
        {
            iconWorldSize = Mathf.Max(0.01f, worldSize);
            iconSpacing = Mathf.Max(0.01f, spacing);
            rowSpacing = Mathf.Max(0.01f, rowGap);
            verticalPadding = padding;
            ForceRefresh();
        }

        private void LateUpdate()
        {
            if (_unit == null)
            {
                SetVisible(false);
                return;
            }

            if (_db == null)
                _db = MasterDatabase.Instance != null ? MasterDatabase.Instance.StatusEffectMetaDatabase : null;

            if (_cam == null) _cam = Camera.main;
            EnsureRoot();

            _iconRoot.position = GetAnchorPosition();
            if (_cam != null)
                _iconRoot.rotation = _cam.transform.rotation;

            if (_unit.IsDead || _db == null)
            {
                SetVisible(false);
                return;
            }

            if (Time.time >= _nextRefreshTime)
            {
                _nextRefreshTime = Time.time + Mathf.Max(0.02f, refreshInterval);
                RefreshIfChanged();
            }
        }

        private void ForceRefresh()
        {
            _lastSignature = null;
            _nextRefreshTime = 0f;
            RefreshIfChanged();
        }

        private void RefreshIfChanged()
        {
            if (_unit == null || _db == null) return;

            _unit.GetActiveStatusEffectTypes(_active);
            BuildRows();

            string signature = BuildSignature();
            if (signature == _lastSignature) return;
            _lastSignature = signature;

            RebuildIcons();
        }

        private void BuildRows()
        {
            _buffs.Clear();
            _bad.Clear();

            for (int i = 0; i < _active.Count; i++)
            {
                StatusEffectType type = _active[i];
                var entry = _db.Get(type);
                if (entry == null || entry.icon == null) continue;

                var category = StatusEffectMetaDatabase.CategorizeByType(type);
                if (category == StatusEffectCategoryGroup.Buff)
                    _buffs.Add(type);
                else
                    _bad.Add(type);
            }
        }

        private string BuildSignature()
        {
            if (_buffs.Count == 0 && _bad.Count == 0) return string.Empty;

            var sig = new StringBuilder();
            for (int i = 0; i < _buffs.Count; i++)
            {
                sig.Append((int)_buffs[i]);
                sig.Append(',');
            }
            sig.Append('|');
            for (int i = 0; i < _bad.Count; i++)
            {
                sig.Append((int)_bad[i]);
                sig.Append(',');
            }
            return sig.ToString();
        }

        private void RebuildIcons()
        {
            ClearIcons();

            int needed = _buffs.Count + _bad.Count;
            if (needed == 0)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            AddRow(_buffs, rowSpacing * 0.5f);
            AddRow(_bad, -rowSpacing * 0.5f);
        }

        private void AddRow(List<StatusEffectType> row, float localY)
        {
            if (row.Count == 0) return;
            float startX = -((row.Count - 1) * iconSpacing) * 0.5f;

            for (int i = 0; i < row.Count; i++)
            {
                var entry = _db.Get(row[i]);
                if (entry == null || entry.icon == null) continue;

                var go = new GameObject($"StatusIcon_{row[i]}");
                go.transform.SetParent(_iconRoot, false);
                go.transform.localPosition = new Vector3(startX + i * iconSpacing, localY, 0f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = entry.icon;
                sr.sortingOrder = sortingOrder;

                Vector2 spriteSize = entry.icon.bounds.size;
                float baseSize = Mathf.Max(0.01f, Mathf.Max(spriteSize.x, spriteSize.y));
                float scale = iconWorldSize / baseSize;
                go.transform.localScale = Vector3.one * scale;

                _renderers.Add(sr);
            }
        }

        private void EnsureRoot()
        {
            if (_iconRoot != null) return;
            var root = new GameObject("StatusEffectIconBarRoot");
            _iconRoot = root.transform;
            _iconRoot.SetParent(transform, false);
            _iconRoot.localPosition = Vector3.zero;
        }

        private Vector3 GetAnchorPosition()
        {
            return transform.TransformPoint(_cachedLocalAnchor + Vector3.up * verticalPadding);
        }

        private void CacheModelAnchor()
        {
            Bounds bounds = default;
            bool hasBounds = false;
            var renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null || r is SpriteRenderer) continue;
                if (_iconRoot != null && r.transform.IsChildOf(_iconRoot)) continue;
                if (!hasBounds)
                {
                    bounds = r.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }
            }

            if (hasBounds)
            {
                var worldAnchor = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
                _cachedLocalAnchor = transform.InverseTransformPoint(worldAnchor);
            }
        }

        private void SetVisible(bool visible)
        {
            if (_iconRoot != null && _iconRoot.gameObject.activeSelf != visible)
                _iconRoot.gameObject.SetActive(visible);
        }

        private void ClearIcons()
        {
            if (_iconRoot == null) return;
            for (int i = _iconRoot.childCount - 1; i >= 0; i--)
                Destroy(_iconRoot.GetChild(i).gameObject);
            _renderers.Clear();
        }

        public void ClearBattleVisuals()
        {
            ClearIcons();
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_iconRoot != null)
                Destroy(_iconRoot.gameObject);
        }
    }
}
