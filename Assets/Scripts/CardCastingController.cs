using UnityEngine;

[DisallowMultipleComponent]
public sealed class CardCastingController : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Transform castOrigin;

    private CardHandController cardHand;

    private void Awake()
    {
        cardHand = GetComponent<CardHandController>();
        if (cardHand == null)
        {
            Debug.LogError("CardCastingController requires a CardHandController.", this);
            enabled = false;
            return;
        }

        if (aimCamera == null)
            aimCamera = GetComponentInChildren<Camera>();

        if (aimCamera == null)
        {
            Debug.LogError("CardCastingController requires an aim camera.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (cardHand != null)
            cardHand.CardUtilized += CastCard;
    }

    private void OnDisable()
    {
        if (cardHand != null)
            cardHand.CardUtilized -= CastCard;
    }

    private void CastCard(CardDefinition card)
    {
        Vector3 direction = aimCamera.transform.forward;
        Vector3 origin = castOrigin != null
            ? castOrigin.position
            : aimCamera.transform.position + direction * 0.8f;
        GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectileObject.name = $"{card.DisplayName} Projectile";
        projectileObject.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(direction));
        projectileObject.transform.localScale = Vector3.one * (card.ProjectileRadius * 2f);

        Collider projectileCollider = projectileObject.GetComponent<Collider>();
        if (projectileCollider != null)
        {
            projectileCollider.enabled = false;
            Destroy(projectileCollider);
        }

        FireballProjectile projectile = projectileObject.AddComponent<FireballProjectile>();

        projectile.Initialize(
            direction,
            card.Damage,
            card.ProjectileSpeed,
            card.ProjectileRadius,
            card.ProjectileLifetime,
            card.Color);
    }
}
