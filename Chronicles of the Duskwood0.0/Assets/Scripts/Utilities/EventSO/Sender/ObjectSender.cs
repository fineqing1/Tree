using UnityEngine;
public class ObjectSender : MonoBehaviour
{
    public Object _object;
    [Header("事件广播")]
    public ObjectEventSO eventSO;
    public void SendEvent()
    {
        eventSO.RaiseEvent(_object, this);
    }
}
