using UnityEngine;
public interface IInteractable
{
    void OnFlourish(); // ��ʢ��Ӧ
    void OnWither();   // ��ή��Ӧ
}
[RequireComponent(typeof(Rigidbody2D))]
public class MagicProjectile : MonoBehaviour
{
    public enum MagicType { Flourish, Wither }
    public MagicType type;

    [Header("Attributes")]
    public float originalSpeed = 10f;
    public float accelerate = -2f; // ����ļ��ٻ�ͨ������ʩ�Ӹ������ٶ�
    public float lifeTime = 5f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction)
    {
        // ֱ�Ӹ���������һ�����ٶ�
        rb.velocity = direction.normalized * originalSpeed;
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate() // ������صļ������ FixedUpdate
    {
        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            // 1. ʵ�ּ����߼� (1.1.2.1.1.1.2)
            float speed = rb.velocity.magnitude;
            speed += accelerate * Time.fixedDeltaTime;
            if (speed < 0) speed = 0;
            rb.velocity = rb.velocity.normalized * speed;

            // 2. ��תָ����з���
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // ע�⣺����Ч�����������ʴ���������ֻ������ײ�����ء����߼�
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. �������
        if (collision.gameObject.CompareTag("Player")) return;

        // 2. ���Ի�ȡ���ؽӿ�
        IInteractable interactable = collision.gameObject.GetComponent<IInteractable>();
        if (interactable != null)
        {
            if (type == MagicType.Flourish) interactable.OnFlourish();
            else interactable.OnWither();

            // ײ������ͨ������Ҫ������ֱ����Ч������
            Destroy(gameObject);
        }

        // ��ʾ��ײ����ͨǽ��ʱ���������ʻ��Զ���������������Ҫ������д��������
    }
}

