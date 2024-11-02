using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject origin_focus;
    public float max_origin_dist = 1000f;

    // controls speed of gravity simulation. only for testing, should be 1
    public int timescale = 1;
    public float fixedTimestep = 0.02f;

    private Rigidbody _origin_rb;
    public Rigidbody origin_rb
    {
        get { return _origin_rb; }
    }

    public Camera main_camera;
    
    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get { return _instance == null ? null : _instance; }
    }

    private bool _in_overview = false;
    public bool in_overview
    {
        get { return _in_overview; }
    }

    public void SetInOverview(bool overview)
    {
        _in_overview = overview;
    }

    void OnEnable()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Physics.simulationMode = SimulationMode.Script;

        if (origin_focus)
        {
            _origin_rb = origin_focus.GetComponent<Rigidbody>();
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < timescale; i++)
        {
            OrbitalBody.SimulateGravity();
            Physics.Simulate(fixedTimestep);
        }

        if (origin_focus && origin_focus.transform.position.magnitude > max_origin_dist)
        {
            Vector3 vel_offset = Vector3.zero;
            if (_origin_rb)
            {
                vel_offset = _origin_rb.linearVelocity;
            }
            OriginShiftController.ShiftAll(origin_focus.transform.position, vel_offset);
        }
    }
}
