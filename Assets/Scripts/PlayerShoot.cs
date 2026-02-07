using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Links")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public SpriteRenderer playerSprite; // <- назначь сюда спрайт лисы (который реально flipX)

    [Header("Shoot params")]
    public float fireRate = 6f;

    [Header("Optional override (если хочешь общие параметры)")]
    public bool overrideProjectileStats = true;
    public float bulletSpeed = 10f;
    public int damage = 1;
    public float lifetime = 1f;

    private float nextShotTime;

    void Awake()
    {
        // если не назначил Ч попробуем найти на себе или в дет€х
        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();
        if (playerSprite == null) playerSprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
        }
    }

    void Shoot()
    {
        if (firePoint == null || bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        bool facingLeft = playerSprite != null && playerSprite.flipX;
        Vector2 dir = facingLeft ? Vector2.left : Vector2.right;

        Projectile proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            // ¬—≈√ƒј берЄм из PlayerShoot (а значит из WeaponSwitch)
            proj.speed = bulletSpeed;
            proj.damage = damage;
            proj.lifetime = lifetime;
            proj.owner = Projectile.Owner.Player;
            proj.SetDirection(dir);
        }
    }
}
