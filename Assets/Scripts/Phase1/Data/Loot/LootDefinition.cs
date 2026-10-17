using UnityEngine;

public enum LootRole
{
    Real,
    Gag
}
public enum LootRarity
{
    Common,
    Rare, 
    Legendary
}

public class LootDefinition : ScriptableObject
{
    public string id;
    public string displayName;

    LootRole lootRole;
    LootRarity lootRarity;

    GameObject prefab;
    
}
