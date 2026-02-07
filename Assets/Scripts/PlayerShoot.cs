using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;

    public float fireRate = 6f;
    public float bulletSpeed = 10f;
    public int damage = 1;
    public float lifetime = 1f;

    private float nextShotTime;

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

        // направление по развороту лисы (flipX)
        var sr = GetComponent<SpriteRenderer>();
        bool facingLeft = sr != null && sr.flipX;
        Vector2 dir = facingLeft ? Vector2.left : Vector2.right;

        // настроим снаряд
        Projectile proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.speed = bulletSpeed;
            proj.damage = damage;
            proj.lifetime = lifetime;
            proj.SetDirection(dir);
        }
    }
}
