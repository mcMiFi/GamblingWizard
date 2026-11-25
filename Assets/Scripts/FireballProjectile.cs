using UnityEngine;

public sealed class FireballProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float damage;
    private float speed;
    private float collisionRadius;
    private float remainingLifetime;
    private bool initialized;

    public void Initialize(
        Vector3 launchDirection,
        float projectileDamage,
        float projectileSpeed,
        float projectileRadius,
        float projectileLifetime,
        Color projectileColor)
    {
        direction = launchDirection.normalized;
        damage = projectileDamage;
        speed = projectileSpeed;
        collisionRadius = projectileRadius;
        remainingLifetime = projectileLifetime;
        initialized = true;

        Renderer projectileRenderer = GetComponent<Renderer>();
        if (projectileRenderer != null)
        {
            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            projectileRenderer.GetPropertyBlock(properties);
            properties.SetColor(Shader.PropertyToID("_BaseColor"), projectileColor);
            properties.SetColor(Shader.PropertyToID("_Color"), projectileColor);
            projectileRenderer.SetPropertyBlock(properties);
        }
    }

    private void Update()
    {
        if (!initialized)
            return;

        float distance = speed * Time.deltaTime;
        if (Physics.SphereCast(
            transform.position,
            collisionRadius,
            direction,
            out RaycastHit hit,
            distance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        transform.position += direction * distance;
        remainingLifetime -= Time.deltaTime;
        if (remainingLifetime <= 0f)
            Destroy(gameObject);
    }
}
