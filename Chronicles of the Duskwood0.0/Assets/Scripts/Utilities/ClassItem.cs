using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[Serializable]
public class ListenerData<T>
{
    [Tooltip("要监听的事件SO列表")]
    public List<BaseEventSO<T>> eventSOList;
    [Tooltip("事件响应")]
    public UnityEvent<T> response;
}
