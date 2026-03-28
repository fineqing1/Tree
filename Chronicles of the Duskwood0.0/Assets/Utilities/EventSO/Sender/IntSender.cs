using UnityEngine;
public class IntSender : MonoBehaviour
{
    [Tooltip("要发送的整数值")]
    public int count;
    [Header("事件广播")]
    [Tooltip("整型事件SO")]
    public IntEventSO eventSO;
    public void SendEvent()
    {
        eventSO.RaiseEvent(count, this);
    }
}
