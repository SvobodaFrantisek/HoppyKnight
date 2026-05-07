using UnityEngine;

public class RotatingLog : MonoBehaviour
{
    // --------------------
    // Nastaveni rotace
    // --------------------
    public Vector3 rotationAxis = Vector3.forward;
    public float rotationSpeed = 90f;
    public bool useLocalSpace = true;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Update()
    {
        Vector3 normalizedAxis = rotationAxis;

        if (normalizedAxis == Vector3.zero)
        {
            return;
        }

        normalizedAxis.Normalize();
        Vector3 rotationStep = normalizedAxis * rotationSpeed * Time.deltaTime;

        if (useLocalSpace)
        {
            transform.Rotate(rotationStep, Space.Self);
        }
        else
        {
            transform.Rotate(rotationStep, Space.World);
        }
    }
}
