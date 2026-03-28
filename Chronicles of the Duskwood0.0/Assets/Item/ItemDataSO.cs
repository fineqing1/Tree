using UnityEngine;

public enum ItemType { ManaBall, Fruit }

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
public class ItemDataSO : ScriptableObject
{
    public int id;
    public string itemName;
    public ItemType type;
    public int value; // 恢复的值（HP或Fuel）
    public GameObject vfxPrefab; // 捡起时的特效
}