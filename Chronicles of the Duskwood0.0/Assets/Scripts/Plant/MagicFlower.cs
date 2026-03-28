using UnityEngine;
using UnityEngine.Events;

// 继承 IInteractable，这样 MagicProjectile 才能识别它
public class MagicFlower : MonoBehaviour, IInteractable
{
    [Header("事件触发")]
    public UnityEvent OnFlowerOpen;
    public UnityEvent OnFlowerClose;

    // 实现接口：繁盛响应
    public void OnFlourish()
    {
        Debug.Log($"<color=green>{gameObject.name} 开启！</color>");
        OnFlowerOpen?.Invoke();
    }

    // 实现接口：枯萎响应
    public void OnWither()
    {
        Debug.Log($"<color=gray>{gameObject.name} 关闭！</color>");
        OnFlowerClose?.Invoke();
    }
}