using System;
using UnityEngine;

public class CollectionZone : MonoBehaviour, IInteractable
{
    private ChaseController m_chaseController;
    private ExtractionController m_extractionController;
    private Collider m_collider;

    private bool m_isExtracting = false;
    private bool m_isConsumed = false;

    public event Action<CollectionZone> Consumed;

    private void Awake()
    {
        m_collider = GetComponent<Collider>();
        Debug.Assert(m_collider != null);
    }

    public void Initialize(ChaseController p_chaseController, ExtractionController p_extractionController)
    {
        m_chaseController = p_chaseController;
        m_extractionController = p_extractionController;
    }

    public bool CanInteract(InteractionContext p_context)
    {
        if (m_chaseController == null || m_extractionController == null) return false;
        if (m_isConsumed || m_isExtracting) return false;
        if (m_chaseController.CurrentState != ChaseState.Exploring) return false;

        return true;
    }

    public void Interact(InteractionContext p_context)
    {
        if (!CanInteract(p_context)) return;

        if (m_extractionController.StartExtraction(this))
        {
            m_isExtracting = true;
            if (m_collider != null) m_collider.enabled = false;
        }
    }

    public void CompleteExtraction()
    {
        if (!m_isExtracting || m_isConsumed) return;

        m_isExtracting = false;
        m_isConsumed = true;

        m_chaseController.ChaseProgress.AddCrate();

        Consumed?.Invoke(this);

        Destroy(gameObject);
    }

    public void CancelExtraction()
    {
        if (!m_isExtracting || m_isConsumed) return;

        m_isExtracting = false;
        if (m_collider != null) m_collider.enabled = true;
    }
}