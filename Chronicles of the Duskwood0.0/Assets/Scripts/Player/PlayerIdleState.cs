using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController p, StateMachine sm) : base(p, sm) { }

    public override void Enter() { }
    public override void Exit() { }

    public override void Update()
    {
        // 1. ÒÆ¶¯Âß¼­
        float move = 0f;
        if (Input.GetKey(KeyCode.A)) move -= 1f;
        if (Input.GetKey(KeyCode.D)) move += 1f;
        player.rb.velocity = new Vector2(move * player.moveSpeed, player.rb.velocity.y);

        // 2. ÌøÔ¾Âß¼­
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

        // 3. ×´Ì¬ÇÐ»»
        if (Input.GetMouseButtonDown(0)) stateMachine.ChangeState<FlourishCastState>();
        else if (Input.GetMouseButtonDown(1)) stateMachine.ChangeState<WitherCastState>();
    }
}