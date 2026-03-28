using UnityEngine;

public class EnemyPatrolState : EnemyStateBase
{
    private float moveDir = 1f;
    private float startPosX;
    public float patrolDistance;

    public EnemyPatrolState(EnemyController e, StateMachine sm, float distance) : base(e, sm)
    {
        this.patrolDistance = distance;
    }

    public override void Enter()
    {
        startPosX = enemy.transform.position.x;
        enemy.rb.velocity = new Vector2(enemy.stats.speed1 * moveDir, enemy.rb.velocity.y);
    }

    public override void Update()
    {
        // 1. 强制匀速
        enemy.rb.velocity = new Vector2(enemy.stats.speed1 * moveDir, enemy.rb.velocity.y);

        // 2. 距离判断 (单向距离)
        float distanceMoved = Mathf.Abs(enemy.transform.position.x - startPosX);
        if (distanceMoved >= patrolDistance)
        {
            Flip();
        }

        // 3. 撞墙判断
        CheckForWalls();
    }

    private void CheckForWalls()
    {
        // 起点稍微靠前一点
        Vector2 rayOrigin = (Vector2)enemy.transform.position + new Vector2(moveDir * 0.7f, 0);
        Vector2 rayDir = new Vector2(moveDir, 0);
        float checkDist = 0.4f;

        // 只检测 Ground 和 Default
        int mask = LayerMask.GetMask("Ground", "Default");
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDir, checkDist, mask);

        //Debug.DrawRay(rayOrigin, rayDir * checkDist, Color.green); // 绿色代表射线正常工作

        if (hit.collider != null)
        {
            // 确保撞到的物体不是敌人自己，也不是玩家
            // 且必须是真实的碰撞体（非 Trigger）
            if (!hit.collider.isTrigger &&
                !hit.collider.CompareTag("Player") &&
                hit.collider.gameObject != enemy.gameObject)
            {
                Debug.Log($"<color=cyan>探测到障碍物: {hit.collider.name}，准备转身</color>");
                Flip();
            }
        }
    }

    private void Flip()
    {
        moveDir *= -1f;
        startPosX = enemy.transform.position.x;

        // 翻转模型
        Vector3 scale = enemy.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * moveDir;
        enemy.transform.localScale = scale;
    }

    public override void Exit() { }
}