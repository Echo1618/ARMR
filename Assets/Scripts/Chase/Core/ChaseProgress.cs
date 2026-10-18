using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores persistent gameplay progress for the Chase:
/// unopened crates and unique useful components.
/// This class is intentionally not a MonoBehaviour.
/// </summary>
public sealed class ChaseProgress
{
    private const int REQUIRED_COMPONENT_COUNT = 6;

    private int m_unopenedCrateCount = 0;
    private readonly HashSet<string> m_collectedComponents = new HashSet<string>();
    private readonly HashSet<string> m_collectedGags = new HashSet<string>();
    private readonly bool m_debugMode;

    public event Action<int> CrateCountChanged;
    public event Action<string, int> ComponentAdded;
    public event Action Completed;

    public int UnopenedCrateCount => m_unopenedCrateCount;
    public int CollectedComponentCount => m_collectedComponents.Count;
    public int RequiredComponentCount => REQUIRED_COMPONENT_COUNT;
    public IReadOnlyCollection<string> CollectedComponents => m_collectedComponents;
    public IReadOnlyCollection<string> CollectedGags => m_collectedGags;
    public bool IsComplete => m_collectedComponents.Count >= REQUIRED_COMPONENT_COUNT;

    /// <summary>
    /// The debug flag is provided by ChaseController because this class has no Inspector.
    /// </summary>
    public ChaseProgress(bool p_debugMode = false)
    {
        m_debugMode = p_debugMode;
    }

    /// <summary>Adds one unopened crate to the player's progress.</summary>
    public void AddCrate()
    {
        m_unopenedCrateCount++;
        if (m_debugMode) Debug.Log($"[ChaseProgress] Crate added. Unopened crates: {m_unopenedCrateCount}");
        CrateCountChanged?.Invoke(m_unopenedCrateCount);
    }

    /// <summary>Consumes one crate if available.</summary>
    public bool ConsumeCrate()
    {
        if (m_unopenedCrateCount <= 0) return false;

        m_unopenedCrateCount--;
        if (m_debugMode) Debug.Log($"[ChaseProgress] Crate consumed. Unopened crates: {m_unopenedCrateCount}");
        CrateCountChanged?.Invoke(m_unopenedCrateCount);
        return true;
    }

    /// <summary>
    /// Adds a useful component by unique ID.
    /// Duplicate IDs and invalid IDs are rejected.
    /// </summary>
    public bool TryAddComponent(string p_componentId)
    {
        if (string.IsNullOrWhiteSpace(p_componentId)) return false;
        if (IsComplete) return false;

        bool added = m_collectedComponents.Add(p_componentId);
        if (!added) return false;

        if (m_debugMode) Debug.Log($"[ChaseProgress] Component added: {p_componentId} ({m_collectedComponents.Count}/{REQUIRED_COMPONENT_COUNT})");
        ComponentAdded?.Invoke(p_componentId, m_collectedComponents.Count);

        if (IsComplete)
        {
            if (m_debugMode) Debug.Log("[ChaseProgress] All useful components collected.");
            Completed?.Invoke();
        }

        return true;
    }

    /// <summary>Returns true if the component ID is already collected.</summary>
    public bool HasComponent(string p_componentId)
    {
        if (string.IsNullOrWhiteSpace(p_componentId)) return false;
        return m_collectedComponents.Contains(p_componentId);
    }

    public bool TryAddGag(string p_gagId)
    {
        if (string.IsNullOrWhiteSpace(p_gagId)) return false;

        bool added = m_collectedGags.Add(p_gagId);

        if (added && m_debugMode)
            Debug.Log($"[ChaseProgress] Gag added: {p_gagId}");

        return added;
    }

    public bool HasGag(string p_gagId)
    {
        return !string.IsNullOrWhiteSpace(p_gagId) &&
               m_collectedGags.Contains(p_gagId);
    }
}
