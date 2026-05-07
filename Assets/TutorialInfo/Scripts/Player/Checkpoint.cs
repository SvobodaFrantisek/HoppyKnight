using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Kdyz do checkpointu vejde hrac, ulozi se mu nova respawn pozice.
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Respawn respawn = other.GetComponent<Respawn>();

        if (respawn != null)
        {
            respawn.SetRespawnPoint(transform);
        }
    }
}
