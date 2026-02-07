using UnityEngine;

public class WeaponVisualFlip : MonoBehaviour
{
    public SpriteRenderer playerSprite;

    void LateUpdate()
    {
        if (!playerSprite) return;

        bool left = playerSprite.flipX;
        transform.localScale = new Vector3(left ? -1f : 1f, 1f, 1f);
    }
}
