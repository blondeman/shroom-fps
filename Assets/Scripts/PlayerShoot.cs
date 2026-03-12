using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public float range;
    public float diameter;
    public Transform tip;
    
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            if (!particleSystem.isEmitting)
                particleSystem.Play();
        }
        else
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    void Cast()
    {
        float radius = diameter / 2f;

        // Raycast first to find distance
        float castDistance = range;
        if (Physics.Raycast(tip.position, tip.forward, out RaycastHit rayHit, range))
        {
            castDistance = rayHit.distance;
        }

        // SphereCastAll up to that distance
        RaycastHit[] hits = Physics.SphereCastAll(
            tip.position,
            radius,
            tip.forward,
            castDistance
        );

        foreach (RaycastHit hit in hits)
        {
            // Handle hits...
        }
    }

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        float radius = diameter / 2f;

        // Replicate raycast
        float castDistance = range;
        if (Physics.Raycast(tip.position, tip.forward, out RaycastHit rayHit, range))
        {
            castDistance = rayHit.distance;

            // Draw the ray hit point
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(rayHit.point, 0.05f);
        }

        // Draw raycast line
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(tip.position, tip.position + tip.forward * castDistance);

        // Replicate SphereCastAll
        RaycastHit[] hits = Physics.SphereCastAll(
            tip.position,
            radius,
            tip.forward,
            castDistance
        );

        bool anyHit = hits.Length > 0;
        Gizmos.color = anyHit ? Color.red : Color.green;

        // Sphere at origin and end
        Gizmos.DrawWireSphere(tip.position, radius);
        Vector3 endPos = tip.position + tip.forward * castDistance;
        Gizmos.DrawWireSphere(endPos, radius);
        Gizmos.DrawLine(
            tip.position + tip.up * radius,
            endPos + tip.up * radius
        );
        Gizmos.DrawLine(
            tip.position - tip.up * radius,
            endPos - tip.up * radius
        );

        // Draw each sphere hit
        foreach (RaycastHit hit in hits)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hit.point, 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(hit.point, hit.normal * 0.5f);
        }
    }
    #endif
}
