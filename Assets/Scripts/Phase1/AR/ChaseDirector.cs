using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ChaseDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChaseController m_chaseController;
    [SerializeField] private AreaSpawnManager m_areaSpawnManager;
    [SerializeField] private ExtractionController m_extractionController;
    [SerializeField] private ARPlaneManager m_planeManager;
    [SerializeField] private GameObject m_collectionZonePrefab;

    [Header("Exploration")]
    [SerializeField] private float m_minAreaForTutorial = 0.5f;
    [SerializeField] private float m_areaForTwoZones = 1.5f;
    [SerializeField] private float m_areaForThreeZones = 3f;
    [SerializeField] private float m_areaCheckInterval = 0.5f;

    [Header("Zones")]
    [Min(1)]
    [SerializeField] private int m_maxActiveZones = 3;

    private bool m_tutorialCompleted = false;
    private bool m_tutorialZoneSpawned = false;
    private float m_maxExploredArea = 0f;
    private float m_nextAreaCheckTime = 0f;

    private readonly List<CollectionZone> m_activeZones = new List<CollectionZone>();

    public bool TutorialCompleted => m_tutorialCompleted;
    public float MaxExploredArea => m_maxExploredArea;
    public int ActiveZoneCount => m_activeZones.Count;

    private void Awake()
    {
        Debug.Assert(m_chaseController != null);
        Debug.Assert(m_areaSpawnManager != null);
        Debug.Assert(m_extractionController != null);
        Debug.Assert(m_planeManager != null);
        Debug.Assert(m_collectionZonePrefab != null);
    }

    private void OnEnable()
    {
        if (m_chaseController != null) m_chaseController.StateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        if (m_chaseController != null) m_chaseController.StateChanged -= OnStateChanged;
    }

    private void Update()
    {
        if (Time.time < m_nextAreaCheckTime) return;

        m_nextAreaCheckTime = Time.time + m_areaCheckInterval;
        UpdateExploredArea();

        if (m_chaseController.CurrentState == ChaseState.Scanning)
        {
            if (m_maxExploredArea >= m_minAreaForTutorial) m_chaseController.FinishScanning();
            return;
        }

        if (m_chaseController.CurrentState == ChaseState.Exploring) RefreshZones();
    }

    //-----------------------------------------------------------------------------
    //                                   Public
    //-----------------------------------------------------------------------------

    public void CompleteTutorial()
    {
        if (m_tutorialCompleted) return;

        m_tutorialCompleted = true;
        RefreshZones();
    }

    //-----------------------------------------------------------------------------
    //                                   Private
    //-----------------------------------------------------------------------------

    private void RefreshZones()
    {
        if (m_chaseController.CurrentState != ChaseState.Exploring) return;

        CleanActiveZones();

        int desiredZoneCount = GetDesiredZoneCount();

        while (m_activeZones.Count < desiredZoneCount)
        {
            if (!TrySpawnZone()) break;
        }
    }

    private bool TrySpawnZone()
    {
        if (m_collectionZonePrefab == null) return false;

        if (!m_areaSpawnManager.TrySpawn(m_collectionZonePrefab, out GameObject spawnedObject)) return false;

        CollectionZone zone = spawnedObject.GetComponent<CollectionZone>();

        if (zone == null)
        {
            Debug.LogError("[ChaseDirector] Spawned CollectionZone prefab has no CollectionZone component.");
            Destroy(spawnedObject);
            return false;
        }

        zone.Initialize(m_chaseController, m_extractionController);
        zone.Consumed += OnZoneConsumed;

        m_activeZones.Add(zone);

        if (!m_tutorialCompleted && !m_tutorialZoneSpawned) m_tutorialZoneSpawned = true;

        return true;
    }

    private void OnZoneConsumed(CollectionZone p_zone)
    {
        if (p_zone == null) return;

        p_zone.Consumed -= OnZoneConsumed;
        m_activeZones.Remove(p_zone);
    }

    private int GetDesiredZoneCount()
    {
        if (!m_tutorialCompleted)
        {
            if (m_tutorialZoneSpawned) return 0;
            return 1;
        }

        if (m_maxExploredArea >= m_areaForThreeZones) return Mathf.Min(3, m_maxActiveZones);
        if (m_maxExploredArea >= m_areaForTwoZones) return Mathf.Min(2, m_maxActiveZones);

        return 1;
    }

    private void UpdateExploredArea()
    {
        float currentArea = CalculateExploredArea();

        if (currentArea > m_maxExploredArea) m_maxExploredArea = currentArea;
    }

    private float CalculateExploredArea()
    {
        if (m_planeManager == null) return 0f;

        float totalArea = 0f;

        foreach (ARPlane plane in m_planeManager.trackables)
        {
            if (plane == null) continue;
            if (plane.alignment != PlaneAlignment.HorizontalUp) continue;
            if (plane.subsumedBy != null) continue;
            if (plane.trackingState != TrackingState.Tracking) continue;

            totalArea += plane.size.x * plane.size.y;
        }

        return totalArea;
    }

    private void CleanActiveZones()
    {
        m_activeZones.RemoveAll(zone => zone == null);
    }

    private void ClearActiveZones()
    {
        foreach (CollectionZone zone in m_activeZones)
        {
            if (zone == null) continue;

            zone.Consumed -= OnZoneConsumed;
            Destroy(zone.gameObject);
        }

        m_activeZones.Clear();
    }

    private void OnStateChanged(ChaseState p_oldState, ChaseState p_newState)
    {
        if (p_newState == ChaseState.Exploring) RefreshZones();

        if (p_newState == ChaseState.FinalAssembly || p_newState == ChaseState.Completed) ClearActiveZones();
    }
}