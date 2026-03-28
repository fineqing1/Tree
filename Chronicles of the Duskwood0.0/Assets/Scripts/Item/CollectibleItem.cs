using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [SerializeField] private ItemDataSO data;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 尝试获取玩家组件
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player != null)
        {
            ApplyEffect(player);

            // 生成特效并销毁自己
            if (data.vfxPrefab != null)
                Instantiate(data.vfxPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }

    private void ApplyEffect(PlayerController player)
    {
        switch (data.type)
        {
            case ItemType.ManaBall:
                // 2.2.1.2 规则：捡到时玩家fuel += value
                player.currentFuel = Mathf.Min(player.currentFuel + data.value, player.maxFuel);
                Debug.Log($"捡到光球，恢复法力: {data.value}");
                break;

            case ItemType.Fruit:
                // 2.2.2.2 规则：捡到时玩家HP += value
                player.currentHP = Mathf.Min(player.currentHP + data.value, player.maxHp);
                Debug.Log($"捡到果实，恢复生命: {data.value}");
                break;
        }
    }
}