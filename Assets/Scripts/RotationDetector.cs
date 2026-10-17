using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotationDetector : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(EnalbeGyro());
    }

    IEnumerator EnalbeGyro()
    {
        yield return new WaitForSeconds(2f); // give sensors time to init

        if (UnityEngine.InputSystem.AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(UnityEngine.InputSystem.AttitudeSensor.current);
            Debug.Log("AttitudeSensor | AttitudeSensor found and enabled");
        }
        else
        {
            Debug.Log("AttitudeSensor | Still no AttitudeSensor after delay");
        }
    }
    private void Update()
    {
        if (UnityEngine.InputSystem.AttitudeSensor.current != null)
            if(UnityEngine.InputSystem.AttitudeSensor.current.enabled)
            {
                Debug.Log($"AttitudeSensor | AttitudeSensor Value : {UnityEngine.InputSystem.AttitudeSensor.current.attitude.ReadValue().eulerAngles}");

                if (UnityEngine.InputSystem.AttitudeSensor.current != null && UnityEngine.InputSystem.AttitudeSensor.current.enabled)
                {
                    Quaternion rawAttitude = UnityEngine.InputSystem.AttitudeSensor.current.attitude.ReadValue();

                    Quaternion attitude = new Quaternion(rawAttitude.x, rawAttitude.y, -rawAttitude.z, -rawAttitude.w);
                    attitude = Quaternion.Euler(90f, 0f, 0f) * attitude;

                    Vector3 screenDirection = attitude * -Vector3.forward;

                    float angleToGround = Vector3.Angle(screenDirection, Vector3.down);

                    bool isLookingAtGround = angleToGround < 20f;

                    Debug.Log($"AttitudeSensor | Angle: {angleToGround:F1}° | Look down: {isLookingAtGround}");
                }

            }


    }

}
