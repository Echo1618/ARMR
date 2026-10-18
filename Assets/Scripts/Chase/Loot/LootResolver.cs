using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Selects a valid loot reward from the configured loot table.
/// Useful components already owned by the player are excluded from future rolls.
/// </summary>
public class LootResolver : MonoBehaviour
{
    [Header("Loot Table")]
    [SerializeField] private List<LootDefinition> m_lootTable = new List<LootDefinition>();

    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    /// <summary>
    /// Returns a valid random loot item.
    /// If p_forceUsefulComponent is true, gag items are excluded from the roll.
    /// </summary>
    public LootDefinition Resolve(ChaseProgress p_progress, bool p_forceUsefulComponent = false)
    {
        if (p_progress == null) return null;

        List<LootDefinition> candidates = new List<LootDefinition>();
        foreach (LootDefinition loot in m_lootTable)
        {
            if (loot == null) continue;

            if (p_forceUsefulComponent &&
                loot.Role != LootRole.UsefulComponent)
            {
                continue;
            }

            if (loot.Role == LootRole.UsefulComponent &&
                p_progress.HasComponent(loot.Id))
            {
                continue;
            }

            if (loot.Role == LootRole.Gag &&
                p_progress.HasGag(loot.Id))
            {
                continue;
            }

            candidates.Add(loot);
        }

        if (candidates.Count == 0)
        {
            if (m_debugMode) Debug.LogWarning("[LootResolver] No valid loot available.");
            return null;
        }

        LootDefinition selectedLoot = candidates[Random.Range(0, candidates.Count)];
        if (m_debugMode) Debug.Log($"[LootResolver] Loot: {selectedLoot.DisplayName} | {selectedLoot.Role} | {selectedLoot.Rarity}");
        return selectedLoot;
    }
}
