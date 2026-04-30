using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerController : SerializedMonoBehaviour
{
    private const string AnimatorMovementSpeedParameter = "MovementSpeed";

    [SerializeField]
    [ShowInInspector]
    private Animator characterAnimator;

    [SerializeField]
    [ShowInInspector]
    private Camera mainCamera;

    [SerializeField]
    public float AccelerationRate = 10f;

    [SerializeField]
    public float WalkingSpeed = 2f;

    [SerializeField]
    public float RunningSpeed = 5f;

    [SerializeField]
    public float RotationSmoothTime = 0.12f;

    [SerializeField]
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [SerializeField]
    [ShowInInspector]
    private float slideAngleThreshold = 30f;

    [SerializeField]
    [ShowInInspector]
    private float slideFriction = 0f;

    [SerializeField]
    [ShowInInspector]
    private bool movementLogsEnabled = false;

    private PlayerStateMachine playerStateMachine;
    private PlayerInputSystem playerInputSystem;
    private CharacterController playerBody;

    private float movementSpeed;
    private float targetRotation;
    private float rotationVelocity;
    private float verticalVelocity;
    private Vector3 slideNormal;
    private bool isSliding = false;

    void Start()
    {
        playerStateMachine = GetComponent<PlayerStateMachine>();
        playerInputSystem = GetComponent<PlayerInputSystem>();
        playerBody = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    private void HandleGravity()
    {
        verticalVelocity += Gravity * Time.deltaTime;
    }

    private void HandleMovement()
    {
        var movementInput = playerInputSystem.Movement;

        var targetSpeed = playerInputSystem.IsRunning ? RunningSpeed : WalkingSpeed;

        if(movementLogsEnabled)
            Debug.Log($"Movement Input: {movementInput}, Target Speed: {targetSpeed}");

        if (movementInput == Vector2.zero)
            targetSpeed = 0f;

        float currentHorizontalSpeed = new Vector3(playerBody.velocity.x, 0.0f, playerBody.velocity.z).magnitude;

        movementSpeed = Mathf.MoveTowards(movementSpeed, targetSpeed, AccelerationRate * Time.deltaTime);

        var movementDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;

        if(movementLogsEnabled)
            Debug.Log($"Current Horizontal Speed: {currentHorizontalSpeed}, Movement Speed: {movementSpeed}, Movement Direction: {movementDirection}");

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (movementInput != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg +
                              mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity,
                RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }


        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        var movementMotion = targetDirection.normalized * (movementSpeed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime;
        movementMotion = ApplySlide(movementMotion);

        if(movementLogsEnabled)
            Debug.Log($"Movement Motion: {movementMotion}");

        // move the player
        playerBody.Move(movementMotion);

        characterAnimator.SetFloat(AnimatorMovementSpeedParameter, movementDirection.magnitude);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        float angleThresholdNormal = Mathf.Cos(slideAngleThreshold * Mathf.Deg2Rad);

        if (hit.normal.y < angleThresholdNormal)
        {
            slideNormal = hit.normal.normalized;
            isSliding = true;
        }
    }

    private Vector3 ApplySlide(Vector3 move)
    {
        if (!isSliding) return move;

        Vector3 horizontal = new Vector3(move.x, 0f, move.z);
        float intoWall = Vector3.Dot(horizontal, slideNormal);

        if (intoWall < 0f)
        {
            // Remove the into-wall component
            Vector3 deflected = horizontal - slideNormal * intoWall;

            // Restore original horizontal speed so the player
            // slides at full speed along the wall, not reduced speed
            if (deflected.magnitude > 0.001f)
                horizontal = deflected.normalized * horizontal.magnitude;
            else
                horizontal = Vector3.zero; // perfectly head-on — full stop
        }

        isSliding = false;
        return new Vector3(horizontal.x, move.y, horizontal.z);
    }
}
