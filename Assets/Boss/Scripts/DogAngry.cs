using UnityEngine;
using System.Collections;

public class DogEnemy : MonoBehaviour, IDamageable

{
    // ====================== ПАРАМЕТРЫ ДВИЖЕНИЯ ======================
    [Header("Movement")]
    public float speed = 3f;              // Скорость собаки (быстрее жука 2f, медведя 1.5f)
    public float patrolDistance = 4f;     // Дистанция патрулирования
    public bool startMovingRight = true;  // Начинает движение вправо

    // ====================== ПАРАМЕТРЫ АТАКИ ======================
    [Header("Attack")]
    public float attackRange = 5f;        // Дистанция обнаружения игрока
    public float attackCooldown = 1.5f;   // Перезарядка между атаками
    public int contactDamage = 1;         // Урон при касании игрока (1 сердце)

    [Header("Knockback")]
    public float knockbackForce = 10f;    // Сила отбрасывания игрока
    public float knockbackDuration = 0.3f;// Длительность отбрасывания

    // ====================== ПАРАМЕТРЫ ЗДОРОВЬЯ ======================
    [Header("Health")]
    public int health = 50;               // Здоровье собаки (как у жука)

    // ====================== ПРИВАТНЫЕ ПЕРЕМЕННЫЕ ======================
    private Vector2 startPos;             // Стартовая позиция для патрулирования
    private bool movingRight;             // Направление движения
    private SpriteRenderer sr;            // Для поворота спрайта
    private Transform player;             // Ссылка на игрока
    private float attackTimer;            // Таймер для атаки
    private Rigidbody2D rb;               // Физическое тело
    private Animator anim;                // Аниматор

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

            // БЛИЖНЯЯ АТАКА (как у медведя) - если перезарядка прошла
            if (attackTimer <= 0f)
            {
                Debug.Log("Собака атакует ближней атакой!");
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

    // ====================== СТОЛКНОВЕНИЕ С ИГРОКОМ (БЛИЖНЯЯ АТАКА) ======================
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Урон игроку при касании
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
        Destroy(gameObject);
    }
}