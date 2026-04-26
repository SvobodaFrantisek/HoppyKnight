using UnityEngine;

public class LogPusher : MonoBehaviour
{
    [Header("Sila odstrceni od klady")]
    public float pushForce = 5f;

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        CharacterController characterController = other.GetComponent<CharacterController>();

        if (characterController == null)
        {
            return;
        }

        PushPlayer(characterController, other.transform.position);
    }

    void PushPlayer(CharacterController characterController, Vector3 playerPosition)
    {
        Vector3 direction = playerPosition - transform.position;
        direction.y = 0f;

        characterController.Move(direction.normalized * pushForce * Time.deltaTime);
    }
}
