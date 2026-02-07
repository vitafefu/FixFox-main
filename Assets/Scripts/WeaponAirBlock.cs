using UnityEngine;

public class WeaponAirBlock : MonoBehaviour
{
    [Header("Links (можно не заполнять вручную)")]
    public Player_Controller controller;
    public GameObject weaponVisualRoot;

    private PlayerShoot shooter;

    public float showDelay = 0.08f;
    private float groundedTime;
    private PlayerMelee melee;



    void Awake()
    {
        if (controller == null) controller = GetComponent<Player_Controller>();
        shooter = GetComponent<PlayerShoot>();
        melee = GetComponent<PlayerMelee>();

        // если не назначили root — попробуем найти по имени (как у тебя)
        if (weaponVisualRoot == null)
        {
            var t = transform.Find("Weapon Socket/WeaponVisualRoot");
            if (t != null) weaponVisualRoot = t.gameObject;
        }
    }

    void Update()
    {
        if (controller == null) return;

        bool grounded = controller.IsGrounded && !controller.IsClimbing && !controller.isKnockedBack;

        if (grounded) groundedTime += Time.deltaTime;
        else groundedTime = 0f;

        bool canUseWeapon = groundedTime >= showDelay;


        if (weaponVisualRoot != null)
            weaponVisualRoot.SetActive(canUseWeapon);

        if (shooter != null)
            shooter.enabled = canUseWeapon;
        if (melee != null)
            melee.canAttack = canUseWeapon;

    }
}
