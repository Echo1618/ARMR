using UnityEngine;

/// <summary>
/// Converts screen taps into 3D physics raycasts and dispatches interactions to IInteractable objects.
/// </summary>
public class InteractionRaycaster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader m_inputReader;
    [SerializeField] private Camera m_camera;

    [Header("Raycast")]
    [SerializeField] private float m_maxDistance = 5f;
    [SerializeField] private LayerMask m_interactionLayer;

    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    private void Awake()
    {
        Debug.Assert(m_inputReader != null);
        Debug.Assert(m_camera != null);
    }

    private void OnEnable()
    {
        m_inputReader.Tapped += Interact;
    }

    private void OnDisable()
    {
        m_inputReader.Tapped -= Interact;
    }

    /// <summary>Raycasts from the camera through the tapped screen position.</summary>
    private void Interact(Vector2 p_screenPos)
    {
        Ray ray = m_camera.ScreenPointToRay(p_screenPos);

        if (!Physics.Raycast(ray, out RaycastHit hit, m_maxDistance, m_interactionLayer))
        {
            if (m_debugMode) Debug.Log("[InteractionRaycaster] No interactable collider hit.");
            return;
        }

        if (m_debugMode) Debug.Log($"[InteractionRaycaster] Hit: {hit.collider.name}");

        if (!hit.collider.TryGetComponent(out IInteractable interactable)) return;

        InteractionContext context = new InteractionContext(p_screenPos, ray, hit);
        if (!interactable.CanInteract(context)) return;

        interactable.Interact(context);
    }
}
