public abstract class EnemyStateBase : StateBase
{
    protected EnemyController enemy;
    protected StateMachine stateMachine;

    public EnemyStateBase(EnemyController enemy, StateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}