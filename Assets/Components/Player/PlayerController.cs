using Sirenix.OdinInspector;
using System;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;

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

    private PlayerInputSystem playerInputSystem;
    private CharacterController playerBody;

    private float movementSpeed;
    private float targetRotation;
    private float rotationVelocity;
    private float verticalVelocity;

    void Start()
    {
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

        if (movementInput == Vector2.zero)
            targetSpeed = 0f;

        float currentHorizontalSpeed = new Vector3(playerBody.velocity.x, 0.0f, playerBody.velocity.z).magnitude;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed || currentHorizontalSpeed > targetSpeed)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            movementSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                Time.deltaTime * AccelerationRate);

            // round speed to 3 decimal places
            movementSpeed = Mathf.Round(movementSpeed * 1000f) / 1000f;
        }
        else
        {
            movementSpeed = targetSpeed;
        }

        var movementDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;

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

        // move the player
        playerBody.Move(targetDirection.normalized * (movementSpeed * Time.deltaTime) +
                         new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

        characterAnimator.SetFloat(AnimatorMovementSpeedParameter, movementDirection.magnitude);
    }
}
