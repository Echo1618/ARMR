using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class AreaSpawnManager : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Minimum required distance between each spawned object.")]
    [Range(0.1f, 3.0f)]
    [SerializeField] private float m_minDistance = 0.5f;
    [SerializeField] private float m_minPlaneSize = 0.5f;
    [SerializeField] private int m_maxAttempts = 100;
    [SerializeField] private bool m_parentToPlane = true;

    [Header("AR")]
    [SerializeField] private ARPlaneManager m_planeManager;

    [Header("History")]
    private readonly List<Vector3> m_spawnedPositions = new List<Vector3>();

    private void Awake()
    {
        if (!m_planeManager) m_planeManager = GetComponent<ARPlaneManager>();
    }

    //-----------------------------------------------------------------------------
    //                                   Public
    //-----------------------------------------------------------------------------

    public bool TrySpawn(GameObject p_prefab, out GameObject p_spawnedObject)
    {
        p_spawnedObject = null;

        if (p_prefab == null) return false;
        if (!TryGetSpawnPose(out Pose pose, out ARPlane plane)) return false;

        p_spawnedObject = Instantiate(p_prefab, pose.position, pose.rotation);

        if (m_parentToPlane) p_spawnedObject.transform.SetParent(plane.transform, true);

        m_spawnedPositions.Add(pose.position);

        return true;
    }

    public bool TrySpawnOnPlane(GameObject p_prefab, ARPlane p_plane, out GameObject p_spawnedObject)
    {
        p_spawnedObject = null;

        if (p_prefab == null) return false;
        if (!IsPlaneUsable(p_plane)) return false;
        if (!TryGetPointOnPlane(p_plane, out Pose pose)) return false;

        p_spawnedObject = Instantiate(p_prefab, pose.position, pose.rotation);

        if (m_parentToPlane) p_spawnedObject.transform.SetParent(p_plane.transform, true);

        m_spawnedPositions.Add(pose.position);

        return true;
    }

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

    public void ResetSpawnHistory()
    {
        m_spawnedPositions.Clear();
    }

    //-----------------------------------------------------------------------------
    //                                   Private
    //-----------------------------------------------------------------------------

    private bool IsPlaneUsable(ARPlane p_plane)
    {
        if (p_plane == null) return false;
        if (p_plane.alignment != PlaneAlignment.HorizontalUp) return false;
        if (p_plane.size.x < m_minPlaneSize || p_plane.size.y < m_minPlaneSize) return false;

        NativeArray<Vector2> boundary = p_plane.boundary;
        if (!boundary.IsCreated || boundary.Length < 3) return false;

        return true;
    }

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

    private bool IsFarEnough(Vector3 p_positionToCheck)
    {
        foreach (Vector3 existingPosition in m_spawnedPositions)
        {
            if (Vector3.Distance(p_positionToCheck, existingPosition) < m_minDistance) return false;
        }

        return true;
    }
}