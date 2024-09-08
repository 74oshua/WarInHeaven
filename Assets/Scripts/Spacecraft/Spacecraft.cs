using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OrbitalBody))]
public class Spacecraft : MonoBehaviour
{
    public List<Thruster> main_thrusters = new();
    public List<Thruster> maneuver_thrusters = new();

    private Rigidbody _rb;

    private float _main_throttle;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        // add parts
        foreach (Thruster t in main_thrusters)
        {
            t.SetSpacecraft(this);
        }
        foreach (Thruster t in maneuver_thrusters)
        {
            t.SetSpacecraft(this);
        }
    }

    void FixedUpdate()
    {
        foreach (Thruster t in main_thrusters)
        {
            t.SetThrottle(_main_throttle);
        }
    }
    
    public void SetMainThrottle(float throttle)
    {
        _main_throttle = throttle;
    }

    public void RelativeTranslate(Vector3 direction)
    {
        foreach (Thruster t in maneuver_thrusters)
        {
            t.Translate(transform.rotation * direction);
        }
    }

    public void Translate(Vector3 direction)
    {
        foreach (Thruster t in maneuver_thrusters)
        {
            t.Translate(direction);
        }
    }

    // fires thrusters to rotates ship about axis
    public void Rotate(Vector3 axis)
    {
        foreach (Thruster t in maneuver_thrusters)
        {
            t.Rotate(axis);
        }
    }

    // fires thrusters to rotates ship about local_axis, which is relative to the ship's transform
    public void RotateRelative(Vector3 local_axis)
    {
        foreach (Thruster t in maneuver_thrusters)
        {
            t.Rotate(transform.rotation * local_axis);
        }
    }
}
