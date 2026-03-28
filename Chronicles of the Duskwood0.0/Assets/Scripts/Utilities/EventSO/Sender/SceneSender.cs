using UnityEngine;
using UnityEngine.AddressableAssets;
public class SceneSender : MonoBehaviour
{
    [Tooltip("要加载的场景引用")]
    public AssetReference scene;
    [Header("事件广播")]
    [Tooltip("对象事件SO")]
    public ObjectEventSO eventSO;
    public void SendEvent()
    {
        eventSO.RaiseEvent(scene, this);
    }
}
