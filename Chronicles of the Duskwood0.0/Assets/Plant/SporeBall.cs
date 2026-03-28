using UnityEngine;

/*
// 统一继承 IInteractable 接口
public class SporeBall : MonoBehaviour, IInteractable
{
    [Header("爆炸设置")]
    public float explosionRadius = 2.5f;     // 初始爆炸半径
    public float maxScaleMultiplier = 2.0f;  // 限制最大生长倍数
    public LayerMask destructibleLayer;      // 设置为包含 "RottenWood" 的层
    public GameObject explosionEffect;       // 爆炸粒子预制体

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    // --- 实现 IInteractable 接口 ---

    // 繁盛：孢子进一步促生长（变大），爆炸半径随之增加
    public void OnFlourish()
    {
        if (transform.localScale.x < originalScale.x * maxScaleMultiplier)
        {
            transform.localScale *= 1.2f;
            explosionRadius *= 1.15f;
            Debug.Log("<color=green>孢子吸收了能量，体积和爆炸潜力增加了！</color>");
        }
    }

    // 枯萎：导致能量不稳定，引爆孢子
    public void OnWither()
    {
        Explode();
    }

    // --- 核心爆炸逻辑 ---

    private void Explode()
    {
        Debug.Log("<color=red>孢子引爆！执行破墙逻辑。</color>");

        // 1. 视觉特效
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // 2. 范围物理检测
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, destructibleLayer);

        foreach (var hit in hits)
        {
            // 优先寻找专门的腐木脚本执行逻辑
            RottenWood wood = hit.GetComponent<RottenWood>();
            if (wood != null)
            {
                wood.Explode();
            }
            else
            {
                // 兜底逻辑：如果属于可破坏层但没脚本，直接销毁
                Destroy(hit.gameObject);
            }
        }

        // 3. 销毁孢子自身
        Destroy(gameObject);
    }

    // 在编辑器中绘制红色圆圈，方便你调整 explosionRadius 的大小
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}*/

using UnityEngine;

public class SporeBall : MonoBehaviour, IInteractable
{
    [Header("爆炸设置")]
    public float baseRadius = 2.5f;
    public LayerMask destructibleLayer;
    public GameObject explosionEffect;

    [Header("生长设置")]
    public float scaleFactor = 0.25f;  // 每次缩放 25%
    private int currentScaleStep = 0;
    private Vector3 originalScale;

    private void Awake() => originalScale = transform.localScale;

    public void OnFlourish()
    {
        currentScaleStep++;
        UpdateSporeState();
        Debug.Log("孢子变大");
    }

    public void OnWither()
    {
        currentScaleStep--;
        if (currentScaleStep < -2) // 如果缩得太小，孢子直接枯萎消失
        {
            Destroy(gameObject);
            return;
        }
        UpdateSporeState();
        Debug.Log("孢子缩小");
    }

    private void UpdateSporeState()
    {
        float multiplier = 1f + (currentScaleStep * scaleFactor);
        multiplier = Mathf.Max(0.2f, multiplier);
        transform.localScale = originalScale * multiplier;
    }

    // 你可以在这里保留一个手动触发引爆的逻辑（比如按另一个键，或者双击）
    // 或者如果你希望“缩小到极致”就是爆炸，就在 OnWither 里改
    public void Explode()
    {
        // 爆炸半径随当前大小缩放
        float currentRadius = baseRadius * (transform.localScale.x / originalScale.x);

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentRadius, destructibleLayer);
        foreach (var hit in hits)
        {
            hit.GetComponent<RottenWood>()?.Explode();
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float currentRadius = baseRadius * (transform.localScale.x / originalScale.x);
        Gizmos.DrawWireSphere(transform.position, currentRadius);
    }
}