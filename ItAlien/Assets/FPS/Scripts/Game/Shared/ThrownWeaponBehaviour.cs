using UnityEngine;
using Unity.FPS.Game;

public class ThrownWeaponBehaviour : MonoBehaviour
{
    public float Damage = 50f;
    public GameObject Owner;
    public LayerMask HitLayers;

    private bool hasHit = false;

    void Update()
    {
        if (!hasHit)
        {
            CheckForHits();
        }
    }

    void CheckForHits()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f, HitLayers);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject != Owner && hit.gameObject != gameObject)
            {
                Health targetHealth = hit.GetComponentInParent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(Damage, Owner);
                    StickToObject(hit.gameObject);
                    hasHit = true;

                    Destroy(gameObject, 2f);
                    break;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    void StickToObject(GameObject target)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.SetParent(target.transform);
    }
}