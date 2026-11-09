using UnityEngine;

public abstract class DropItem : MonoBehaviour
{
    private bool isCollected = false;

     protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (isCollected)
            return;

        isCollected = true;

        Collect(other);

        Destroy(gameObject);
    }

    protected abstract void Collect(Collider player);
}
