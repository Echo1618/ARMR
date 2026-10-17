using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARSurfaceProvider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARPlaneManager m_planeManager;
    [SerializeField] private float m_minSurfaceSize = 0.5f;

    private readonly List<ARPlane> m_usableSurfaces = new List<ARPlane>();

    public IReadOnlyList<ARPlane> UsableSurfaces => m_usableSurfaces;
    public bool HasUsableSurface => m_usableSurfaces.Count > 0;

    public event Action<int> SurfacesChanged;

    private void OnEnable()
    {
        m_planeManager.trackablesChanged.AddListener(OnTrackablesChanged);
        RefreshSurfaces();
    }

    private void OnDisable()
    {
        m_planeManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }

    private void OnTrackablesChanged(
        ARTrackablesChangedEventArgs<ARPlane> p_changes)
    {
        RefreshSurfaces();
    }

    private bool IsSurfaceUsable(ARPlane p_plane)
    {
        if (p_plane == null)
            return false;

        if (p_plane.alignment != PlaneAlignment.HorizontalUp)
            return false;

        if (p_plane.size.x < m_minSurfaceSize)
            return false;

        if (p_plane.size.y < m_minSurfaceSize)
            return false;

        return true;
    }

    private void RefreshSurfaces()
    {
        m_usableSurfaces.Clear();

        foreach (ARPlane plane in m_planeManager.trackables)
        {
            if (IsSurfaceUsable(plane))
                m_usableSurfaces.Add(plane);
        }

        Debug.Log($"[AR SURFACES] Usable surfaces : {m_usableSurfaces.Count}");

        SurfacesChanged?.Invoke(m_usableSurfaces.Count);
    }
}