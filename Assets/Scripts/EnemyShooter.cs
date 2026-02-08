using UnityEngine;
using System.Collections;

public class EnemyShooter : MonoBehaviour
{
    public enum Pattern { Single, Burst, Spread, Wave }

    [Header("Links")]
    public Transform firePoint;
    public GameObject projectilePrefab;
    public SpriteRenderer enemySprite; // если хочешь стрел€ть Упо взгл€дуФ

    [Header("Target")]
    public Transform player;
    public bool aimAtPlayer = true;     // если true Ч целитс€ в игрока
    public bool useFacingDirection = false; // если true Ч стрел€ет по flipX

    [Header("Pattern")]
    public Pattern pattern = Pattern.Single;

    [Header("Timing")]
    public float cooldown = 1.5f;

    [Header("Projectile")]
    public float bulletSpeed = 8f;
    public int damage = 1;
    public float lifetime = 2f;

    [Header("Burst")]
    public int burstCount = 3;
    public float burstInterval = 0.12f;

    [Header("Spread")]
    public int spreadCount = 5;
    public float spreadAngle = 35f; // общий угол веера

    [Header("Wave")]
    public int waveShots = 6;
    public float waveAngle = 25f;
    public float waveInterval = 0.08f;

    float nextTime;

    void Awake()
    {
        if (enemySprite == null) enemySprite = GetComponent<SpriteRenderer>();
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Update()
    {
        if (Time.time < nextTime) return;
        if (firePoint == null || projectilePrefab == null) return;

        // можно сделать активацию по дистанции, но пока просто стрел€ет по таймеру
        nextTime = Time.time + cooldown;

        switch (pattern)
        {
            case Pattern.Single:
                Fire(GetBaseDir());
                break;

            case Pattern.Burst:
                StartCoroutine(BurstRoutine());
                break;

            case Pattern.Spread:
                FireSpread();
                break;

            case Pattern.Wave:
                StartCoroutine(WaveRoutine());
                break;
        }
    }

    Vector2 GetBaseDir()
    {
        if (useFacingDirection && enemySprite != null)
            return enemySprite.flipX ? Vector2.right : Vector2.left;

        if (aimAtPlayer && player != null)
            return ((Vector2)(player.position - firePoint.position)).normalized;

        // по умолчанию Ч влево (как враги справа налево)
        return Vector2.left;
    }

    void Fire(Vector2 dir)
    {
        var bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        var proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.speed = bulletSpeed;
            proj.damage = damage;
            proj.lifetime = lifetime;
            proj.owner = Projectile.Owner.Enemy; // важно
            proj.SetDirection(dir);
        }
        else
        {
            // если вдруг без Projectile Ч просто двинем трансформ
            var rb = bullet.GetComponent<Rigidbody2D>();
            if (rb) rb.velocity = dir * bulletSpeed;
            Destroy(bullet, lifetime);
        }
    }

    IEnumerator BurstRoutine()
    {
        Vector2 dir = GetBaseDir();
        for (int i = 0; i < burstCount; i++)
        {
            Fire(dir);
            yield return new WaitForSeconds(burstInterval);
        }
    }

    void FireSpread()
    {
        Vector2 baseDir = GetBaseDir();

        if (spreadCount <= 1)
        {
            Fire(baseDir);
            return;
        }

        float start = -spreadAngle * 0.5f;
        float step = spreadAngle / (spreadCount - 1);

        for (int i = 0; i < spreadCount; i++)
        {
            float a = start + step * i;
            Vector2 dir = (Quaternion.Euler(0, 0, a) * baseDir).normalized;
            Fire(dir);
        }
    }

    IEnumerator WaveRoutine()
    {
        Vector2 baseDir = GetBaseDir();

        for (int i = 0; i < waveShots; i++)
        {
            float t = (waveShots <= 1) ? 0f : (i / (float)(waveShots - 1));
            float a = Mathf.Sin(t * Mathf.PI * 2f) * waveAngle;

            Vector2 dir = (Quaternion.Euler(0, 0, a) * baseDir).normalized;
            Fire(dir);

            yield return new WaitForSeconds(waveInterval);
        }
    }
}
