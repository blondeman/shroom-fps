using UnityEngine;

public class Billboard : MonoBehaviour
{
    void Update()
    {
        // Rotate towards a target Transform around the Y axis
        Vector3 direction = Camera.main.transform.position - transform.position;
        direction.y = 0f; // Lock to Y axis only
        
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(-direction);
    }
}
