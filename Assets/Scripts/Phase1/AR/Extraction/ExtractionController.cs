using UnityEngine;

public class ExtractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChaseController m_chaseController;
    [SerializeField] private GameObject m_extractionPanel;

    private CollectionZone m_currentZone;

    public bool IsExtracting => m_currentZone != null;

    private void Awake()
    {
        Debug.Assert(m_chaseController != null);

        if (m_extractionPanel != null) m_extractionPanel.SetActive(false);
    }

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

        return true;
    }

    public void CompleteExtraction()
    {
        if (m_currentZone == null) return;

        CollectionZone zone = m_currentZone;
        m_currentZone = null;

        if (m_extractionPanel != null) m_extractionPanel.SetActive(false);

        if (zone != null) zone.CompleteExtraction();

        if (m_chaseController.CurrentState == ChaseState.Extracting) m_chaseController.FinishExtracting();
    }

    public void CancelExtraction()
    {
        if (m_currentZone == null) return;

        CollectionZone zone = m_currentZone;
        m_currentZone = null;

        if (m_extractionPanel != null) m_extractionPanel.SetActive(false);

        if (zone != null) zone.CancelExtraction();

        if (m_chaseController.CurrentState == ChaseState.Extracting) m_chaseController.FinishExtracting();
    }
}