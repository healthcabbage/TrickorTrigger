using UnityEngine;

public class SoulDrop : MonoBehaviour
{
    private float healAmount;
    private float lifetime = 12f;
    private float bobTime;
    private Material soulMaterial;

    public static void Spawn(Vector3 position, float healAmount)
    {
        GameObject soul = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        soul.name = "Healing Soul";
        soul.transform.position = position + Vector3.up * 0.65f;
        soul.transform.localScale = Vector3.one * 0.32f;

        SphereCollider trigger = soul.GetComponent<SphereCollider>();
        trigger.isTrigger = true;

        Rigidbody body = soul.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        SoulDrop drop = soul.AddComponent<SoulDrop>();
        drop.healAmount = healAmount;
        drop.soulMaterial = CreateSoulMaterial();
        Renderer renderer = soul.GetComponent<Renderer>();
        renderer.sharedMaterial = drop.soulMaterial;
    }

    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        bobTime += Time.deltaTime * 3f;
        Vector3 position = transform.position;
        position.y += Mathf.Sin(bobTime) * Time.deltaTime * 0.3f;
        transform.position = position;
        transform.Rotate(Vector3.up, 100f * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null || player.IsDead)
            return;

        player.Heal(healAmount);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (soulMaterial != null)
            Destroy(soulMaterial);
    }

    private static Material CreateSoulMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");
        if (shader == null)
        {
            Debug.LogWarning("영혼 드랍에 사용할 셰이더를 찾을 수 없습니다.");
            return null;
        }

        Material material = new Material(shader);
        Color soulColor = new Color(0.25f, 1f, 0.75f, 1f);
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", soulColor);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", soulColor);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", soulColor * 1.5f);
        }

        return material;
    }
}
