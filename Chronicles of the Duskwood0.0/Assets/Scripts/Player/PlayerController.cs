using UnityEngine;
/*
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHp = 100;
    public int currentHP = 100;
    public int maxFuel = 100;
    public int currentFuel = 100;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Ground")]
    [SerializeField] LayerMask groundLayers = 1;
    [SerializeField] float groundCheckDistance = 0.15f;
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBuffer = 0.2f;

    Rigidbody2D rb;
    BoxCollider2D col;

    float lastGroundedTime = -100f;
    float lastJumpPressedTime = -100f;

    private StateMachine stateMachine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        stateMachine = new StateMachine();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            lastJumpPressedTime = Time.time;
    }

    void FixedUpdate()
    {
        float move = 0f;
        if (Input.GetKey(KeyCode.A)) move -= 1f;
        if (Input.GetKey(KeyCode.D)) move += 1f;

        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);

        bool grounded = IsGrounded();
        if (grounded)
            lastGroundedTime = Time.time;

        bool bufferedJump = Time.time - lastJumpPressedTime <= jumpBuffer;
        bool coyote = Time.time - lastGroundedTime <= coyoteTime;
        if (bufferedJump && coyote && rb.velocity.y < 0.25f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            lastGroundedTime = -100f;
            lastJumpPressedTime = -100f;
        }
    }

    bool IsGrounded()
    {
        if (col.IsTouchingLayers(groundLayers))
            return true;

        Bounds b = col.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y - 0.03f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayers);
        if (hit.collider != null && hit.collider.gameObject != gameObject)
            return true;

        float half = b.extents.x * 0.85f;
        RaycastHit2D hitL = Physics2D.Raycast(new Vector2(b.center.x - half, b.min.y - 0.03f), Vector2.down, groundCheckDistance, groundLayers);
        RaycastHit2D hitR = Physics2D.Raycast(new Vector2(b.center.x + half, b.min.y - 0.03f), Vector2.down, groundCheckDistance, groundLayers);
        return (hitL.collider != null && hitL.collider.gameObject != gameObject)
            || (hitR.collider != null && hitR.collider.gameObject != gameObject);
    }
}
*/

using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHp = 100;
    public float currentHP = 100f;
    public int maxFuel = 100;
    public float currentFuel = 100f;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Magic Settings")]
    public GameObject magicBallPrefab;

    [Header("Ground Check")]
    public LayerMask groundLayers = 1;
    public float groundCheckDistance = 0.15f;
    public float coyoteTime = 0.12f;
    public float jumpBuffer = 0.2f;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public CapsuleCollider2D col;
    [HideInInspector] public float lastGroundedTime = -100f;
    [HideInInspector] public float lastJumpPressedTime = -100f;

    private StateMachine stateMachine;
    private bool isDead = false;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();

        stateMachine = new StateMachine();
        stateMachine.AddState(typeof(PlayerIdleState), new PlayerIdleState(this, stateMachine));
        stateMachine.AddState(typeof(FlourishCastState), new FlourishCastState(this, stateMachine));
        stateMachine.AddState(typeof(WitherCastState), new WitherCastState(this, stateMachine));

        stateMachine.ChangeState<PlayerIdleState>();
    }

    void Update()
    {
        if (isDead) return;

        stateMachine.Update();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHP -= amount;

        Debug.Log($"[Player] Damage taken: {amount:F2}, HP remaining: {currentHP:F2}");

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }
    private void Die()
    {
        isDead = true;
        Debug.Log("[Player] Dead - reloading scene");

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsGrounded()
    {
        // ???? IsTouchingLayers????????????????? Ground????????????/coyote??
        // ???????????????????
        Bounds b = col.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y - 0.03f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayers);
        if (hit.collider != null && hit.collider.gameObject != gameObject) return true;

        float half = b.extents.x * 0.85f;
        RaycastHit2D hitL = Physics2D.Raycast(new Vector2(b.center.x - half, b.min.y - 0.03f), Vector2.down, groundCheckDistance, groundLayers);
        RaycastHit2D hitR = Physics2D.Raycast(new Vector2(b.center.x + half, b.min.y - 0.03f), Vector2.down, groundCheckDistance, groundLayers);
        return (hitL.collider != null && hitL.collider.gameObject != gameObject) || (hitR.collider != null && hitR.collider.gameObject != gameObject);
    }
}