using UnityEngine;

public class WeaponSocketFollow : MonoBehaviour
{
    public SpriteRenderer playerSprite;

    [Header("Weapon socket (hand)")]
    public float leftSubtract = 3f;      // смещение сокета ТОЛЬКО слева

    [Header("Bullet spawn (FirePoint)")]
    public Transform firePoint;          // ссылка на FirePoint
    public float firePointLeftX = 0f;    // доп. смещение пули по X слева
    public float firePointLeftY = 0f;    // доп. смещение пули по Y слева

    private Vector3 rightPos;
    private Vector3 firePointRightPos;

    void Awake()
    {
        // эталонные позиции ДЛЯ ВПРАВО
        rightPos = transform.localPosition;

        if (firePoint != null)
            firePointRightPos = firePoint.localPosition;
    }

    void LateUpdate()
    {
        if (!playerSprite) return;

        bool left = playerSprite.flipX;

        // ===== SOCKET (РУКА) =====
        var p = rightPos;

        if (left)
            p.x = -rightPos.x + leftSubtract;
        else
            p.x = rightPos.x;

        transform.localPosition = p;

        // ===== FIRE POINT (ПУЛИ) =====
        if (firePoint != null)
        {
            var fp = firePointRightPos;

            if (left)
            {
                fp.x = firePointRightPos.x + firePointLeftX;
                fp.y = firePointRightPos.y + firePointLeftY;
            }

            firePoint.localPosition = fp;
        }
    }
}
