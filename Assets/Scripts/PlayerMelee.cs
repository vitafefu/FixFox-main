using UnityEngine;
using System.Collections;

public class PlayerMelee : MonoBehaviour
{
    [Header("Hitbox")]
    public Transform meleePoint;
    public GameObject hitboxPrefab;

    [Header("Slash FX")]
    public GameObject slashPrefab;
    public float slashLife = 0.12f;
    public Vector3 slashLocalOffset = new Vector3(0.6f, 0.15f, 0f);
    public float slashZRight = -20f;   // небольшой наклон спрайта вправо
    public float slashZLeft = 20f;   // наклон спрайта влево

    [Header("Knife swing (weapon visual)")]
    public Transform weaponVisualRoot;      // Weapon Socket/WeaponVisualRoot
    public float swingDuration = 0.09f;     // резкость
    public float swingStartZ = 70f;         // сверху
    public float swingEndZ = -70f;        // вниз
    public AnimationCurve swingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Stats")]
    public float attackCooldown = 0.25f;
    public int damage = 2;
    public bool canAttack = true;

    float nextAttackTime;
    Animator anim;
    SpriteRenderer sr;
    Coroutine swingRoutine;

    void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (weaponVisualRoot == null)
        {
            var t = transform.Find("Weapon Socket/WeaponVisualRoot");
            if (t != null) weaponVisualRoot = t;
        }
    }

    void Update()
    {
        if (!canAttack) return;

        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            if (anim != null)
                anim.SetTrigger("melee");
            else
                SpawnMeleeHitbox(); // если вдруг без анимации
        }
    }

    // ✅ вызывай это из Animation Event на клипе атаки
    public void SpawnMeleeHitbox()
    {
        bool left = sr != null && sr.flipX;

        // 1) Hitbox
        if (meleePoint != null && hitboxPrefab != null)
        {
            var go = Instantiate(hitboxPrefab, meleePoint.position, Quaternion.identity);

            var hb = go.GetComponent<MeleeHitbox>();
            if (hb != null)
            {
                hb.damage = damage;
                hb.SetDirection(left ? Vector2.left : Vector2.right);
            }

            go.transform.localScale = new Vector3(left ? -1f : 1f, 1f, 1f);
        }

        // 2) Slash FX
        if (slashPrefab != null && meleePoint != null)
        {
            Vector3 pos = meleePoint.position;

            // небольшой оффсет “вперёд” относительно направления
            Vector3 off = slashLocalOffset;
            if (left) off.x = -off.x;
            pos += off;

            float z = left ? slashZLeft : slashZRight;
            var fx = Instantiate(slashPrefab, pos, Quaternion.Euler(0, 0, z));

            // зеркалим по X если надо
            var s = fx.transform.localScale;
            fx.transform.localScale = new Vector3(left ? -Mathf.Abs(s.x) : Mathf.Abs(s.x), s.y, s.z);

            Destroy(fx, slashLife);
        }

        // 3) Knife swing (rotate weapon visual)
        if (weaponVisualRoot != null)
        {
            // берём текущий визуал оружия (первый ребёнок в root)
            Transform weapon = weaponVisualRoot.childCount > 0 ? weaponVisualRoot.GetChild(0) : null;
            if (weapon != null)
            {
                if (swingRoutine != null) StopCoroutine(swingRoutine);
                swingRoutine = StartCoroutine(SwingZ(weapon, left));
            }
        }
    }

    IEnumerator SwingZ(Transform weapon, bool left)
    {
        float t = 0f;

        // базовый поворот (на случай если оружие уже было повернуто)
        Quaternion baseRot = weapon.localRotation;

        // направление взмаха можно чуть инвертировать для левой стороны, если захочешь
        float a0 = swingStartZ;
        float a1 = swingEndZ;

        while (t < swingDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / swingDuration);
            float kk = swingCurve.Evaluate(k);

            float ang = Mathf.Lerp(a0, a1, kk);

            // применяем поверх базового
            weapon.localRotation = baseRot * Quaternion.Euler(0, 0, ang);
            yield return null;
        }

        // возвращаем обратно
        weapon.localRotation = baseRot;
    }
}
