using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeapon : Weapon
{
    public Gimbal gimbal;

    // projectile prefab
    public OrbitalBody projectile;

    public float accuracy = 0.01f;
    public float jitter = 0.01f;
    public float exit_speed = 1;

    Vector3 CalculateIntercept(Vector3 pos, Vector3 vel, float s)
    {
        float a = Mathf.Pow(s, 2) - vel.sqrMagnitude;
        float b = -Vector3.Dot(pos, vel) * 2;
        float c = -pos.sqrMagnitude;

        float t1 = (-b + Mathf.Sqrt(Mathf.Pow(b, 2) - 4 * a * c)) / (2 * a);
        float t2 = (-b - Mathf.Sqrt(Mathf.Pow(b, 2) - 4 * a * c)) / (2 * a);

        if (t1 < t2)
        {
            t1 = t2;
        }

        if (t1 < 0)
        {
            return Vector3.zero;
        }

        Vector3 direction = vel + pos / t1;
        return direction;
    }

    Vector3 CalculateInterceptAccel(Vector3 pos, Vector3 vel, Vector3 accel, float s)
    {
        // List<float> coefficents = new()
        // {
        //     -accel.sqrMagnitude,
        //     -2 * Vector3.Dot(accel, vel),
        //     Mathf.Pow(s, 2) - vel.sqrMagnitude - 2 * Vector3.Dot(pos, accel),
        //     -2 * Vector3.Dot(pos, vel),
        //     -pos.sqrMagnitude
        // };

        List<float> coefficents = new()
        {
            accel.sqrMagnitude / 4,
            Vector3.Dot(accel, vel),
            Vector3.Dot(accel, pos) + vel.sqrMagnitude - Mathf.Pow(s, 2),
            2 * Vector3.Dot(pos, vel),
            pos.sqrMagnitude
        };

        float t = PolySolver.SolvePoly(coefficents, 20, pos.magnitude / s);

        if (t < 0)
        {
            return Vector3.zero;
        }

        Vector3 direction = (pos + vel * t + Mathf.Pow(t, 2) * 0.5f * accel) / t;
        return direction;
    }

    Vector3 CalculateInterceptAccel(Targetable target, float s)
    {
        // List<float> coefficents = new()
        // {
        //     -accel.sqrMagnitude,
        //     -2 * Vector3.Dot(accel, vel),
        //     Mathf.Pow(s, 2) - vel.sqrMagnitude - 2 * Vector3.Dot(pos, accel),
        //     -2 * Vector3.Dot(pos, vel),
        //     -pos.sqrMagnitude
        // };

        Vector3 accel = target.ob.GetAcceleration();
        Vector3 vel = target.ob.state.velocity - _ob.state.velocity;
        Vector3 pos = target.ob.state.position - emitter.transform.position;

        List<float> coefficents = new()
        {
            accel.sqrMagnitude / 4,
            Vector3.Dot(accel, vel),
            Vector3.Dot(accel, pos) + vel.sqrMagnitude - Mathf.Pow(s, 2),
            2 * Vector3.Dot(pos, vel),
            pos.sqrMagnitude
        };

        float t = PolySolver.SolvePoly(coefficents, 20, pos.magnitude / s);

        if (t < 0)
        {
            return Vector3.zero;
        }

        Vector3 direction = (pos + vel * t + Mathf.Pow(t, 2) * 0.5f * accel) / t;
        // Debug.DrawLine(emitter.transform.position, target.ob.GetStateInFuture(t).position);
        // Debug.Log(target.ob.state);
        return direction;
    }

    void FixedUpdate()
    {
        if (gimbal && _target)
        {
            // Vector3 lead = accel.normalized * Mathf.Pow(accel.magnitude, 2) * 0.5f;
            // float effective_speed = (emitter.transform.forward * exit_speed - (_rb.linearVelocity - _target.ob.state.velocity)).magnitude;
            // Vector3 target_point = CalculateInterceptAccel(_target.ob.state.position - transform.position,
            //                                             _target.ob.state.velocity - _ob.state.velocity,
            //                                             _target.ob.GetAcceleration(),
            //                                             // Vector3.zero,
            //                                             exit_speed);
            Vector3 target_point = CalculateInterceptAccel(_target, exit_speed);
            
            if (target_point != Vector3.zero)
            {
                gimbal.SetTargetPoint(target_point);
            }

            // Debug.DrawLine(transform.position, _target.ob.state.position, Color.red);
            // Debug.DrawLine(transform.position, transform.position + (_target.ob.state.velocity - _ob.state.velocity), Color.green);
            // Debug.DrawLine(transform.position, transform.position + target_point, Color.blue);
            // Debug.DrawLine(_target.transform.position, _target.transform.position + _target.ob.GetAcceleration(), Color.blue);
        }
    }

    public override bool Fire()
    {
        // only fire if target is selected
        if (_target == null)
        {
            return false;
        }

        // do not fire if misaligned
        if (gimbal && !gimbal.IsAligned(accuracy))
        {
            return false;
        }

        projectile.gameObject.SetActive(false);
        OrbitalBody p = Instantiate(projectile, emitter.transform.position, emitter.transform.rotation);
        p.initial_velocity = _rb.velocity + emitter.up * exit_speed;
        Scanner scanner = p.GetComponent<Scanner>();
        p.gameObject.SetActive(true);
        if (scanner)
        {
            scanner.SetSearchTarget(_target);
        }
        return true;
    }
}
