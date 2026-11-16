using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Weapon Choices")]
    [SerializeField] private BaseWeapon[] availableWeapons;
    [SerializeField] private BaseWeapon currentWeapon;

    [Header("Aim")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform playerTransform;

    private float attackTimer;
    private bool attackQueued;
    private bool gameplayActive = true;

    public BaseWeapon[] AvailableWeapons => availableWeapons;
    public BaseWeapon CurrentWeapon => currentWeapon;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (playerTransform == null)
            playerTransform = transform;

        if (availableWeapons != null && availableWeapons.Length > 0)
        {
            if (currentWeapon == null)
                currentWeapon = availableWeapons[0];

            for (int i = 0; i < availableWeapons.Length; i++)
            {
                if (availableWeapons[i] != null)
                    availableWeapons[i].SetSelected(false);
            }

            currentWeapon = null;
        }
    }

    private void Update()
    {
        if (!gameplayActive || currentWeapon == null)
            return;

        attackTimer += Time.deltaTime;
        Vector3 direction = GetMouseDirection();
        currentWeapon.SetAimDirection(direction);

        if (Input.GetMouseButtonDown(0))
            attackQueued = true;

        if (!attackQueued || attackTimer < currentWeapon.AttackInterval || direction == Vector3.zero)
            return;

        currentWeapon.Attack(direction);
        attackTimer = 0f;
        attackQueued = false;
    }

    public bool ChooseWeapon(int index)
    {
        if (availableWeapons == null || index < 0 || index >= availableWeapons.Length || availableWeapons[index] == null)
            return false;

        if (currentWeapon != null)
            currentWeapon.SetSelected(false);

        currentWeapon = availableWeapons[index];
        currentWeapon.SetSelected(true);
        attackTimer = currentWeapon.AttackInterval;
        attackQueued = false;
        gameplayActive = true;
        return true;
    }

    public void SetGameplayActive(bool active)
    {
        gameplayActive = active;
        attackQueued = false;
        if (currentWeapon != null)
            currentWeapon.SetSelected(active);
    }

    private Vector3 GetMouseDirection()
    {
        if (mainCamera == null)
            return Vector3.zero;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(
            Vector3.up,
            new Vector3(0f, playerTransform.position.y, 0f));

        if (!groundPlane.Raycast(ray, out float distance))
            return Vector3.zero;

        Vector3 mouseWorldPosition = ray.GetPoint(distance);
        Vector3 direction = mouseWorldPosition - playerTransform.position;
        direction.y = 0f;
        return direction.normalized;
    }
}
