// 机关基类
using UnityEngine;

public abstract class MechanismBase : MonoBehaviour, IInteractable
{
    public abstract void OnFlourish();
    public abstract void OnWither();
}

// 示例：乔木
public class ArborTree : MechanismBase
{
    public float heightStep = 1f;

    public override void OnFlourish()
    {
        // 提升高度，阻塞上方
        transform.position += Vector3.up * heightStep;
        Debug.Log("乔木生长：提升了高度");
    }

    public override void OnWither()
    {
        // 降低高度
        transform.position -= Vector3.up * heightStep;
        Debug.Log("乔木萎缩：降低了高度");
    }
}

// 示例：速生藤
public class FastVine : MechanismBase
{
    public GameObject platformToLift;

    public override void OnFlourish()
    {
        // 这里的逻辑可以是拉起平台
        if (platformToLift != null) platformToLift.transform.position += Vector3.up * 2f;
    }

    public override void OnWither()
    {
        // 这里的逻辑可以是让藤蔓消失，平台掉落
        Destroy(gameObject);
    }
}