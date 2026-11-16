using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("체력")]
    [SerializeField, Min(1f)] private float maxHealth = 100f;

    [Header("이동")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float gravity = -20f;

    [Header("회전")]
    [SerializeField] private float rotationSpeed = 1080f;

    private CharacterController controller;
    private Camera mainCamera;
    private float verticalVelocity;
    private float shieldTimeRemaining;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0f;
    public bool IsShielded => shieldTimeRemaining > 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || IsShielded || damage <= 0f)
            return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        if (IsDead)
        {
            shieldTimeRemaining = 0f;
            enabled = false;
        }
    }

    public void GrantShield(float duration)
    {
        if (IsDead || duration <= 0f)
            return;

        shieldTimeRemaining = Mathf.Max(shieldTimeRemaining, duration);
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }

    // Update is called once per frame
    void Update()
    {
        shieldTimeRemaining = Mathf.Max(0f, shieldTimeRemaining - Time.deltaTime);
        Move();
        LookAtMouse();
    }

    private void Move()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        Vector2 input = Vector2.zero;
        if (keyboard.wKey.isPressed) input.y += 1f;
        if (keyboard.sKey.isPressed) input.y -= 1f;
        if (keyboard.dKey.isPressed) input.x += 1f;
        if (keyboard.aKey.isPressed) input.x -= 1f;

        input = Vector2.ClampMagnitude(input, 1f);

        Vector3 velocity = new Vector3(input.x, 0f, input.y) * moveSpeed;
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private void LookAtMouse()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(mouse.position.ReadValue());

        Plane ground = new Plane(Vector3.up, transform.position);
        if (!ground.Raycast(ray, out float distance)) return;
        Vector3 mousePoint = ray.GetPoint(distance);

        Vector3 direction = mousePoint - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion target = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, target, rotationSpeed * Time.deltaTime);
    }
}
