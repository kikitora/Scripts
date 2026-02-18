using System;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// MasterDatabase の Inspector 配列で「IDとPrefab」を直接登録するための入れ物。
    /// （ScriptableObjectは使わない）
    /// </summary>
    [Serializable]
    public class PrefabEntry
    {
        [SerializeField] private string id;
        [SerializeField] private GameObject prefab;

        public string Id => id;
        public GameObject Prefab => prefab;
    }
}
