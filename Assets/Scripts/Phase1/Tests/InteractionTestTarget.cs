using UnityEngine;

public class InteractionTestTarget : MonoBehaviour, IInteractable
{
    public bool CanInteract(InteractionContext p_context){return true;}
    public void Interact(InteractionContext p_context){Debug.Log("Hello");}
}
