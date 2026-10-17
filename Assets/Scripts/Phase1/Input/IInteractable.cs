/// <summary>
/// Interface for interactable objects.
/// </summary>
public interface IInteractable
{
    bool CanInteract(InteractionContext p_context);
    void Interact(InteractionContext p_context);
};