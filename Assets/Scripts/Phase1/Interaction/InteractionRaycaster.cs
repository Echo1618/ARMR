using UnityEngine;

public class InteractionRaycaster : MonoBehaviour
{
    [SerializeField] private InputReader m_inputReader;
    [SerializeField] private Camera m_camera;
    [SerializeField] private float m_maxDistance = 5f;
    [SerializeField] private LayerMask m_interactionLayer;

    private void Awake() {
        Debug.Assert(m_inputReader);
        Debug.Assert(m_camera);
    }

    private void OnEnable()
    {
        m_inputReader.Tapped += Interact;
    }
    private void OnDisable()
    {
        m_inputReader.Tapped -= Interact;
    }
    private void Interact(Vector2 p_screenPos)
    {
        Ray ray = m_camera.ScreenPointToRay(p_screenPos);

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            m_maxDistance,
            m_interactionLayer))
        {
            Debug.Log("[INFO] : No hit");
            return;
        }

        Debug.Log("[INFO] : Hit");

        if (!hit.collider.TryGetComponent(
            out IInteractable interactable))
            return;

        InteractionContext context = new InteractionContext(
            p_screenPos,
            ray,
            hit
        );

        if (!interactable.CanInteract(context))
            return;
        

        interactable.Interact(context);
    }
}
