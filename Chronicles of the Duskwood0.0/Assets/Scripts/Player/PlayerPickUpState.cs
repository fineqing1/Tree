using UnityEngine;

public class PlayerPickUpState : PlayerState
{
    private float timer;
    private float duration = 0.5f;

    public PlayerPickUpState(PlayerController p, StateMachine sm) : base(p, sm) { }

    public override void Enter()
    {
        timer = duration;
        // 这里可以禁用玩家移动逻辑或播放动画
        Debug.Log("[Player] Pickup state");
    }

    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            stateMachine.ChangeState<PlayerIdleState>();
        }
    }
}