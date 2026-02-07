using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Bettle : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float patrolDistance = 3f;
    public bool startMovingRight = true;

    [Header("Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float attackRange = 5f;
    public float attackCooldown = 2f;

    [Header("Contact damage")]
    public int contactDamage = 1;
    public float playerKnockbackX = 12f;
    public float playerKnockbackY = 1f;
    public float knockbackDuration = 0.3f;

    [Header("Health")]
    public int health = 50;

    private Vector2 startPos;
    private bool movingRight;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;

    private float attackTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (anim != null && anim.runtimeAnimatorController == null)
            anim = null;

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        startPos = transform.position;
        movingRight = startMovingRight;
        attackTimer = attackCooldown;
    }

    void Start()
    {
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj != null ? playerObj.transform : null;
    }

    void Update()
    {
        if (player == null)
        {
            // если игрок появился позже (respawn/scene load) — попробуем найти
            FindPlayer();
            if (player == null) return;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        attackTimer -= Time.deltaTime;

        if (dist <= attackRange)
        {
            StopMove();
            FaceToPlayer();

            if (anim != null) anim.SetBool("isAttacking", true);

            if (attackTimer <= 0f)
            {
                Shoot();
                attackTimer = attackCooldown;
            }
        }
        else
        {
            if (anim != null) anim.SetBool("isAttacking", false);
            Patrol();
        }

        if (anim != null)
            anim.SetFloat("speed", Mathf.Abs(rb.velocity.x));
    }

    void StopMove()
    {
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    void FaceToPlayer()
    {
        bool playerIsRight = player.position.x > transform.position.x;

        // flipX: подстрой под свой спрайт (если надо — инвертнёшь)
        if (sr != null) sr.flipX = playerIsRight;
    }

    void Patrol()
    {
        float dir = movingRight ? 1f : -1f;
        rb.velocity = new Vector2(dir * speed, rb.velocity.y);

        // визуальный поворот при патруле
        if (sr != null) sr.flipX = movingRight;

        if (movingRight && transform.position.x >= startPos.x + patrolDistance)
            movingRight = false;

        if (!movingRight && transform.position.x <= startPos.x - patrolDistance)
            movingRight = true;
    }

    void Shoot()
    {
        if (anim != null) anim.SetTrigger("attack");

        if (projectilePrefab == null || firePoint == null) return;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        projectile.name = "Bullet_" + Time.time;

        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            // направление по flipX (подстрой если у тебя наоборот)
            Vector2 direction = (sr != null && sr.flipX) ? Vector2.right : Vector2.left;
            proj.owner = Projectile.Owner.Enemy;
            proj.SetDirection(direction);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(contactDamage);

        Player_Controller pc = collision.gameObject.GetComponent<Player_Controller>();
        if (pc != null)
        {
            pc.isKnockedBack = true;
            StartCoroutine(EnableControl(pc));
        }

        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            float dir = (collision.transform.position.x > transform.position.x) ? 1f : -1f;
            playerRb.velocity = new Vector2(
                dir * playerKnockbackX,
                Mathf.Min(playerRb.velocity.y + playerKnockbackY, 5f)
            );
        }
    }

    IEnumerator EnableControl(Player_Controller playerCtrl)
    {
        yield return new WaitForSeconds(knockbackDuration);
        if (playerCtrl != null) playerCtrl.isKnockedBack = false;
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;

        if (health <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(DamageFlash());
    }

    IEnumerator DamageFlash()
    {
        if (sr == null) yield break;

        Color old = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (sr != null) sr.color = old;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
