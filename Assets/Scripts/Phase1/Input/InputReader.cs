using System;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Its role is to interface between Unity's New Input System and our gameplay.
/// It listens to the Tap action, reads the Point position, and then triggers our own event by sending a Vector2.
/// </summary>
public class InputReader : MonoBehaviour
{
    //---------------------------------------------
    // Private Var
    //---------------------------------------------
    private MobileInputActions m_mobileInputActions;
    //---------------------------------------------
    // Public Var
    //---------------------------------------------
    public event Action<Vector2> Tapped;

    private void Awake()
    {
        //Create a MobileInputActions
        m_mobileInputActions = new MobileInputActions();
    }
    void OnEnable()
    {
        //Subscribe to the Tap event.
        m_mobileInputActions.Mobile.Tap.performed += Tap;
        m_mobileInputActions.Mobile.Enable();
    }
    void OnDisable()
    {
        //Unsubscribe to the Tap event.
        m_mobileInputActions.Mobile.Tap.performed -= Tap;
        m_mobileInputActions.Mobile.Disable();
    }

    private void Tap(InputAction.CallbackContext callbackContext)
    {
        //Trigger our Tapped event and sent the Tap position on screen to all subrscibed  
        Vector2 screenPosition = m_mobileInputActions.Mobile.Point.ReadValue<Vector2>();
        Tapped?.Invoke(screenPosition);
    }

    void OnDestroy()
    {
        m_mobileInputActions?.Dispose();
    }
}
