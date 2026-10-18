using UnityEngine;

/// <summary>Gameplay role of a loot item.</summary>
public enum LootRole
{
    UsefulComponent,
    Gag
}

/// <summary>Visual/display rarity. Rarity is intentionally independent from usefulness.</summary>
public enum LootRarity
{
    Common,
    Rare,
    Epic,
    Legendary,
    UltraRare
}

/// <summary>
/// Data asset describing one possible crate reward.
/// This class intentionally contains no gameplay logic or Debug.Log calls.
/// </summary>
[CreateAssetMenu(fileName = "Loot_", menuName = "Chase/Loot Definition")]
public class LootDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string m_id;
    [SerializeField] private string m_displayName;

    [Header("Gameplay")]
    [SerializeField] private LootRole m_role;
    [SerializeField] private LootRarity m_rarity;

    [Header("Visual")]
    [SerializeField] private GameObject m_prefab;

    public string Id => m_id;
    public string DisplayName => m_displayName;
    public LootRole Role => m_role;
    public LootRarity Rarity => m_rarity;
    public GameObject Prefab => m_prefab;
}
