using UnityEngine;
using UnityEngine.InputSystem;

public class ChaseControllerTest : MonoBehaviour
{
    [SerializeField] private ChaseController m_chaseController;

    private int m_testComponentIndex = 0;

    private void OnEnable()
    {
        m_chaseController.StateChanged += OnStateChanged;
        m_chaseController.ChaseProgress.CrateCountChanged += OnCrateCountChanged;
        m_chaseController.ChaseProgress.ComponentAdded += OnComponentAdded;
        m_chaseController.ChaseProgress.Completed += OnProgressCompleted;
    }

    private void OnDisable()
    {
        m_chaseController.StateChanged -= OnStateChanged;
        m_chaseController.ChaseProgress.CrateCountChanged -= OnCrateCountChanged;
        m_chaseController.ChaseProgress.ComponentAdded -= OnComponentAdded;
        m_chaseController.ChaseProgress.Completed -= OnProgressCompleted;
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            m_chaseController.FinishScanning();

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            m_chaseController.StartExtracting();

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            m_chaseController.FinishExtracting();

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            m_chaseController.ChaseProgress.AddCrate();

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
            m_chaseController.StartCrateOpening();

        if (Keyboard.current.digit6Key.wasPressedThisFrame)
            m_chaseController.FinishCrateOpening();

        if (Keyboard.current.hKey.wasPressedThisFrame)
            m_chaseController.StartHologram();

        if (Keyboard.current.jKey.wasPressedThisFrame)
            m_chaseController.FinishHologram();

        if (Keyboard.current.cKey.wasPressedThisFrame)
            AddTestComponent();

        if (Keyboard.current.fKey.wasPressedThisFrame)
            m_chaseController.FinishFinalAssembly();
    }

    private void AddTestComponent()
    {
        m_testComponentIndex++;

        string componentId = "TestComponent_" + m_testComponentIndex;

        bool success =
            m_chaseController.ChaseProgress.TryAddComponent(componentId);

        Debug.Log(
            $"[CHASE TEST] Add component '{componentId}' : {success}"
        );
    }

    private void OnStateChanged(
        ChaseState p_oldState,
        ChaseState p_newState)
    {
        Debug.Log(
            $"[CHASE TEST] State : {p_oldState} -> {p_newState}"
        );
    }

    private void OnCrateCountChanged(int p_newCount)
    {
        Debug.Log(
            $"[CHASE TEST] Unopened crates : {p_newCount}"
        );
    }

    private void OnComponentAdded(
        string p_componentId,
        int p_componentCount)
    {
        Debug.Log(
            $"[CHASE TEST] Component : {p_componentId} | " +
            $"Total : {p_componentCount}"
        );
    }

    private void OnProgressCompleted()
    {
        Debug.Log("[CHASE TEST] Progress completed.");
    }
}