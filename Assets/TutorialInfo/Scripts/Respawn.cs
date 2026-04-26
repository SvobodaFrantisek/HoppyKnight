using UnityEngine;

public class Respawn : MonoBehaviour
{
    public Transform respawnPoint;
    public float fallLimitY = -20f;

    private CharacterController cc;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (transform.position.y < fallLimitY)
        {
            PlayerRespawn();
        }
    }

    public void SetRespawnPoint(Transform newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;
    }

    public void PlayerRespawn()
    {
        if (respawnPoint == null)
        {
            Debug.LogWarning("Respawn point neni nastaveny.");
            return;
        }

        SetCharacterControllerEnabled(false);

        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        SetCharacterControllerEnabled(true);
    }

    void SetCharacterControllerEnabled(bool enabledState)
    {
        if (cc != null)
        {
            cc.enabled = enabledState;
        }
    }
}
