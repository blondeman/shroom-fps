using UnityEngine;

/// <summary>
/// First Person Character Controller
/// 
/// Setup:
///   1. Add this script to a GameObject with a CharacterController component.
///   2. Create a Camera as a child of the GameObject and assign it to 'playerCamera'.
///   3. Adjust inspector values to taste.
///
/// Features:
///   - WASD / arrow key movement
///   - Mouse look (pitch + yaw)
///   - Jumping
///   - Sprinting (hold Left Shift)
///   - Crouching (hold Left Ctrl)
///   - Gravity + slope handling
///   - Cursor lock/unlock (Escape)
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────

    [Header("Camera")]
    public Camera playerCamera;
    [Range(1f, 20f)]  public float mouseSensitivity   = 5f;
    [Range(-90f, 0f)] public float minPitch            = -85f;
    [Range(0f, 90f)]  public float maxPitch            =  85f;
    public bool invertY = false;

    [Header("Movement")]
    public float walkSpeed   = 5f;
    public float sprintSpeed = 9f;
    public float crouchSpeed = 2.5f;
    public float acceleration = 12f;   // how quickly speed ramps up
    public float deceleration = 10f;   // how quickly it stops

    [Header("Jumping & Gravity")]
    public float jumpHeight   = 1.2f;
    public float coyoteTime = 0.15f;
    public float gravity      = -18f;  // negative = downward
    public float groundedGravity = -2f; // small constant gravity when grounded (prevents floating)

    [Header("Crouching")]
    public float standHeight  = 2f;
    public float crouchHeight = 1f;
    public float crouchTransitionSpeed = 8f;
    private float targetHeight;
    // Camera local Y positions (relative to the Player GameObject's origin at its feet)
    // Standing: near top of 2m capsule; crouching: near top of 1m capsule
    private float cameraStandY  = 1.7f;
    private float cameraCrouchY = 0.85f;

    // ── Private state ─────────────────────────────────────────────────────────

    private CharacterController cc;
    private float   pitch        = 0f;           // vertical camera angle
    private Vector3 velocity     = Vector3.zero; // world-space velocity
    private Vector3 moveVelocity = Vector3.zero; // horizontal velocity (smoothed)
    private bool    isCrouching  = false;
    private bool    isSprinting  = false;
    private float   coyoteTimer  = 0f;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        // Initialise heights
        cc.height = standHeight;
        targetHeight = standHeight;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
        HandleCursorLock();
        HandleMouseLook();
        HandleCrouch();
        HandleMovement();
    }

    // ── Cursor ────────────────────────────────────────────────────────────────

    void HandleCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible   = locked;
        }
    }

    // ── Mouse look ────────────────────────────────────────────────────────────

    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * (invertY ? 1f : -1f);

        // Yaw — rotate the whole character
        transform.Rotate(Vector3.up * mouseX);

        // Pitch — rotate only the camera
        pitch = Mathf.Clamp(pitch + mouseY, minPitch, maxPitch);
        if (playerCamera != null)
        {
            Vector3 camEuler = playerCamera.transform.localEulerAngles;
            playerCamera.transform.localEulerAngles = new Vector3(pitch, camEuler.y, camEuler.z);
        }
    }

    // ── Crouching ─────────────────────────────────────────────────────────────

    void HandleCrouch()
    {
        bool wantCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);

        // Try to stand up — check there's headroom
        if (isCrouching && !wantCrouch)
        {
            if (!CeilingAbove())
            {
                isCrouching = false;
                targetHeight = standHeight;
            }
        }
        else if (!isCrouching && wantCrouch)
        {
            isCrouching = true;
            targetHeight = crouchHeight;
        }

        // Smoothly transition controller height
        cc.height = Mathf.Lerp(cc.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        cc.center = new Vector3(0f, cc.height / 2f, 0f);

        // Move camera to match — lerp to explicit eye-height positions
        if (playerCamera != null)
        {
            float targetCamY = isCrouching ? cameraCrouchY : cameraStandY;
            float newCamY = Mathf.Lerp(
                playerCamera.transform.localPosition.y,
                targetCamY,
                Time.deltaTime * crouchTransitionSpeed);

            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                newCamY,
                playerCamera.transform.localPosition.z);
        }
    }

    bool CeilingAbove()
    {
        // Cast upward from the top of the crouched controller
        Vector3 top = transform.position + Vector3.up * (crouchHeight - 0.1f);
        return Physics.SphereCast(top, cc.radius, Vector3.up, out _, standHeight - crouchHeight + 0.1f);
    }

    // ── Movement & gravity ────────────────────────────────────────────────────

    void HandleMovement()
    {
        bool grounded = cc.isGrounded;

        // Coyote time — count down after leaving ground
        if (grounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // Ground gravity reset
        if (grounded && velocity.y < 0f)
            velocity.y = groundedGravity;

        // Input axes
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        // Sprint / crouch speed selection
        isSprinting = grounded && !isCrouching && Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = isCrouching ? crouchSpeed :
                            isSprinting  ? sprintSpeed : walkSpeed;

        // Convert input to world space
        Vector3 desiredMove = transform.TransformDirection(inputDir) * targetSpeed;

        // Smooth acceleration / deceleration
        float accel = inputDir.sqrMagnitude > 0.01f ? acceleration : deceleration;
        moveVelocity = Vector3.MoveTowards(moveVelocity, desiredMove, accel * Time.deltaTime);

        // Jump — allowed while grounded or within coyote window
        if (Input.GetButtonDown("Jump") && coyoteTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            coyoteTimer = 0f; // consume the window
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Combine horizontal + vertical
        Vector3 motion = moveVelocity + Vector3.up * velocity.y;
        cc.Move(motion * Time.deltaTime);
    }

    // ── Gizmos (editor only) ──────────────────────────────────────────────────

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, 0.3f);
    }
#endif
}