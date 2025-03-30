using UnityEngine;

[RequireComponent(typeof(Spacecraft))]
[RequireComponent(typeof(Scanner))]
public class MissileController : SpacecraftController
{
    public float target_speed = 1000;
    public float collision_speed = 200;
    public float turnaround_time = 0.75f;
    public float dampening = 20;
    public float lateral_bias = 1;
    private Scanner _scanner;
    private float _max_thrust;
    private bool _decelerating = false;

    void Awake()
    {
        _scanner = GetComponent<Scanner>();
    }

    protected override void Init()
    {
        _max_thrust = 0;
        foreach (Thruster thruster in _sc.main_thrusters)
        {
            _max_thrust += thruster.max_thrust * Vector3.Dot(transform.up, thruster.transform.up);
        }
    }

    void FixedUpdate()
    {
        Targetable target = _scanner.GetPrimaryTarget();
        if (target)
        {
            float relative_speed = (_rb.velocity - target.ob.state.velocity).magnitude;

            float current_target_speed = target_speed;
            float distance_to_decelerate = Mathf.Abs(Mathf.Pow(relative_speed, 2) - Mathf.Pow(collision_speed, 2))
                / (_max_thrust / _rb.mass * 2)
                + turnaround_time * relative_speed;

            if (_decelerating || Vector3.Distance(target.transform.position, transform.position) < distance_to_decelerate)
            {
                current_target_speed = collision_speed;
                _decelerating = true;
            }

            if (_decelerating && Vector3.Distance(target.transform.position, transform.position) * 2 > distance_to_decelerate)
            {
                _decelerating = false;
            }

            Vector3 target_vel = target.ob.state.velocity + (target.transform.position - transform.position).normalized * current_target_speed - _rb.velocity;
            Vector3 lateral_vel = Vector3.ProjectOnPlane(_rb.velocity - target.ob.state.velocity, target.transform.position - transform.position);
            float target_throttle = target_vel.magnitude;
            target_vel -= lateral_vel * Mathf.Log10(target_throttle) / dampening * lateral_bias;
            RotateTo(target_vel);

            if (Vector3.Dot(transform.up, target_vel.normalized) > 0.999f && target_throttle > 0.1f)
            {
                _sc.SetMainThrottle(Mathf.Log10(target_throttle) / dampening);
            }
            else
            {
                _sc.SetMainThrottle(0);
            }
        }
    }

    public void SetTarget(Targetable target)
    {
        _scanner.SelectTarget(target);
    }
}
