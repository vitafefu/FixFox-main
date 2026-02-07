using UnityEngine;

public class PlayerMelee : MonoBehaviour
{
    public Transform meleePoint;
    public GameObject hitboxPrefab;

    public float attackCooldown = 0.25f;
    public int damage = 2;

    private float nextAttackTime;

    public bool canAttack = true; // WeaponAirBlock будет выключать в прыжке

    void Update()
    {
        if (!canAttack) return;

        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void Attack()
    {
        if (meleePoint == null || hitboxPrefab == null) return;

        var go = Instantiate(hitboxPrefab, meleePoint.position, Quaternion.identity);
        var hb = go.GetComponent<MeleeHitbox>();
        if (hb != null) hb.damage = damage;

        // направление (влево/вправо) Ч хитбокс зеркалим по игроку
        var sr = GetComponent<SpriteRenderer>();
        bool left = sr != null && sr.flipX;
        go.transform.localScale = new Vector3(left ? -1f : 1f, 1f, 1f);
    }
}
