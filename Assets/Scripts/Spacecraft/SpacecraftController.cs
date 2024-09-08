using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Spacecraft))]
public class SpacecraftController : MonoBehaviour
{
    // multiplier for how quickly the craft should attempt to orient towards the given heading
    public float tracking_factor = 2;

    // multiplier for how much thruster power the craft should use while reorienting, increase if the craft is overshooting it's heading
    public float power_factor = 5;
    
    protected Spacecraft _sc;
    protected Rigidbody _rb;

    // Start is called before the first frame update
    protected void Start()
    {
        _sc = GetComponent<Spacecraft>();
        _rb = GetComponent<Rigidbody>();
    }

    // rotate ship towards heading, aiming to stop the rotation when we're aligned
    // heading: target heading in world space
    protected void RotateTo(Vector3 heading)
    {
        Vector3 target_rotation = Vector3.Cross(_sc.transform.up, heading.normalized);
        float sin_comp = Mathf.Sin(Vector3.Angle(_sc.transform.up, heading.normalized) * Mathf.Deg2Rad / 2);

        Vector3 ideal_angular_velocity = sin_comp * tracking_factor * target_rotation.normalized;
        Vector3 angular_velocity_difference = ideal_angular_velocity - _rb.angularVelocity;
        _sc.Rotate(power_factor * angular_velocity_difference);
    }
}
