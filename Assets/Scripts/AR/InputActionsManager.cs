using UnityEngine;

/// <summary>
/// Manage the current Input Actions
/// </summary>
public class InputActionsManager : MonoBehaviour
{
    /// <summary>
    /// Instance of the Mobile Input Actions
    /// </summary>
    static private MobileInputActions m_InstanceInputActions;

    /// <summary>
    /// Get Instance of the Mobile Input Actions
    /// </summary>
    static public MobileInputActions InstanceInputActions
    {
        get { return m_InstanceInputActions; }
    }

    private void Awake()
    {
        // Instanciate Input Action
        m_InstanceInputActions = new MobileInputActions();
    }

    void Start()
    {
        // Enable Input Action
        m_InstanceInputActions.Enable();
    }

}
