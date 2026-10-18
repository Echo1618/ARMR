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
    [SerializeField] private CrateOpeningController m_crateOpeningController;
    [SerializeField] private ARPlaneManager m_planeManager;
    [SerializeField] private GameObject m_collectionZonePrefab;
    [SerializeField] private GameObject m_openingMachinePrefab;

    [Header("Exploration")]
    [SerializeField] private float m_minAreaForTutorial = 0.5f;
    [SerializeField] private float m_areaForTwoZones = 1.5f;
    [SerializeField] private float m_areaForThreeZones = 3f;
    [SerializeField] private float m_areaCheckInterval = 0.5f;

    [Header("Adaptive Spawn")]
    [Min(1)]
    [SerializeField] private int m_maxStoredCratesBeforePause = 2;
    [SerializeField] private Vector2 m_earlySpawnDelay = new Vector2(2f, 4f);
    [SerializeField] private Vector2 m_midSpawnDelay = new Vector2(4f, 7f);
    [SerializeField] private Vector2 m_lateSpawnDelay = new Vector2(6f, 10f);
    [SerializeField] private float m_spawnRetryDelay = 1f;
    [SerializeField] private float m_postTutorialDelay = 1f;

    [Header("Zones")]
    [Min(1)]
    [SerializeField] private int m_maxActiveZones = 3;

    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    private bool m_tutorialCompleted = false;
    private bool m_tutorialZoneCollected = false;
    private bool m_openingMachineUnlocked = false;

    private float m_maxExploredArea = 0f;
    private float m_nextAreaCheckTime = 0f;
    private float m_nextZoneSpawnTime = 0f;

    private OpeningMachine m_openingMachine;

    private readonly List<CollectionZone> m_activeZones = new List<CollectionZone>();

    public bool TutorialCompleted => m_tutorialCompleted;
    public bool OpeningMachineUnlocked => m_openingMachineUnlocked;
    public bool HasOpeningMachine => m_openingMachine != null;

    public float MaxExploredArea => m_maxExploredArea;
    public int ActiveZoneCount => m_activeZones.Count;

    private void Awake()
    {
        Debug.Assert(m_chaseController != null);
        Debug.Assert(m_areaSpawnManager != null);
        Debug.Assert(m_extractionController != null);
        Debug.Assert(m_crateOpeningController != null);
        Debug.Assert(m_planeManager != null);
        Debug.Assert(m_collectionZonePrefab != null);
        Debug.Assert(m_openingMachinePrefab != null);
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
        CleanActiveZones();

        if (m_openingMachineUnlocked && m_openingMachine == null &&
            m_chaseController.CurrentState != ChaseState.FinalAssembly &&
            m_chaseController.CurrentState != ChaseState.Completed)
        {
            TrySpawnOpeningMachine();
        }

        if (m_chaseController.CurrentState == ChaseState.Scanning)
        {
            if (m_maxExploredArea >= m_minAreaForTutorial) m_chaseController.FinishScanning();
            return;
        }

        if (m_chaseController.CurrentState != ChaseState.Exploring) return;

        if (!m_tutorialCompleted)
        {
            EnsureTutorialZone();
            return;
        }

        UpdateAdaptiveSpawning();
    }

    //-----------------------------------------------------------------------------
    // Public
    //-----------------------------------------------------------------------------

    public void CompleteTutorial()
    {
        if (m_tutorialCompleted) return;

        m_tutorialCompleted = true;
        m_nextZoneSpawnTime = Time.time + m_postTutorialDelay;

        if (m_debugMode) Debug.Log("[ChaseDirector] Tutorial completed.");
    }

    //-----------------------------------------------------------------------------
    // Adaptive Spawn
    //-----------------------------------------------------------------------------

    private void EnsureTutorialZone()
    {
        if (m_tutorialZoneCollected) return;
        if (m_activeZones.Count > 0) return;
        if (Time.time < m_nextZoneSpawnTime) return;

        if (TrySpawnZone())
        {
            if (m_debugMode) Debug.Log("[ChaseDirector] Tutorial zone spawned.");
        }
        else
        {
            m_nextZoneSpawnTime = Time.time + m_spawnRetryDelay;
        }
    }

    private void UpdateAdaptiveSpawning()
    {
        int capacity = GetZoneCapacity();

        if (capacity <= 0) return;
        if (m_activeZones.Count >= capacity) return;
        if (Time.time < m_nextZoneSpawnTime) return;

        if (TrySpawnZone())
        {
            ScheduleNextSpawn();

            if (m_debugMode)
                Debug.Log($"[ChaseDirector] Zone spawned. Active: {m_activeZones.Count} / Capacity: {capacity}");
        }
        else
        {
            m_nextZoneSpawnTime = Time.time + m_spawnRetryDelay;
        }
    }

    private int GetZoneCapacity()
    {
        int capacity = 1;

        if (m_maxExploredArea >= m_areaForThreeZones) capacity = 3;
        else if (m_maxExploredArea >= m_areaForTwoZones) capacity = 2;

        capacity = Mathf.Min(capacity, m_maxActiveZones);

        int crateCount = m_chaseController.ChaseProgress.UnopenedCrateCount;

        if (crateCount >= m_maxStoredCratesBeforePause) return 0;
        if (crateCount == 1) return Mathf.Min(capacity, 1);

        return capacity;
    }

    private float GetSpawnDelay()
    {
        int componentCount = m_chaseController.ChaseProgress.CollectedComponentCount;
        Vector2 delayRange;

        if (componentCount <= 1) delayRange = m_earlySpawnDelay;
        else if (componentCount <= 3) delayRange = m_midSpawnDelay;
        else delayRange = m_lateSpawnDelay;

        return Random.Range(delayRange.x, delayRange.y);
    }

    private void ScheduleNextSpawn()
    {
        float delay = GetSpawnDelay();
        m_nextZoneSpawnTime = Time.time + delay;

        if (m_debugMode) Debug.Log($"[ChaseDirector] Next zone allowed in {delay:F1}s.");
    }

    //-----------------------------------------------------------------------------
    // Collection Zones
    //-----------------------------------------------------------------------------

    private bool TrySpawnZone()
    {
        if (m_collectionZonePrefab == null) return false;

        // Les zones restent parentées aux ARPlanes.
        if (!m_areaSpawnManager.TrySpawn(m_collectionZonePrefab, out GameObject spawnedObject)) return false;

        CollectionZone zone = spawnedObject.GetComponent<CollectionZone>();

        if (zone == null)
        {
            Debug.LogError("[ChaseDirector] CollectionZone prefab has no CollectionZone component.");
            Destroy(spawnedObject);
            return false;
        }

        zone.Initialize(m_chaseController, m_extractionController);
        zone.Consumed += OnZoneConsumed;

        m_activeZones.Add(zone);

        return true;
    }

    private void OnZoneConsumed(CollectionZone p_zone)
    {
        if (p_zone == null) return;

        p_zone.Consumed -= OnZoneConsumed;
        m_activeZones.Remove(p_zone);

        if (!m_tutorialCompleted)
        {
            m_tutorialZoneCollected = true;
            m_openingMachineUnlocked = true;

            TrySpawnOpeningMachine();

            if (m_debugMode) Debug.Log("[ChaseDirector] Tutorial zone collected.");

            return;
        }

        ScheduleNextSpawn();

        if (m_debugMode)
            Debug.Log($"[ChaseDirector] Zone collected. Remaining active zones: {m_activeZones.Count}");
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

    //-----------------------------------------------------------------------------
    // Opening Machine
    //-----------------------------------------------------------------------------

    private bool TrySpawnOpeningMachine()
    {
        if (m_openingMachine != null) return true;
        if (m_openingMachinePrefab == null) return false;

        // Contrairement aux CollectionZones, la machine n'est PAS parentée au
        // ARPlane. Elle ne sera donc pas détruite si AR Foundation fusionne ou
        // supprime le plane qui a servi à trouver sa position.
        if (!m_areaSpawnManager.TrySpawn(
                m_openingMachinePrefab,
                out GameObject spawnedObject,
                false))
        {
            return false;
        }

        OpeningMachine machine = spawnedObject.GetComponent<OpeningMachine>();

        if (machine == null)
        {
            Debug.LogError("[ChaseDirector] OpeningMachine prefab has no OpeningMachine component.");
            Destroy(spawnedObject);
            return false;
        }

        machine.Initialize(m_chaseController, m_crateOpeningController);
        m_openingMachine = machine;

        if (m_debugMode) Debug.Log("[ChaseDirector] Opening machine spawned.");

        return true;
    }

    private void ClearOpeningMachine()
    {
        if (m_openingMachine != null) Destroy(m_openingMachine.gameObject);

        m_openingMachine = null;
        m_openingMachineUnlocked = false;

        if (m_debugMode) Debug.Log("[ChaseDirector] Opening machine cleared.");
    }

    //-----------------------------------------------------------------------------
    // Exploration
    //-----------------------------------------------------------------------------

    private void UpdateExploredArea()
    {
        float currentArea = CalculateExploredArea();

        if (currentArea <= m_maxExploredArea) return;

        m_maxExploredArea = currentArea;

        if (m_debugMode)
            Debug.Log($"[ChaseDirector] Max explored area: {m_maxExploredArea:F2} m²");
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

    //-----------------------------------------------------------------------------
    // Chase State
    //-----------------------------------------------------------------------------

    private void OnStateChanged(ChaseState p_oldState, ChaseState p_newState)
    {
        if (p_oldState == ChaseState.OpeningCrate &&
            p_newState == ChaseState.Exploring &&
            m_tutorialCompleted)
        {
            ScheduleNextSpawn();
        }

        if (p_newState == ChaseState.FinalAssembly ||
            p_newState == ChaseState.Completed)
        {
            ClearActiveZones();
            ClearOpeningMachine();
        }
    }
}