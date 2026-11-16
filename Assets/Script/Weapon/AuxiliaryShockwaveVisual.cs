using UnityEngine;
using System.Collections.Generic;

public class AuxiliaryShockwaveVisual : MonoBehaviour
{
    private LineRenderer line;
    private Material materialInstance;
    private Vector3 center;
    private float maxRadius;
    private float duration;
    private float timer;
    private float previousRadius;
    private float knockbackPower;
    private readonly HashSet<BaseMonster> hitMonsters = new HashSet<BaseMonster>();

    public static void Show(Vector3 center, float radius, float duration, float knockbackPower)
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");
        if (shader == null)
            return;

        GameObject visual = new GameObject("Auxiliary Pumpkin Shockwave");
        AuxiliaryShockwaveVisual shockwave = visual.AddComponent<AuxiliaryShockwaveVisual>();
        shockwave.Initialize(center, radius, duration, knockbackPower, shader);
    }

    private void Initialize(Vector3 origin, float radius, float life, float knockback, Shader shader)
    {
        center = origin + Vector3.up * 0.08f;
        maxRadius = radius;
        duration = Mathf.Max(0.05f, life);
        knockbackPower = knockback;
        materialInstance = new Material(shader);
        line = gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = true;
        line.positionCount = 48;
        line.widthMultiplier = 0.1f;
        line.startColor = new Color(0.7f, 0.45f, 1f, 0.9f);
        line.endColor = line.startColor;
        line.sharedMaterial = materialInstance;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.alignment = LineAlignment.View;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / duration);
        float radius = Mathf.Lerp(0f, maxRadius, progress);
        ApplyWaveKnockback(radius);
        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = 2f * Mathf.PI * i / line.positionCount;
            line.SetPosition(i, center + new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius));
        }

        Color color = new Color(0.7f, 0.45f, 1f, 0.9f * (1f - progress));
        line.startColor = color;
        line.endColor = color;
        if (progress >= 1f)
            Destroy(gameObject);
        previousRadius = radius;
    }

    private void ApplyWaveKnockback(float currentRadius)
    {
        Collider[] hits = Physics.OverlapSphere(
            center,
            currentRadius,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            BaseMonster monster = hit.GetComponentInParent<BaseMonster>();
            if (monster == null || monster.IsDead || hitMonsters.Contains(monster))
                continue;

            Vector3 direction = monster.transform.position - center;
            direction.y = 0f;
            float distance = direction.magnitude;
            if (distance > currentRadius || distance <= previousRadius)
                continue;

            hitMonsters.Add(monster);
            if (direction.sqrMagnitude > 0.001f)
                monster.ApplyKnockback(direction.normalized, knockbackPower);
        }
    }

    private void OnDestroy()
    {
        if (materialInstance != null)
            Destroy(materialInstance);
    }
}
