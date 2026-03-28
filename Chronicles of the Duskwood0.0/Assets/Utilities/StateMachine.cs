using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;


// 状态机（只负责切换）
public class StateMachine
{
    private StateBase currentState;
    private Dictionary<Type, StateBase> stateDic = new Dictionary<Type, StateBase>();

    public void AddState(Type type, StateBase state)
    {
        if (!stateDic.ContainsKey(type))
            stateDic.Add(type, state);
    }
    public void ChangeState <T>() where T : StateBase
    {
        Type type = typeof(T);
        if (!stateDic.ContainsKey(type)) return;



        currentState?.Exit();
        currentState = stateDic[type];
        currentState?.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }
}

