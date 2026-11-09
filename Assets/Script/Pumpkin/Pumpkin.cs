using UnityEngine;

public class Pumpkin : MonoBehaviour
{
    [Header("State")]
    [SerializeField]
    private PumpkinState stats;

    private float currentHealth;

        private void Awake()
    {
        if (stats == null)
        {
            Debug.LogError("PumpkinStats가 연결되지 않았습니다.");
            return;
        }

        currentHealth = stats.maxHealth;
        Debug.Log(currentHealth);
    }

        public void TakeDamage(float damage)
    {
        float finalDamage = Mathf.Max(
            0f,
            damage - stats.defense
        );

        currentHealth -= finalDamage;

        currentHealth = Mathf.Max(
            0f,
            currentHealth
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

        public void Heal(float amount)
    {
        currentHealth += amount;

        currentHealth = Mathf.Min(
            currentHealth,
            stats.maxHealth
        );
    }

    private void Die()
    {
        this.gameObject.SetActive(false);
        Debug.Log("호박 파괴!");
    }
    
}


