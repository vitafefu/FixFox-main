using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    Rigidbody2D rb;
    Player_Controller controller;

    [Header("Links")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public SpriteRenderer playerSprite;

    [Header("Shoot params")]
    public float fireRate = 6f;

    [Header("Accuracy")]
    public float baseSpread = 0f;          // идеальная точность
    public float maxSpread = 18f;           // максимальный угол
    public float spreadIncrease = 1.95f;    // рост за выстрел
    public float spreadRecovery = 5f;       // восстановление в секунду

    [Header("Movement spread")]
    public float moveSpread = 14f;      // доп. разброс при беге
    public float airSpread = 17f;      // доп. разброс в прыжке
    public float moveSpeedThreshold = 0.1f;

    [Header("Optional override (если хочешь общие параметры)")]
    public bool overrideProjectileStats = true;
    public float bulletSpeed = 10f;
    public int damage = 1;
    public float lifetime = 1f;

    private float nextShotTime;

    float currentSpread = 0f;

    void Awake()
    {
        // если не назначил — попробуем найти на себе или в детях
        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();
        if (playerSprite == null) playerSprite = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<Player_Controller>();

    }

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
        }
        float targetSpread = baseSpread;

        // бег
        if (rb != null && Mathf.Abs(rb.velocity.x) > moveSpeedThreshold)
        {
            targetSpread += moveSpread;
        }

        // прыжок
        if (controller != null && !controller.IsGrounded)
        {
            targetSpread += airSpread;
        }

        // плавно тянем currentSpread к нужному
        currentSpread = Mathf.MoveTowards(
            currentSpread,
            targetSpread,
            spreadRecovery * Time.deltaTime
        );

    }

    void Shoot()
    {
        if (firePoint == null || bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        bool facingLeft = playerSprite != null && playerSprite.flipX;

        Vector2 baseDir = facingLeft ? Vector2.left : Vector2.right;

        float spread = Random.Range(-currentSpread, currentSpread);
        Vector2 dir = Quaternion.Euler(0, 0, spread) * baseDir;

        // накапливаем разброс за выстрел
        currentSpread += spreadIncrease;
        currentSpread = Mathf.Clamp(currentSpread, baseSpread, maxSpread);

        Projectile proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.speed = bulletSpeed;
            proj.damage = damage;
            proj.lifetime = lifetime;
            proj.owner = Projectile.Owner.Player;
            proj.SetDirection(dir);
        }
    }

}
