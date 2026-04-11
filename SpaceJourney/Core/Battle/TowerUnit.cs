using System;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// タワー面に配置されるタワーユニット。
    /// キャラとは別枠で、EP消費による遠距離攻撃を行う。
    /// タワー面にはキャラを配置できない。
    ///
    /// タワーが破壊されるとサイドはEmpty扱いになる。
    /// コスト = 0 (無料)。
    /// </summary>
    [Serializable]
    public class TowerUnit
    {
        [Header("基本")]
        [SerializeField] private string towerId;
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int currentHp;
        [SerializeField] private bool isDestroyed = false;

        [Header("攻撃")]
        [SerializeField] private int attackPower = 30;
        [SerializeField] private int epCostPerAttack = 10;
        [SerializeField] private int attackCoolTime = 2;

        public string TowerId => towerId;
        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public bool IsDestroyed => isDestroyed;
        public int AttackPower => attackPower;
        public int EpCostPerAttack => epCostPerAttack;
        public int AttackCoolTime => attackCoolTime;

        public TowerUnit(string towerId, int maxHp = 100, int attackPower = 30, int epCost = 10)
        {
            this.towerId = towerId;
            this.maxHp = maxHp;
            this.currentHp = maxHp;
            this.attackPower = attackPower;
            this.epCostPerAttack = epCost;
        }

        public void TakeDamage(int amount)
        {
            if (isDestroyed || amount <= 0) return;
            currentHp -= amount;
            if (currentHp <= 0)
            {
                currentHp = 0;
                isDestroyed = true;
            }
        }

        public void Repair(int amount)
        {
            if (amount <= 0) return;
            currentHp = Mathf.Min(currentHp + amount, maxHp);
            if (currentHp > 0) isDestroyed = false;
        }
    }
}
