using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;

public interface IState
{
    void OnEnter();
    void OnUpdate();
    void OnExit();
}

// 状态机（只负责切换）
public class StateMachine
{
    private IState currentState;
    private Dictionary<Type, IState> stateDic = new Dictionary<Type, IState>();

    public void AddState(Type type, IState state)
    {
        if (!stateDic.ContainsKey(type))
            stateDic.Add(type, state);
    }
    public void ChangeState <T>() where T : IState
    {
        Type type = typeof(T);
        if (!stateDic.ContainsKey(type)) return;



        currentState?.OnExit();
        currentState = stateDic[type];
        currentState?.OnEnter();
    }

    public void Update()
    {
        currentState?.OnUpdate();
    }
}

