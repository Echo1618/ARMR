using System;
using UnityEngine;
using UnityEngine.UI;

public class CrateOpeningController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChaseController m_chaseController;
    [SerializeField] private LootResolver m_lootResolver;

    [Header("UI")]
    [SerializeField] private GameObject m_openingPanel;
    [SerializeField] private Button m_openButton;
    [SerializeField] private Button m_cancelButton;

    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    private OpeningMachine m_currentMachine;
    private int m_finalComponentAttempts = 0;

    public bool IsOpening => m_currentMachine != null;
    public LootDefinition LastLoot { get; private set; }

    public event Action<LootDefinition> LootResolved;

    private void Awake()
    {
        Debug.Assert(m_chaseController != null);
        Debug.Assert(m_lootResolver != null);
        Debug.Assert(m_openingPanel != null);
        Debug.Assert(m_openButton != null);
        Debug.Assert(m_cancelButton != null);

        m_openingPanel.SetActive(false);
    }

    private void OnEnable()
    {
        m_openButton.onClick.AddListener(CompleteOpening);
        m_cancelButton.onClick.AddListener(CancelOpening);
    }

    private void OnDisable()
    {
        m_openButton.onClick.RemoveListener(CompleteOpening);
        m_cancelButton.onClick.RemoveListener(CancelOpening);
    }

    public void StartOpening(OpeningMachine p_machine)
    {
        if (p_machine == null) return;
        if (m_currentMachine != null) return;
        if (m_chaseController.CurrentState != ChaseState.Exploring) return;
        if (m_chaseController.ChaseProgress.UnopenedCrateCount <= 0) return;

        m_currentMachine = p_machine;

        // Important : le panel existe AVANT le changement d'état.
        // Le tutoriel pourra donc le masquer immédiatement.
        m_openingPanel.SetActive(true);

        m_chaseController.StartCrateOpening();

        if (m_chaseController.CurrentState != ChaseState.OpeningCrate)
        {
            m_currentMachine = null;
            m_openingPanel.SetActive(false);
            return;
        }

        if (m_debugMode)
            Debug.Log("[CrateOpeningController] Opening started.");
    }

    public void CompleteOpening()
    {
        if (m_currentMachine == null)
        {
            if (m_debugMode)
                Debug.LogWarning("[CrateOpeningController] Open ignored: no current machine.");
            return;
        }

        if (m_chaseController.CurrentState != ChaseState.OpeningCrate)
        {
            if (m_debugMode)
                Debug.LogWarning($"[CrateOpeningController] Open ignored from state {m_chaseController.CurrentState}.");
            return;
        }

        ChaseProgress progress = m_chaseController.ChaseProgress;

        bool isFirstComponent = progress.CollectedComponentCount == 0;
        bool isMissingFinalComponent =
            progress.CollectedComponentCount == progress.RequiredComponentCount - 1;

        if (isMissingFinalComponent)
            m_finalComponentAttempts++;

        bool forceFinalComponent =
            isMissingFinalComponent &&
            m_finalComponentAttempts >= 3;

        bool forceUsefulComponent =
            isFirstComponent ||
            forceFinalComponent;

        LootDefinition loot = m_lootResolver.Resolve(
            progress,
            forceUsefulComponent
        );

        if (loot == null)
        {
            Debug.LogError("[CrateOpeningController] No loot available.");
            return;
        }

        if (!progress.ConsumeCrate())
        {
            Debug.LogError("[CrateOpeningController] Could not consume crate.");
            return;
        }

        LastLoot = loot;

        if (loot.Role == LootRole.UsefulComponent)
        {
            progress.TryAddComponent(loot.Id);

            if (isMissingFinalComponent)
                m_finalComponentAttempts = 0;
        }
        else if (loot.Role == LootRole.Gag)
        {
            progress.TryAddGag(loot.Id);
        }

        OpeningMachine completedMachine = m_currentMachine;
        m_currentMachine = null;

        m_openingPanel.SetActive(false);

        // On revient d'abord proprement en Exploring.
        if (m_chaseController.CurrentState == ChaseState.OpeningCrate)
            m_chaseController.FinishCrateOpening();

        if (m_debugMode)
        {
            Debug.Log($"[CrateOpeningController] Loot: {loot.DisplayName}");

            if (forceFinalComponent)
                Debug.Log("[CrateOpeningController] Final component pity protection triggered.");
        }

        // L'événement n'est envoyé qu'une fois l'état revenu à Exploring.
        LootResolved?.Invoke(loot);
    }

    public void CancelOpening()
    {
        if (m_currentMachine == null) return;

        m_currentMachine = null;
        m_openingPanel.SetActive(false);

        if (m_chaseController.CurrentState == ChaseState.OpeningCrate)
            m_chaseController.FinishCrateOpening();

        if (m_debugMode)
            Debug.Log("[CrateOpeningController] Opening cancelled.");
    }

    public void SetOpeningPanelVisible(bool p_visible)
    {
        if (m_openingPanel == null) return;

        // On ne réaffiche le panel que si une ouverture est réellement active.
        if (p_visible)
        {
            if (m_currentMachine != null &&
                m_chaseController.CurrentState == ChaseState.OpeningCrate)
            {
                m_openingPanel.SetActive(true);
            }

            return;
        }

        m_openingPanel.SetActive(false);
    }
}