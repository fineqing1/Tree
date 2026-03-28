using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController p, StateMachine sm) : base(p, sm) { }

    public override void Enter() { }
    public override void Exit() { }

    /*
    public override void Update()
    {
        // 1. 移动逻辑
        float move = 0f;
        if (Input.GetKey(KeyCode.A)) move -= 1f;
        if (Input.GetKey(KeyCode.D)) move += 1f;
        player.rb.velocity = new Vector2(move * player.moveSpeed, player.rb.velocity.y);

        // 2. 跳跃逻辑
        if (Input.GetKeyDown(KeyCode.Space))
            player.lastJumpPressedTime = Time.time;

        bool grounded = player.IsGrounded();
        if (grounded) player.lastGroundedTime = Time.time;

        bool bufferedJump = Time.time - player.lastJumpPressedTime <= player.jumpBuffer;
        bool coyote = Time.time - player.lastGroundedTime <= player.coyoteTime;

        if (bufferedJump && coyote && player.rb.velocity.y < 0.25f)
        {
            player.rb.velocity = new Vector2(player.rb.velocity.x, player.jumpForce);
            player.lastGroundedTime = -100f;
            player.lastJumpPressedTime = -100f;
        }

        // 3. 状态切换
        if (Input.GetMouseButtonDown(0)) stateMachine.ChangeState<FlourishCastState>();
        else if (Input.GetMouseButtonDown(1)) stateMachine.ChangeState<WitherCastState>();
    }*/
    public override void Update()
    {
        // 1. 移动逻辑优化
        float move = 0f;
        if (Input.GetKey(KeyCode.A)) move -= 1f;
        if (Input.GetKey(KeyCode.D)) move += 1f;

        // --- 关键修改：只在有按键输入或需要摩擦力时处理 X，绝对不要无脑覆盖 Y ---
        // 获取当前 Y 速度，用于保留物理效果（如蘑菇弹跳）
        float currentY = player.rb.velocity.y;
        player.rb.velocity = new Vector2(move * player.moveSpeed, currentY);

        // 2. 跳跃逻辑
        if (Input.GetKeyDown(KeyCode.Space))
            player.lastJumpPressedTime = Time.time;

        bool grounded = player.IsGrounded();
        if (grounded) player.lastGroundedTime = Time.time;

        bool bufferedJump = Time.time - player.lastJumpPressedTime <= player.jumpBuffer;
        bool coyote = Time.time - player.lastGroundedTime <= player.coyoteTime;

        // 只有在向下掉落或静止时才能跳跃，防止二段跳干扰
        if (bufferedJump && coyote && player.rb.velocity.y < 0.25f)
        {
            // 调用我们统一的跳跃方法
            player.rb.velocity = new Vector2(player.rb.velocity.x, player.jumpForce);
            player.lastGroundedTime = -100f;
            player.lastJumpPressedTime = -100f;
        }

        // 3. 状态切换
        if (Input.GetMouseButtonDown(0)) stateMachine.ChangeState<FlourishCastState>();
        else if (Input.GetMouseButtonDown(1)) stateMachine.ChangeState<WitherCastState>();
    }
}