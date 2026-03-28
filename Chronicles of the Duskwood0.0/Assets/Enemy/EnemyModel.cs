using UnityEngine;

[System.Serializable]
public class EnemyModel
{
    [Header("Health Settings")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("Movement Settings")]
    public float speed1 = 2f; // 基础移动速度
    public float chaseSpeed = 4f; // 追逐速度

    [Header("Combat Settings")]
    public float damage = 10f; // 持续扣血的伤害量
    public float attackInterval = 0.5f; // 伤害触发间隔

    public void Initialize()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0);
    }
}