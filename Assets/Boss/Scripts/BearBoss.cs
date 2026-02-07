using UnityEngine;
using System.Collections;

public class BearBoss : MonoBehaviour, IDamageable

{
    // ====================== ПАРАМЕТРЫ ДВИЖЕНИЯ ======================
    [Header("Движение")]
    public float speed = 1.5f;              // Скорость медведя
    public float patrolDistance = 6f;       // Дистанция патрулирования
    public bool startMovingRight = true;    // Начинает движение вправо

    // ====================== ПАРАМЕТРЫ АТАКИ ======================
    [Header("Атака")]
    public float attackRange = 3f;          // Дистанция обнаружения игрока
    public float attackCooldown = 1.5f;     // Перезарядка между атаками
    public int contactDamage = 1;           // Урон при касании игрока

    [Header("Отбрасывание")]
    public float knockbackForce = 12f;      // Сила отбрасывания игрока
    public float knockbackDuration = 0.3f;  // Длительность отбрасывания

    // ====================== ПАРАМЕТРЫ ЗДОРОВЬЯ ======================
    [Header("Здоровье")]
    public int health = 50;                // Здоровье медведя

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
        // Сохраняем стартовую позицию для патрулирования
        startPos = transform.position;
        movingRight = startMovingRight;

        // Получаем компоненты с объекта
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Ищем игрока по тегу
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("Не найден игрок!");

        // Начинаем с полным таймером атаки
        attackTimer = attackCooldown;
    }

    // ====================== ОСНОВНОЙ ЦИКЛ ОБНОВЛЕНИЯ ======================
    void Update()
    {
        // Если игрок не найден - выходим
        if (player == null) return;

        // Вычисляем дистанцию до игрока
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Уменьшаем таймер атаки
        attackTimer -= Time.deltaTime;

        // Если игрок в радиусе атаки
        if (distanceToPlayer <= attackRange)
        {
            // ОСТАНОВКА ДВИЖЕНИЯ - медведь останавливается при обнаружении
            rb.velocity = Vector2.zero;

            // ПОВОРОТ К ИГРОКУ - смотрим в сторону игрока
            bool playerIsRight = player.position.x > transform.position.x;
            sr.flipX = playerIsRight;

            // АНИМАЦИЯ АТАКИ - включаем анимацию атаки, выключаем ходьбу
            if (anim != null)
            {
                anim.SetBool("isAttacking", true);
                anim.SetFloat("speed", 0);
            }

            // БЛИЖНЯЯ АТАКА - если перезарядка прошла
            if (attackTimer <= 0f)
            {
                // Можно добавить логику ближней атаки здесь
                Debug.Log("Медведь атакует!");
                attackTimer = attackCooldown;
            }
        }
        else
        {
            // ПАТРУЛИРОВАНИЕ - если игрок далеко
            Patrol();

            // АНИМАЦИЯ ПЕРЕДВИЖЕНИЯ - выключаем атаку, включаем ходьбу
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
        // Движение вправо
        if (movingRight)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
            sr.flipX = true;  // Спрайт смотрит вправо

            // Если достигли правой границы - разворачиваемся
            if (transform.position.x >= startPos.x + patrolDistance)
                movingRight = false;
        }
        // Движение влево
        else
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            sr.flipX = false; // Спрайт смотрит влево

            // Если достигли левой границы - разворачиваемся
            if (transform.position.x <= startPos.x - patrolDistance)
                movingRight = true;
        }
    }

    // ====================== СТОЛКНОВЕНИЕ С ИГРОКОМ ======================
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Если столкнулись с игроком
        if (collision.gameObject.CompareTag("Player"))
        {
            // НАНОСИМ УРОН ИГРОКУ
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(contactDamage);

            // ОТКЛЮЧАЕМ УПРАВЛЕНИЕ ИГРОКА (отбрасывание)
            Player_Controller playerCtrl = collision.gameObject.GetComponent<Player_Controller>();
            if (playerCtrl != null)
            {
                playerCtrl.isKnockedBack = true;
                StartCoroutine(EnableControl(playerCtrl));
            }

            // ОТБРАСЫВАЕМ ИГРОКА
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Определяем направление отбрасывания
                float direction = (collision.transform.position.x > transform.position.x) ? 1f : -1f;

                // Чисто горизонтальное отбрасывание с небольшим подскоком
                playerRb.velocity = new Vector2(
                    direction * knockbackForce,    // Горизонтальная сила
                    Mathf.Min(playerRb.velocity.y + 1f, 5f) // Небольшой вертикальный толчок
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
        // Уменьшаем здоровье
        health -= damage;

        // Если здоровье закончилось - умираем
        if (health <= 0)
            Die();
        else
            StartCoroutine(DamageFlash()); // Эффект получения урона
    }

    // ====================== ЭФФЕКТ ПОЛУЧЕНИЯ УРОНА (мигание красным) ======================
    IEnumerator DamageFlash()
    {
        sr.color = Color.red;      // Красим в красный
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;    // Возвращаем обычный цвет
    }

    // ====================== СМЕРТЬ МЕДВЕДЯ ======================
    void Die()
    {
        Debug.Log("Медведь побежден!");
        // Пока просто уничтожаем объект
        // Позже можно добавить анимацию смерти
        Destroy(gameObject);
    }
}