using UnityEngine;

public class OpeningMachine : MonoBehaviour, IInteractable
{
    private ChaseController m_chaseController;
    private CrateOpeningController m_crateOpeningController;

    public void Initialize(ChaseController p_chaseController, CrateOpeningController p_crateOpeningController)
    {
        m_chaseController = p_chaseController;
        m_crateOpeningController = p_crateOpeningController;
    }

    public bool CanInteract(InteractionContext p_context)
    {
        if (m_chaseController == null || m_crateOpeningController == null) return false;
        if (m_chaseController.CurrentState != ChaseState.Exploring) return false;
        if (m_chaseController.ChaseProgress.UnopenedCrateCount <= 0) return false;

        return true;
    }

    public void Interact(InteractionContext p_context)
    {
        if (!CanInteract(p_context)) return;

        m_crateOpeningController.StartOpening(this);
    }
}
