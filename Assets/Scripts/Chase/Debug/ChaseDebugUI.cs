using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Development-only HUD showing the current Chase state and progression.
/// This UI must not be used as the final player-facing progression display.
/// </summary>
public class ChaseDebugUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChaseController m_chaseController;
    [SerializeField] private ChaseDirector m_chaseDirector;
    [SerializeField] private ExtractionController m_extractionController;
    [SerializeField] private CrateOpeningController m_crateOpeningController;
    [SerializeField] private TMP_Text m_debugText;

    [Header("Settings")]
    [SerializeField] private float m_refreshInterval = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    private float m_nextRefreshTime = 0f;
    private readonly StringBuilder m_builder = new StringBuilder();

    private void Awake()
    {
        Debug.Assert(m_chaseController != null);
        Debug.Assert(m_chaseDirector != null);
        Debug.Assert(m_extractionController != null);
        Debug.Assert(m_crateOpeningController != null);
        Debug.Assert(m_debugText != null);
    }

    private void OnEnable()
    {
        if (m_debugMode) Debug.Log("[ChaseDebugUI] Debug HUD enabled.");
        RefreshDebugUI();
    }

    private void Update()
    {
        if (Time.unscaledTime < m_nextRefreshTime) return;

        m_nextRefreshTime = Time.unscaledTime + m_refreshInterval;
        RefreshDebugUI();
    }

    /// <summary>Rebuilds the development HUD from current runtime state.</summary>
    private void RefreshDebugUI()
    {
        if (m_chaseController == null || m_debugText == null) return;

        ChaseProgress progress = m_chaseController.ChaseProgress;
        LootDefinition lastLoot = m_crateOpeningController != null ? m_crateOpeningController.LastLoot : null;

        m_builder.Clear();
        m_builder.AppendLine("=== CHASE DEBUG ===");
        m_builder.AppendLine($"State : {m_chaseController.CurrentState}");
        m_builder.AppendLine();
        m_builder.AppendLine($"Crates : {progress.UnopenedCrateCount}");
        m_builder.AppendLine($"Components : {progress.CollectedComponentCount} / {progress.RequiredComponentCount}");
        m_builder.AppendLine($"Complete : {progress.IsComplete}");
        m_builder.AppendLine();
        m_builder.AppendLine($"Active zones : {m_chaseDirector.ActiveZoneCount}");
        m_builder.AppendLine($"Explored area : {m_chaseDirector.MaxExploredArea:F2} m²");
        m_builder.AppendLine($"Tutorial complete : {m_chaseDirector.TutorialCompleted}");
        m_builder.AppendLine();
        m_builder.AppendLine($"Extracting : {m_extractionController.IsExtracting}");
        m_builder.AppendLine($"Opening crate : {m_crateOpeningController.IsOpening}");

        if (lastLoot != null)
        {
            m_builder.AppendLine();
            m_builder.AppendLine($"Last loot : {lastLoot.DisplayName}");
            m_builder.AppendLine($"Role : {lastLoot.Role}");
            m_builder.AppendLine($"Rarity : {lastLoot.Rarity}");
        }

        if (progress.CollectedComponentCount > 0)
        {
            m_builder.AppendLine();
            m_builder.AppendLine("Collected components :");
            foreach (string componentId in progress.CollectedComponents) m_builder.AppendLine($"- {componentId}");
        }

        m_debugText.text = m_builder.ToString();
    }
}
