using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : SerializedMonoBehaviour
{
    [ShowInInspector, ReadOnly]
    public Vector2 Movement { get; private set; } = Vector2.zero;

    [ShowInInspector, ReadOnly]
    public bool IsRunning { get; private set; } = false;

    public void OnMove(InputAction.CallbackContext context)
    {
        Movement = context.ReadValue<Vector2>();
    }
}
