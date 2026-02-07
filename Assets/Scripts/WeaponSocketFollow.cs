using UnityEngine;

public class WeaponSocketFollow : MonoBehaviour
{
    public SpriteRenderer playerSprite; // лиса
    private Vector3 rightPos;

    void Awake()
    {
        rightPos = transform.localPosition; // выставь вручную в руку ВПРАВО и это запомнится
    }

    void LateUpdate()
    {
        if (!playerSprite) return;

        bool left = playerSprite.flipX;

        var p = rightPos;
        p.x = left ? -rightPos.x : rightPos.x; // ТОЛЬКО X
        transform.localPosition = p;
    }
}
