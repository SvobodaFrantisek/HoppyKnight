using UnityEngine;

public class Checkpoint : MonoBehaviour
{
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
