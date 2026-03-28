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
        // 初始速度赋值
        enemy.rb.velocity = new Vector2(enemy.stats.speed1 * moveDir, enemy.rb.velocity.y);
    }

    public override void Update()
    {
        // 1. 恒定速度移动 (处理物理摩擦导致的减速)
        enemy.rb.velocity = new Vector2(enemy.stats.speed1 * moveDir, enemy.rb.velocity.y);

        // 2. 逻辑：走动适当距离就往反方向走
        float distanceMoved = Mathf.Abs(enemy.transform.position.x - startPosX);
        if (distanceMoved >= patrolDistance)
        {
            Flip();
        }

        // 3. 撞墙反转
        CheckForWalls();
    }

    private void CheckForWalls()
    {
        // 关键优化：射线起点稍微往移动方向偏一点，防止射到敌人自己的 Collider 导致原地抖动
        Vector2 rayOrigin = (Vector2)enemy.transform.position + new Vector2(moveDir * 0.5f, 0);
        Vector2 rayDir = new Vector2(moveDir, 0);
        float checkDist = 0.3f;

        // 这里的 LayerMask 必须排除敌人所在的层，或者确保 Ground 层设置正确
        int layerMask = LayerMask.GetMask("Ground", "Default");
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDir, checkDist, layerMask);

       

        if (hit.collider != null)
        {
            // 确保撞到的不是玩家（玩家伤害由碰撞函数处理）
            if (!hit.collider.CompareTag("Player"))
            {
                Flip();
            }
        }
    }

    private void Flip()
    {
        moveDir *= -1f;
        startPosX = enemy.transform.position.x;

        // 翻转图片
        Vector3 scale = enemy.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * moveDir;
        enemy.transform.localScale = scale;
    }

    public override void Exit()
    {
        enemy.rb.velocity = Vector2.zero;
    }
}