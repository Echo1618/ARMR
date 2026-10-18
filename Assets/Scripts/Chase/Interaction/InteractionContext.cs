using UnityEngine;

/// <summary>
/// Immutable data package describing one interaction raycast.
/// Pure value type: no runtime debug field belongs here.
/// </summary>
public readonly struct InteractionContext
{
    public Vector2 ScreenPosition { get; }
    public Ray Ray { get; }
    public RaycastHit RaycastHit { get; }

    public InteractionContext(Vector2 p_screenPosition, Ray p_ray, RaycastHit p_raycastHit)
    {
        ScreenPosition = p_screenPosition;
        Ray = p_ray;
        RaycastHit = p_raycastHit;
    }
}
