using UnityEngine;
using UnityEngine.Events;

public class MagicFlower : MonoBehaviour, IMagicInteractable
{
    public UnityEvent OnFlowerOpen;
    public UnityEvent OnFlowerClose;

    public void ApplyMagic(MagicEffectType type)
    {
        if (type == MagicEffectType.Flourish) OnFlowerOpen?.Invoke();
        else OnFlowerClose?.Invoke();
    }
}