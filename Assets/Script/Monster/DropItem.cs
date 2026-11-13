using UnityEngine;

public class DropItem : MonoBehaviour
{
    private int experienceAmount;
    private bool isCollected;

    public void Initialize(int amount)
    {
        experienceAmount = amount;
        isCollected = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected)
            return;

        PlayerExperience playerExperience =
            other.GetComponentInParent<PlayerExperience>();

        if (playerExperience == null)
            return;

        isCollected = true;

        playerExperience.AddExperience(experienceAmount);

        Destroy(gameObject);
    }
}