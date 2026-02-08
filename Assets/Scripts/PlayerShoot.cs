using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Links")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public SpriteRenderer playerSprite;

    [Header("Shoot params")]
    public float fireRate = 6f;

    [Header("8-direction aim")]
    public bool allowShootDown = true;     // Contra часто = false
    public bool allowShootUp = true;

    [Header("Accuracy")]
    public float baseSpread = 0f;
    public float maxSpread = 8f;
    public float spreadIncrease = 1.2f;
    public float spreadRecovery = 6f;

    [Header("Optional projectile stats")]
    public bool overrideProjectileStats = true;
    public float bulletSpeed = 10f;
    public int damage = 1;
    public float lifetime = 1f;

    private float nextShotTime;
    private float currentSpread = 0f;

    void Awake()
    {
        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();
        if (playerSprite == null) playerSprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // восстановление разброса
        currentSpread -= spreadRecovery * Time.deltaTime;
        currentSpread = Mathf.Max(baseSpread, currentSpread);

        if (Input.GetButton("Fire1") && Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
        }
    }

    Vector2 GetAimDir()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // ограничители (по желанию)
        if (!allowShootDown && v < 0) v = 0;
        if (!allowShootUp && v > 0) v = 0;

        Vector2 dir = new Vector2(h, v);

        // если не целимся — стреляем туда, куда смотрим
        if (dir == Vector2.zero)
        {
            bool facingLeft = playerSprite != null && playerSprite.flipX;
            dir = facingLeft ? Vector2.left : Vector2.right;
        }

        return dir.normalized;
    }

    void Shoot()
    {
        if (firePoint == null || bulletPrefab == null) return;

        Vector2 baseDir = GetAimDir();

        // разброс вокруг baseDir
        float spread = Random.Range(-currentSpread, currentSpread);
        Vector2 dir = (Quaternion.Euler(0, 0, spread) * baseDir).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Projectile proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            if (overrideProjectileStats)
            {
                proj.speed = bulletSpeed;
                proj.damage = damage;
                proj.lifetime = lifetime;
            }

            proj.owner = Projectile.Owner.Player;
            proj.SetDirection(dir);
        }

        // накапливаем разброс от очереди
        currentSpread += spreadIncrease;
        currentSpread = Mathf.Clamp(currentSpread, baseSpread, maxSpread);
    }
}

//using UnityEngine;

//public class PlayerShoot : MonoBehaviour
//{
//    [Header("Links")]
//    public Transform firePoint;
//    public GameObject bulletPrefab;
//    public SpriteRenderer playerSprite;

//    [Header("Shoot params")]
//    public float fireRate = 6f;

//    [Header("Aim mode")]
//    public KeyCode aimKey = KeyCode.LeftShift; // держи для 8 направлений
//    public bool allowShootDown = false;        // Contra-стиль (вниз нельзя)

//    [Header("Accuracy")]
//    public float baseSpread = 0f;
//    public float maxSpread = 8f;
//    public float spreadIncrease = 1.2f;
//    public float spreadRecovery = 6f;

//    [Header("Optional projectile stats")]
//    public bool overrideProjectileStats = true;
//    public float bulletSpeed = 10f;
//    public int damage = 1;
//    public float lifetime = 1f;

//    private float nextShotTime;
//    private float currentSpread = 0f;

//    void Awake()
//    {
//        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();
//        if (playerSprite == null) playerSprite = GetComponentInChildren<SpriteRenderer>();
//    }

//    void Update()
//    {
//        // восстановление разброса
//        currentSpread -= spreadRecovery * Time.deltaTime;
//        currentSpread = Mathf.Max(baseSpread, currentSpread);

//        if (Input.GetButton("Fire1") && Time.time >= nextShotTime)
//        {
//            Shoot();
//            nextShotTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
//        }
//    }

//    Vector2 GetAimDir()
//    {
//        bool isAiming = Input.GetKey(aimKey);

//        // если НЕ целимся — всегда стреляем вперёд (куда смотрит персонаж)
//        if (!isAiming)
//        {
//            bool facingLeft = playerSprite != null && playerSprite.flipX;
//            return facingLeft ? Vector2.left : Vector2.right;
//        }

//        // AIM MODE (8 направлений)
//        float h = Input.GetAxisRaw("Horizontal");
//        float v = Input.GetAxisRaw("Vertical");

//        if (!allowShootDown && v < 0) v = 0;

//        Vector2 dir = new Vector2(h, v);

//        // если aim зажат, но направления нет — тоже вперёд
//        if (dir == Vector2.zero)
//        {
//            bool facingLeft = playerSprite != null && playerSprite.flipX;
//            dir = facingLeft ? Vector2.left : Vector2.right;
//        }

//        return dir.normalized;
//    }

//    void Shoot()
//    {
//        if (firePoint == null || bulletPrefab == null) return;

//        Vector2 baseDir = GetAimDir();

//        // разброс вокруг baseDir
//        float spread = Random.Range(-currentSpread, currentSpread);
//        Vector2 dir = (Quaternion.Euler(0, 0, spread) * baseDir).normalized;

//        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

//        Projectile proj = bullet.GetComponent<Projectile>();
//        if (proj != null)
//        {
//            if (overrideProjectileStats)
//            {
//                proj.speed = bulletSpeed;
//                proj.damage = damage;
//                proj.lifetime = lifetime;
//            }

//            proj.owner = Projectile.Owner.Player;
//            proj.SetDirection(dir);
//        }

//        // накапливаем разброс от очереди
//        currentSpread += spreadIncrease;
//        currentSpread = Mathf.Clamp(currentSpread, baseSpread, maxSpread);
//    }
//}

