using UnityEngine;

public class BouncyMushroom : MonoBehaviour, IMagicInteractable
{
    [Header("弹跳设置")]
    public float baseForce = 10f;
    public float boostedForce = 20f;

    [Header("孢子生成")]
    public GameObject sporePrefab;      // 孢子预制体
    public Transform spawnPoint;        // 孢子掉落起点
    public float spawnCooldown = 2f;    // 产出冷却，防止无限刷

    private bool isBoosted = false;
    private float lastSpawnTime;
    private Vector3 originalScale;

    private void Awake() => originalScale = transform.localScale;

    public void ApplyMagic(MagicEffectType type)
    {
        if (type == MagicEffectType.Flourish)
        {
            isBoosted = true;
            transform.localScale = originalScale * 1.3f; // 视觉上变大
            TryDropSpore();
        }
        else
        {
            isBoosted = false;
            transform.localScale = originalScale; // 恢复原状
        }
    }

    private void TryDropSpore()
    {
        if (Time.time >= lastSpawnTime + spawnCooldown && sporePrefab != null)
        {
            GameObject spore = Instantiate(sporePrefab, spawnPoint.position, Quaternion.identity);

            // 给孢子一个随机的弹出小力矩，更有“掉落”感
            Rigidbody2D rb = spore.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(new Vector2(Random.Range(-1f, 1f), 2f), ForceMode2D.Impulse);
            }

            lastSpawnTime = Time.time;
            Debug.Log("<color=green>蘑菇受到繁茂影响，掉落了一个孢子！</color>");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 只有向上接触时才弹跳，防止侧碰起跳
            if (collision.contacts[0].normal.y < -0.5f)
            {
                float force = isBoosted ? boostedForce : baseForce;
                rb.velocity = new Vector2(rb.velocity.x, force);
                Debug.Log("蘑菇弹跳！");
            }
        }
    }
}