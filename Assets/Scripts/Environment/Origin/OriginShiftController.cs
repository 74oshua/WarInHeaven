using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IShiftable))]
public class OriginShiftController : MonoBehaviour
{
    public static List<OriginShiftController> controllers = new List<OriginShiftController>();
    
    protected IShiftable _s;
    protected static Vector3 _true_position = Vector3.zero;
    protected static Vector3 _true_velocity = Vector3.zero;

    public static Vector3 true_position
    {
        get { return _true_position; }
    }
    public static Vector3 true_velocity
    {
        get { return _true_velocity; }
    }

    protected virtual void Start()
    {
        _s = GetComponent<IShiftable>();
    }

    void OnEnable()
    {
        controllers.Add(this);
    }

    void OnDisable()
    {
        controllers.Remove(this);
    }

    public static void ShiftAll(Vector3 pos_offset, Vector3 vel_offset)
    {
        _true_position -= pos_offset;
        _true_velocity -= vel_offset;
        
        foreach (OriginShiftController controller in controllers)
        {
            controller.Shift(pos_offset, vel_offset);
        }
    }

    public virtual void Shift(Vector3 pos_offset, Vector3 vel_offset)
    {
        _s.Shift(pos_offset, vel_offset);
        // transform.position -= pos_offset;
        // if (_rb)
        // {
        //     _rb.linearVelocity -= vel_offset;
        //     _rb.position -= pos_offset;
        // }
    }
}
