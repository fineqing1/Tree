using Unity.VisualScripting;

public abstract class PlayerState : StateBase
{
    protected PlayerController player;
    protected StateMachine stateMachine;

    public PlayerState(PlayerController player, StateMachine sm)
    {
        this.player = player;
        this.stateMachine = sm;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }

    // ������������������ʩ������
    protected bool CanCast(int cost)
    {
        if (player.currentFuel >= cost)
        {
            player.currentFuel -= cost; // �۳�����
            return true;
        }
        return false;
    }
}