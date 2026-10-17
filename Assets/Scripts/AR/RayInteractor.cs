// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.InputSystem;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;

// public class RayInteractor : MonoBehaviour
// {
//     /// <summary>
//     /// Instance of the input actions
//     /// </summary>
//     private MobileInputActions m_InputActions;

//     [SerializeField] private InputActionReference m_InteractInputActionReference;

//     void Start()
//     {
//         m_InputActions = InputActionsManager.InstanceInputActions;
//         // Bind Tap function when input on Tap action is performed
//         //m_InputActions.Mobile.Tap.performed += Tap;
//         m_InteractInputActionReference.action.performed += Tap;
//     }

//     /// <summary>
//     /// Function call when Tap Action is detected
//     /// </summary>
//     /// <param name="callbackContext">Contains metadata of the action performed</param>
//     private void Tap(InputAction.CallbackContext callbackContext)
//     {
//         //Create Ray based on screen position of the tap and the camera position
//         Ray ray = Camera.main.ScreenPointToRay(m_InputActions.Mobile.Point.ReadValue<Vector2>());

//         RaycastHit hitinfo;
//         Physics.Raycast(ray, out hitinfo);

//         Debug.Log("[My Debug] RayInteractor : Interact Ray");

//         if (hitinfo.collider)
//         {
//             Debug.Log("[My Debug] RayInteractor : Collider");

//             if(hitinfo.collider.gameObject.GetComponent<IInteractable>() != null)
//             {
//                 hitinfo.collider.gameObject.GetComponent<IInteractable>().Interact();   
//                 Debug.Log("[My Debug] RayInteractor : IInteractable");
//             }
//         }
//         else
//         {
//             Debug.Log("[My Debug] RayInteractor : Null");
//         }


//         //if (hitinfo.collider.gameObject.CompareTag("Ghost"))
//         //{
//         //    Debug.Log("Interact Hit");
//         //    hitinfo.collider.gameObject.GetComponent<Animator>().enabled = true;
//         //}
//     }

// }

