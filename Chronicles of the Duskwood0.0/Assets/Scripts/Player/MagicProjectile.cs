using UnityEngine;
/*
using UnityEngine;

public interface IInteractable
{
    void OnFlourish(); // 繁盛响应
    void OnWither();   // 枯萎响应
}

public class MagicProjectile : MonoBehaviour
{
    public enum MagicType { Flourish, Wither }
    public MagicType type;

    [Header("Attributes")]
    public float originalSpeed = 10f;
    public float accelerate = -2f;
    public float gravity = 1f;
    public float lifeTime = 5f; // 自动销毁时间

    private Vector2 velocity;

    public void Launch(Vector2 direction)
    {
        velocity = direction.normalized * originalSpeed;
        // 5秒后如果不撞击也自动销毁
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 1. 速度计算
        float speed = velocity.magnitude;
        speed += accelerate * Time.deltaTime;
        if (speed < 0) speed = 0;

        velocity = velocity.normalized * speed;

        // 2. 重力影响
        velocity.y -= gravity * Time.deltaTime;

        // 3. 应用位移
        transform.position += (Vector3)velocity * Time.deltaTime;

        // 可选：让球体旋转指向飞行方向
        if (velocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // 关键：碰撞处理（防穿墙）
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 忽略玩家（防止球一出生就撞到自己销毁）
        if (other.CompareTag("Player")) return;

        // 2. 检查是否撞到了机关
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            if (type == MagicType.Flourish) interactable.OnFlourish();
            else interactable.OnWither();
            Destroy(gameObject);
            return;
        }

        // 3. 检查层级（防穿墙）
        // 这里确保 Default 层能被识别。如果地板确实是 Default，代码没问题。
        // 但建议专门为地板建一个层叫 "Ground"，在 Inspector 里把地板改过去。
        int layerMask = LayerMask.GetMask("Ground", "Default");
        if (((1 << other.gameObject.layer) & layerMask) != 0)
        {
            Destroy(gameObject);
        }
    }
    
}*/

using UnityEngine;
public interface IInteractable
{
    void OnFlourish(); // 繁盛响应
    void OnWither();   // 枯萎响应
}
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
        // 直接给物理引擎一个初速度
        rb.velocity = direction.normalized * originalSpeed;
        Destroy(gameObject, lifeTime);
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
}