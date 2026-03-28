// 机关基类
using UnityEngine;

public abstract class MechanismBase : MonoBehaviour, IInteractable
{
    public abstract void OnFlourish();
    public abstract void OnWither();
}
