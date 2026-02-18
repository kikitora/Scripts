using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SteraCube.SpaceJourney;
using UnityEngine;

public class TestCharaMake : MonoBehaviour
{
    // このクラスで何をするか：
    // SoulInstance をランダム生成し、
    // SoulInstance / BodyInstance のフィールド/プロパティをできるだけ全部 Debug.Log にダンプするテスト用クラス。

    [Header("ボディ生成用テストID")]
    [SerializeField] private string testRaceId;
    [SerializeField] private string testBodyJobId;

    void Start()
    {
    }

    void Update()
    {
    }

    public void TesyCharaMakeBtn()
    {
        // 1) ソウル生成
        var soul = SoulInstance.CreateRandomInitialSoul(rank: 2, growthType: GrowthType.Normal);

        // 2) ボディ生成（raceId / bodyJobId が設定されている場合のみ）
        BodyInstance body = null;

        if (MasterDatabase.Instance == null)
        {
            Debug.LogWarning("[TestCharaMake] MasterDatabase.Instance が null のため、ボディは生成されません。");
        }
        else if (!string.IsNullOrEmpty(testRaceId) && !string.IsNullOrEmpty(testBodyJobId))
        {
            try
            {
                body = BodyFactory.CreateRandom(
                    raceId: testRaceId,
                    bodyJobId: testBodyJobId,
                    rank: soul.Rank);
            }
            catch (Exception e)
            {
                Debug.LogError($"[TestCharaMake] BodyFactory.CreateRandom で例外発生: {e.GetType().Name}: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("[TestCharaMake] testRaceId / testBodyJobId が未設定のため、ボディは生成されません。");
        }

        // 2-1) ボディが生成できていれば、ソウルに装備として紐づける
        if (body != null)
        {
            soul.EquipBody(body);
        }

        // 3) スナップショット（ソウル＋ボディ最終ステ）を取得
        var snapshot = soul.GetFinalSnapshot(body);

        // 4) ログ出力
        var sb = new StringBuilder();

        // 4-1) ソウル側
        sb.AppendLine("[SoulInstance Dump]");
        sb.AppendLine($"{soul.SoulName} / Rank:{soul.Rank} / Job:{soul.Job?.JobName} / Talent:{soul.Talent}");
        sb.AppendLine("--------------------------------------------------");
        sb.AppendLine(DumpObject(soul, maxDepth: 4, maxEnumerableItems: 30));

        // 4-2) ボディ側
        sb.AppendLine();
        if (body != null)
        {
            sb.AppendLine("[BodyInstance Dump]");
            sb.AppendLine(DumpObject(body, maxDepth: 4, maxEnumerableItems: 30));
        }
        else
        {
            sb.AppendLine("[BodyInstance] (生成されていません)");
        }

        // 4-3) 最終ステ
        sb.AppendLine();
        sb.AppendLine("[Final Snapshot (Soul + Body)]");
        sb.AppendLine(
            $"AT:{snapshot.At} / DF:{snapshot.Df} / AGI:{snapshot.Agi} / " +
            $"MAT:{snapshot.Mat} / MDF:{snapshot.Mdf} / MaxHp:{snapshot.MaxHp}"
        );

        Debug.Log(sb.ToString());

        Debug.Log(
            $"[TestCharaMake] Soul:{soul.SoulName} / " +
            $"Body:{(body != null ? $"{body.RaceId}/{body.BodyJobId} (InstanceId={body.InstanceId})" : "none")} / " +
            $"EquippedBodyInstanceId:{(soul.EquippedBodyInstanceId ?? "null")}"
        );
    }


    private static string DumpObject(object obj, int maxDepth, int maxEnumerableItems)
    {
        var sb = new StringBuilder();
        var visited = new HashSet<int>();
        DumpObjectInternal(sb, obj, depth: 0, maxDepth: maxDepth, maxEnumerableItems: maxEnumerableItems, visited: visited);
        return sb.ToString();
    }

    private static void DumpObjectInternal(
        StringBuilder sb,
        object obj,
        int depth,
        int maxDepth,
        int maxEnumerableItems,
        HashSet<int> visited)
    {
        string indent = new string(' ', depth * 2);

        if (obj == null)
        {
            sb.AppendLine($"{indent}null");
            return;
        }

        Type t = obj.GetType();

        // プリミティブ / string / enum はそのまま表示
        if (t.IsPrimitive || obj is string || t.IsEnum || obj is decimal)
        {
            sb.AppendLine($"{indent}{obj}");
            return;
        }

        // 深さ制限
        if (depth >= maxDepth)
        {
            sb.AppendLine($"{indent}{t.Name} (maxDepth reached)");
            return;
        }

        // 循環参照対策（参照型のみ）
        if (!t.IsValueType)
        {
            int key = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            if (visited.Contains(key))
            {
                sb.AppendLine($"{indent}{t.Name} (circular ref)");
                return;
            }
            visited.Add(key);
        }

        // IEnumerable（配列/リスト等）
        if (obj is System.Collections.IEnumerable enumerable && !(obj is string))
        {
            sb.AppendLine($"{indent}{t.Name} [");
            int i = 0;
            foreach (var item in enumerable)
            {
                if (i >= maxEnumerableItems)
                {
                    sb.AppendLine($"{indent}  ... (maxEnumerableItems reached)");
                    break;
                }

                sb.AppendLine($"{indent}  [{i}] = ");
                DumpObjectInternal(sb, item, depth + 2, maxDepth, maxEnumerableItems, visited);
                i++;
            }
            sb.AppendLine($"{indent}]");
            return;
        }

        // 通常オブジェクト：フィールド＋プロパティを列挙
        sb.AppendLine($"{indent}{t.FullName} {{");

        // Fields（public/private 両方、SerializeFieldも拾う）
        var fieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        foreach (var f in t.GetFields(fieldFlags))
        {
            object value = SafeGet(() => f.GetValue(obj));
            sb.Append($"{indent}  (field) {f.Name} = ");
            DumpObjectInternal(sb, value, depth + 1, maxDepth, maxEnumerableItems, visited);
        }

        // Properties（public インスタンスのみ）
        foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!p.CanRead) continue;

            object value = SafeGet(() => p.GetValue(obj, null));
            sb.Append($"{indent}  (prop) {p.Name} = ");
            DumpObjectInternal(sb, value, depth + 1, maxDepth, maxEnumerableItems, visited);
        }

        sb.AppendLine($"{indent}}}");
    }

    private static object SafeGet(Func<object> getter)
    {
        try
        {
            return getter();
        }
        catch (Exception e)
        {
            return $"(exception: {e.GetType().Name}: {e.Message})";
        }
    }
}
