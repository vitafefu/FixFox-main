using UnityEngine;

public class BossArenaTrigger : MonoBehaviour
{
    public GameObject boss;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            boss.SetActive(true);
            Destroy(gameObject);
        }
    }
}
