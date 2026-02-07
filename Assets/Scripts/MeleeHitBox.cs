using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    public int damage = 2;
    public float lifeTime = 0.08f;   // сколько живёт хитбокс
    public string targetTag = "Enemy";

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag)) return;

    }
}
