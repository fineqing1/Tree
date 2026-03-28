using UnityEngine;
using System.Collections;

/*
public class BouncyMushroom : MonoBehaviour, IInteractable
{
    [Header("弹跳设置")]
    public float baseForce = 10f;
    public float boostedForce = 20f;

    [Header("变大效果")]
    public float scaleMultiplier = 1.3f; // 变大倍数
    public float boostDuration = 3f;    // 变大持续时间
    private Vector3 originalScale;
    private bool isBoosted = false;
    private Coroutine resetCoroutine;

    [Header("孢子生成")]
    public GameObject sporePrefab;
    public Transform spawnPoint;
    public Vector2 spawnVelocity = new Vector2(3f, 5f); // 喷射初速度

    private void Awake() => originalScale = transform.localScale;

    // --- 实现 IInteractable 接口 ---

    public void OnFlourish()
    {
        // 1. 变大逻辑
        isBoosted = true;
        transform.localScale = originalScale * scaleMultiplier;

        // 2. 发射孢子（收到繁荣球立即发射）
        ShootSpore();

        // 3. 开启/重置 自动回缩计时器
        if (resetCoroutine != null) StopCoroutine(resetCoroutine);
        resetCoroutine = StartCoroutine(ResetScaleRoutine());
    }

    public void OnWither()
    {
        // 如果被枯萎球击中，立即缩回
        isBoosted = false;
        transform.localScale = originalScale;
        if (resetCoroutine != null) StopCoroutine(resetCoroutine);
    }

    // --- 内部逻辑 ---

    private void ShootSpore()
    {
        if (sporePrefab == null || spawnPoint == null) return;

        GameObject spore = Instantiate(sporePrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = spore.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // 给孢子一个向前的弧线速度
            // 这里你可以根据需要微调 Random 的范围
            Vector2 finalVel = new Vector2(
                Random.Range(spawnVelocity.x - 0.5f, spawnVelocity.x + 0.5f),
                spawnVelocity.y
            );
            rb.velocity = finalVel;
        }
        Debug.Log("<color=green>蘑菇受激！喷射孢子！</color>");
    }

    private IEnumerator ResetScaleRoutine()
    {
        yield return new WaitForSeconds(boostDuration);
        isBoosted = false;
        transform.localScale = originalScale;
        Debug.Log("蘑菇疲劳了，缩回去了。");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 撞击蘑菇顶部触发
            if (collision.contacts[0].normal.y < -0.5f)
            {
                float force = isBoosted ? boostedForce : baseForce;
                rb.velocity = new Vector2(rb.velocity.x, force);
            }
        }
    }
}*/

using UnityEngine;
using System.Collections;

public class BouncyMushroom : MonoBehaviour, IInteractable
{
    [Header("高度精准校准")]
    [Tooltip("普通状态施加的弹力")]
    public float baseForce = 7.5f;

    [Tooltip("繁荣状态施加的弹力")]
    public float boostedForce = 9f;

    [Header("变大效果")]
    public float scaleMultiplier = 1.2f;
    public float boostDuration = 3f;
    private Vector3 originalScale;
    private bool isBoosted = false;
   // private Coroutine resetCoroutine;

    [Header("孢子生成")]
    public GameObject sporePrefab;
    public Transform spawnPoint;

    private void Awake() => originalScale = transform.localScale;

    public void OnFlourish()
    {
        isBoosted = true;
        transform.localScale = originalScale * scaleMultiplier;
        ShootSpore();
       // if (resetCoroutine != null) StopCoroutine(resetCoroutine);
       // resetCoroutine = StartCoroutine(ResetScaleRoutine());
    }

    public void OnWither() => StopBoost();

    private void StopBoost()
    {
        isBoosted = false;
        transform.localScale = originalScale;
      //  if (resetCoroutine != null) StopCoroutine(resetCoroutine);
    }

    private void ShootSpore()
    {
        if (sporePrefab == null || spawnPoint == null) return;
        GameObject spore = Instantiate(sporePrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = spore.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = new Vector2(Random.Range(-1.5f, 1.5f), 5f);
    }

   // private IEnumerator ResetScaleRoutine()
   // {
   //     yield return new WaitForSeconds(boostDuration);
    //    StopBoost();
   // }

   
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 判定从上方落下
            if (collision.contacts.Length > 0 && collision.contacts[0].normal.y < -0.5f)
            {
                // 精准数值：
                // 7.5 的推力在重力1下约为 1 格多，绝对跳不上 4 格。
                // 17.0 的推力能稳上 4 格。
                float targetForce = isBoosted ? boostedForce : baseForce;

                // 必须清零，防止 Depenetration 穿透补偿速度叠加
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.velocity = new Vector2(rb.velocity.x, targetForce);

                Debug.Log($"蘑菇弹起！状态:{(isBoosted ? "繁茂" : "普通")}, 力量:{targetForce}");
            }
        }
    }
}