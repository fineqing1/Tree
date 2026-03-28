using UnityEngine;
public interface IInteractable
{
    void OnFlourish(); // 繁盛响应
    void OnWither();   // 枯萎响应
}
/*
[RequireComponent(typeof(Rigidbody2D))]
public class MagicProjectile : MonoBehaviour
{
    public enum MagicType { Flourish, Wither }
    public MagicType type;

    [Header("Attributes")]
    public float originalSpeed = 10f;
    public float accelerate = -2f; // 这里的减速会通过代码施加给物理速度
    public float lifeTime = 5f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.velocity = direction.normalized * originalSpeed;

        // --- 视觉反馈 (可选) ---
        // 让你一眼看出射出的是哪种球
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = (type == MagicType.Flourish) ? Color.green : Color.magenta;
        }
    }

    void FixedUpdate() // 物理相关的计算放在 FixedUpdate
    {
        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            // 1. 实现减速逻辑 (1.1.2.1.1.1.2)
            float speed = rb.velocity.magnitude;
            speed += accelerate * Time.fixedDeltaTime;
            if (speed < 0) speed = 0;
            rb.velocity = rb.velocity.normalized * speed;

            // 2. 旋转指向飞行方向
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // 注意：反弹效果由物理材质处理，这里只处理“撞到机关”的逻辑
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 忽略玩家
        if (collision.gameObject.CompareTag("Player")) return;

        // 2. 尝试获取机关接口
        IInteractable interactable = collision.gameObject.GetComponent<IInteractable>();
        if (interactable != null)
        {
            if (type == MagicType.Flourish) interactable.OnFlourish();
            else interactable.OnWither();

            // 撞到机关通常不需要反弹，直接生效并销毁
            Destroy(gameObject);
        }

        // 提示：撞到普通墙壁时，物理材质会自动处理反弹，不需要在这里写代码销毁
    }
}*/
[RequireComponent(typeof(Rigidbody2D))]
public class MagicProjectile : MonoBehaviour
{
    public enum MagicType { Flourish, Wither }
    public MagicType type;

    [Header("Attributes")]
    public float originalSpeed = 10f;
    public float accelerate = -2f;
    public float lifeTime = 5f;
    public int maxBounces = 3; // 最大反弹次数

    private Rigidbody2D rb;
    private int currentBounces = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.velocity = direction.normalized * originalSpeed;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = (type == MagicType.Flourish) ? Color.green : Color.magenta;
        }
    }

    void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            float speed = rb.velocity.magnitude;
            speed += accelerate * Time.fixedDeltaTime;
            if (speed < 0) speed = 0;
            rb.velocity = rb.velocity.normalized * speed;

            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 忽略玩家
        if (collision.gameObject.CompareTag("Player")) return;

        // 2. 尝试获取机关接口 IInteractable
        IInteractable interactable = collision.gameObject.GetComponent<IInteractable>();

        if (interactable != null)
        {
            // 命中植物：触发对应逻辑
            if (type == MagicType.Flourish) interactable.OnFlourish();
            else interactable.OnWither();

            Debug.Log($"<color=cyan>[魔法激活]</color> 目标: {collision.gameObject.name}");
            Destroy(gameObject);
            return;
        }

        // 3. 没撞到植物，执行普通反弹计数
        currentBounces++;
        if (currentBounces > maxBounces)
        {
            Destroy(gameObject);
        }
        Debug.Log($"魔法球反弹({currentBounces}): {collision.gameObject.name}");
    }
}
