using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerModel playerModel;
    [HideInInspector]
    public bool isLeft;
    [HideInInspector]
    public bool isRight;

    private bool isGrounded;
    private Rigidbody2D rb;

    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    public GameObject pauseUI;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        playerModel = GetComponent<PlayerModel>();
        if (pauseUI != null)
            pauseUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        
        HandleJump();
        HandleCasting();
        HandlePause();
    }

    void FixedUpdate()
    {
        HandleMove();
        CheckGround();
    }

    // 1️⃣ 输入处理
    void HandleInput()
    {
        isLeft = Input.GetKey(KeyCode.A);
        isRight = Input.GetKey(KeyCode.D);
    }

    // 2️⃣ 移动
    void HandleMove()
    {
        float h = 0;
        if (isLeft) h = -1;
        if (isRight) h = 1;

        rb.velocity = new Vector2(h * playerModel.moveSpeed, rb.velocity.y);
    }

    // 3️⃣ 检测地面
    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // 4️⃣ 跳跃
    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, playerModel.jumpForce);
        }
    }

    // 5️⃣ 施法
    void HandleCasting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("繁盛施法！");
            // TODO: 触发繁盛施法动画或技能逻辑
        }
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("枯萎施法！");
            // TODO: 触发枯萎施法动画或技能逻辑
        }
    }

    // 6️⃣ 暂停
    void HandlePause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseUI != null)
            {
                bool isActive = pauseUI.activeSelf;
                pauseUI.SetActive(!isActive);
                Time.timeScale = isActive ? 1f : 0f;
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}


