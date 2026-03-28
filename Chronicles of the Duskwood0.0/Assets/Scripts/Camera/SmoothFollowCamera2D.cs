using UnityEngine;

/// <summary>
/// 2D 正交相机：死区内只刹车；死区外跟随时只保留「指向理想点」方向的速度分量，
/// 每帧去掉切向速度，避免像月球公转一样绕圈停不下来。
/// </summary>
[DisallowMultipleComponent]
public class SmoothFollowCamera2D : MonoBehaviour
{
    [Header("目标")]
    [Tooltip("留空：运行时自动查找 PlayerController（Additive 关卡与 Persistence 相机跨场景时请勿拖玩家）。仅当目标与本相机在同一持久场景内时才可指定。")]
    [SerializeField] Transform target;
    [Tooltip("相对玩家的相机理想位置偏移（Z 一般为相机深度）")]
    [SerializeField] Vector3 followOffset = new Vector3(0f, 0f, -10f);

    [Header("死区与滞回")]
    [Tooltip("理想点与相机 XY 距离 ≤ 此值时视为「在范围内」：只刹车，不向目标加速")]
    [SerializeField] float deadZoneRadius = 0.35f;
    [Tooltip("偏移 ≥ 此值时重新开始跟随；应略大于死区半径，避免在边界来回抖动")]
    [SerializeField] float followResumeRadius = 0.55f;

    [Header("运动参数")]
    [Tooltip("正在跟随时，沿指向理想点方向的加速度")]
    [SerializeField] float followAcceleration = 48f;
    [Tooltip("制动时沿速度反向的减速度（死区内可乘以倍率）")]
    [SerializeField] float followDeceleration = 40f;
    [Tooltip("死区内制动倍率，越大越快停住")]
    [SerializeField] float deadZoneBrakeMultiplier = 2.5f;
    [Tooltip("跟随时 XY 速度上限")]
    [SerializeField] float maxFollowSpeed = 16f;

    Vector2 velocity;
    bool isChasing = true;

    void OnEnable()
    {
        ClearSerializedTargetIfCrossScene();
    }

    void Start()
    {
        ClearSerializedTargetIfCrossScene();
        if (followOffset.z == 0f && Mathf.Abs(transform.position.z) > 0.01f)
            followOffset.z = transform.position.z;
        ResolveTargetIfNeeded();
        float resume = EffectiveResumeRadius();
        if (target != null)
        {
            Vector2 err = IdealXY() - (Vector2)transform.position;
            isChasing = err.magnitude >= resume;
        }
    }

    void LateUpdate()
    {
        ResolveTargetIfNeeded();
        if (target == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 ideal = target.position + followOffset;
        ideal.z = followOffset.z != 0f ? followOffset.z : transform.position.z;

        Vector2 pos = transform.position;
        Vector2 idealXY = IdealXY();
        Vector2 err = idealXY - pos;
        float errMag = err.magnitude;
        Vector2 errDir = errMag > 1e-5f ? err / errMag : Vector2.zero;

        float resume = EffectiveResumeRadius();

        if (isChasing)
        {
            if (errMag <= deadZoneRadius)
                isChasing = false;
        }
        else
        {
            if (errMag >= resume)
                isChasing = true;
        }

        if (isChasing)
        {
            if (errMag > 1e-5f)
            {
                // Strip tangential velocity (orbit component); only move along camera -> ideal axis.
                float radialSpeed = Vector2.Dot(velocity, errDir);
                velocity = errDir * radialSpeed;
                velocity += errDir * (followAcceleration * dt);
            }

            float speed = velocity.magnitude;
            if (speed > maxFollowSpeed)
                velocity *= maxFollowSpeed / speed;
        }
        else
        {
            ApplyDeceleration(dt, deadZoneBrakeMultiplier);
        }

        pos += velocity * dt;
        transform.position = new Vector3(pos.x, pos.y, ideal.z);
    }

    Vector2 IdealXY()
    {
        Vector3 ideal = target.position + followOffset;
        return new Vector2(ideal.x, ideal.y);
    }

    float EffectiveResumeRadius()
    {
        return followResumeRadius > deadZoneRadius ? followResumeRadius : deadZoneRadius * 1.35f;
    }

    void ApplyDeceleration(float dt, float multiplier = 1f)
    {
        float decel = followDeceleration * multiplier;
        float speed = velocity.magnitude;
        if (speed < 1e-5f)
        {
            velocity = Vector2.zero;
            return;
        }

        float drop = decel * dt;
        if (drop >= speed)
            velocity = Vector2.zero;
        else
            velocity -= velocity.normalized * drop;
    }

    void ResolveTargetIfNeeded()
    {
        if (target != null && (!target.gameObject || !target.gameObject.activeInHierarchy))
            target = null;
        if (target != null) return;

        if (!Application.isPlaying)
            return;

        var pc = FindObjectOfType<PlayerController>();
        if (pc != null)
            target = pc.transform;
    }

    /// <summary>
    /// Unity 不允许把 A 场景里的对象引用存进 B 场景的序列化数据。仅在编辑模式检查；
    /// 运行时用 Find 得到的跨场景引用是合法的，不可每帧清空（否则会刷警告且跟丢目标）。
    /// </summary>
    void ClearSerializedTargetIfCrossScene()
    {
        if (Application.isPlaying)
            return;
        if (target == null) return;
        if (!target.gameObject.scene.IsValid()) { target = null; return; }
        if (!gameObject.scene.IsValid()) return;

        if (target.gameObject.scene != gameObject.scene)
        {
            Debug.LogWarning(
                "[SmoothFollowCamera2D] Target is in a different scene than this camera; cross-scene references are not saved. " +
                "Leave Target empty and the player will be found at runtime. Clearing Target.",
                this);
            target = null;
        }
    }

    void OnValidate()
    {
        ClearSerializedTargetIfCrossScene();
        if (followResumeRadius > 0f && followResumeRadius < deadZoneRadius)
            followResumeRadius = deadZoneRadius * 1.35f;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        ClearSerializedTargetIfCrossScene();
        Transform t = target;
        if (t == null && !Application.isPlaying)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) t = pc.transform;
        }

        if (t == null) return;

        Vector3 ideal = t.position + followOffset;
        ideal.z = transform.position.z;
        Gizmos.color = new Color(0.3f, 0.85f, 0.4f, 0.35f);
        Gizmos.DrawWireSphere(ideal, deadZoneRadius);
        Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.25f);
        Gizmos.DrawWireSphere(ideal, EffectiveResumeRadius());
    }
#endif
}
