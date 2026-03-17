using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed   = 5f;
    public float sprintSpeed = 9f;
    public float acceleration = 20f;   // how quickly speed ramps up
    public float rotationSmoothTime = 0.1f;
    private float _rotationVelocity;

    [Header("Jumping & Gravity")]
    public float gravity      = -18f;  // negative = downward
    public float groundedGravity = -2f;

    [Header("Target")]
    public Transform target;
    public float chaseDistance;
    public float idleRotationSpeed;
    private int rotationDirection = 1;
    private bool inRange = false;

    private CharacterController cc;
    private Animator anim;

    private Vector3 velocity = Vector3.zero; // world-space velocity
    private Vector3 moveVelocity = Vector3.zero; // horizontal velocity (smoothed)
    private bool isSprinting  = false;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        RandomRotation();
        if(!target)
        {
            target = FindFirstObjectByType<FirstPersonController>().transform;
        }
    }

    void RandomRotation()
    {
        rotationDirection = Random.value > 0.5f ? 1 : -1;
    }

    void Update()
    {
        CheckDistance();
        HandleRotation();
        HandleMovement();
    }

    void CheckDistance()
    {
        bool changed = inRange;
        inRange = Vector3.Distance(transform.position, target.position) < chaseDistance;
        if(!inRange && changed!=inRange)
        {
            RandomRotation();
        }
    }

    void HandleRotation()
    {
        if (inRange) {
            Vector3 targetDirection = target.position - transform.position;
            float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

            float smoothAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref _rotationVelocity,
                rotationSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }else {
            transform.Rotate(0f, idleRotationSpeed * rotationDirection * Time.deltaTime, 0f);
        }
    }

    void HandleMovement()
    {
        bool grounded = cc.isGrounded;

        // Ground gravity reset
        if (grounded && velocity.y < 0f)
            velocity.y = groundedGravity;

        // Input axes
        Vector3 inputDir = new Vector3(0, 0f, 1).normalized;

        // Sprint / crouch speed selection
        isSprinting = inRange;
        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // Convert input to world space
        Vector3 desiredMove = transform.TransformDirection(inputDir) * targetSpeed;

        moveVelocity = Vector3.MoveTowards(moveVelocity, desiredMove, acceleration * Time.deltaTime);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Combine horizontal + vertical
        Vector3 motion = moveVelocity + Vector3.up * velocity.y;
        cc.Move(motion * Time.deltaTime);
        
        float currentSpeed = cc.velocity.magnitude;
        anim.SetFloat("Speed", Mathf.Clamp01(currentSpeed / sprintSpeed));
    }
}
