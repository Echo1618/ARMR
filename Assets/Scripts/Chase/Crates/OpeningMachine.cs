using UnityEngine;

/// <summary>
/// Permanent interactive machine used to start crate opening.
/// It only validates whether interaction is currently allowed and delegates the sequence to CrateOpeningController.
/// </summary>
[RequireComponent(typeof(Collider))]
public class OpeningMachine : MonoBehaviour, IInteractable
{
    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    private ChaseController m_chaseController;
    private CrateOpeningController m_crateOpeningController;

    /// <summary>Injects scene dependencies after the machine prefab is spawned.</summary>
    public void Initialize(ChaseController p_chaseController, CrateOpeningController p_crateOpeningController)
    {
        m_chaseController = p_chaseController;
        m_crateOpeningController = p_crateOpeningController;
        if (m_debugMode) Debug.Log("[OpeningMachine] Initialized.");
    }

    /// <summary>Machine is usable only while exploring and when at least one crate is available.</summary>
    public bool CanInteract(InteractionContext p_context)
    {
        if (m_chaseController == null || m_crateOpeningController == null) return false;
        if (m_chaseController.CurrentState != ChaseState.Exploring) return false;
        if (m_chaseController.ChaseProgress.UnopenedCrateCount <= 0) return false;
        if (m_crateOpeningController.IsOpening) return false;
        return true;
    }

    /// <summary>Requests the crate opening sequence.</summary>
    public void Interact(InteractionContext p_context)
    {
        if (!CanInteract(p_context)) return;

        if (m_debugMode) Debug.Log("[OpeningMachine] Interaction accepted.");
        m_crateOpeningController.StartOpening(this);
    }
}
