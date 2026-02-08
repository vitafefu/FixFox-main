using UnityEngine;
using System;

public class WeaponSwitch : MonoBehaviour
{
    [Serializable]
    public class WeaponSlot
    {
        public string name;

        [Header("Visual")]
        public GameObject weaponVisualPrefab;

        [Header("Type")]
        public bool isMelee;

        [Header("Ranged")]
        public GameObject bulletPrefab;
        public float fireRate = 6f;
        public float bulletSpeed = 10f;
        public int damage = 1;
        public float lifetime = 3f;

        [Header("Melee")]
        public int meleeDamage = 2;
        public float meleeCooldown = 0.25f;
        public GameObject meleeHitboxPrefab;

        [Header("Offsets (LOCAL, for RIGHT-facing)")]
        public Vector2 firePointLocalOffset;
        public Vector2 weaponLocalOffset;

        public bool isHybrid;
        public GameObject hybridProjectilePrefab;
        public int hybridMeleeDamage = 2;
        public float hybridShootDelay = 0.08f;
        public float hybridBulletSpeed = 10f;
        public int hybridBulletDamage = 1;
        public float hybridBulletLifetime = 1.2f;
        public float hybridCooldown = 0.5f;
    }

    [Header("Scene links (auto-find if empty)")]
    public Transform weaponVisualRoot;   // Weapon Socket/WeaponVisualRoot
    public Transform firePoint;          // Weapon Socket/FirePoint
    public PlayerShoot shooter;          // on Player
    public PlayerMelee melee;            // on Player (optional)

    [Header("Weapons")]
    public WeaponSlot[] weapons;

    [Header("Input")]
    public KeyCode nextKey = KeyCode.Q;
    public KeyCode prevKey = KeyCode.E;

    private int index = 0;
    private GameObject currentVisual;
    public PlayerStaff staff;


    void Awake()
    {
        if (shooter == null) shooter = GetComponent<PlayerShoot>();
        if (melee == null) melee = GetComponent<PlayerMelee>();

        if (weaponVisualRoot == null)
        {
            var t = transform.Find("Weapon Socket/WeaponVisualRoot");
            if (t != null) weaponVisualRoot = t;
        }

        if (firePoint == null)
        {
            var t = transform.Find("Weapon Socket/FirePoint");
            if (t != null) firePoint = t;
        }
        if (staff == null) staff = GetComponent<PlayerStaff>();

    }

    void Start()
    {
        if (weapons != null && weapons.Length > 0)
            Equip(0);
    }

    void Update()
    {
        if (weapons == null || weapons.Length == 0) return;

        // 1-4 quick select
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Equip(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Equip(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) Equip(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) Equip(4);

        // Q/E cycle
        if (Input.GetKeyDown(nextKey)) Equip((index + 1) % weapons.Length);
        if (Input.GetKeyDown(prevKey)) Equip((index - 1 + weapons.Length) % weapons.Length);
    }

    public void Equip(int newIndex)
    {
        if (weapons == null || weapons.Length == 0) return;
        if (newIndex < 0 || newIndex >= weapons.Length) return;

        index = newIndex;
        var w = weapons[index];

        // 1) Replace visual
        if (currentVisual != null) Destroy(currentVisual);
        currentVisual = null;

        if (weaponVisualRoot != null && w.weaponVisualPrefab != null)
        {
            currentVisual = Instantiate(w.weaponVisualPrefab, weaponVisualRoot);
            currentVisual.transform.localRotation = Quaternion.identity;
            currentVisual.transform.localScale = Vector3.one;
            currentVisual.transform.localPosition = w.weaponLocalOffset; // БЕЗ зеркала
        }

        // 2) FirePoint offset (БЕЗ зеркала)
        if (firePoint != null)
            firePoint.localPosition = w.firePointLocalOffset;

        if (w.isHybrid)
        {
            if (shooter != null)
            {
                shooter.enabled = false;
                shooter.bulletPrefab = null;
            }

            if (melee != null)
                melee.enabled = false;

            if (staff != null)
            {
                staff.enabled = true;
                if (shooter != null) shooter.enabled = false;

                staff.hitboxPrefab = w.meleeHitboxPrefab;
                staff.meleeDamage = w.meleeDamage;

                staff.projectilePrefab = w.bulletPrefab;
                staff.bulletSpeed = w.bulletSpeed;
                staff.bulletDamage = w.damage;
                staff.bulletLifetime = w.lifetime;

                staff.attackCooldown = Mathf.Max(0.05f, w.meleeCooldown); // или отдельное поле
                staff.shootDelay = 0.08f; // можно тоже вынести в слот
            }
            return;
        }
        else
        {
            if (staff != null) staff.enabled = false;
        }

        // 3) Enable correct combat mode
        if (w.isMelee)
        {
            if (shooter != null)
            {
                shooter.enabled = false;
                shooter.bulletPrefab = null;
            }

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
            if (melee != null) melee.enabled = false;

            if (shooter != null)
            {
                shooter.enabled = true;
                shooter.bulletPrefab = w.bulletPrefab;
                shooter.fireRate = w.fireRate;
                shooter.bulletSpeed = w.bulletSpeed;
                shooter.damage = w.damage;
                shooter.lifetime = w.lifetime;
            }
        }
    }
}
