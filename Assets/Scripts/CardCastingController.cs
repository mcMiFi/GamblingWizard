using System.Collections;
using System.Collections.Generic;
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
        {
            cardHand.CardUtilized += CastCard;
            cardHand.CardsUtilized += CastCards;
        }
    }

    private void OnDisable()
    {
        if (cardHand != null)
        {
            cardHand.CardUtilized -= CastCard;
            cardHand.CardsUtilized -= CastCards;
        }
    }

    private void CastCard(CardDefinition card)
    {
        CastCard(card, 1f);
    }

    private void CastCards(IReadOnlyList<CardDefinition> cards)
    {
        if (TryMatchCombo(cards, out ComboDefinition combo, out List<CardDefinition> remainingCards))
        {
            CastCombo(combo);
            if (remainingCards.Count > 0)
                CastAttackAndHealCards(remainingCards);
            return;
        }

        if (cards.Count == 1)
        {
            CastCard(cards[0]);
            return;
        }

        CastAttackAndHealCards(cards);
    }

    private bool TryMatchCombo(
        IReadOnlyList<CardDefinition> cards,
        out ComboDefinition matchedCombo,
        out List<CardDefinition> remainingCards)
    {
        IReadOnlyList<ComboDefinition> combos = cardHand.Database.Combos;
        if (combos != null)
        {
            foreach (ComboDefinition combo in combos)
            {
                if (combo == null || combo.RequiredCards == null || combo.RequiredCards.Count == 0)
                    continue;

                List<CardDefinition> unmatchedCards = new List<CardDefinition>(cards);
                bool requirementsMet = true;
                foreach (CardDefinition requiredCard in combo.RequiredCards)
                {
                    int matchingIndex = unmatchedCards.IndexOf(requiredCard);
                    if (requiredCard == null || matchingIndex < 0)
                    {
                        requirementsMet = false;
                        break;
                    }

                    unmatchedCards.RemoveAt(matchingIndex);
                }

                if (requirementsMet)
                {
                    matchedCombo = combo;
                    remainingCards = unmatchedCards;
                    return true;
                }
            }
        }

        matchedCombo = null;
        remainingCards = null;
        return false;
    }

    private void CastAttackAndHealCards(IReadOnlyList<CardDefinition> cards)
    {
        foreach (CardDefinition card in cards)
        {
            if (card.Category == CardSpellCategory.Attack)
                CastCard(card, 1f / 3f);
            else if (card.Category == CardSpellCategory.Heal)
                CastCard(card, 1f);
        }
    }

    private void CastCombo(ComboDefinition combo)
    {
        switch (combo.EffectType)
        {
            case ComboEffectType.KillDash:
                PlayerMotor playerMotor = GetComponent<PlayerMotor>();
                if (playerMotor == null)
                {
                    Debug.LogError($"Combo '{combo.Id}' requires a PlayerMotor component.", this);
                    return;
                }

                playerMotor.TryKillDash(
                    combo.Damage,
                    combo.DashLengthMultiplier,
                    combo.DashWidthMultiplier,
                    combo.DashDuration);
                break;
            default:
                Debug.LogError($"Combo '{combo.Id}' has an unsupported effect type.", combo);
                break;
        }
    }

    private void CastCard(CardDefinition card, float damageMultiplier)
    {
        switch (card.SpellType)
        {
            case CardSpellType.Regeneration:
                CastRegeneration(card);
                break;
            case CardSpellType.ChainLightning:
                CastChainLightning(card, damageMultiplier);
                break;
            case CardSpellType.KillDash:
                CastKillDash(card, damageMultiplier);
                break;
            case CardSpellType.Fireball:
                CastFireball(card, damageMultiplier);
                break;
            default:
                Debug.LogError($"Card '{card.Id}' has an unsupported spell type.", card);
                break;
        }
    }

    private void CastKillDash(CardDefinition card, float damageMultiplier)
    {
        PlayerMotor playerMotor = GetComponent<PlayerMotor>();
        if (playerMotor == null)
        {
            Debug.LogError("Kill Dash requires a PlayerMotor component.", this);
            return;
        }

        playerMotor.TryKillDash(
            card.Damage * damageMultiplier,
            card.DashLengthMultiplier,
            card.DashWidthMultiplier,
            card.DashDuration);
    }

    private void CastRegeneration(CardDefinition card)
    {
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("Regeneration requires a PlayerHealth component.", this);
            return;
        }

        playerHealth.Heal(card.HealingAmount);
    }

    private void CastFireball(CardDefinition card, float damageMultiplier)
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
            card.Damage * damageMultiplier,
            card.ProjectileSpeed,
            card.ProjectileRadius,
            card.ProjectileLifetime,
            card.Color);
    }

    private void CastChainLightning(CardDefinition card, float damageMultiplier)
    {
        Ray ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);
        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            100f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            return;
        }

        EnemyHealth firstTarget = hit.collider.GetComponentInParent<EnemyHealth>();
        float damage = card.Damage * damageMultiplier;
        if (firstTarget == null || !firstTarget.TakeDamage(damage))
            return;

        StartCoroutine(ChainLightning(card, firstTarget, damage));
    }

    private IEnumerator ChainLightning(CardDefinition card, EnemyHealth firstTarget, float damage)
    {
        HashSet<EnemyHealth> hitTargets = new HashSet<EnemyHealth> { firstTarget };
        Vector3 previousPosition = firstTarget.transform.position;

        for (int targetIndex = 0; targetIndex < card.MaxChainTargets; targetIndex++)
        {
            EnemyHealth nextTarget = FindNearestUnhitEnemy(previousPosition, card.ChainRange, hitTargets);
            if (nextTarget == null)
                yield break;

            hitTargets.Add(nextTarget);
            LightningChainVfx.Spawn(
                previousPosition,
                nextTarget.transform,
                card.Color,
                card.ChainVfxDuration);

            if (card.ChainDelay > 0f)
                yield return new WaitForSeconds(card.ChainDelay);

            if (nextTarget != null)
                nextTarget.TakeDamage(damage);

            if (nextTarget != null)
                previousPosition = nextTarget.transform.position;
        }
    }

    private static EnemyHealth FindNearestUnhitEnemy(
        Vector3 origin,
        float range,
        HashSet<EnemyHealth> hitTargets)
    {
        EnemyHealth nearest = null;
        float nearestDistanceSquared = range * range;
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>();

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || enemy.IsDead || hitTargets.Contains(enemy))
                continue;

            float distanceSquared = (enemy.transform.position - origin).sqrMagnitude;
            if (distanceSquared < nearestDistanceSquared)
            {
                nearest = enemy;
                nearestDistanceSquared = distanceSquared;
            }
        }

        return nearest;
    }
}
