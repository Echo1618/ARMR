using System;
using UnityEngine;

[Serializable]
public class HologramComponentSlot
{
    [SerializeField] private string m_componentId;
    [SerializeField] private GameObject m_visual;

    public string ComponentId => m_componentId;
    public GameObject Visual => m_visual;
}

public class HologramController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChaseController m_chaseController;
    [SerializeField] private HologramOrientationDetector m_orientationDetector;
    [SerializeField] private PoulpyDialogueController m_dialogueController;
    [SerializeField] private GameObject m_hologramRoot;

    [Header("Components")]
    [SerializeField] private HologramComponentSlot[] m_componentSlots;

    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    private bool m_isViewing = false;
    private bool m_exitLocked = false;

    public bool IsViewing => m_isViewing;
    public bool ExitLocked => m_exitLocked;

    public event Action HologramOpened;
    public event Action HologramCompleted;

    private void Awake()
    {
        Debug.Assert(m_chaseController != null);
        Debug.Assert(m_orientationDetector != null);
        Debug.Assert(m_dialogueController != null);
        Debug.Assert(m_hologramRoot != null);

        m_hologramRoot.SetActive(false);
    }

    private void OnEnable()
    {
        m_orientationDetector.FaceDownChanged += OnFaceDownChanged;
    }

    private void OnDisable()
    {
        m_orientationDetector.FaceDownChanged -= OnFaceDownChanged;
    }

    private void Update()
    {
        // Si le joueur avait déjà retourné la tablette pendant un dialogue,
        // l'hologramme démarre dès que Poulpy a fini de parler.
        if (!m_isViewing &&
            m_orientationDetector.IsFaceDown &&
            !m_dialogueController.IsOpen &&
            m_chaseController.CurrentState == ChaseState.Exploring)
        {
            TryOpenHologram();
        }
    }

    private void OnFaceDownChanged(bool p_faceDown)
    {
        if (p_faceDown)
        {
            TryOpenHologram();
            return;
        }

        if (!m_exitLocked)
            TryCloseHologram();
    }

    public bool TryOpenHologram()
    {
        if (m_isViewing) return false;
        if (m_dialogueController.IsOpen) return false;

        if (m_chaseController.CurrentState != ChaseState.Exploring)
        {
            if (m_debugMode)
                Debug.Log($"[HologramController] Cannot open from {m_chaseController.CurrentState}.");

            return false;
        }

        m_chaseController.StartHologram();

        if (m_chaseController.CurrentState != ChaseState.ViewingHologram)
            return false;

        m_isViewing = true;

        RefreshHologram();
        m_hologramRoot.SetActive(true);

        if (m_debugMode)
            Debug.Log("[HologramController] Hologram opened.");

        HologramOpened?.Invoke();

        return true;
    }

    public bool TryCloseHologram()
    {
        if (!m_isViewing) return false;
        if (m_exitLocked) return false;

        m_hologramRoot.SetActive(false);
        m_isViewing = false;

        if (m_chaseController.CurrentState == ChaseState.ViewingHologram)
            m_chaseController.FinishHologram();

        if (m_debugMode)
            Debug.Log("[HologramController] Hologram closed.");

        HologramCompleted?.Invoke();

        return true;
    }

    public void RefreshHologram()
    {
        ChaseProgress progress = m_chaseController.ChaseProgress;

        foreach (HologramComponentSlot slot in m_componentSlots)
        {
            if (slot == null || slot.Visual == null) continue;

            slot.Visual.SetActive(
                progress.HasComponent(slot.ComponentId)
            );
        }

        if (m_debugMode)
            Debug.Log($"[HologramController] Components displayed: {progress.CollectedComponentCount}.");
    }

    public void SetExitLocked(bool p_locked)
    {
        m_exitLocked = p_locked;

        if (m_debugMode)
            Debug.Log($"[HologramController] Exit locked: {m_exitLocked}");
    }

    public void SetComponentVisualsVisible(bool p_visible)
    {
        if (p_visible)
        {
            RefreshHologram();
            return;
        }

        foreach (HologramComponentSlot slot in m_componentSlots)
        {
            if (slot == null || slot.Visual == null) continue;
            slot.Visual.SetActive(false);
        }
    }

    public void CloseAfterFinalSequence()
    {
        m_exitLocked = false;
        m_isViewing = false;
        m_hologramRoot.SetActive(false);

        if (m_debugMode)
            Debug.Log("[HologramController] Final hologram closed.");
    }
}