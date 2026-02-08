using UnityEngine;
using System.Collections;

public class PlayerStaff : MonoBehaviour
{
    [Header("Links (can be auto-found)")]
    public Transform meleePoint;
    public GameObject hitboxPrefab;

    public Transform firePoint;
    public GameObject projectilePrefab;

    public SpriteRenderer playerSprite;
    public Animator anim;

    [Header("Animator (optional)")]
    public string attackTrigger = "melee"; // если нет Ч можно оставить, логика всЄ равно работает

    [Header("Melee")]
    public int meleeDamage = 2;

    [Header("Ranged (wave)")]
    public float bulletSpeed = 10f;
    public int bulletDamage = 1;
    public float bulletLifetime = 1.2f;

    [Header("Timing")]
    public float attackCooldown = 0.5f;
    public float shootDelay = 0.08f;

    float nextTime;
    Coroutine routine;

    void Awake()
    {
        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();

        // авто-поиск точек (если не назначил руками)
        if (meleePoint == null)
        {
            var t = transform.Find("MeleePoint");
            if (t != null) meleePoint = t;
        }

        if (firePoint == null)
        {
            var t = transform.Find("Weapon Socket/FirePoint");
            if (t != null) firePoint = t;
        }
    }

    void OnEnable()
    {
        // чтобы при переключении оружи€ не было УзалипшихФ корутин
        if (routine != null) StopCoroutine(routine);
        routine = null;
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
    }

    void Update()
    {
        if (Time.time < nextTime) return;

        if (Input.GetButtonDown("Fire1"))
        {
            nextTime = Time.time + attackCooldown;

            // анимаци€ Ч опционально
            if (anim != null && !string.IsNullOrEmpty(attackTrigger))
                anim.SetTrigger(attackTrigger);

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(DoStaffAttack());
        }
    }

    IEnumerator DoStaffAttack()
    {

        yield return new WaitForSeconds(0.37f);

        SpawnHitbox();

        yield return new WaitForSeconds(shootDelay);
        SpawnProjectile();
    }


    void SpawnHitbox()
    {
        if (meleePoint == null)
        {
            return;
        }
        if (hitboxPrefab == null)
        {
            return;
        }

        bool left = playerSprite != null && playerSprite.flipX;

        var go = Instantiate(hitboxPrefab, meleePoint.position, Quaternion.identity);
        go.transform.localScale = new Vector3(left ? -1f : 1f, 1f, 1f);

        var hb = go.GetComponent<MeleeHitbox>();
        if (hb != null)
        {
            hb.damage = meleeDamage;
            hb.SetDirection(left ? Vector2.left : Vector2.right);
        }
    }

    void SpawnProjectile()
    {
        if (firePoint == null)
        {
            return;
        }
        if (projectilePrefab == null)
        {
            return;
        }

        bool left = playerSprite != null && playerSprite.flipX;
        Vector2 dir = left ? Vector2.left : Vector2.right;

        var bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        var proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.speed = bulletSpeed;
            proj.damage = bulletDamage;
            proj.lifetime = bulletLifetime;
            proj.owner = Projectile.Owner.Player;
            proj.SetDirection(dir);
        }
    }
}
