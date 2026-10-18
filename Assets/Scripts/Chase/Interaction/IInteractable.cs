/// <summary>
/// Contract implemented by world objects that can react to the central interaction raycaster.
/// Pure interface: no runtime state or debug field belongs here.
/// </summary>
public interface IInteractable
{
    /// <summary>Returns whether the object currently accepts this interaction.</summary>
    bool CanInteract(InteractionContext p_context);

    /// <summary>Executes the interaction after CanInteract has returned true.</summary>
    void Interact(InteractionContext p_context);
}
