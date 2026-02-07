using UnityEngine;
using System.Collections;

public class SlimeEnemy : MonoBehaviour, IDamageable

{
    // ====================== ПАРАМЕТРЫ ДВИЖЕНИЯ ======================
    [Header("Movement")]
    public float speed = 1.4f;              // Скорость слизня
    public float patrolDistance = 8f;       // Дистанция патрулирования
    public bool startMovingRight = true;    // Начинает движение вправо

    // ====================== ПАРАМЕТРЫ АТАКИ ======================
    [Header("Attack")]
    public float attackRange = 6f;          // Дистанция обнаружения игрока
    public float attackCooldown = 2f;       // Перезарядка между атаками
    public int contactDamage = 1;           // Урон при касании игрока

    [Header("Knockback")]
    public float knockbackForce = 10f;       // Сила отбрасывания игрока
    public float knockbackDuration = 0.3f;  // Длительность отбрасывания

    // ====================== ПАРАМЕТРЫ ЗДОРОВЬЯ ======================
    [Header("Health")]
    public int health = 40;                 // Здоровье слизня

    // ====================== ПРИВАТНЫЕ ПЕРЕМЕННЫЕ ======================
    private Vector2 startPos;               // Стартовая позиция для патрулирования
    private bool movingRight;               // Направление движения
    private SpriteRenderer sr;              // Для поворота спрайта
    private Transform player;               // Ссылка на игрока
    private float attackTimer;              // Таймер для атаки
    private Rigidbody2D rb;                 // Физическое тело
    private Animator anim;                  // Аниматор

    // ====================== НАЧАЛЬНАЯ ИНИЦИАЛИЗАЦИЯ ======================
    void Start()
    {
        startPos = transform.position;
        movingRight = startMovingRight;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Найти игрока
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("Player not found!");

        // Начинаем с полным таймером
        attackTimer = attackCooldown;
    }

    // ====================== ОСНОВНОЙ ЦИКЛ ОБНОВЛЕНИЯ ======================
    void Update()
    {
        if (player == null) return;

        if (anim != null) anim.speed = 0.5f; // Анимация движения замедлена

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        attackTimer -= Time.deltaTime;

        // Если игрок в радиусе атаки
        if (distanceToPlayer <= attackRange)
        {
            // Останавливаемся
            rb.velocity = Vector2.zero;

            // Поворачиваемся к игроку
            bool playerIsRight = player.position.x > transform.position.x;
            sr.flipX = playerIsRight;

            // Анимация: остановка и атака
            if (anim != null)
            {
                anim.SetFloat("speed", 0);
                anim.SetBool("isAttacking", true);
            }

            // БЛИЖНЯЯ АТАКА - если перезарядка прошла
            if (attackTimer <= 0f)
            {
                Debug.Log("Слизень атакует!");
                attackTimer = attackCooldown;
            }
        }
        else
        {
            // Патрулируем если игрок далеко
            Patrol();

            // Анимация: ходьба
            if (anim != null)
            {
                anim.SetBool("isAttacking", false);
                anim.SetFloat("speed", Mathf.Abs(rb.velocity.x));
            }
        }
    }

    // ====================== ПАТРУЛИРОВАНИЕ ======================
    void Patrol()
    {
        if (movingRight)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
            sr.flipX = true;

            if (transform.position.x >= startPos.x + patrolDistance)
                movingRight = false;
        }
        else
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            sr.flipX = false;

            if (transform.position.x <= startPos.x - patrolDistance)
                movingRight = true;
        }
    }

    // ====================== СТОЛКНОВЕНИЕ С ИГРОКОМ ======================
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Урон игроку
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(contactDamage);

            // Отключить управление (отбрасывание)
            Player_Controller playerCtrl = collision.gameObject.GetComponent<Player_Controller>();
            if (playerCtrl != null)
            {
                playerCtrl.isKnockedBack = true;
                StartCoroutine(EnableControl(playerCtrl));
            }

            // Отбрасывание игрока
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                float direction = (collision.transform.position.x > transform.position.x) ? 1f : -1f;
                playerRb.velocity = new Vector2(
                    direction * knockbackForce,
                    Mathf.Min(playerRb.velocity.y + 1f, 5f)
                );
            }
        }
    }

    // ====================== ВОЗВРАТ УПРАВЛЕНИЯ ИГРОКУ ======================
    IEnumerator EnableControl(Player_Controller playerCtrl)
    {
        yield return new WaitForSeconds(knockbackDuration);
        playerCtrl.isKnockedBack = false;
    }

    // ====================== ПОЛУЧЕНИЕ УРОНА ======================
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
            Die();
        else
            StartCoroutine(DamageFlash());
    }

    // ====================== ЭФФЕКТ ПОЛУЧЕНИЯ УРОНА ======================
    IEnumerator DamageFlash()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    // ====================== СМЕРТЬ ======================
    void Die()
    {
        Debug.Log("Слизень побежден!");
        Destroy(gameObject);
    }
}