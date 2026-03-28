using UnityEngine;
using System.Collections;

public class ThornVine : MonoBehaviour, IInteractable
{
    [Header("组件引用")]
    public Collider2D thornCollider;    // 建议：始终不禁用，仅切换 IsTrigger
    public Transform spikeVisual;       // 拖入代表“刺”的子物体
    public SpriteRenderer spikeRenderer;

    [Header("生长设置")]
    public Vector3 hiddenPos = Vector3.zero;     // 刺缩起来的位置
    public Vector3 grownPos = new Vector3(0, 1, 0); // 刺伸出来的目标位置
    public float growSpeed = 5f;

    [Header("伤害设置")]
    public float damage = 10f;
    public float knockbackForce = 5f;

    private bool isActive = true;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        // 初始状态：确保 Collider 是开启的，用于接收魔法球
        if (thornCollider != null) thornCollider.enabled = true;

        // 初始强制设为激活状态或根据需求设置
        SetThornState(true, true);
    }

    // --- 实现 IInteractable 接口 ---

    public void OnFlourish()
    {
        if (!isActive)
        {
            StopActiveTransition();
            transitionCoroutine = StartCoroutine(MoveSpike(grownPos, true));
            Debug.Log("<color=green>荆棘刺出！</color>");
        }
    }

    public void OnWither()
    {
        if (isActive)
        {
            StopActiveTransition();
            transitionCoroutine = StartCoroutine(MoveSpike(hiddenPos, false));
            Debug.Log("<color=gray>荆棘缩回...</color>");
        }
    }

    // --- 核心逻辑 ---

    private IEnumerator MoveSpike(Vector3 targetPos, bool targetActive)
    {
        // 如果是变回激活态，逻辑上先判定为 Active，防止玩家在刺出来的过程中无敌
        if (targetActive) isActive = true;

        while (Vector3.Distance(spikeVisual.localPosition, targetPos) > 0.01f)
        {
            spikeVisual.localPosition = Vector3.MoveTowards(
                spikeVisual.localPosition,
                targetPos,
                growSpeed * Time.deltaTime
            );
            yield return null;
        }

        spikeVisual.localPosition = targetPos;
        SetThornState(targetActive, false); // 完成移动后更新最终状态
    }

    private void SetThornState(bool active, bool immediate)
    {
        isActive = active;

        // 核心：如果是激活态，它是实心的；如果是枯萎态，它是触发器（玩家可穿过，魔法球能打中）
        if (thornCollider != null)
        {
            thornCollider.isTrigger = !active;
        }

        if (immediate)
        {
            spikeVisual.localPosition = active ? grownPos : hiddenPos;
        }

        if (spikeRenderer != null)
        {
            spikeRenderer.color = active ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.3f);
        }
    }

    private void StopActiveTransition()
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
    }

    // 只有在非 Trigger（即 isActive 为 true）时，OnCollisionEnter2D 才会触发
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isActive) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                    rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}