using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;

[RequireComponent(typeof(ARPlaneManager))]
public class AccurateRandomSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("The Prefab of the object to spawn.")]
    public GameObject objectToSpawn;

    [Tooltip("Number of objects to generate.")]
    public int countToSpawn = 10;

    [Header("Distance Settings")]
    [Tooltip("Minimum required distance between each spawned object.")]
    [Range(0.1f, 3.0f)]
    public float minDistance = 0.5f;

    private ARPlaneManager arPlaneManager;
    private bool hasSpawned = false;

    // Stores the world positions of already spawned objects during the loop
    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Awake()
    {
        arPlaneManager = GetComponent<ARPlaneManager>();
    }

    /// <summary>
    /// Call this function via a UI Button event or custom trigger script to start spawning.
    /// </summary>
    public void TriggerManualSpawn()
    {
        // if (hasSpawned)
        // {
        //     Debug.LogWarning("AccurateRandomSpawner: Spawning has already been triggered once!");
        //     return;
        // }

        // Clear previous entries just in case
        spawnedPositions.Clear();

        foreach (var trackable in arPlaneManager.trackables)
        {
            ARPlane plane = trackable;

            // Ignore vertical walls and ensure the plane is large enough
            if (plane.alignment != PlaneAlignment.Vertical && plane.extents.magnitude > 0.5f)
            {
                SpawnMultipleObjects(plane);
                hasSpawned = true;
                break;
            }
        }

        if (!hasSpawned)
        {
            Debug.LogWarning("AccurateRandomSpawner: Cannot spawn. No valid horizontal AR plane detected yet.");
        }
    }

    private void SpawnMultipleObjects(ARPlane plane)
    {
        Debug.Log($"AccurateRandomSpawner: Launching manual spawn on plane {plane.trackableId}");
        Vector2 limits = plane.extents;
        int spawnedCount = 0;
        int attempts = 0;
        int maxAttempts = 200; // Increased attempts limit to allow finding spots with spacing rules

        NativeArray<Vector2> polygonPoints = plane.boundary;

        if (!polygonPoints.IsCreated || polygonPoints.Length < 3) return;

        while (spawnedCount < countToSpawn && attempts < maxAttempts)
        {
            attempts++;

            // 1. Generate local 2D random position within bounding box
            float randomX = Random.Range(-limits.x, limits.x);
            float randomZ = Random.Range(-limits.y, limits.y);
            Vector2 localPoint = new Vector2(randomX, randomZ);

            // 2. CHECK 1: Is the point inside the real physical shape of the plane?
            if (IsPointInPolygon(localPoint, polygonPoints))
            {
                // Convert to local 3D and then to World space coordinates
                Vector3 localPos3D = new Vector3(randomX, 0, randomZ);
                Vector3 worldPosition = plane.transform.TransformPoint(localPos3D);

                // 3. CHECK 2: Is it far enough from all other spawned entities?
                if (IsFarEnough(worldPosition))
                {
                    Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                    // 4. Spawn object and attach to plane
                    GameObject newObject = Instantiate(objectToSpawn, worldPosition, randomRotation);
                    newObject.transform.parent = plane.transform;

                    // Save the position to check against next iterations
                    spawnedPositions.Add(worldPosition);
                    spawnedCount++;
                }
            }
        }

        Debug.Log($"AccurateRandomSpawner: Successfully spawned {spawnedCount} objects out of {countToSpawn} target after {attempts} total attempts.");
    }

    // Returns true if the position is far enough from all previously spawned positions
    private bool IsFarEnough(Vector3 positionToCheck)
    {
        foreach (Vector3 existingPosition in spawnedPositions)
        {
            if (Vector3.Distance(positionToCheck, existingPosition) < minDistance)
            {
                return false; // Too close!
            }
        }
        return true; // Safe spacing distance verified
    }

    // Mathematical Ray-Casting algorithm (Jordan Curve Theorem) for inclusion testing
    private bool IsPointInPolygon(Vector2 point, NativeArray<Vector2> polygon)
    {
        bool inside = false;
        int j = polygon.Length - 1;

        for (int i = 0; i < polygon.Length; i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
            j = i;
        }

        return inside;
    }
}
