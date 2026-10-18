using System;
using UnityEngine;

/// <summary>
/// Interactive AR collection anomaly.
/// It delegates extraction gameplay to ExtractionController and becomes consumed on success.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CollectionZone : MonoBehaviour, IInteractable
{
    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

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

    /// <summary>Injects scene dependencies after the prefab is spawned.</summary>
    public void Initialize(ChaseController p_chaseController, ExtractionController p_extractionController)
    {
        m_chaseController = p_chaseController;
        m_extractionController = p_extractionController;
        if (m_debugMode) Debug.Log("[CollectionZone] Initialized.");
    }

    /// <summary>Returns true only while the Chase is freely exploring and this zone is available.</summary>
    public bool CanInteract(InteractionContext p_context)
    {
        if (m_chaseController == null || m_extractionController == null) return false;
        if (m_isConsumed || m_isExtracting) return false;
        if (m_chaseController.CurrentState != ChaseState.Exploring) return false;
        return true;
    }

    /// <summary>Starts the extraction sequence through ExtractionController.</summary>
    public void Interact(InteractionContext p_context)
    {
        if (!CanInteract(p_context)) return;

        if (m_extractionController.StartExtraction(this))
        {
            m_isExtracting = true;
            if (m_collider != null) m_collider.enabled = false;
            if (m_debugMode) Debug.Log("[CollectionZone] Extraction started.");
        }
    }

    /// <summary>Grants one crate, notifies ChaseDirector and destroys this zone.</summary>
    public void CompleteExtraction()
    {
        if (!m_isExtracting || m_isConsumed) return;

        m_isExtracting = false;
        m_isConsumed = true;
        m_chaseController.ChaseProgress.AddCrate();

        if (m_debugMode) Debug.Log("[CollectionZone] Extraction completed. Crate granted.");
        Consumed?.Invoke(this);
        Destroy(gameObject);
    }

    /// <summary>Cancels extraction and makes the same zone interactable again.</summary>
    public void CancelExtraction()
    {
        if (!m_isExtracting || m_isConsumed) return;

        m_isExtracting = false;
        if (m_collider != null) m_collider.enabled = true;
        if (m_debugMode) Debug.Log("[CollectionZone] Extraction cancelled.");
    }
}
