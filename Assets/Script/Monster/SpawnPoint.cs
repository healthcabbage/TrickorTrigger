using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform center;
    [SerializeField] private float spawnRadius = 15f;
    [SerializeField] private float groundHeight = 0f;

    public Vector3 GetRandomSpawnPosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        Vector3 direction = new Vector3(
            Mathf.Cos(angle),
            0f,
            Mathf.Sin(angle)
        );

        Vector3 spawnPosition =
            center.position + direction * spawnRadius;

        spawnPosition.y = groundHeight;

        return spawnPosition;
    }
}