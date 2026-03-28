using System.Collections.Generic;
using UnityEngine;

public class BaseEventListener<T> : MonoBehaviour
{
    [Tooltip("监听数据列表")]
    public List<ListenerData<T>> listenerDataList;
    
    void OnEnable()
    {
        foreach (var listenerData in listenerDataList)
        {
            foreach (var eventSO in listenerData.eventSOList)
            {
                eventSO.OnEventRaised += (value) => OnEventRaisedForData(listenerData, value);
            }
        }
    }
    
    void OnDisable()
    {
        foreach (var listenerData in listenerDataList)
        {
            foreach (var eventSO in listenerData.eventSOList)
            {
                eventSO.OnEventRaised -= (value) => OnEventRaisedForData(listenerData, value);
            }
        }
    }
    private void OnEventRaisedForData(ListenerData<T> listenerData, T value)
    {
        listenerData.response.Invoke(value);
    }
}
