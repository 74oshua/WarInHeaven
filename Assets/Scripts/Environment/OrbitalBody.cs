using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Targetable))]
public class OrbitalBody : MonoBehaviour
{
    // class containing state vectors for a body
    public class BodyState
    {
        public BodyState() {}
        
        public BodyState(BodyState b)
        {
            position = b.position;
            velocity = b.velocity;
            mass = b.mass;
            attractor = b.attractor;
        }

        // public static bool operator ==(BodyState a, BodyState b)
        // {
        //     return a.position == b.position && a.velocity == b.velocity && a.mass == b.mass && a.attractor == b.attractor;
        // }

        // public static bool operator !=(BodyState a, BodyState b)
        // {
        //     return a.position != b.position || a.velocity != b.velocity || a.mass != b.mass || a.attractor == b.attractor;
        // }

        // public override bool Equals(object obj)
        // {
        //     return this == (BodyState)obj;
        // }

        // public override int GetHashCode()
        // {
        //     return base.GetHashCode();
        // }

        public Vector3 position;
        public Vector3 velocity;
        public float mass;
        public bool attractor;
    }

    public static float BIG_G = 1e+6f;
    // public static float BIG_G = 1;

    public Vector3 initial_velocity = Vector3.zero;
    // public bool on_rails;

    public bool attractor = true;

    // every registered OrbitalBody
    private static List<OrbitalBody> _orbitalBodies = new List<OrbitalBody>();

    // get current BodyState
    virtual public BodyState state
    {
        get { return new BodyState{
            position = transform.position,
            velocity = _rb.velocity,
            mass = _rb.mass,
            attractor = attractor
            }; }
    }

    // Rigidbody component
    protected Rigidbody _rb;
    public Rigidbody rb
    {
        get { return _rb; }
    }

    // protected List<BodyState> _future_states = new List<BodyState>();

    // true position and velocity, before origin shift
    protected BodyState _true_state = new BodyState();

    // gravity simulation tick. call from GameManager FixedUpdate()
    public static void SimulateGravity()
    {
        // calculate positions for on_rails bodies
        // CalcFuturePositions(1, Time.fixedDeltaTime);

        foreach (OrbitalBody body in _orbitalBodies)
        {
            body.GravityUpdate();
        }

        // for every OrbitalBody
        foreach (OrbitalBody a in _orbitalBodies)
        {
            // for every body except a
            foreach (OrbitalBody b in _orbitalBodies.Except(new List<OrbitalBody>{a}))
            {
                // attract the two bodies together according to gravity
                if (b.attractor)
                {
                    a.AttractTo(b);
                }
            }
        }
    }

    // attract a to b
    // static void Attract(OrbitalBody a, OrbitalBody b)
    // {
    //     a.AttractTo(b);
    // }

    protected virtual void AttractTo(OrbitalBody b)
    {
        _rb.velocity += GetAttractAcceleration(state, b.state) * GameManager.Instance.fixedTimestep;
    }

    // called at the end of every gravity tick
    protected virtual void GravityUpdate()
    {
        // if (on_rails)
        // {
        //     // transform.position = _future_states[1].position - GameManager.Instance.origin_focus.transform.position;
        //     // rb.position = _future_states[1].position - GameManager.Instance.origin_focus.transform.position;
        //     _true_state.position = _future_states[1].position;
        //     _true_state.velocity = _future_states[1].velocity;

        //     Vector3 next_position = _future_states[1].position + OriginShiftController.true_position;
        //     rb.velocity = (next_position - transform.position) / Time.fixedDeltaTime;
        //     // rb.velocity = _future_states[1].velocity;
        //     _future_states.RemoveAt(0);
        //     // DrawPath(10);

        //     Debug.Log(GameManager.Instance.origin_focus.transform.position + OriginShiftController.true_position);
        // }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;

        _rb.velocity = initial_velocity;
        _rb.drag = 0;
        _rb.angularDrag = 0;

        _true_state.position = transform.position;
        _true_state.velocity = _rb.velocity;
        _true_state.attractor = attractor;
        _true_state.mass = _rb.mass;

        // if (on_rails)
        // {
        //     if (_future_states.Count < 1)
        //     {
        //         _future_states.Add(state);
        //     }

        //     _onRailsOrbitalBodies.Add(this);
        // }
    }

    void OnEnable()
    {
        _orbitalBodies.Add(this);
    }

    void OnDisable()
    {
        _orbitalBodies.Remove(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual BodyState GetStateInFuture(float seconds)
    {
        BodyState s = new(state);
        s.position += state.velocity * seconds;
        return s;
    }

    public virtual BodyState GetStateInFutureStep(int ticks)
    {
        BodyState s = new(state);
        s.position += state.velocity * (ticks * GameManager.Instance.fixedTimestep);
        return s;
    }

    // predict where an OrbitalBody will be
    public static BodyState PredictState(BodyState body, List<OrbitalBody> attractors, float timestep, int num_steps = 1, float start_epoch = 0)
    {
        // List<BodyState> attractor_state = new List<BodyState>(attractors);
        BodyState s = new BodyState(body);

        for (int i = 0; i < num_steps; i++)
        {
            // for every attractor
            for (int j = 0; j < attractors.Count; j++)
            {
                // continue if not attractor or attractor is body
                if (!attractors[j].attractor || attractors[j].state == body)
                {
                    continue;
                }
                
                // // simulate force of gravity on body
                // Vector3 difference = attractor_state[j].position - body.position;
                // body.velocity += attractor_state[j].mass * BIG_G * difference.normalized / difference.sqrMagnitude * timestep;

                // simulate force of gravity on body
                BodyState a = attractors[j].GetStateInFuture(i * timestep + start_epoch);
                Vector3 difference = a.position - s.position;
                // Debug.Log(i * timestep);
                s.velocity += a.mass * BIG_G * difference.normalized / difference.sqrMagnitude * timestep;

                // // for every attractor
                // for (int k = 0; k < attractor_state.Count; k++)
                // {
                //     // don't attract ourselves!
                //     if (k == j)
                //     {
                //         continue;
                //     }

                //     // simulate force of gravity on attractor
                //     difference = attractor_state[k].position - attractor_state[j].position;
                //     attractor_state[j].velocity += attractor_state[k].mass * BIG_G * difference.normalized / difference.sqrMagnitude * timestep;
                // }
            }

            // update positions of every attractor and body for next timestep
            s.position += s.velocity * timestep;

            // // for every attractor
            // for (int j = 0; j < attractor_state.Count; j++)
            // {
            //     attractor_state[j].position += attractor_state[j].velocity;
            // }
        }

        return s;
    }

    // get the acceleration a would experience due to gravity based on given positions
    public static Vector3 GetAttractAcceleration(BodyState a, BodyState b)
    {
        Vector3 difference = b.position - a.position;
        return b.mass * BIG_G * difference.normalized / difference.sqrMagnitude;
    }

    // public static List<Vector3> PredictPositions(BodyState body, List<BodyState> attractors, float timestep, int num_steps = 1)
    // {
    //     List<BodyState> attractor_state = new List<BodyState>(attractors);
    //     List<Vector3> next_positions = new List<Vector3>();

    //     for (int i = 0; i < num_steps; i++)
    //     {
    //         // for every attractor
    //         for (int j = 0; j < attractor_state.Count; j++)
    //         {
    //             // continue if not attractor
    //             if (!attractor_state[j].attractor)
    //             {
    //                 continue;
    //             }
                
    //             // simulate force of gravity on body
    //             Vector3 difference = attractor_state[j].position - body.position;
    //             body.velocity += attractor_state[j].mass * BIG_G * difference.normalized / difference.sqrMagnitude * timestep;

    //             // for every attractor
    //             for (int k = 0; k < attractor_state.Count; k++)
    //             {
    //                 // don't attract ourselves!
    //                 if (k == j)
    //                 {
    //                     continue;
    //                 }

    //                 // simulate force of gravity on attractor
    //                 difference = attractor_state[k].position - attractor_state[j].position;
    //                 attractor_state[j].velocity += attractor_state[k].mass * BIG_G * difference.normalized / difference.sqrMagnitude * timestep;
    //             }
    //         }

    //         // update positions of every attractor and body for next timestep
    //         body.position += body.velocity * timestep;
    //         next_positions.Add(body.position);

    //         // for every attractor
    //         for (int j = 0; j < attractor_state.Count; j++)
    //         {
    //             attractor_state[j].position += attractor_state[j].velocity;
    //         }
    //     }

    //     return next_positions;
    // }
}
