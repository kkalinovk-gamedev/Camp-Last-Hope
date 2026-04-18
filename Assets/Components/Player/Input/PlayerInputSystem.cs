using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : SerializedMonoBehaviour
{
    public Vector2 Movement { get; private set; } = Vector2.zero;
    public bool IsRunning { get; private set; } = false;

    public void OnMove(InputAction.CallbackContext context)
    {
        Movement = context.ReadValue<Vector2>();
        
        // Handle movement input here
        Debug.Log($"Move input: {Movement}");
    }
}
