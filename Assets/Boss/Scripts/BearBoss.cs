using UnityEngine;
using System.Collections;

public class BearBoss : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    public float speed = 1.5f;
    public float patrolDistance = 6f;
    public bool startMovingRight = true;

    [Header("Detect/Attack")]
    public float attackRange = 3f;
    public float attackCooldown = 1.5f;
    public int contactDamage = 1;

    [Header("Knockback player")]
    public float knockbackForce = 12f;
    public float knockbackDuration = 0.3f;

    [Header("Health")]
    public int maxHealth = 50;

    [Header("Animation params (optional)")]
    public string paramIsAttacking = "isAttacking";
    public string paramSpeed = "speed";
    public string triggerAttack = "attack";

    int health;

    Vector2 startPos;
    bool movingRight;

    SpriteRenderer sr;
    Rigidbody2D rb;
    Animator anim;
    Transform player;

    float attackTimer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        health = maxHealth;
    }

    void Start()
    {
        startPos = transform.position;
        movingRight = startMovingRight;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        attackTimer = 0f;
    }

    void Update()
    {
        if (!player) { Patrol(); return; }

        float dist = Vector2.Distance(transform.position, player.position);
        attackTimer -= Time.deltaTime;

        if (dist <= attackRange)
        {
            // стоп и поворот
            rb.velocity = new Vector2(0f, rb.velocity.y);

            bool playerIsRight = player.position.x > transform.position.x;
            if (sr) sr.flipX = playerIsRight;

            // анимации (если есть)
            if (anim && !string.IsNullOrEmpty(paramIsAttacking))
                anim.SetBool(paramIsAttacking, true);
            if (anim && !string.IsNullOrEmpty(paramSpeed))
                anim.SetFloat(paramSpeed, 0f);

            // атака по кд
            if (attackTimer <= 0f)
            {
                attackTimer = attackCooldown;

                if (anim && !string.IsNullOrEmpty(triggerAttack))
                    anim.SetTrigger(triggerAttack);

                // —јћј ближн€€ атака у теб€ сейчас = контактный урон в OnCollisionEnter2D
                // “ут просто синхроним с анимацией/кд.
            }
        }
        else
        {
            // патруль
            Patrol();

            if (anim && !string.IsNullOrEmpty(paramIsAttacking))
                anim.SetBool(paramIsAttacking, false);
            if (anim && !string.IsNullOrEmpty(paramSpeed))
                anim.SetFloat(paramSpeed, Mathf.Abs(rb.velocity.x));
        }
    }

    void Patrol()
    {
        if (!rb) return;

        if (movingRight)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
            if (sr) sr.flipX = true;

            if (transform.position.x >= startPos.x + patrolDistance)
                movingRight = false;
        }
        else
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            if (sr) sr.flipX = false;

            if (transform.position.x <= startPos.x - patrolDistance)
                movingRight = true;
        }

        if (anim && !string.IsNullOrEmpty(paramSpeed))
            anim.SetFloat(paramSpeed, Mathf.Abs(rb.velocity.x));
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

        Rigidbody2D prb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (prb != null)
        {
            float dir = (collision.transform.position.x > transform.position.x) ? 1f : -1f;
            prb.velocity = new Vector2(dir * knockbackForce, Mathf.Min(prb.velocity.y + 1f, 5f));
        }
    }

    IEnumerator EnableControl(Player_Controller pc)
    {
        yield return new WaitForSeconds(knockbackDuration);
        if (pc != null) pc.isKnockedBack = false;
    }

    // ===== IDamageable =====
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
        else
        {
            if (sr != null) StartCoroutine(DamageFlash());
        }
    }

    IEnumerator DamageFlash()
    {
        if (!sr) yield break;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        if (sr) sr.color = Color.white;
    }

    void Die()
    {
        // тут можно: дроп, звук, анимаци€ смерти
        Destroy(gameObject);
    }
}
