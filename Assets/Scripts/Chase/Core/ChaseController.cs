using System;
using UnityEngine;

/// <summary>
/// Central state machine for the Chase.
/// It owns ChaseProgress and validates all major gameplay transitions.
/// </summary>
public class ChaseController : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    private ChaseState m_currentState;
    private ChaseProgress m_chaseProgress;

    public event Action<ChaseState, ChaseState> StateChanged;

    public ChaseState CurrentState => m_currentState;
    public ChaseProgress ChaseProgress => m_chaseProgress;

    private void Awake()
    {
        m_chaseProgress = new ChaseProgress(m_debugMode);
        m_currentState = ChaseState.Scanning;
    }


    /// <summary>Attempts to change the Chase state if the transition is allowed.</summary>
    private void ChangeState(ChaseState p_newState)
    {
        if (p_newState == m_currentState) return;

        if (!CanChangeState(m_currentState, p_newState))
        {
            if (m_debugMode) Debug.LogWarning($"[ChaseController] Rejected transition: {m_currentState} -> {p_newState}");
            return;
        }

        ChaseState oldState = m_currentState;
        m_currentState = p_newState;

        if (m_debugMode) Debug.Log($"[ChaseController] State: {oldState} -> {m_currentState}");
        StateChanged?.Invoke(oldState, m_currentState);
    }

    /// <summary>Defines every legal high-level transition of the Chase.</summary>
    private bool CanChangeState(ChaseState p_oldState, ChaseState p_newState)
    {
        return (p_oldState, p_newState) switch
        {
            (ChaseState.Scanning, ChaseState.Exploring) => true,
            (ChaseState.Exploring, ChaseState.Extracting) => true,
            (ChaseState.Extracting, ChaseState.Exploring) => true,
            (ChaseState.Exploring, ChaseState.OpeningCrate) => true,
            (ChaseState.OpeningCrate, ChaseState.Exploring) => true,
            (ChaseState.Exploring, ChaseState.ViewingHologram) => true,
            (ChaseState.ViewingHologram, ChaseState.Exploring) => true,
            (ChaseState.OpeningCrate, ChaseState.FinalAssembly) => true,
            (ChaseState.ViewingHologram, ChaseState.FinalAssembly) => true,
            (ChaseState.FinalAssembly, ChaseState.Completed) => true,
            _ => false
        };
    }

    public void FinishScanning() => ChangeState(ChaseState.Exploring);
    public void StartExtracting() => ChangeState(ChaseState.Extracting);
    public void FinishExtracting() => ChangeState(ChaseState.Exploring);
    public void StartCrateOpening() => ChangeState(ChaseState.OpeningCrate);
    public void FinishCrateOpening() => ChangeState(ChaseState.Exploring);
    public void StartHologram() => ChangeState(ChaseState.ViewingHologram);
    public void FinishHologram() => ChangeState(ChaseState.Exploring);

    /// <summary>
    /// Triggered when ChaseProgress reaches six components.
    /// TODO: when a loot reveal animation exists, delay this transition until the reveal is finished.
    /// </summary>
    public void StartFinalAssembly() => ChangeState(ChaseState.FinalAssembly);

    public void FinishFinalAssembly() => ChangeState(ChaseState.Completed);
}
