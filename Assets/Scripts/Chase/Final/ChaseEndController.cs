using System.Collections;
using UnityEngine;

public class ChaseEndController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChaseController m_chaseController;
    [SerializeField] private CrateOpeningController m_crateOpeningController;
    [SerializeField] private HologramController m_hologramController;
    [SerializeField] private PoulpyDialogueController m_dialogueController;

    [Header("Final Assembly")]
    [SerializeField] private GameObject m_fusionRoot;
    [SerializeField] private float m_componentPreviewDuration = 2f;

    [Header("Fusion Placeholder")]
    [SerializeField] private bool m_autoCompleteFusion = true;
    [SerializeField] private float m_fusionDuration = 2f;

    [Header("Dialogues")]
    [TextArea(2, 5)]
    [SerializeField]
    private string m_allComponentsMessage =
        "Tu les as tous trouvés ! Bien joué ! Tu peux encore continuer à chercher si tu veux, mais pour terminer la chasse, retourne au dock et passe en mode hologramme.";

    [TextArea(2, 5)]
    [SerializeField]
    private string m_vrTransitionMessage =
        "C'est prêt ! On a maintenant tout ce qu'il nous faut pour continuer. Enfile le casque VR et prépare-toi pour la prochaine étape !";

    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    private bool m_finalGoalReached = false;
    private bool m_completionDialogueShown = false;
    private bool m_finalSequenceStarted = false;
    private bool m_fusionCompleted = false;

    private Coroutine m_finalSequenceCoroutine;

    private void Awake()
    {
        Debug.Assert(m_chaseController != null);
        Debug.Assert(m_crateOpeningController != null);
        Debug.Assert(m_hologramController != null);
        Debug.Assert(m_dialogueController != null);
        Debug.Assert(m_fusionRoot != null);

        m_fusionRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (m_crateOpeningController != null)
            m_crateOpeningController.LootResolved += OnLootResolved;

        if (m_hologramController != null)
            m_hologramController.HologramOpened += OnHologramOpened;
    }

    private void OnDisable()
    {
        if (m_crateOpeningController != null)
            m_crateOpeningController.LootResolved -= OnLootResolved;

        if (m_hologramController != null)
            m_hologramController.HologramOpened -= OnHologramOpened;

        if (m_finalSequenceCoroutine != null)
        {
            StopCoroutine(m_finalSequenceCoroutine);
            m_finalSequenceCoroutine = null;
        }
    }

    private void Update()
    {
        if (!m_finalGoalReached) return;
        if (m_completionDialogueShown) return;
        if (m_dialogueController.IsOpen) return;

        m_completionDialogueShown = true;
        m_dialogueController.ShowMessage(m_allComponentsMessage);

        if (m_debugMode)
            Debug.Log("[ChaseEndController] Final goal dialogue shown.");
    }

    private void OnLootResolved(LootDefinition p_loot)
    {
        if (m_finalGoalReached) return;
        if (!m_chaseController.ChaseProgress.IsComplete) return;

        m_finalGoalReached = true;

        if (m_debugMode)
            Debug.Log("[ChaseEndController] All useful components collected.");
    }

    private void OnHologramOpened()
    {
        if (!m_finalGoalReached) return;
        if (m_finalSequenceStarted) return;

        if (!m_chaseController.ChaseProgress.IsComplete)
        {
            Debug.LogError("[ChaseEndController] Final hologram started without complete progress.");
            return;
        }

        m_finalSequenceStarted = true;

        m_hologramController.RefreshHologram();
        m_hologramController.SetExitLocked(true);

        m_finalSequenceCoroutine = StartCoroutine(FinalSequence());

        if (m_debugMode)
            Debug.Log($"[ChaseEndController] Final sequence started with {m_chaseController.ChaseProgress.CollectedComponentCount}/{m_chaseController.ChaseProgress.RequiredComponentCount} components.");
    }

    private IEnumerator FinalSequence()
    {
        // Le joueur voit Poulpy avec les 6 composants avant la fusion.
        yield return new WaitForSecondsRealtime(m_componentPreviewDuration);

        m_chaseController.StartFinalAssembly();

        if (m_chaseController.CurrentState != ChaseState.FinalAssembly)
        {
            Debug.LogError("[ChaseEndController] Could not enter FinalAssembly.");
            yield break;
        }

        m_hologramController.RefreshHologram();
        m_fusionRoot.SetActive(true);

        if (m_debugMode)
            Debug.Log("[ChaseEndController] Fusion started.");

        if (!m_autoCompleteFusion) yield break;

        yield return new WaitForSecondsRealtime(m_fusionDuration);

        if (!m_fusionCompleted)
            CompleteFusion();
    }

    public void CompleteFusion()
    {
        if (!m_finalSequenceStarted) return;
        if (m_fusionCompleted) return;

        m_fusionCompleted = true;

        if (m_debugMode)
            Debug.Log("[ChaseEndController] Fusion completed.");

        StartCoroutine(ShowFinalDialogueWhenAvailable());
    }

    private IEnumerator ShowFinalDialogueWhenAvailable()
    {
        while (m_dialogueController.IsOpen)
            yield return null;

        m_dialogueController.ShowMessage(m_vrTransitionMessage, CompleteChase);
    }

    private void CompleteChase()
    {
        m_fusionRoot.SetActive(false);
        m_hologramController.CloseAfterFinalSequence();

        if (m_chaseController.CurrentState == ChaseState.FinalAssembly)
            m_chaseController.FinishFinalAssembly();

        if (m_debugMode)
            Debug.Log("[ChaseEndController] Chase completed.");
    }
}