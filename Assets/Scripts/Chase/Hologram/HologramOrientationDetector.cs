using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HologramOrientationDetector : MonoBehaviour
{
    [Header("Simulation")]
    [SerializeField] private bool m_useSimulation = true;
    [SerializeField] private bool m_simulatedFaceDown = false;

    [Header("Real Device Detection")]
    [Tooltip("Valeur Z à partir de laquelle la tablette est considérée face vers le sol.")]
    [Range(-1f, 1f)]
    [SerializeField] private float m_faceDownThreshold = 0.7f;

    [Tooltip("Inverse la détection si l'axe Z du capteur est opposé sur l'appareil.")]
    [SerializeField] private bool m_invertZ = false;

    [Header("Stability")]
    [SerializeField] private float m_validationDelay = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    private bool m_isFaceDown = false;
    private bool m_pendingState = false;
    private float m_pendingSince = 0f;

    public bool IsFaceDown => m_isFaceDown;
    public bool UseSimulation => m_useSimulation;

    public event Action<bool> FaceDownChanged;

    private void OnEnable()
    {
        if (!m_useSimulation && GravitySensor.current != null)
            InputSystem.EnableDevice(GravitySensor.current);
    }

    private void OnDisable()
    {
        if (GravitySensor.current != null)
            InputSystem.DisableDevice(GravitySensor.current);
    }

    private void Update()
    {
        bool detectedFaceDown = m_useSimulation
            ? m_simulatedFaceDown
            : ReadRealOrientation();

        UpdateStableState(detectedFaceDown);
    }

    private bool ReadRealOrientation()
    {
        if (GravitySensor.current == null)
        {
            if (m_debugMode)
                Debug.LogWarning("[HologramOrientationDetector] No GravitySensor available.");

            return false;
        }

        Vector3 gravity = GravitySensor.current.gravity.ReadValue();

        float z = m_invertZ ? -gravity.z : gravity.z;

        if (m_debugMode)
            Debug.Log($"[HologramOrientationDetector] Gravity: {gravity} / Z used: {z:F2}");

        return z >= m_faceDownThreshold;
    }

    private void UpdateStableState(bool p_detectedFaceDown)
    {
        if (p_detectedFaceDown == m_isFaceDown)
        {
            m_pendingState = p_detectedFaceDown;
            m_pendingSince = Time.unscaledTime;
            return;
        }

        if (p_detectedFaceDown != m_pendingState)
        {
            m_pendingState = p_detectedFaceDown;
            m_pendingSince = Time.unscaledTime;
            return;
        }

        if (Time.unscaledTime - m_pendingSince < m_validationDelay) return;

        m_isFaceDown = p_detectedFaceDown;

        if (m_debugMode)
            Debug.Log($"[HologramOrientationDetector] Face down: {m_isFaceDown}");

        FaceDownChanged?.Invoke(m_isFaceDown);
    }
}