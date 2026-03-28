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
        // 1. ???????
        enemy.rb.velocity = new Vector2(enemy.stats.speed1 * moveDir, enemy.rb.velocity.y);

        // 2. ?????��? (???????)
        float distanceMoved = Mathf.Abs(enemy.transform.position.x - startPosX);
        if (distanceMoved >= patrolDistance)
        {
            Flip();
        }

        // 3. ???��?
        CheckForWalls();
    }

    private void CheckForWalls()
    {
        // ????????????
        Vector2 rayOrigin = (Vector2)enemy.transform.position + new Vector2(moveDir * 0.7f, 0);
        Vector2 rayDir = new Vector2(moveDir, 0);
        float checkDist = 0.4f;

        // ???? Ground ?? Default
        int mask = LayerMask.GetMask("Ground", "Default");
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDir, checkDist, mask);

        //Debug.DrawRay(rayOrigin, rayDir * checkDist, Color.green); // ???????????????????

        if (hit.collider != null)
        {
            // ???????????�I??????????????????
            // ????????????????��?? Trigger??
            if (!hit.collider.isTrigger &&
                !hit.collider.CompareTag("Player") &&
                hit.collider.gameObject != enemy.gameObject)
            {
                Flip();
            }
        }
    }

    private void Flip()
    {
        moveDir *= -1f;
        startPosX = enemy.transform.position.x;

        // ??????
        Vector3 scale = enemy.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * moveDir;
        enemy.transform.localScale = scale;
    }

    public override void Exit() { }
}