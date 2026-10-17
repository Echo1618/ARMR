using UnityEngine;

public class CrateOpeningController : MonoBehaviour
{
    [SerializeField] private ChaseController m_chaseController;
    [SerializeField] private GameObject m_openingPanel;

    private OpeningMachine m_currentMachine;
    public bool IsOpening => m_currentMachine != null;

    public bool StartOpening(OpeningMachine p_machine)
    {
        if (!p_machine) return false;
        if (m_chaseController.CurrentState != ChaseState.Exploring) return false;
        m_currentMachine = p_machine;
        m_chaseController.StartCrateOpening();
        m_openingPanel.SetActive(true);
        return false;
    }
    public void CompleteOpening()
    {

    }
    public void CancelOpening()
    {

    }
}
