using UnityEngine;

public class SporeBall : MonoBehaviour, IMagicInteractable
{
    [Header("爆炸设置")]
    public float explosionRadius = 2.5f;
    public LayerMask destructibleLayer; // 设置为包含 "RottenWood" 的层
    public GameObject explosionEffect;  // 爆炸粒子预制体

    public void ApplyMagic(MagicEffectType type)
    {
        // 孢子只有在“繁茂”时会继续膨胀，在“枯萎”时会不稳定而爆炸
        if (type == MagicEffectType.Flourish)
        {
            transform.localScale *= 1.2f;
            explosionRadius *= 1.1f; // 威力随体型增大
            Debug.Log("孢子过度生长，变得更加不稳定...");
        }
        else
        {
            Explode();
        }
    }

    private void Explode()
    {
        Debug.Log("<color=red>孢子引爆！</color>");

        // 1. 产生爆炸视觉反馈
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // 2. 范围检测破墙
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, destructibleLayer);
        foreach (var hit in hits)
        {
            // 尝试摧毁腐木
            RottenWood wood = hit.GetComponent<RottenWood>();
            if (wood != null)
            {
                wood.Explode();
            }
            else
            {
                // 如果没有专门的脚本，但属于可破坏层，直接销毁（兜底逻辑）
                Destroy(hit.gameObject);
            }
        }

        // 3. 销毁孢子自身
        Destroy(gameObject);
    }

    // 在编辑器里画出爆炸范围，方便调试
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}