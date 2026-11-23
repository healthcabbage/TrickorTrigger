using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BombProjectile : MonoBehaviour
{
    private Rigidbody body;
    private Vector3 direction;
    private Vector3 startPosition;
    private float damage;
    private float knockbackPower;
    private float speed;
    private float throwRange;
    private float explosionDelay;
    private float explosionRadius;
    private float fuseTimer;
    private bool fuseStarted;
    private bool initialized;
    private bool hasExploded;
    private float explosionEffectTimer;
    private GameObject owner;
    private Collider bombCollider;
    private Renderer bombRenderer;
    private LineRenderer explosionRangeIndicator;
    private Material explosionIndicatorMaterial;

    private const float ExplosionEffectDuration = 0.3f;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        bombCollider = GetComponent<Collider>();
        bombRenderer = GetComponent<Renderer>();
        CreateExplosionRangeIndicator();
    }

    public void Initialize(
        Vector3 throwDirection,
        float damageAmount,
        float knockback,
        float throwSpeed,
        float range,
        float delay,
        float blastRadius,
        GameObject thrower)
    {
        direction = throwDirection.normalized;
        damage = damageAmount;
        knockbackPower = knockback;
        speed = Mathf.Max(0f, throwSpeed);
        throwRange = Mathf.Max(0f, range);
        explosionDelay = Mathf.Max(0f, delay);
        explosionRadius = Mathf.Max(0f, blastRadius);
        owner = thrower;
        startPosition = body.position;
        body.useGravity = false;
        body.isKinematic = true;
        initialized = true;

        if (throwRange <= 0f || speed <= 0f)
            StartFuse();
    }

    private void FixedUpdate()
    {
        if (!initialized || fuseStarted)
            return;

        float travelled = Vector3.Distance(startPosition, body.position);
        float remaining = throwRange - travelled;
        if (remaining <= 0.001f)
        {
            StartFuse();
            return;
        }

        float step = Mathf.Min(speed * Time.fixedDeltaTime, remaining);
        body.MovePosition(body.position + direction * step);
        if (remaining <= step + 0.001f)
            StartFuse();
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (hasExploded)
        {
            UpdateExplosionVisual();
            return;
        }

        if (!fuseStarted)
        {
            UpdateExplosionVisual();
            return;
        }

        fuseTimer += Time.deltaTime;
        if (fuseTimer >= explosionDelay)
            Explode();
    }

    private void StartFuse()
    {
        if (fuseStarted)
            return;

        fuseStarted = true;
        fuseTimer = 0f;
    }

    private void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;
        explosionEffectTimer = 0f;
        if (bombRenderer != null)
            bombRenderer.enabled = false;
        if (bombCollider != null)
            bombCollider.enabled = false;
        body.detectCollisions = false;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            explosionRadius,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide);
        HashSet<BaseMonster> hitMonsters = new HashSet<BaseMonster>();

        foreach (Collider hit in hits)
        {
            BaseMonster monster = hit.GetComponentInParent<BaseMonster>();
            if (monster == null || monster.gameObject == owner || hitMonsters.Contains(monster))
                continue;

            // Use the nearest point on the hit collider, not the monster pivot:
            // large enemies can overlap the blast while their root stays outside.
            Vector3 closestPoint = hit.ClosestPoint(transform.position);
            Vector3 directionFromBlast = closestPoint - transform.position;
            directionFromBlast.y = 0f;
            if (directionFromBlast.magnitude > explosionRadius)
                continue;

            hitMonsters.Add(monster);

            if (directionFromBlast.sqrMagnitude <= 0.001f)
                directionFromBlast = direction;
            else
                directionFromBlast.Normalize();

            monster.TakeDamage(new DamageInfo
            {
                damage = damage,
                attacker = owner,
                attackSource = gameObject,
                knockbackPower = knockbackPower,
                knockbackDirection = directionFromBlast
            });
        }

    }

    private void CreateExplosionRangeIndicator()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");
        if (shader == null)
            return;

        explosionIndicatorMaterial = new Material(shader);
        GameObject indicatorObject = new GameObject("Bomb Blast Radius");
        indicatorObject.transform.SetParent(transform, false);
        explosionRangeIndicator = indicatorObject.AddComponent<LineRenderer>();
        explosionRangeIndicator.useWorldSpace = true;
        explosionRangeIndicator.widthMultiplier = 0.08f;
        explosionRangeIndicator.startColor = new Color(1f, 0.25f, 0.05f, 0.9f);
        explosionRangeIndicator.endColor = explosionRangeIndicator.startColor;
        explosionRangeIndicator.sharedMaterial = explosionIndicatorMaterial;
        explosionRangeIndicator.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        explosionRangeIndicator.receiveShadows = false;
        explosionRangeIndicator.alignment = LineAlignment.View;
        explosionRangeIndicator.enabled = false;
    }

    private void UpdateExplosionVisual()
    {
        if (!initialized || explosionRangeIndicator == null)
            return;

        float radius = explosionRadius;
        if (hasExploded)
        {
            explosionEffectTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(explosionEffectTimer / ExplosionEffectDuration);
            radius *= progress;
            explosionRangeIndicator.startColor = Color.Lerp(
                new Color(1f, 1f, 0.65f, 1f),
                new Color(1f, 0.25f, 0.05f, 0.75f),
                progress);
            explosionRangeIndicator.endColor = explosionRangeIndicator.startColor;
            if (progress >= 1f)
            {
                Destroy(gameObject);
                return;
            }
        }

        Vector3 center = transform.position + Vector3.up * 0.08f;
        const int segments = 48;
        explosionRangeIndicator.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 point = center + new Vector3(
                Mathf.Sin(angle) * radius,
                0f,
                Mathf.Cos(angle) * radius);
            explosionRangeIndicator.SetPosition(i, point);
        }
        explosionRangeIndicator.enabled = true;
    }

    private void OnDestroy()
    {
        if (explosionIndicatorMaterial != null)
            Destroy(explosionIndicatorMaterial);
    }
}
