using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PlayerModel))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] LayerMask groundLayers = 1;
    [SerializeField] float groundCheckDistance = 0.15f;
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBuffer = 0.2f;

    Rigidbody2D rb;
    BoxCollider2D col;
    PlayerModel model;

    float lastGroundedTime = -100f;
    float lastJumpPressedTime = -100f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        model = GetComponent<PlayerModel>();
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

        rb.velocity = new Vector2(move * model.moveSpeed, rb.velocity.y);

        bool grounded = IsGrounded();
        if (grounded)
            lastGroundedTime = Time.time;

        bool bufferedJump = Time.time - lastJumpPressedTime <= jumpBuffer;
        bool coyote = Time.time - lastGroundedTime <= coyoteTime;
        if (bufferedJump && coyote && rb.velocity.y < 0.25f)
        {
            rb.velocity = new Vector2(rb.velocity.x, model.jumpForce);
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
