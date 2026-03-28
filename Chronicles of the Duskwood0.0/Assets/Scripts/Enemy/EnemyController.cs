using UnityEngine;
/*
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class EnemyController : MonoBehaviour
{
    public EnemyModel stats; // 在编辑器里填入属性数据

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public StateMachine stateMachine;
    [HideInInspector] public Transform playerTransform;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats.Initialize();
        stateMachine = new StateMachine();

        // 查找玩家（建议通过单例或标签）
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    protected virtual void Update()
    {
        stateMachine.Update();

        if (stats.currentHP <= 0) Die();
    }

    // 规则：玩家进入持续扣血 (Trigger版)
    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // 每秒扣除 stats.damage 的血量
                player.currentHP -= (int)(stats.damage * Time.deltaTime);
            }
        }
    }

    protected virtual void Die()
    {
        Debug.Log(gameObject.name + " 已死亡");
        Destroy(gameObject);
    }
}*/

using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyModel stats;
    [Header("Patrol Settings")]
    public float patrolDistance = 10f; // 增大默认巡逻距离

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public StateMachine stateMachine;
    [HideInInspector] public Transform playerTransform;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats.Initialize();

        // 必须锁定旋转，否则撞墙后敌人会倒下
        rb.freezeRotation = true;

        stateMachine = new StateMachine();

        // 注入状态
        stateMachine.AddState(typeof(EnemyPatrolState), new EnemyPatrolState(this, stateMachine, patrolDistance));
        stateMachine.ChangeState<EnemyPatrolState>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    protected virtual void Update()
    {
        stateMachine.Update();

        // 死亡检测
        if (stats.currentHP <= 0) Die();
    }

    // --- 核心修改：非 Trigger 模式下的碰撞扣血 ---
    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                // 持续伤害逻辑
                player.TakeDamage(stats.damage * Time.deltaTime);
                Debug.Log("正在碰撞玩家，造成伤害: " + stats.damage * Time.deltaTime);
            }
        }
    }

    protected virtual void Die()
    {
        Debug.Log(gameObject.name + " 已死亡");
        Destroy(gameObject);
    }
}