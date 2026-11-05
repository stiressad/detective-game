using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4.5f;
    public float runSpeed = 7f;
    public float acceleration = 0.12f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;

    [Header("Head Bobbing")]
    public bool enableHeadBob = true;
    public float walkBobFrequency = 1.9f;
    public float runBobFrequency = 2.7f;
    public float bobHeight = 0.045f;
    public float bobSide = 0.025f;
    public float bobSmoothness = 10f;

    [Header("Camera Tilt")]
    public bool enableTilt = true;
    public float strafeTilt = 1.5f;
    public float tiltSmoothness = 8f;

    [Header("Landing Effect")]
    public float landingDip = 0.09f;
    public float landingRecovery = 0.7f;

    [Header("Running FOV")]
    public float runFovIncrease = 3f;
    public float fovSmoothness = 5f;

    private CharacterController controller;
    private Camera cam;

    private Vector3 horizontalVelocity;
    private Vector3 smoothVelocity;
    private float verticalVelocity;

    private Vector3 cameraStartPosition;
    private float cameraPitch;
    private float currentTilt;
    private float bobTimer;
    private float landingOffset;
    private float normalFov;

    private Vector2 moveInput;
    private bool isRunning;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();

            if (childCamera != null)
                playerCamera = childCamera.transform;
        }

        if (playerCamera == null)
        {
            Debug.LogError("Player Camera is missing!");
            enabled = false;
            return;
        }

        cam = playerCamera.GetComponent<Camera>();

        cameraStartPosition = playerCamera.localPosition;

        if (cam != null)
            normalFov = cam.fieldOfView;

        LockCursor();
    }

    void Update()
    {
        HandleCursor();
        HandleMouseLook();
        HandleMovement();
        HandleCameraEffects();
    }

    void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) &&
        Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        moveInput = new Vector2(
        Input.GetAxisRaw("Horizontal"),
        Input.GetAxisRaw("Vertical")
        );

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        bool wasGrounded = controller.isGrounded;

        isRunning =
        Input.GetKey(KeyCode.LeftShift) &&
        moveInput.y > 0.1f &&
        wasGrounded;

        float speed = isRunning ? runSpeed : walkSpeed;

        Vector3 targetVelocity =
        (transform.right * moveInput.x +
        transform.forward * moveInput.y) * speed;

        horizontalVelocity = Vector3.SmoothDamp(
        horizontalVelocity,
        targetVelocity,
        ref smoothVelocity,
        acceleration
        );

        if (wasGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (wasGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity =
            Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = horizontalVelocity;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);

        // Landing effect
        if (!wasGrounded &&
        controller.isGrounded &&
        verticalVelocity < -4f)
        {
            landingOffset = landingDip;
        }

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
    }

    void HandleCameraEffects()
    {
        float dt = Time.deltaTime;

        bool moving =
        controller.isGrounded &&
        moveInput.sqrMagnitude > 0.01f &&
        horizontalVelocity.magnitude > 0.15f;

        Vector3 targetPosition = cameraStartPosition;

        // Head bobbing
        if (enableHeadBob && moving)
        {
            float frequency = isRunning
            ? runBobFrequency
            : walkBobFrequency;

            float maxSpeed = isRunning
            ? runSpeed
            : walkSpeed;

            float speedFactor = Mathf.Clamp01(
            horizontalVelocity.magnitude / maxSpeed
            );

            bobTimer += dt * frequency *
            Mathf.PI * 2f * speedFactor;

            float intensity = isRunning ? 1.35f : 1f;

            float verticalBob =
            Mathf.Sin(bobTimer) *
            bobHeight * intensity * speedFactor;

            float horizontalBob =
            Mathf.Cos(bobTimer * 0.5f) *
            bobSide * intensity * speedFactor;

            targetPosition += new Vector3(
            horizontalBob,
            verticalBob,
            0f
            );
        }

        // Landing camera dip
        targetPosition.y -= landingOffset;

        landingOffset = Mathf.MoveTowards(
        landingOffset,
        0f,
        landingRecovery * dt
        );

        // Smooth camera position
        float positionLerp =
        1f - Mathf.Exp(-bobSmoothness * dt);

        playerCamera.localPosition = Vector3.Lerp(
        playerCamera.localPosition,
        targetPosition,
        positionLerp
        );

        // Camera roll when strafing
        float targetTilt = 0f;

        if (enableTilt && controller.isGrounded)
        {
            targetTilt = -moveInput.x * strafeTilt;
        }

        float tiltLerp =
        1f - Mathf.Exp(-tiltSmoothness * dt);

        currentTilt = Mathf.Lerp(
        currentTilt,
        targetTilt,
        tiltLerp
        );

        playerCamera.localRotation = Quaternion.Euler(
        cameraPitch,
        0f,
        currentTilt
        );

        // Dynamic FOV while running
        if (cam != null)
        {
            float targetFov = normalFov;

            if (isRunning && moving)
                targetFov += runFovIncrease;

            float fovLerp =
            1f - Mathf.Exp(-fovSmoothness * dt);

            cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFov,
            fovLerp
            );
        }
    }
}