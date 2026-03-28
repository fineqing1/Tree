
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

        // ????????
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // ???????
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep; // ???????????????????????��

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

    // --- ?????? ---
    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        // ??? Tag ???? Player
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(stats.damage * Time.deltaTime);
                // ?????????????????????????????????
            }
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}