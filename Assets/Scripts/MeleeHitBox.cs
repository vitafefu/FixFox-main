using UnityEngine;
using System.Collections.Generic;

public class MeleeHitbox : MonoBehaviour
{
    public int damage = 2;
    public float lifeTime = 0.08f;
    public string targetTag = "Enemy";

    [Header("Knockback")]
    public bool useKnockback = true;
    public float knockbackForce = 6f;

    private Vector2 direction = Vector2.right;
    private HashSet<int> hitIds = new HashSet<int>();

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.0001f)
            direction = dir.normalized;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag)) return;

        int key = other.attachedRigidbody
            ? other.attachedRigidbody.GetInstanceID()
            : other.GetInstanceID();

        if (hitIds.Contains(key)) return;
        hitIds.Add(key);

        Bettle enemy = other.GetComponentInParent<Bettle>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        if (useKnockback)
        {
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.AddForce(
                    new Vector2(direction.x * knockbackForce, 0f),
                    ForceMode2D.Impulse
                );
            }
        }
    }
}
