using UnityEngine;
/// <summary>
/// Readonly struct that contains all the useful information about a specific interaction :
/// InteractionContext
/// │
/// ├── screenPosition : Vector2
/// ├── ray            : Ray
/// └── raycastHit     : RaycastHit
/// </summary>
public readonly struct InteractionContext
{
    public Vector2 m_screenPosition {get;}
    public Ray m_ray {get;}
    public RaycastHit m_raycastHit {get;}    
    public InteractionContext( 
                        Vector2 p_screenPosition,
                        Ray p_ray,
                        RaycastHit p_raycastHit
                        ){
        m_screenPosition = p_screenPosition;
        m_ray = p_ray;
        m_raycastHit = p_raycastHit;
    }
}