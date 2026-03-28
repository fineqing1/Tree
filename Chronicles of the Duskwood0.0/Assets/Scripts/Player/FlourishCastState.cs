using UnityEngine;

public class FlourishCastState : PlayerState
{
    public FlourishCastState(PlayerController p, StateMachine sm) : base(p, sm) { }

    public override void Enter()
    {
        if (CanCast(10)) Fire();
        stateMachine.ChangeState<PlayerIdleState>();
    }

    public override void Update() { }
    public override void Exit() { }

    private void Fire()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector2 direction = (mouseWorldPos - player.transform.position).normalized;

        GameObject projectileGO = GameObject.Instantiate(player.magicBallPrefab, player.transform.position, Quaternion.identity);
        MagicProjectile proj = projectileGO.GetComponent<MagicProjectile>();
        if (proj != null)
        {
            proj.type = MagicProjectile.MagicType.Flourish;
            proj.Launch(direction);
        }
    }
}