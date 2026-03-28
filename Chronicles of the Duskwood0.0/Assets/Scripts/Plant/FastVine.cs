using UnityEngine;
using System.Collections;

public class FastVine : MonoBehaviour, IMagicInteractable
{
    [Header("生长属性")]
    public float maxLong = 5f;        // 最大长度
    public float growSpeed = 2f;      // 生长速度

    [Header("引用")]
    public Transform vineVisual;      // 藤蔓的视觉物体（确保其 Pivot 在顶部）
    public DistanceJoint2D connectedJoint; // 连接平台的物理组件

    private float currentLength = 0f;
    private Coroutine growCoroutine;
    private bool isFlourishing = false;

    private void Awake()
    {
        // 初始状态：长度为0，关闭物理连接
        if (vineVisual != null) vineVisual.localScale = new Vector3(1, 0, 1);
        if (connectedJoint != null) connectedJoint.enabled = false;
    }

    public void ApplyMagic(MagicEffectType type)
    {
        if (type == MagicEffectType.Flourish)
        {
            if (!isFlourishing)
            {
                StopActiveCoroutine();
                growCoroutine = StartCoroutine(GrowRoutine(maxLong));
                isFlourishing = true;
            }
        }
        else // Wither
        {
            if (isFlourishing)
            {
                StopActiveCoroutine();
                growCoroutine = StartCoroutine(GrowRoutine(0f));
                isFlourishing = false;
            }
        }
    }

    private IEnumerator GrowRoutine(float targetLength)
    {
        while (!Mathf.Approximately(currentLength, targetLength))
        {
            // 平滑改变当前长度
            currentLength = Mathf.MoveTowards(currentLength, targetLength, growSpeed * Time.deltaTime);

            // 更新视觉：假设藤蔓是垂直向下长的，修改 Y 轴缩放
            if (vineVisual != null)
            {
                // 注意：这里假设你的 Sprite 默认高度是 1 单位
                vineVisual.localScale = new Vector3(1, currentLength, 1);
            }

            // 更新物理：如果是 DistanceJoint，我们可以动态改变其距离
            if (connectedJoint != null)
            {
                if (currentLength > 0.1f) // 稍微长出来一点再开启物理
                {
                    connectedJoint.enabled = true;
                    // 让 Joint 的长度跟随藤蔓长度，达到“吊起”或“放下”的效果
                    connectedJoint.distance = currentLength;
                }
                else
                {
                    connectedJoint.enabled = false;
                }
            }

            yield return null;
        }

        // 枯萎到最后直接关闭物理，防止物理残留
        if (currentLength <= 0.01f && connectedJoint != null)
        {
            connectedJoint.enabled = false;
        }
    }

    private void StopActiveCoroutine()
    {
        if (growCoroutine != null) StopCoroutine(growCoroutine);
    }
}