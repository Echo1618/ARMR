using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Interface between Unity's New Input System and Chase gameplay.
/// Converts the generated Tap/Point actions into a simple Tapped(Vector2) event.
/// </summary>
public class InputReader : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    private MobileInputActions m_mobileInputActions;

    public event Action<Vector2> Tapped;

    private void Awake()
    {
        m_mobileInputActions = new MobileInputActions();
    }

    private void OnEnable()
    {
        m_mobileInputActions.Mobile.Tap.performed += Tap;
        m_mobileInputActions.Mobile.Enable();
    }

    private void OnDisable()
    {
        m_mobileInputActions.Mobile.Tap.performed -= Tap;
        m_mobileInputActions.Mobile.Disable();
    }

    /// <summary>Reads the current pointer/touch position and emits the project-level tap event.</summary>
    private void Tap(InputAction.CallbackContext p_context)
    {
        Vector2 screenPosition = m_mobileInputActions.Mobile.Point.ReadValue<Vector2>();
        if (m_debugMode) Debug.Log($"[InputReader] Tap at {screenPosition}");
        Tapped?.Invoke(screenPosition);
    }

    private void OnDestroy()
    {
        m_mobileInputActions?.Dispose();
    }
}
