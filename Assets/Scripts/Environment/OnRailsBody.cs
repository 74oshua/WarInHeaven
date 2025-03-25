using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnRailsBody : OrbitalBody
{
    // every on rails body
    private static List<OnRailsBody> _onRailsBodies = new List<OnRailsBody>();

    // future states of this body
    public List<BodyState> future_states = new List<BodyState>();

    // number of future positions to calculate at a time
    public int cacheLength = 1;

    // how many ticks it takes to transition from one future state to the next state
    public int stepsPerTick = 1;

    // how many steps until we transition to the next state
    private int _stepNum = 0;

    private float _timestep = 0;

    private Vector3 GetTrueVelocity()
    {
        if (future_states.Count > 1)
        {
            return Vector3.Lerp(future_states[0].velocity, future_states[1].velocity, (stepsPerTick - _stepNum) / stepsPerTick);
        }
        else if (future_states.Count > 0)
        {
            return future_states[0].velocity;
        }
        return _rb.velocity;
    }

    override public BodyState state
    {
        get { return new BodyState{
            position = transform.position,
            velocity = GetTrueVelocity(),
            mass = _rb.mass,
            attractor = attractor
            }; }
    }

    protected override void Start()
    {
        base.Start();
        _timestep = GameManager.Instance.fixedTimestep * stepsPerTick;

        if (future_states.Count < 1)
        {
            future_states.Add(state);
        }

        _onRailsBodies.Add(this);
    }

    protected override void GravityUpdate()
    {
        Debug.DrawLine(transform.position, transform.position + GetAcceleration());
        prev_state = state;

        if (future_states.Count < 2)
        {
            CalcFuturePositions(stepsPerTick);
            // _stepNum = stepsPerTick;
        }
        else if (_stepNum >= stepsPerTick)
        {
            _stepNum = 0;
            CalcFuturePositions(cacheLength);
            future_states.RemoveAt(0);
        }
        // else if (future_states.Count < 2)
        // {
        //     CalcFuturePositions(cacheLength);
        // }

        Vector3 next_position = future_states[1].position;
        rb.velocity = (next_position - transform.position) / (GameManager.Instance.fixedTimestep * (stepsPerTick - _stepNum));
        _stepNum++;

        DrawPath(10);
    }

    public override BodyState GetStateInFutureStep(int steps)
    {
        // return null if seconds is less than zero
        // if (seconds < 0)
        // {
        //     Debug.Log("cannot request negative interval!");
        //     Debug.Log(seconds);
        //     return new BodyState();
        // }

        int fraction = steps % stepsPerTick;

        int future_index = steps / stepsPerTick;

        // float future_step = (seconds + fraction) % _timestep / _timestep;
        
        // if (future_step < 0)
        // {
        //     Debug.Log(future_step);
        // }

        // return last state if too far in future
        if (future_index >= future_states.Count)
        {
            // Debug.Log("too far in future!");
            // Debug.Log(name + ": " + steps + " steps, fraction: " + fraction + ", index " + future_index + ". Highest index is " + (future_states.Count - 1) + ", stepnum: " + _stepNum + ", timestep: " + _timestep);
            return future_states[future_states.Count-1];
        }

        // no need to lerp
        if (fraction == 0)
        {
            return future_states[future_index];
        }

        // create a new body state to avoid returning reference
        BodyState s = future_states[future_index];
        BodyState r = new()
        {
            mass = s.mass,
            attractor = s.attractor
        };
        
        // lerp between last state and next state
        if (future_states.Count > future_index+1)
        {
            r.position = Vector3.Lerp(future_states[future_index].position, future_states[future_index+1].position, fraction / (float)stepsPerTick);
            r.velocity = Vector3.Lerp(future_states[future_index].velocity, future_states[future_index+1].velocity, fraction / (float)stepsPerTick);
        }
        // if this body has more stepsPerTick than another, the most recent future value will be used in instances where another body needs it's position before this body has updated
        // for maximum accuracy, all bodies should have the same stepsPerTick, but larger values can be used to reduce updates for larger bodies that don't move much
        else
        {
            r.position = future_states[future_index].position;
            r.velocity = future_states[future_index].velocity;
        }

        return r;
    }

    public override BodyState GetStateInFuture(float seconds)
    {
        int steps = Mathf.FloorToInt(seconds / GameManager.Instance.fixedTimestep) + _stepNum;
        return GetStateInFutureStep(steps);
    }

    protected override void AttractTo(OrbitalBody b)
    {
        return;
    }

    public void DrawPath(int resolution)
    {
        for (int i = resolution; i < future_states.Count; i += resolution)
        {
            Debug.DrawLine(future_states[i-resolution].position, future_states[i].position, Color.red);
        }
    }

    public override void Shift(Vector3 pos_offset, Vector3 vel_offset)
    {
        base.Shift(pos_offset, vel_offset);
        
        for (int i = 0; i < future_states.Count; i++)
        {
            future_states[i].velocity -= vel_offset;
            future_states[i].position -= pos_offset + _timestep * (i - _stepNum / (float)stepsPerTick) * vel_offset;
        }
    }

    // get acceleration of a body at a given point in time
    // exclude_body will be ignored, useful to avoid attracting a body to itself in the future
    protected static Vector3 GetAccelAtTimeTick(BodyState s, int ticks = 0, OrbitalBody exclude_body = null)
    {
        Vector3 accel = Vector3.zero;
        foreach (OnRailsBody other_body in _onRailsBodies)
        {
            BodyState other_body_state = other_body.GetStateInFutureStep(ticks);
            // Debug.Log(other_body.name);
            // Debug.Log(other_body_state.position);

            // don't attract exclude_body and disregard non-attractors
            if (!other_body.attractor || (exclude_body && other_body == exclude_body))
            {
                continue;
            }

            // simulate force of gravity on body
            accel += GetAttractAcceleration(s, other_body_state);
        }
        return accel;
    }

    public static void CalcFuturePositions(int num_steps)
    {
        // get index to start at
        int lowest_future = int.MaxValue;
        for (int i = 0; i < _onRailsBodies.Count; i++)
        {
            if (_onRailsBodies[i].future_states.Count * _onRailsBodies[i].stepsPerTick < lowest_future)
            {
                lowest_future = (_onRailsBodies[i].future_states.Count - 1) * _onRailsBodies[i].stepsPerTick;
            }
        }

        // if (lowest_future < 1)
        // {
        //     lowest_future = 1;
        // }

        // _future_positions[0] is the present
        for (int i = lowest_future; i < num_steps + 1; i++)
        {
            // float curr_time = i * GameManager.Instance.fixedTimestep;
            // if (curr_time < 0)
            // {
            //     curr_time = 0;
            // }

            // for every body
            for (int j = 0; j < _onRailsBodies.Count; j++)
            {
                // if this body doesn't have a tick this step or state has already been saved, continue
                if (((i + _onRailsBodies[j]._stepNum) % _onRailsBodies[j].stepsPerTick) != 0 || (_onRailsBodies[j].future_states.Count-1) * _onRailsBodies[j].stepsPerTick > i)
                {
                    continue;
                }

                // float prev_time = curr_time - _onRailsBodies[j].stepsPerTick * _onRailsBodies[j]._timestep;
                // if (prev_time < 0)
                // {
                //     prev_time = 0;
                // }

                // for every other on rails body
                BodyState curr_body_state = _onRailsBodies[j].GetStateInFutureStep(i);
                BodyState new_state = new BodyState(curr_body_state);
                Vector3 k1 = Vector3.zero;
                Vector3 k2 = Vector3.zero;
                Vector3 k3 = Vector3.zero;
                Vector3 k4 = Vector3.zero;

                // calculate k1
                k1 += GetAccelAtTimeTick(new_state, i, _onRailsBodies[j]);

                // new_state.velocity += k1 * timestep * 0.5f;
                new_state.position += new_state.velocity * _onRailsBodies[j]._timestep * 0.5f + 0.5f * k1 * Mathf.Pow(0.5f * _onRailsBodies[j]._timestep, 2);

                k2 += GetAccelAtTimeTick(new_state, i, _onRailsBodies[j]);

                new_state.velocity = curr_body_state.velocity;
                new_state.position = curr_body_state.position;
                new_state.position += _onRailsBodies[j]._timestep * 0.5f * new_state.velocity + 0.5f * Mathf.Pow(0.5f * _onRailsBodies[j]._timestep, 2) * k2;

                k3 += GetAccelAtTimeTick(new_state, i, _onRailsBodies[j]);

                new_state.velocity = curr_body_state.velocity;
                new_state.position = curr_body_state.position;
                new_state.position += new_state.velocity * _onRailsBodies[j]._timestep + 0.5f * k3 * Mathf.Pow(_onRailsBodies[j]._timestep, 2);

                // calculate k4
                k4 += GetAccelAtTimeTick(new_state, i, _onRailsBodies[j]);

                new_state.velocity = curr_body_state.velocity;
                new_state.position = curr_body_state.position;
                
                Vector3 final_acceleration = (k1 + 2 * k2 + 2 * k3 + k4) / 6;

                new_state.position += new_state.velocity * _onRailsBodies[j]._timestep + 0.5f * final_acceleration * Mathf.Pow(_onRailsBodies[j]._timestep, 2);
                new_state.velocity += final_acceleration * _onRailsBodies[j]._timestep;
                
                _onRailsBodies[j].future_states.Add(new_state);
            }
        }
    }
}
