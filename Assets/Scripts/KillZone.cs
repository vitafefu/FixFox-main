using UnityEngine;

public class KillZone : MonoBehaviour
{
    public float respawnYOffset = 1.2f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // 1) гасим скорость, чтобы не улетал дальше
        var rb = other.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;

        // 2) телепорт на чекпоинт (или старт)
        Vector3 targetPos = other.transform.position;

        if (SaveManager.Instance != null &&
            SaveManager.Instance.data != null &&
            SaveManager.Instance.data.checkpointPosition != Vector3.zero)
        {
            targetPos = SaveManager.Instance.data.checkpointPosition + Vector3.up * respawnYOffset;
        }
        else
        {
            // если нет чекпоинта Ч пробуем defaultSpawnPoint из PlayerRespawn
            var resp = other.GetComponent<PlayerRespawn>();
            if (resp != null && resp.defaultSpawnPoint != null)
                targetPos = resp.defaultSpawnPoint.position;
        }

        other.transform.position = targetPos;

        // 3) (опционально) дать небольшой "инвул" чтобы не умереть сразу снова
        // тут можно потом добавить мерцание/неу€звимость
    }
}
