using UnityEngine;

public class LogPusher : MonoBehaviour
{
    [Header("Síla odtlačení od klády")]
    public float pushForce = 5f;

    // Tato funkce se volá automaticky, když se hráč nachází uvnitř zóny (Triggeru)
    void OnTriggerStay(Collider other)
    {
        // Zajímá nás jenom hráč
        if (other.CompareTag("Player"))
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            
            if (cc != null)
            {
                // 1. Spočítáme směr (šipku) od středu klády směrem ven k hráči
                Vector3 direction = other.transform.position - transform.position;
                
                // 2. Chceme hráče odstrkávat jen do stran, ne do vzduchu
                direction.y = 0;

                // 3. Řekneme hráči, ať kousek uhne
                cc.Move(direction.normalized * pushForce * Time.deltaTime);
            }
        }
    }
}
