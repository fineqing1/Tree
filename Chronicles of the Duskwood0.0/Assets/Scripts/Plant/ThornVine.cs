using UnityEngine;

public class ThornVine : MonoBehaviour, IMagicInteractable
{
    public Collider2D thornCollider;
    public SpriteRenderer thornRenderer;
    public float damage = 10f;

    public void ApplyMagic(MagicEffectType type)
    {
        bool isFlourish = (type == MagicEffectType.Flourish);
        thornCollider.enabled = isFlourish;
        thornRenderer.color = isFlourish ? Color.white : new Color(1, 1, 1, 0.3f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>()?.TakeDamage(damage);
        }
    }
}