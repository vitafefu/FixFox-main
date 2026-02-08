using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 7;
    private int currentHealth;

    [Header("Hearts UI (по порядку слева направо)")]
    public GameObject[] hearts;

    [Header("Auto Regen")]
    public bool autoRegen = true;
    public float regenDelay = 1.7f;      // сколько ждать после урона перед регеном
    public float regenInterval = 0.52f;   // как часто лечить (меньше = быстрее)
    public int regenAmount = 1;          // сколько хп за тик

    private float lastDamageTime;
    private float regenTimer;

    void Start()
    {
        // защита от рассинхрона: maxHealth не больше количества сердец
        if (hearts != null && hearts.Length > 0)
            maxHealth = Mathf.Min(maxHealth, hearts.Length);

        currentHealth = maxHealth;
        lastDamageTime = -999f;
        regenTimer = 0f;

        UpdateHearts();
    }

    void Update()
    {
        if (!autoRegen) return;
        if (currentHealth <= 0) return;
        if (currentHealth >= maxHealth) return;

        // ждём после урона
        if (Time.time - lastDamageTime < regenDelay) return;

        regenTimer += Time.deltaTime;

        if (regenTimer >= regenInterval)
        {
            Heal(regenAmount);
            regenTimer = 0f;
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        lastDamageTime = Time.time;  // сброс таймера регена
        regenTimer = 0f;

        UpdateHearts();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHearts();
    }

    void UpdateHearts()
    {
        if (hearts == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            hearts[i].SetActive(i < currentHealth);
        }
    }

    void Die()
    {
        Debug.Log("Player Dead");

        // выключаем управление и оружие
        var pc = GetComponent<Player_Controller>();
        if (pc != null) pc.enabled = false;

        var shoot = GetComponent<PlayerShoot>();
        if (shoot != null) shoot.enabled = false;

        var melee = GetComponent<PlayerMelee>();
        if (melee != null) melee.enabled = false;

        var staff = GetComponent<PlayerStaff>();
        if (staff != null) staff.enabled = false;

        // обнуляем скорость
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;

        // респавн на чекпоинт
        Invoke(nameof(RespawnToCheckpoint), 0.2f);
    }

    void RespawnToCheckpoint()
    {
        Vector3 target = transform.position;

        if (SaveManager.Instance != null &&
            SaveManager.Instance.data != null &&
            SaveManager.Instance.data.checkpointPosition != Vector3.zero)
        {
            target = SaveManager.Instance.data.checkpointPosition + Vector3.up * 1.2f;
        }

        transform.position = target;

        // восстановить хп
        currentHealth = maxHealth;
        UpdateHearts();

        // сброс регена
        lastDamageTime = Time.time;
        regenTimer = 0f;

        // вернуть управление и оружие
        var pc = GetComponent<Player_Controller>();
        if (pc != null) pc.enabled = true;

        var shoot = GetComponent<PlayerShoot>();
        if (shoot != null) shoot.enabled = true;

        var melee = GetComponent<PlayerMelee>();
        if (melee != null) melee.enabled = true;

        var staff = GetComponent<PlayerStaff>();
        if (staff != null) staff.enabled = true;
    }
}
