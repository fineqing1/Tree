using UnityEngine;
/*
public class TreePlatform : MonoBehaviour, IMagicInteractable
{
    [Header("¸ß¶ÈÉèÖÃ")]
    public float maxHeight = 5f;
    public float minHeight = 0f;
    public float moveSpeed = 2f;

    private float targetHeight;
    private Vector3 initialPos;

    private void Awake() => initialPos = transform.position;

    public void ApplyMagic(MagicEffectType type)
    {
        if (type == MagicEffectType.Flourish) targetHeight = maxHeight;
        else targetHeight = minHeight;
    }

    private void Update()
    {
        Vector3 targetPos = initialPos + Vector3.up * targetHeight;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
    }
}*/