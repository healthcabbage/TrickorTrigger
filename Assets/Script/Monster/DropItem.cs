using UnityEngine;

public class DropItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Destroy(gameObject);
    }
}
