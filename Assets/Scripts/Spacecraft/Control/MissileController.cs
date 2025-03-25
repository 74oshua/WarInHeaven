using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Spacecraft))]
public class MissileController : SpacecraftController
{
    public Targetable target;
    public float target_speed = 1000;
    public float dampening = 20;
    public float lateral_bias = 1;

    void FixedUpdate()
    {
        if (target)
        {
            Vector3 target_vel = target.ob.state.velocity + (target.transform.position - transform.position).normalized * target_speed - _rb.velocity;
            Vector3 lateral_vel = Vector3.ProjectOnPlane(_rb.velocity - target.ob.state.velocity, target.transform.position - transform.position);
            float target_throttle = target_vel.magnitude;
            target_vel -= lateral_vel * (target_throttle / dampening) * lateral_bias;
            RotateTo(target_vel);

            if (Vector3.Dot(transform.up, target_vel.normalized) > 0.99f && target_throttle > 0.01f)
            {
                _sc.SetMainThrottle(target_throttle / dampening);
            }
            else
            {
                _sc.SetMainThrottle(0);
            }
        }
    }
}
