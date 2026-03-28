
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyModel stats;
    [Header("Patrol Settings")]
    public float patrolDistance = 5f;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public StateMachine stateMachine;
    [HideInInspector] public Transform playerTransform;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats.Initialize();

        // 物理配置
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 防止穿屏
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep; // 强制保持激活，确保碰撞检测始终生效

        stateMachine = new StateMachine();
        stateMachine.AddState(typeof(EnemyPatrolState), new EnemyPatrolState(this, stateMachine, patrolDistance));
        stateMachine.ChangeState<EnemyPatrolState>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    protected virtual void Update()
    {
        stateMachine.Update();
        if (stats.currentHP <= 0) Die();
    }

    // --- 碰撞伤害 ---
    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        // 检查 Tag 是否为 Player
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(stats.damage * Time.deltaTime);
                // 如果控制台没输出这个，说明物理矩阵没调好
                Debug.Log($"[伤害记录] 正在碰撞玩家，扣血中...");
            }
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}