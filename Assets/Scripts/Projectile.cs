using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum Owner { Player, Enemy }
    [Header("Who fired this projectile?")]
    public Owner owner = Owner.Player;

    public float speed = 8f;
    public int damage = 1;
    public float lifetime = 1f;

    private Vector2 direction;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = GetComponent<Collider2D>();
        if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (direction != Vector2.zero)
            rb.velocity = direction * speed;
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Игнор своих
        if (owner == Owner.Player && other.CompareTag("Player")) return;
        if (owner == Owner.Enemy && other.CompareTag("Enemy")) return;

        // Попадание по цели
        if (owner == Owner.Player)
        {
            // бьём врага
            IDamageable dmg = other.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }
        else // Owner.Enemy
        {
            // бьём игрока
            var ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        // Стены/земля/платформы — уничтожаем
        //if (other.CompareTag("Ground") || other.CompareTag("Platform") || other.CompareTag("Wall"))
        //{
        //    Destroy(gameObject);
        //    return;
        //}
    }
}
