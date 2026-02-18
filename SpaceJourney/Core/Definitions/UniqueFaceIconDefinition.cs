using System;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// MasterDatabase の Inspector 配列で「ユニークアイコンIDとSprite」を直接登録するための入れ物。
    /// （ScriptableObjectは使わない）
    /// </summary>
    [Serializable]
    public class UniqueFaceIconDefinition
    {
        [SerializeField] private string uniqueIconId;
        [SerializeField] private Sprite iconSprite;

        public string UniqueIconId => uniqueIconId;
        public Sprite IconSprite => iconSprite;
    }
}
