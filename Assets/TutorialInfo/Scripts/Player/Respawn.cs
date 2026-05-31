using UnityEngine;

public class Respawn : MonoBehaviour
{
    // --------------------
    // Nastaveni respawnu
    // --------------------
    public float fallLimitY = -20f;

    private CharacterController cc;
    private Transform checkpointRespawnPoint;
    private Vector3 startRespawnPosition;
    private Quaternion startRespawnRotation;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        cc = GetComponent<CharacterController>();
        startRespawnPosition = transform.position;
        startRespawnRotation = transform.rotation;
    }

    void Update()
    {
        if (transform.position.y < fallLimitY)
        {
            PlayerRespawn();
        }
    }

    // --------------------
    // Verejne API pro checkpointy
    // --------------------
    public void SetRespawnPoint(Transform newRespawnPoint)
    {
        checkpointRespawnPoint = newRespawnPoint;
    }

    public void PlayerRespawn()
    {
        SetCharacterControllerEnabled(false);

        if (checkpointRespawnPoint != null)
        {
            transform.position = checkpointRespawnPoint.position;
            transform.rotation = checkpointRespawnPoint.rotation;
        }
        else
        {
            transform.position = startRespawnPosition;
            transform.rotation = startRespawnRotation;
        }

        SetCharacterControllerEnabled(true);
    }

    // --------------------
    // CharacterController helper
    // --------------------
    void SetCharacterControllerEnabled(bool enabledState)
    {
        if (cc != null)
        {
            cc.enabled = enabledState;
        }
    }
}
