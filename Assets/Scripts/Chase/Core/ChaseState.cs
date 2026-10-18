/// <summary>
/// High-level states of the Chase gameplay loop.
/// Keep this enum limited to major gameplay phases, not visual sub-steps.
/// </summary>
public enum ChaseState
{
    /// <summary>AR is still detecting enough usable environment.</summary>
    Scanning,

    /// <summary>Main free exploration state.</summary>
    Exploring,

    /// <summary>A collection zone extraction mini-game is running.</summary>
    Extracting,

    /// <summary>A crate opening sequence is running.</summary>
    OpeningCrate,

    /// <summary>The player is consulting the Poulpy hologram.</summary>
    ViewingHologram,

    /// <summary>Poulpy is assembling the six useful components.</summary>
    FinalAssembly,

    /// <summary>The Chase is fully completed.</summary>
    Completed
}
