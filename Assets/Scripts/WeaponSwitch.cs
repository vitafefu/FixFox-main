using UnityEngine;
using System;

public class WeaponSwitch : MonoBehaviour
{
    [Serializable]
    public class WeaponSlot
    {
        public string name;                 // "Pistol"
        public GameObject weaponVisualPrefab;
        public GameObject bulletPrefab;

        [Header("Shoot params")]
        public float fireRate = 6f;
        public float bulletSpeed = 10f;
        public int damage = 1;
        public float lifetime = 3f;

        public bool isMelee;
        public int meleeDamage = 2;
        public float meleeCooldown = 0.25f;
        public GameObject meleeHitboxPrefab;

    }

    public Transform weaponVisualRoot;      // Weapon Socket/WeaponVisualRoot
    public PlayerShoot shooter;             // твой PlayerShoot
    public WeaponSlot[] weapons;

    public KeyCode nextKey = KeyCode.Q;
    public KeyCode prevKey = KeyCode.E;

    private int index = 0;
    private GameObject currentVisual;

    void Awake()
    {
        if (shooter == null) shooter = GetComponent<PlayerShoot>();
        if (weaponVisualRoot == null)
        {
            var t = transform.Find("Weapon Socket/WeaponVisualRoot");
            if (t != null) weaponVisualRoot = t;
        }
    }

    void Start()
    {
        Equip(0);
    }

    void Update()
    {
        if (weapons == null || weapons.Length == 0) return;

        // 1-4 быстрый выбор
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Equip(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Equip(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) Equip(3);

        // Q/E листание
        if (Input.GetKeyDown(nextKey)) Equip((index + 1) % weapons.Length);
        if (Input.GetKeyDown(prevKey)) Equip((index - 1 + weapons.Length) % weapons.Length);
    }

    public void Equip(int newIndex)
    {
        if (weapons == null || weapons.Length == 0) return;
        if (newIndex < 0 || newIndex >= weapons.Length) return;

        index = newIndex;

        // Удаляем старый визуал
        if (currentVisual != null) Destroy(currentVisual);

        var w = weapons[index];

        // Ставим новый визуал
        if (weaponVisualRoot != null && w.weaponVisualPrefab != null)
        {
            currentVisual = Instantiate(w.weaponVisualPrefab, weaponVisualRoot);
            currentVisual.transform.localPosition = Vector3.zero;
            currentVisual.transform.localRotation = Quaternion.identity;
            currentVisual.transform.localScale = Vector3.one;
        }

        // Обновляем стрельбу
        if (shooter != null)
        {
            shooter.bulletPrefab = w.bulletPrefab;
            shooter.fireRate = w.fireRate;
            shooter.bulletSpeed = w.bulletSpeed;
            shooter.damage = w.damage;
            shooter.lifetime = w.lifetime;
        }

        var melee = GetComponent<PlayerMelee>();

        if (w.isMelee)
        {
            // выключаем стрельбу
            if (shooter != null) shooter.enabled = false;

            // включаем ближний бой
            if (melee != null)
            {
                melee.enabled = true;
                melee.damage = w.meleeDamage;
                melee.attackCooldown = w.meleeCooldown;
                melee.hitboxPrefab = w.meleeHitboxPrefab;
            }
        }
        else
        {
            // включаем стрельбу
            if (shooter != null) shooter.enabled = true;

            // выключаем ближний бой
            if (melee != null) melee.enabled = false;

            // выставляем параметры шутера как раньше
            shooter.bulletPrefab = w.bulletPrefab;
            shooter.fireRate = w.fireRate;
            shooter.bulletSpeed = w.bulletSpeed;
            shooter.damage = w.damage;
            shooter.lifetime = w.lifetime;
        }

    }
}
