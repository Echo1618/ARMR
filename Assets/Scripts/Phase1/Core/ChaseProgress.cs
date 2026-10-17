using System;
using System.Collections.Generic;

public sealed class ChaseProgress
{
    private const int REQUIRED_COMPONENT_COUNT = 6;

    private int m_unopenedCrateCount = 0;
    private readonly HashSet<string> m_collectedComponents = new HashSet<string>();

    public event Action<int> CrateCountChanged;
    public event Action<string, int> ComponentAdded;
    public event Action Completed;


    public int UnopenedCrateCount => m_unopenedCrateCount;

    public int CollectedComponentCount => m_collectedComponents.Count;

    public IReadOnlyCollection<string> CollectedComponents => m_collectedComponents;

    public bool IsComplete => m_collectedComponents.Count >= REQUIRED_COMPONENT_COUNT;

    public void AddCrate()
    {
        m_unopenedCrateCount++;
        CrateCountChanged?.Invoke(m_unopenedCrateCount);
    }

    public bool ConsumeCrate()
    {
        if (m_unopenedCrateCount <= 0)
            return false;

        m_unopenedCrateCount--;
        CrateCountChanged?.Invoke(m_unopenedCrateCount);
        return true;
    }

    public bool TryAddComponent(string p_componentId)
    {
        if (string.IsNullOrWhiteSpace(p_componentId))
            return false;

        if (IsComplete)
            return false;

        bool added = m_collectedComponents.Add(p_componentId);

        if (!added)
            return false;

        ComponentAdded?.Invoke(
            p_componentId,
            m_collectedComponents.Count
        );

        if (IsComplete)
            Completed?.Invoke();

        return true;
    }
}