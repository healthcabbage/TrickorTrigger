using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField]
    private Vector2 spawnArea = new Vector2(20f, 20f);

    public Vector3 GetRandomSpawnPosition()
    {
        float halfWidth = spawnArea.x * 0.5f;
        float halfHeight = spawnArea.y * 0.5f;

        // 사각형의 네 변 중 하나 선택
        int side = Random.Range(0, 4);

        float x = 0f;
        float z = 0f;

        switch (side)
        {
            case 0: // 위쪽
                x = Random.Range(-halfWidth, halfWidth);
                z = halfHeight;
                break;

            case 1: // 아래쪽
                x = Random.Range(-halfWidth, halfWidth);
                z = -halfHeight;
                break;

            case 2: // 왼쪽
                x = -halfWidth;
                z = Random.Range(-halfHeight, halfHeight);
                break;

            case 3: // 오른쪽
                x = halfWidth;
                z = Random.Range(-halfHeight, halfHeight);
                break;
        }

        return new Vector3(x, 1f, z);
    }
}
