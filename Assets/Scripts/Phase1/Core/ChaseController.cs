using System;
using UnityEngine;

public class ChaseController : MonoBehaviour
{
    private ChaseState m_currentState;
    private ChaseProgress m_chaseProgress;

    public event Action<ChaseState, ChaseState> StateChanged;

    public ChaseState CurrentState => m_currentState;
    public ChaseProgress ChaseProgress => m_chaseProgress;

    private void Awake()
    {
        m_chaseProgress = new ChaseProgress();

        m_currentState = ChaseState.Scanning;
    }

    private void OnEnable() { m_chaseProgress.Completed += StartFinalAssembly; }
    private void OnDisable() { m_chaseProgress.Completed -= StartFinalAssembly; }

    private void ChangeState(ChaseState p_newState)
    {
        if (p_newState == m_currentState) return;
        if (!CanChangeState(m_currentState, p_newState)) return;

        ChaseState oldState = m_currentState;
        m_currentState = p_newState;

        StateChanged?.Invoke(oldState, m_currentState);
    }

    private bool CanChangeState(
    ChaseState p_oldState,
    ChaseState p_newState)
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
            (ChaseState.Exploring, ChaseState.FinalAssembly) => true,

            (ChaseState.FinalAssembly, ChaseState.Completed) => true,

            _ => false
        };
    }

    public void FinishScanning()
    {
        ChangeState(ChaseState.Exploring);
    }
    public void StartExtracting()
    {
        ChangeState(ChaseState.Extracting);
    }
    public void FinishExtracting()
    {
        ChangeState(ChaseState.Exploring);
    }
    public void StartCrateOpening()
    {
        ChangeState(ChaseState.OpeningCrate);
    }
    public void FinishCrateOpening()
    {
        ChangeState(ChaseState.Exploring);
    }
    public void StartHologram()
    {
        ChangeState(ChaseState.ViewingHologram);
    }
    public void FinishHologram()
    {
        ChangeState(ChaseState.Exploring);
    }
    private void StartFinalAssembly()
    {
        ChangeState(ChaseState.FinalAssembly);
    }
    public void FinishFinalAssembly()
    {
        ChangeState(ChaseState.Completed);
    }
}
