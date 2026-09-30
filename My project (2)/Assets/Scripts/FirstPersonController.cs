using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Basic first-person controller: WASD to move, mouse to look,
/// Shift to sprint, Space to jump, T / Y to decrease / increase the FOV.
/// Uses the new Input System (default in Unity 6), so no Input Actions asset is needed.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The camera that is a child of the player (used for looking up/down).")]
    public Transform cameraTransform;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Look")]
    public float mouseSensitivity = 0.1f;
    public float maxLookAngle = 85f;

    [Header("Field of View")]
    public float minFov = 50f;
    public float maxFov = 120f;
    [Tooltip("Degrees per second while holding T or Y.")]
    public float fovChangeSpeed = 40f;

    private CharacterController controller;
    private Camera playerCamera;
    private float verticalVelocity;
    private float cameraPitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform != null)
        {
            playerCamera = cameraTransform.GetComponent<Camera>();
            if (playerCamera != null)
                playerCamera.fieldOfView = Mathf.Clamp(playerCamera.fieldOfView, minFov, maxFov);
        }
    }

    private void OnEnable()
    {
        // Lock and hide the cursor while playing.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleFov();
    }

    private void HandleLook()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;

        // Rotate the whole body left/right.
        transform.Rotate(Vector3.up * mouseDelta.x);

        // Rotate only the camera up/down.
        cameraPitch = Mathf.Clamp(cameraPitch - mouseDelta.y, -maxLookAngle, maxLookAngle);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // Grounded state comes from the single Move() call of the previous frame.
        bool grounded = controller.isGrounded;

        // Read WASD input.
        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float z = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        Vector3 move = transform.right * x + transform.forward * z;
        move = Vector3.ClampMagnitude(move, 1f);
        float speed = kb.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;

        // Gravity and jumping.
        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // small downward force keeps the player stuck to the ground

        if (grounded && kb.spaceKey.wasPressedThisFrame)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;

        // Horizontal + vertical movement in ONE Move() call, so isGrounded stays reliable.
        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleFov()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || playerCamera == null) return;

        float input = 0f;
        if (kb.yKey.isPressed) input += 1f; // Y = increase FOV
        if (kb.tKey.isPressed) input -= 1f; // T = decrease FOV

        if (input != 0f)
        {
            float newFov = playerCamera.fieldOfView + input * fovChangeSpeed * Time.deltaTime;
            playerCamera.fieldOfView = Mathf.Clamp(newFov, minFov, maxFov);
        }
    }
}
