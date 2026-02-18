using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// キューブの「移動」「向き(direction=Yaw)」「上面(rotation=Pitch/Roll)」をDOTweenで制御する。
    /// さらに、戦闘配置のために 6サイドぶんの配置ルート（UpperSideNum順）を保持し、
    /// キャラクター配置対象の4サイド(up/right/dawn/left)のみを回転（0..3）で切り替えられるようにする。
    ///
    /// 追加：
    /// - グラウンド（床/土台）を生成してぶら下げる親Transform（groundRoot）を保持する。
    ///
    /// 戦闘配置仕様（ユーザー指定）：
    /// - UpperSideNum の順番：up(0), right(1), dawn(2), left(3), flont(4), back(5)
    /// - キャラ配置は 0..3 のみ（up/right/dawn/left）
    /// - キャラ配置の回転状態は 0..3（上向いた上で、左回転順）を右回転/左回転で変更
    /// - front/back(4,5) は建物配置に使う
    /// - 各サイドルートは (0,0) が中央になるよう調整済み。(-3,-3)〜(3,3) の local に置けば良い
    /// </summary>
    public class CubeUnit : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        // Hierarchy Targets
        // ─────────────────────────────────────────────────────────────

        [Header("Hierarchy Targets")]
        [SerializeField] private Transform rootPosition;   // RootPosition：ワールド移動する親
        [SerializeField] private Transform directionRoot;  // direction：向き（Yaw）を回す親
        [SerializeField] private Transform rotateRoot;     // rotation：上面（Pitch/Roll）を回す親
        [SerializeField] private Transform floatVisual;    // FloatVisual：上下/揺れ演出（矢印も上下させたい階層）

        // ─────────────────────────────────────────────────────────────
        // Ground Root
        // ─────────────────────────────────────────────────────────────

        [Header("Ground Root")]
        [Tooltip("このキューブのグラウンド（床/土台）を生成してぶら下げる親。未設定なら this.transform を使う。")]
        [SerializeField] private Transform groundRoot;

        // ─────────────────────────────────────────────────────────────
        // Battle Side Roots (UpperSideNum order)
        // ─────────────────────────────────────────────────────────────

        [Header("Side Roots (UpperSideNum order)")]
        [Tooltip("UpperSideNum の順番に合わせて 6面ぶん登録。\nup(0), right(1), dawn(2), left(3), flont(4), back(5)")]
        [SerializeField] private Transform[] sideRoots = new Transform[6];

        [Header("Character Side Rotation (0..3 only)")]
        [Tooltip("キャラ配置対象の4サイド(up/right/dawn/left)の回転状態。0=初期、1=左回転1回…")]
        [SerializeField, Range(0, 3)] private int characterSideRotation = 0;

        // ─────────────────────────────────────────────────────────────
        // Public getters
        // ─────────────────────────────────────────────────────────────

        public bool IsBusy => _isBusy;
        public Transform FloatVisual => floatVisual;

        /// <summary>現在の向き（キューブのYaw向き）。</summary>
        public Dir FacingDir => _facingDir;

        /// <summary>キャラ配置側の回転（0..3）</summary>
        public int CharacterSideRotation => characterSideRotation;

        /// <summary>グラウンドの親（未設定なら this.transform）</summary>
        public Transform GroundRoot => groundRoot != null ? groundRoot : transform;

        // ─────────────────────────────────────────────────────────────
        // Runtime state
        // ─────────────────────────────────────────────────────────────

        private bool _isBusy;
        private bool _inBattle;
        private int _adjacentCount;

        private Tween _twAction;     // 1動作ずつの本体Tween
        private Tween _twIdlePos;    // 常時浮遊（位置）
        private Tween _twIdleRot;    // 常時浮遊（傾き：任意）
        private Quaternion _floatRestRot;

        [SerializeField] private Dir _facingDir = Dir.North;

        // ─────────────────────────────────────────────────────────────
        // Unity
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (floatVisual != null) _floatRestRot = floatVisual.localRotation;

            // シーン上の初期回転から facing を推定（directionRootが無ければNorth）
            _facingDir = GuessFacingDirFromDirectionRoot();
        }

        private void OnEnable()
        {
            RefreshIdleFloat(forceY0: true);
        }

        private void OnDisable()
        {
            KillAllTweens(resetY0: true);
        }

        // ─────────────────────────────────────────────────────────────
        // Ground attach helpers
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 生成済みのグラウンド(床)オブジェクトを、このキューブの groundRoot の子にする。
        /// resetLocal=true の場合は local pos/rot/scale を初期化する。
        /// </summary>
        public void AttachGround(GameObject groundInstance, bool resetLocal = true)
        {
            if (groundInstance == null) return;

            var t = groundInstance.transform;
            t.SetParent(GroundRoot, worldPositionStays: false);

            if (resetLocal)
            {
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Facing (Dir)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// ロード/生成直後用（演出なし即時反映）。
        /// </summary>
        public void SetFacingDirImmediate(Dir dir)
        {
            _facingDir = dir;

            var t = directionRoot != null ? directionRoot : transform;
            Vector3 e = t.localEulerAngles;
            e.y = DirToYaw(dir);
            t.localEulerAngles = e;
        }

        // ─────────────────────────────────────────────────────────────
        // Adjacent notify (IdleFloat OFF)
        // ─────────────────────────────────────────────────────────────

        public void NotifyAdjacentEnter()
        {
            _adjacentCount++;
            RefreshIdleFloat(forceY0: true);
        }

        public void NotifyAdjacentExit()
        {
            _adjacentCount = Mathf.Max(0, _adjacentCount - 1);
            RefreshIdleFloat(forceY0: true);
        }

        private bool IsAdjacent() => _adjacentCount > 0;

        // ─────────────────────────────────────────────────────────────
        // Input API (start if possible)
        // ─────────────────────────────────────────────────────────────

        public bool TryMove(Vector3 targetWorldPos)
        {
            if (_inBattle || _isBusy) return false;
            StartCoroutine(CoMove(targetWorldPos));
            return true;
        }

        public bool TryTurnRight()
        {
            if (_inBattle || _isBusy) return false;
            StartCoroutine(CoTurnRight());
            return true;
        }

        public bool TryTurnLeft()
        {
            if (_inBattle || _isBusy) return false;
            StartCoroutine(CoTurnLeft());
            return true;
        }

        public bool TryRotateTop(Vector3 deltaEuler)
        {
            if (_inBattle || _isBusy) return false;
            StartCoroutine(CoRotateTop(deltaEuler));
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // Composite movement API
        // ─────────────────────────────────────────────────────────────

        public enum MoveFacingMode
        {
            FacingFixed_MoveOnly = 0,        // 向き固定（回転しない）
            RotateToFacing_ThenMove = 1,     // 指定向きへ回転 → 移動
        }

        public bool TryMoveFacingFixed(Vector3 targetWorldPos)
        {
            if (_inBattle || _isBusy) return false;
            StartCoroutine(CoMoveFacingFixed(targetWorldPos));
            return true;
        }

        public bool TryRotateToFacingThenMove(Vector3 targetWorldPos, Dir desiredFacingDir)
        {
            if (_inBattle || _isBusy) return false;
            StartCoroutine(CoRotateToFacingThenMove(targetWorldPos, desiredFacingDir));
            return true;
        }

        public bool TryMoveWithFacingMode(Vector3 targetWorldPos, MoveFacingMode mode, Dir desiredFacingDirIfNeeded)
        {
            if (_inBattle || _isBusy) return false;

            switch (mode)
            {
                case MoveFacingMode.FacingFixed_MoveOnly:
                    StartCoroutine(CoMoveFacingFixed(targetWorldPos));
                    return true;

                case MoveFacingMode.RotateToFacing_ThenMove:
                    StartCoroutine(CoRotateToFacingThenMove(targetWorldPos, desiredFacingDirIfNeeded));
                    return true;

                default:
                    return false;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Coroutines (wait for completion)
        // ─────────────────────────────────────────────────────────────

        public IEnumerator CoMove(Vector3 targetWorldPos)
        {
            if (rootPosition == null) yield break;

            yield return CoRunExclusive(() =>
            {
                float dur = SpaceJourneyConstants.CubeMoveAnimDuration;

                Sequence seq = DOTween.Sequence();
                seq.Append(rootPosition.DOMove(targetWorldPos, dur).SetEase(Ease.InOutQuad));

                // 移動中だけの浮遊感（隣接中でもOK）
                if (floatVisual != null && SpaceJourneyConstants.EnableMoveFloat)
                {
                    Vector3 startPos = floatVisual.localPosition;
                    Quaternion startRot = floatVisual.localRotation;

                    seq.Join(
                        floatVisual.DOLocalMoveY(startPos.y + SpaceJourneyConstants.MoveAmplitude, dur * 0.5f)
                                   .SetEase(Ease.InOutSine)
                                   .SetLoops(2, LoopType.Yoyo)
                    );

                    float a = SpaceJourneyConstants.MoveSwayAngle;
                    if (Mathf.Abs(a) > 0.0001f)
                    {
                        seq.Join(
                            floatVisual.DOLocalRotateQuaternion(startRot * Quaternion.Euler(a, 0f, -a * 0.6f), dur * 0.5f)
                                       .SetEase(Ease.InOutSine)
                                       .SetLoops(2, LoopType.Yoyo)
                        );
                    }

                    seq.OnComplete(() =>
                    {
                        if (floatVisual == null) return;

                        if (_inBattle || IsAdjacent())
                        {
                            Vector3 p = floatVisual.localPosition;
                            p.y = 0f;
                            floatVisual.localPosition = p;
                            floatVisual.localRotation = _floatRestRot;
                        }
                        else
                        {
                            floatVisual.localPosition = startPos;
                            floatVisual.localRotation = startRot;
                        }
                    });
                }

                return seq;
            });
        }

        public IEnumerator CoMoveFacingFixed(Vector3 targetWorldPos)
        {
            yield return CoMove(targetWorldPos);
        }

        public IEnumerator CoRotateToFacingThenMove(Vector3 targetWorldPos, Dir desiredFacingDir)
        {
            yield return CoRotateToFacingDir(desiredFacingDir);
            yield return CoMove(targetWorldPos);
        }

        public IEnumerator CoTurnRight()
        {
            yield return CoRotateDirectionYaw(+90f);
            _facingDir = TurnRight(_facingDir);
        }

        public IEnumerator CoTurnLeft()
        {
            yield return CoRotateDirectionYaw(-90f);
            _facingDir = TurnLeft(_facingDir);
        }

        public IEnumerator CoRotateToFacingDir(Dir desiredFacingDir)
        {
            if (directionRoot == null) yield break;
            if (desiredFacingDir == _facingDir) yield break;

            // 差分を -90/ +90/ +180 のどれにするか（最短）
            int cur = DirToIndex(_facingDir);
            int dst = DirToIndex(desiredFacingDir);
            int delta = (dst - cur + 4) % 4;

            if (delta == 2)
            {
                yield return CoRotateDirectionYaw(+180f);
                _facingDir = desiredFacingDir;
                yield break;
            }
            if (delta == 1)
            {
                yield return CoRotateDirectionYaw(+90f);
                _facingDir = desiredFacingDir;
                yield break;
            }

            // delta == 3
            yield return CoRotateDirectionYaw(-90f);
            _facingDir = desiredFacingDir;
        }

        private IEnumerator CoRotateDirectionYaw(float deltaDeg)
        {
            if (directionRoot == null) yield break;

            yield return CoRunExclusive(() =>
                directionRoot.DOLocalRotate(
                        new Vector3(0f, deltaDeg, 0f),
                        SpaceJourneyConstants.CubeDirChangeAnimDuration,
                        RotateMode.LocalAxisAdd
                    )
                    .SetEase(Ease.InOutSine)
            );
        }

        private IEnumerator CoRotateTop(Vector3 deltaEuler)
        {
            if (rotateRoot == null) yield break;

            yield return CoRunExclusive(() =>
                rotateRoot.DOLocalRotate(
                        deltaEuler,
                        SpaceJourneyConstants.CubeRotateAnimDuration,
                        RotateMode.LocalAxisAdd
                    )
                    .SetEase(Ease.InOutSine)
            );
        }

        // ─────────────────────────────────────────────────────────────
        // Exclusive run (one action at a time)
        // ─────────────────────────────────────────────────────────────

        private IEnumerator CoRunExclusive(System.Func<Tween> build)
        {
            if (_isBusy) yield break;
            _isBusy = true;

            try
            {
                StopIdleFloat(resetY0: false);

                _twAction?.Kill();
                _twAction = build?.Invoke();

                if (_twAction != null)
                    yield return _twAction.WaitForCompletion();
            }
            finally
            {
                _twAction = null;
                _isBusy = false;
                RefreshIdleFloat(forceY0: true);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // IdleFloat
        // ─────────────────────────────────────────────────────────────

        private void RefreshIdleFloat(bool forceY0)
        {
            if (floatVisual == null) return;

            if (_inBattle || IsAdjacent() || !SpaceJourneyConstants.EnableIdleFloat)
            {
                StopIdleFloat(resetY0: forceY0);
                return;
            }

            StartIdleFloatRandomPhase();
        }

        private void StartIdleFloatRandomPhase()
        {
            StopIdleFloat(resetY0: true);

            Vector3 basePos = floatVisual.localPosition;
            basePos.y = 0f;
            floatVisual.localPosition = basePos;
            floatVisual.localRotation = _floatRestRot;

            float amp = SpaceJourneyConstants.IdleAmplitude;
            float period = Mathf.Max(0.01f, SpaceJourneyConstants.IdlePeriod);
            float half = period * 0.5f;

            Sequence posSeq = DOTween.Sequence();
            posSeq.Append(floatVisual.DOLocalMoveY(basePos.y + amp, half).SetEase(Ease.InOutSine));
            posSeq.Append(floatVisual.DOLocalMoveY(basePos.y, half).SetEase(Ease.InOutSine));
            posSeq.SetLoops(-1, LoopType.Restart);

            float t0 = Random.Range(0f, period);
            posSeq.Goto(t0, andPlay: true);
            _twIdlePos = posSeq;

            if (SpaceJourneyConstants.EnableIdleSway && Mathf.Abs(SpaceJourneyConstants.IdleSwayAngle) > 0.0001f)
            {
                float a = SpaceJourneyConstants.IdleSwayAngle;
                Quaternion target = _floatRestRot * Quaternion.Euler(a, 0f, -a * 0.6f);

                Sequence rotSeq = DOTween.Sequence();
                rotSeq.Append(floatVisual.DOLocalRotateQuaternion(target, half).SetEase(Ease.InOutSine));
                rotSeq.Append(floatVisual.DOLocalRotateQuaternion(_floatRestRot, half).SetEase(Ease.InOutSine));
                rotSeq.SetLoops(-1, LoopType.Restart);

                rotSeq.Goto(t0, andPlay: true);
                _twIdleRot = rotSeq;
            }
        }

        private void StopIdleFloat(bool resetY0)
        {
            _twIdlePos?.Kill();
            _twIdleRot?.Kill();
            _twIdlePos = null;
            _twIdleRot = null;

            if (floatVisual == null) return;

            if (resetY0)
            {
                Vector3 p = floatVisual.localPosition;
                p.y = 0f;
                floatVisual.localPosition = p;
            }
            floatVisual.localRotation = _floatRestRot;
        }

        private void KillAllTweens(bool resetY0)
        {
            _twAction?.Kill();
            _twIdlePos?.Kill();
            _twIdleRot?.Kill();

            _twAction = null;
            _twIdlePos = null;
            _twIdleRot = null;

            if (floatVisual != null)
            {
                if (resetY0)
                {
                    Vector3 p = floatVisual.localPosition;
                    p.y = 0f;
                    floatVisual.localPosition = p;
                }
                floatVisual.localRotation = _floatRestRot;
            }

            _isBusy = false;
        }

        // ─────────────────────────────────────────────────────────────
        // Battle placement: Side roots + rotating character sides (0..3)
        // ─────────────────────────────────────────────────────────────

        public Transform GetSideRoot(UpperSideNum side)
        {
            if (sideRoots == null) return null;
            int i = (int)side;
            if (i < 0 || i >= sideRoots.Length) return null;
            return sideRoots[i];
        }

        public Transform GetFrontBuildingRoot() => GetSideRoot(UpperSideNum.flont);
        public Transform GetBackBuildingRoot() => GetSideRoot(UpperSideNum.back);

        public void RotateCharacterSidesLeft()
        {
            characterSideRotation = (characterSideRotation + 1) & 3;
        }

        public void RotateCharacterSidesRight()
        {
            characterSideRotation = (characterSideRotation + 3) & 3;
        }

        public void SetCharacterSideRotation(int rot)
        {
            characterSideRotation = ((rot % 4) + 4) % 4;
        }

        public UpperSideNum ResolveCharacterSide(int logicalIndex0to3)
        {
            int idx = ((logicalIndex0to3 % 4) + 4) % 4;
            int rotated = (idx + characterSideRotation) & 3;
            return (UpperSideNum)rotated; // up/right/dawn/left のみ
        }

        public bool TryPlaceCharacterOnRotatingSide(Transform unitTransform, int logicalIndex0to3, Vector2Int localCell)
        {
            if (unitTransform == null) return false;

            if (!IsCellInRange(localCell))
            {
                Debug.LogWarning($"[CubeUnit] localCell out of range (-3..3): cell={localCell}");
                return false;
            }

            UpperSideNum side = ResolveCharacterSide(logicalIndex0to3);
            Transform root = GetSideRoot(side);
            if (root == null)
            {
                Debug.LogWarning($"[CubeUnit] sideRoot is null: side={side}");
                return false;
            }

            unitTransform.SetParent(root, worldPositionStays: false);
            unitTransform.localPosition = new Vector3(localCell.x, 0f, localCell.y);
            unitTransform.localRotation = Quaternion.identity;
            unitTransform.localScale = Vector3.one;
            return true;
        }

        public bool TryPlaceBuilding(Transform buildingTransform, UpperSideNum sideFrontOrBack, Vector3 localPos)
        {
            if (buildingTransform == null) return false;
            if (sideFrontOrBack != UpperSideNum.flont && sideFrontOrBack != UpperSideNum.back)
            {
                Debug.LogWarning($"[CubeUnit] TryPlaceBuilding side must be flont/back. side={sideFrontOrBack}");
                return false;
            }

            Transform root = GetSideRoot(sideFrontOrBack);
            if (root == null)
            {
                Debug.LogWarning($"[CubeUnit] building root is null: side={sideFrontOrBack}");
                return false;
            }

            buildingTransform.SetParent(root, worldPositionStays: false);
            buildingTransform.localPosition = localPos;
            buildingTransform.localRotation = Quaternion.identity;
            buildingTransform.localScale = Vector3.one;
            return true;
        }

        private static bool IsCellInRange(Vector2Int cell)
        {
            return cell.x >= -3 && cell.x <= 3 && cell.y >= -3 && cell.y <= 3;
        }

        // ─────────────────────────────────────────────────────────────
        // Utility (Dir)
        // ─────────────────────────────────────────────────────────────

        private static float DirToYaw(Dir dir)
        {
            return dir switch
            {
                Dir.North => 0f,
                Dir.East => 90f,
                Dir.South => 180f,
                Dir.West => 270f,
                _ => 0f
            };
        }

        private static int DirToIndex(Dir dir)
        {
            return dir switch
            {
                Dir.North => 0,
                Dir.East => 1,
                Dir.South => 2,
                Dir.West => 3,
                _ => 0
            };
        }

        private static Dir TurnRight(Dir dir)
        {
            return dir switch
            {
                Dir.North => Dir.East,
                Dir.East => Dir.South,
                Dir.South => Dir.West,
                Dir.West => Dir.North,
                _ => Dir.North
            };
        }

        private static Dir TurnLeft(Dir dir)
        {
            return dir switch
            {
                Dir.North => Dir.West,
                Dir.West => Dir.South,
                Dir.South => Dir.East,
                Dir.East => Dir.North,
                _ => Dir.North
            };
        }

        private Dir GuessFacingDirFromDirectionRoot()
        {
            if (directionRoot == null) return _facingDir;

            float y = directionRoot.localEulerAngles.y;
            int idx = Mathf.RoundToInt(y / 90f) % 4;
            if (idx < 0) idx += 4;

            return idx switch
            {
                0 => Dir.North,
                1 => Dir.East,
                2 => Dir.South,
                3 => Dir.West,
                _ => Dir.North
            };
        }
    }
}
