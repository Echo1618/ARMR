using UnityEngine;

/// <summary>
/// Controls one collection-zone extraction sequence at a time.
/// The current UI is a temporary MVP and can later be replaced by the real extraction mini-game.
/// </summary>
public class ExtractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChaseController m_chaseController;
    [SerializeField] private GameObject m_extractionPanel;

    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    private CollectionZone m_currentZone;

    public bool IsExtracting => m_currentZone != null;

    private void Awake()
    {
        Debug.Assert(m_chaseController != null);
        if (m_extractionPanel != null) m_extractionPanel.SetActive(false);
    }

    /// <summary>Starts extraction if no other extraction is active and the Chase is exploring.</summary>
    public bool StartExtraction(CollectionZone p_zone)
    {
        if (p_zone == null) return false;
        if (m_currentZone != null) return false;
        if (m_chaseController.CurrentState != ChaseState.Exploring) return false;

        m_currentZone = p_zone;
        m_chaseController.StartExtracting();

        if (m_chaseController.CurrentState != ChaseState.Extracting)
        {
            m_currentZone = null;
            return false;
        }

        if (m_extractionPanel != null) m_extractionPanel.SetActive(true);
        if (m_debugMode) Debug.Log("[ExtractionController] Extraction started.");
        return true;
    }

    /// <summary>Validates the extraction and returns the Chase to Exploring.</summary>
    public void CompleteExtraction()
    {
        if (m_currentZone == null) return;

        CollectionZone zone = m_currentZone;
        m_currentZone = null;

        if (m_extractionPanel != null) m_extractionPanel.SetActive(false);
        if (zone != null) zone.CompleteExtraction();
        if (m_chaseController.CurrentState == ChaseState.Extracting) m_chaseController.FinishExtracting();

        if (m_debugMode) Debug.Log("[ExtractionController] Extraction completed.");
    }

    /// <summary>Cancels extraction without granting a crate.</summary>
    public void CancelExtraction()
    {
        if (m_currentZone == null) return;

        CollectionZone zone = m_currentZone;
        m_currentZone = null;

        if (m_extractionPanel != null) m_extractionPanel.SetActive(false);
        if (zone != null) zone.CancelExtraction();
        if (m_chaseController.CurrentState == ChaseState.Extracting) m_chaseController.FinishExtracting();

        if (m_debugMode) Debug.Log("[ExtractionController] Extraction cancelled.");
    }
}
