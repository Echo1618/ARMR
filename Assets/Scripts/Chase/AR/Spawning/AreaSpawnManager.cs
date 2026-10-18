using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Generic AR spawn service.
/// Finds valid random positions on tracked horizontal AR planes and keeps spawned objects separated.
/// It does not know anything about Chase gameplay rules.
/// </summary>
[RequireComponent(typeof(ARPlaneManager))]
public class AreaSpawnManager : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Minimum required distance between each currently alive spawned object.")]
    [Range(0.1f, 3.0f)]
    [SerializeField] private float m_minDistance = 0.5f;
    [SerializeField] private float m_minPlaneSize = 0.5f;
    [SerializeField] private int m_maxAttempts = 100;
    [SerializeField] private bool m_parentToPlane = true;

    [Header("AR")]
    [SerializeField] private ARPlaneManager m_planeManager;

    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    // Tracks currently alive spawned objects so destroyed objects no longer block future spawn positions.
    private readonly List<Transform> m_spawnedObjects = new List<Transform>();

    private void Awake()
    {
        if (!m_planeManager) m_planeManager = GetComponent<ARPlaneManager>();
    }

    /// <summary>Tries to spawn a prefab on any usable AR plane.</summary>
    public bool TrySpawn(GameObject p_prefab, out GameObject p_spawnedObject, bool p_parentToPlane = true)
    {
        p_spawnedObject = null;

        if (p_prefab == null) return false;
        if (!TryGetSpawnPose(out Pose pose, out ARPlane plane)) return false;

        p_spawnedObject = Instantiate(p_prefab, pose.position, pose.rotation);

        if (p_parentToPlane) p_spawnedObject.transform.SetParent(plane.transform, true);

        m_spawnedObjects.Add(p_spawnedObject.transform);

        if (m_debugMode)
            Debug.Log($"[AreaSpawnManager] Spawned {p_prefab.name} on {plane.name}. Parent to plane: {p_parentToPlane}");

        return true;
    }

    /// <summary>Tries to spawn a prefab on a specific AR plane.</summary>
    public bool TrySpawnOnPlane(GameObject p_prefab, ARPlane p_plane, out GameObject p_spawnedObject)
    {
        p_spawnedObject = null;

        if (p_prefab == null) return false;
        if (!IsPlaneUsable(p_plane)) return false;
        if (!TryGetPointOnPlane(p_plane, out Pose pose)) return false;

        p_spawnedObject = Instantiate(p_prefab, pose.position, pose.rotation);
        if (m_parentToPlane) p_spawnedObject.transform.SetParent(p_plane.transform, true);

        m_spawnedObjects.Add(p_spawnedObject.transform);
        if (m_debugMode) Debug.Log($"[AreaSpawnManager] Spawned '{p_prefab.name}' on requested plane {p_plane.trackableId}.");
        return true;
    }

    /// <summary>
    /// Finds a valid pose and corresponding plane without instantiating anything.
    /// No spawn history is modified by this method.
    /// </summary>
    public bool TryGetSpawnPose(out Pose p_pose, out ARPlane p_plane)
    {
        p_pose = default;
        p_plane = null;

        if (!m_planeManager) return false;

        List<ARPlane> usablePlanes = new List<ARPlane>();
        foreach (ARPlane plane in m_planeManager.trackables)
        {
            if (IsPlaneUsable(plane)) usablePlanes.Add(plane);
        }

        // Try each usable plane at most once, in random order.
        while (usablePlanes.Count > 0)
        {
            int randomIndex = Random.Range(0, usablePlanes.Count);
            ARPlane selectedPlane = usablePlanes[randomIndex];

            if (TryGetPointOnPlane(selectedPlane, out Pose pose))
            {
                p_pose = pose;
                p_plane = selectedPlane;
                return true;
            }

            usablePlanes.RemoveAt(randomIndex);
        }

        return false;
    }

    /// <summary>Clears all currently remembered spawned objects.</summary>
    public void ResetSpawnHistory()
    {
        m_spawnedObjects.Clear();
        if (m_debugMode) Debug.Log("[AreaSpawnManager] Spawn tracking cleared.");
    }

    /// <summary>Checks whether a plane is suitable for random placement.</summary>
    private bool IsPlaneUsable(ARPlane p_plane)
    {
        if (p_plane == null) return false;
        if (p_plane.alignment != PlaneAlignment.HorizontalUp) return false;
        if (p_plane.size.x < m_minPlaneSize || p_plane.size.y < m_minPlaneSize) return false;

        NativeArray<Vector2> boundary = p_plane.boundary;
        if (!boundary.IsCreated || boundary.Length < 3) return false;

        return true;
    }

    /// <summary>Finds a random valid pose inside one plane's detected polygon.</summary>
    private bool TryGetPointOnPlane(ARPlane p_plane, out Pose p_pose)
    {
        p_pose = default;
        if (!IsPlaneUsable(p_plane)) return false;

        NativeArray<Vector2> polygonPoints = p_plane.boundary;
        Vector2 limits = p_plane.extents;

        for (int i = 0; i < m_maxAttempts; i++)
        {
            float randomX = Random.Range(-limits.x, limits.x);
            float randomZ = Random.Range(-limits.y, limits.y);
            Vector2 localPoint = new Vector2(randomX, randomZ);

            if (!IsPointInPolygon(localPoint, polygonPoints)) continue;

            Vector3 localPosition = new Vector3(localPoint.x, 0f, localPoint.y);
            Vector3 worldPosition = p_plane.transform.TransformPoint(localPosition);
            if (!IsFarEnough(worldPosition)) continue;

            Quaternion rotation = p_plane.transform.rotation * Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            p_pose = new Pose(worldPosition, rotation);
            return true;
        }

        return false;
    }

    /// <summary>Jordan curve ray-casting test for a point inside the AR plane boundary polygon.</summary>
    private bool IsPointInPolygon(Vector2 p_point, NativeArray<Vector2> p_polygon)
    {
        bool inside = false;
        int j = p_polygon.Length - 1;

        for (int i = 0; i < p_polygon.Length; i++)
        {
            if (((p_polygon[i].y > p_point.y) != (p_polygon[j].y > p_point.y)) &&
                (p_point.x < (p_polygon[j].x - p_polygon[i].x) * (p_point.y - p_polygon[i].y) /
                (p_polygon[j].y - p_polygon[i].y) + p_polygon[i].x))
            {
                inside = !inside;
            }

            j = i;
        }

        return inside;
    }

    /// <summary>Checks spacing against every currently alive spawned object.</summary>
    private bool IsFarEnough(Vector3 p_positionToCheck)
    {
        m_spawnedObjects.RemoveAll(spawnedObject => spawnedObject == null);

        foreach (Transform spawnedObject in m_spawnedObjects)
        {
            if (Vector3.Distance(p_positionToCheck, spawnedObject.position) < m_minDistance) return false;
        }

        return true;
    }
}
