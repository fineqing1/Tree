using UnityEngine;
using System.Collections;

// 必须实现 IInteractable 接口才能接收交互
public class FastVine : MonoBehaviour, IInteractable
{
    [Header("生长配置")]
    [Tooltip("勾选则向上顶(地板往天长)，不勾选则向下吊(天花板往地长)")]
    public bool isUpward = false;
    [Tooltip("初始状态那一小截藤蔓的长度，建议设为 1")]
    public float minLength = 1f;
    [Tooltip("繁茂状态下的最大长度")]
    public float maxLength = 5f;
    [Tooltip("生长/枯萎的速度")]
    public float growSpeed = 5f;

    [Header("引用")]
    [Tooltip("T字的竖线：Pivot 必须在顶部(吊)或底部(顶)")]
    public Transform vineStem;
    [Tooltip("T字的横杠平台：Rigidbody2D 必须设为 Kinematic，且删掉 Joint 组件")]
    public Transform platformTop;

    private float currentLength;        // 当前长度
    private Coroutine growCoroutine;
    private bool isFlourishing = false; // 当前是否处于繁茂（伸长）状态

    private void Awake()
    {
        // 确保物体本身有 Collider 2D 用于接收魔法球交互
        if (GetComponent<Collider2D>() == null && vineStem == null)
        {
            Debug.LogWarning("FastVine物体本身或VineStem子物体缺少Collider2D，玩家可能无法发射魔法球命中交互！");
        }

        // 初始化长度为那一小截
        currentLength = minLength;
        // 初始确保坐标对齐
        UpdateVineVisual(currentLength);
    }

    // --- 临时按键测试：方便你在没有魔法球时测试功能 ---
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) // 按 H 键触发生长
        {
            OnFlourish();
        }
        if (Input.GetKeyDown(KeyCode.J)) // 按 J 键触发枯萎
        {
            OnWither();
        }
    }

    // 实现 IInteractable 的 OnFlourish 方法
    public void OnFlourish()
    {
        if (!isFlourishing)
        {
            StopActiveCoroutine();
            growCoroutine = StartCoroutine(GrowRoutine(maxLength));
            isFlourishing = true;
            Debug.Log("<color=green>藤蔓接受魔法，开始向上/下喷涌生长...</color>");
        }
    }

    // 实现 IInteractable 的 OnWither 方法
    public void OnWither()
    {
        if (isFlourishing)
        {
            StopActiveCoroutine();
            growCoroutine = StartCoroutine(GrowRoutine(minLength));
            isFlourishing = false;
            Debug.Log("<color=gray>藤蔓受到凋零，缩回至初始小节...</color>");
        }
    }

    private IEnumerator GrowRoutine(float targetLength)
    {
        while (!Mathf.Approximately(currentLength, targetLength))
        {
            currentLength = Mathf.MoveTowards(currentLength, targetLength, growSpeed * Time.deltaTime);
            UpdateVineVisual(currentLength);
            yield return null;
        }
    }

    private void UpdateVineVisual(float length)
    {
        // 1. 竖线生长：只要图片的 Pivot 设置正确.
        if (vineStem != null)
        {
            vineStem.localScale = new Vector3(1, length, 1);
        }

        // 2. 【核心修复】横杠位置同步：完全由代码驱动坐标，放弃Joint
        if (platformTop != null)
        {
            // 假设父物体(0,0)，如果是上顶，平台在(0, length)；如果是下吊，平台在(0, -length)
            float yPos = isUpward ? length : -length;
            platformTop.localPosition = new Vector3(0, yPos, 0);

            // 初始 minLength 时是否显示平台由你决定
            platformTop.gameObject.SetActive(true);
        }
    }

    private void StopActiveCoroutine()
    {
        if (growCoroutine != null) StopCoroutine(growCoroutine);
    }
}