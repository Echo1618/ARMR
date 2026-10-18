using UnityEngine;

public class ChaseTutorialController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChaseController m_chaseController;
    [SerializeField] private ChaseDirector m_chaseDirector;
    [SerializeField] private CrateOpeningController m_crateOpeningController;
    [SerializeField] private HologramController m_hologramController;
    [SerializeField] private PoulpyDialogueController m_dialogueController;

    [Header("Introduction")]
    [TextArea(2, 5)]
    [SerializeField]
    private string m_introductionMessage =
        "Salut ! Moi, c'est Poulpy. Pour continuer notre route, il va falloir bricoler quelque chose. J'ai besoin de 6 objets. À toi de les retrouver !";

    [Header("First Zone")]
    [TextArea(2, 5)]
    [SerializeField]
    private string m_firstZoneMessage =
        "Les objets sont cachés dans des caisses. Cherche les zones étranges autour de toi : c'est là qu'on peut en récupérer !";

    [Header("Extraction")]
    [TextArea(2, 5)]
    [SerializeField]
    private string m_extractionMessage =
        "Une caisse est coincée ici. Il va falloir la récupérer !";

    [Header("Opening Machine")]
    [TextArea(2, 5)]
    [SerializeField]
    private string m_openingMachineMessage =
        "Bien joué ! Mais cette caisse est verrouillée... Heureusement, je t'ai envoyé une machine pour l'ouvrir. Cherche-la autour de toi !";

    [Header("Crate Opening")]
    [TextArea(2, 5)]
    [SerializeField]
    private string m_crateOpeningMessage =
        "Place ta caisse dans la machine pour découvrir ce qu'elle contient.";

    [TextArea(2, 5)]
    [SerializeField]
    private string m_crateUpgradeMessage =
        "Toutes les caisses ne se valent pas ! En les améliorant, tu pourras augmenter tes chances d'obtenir quelque chose d'intéressant.";

    [Header("First Hologram")]
    [TextArea(2, 5)]
    [SerializeField]
    private string m_firstHologramMessage =
        "Parfait, on a notre premier objet ! Pour voir ce qu'on a récupéré, retourne la tablette face vers le sol et passe en mode hologramme.";

    [Header("Tutorial Ending")]
    [TextArea(2, 5)]
    [SerializeField]
    private string m_tutorialEndingMessage =
        "Tu sais maintenant tout ce qu'il faut ! À toi de trouver les 6 objets. Je te laisse te débrouiller... Reviens me voir quand tu les auras tous !";

    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    private ChaseTutorialStep m_currentStep = ChaseTutorialStep.Introduction;

    private bool m_waitingForFirstZone = false;
    private bool m_extractionExplained = false;
    private bool m_waitingForOpeningMachine = false;
    private bool m_openingMachineExplained = false;
    private bool m_crateOpeningExplained = false;
    private bool m_firstHologramDialogueRequested = false;
    private bool m_waitingForFirstHologram = false;
    private bool m_tutorialEndingStarted = false;

    public ChaseTutorialStep CurrentStep => m_currentStep;

    private void Awake()
    {
        Debug.Assert(m_chaseController != null);
        Debug.Assert(m_chaseDirector != null);
        Debug.Assert(m_crateOpeningController != null);
        Debug.Assert(m_hologramController != null);
        Debug.Assert(m_dialogueController != null);
    }

    private void OnEnable()
    {
        m_chaseController.StateChanged += OnStateChanged;
        m_crateOpeningController.LootResolved += OnLootResolved;
        m_hologramController.HologramCompleted += OnHologramCompleted;
    }

    private void OnDisable()
    {
        m_chaseController.StateChanged -= OnStateChanged;
        m_crateOpeningController.LootResolved -= OnLootResolved;
        m_hologramController.HologramCompleted -= OnHologramCompleted;
    }

    private void Start()
    {
        if (m_chaseDirector.TutorialCompleted)
        {
            SetStep(ChaseTutorialStep.Completed);
            return;
        }

        ShowIntroduction();
    }

    private void Update()
    {
        if (m_chaseDirector.TutorialCompleted) return;

        if (m_waitingForFirstZone &&
            m_chaseDirector.ActiveZoneCount > 0)
        {
            m_waitingForFirstZone = false;
            ShowFirstZoneDialogue();
        }

        if (m_waitingForOpeningMachine &&
            m_chaseDirector.HasOpeningMachine)
        {
            m_waitingForOpeningMachine = false;
            ShowOpeningMachineDialogue();
        }
        TryShowFirstHologramDialogue();
    }

    private void ShowIntroduction()
    {
        SetStep(ChaseTutorialStep.Introduction);

        m_dialogueController.ShowMessage(
            m_introductionMessage,
            () => m_waitingForFirstZone = true
        );
    }

    private void ShowFirstZoneDialogue()
    {
        SetStep(ChaseTutorialStep.FirstZone);
        m_dialogueController.ShowMessage(m_firstZoneMessage);
    }

    private void OnStateChanged(
        ChaseState p_oldState,
        ChaseState p_newState)
    {
        if (m_chaseDirector.TutorialCompleted) return;

        if (p_newState == ChaseState.Extracting &&
            !m_extractionExplained)
        {
            m_extractionExplained = true;
            ShowExtractionDialogue();
        }

        if (p_oldState == ChaseState.Extracting &&
            p_newState == ChaseState.Exploring &&
            m_chaseDirector.OpeningMachineUnlocked &&
            !m_openingMachineExplained)
        {
            m_waitingForOpeningMachine = true;
        }

        if (p_newState == ChaseState.OpeningCrate &&
            !m_crateOpeningExplained)
        {
            m_crateOpeningExplained = true;
            ShowCrateOpeningDialogue();
        }
    }

    private void ShowExtractionDialogue()
    {
        SetStep(ChaseTutorialStep.FirstExtraction);
        m_dialogueController.ShowMessage(m_extractionMessage);
    }

    private void ShowOpeningMachineDialogue()
    {
        m_openingMachineExplained = true;
        SetStep(ChaseTutorialStep.OpeningMachine);

        m_dialogueController.ShowMessage(
            m_openingMachineMessage
        );
    }

    private void ShowCrateOpeningDialogue()
    {
        SetStep(ChaseTutorialStep.FirstCrateOpening);

        // Pendant l'explication, aucun bouton Open n'est accessible.
        m_crateOpeningController.SetOpeningPanelVisible(false);

        m_dialogueController.ShowSequence(
            new[]
            {
                m_crateOpeningMessage,
                m_crateUpgradeMessage
            },
            () =>
            {
                m_crateOpeningController.SetOpeningPanelVisible(true);

                if (m_debugMode)
                    Debug.Log("[ChaseTutorialController] Opening panel unlocked.");
            }
        );
    }

    private void OnLootResolved(LootDefinition p_loot)
    {
        if (m_chaseDirector.TutorialCompleted) return;

        if (m_debugMode)
            Debug.Log($"[ChaseTutorialController] Loot resolved: {p_loot.DisplayName}");

        TryShowFirstHologramDialogue();
    }

    private void OnHologramCompleted()
    {
        if (!m_waitingForFirstHologram) return;
        if (m_tutorialEndingStarted) return;

        m_waitingForFirstHologram = false;
        m_tutorialEndingStarted = true;

        ShowTutorialEnding();
    }

    private void ShowTutorialEnding()
    {
        SetStep(ChaseTutorialStep.TutorialEnding);

        m_dialogueController.ShowMessage(
            m_tutorialEndingMessage,
            () =>
            {
                m_chaseDirector.CompleteTutorial();
                SetStep(ChaseTutorialStep.Completed);

                if (m_debugMode)
                    Debug.Log("[ChaseTutorialController] Tutorial completed.");
            }
        );
    }

    private void SetStep(ChaseTutorialStep p_step)
    {
        m_currentStep = p_step;

        if (m_debugMode)
            Debug.Log($"[ChaseTutorialController] Step: {m_currentStep}");
    }

    private void TryShowFirstHologramDialogue()
    {
        if (m_chaseDirector.TutorialCompleted) return;
        if (m_firstHologramDialogueRequested) return;
        if (m_chaseController.ChaseProgress.CollectedComponentCount <= 0) return;
        if (m_chaseController.CurrentState != ChaseState.Exploring) return;
        if (m_dialogueController.IsOpen) return;

        m_firstHologramDialogueRequested = true;
        SetStep(ChaseTutorialStep.FirstHologram);

        m_dialogueController.ShowMessage(
            m_firstHologramMessage,
            () =>
            {
                m_waitingForFirstHologram = true;

                if (m_debugMode)
                    Debug.Log("[ChaseTutorialController] Waiting for first hologram.");
            }
        );

        if (m_debugMode)
            Debug.Log("[ChaseTutorialController] First hologram dialogue started.");
    }
}