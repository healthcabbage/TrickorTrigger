
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private BaseWeapon currentWeapon;

    [Header("Aim")]
    [SerializeField] private Camera mainCamera;

    [SerializeField] private Transform playerTransform;

    private float attackTimer;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (playerTransform == null)
            playerTransform = transform;
    }

    private void Update()
    {
        if (currentWeapon == null)
            return;

        attackTimer += Time.deltaTime;

        // 마우스 왼쪽 버튼을 누르고 있는 동안 공격
        if (!Input.GetMouseButton(0))
            return;

        if (attackTimer < currentWeapon.AttackInterval)
            return;

        Vector3 direction = GetMouseDirection();

        if (direction == Vector3.zero)
            return;

        currentWeapon.Attack(direction);

        attackTimer = 0f;
    }

    private Vector3 GetMouseDirection()
    {
        if (mainCamera == null)
            return Vector3.zero;

        Ray ray = mainCamera.ScreenPointToRay(
            Input.mousePosition
        );

        // 플레이어 높이와 같은 수평면을 기준으로 조준
        Plane groundPlane = new Plane(
            Vector3.up,
            new Vector3(
                0f,
                playerTransform.position.y,
                0f
            )
        );

        if (!groundPlane.Raycast(ray, out float distance))
            return Vector3.zero;

        Vector3 mouseWorldPosition =
            ray.GetPoint(distance);

        Vector3 direction =
            mouseWorldPosition - playerTransform.position;

        direction.y = 0f;

        return direction.normalized;
    }
}
