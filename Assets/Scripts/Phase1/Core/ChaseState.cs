public enum ChaseState
{
    // corresponds to the start of the AR. Unity is still detecting the environment,
    // the planes and the usable surfaces. The player is not really in the hunt yet.
    Scanning,
    // is the main state. The player can look around, 
    // find the AR areas, move to the machine, 
    // possibly consult the hologram, etc.
    Exploring,
    // means that an area has been found and the crate extraction mini-game is in progress.
    Extracting,
    // corresponds to the second mini-game: the player is at the machine and tries to open/upgrade a crate.
    OpeningCrate,
    // means that the tablet is on the device and that the representation of Poulpy with the collected objects is displayed.
    ViewingHologram,
    // is the particular state of the end where Poulpy has the six components 
    // and assembles them into the object allowing to continue to the island.
    FinalAssembly,
    // means that the Chase is definitely finished and the next transition can be started.
    Completed
}